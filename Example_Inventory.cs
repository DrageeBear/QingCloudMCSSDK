using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using Citrix.ManagedMachineAPI;



namespace ExampleProvisioningPlugin
{
    /// <summary>
    /// A very minimal demo implementation of an inventory tree.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The first thing to understand about this demo code is that it just returns hard-wired data, without
    /// querying the back-end hosting infrastructure at all(Here we only hard-coded the resource we used).
    /// A real plugin will always need to replace this code in order
    /// to make calls to the infrastructure/cloud to actually gather the objects that need to be modelled in a tree.
    /// </para>
    /// <para>
    /// The inventory model is a tree of objects, similar to a filesystem. The tree needn't have a complex structure.
    /// The structure of the tree will often be suggested by the natural structure of zones, pools, clusters and other
    /// resources/objects on the infrastructure. (named as tree in fact usually as filesystem)
    /// </para>
    /// <para>
    /// This demo inventory tree assumes a completely flat structure: all objects are direct children of the root.
    /// And we only have three types of object: networks, storage pools and templates (images used as a basis for
    /// provisioning)(for Qing cloud, not explicit storage so no need specific storage pool) . why vms here? Paul?
    /// This is enough to allow the plugin to support the creation of a hosting connection.
    /// </para>
    /// </remarks>
    public partial class ExampleService : IInventoryProvider
    {
        private const string RootFolderPath = "/";

        private readonly Dictionary<string, string> networks = new Dictionary<string, string>();
        private readonly Dictionary<string, string> storagePools = new Dictionary<string, string>();    //can be placed here no need to commented out even not used??
        private readonly Dictionary<string, string> templates = new Dictionary<string, string>();
        private readonly Dictionary<string, string> vms = new Dictionary<string, string>();
        private readonly Dictionary<string, string> ServiceOfferings = new Dictionary<string, string>();    //added ServiceOfferings for MCS to choose.
 
        /// <summary>
        /// Dummy extension data dictionary attached to all inventory items. Has no entries.
        /// </summary>
        /// <remarks>
        /// In the Provisioning SDK, all inventory items can be annotated with "extension data", in the form
        /// of arbitrary string key/value pairs. This allows all inventory items to carry a payload of additional,
        /// vendor-specific properties throughout the rest of the system. They can even be presented in the     
        /// UI.
        /// </remarks>
        private readonly Dictionary<string, string> nullExtensionData = new Dictionary<string, string>();
 
        /// <summary>
        /// Gets all direct children of a given path.
        /// </summary>
        /// <param name="connectionSettings">The settings needed to contact and authenticate with the hosting
        /// infrastructure. This demo code doesn't consult the connection settings, because the inventory being
        /// returned is just hard-coded dummy data. A "real" implementation would use these settings to make the
        /// API calls needed to discover the inventory.</param>
        /// <param name="path">The path whose direct children are required.</param>
        /// <returns>
        /// The return value is a flat list of items, whose tree structure is implied by the path of each item. The
        /// plugin can choose to return all recursive children here, or just the direct children of the given path.
        /// This dummy implementation assumes a simple, flat tree, where all networks/storage/templates are direct
        /// children of the root. Therefore, it only returns any data when "/" is passed in as the query path.
        /// If you use Powershell to do a "dir" within the XDHyp:\Connections\ tree, you'll be able to see
        /// these items.
        /// </returns>
        ///CALL API like a DescribeNetworks to build inventory tree yet not all item will be updated in the tree check with paul eg the template after modified by prep-VM bassed on template
        public IList<IInventoryItem> GetContents(ConnectionSettings connectionSettings, string path)
        {
            logger.TraceMsg("GetContents({0})", path);

            //long on local file trace.txt
            LocalTrace("GetContents called with({0})", path);

            List<IInventoryItem> inventoryItems = new List<IInventoryItem>();
            //storage pool network in xdhyp connection dir($hypRootPath = "xdhyp:\Connections\"+$hypConnName+"\") template  network XDHyp:\HostingUnits\$hostingUnitName   $hostingUnitName = "Cluster1"    
            // templates.Add("47FC5151-819C-43BD-A074-AC9ED8B48012", "VDIImageWindows8_64bit");    img-gp3zihbr here template in the rootpath =host unit path
            if (path == RootFolderPath)     //all objects are direct children of the root. seems inventory tree root->hosting unit /?
            {
                AddNetworks(path, inventoryItems);
                AddStoragePools(path, inventoryItems);
                AddTemplates(path, inventoryItems);
                AddServiceOfferings(path, inventoryItems);
                AddVms(path, inventoryItems);
            }
            return inventoryItems;
        }

