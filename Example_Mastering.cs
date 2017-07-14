using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using QingCloudSDK.service;

namespace ExampleProvisioningPlugin
{
    public partial class ExampleService : ISynchronousMastering
    {
        public MasteringDurationHints GetMasteringOperationRelativeDurationHints(ConnectionSettings connectionSettings)
        {
            // Let's assume that we spend all of our time in consolidation.
            return new MasteringDurationHints(100, 0);
        }

        public DiskImageSpecification ConsolidateMasterImage(ConnectionSettings connectionSettings, HostingSettings hostingSettings,
            Action<int> progressCallback, InventoryItemReference sourceItem, DiskImageSpecification requestedDiskProperties)
        {
            // nothing to do and just return DiskImageSpecification object with info from sourceItem only id and type need to be assigned
            logger.TraceEntryExit("ExampleService.GetFinalizeMasterImage (no-op)");
            LocalTrace("ConsolidateMasterImage ({0})", "called");

            //describeinstace to ensure the infra available check api infra works or not
            Instance.DescribeInstancesInput input = new Instance.DescribeInstancesInput();
            Instance.DescribeInstancesOutput output = new Instance.DescribeInstancesOutput();
            output = intance.DescribeInstances(input);

            // Print the return code.
            //Console.WriteLine(output.getRet_code());
            LocalTrace("PowerOnMachines() retcode is {0}", (output.getRetCode()).ToString());

            
            var consilidatonResultDisk = new DiskImageSpecification(
                string.Empty,
                string.Empty, 
                InventoryItemTypes.Template,
                sourceItem.Id,
                string.Empty,
                string.Empty);
            return consilidatonResultDisk;
        }

        //need to abstrace new template from the halted Preparation VM(Qing require stop VM so may call API to stop at the begiining of this function) no need to store the new template in the plugin
        //and do some code to  filter such 'private'templates out of the tree,say when update inventory tree via describeXXX,the new one will be added(to avaoid this by add id in exception list?)
        public DiskImageSpecification FinalizeMasterImage(ConnectionSettings connectionSettings, HostingSettings hostingSettings,
            DiskAccessRequest masterDiskAccess, Action<int> progressCallback, DiskImageSpecification requestedDiskProperties)
        {
            // nothing to do and just return DiskImageSpecification object with info from masterDiskAccess only id and type need to be assigned
            logger.TraceEntryExit("ExampleService.GetFinalizeMasterImage (no-op)");
            LocalTrace("FinalizeMasterImage ({0})", "called");
            //progressCallback(100);
            return requestedDiskProperties;
        }
    }
}