        public IInventoryItem GetItemFromPath(ConnectionSettings connectionSettings, string path)      
        //IInventoryItem definition can be available in chm also more details on code definition. fullname Gets the full path of the item, including the fullname. Name.ItemType
        {
            if (path.StartsWith("//"))
            {
                // MCS sometimes doubles up the root path.
                path = path.Substring(1);
            }

            logger.TraceMsg("GetItemFromPath({0})", path);
            LocalTrace("GetItemFromPath called with({0})", path);

            if (path == RootFolderPath)
            {
                Dictionary<string, string> dict = new Dictionary<string, string>();
                dict.Add("IsSymLink", bool.TrueString);

                logger.TraceMsg("Returning degenerate root item.");
                LocalTrace("GetItemFromPath called Returning degenerate root item({0}).", "yes");
                // The root path "/" does not refer to a concrete item, but it is a valid path, so don't fail if a query is made.
                // In practice, this query is only ever made to check that the root path is valid, rather than to care about the
                // item returned.
                return new InventoryItem(
                    "root",
                    "root",
                    "root",
                    path,
                    true,
                    dict);
            }

            // Brute-force demo implementation. Just list the entire inventory, and return the item that has the path we're
            // looking for.
            return GetContents(connectionSettings, RootFolderPath).FirstOrDefault(item => item.FullPath == path);
        }

        public IInventoryItem GetItemFromId(ConnectionSettings connectionSettings, string id, string type)
        {
            logger.TraceMsg("GetItemFromId({0}:{1})", type, id);
            LocalTrace("GetItemFromId({0})", type);
            LocalTrace("GetItemFromId({0})", id);
            // Brute-force demo implementation. Just list the entire inventory, and return the item that has the ID we're
            // looking for.
            return GetContents(connectionSettings, RootFolderPath).FirstOrDefault(item => item.Id == id);
        }


        //ExampleService constructor will call this to gather current resource. May need to be called each create new catalog? so call it at the begging of Master step?? NO can not ensure when constructor called via paul

        internal void GatherInventory()     
        {
        LocalTrace("GatherInventory {0}", "called");
            // Assume the existence of some networks. We'll assume that GUIDs are used as the unique ID
            // for each resource here.
            networks.Add("vxnet-0", "BasicNetwork");           // created in consol
            networks.Add("vxnet-dpy53wz", "PrivateInternal");   //created in consol

            // Assume the existence of some storage pools no need here just leave here not delete
            storagePools.Add("3CF5084F-BF77-4A05-940F-1618008945C1", "PoolA");
            storagePools.Add("3A74A889-4DDA-4918-B9EB-081F5A65536B", "PoolB");

            // Assume the existence of some master image templates 
            templates.Add("img-h3bd91ul", "VDA_WIN7");  //2 config added on Friday ok image
            templates.Add("img-1rg09wd3", "VDAlater_WIN7"); //the img for success on Friday.
            templates.Add("01BFF98B-398B-4833-B1BA-B43AA34FB2AC", "AppServerImageWindows2012_R2");

            // Emulate the existence of loads of VMs  no need here?
            /*
            for (int i = 0; i < 1400; i++)
            {
                vms.Add(Guid.NewGuid().ToString(), "FakeVm" + i);
            }
            */

            //add one hardcode prepare VM
            vms.Add("i-tvgizngz", "PrepVM");
            vms.Add("i-a83szwe8", "PWRDLT001");

            //service offering add only one here for prototype can API DescribeInstanceTypes to get supported instance type
            ServiceOfferings.Add("c2m8", "medium_use");     //2 core 8G mem  zone is pekdemo1
            
        }

        private void AddNetworks(string containingPath, List<IInventoryItem> items)     //path is root so... add all items in the list for seach
        {
            foreach (KeyValuePair<string, string> network in networks)
            {
                var item = new InventoryItem(
                    network.Value,
                    InventoryItemTypes.Network,
                    network.Key,
                    containingPath,
                    false,
                    nullExtensionData);

                items.Add(item);
            }
        }

        private void AddStoragePools(string containingPath, List<IInventoryItem> items)
        {
            foreach (KeyValuePair<string, string> storagePool in storagePools)
            {
                var item = new InventoryItem(
                    storagePool.Value,
                    InventoryItemTypes.Storage,
                    storagePool.Key,
                    containingPath,
                    false,
                    nullExtensionData);

                items.Add(item);
            }
        }

        private void AddTemplates(string containingPath, List<IInventoryItem> items)
        {
            foreach (KeyValuePair<string, string> template in templates)
            {
                var item = new InventoryItem(
                    template.Value,
                    InventoryItemTypes.Template,
                    template.Key,
                    containingPath,
                    false,
                    nullExtensionData);

                items.Add(item);
            }
        }

        private void AddServiceOfferings(string containingPath, List<IInventoryItem> items)
        {
            foreach (KeyValuePair<string, string> serviceoffering in ServiceOfferings)
            {
                var item = new InventoryItem(
                    serviceoffering.Value,
                    InventoryItemTypes.ServiceOffering,
                    serviceoffering.Key,
                    containingPath,
                    false,
                    nullExtensionData);

                items.Add(item);
            }
        }
        

        private void AddVms(string containingPath, List<IInventoryItem> items)      //need to call coz vm also will be searched by MCS(not sure)
        {
            foreach (KeyValuePair<string, string> vm in vms)
            {
                var item = new InventoryItem(
                    vm.Value,
                    InventoryItemTypes.VirtualMachine,
                    vm.Key,
                    containingPath,
                    false,
                    nullExtensionData);

                items.Add(item);
            }
        }
    }
}
