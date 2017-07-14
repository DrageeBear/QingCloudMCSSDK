using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using System.Net;  
using System.Net.Sockets;  
using System.Threading;
using QingCloudSDK.service;

namespace ExampleProvisioningPlugin
{
    public partial class ExampleService : ISynchronousProvisioning
    {
        public bool SupportsBatchedCreationRequests
        { 
            // In current versions of the SDK, all calls are unbatched (a separate call is made for each operation),
            // so we may as well return false here. A return value of true is valid, but wouldn't actually change the
            // behaviour in terms of the pattern of calls to the plugin. to check with Paul.
            get { return false; }
        }

        public bool SupportsBatchedDeletionRequests
        { 
            // In current versions of the SDK, all calls are unbatched (a separate call is made for each operation),
            // so we may as well return false here. A return value of true is valid, but wouldn't actually change the
            // behaviour in terms of the pattern of calls to the plugin.
            get { return false; }
        }


        //ProvisioningSettings provisioningSettings not used(we care serviceoffering in it), just use machineCreationRequests as OS disk
        public IList<MachineCreationResult> CreateMachines(ConnectionSettings connectionSettings, HostingSettings hostingSettings,
            ProvisioningSettings provisioningSettings, IList<MachineCreationRequest> machineCreationRequests)
        {
            //can check whether 'isolated''in the parameter to ensre whether prep VM. If yes return the hardcode VM. need to consider support bach create machine or not.
            MachineCreationRequest request = machineCreationRequests.First();
            logger.TraceMsg("Received request to create machine {0}", request.MachineName); //Received request to create machine Preparation - VdiCatalog17  MCS will define the name for prep_machine
            LocalTrace("new Received request to create machine {0}", request.MachineName);
            string strIDDisk = string.Empty;
            //InfrastructureEmulator emulator = new InfrastructureEmulator(Path.GetTempPath());
            //string id = emulator.CreateVirtualMachine(request);
            //logger.TraceMsg("Machine {0} has id {1}", request.MachineName, id);

            //check whether it is request for prep-VM if yes hard code
            if(request.RunIsolated == true)     //better compare name of machine "prepareXXX"? check with Paul eles tem VMs besides prep-VMs?
            {
                LocalTrace("RunIsolated VM created {0}", request.MachineName);
                string id = "i-tvgizngz";       //hardcoded prep VM  can the id be "Preparation - VdiCatalog38"? list for struct index_dictionary {id, count } when get details check id if contain 'prep' then search and update count.

                MachineCreationResult result = new MachineCreationResult(
                    request.MachineName,    //name should be defined by MCS such as TEST01,02 
                    id,
                    request.DiskAttachmentRequests.Select(a => new DiskAttachmentResult(a.LocationInventoryType, a.LocationId, a.DiskInventoryType, a.DiskId, a.AttachmentIndex))); //diskattachmentresult should have 2 attach
                    //both attach request and attach result is list  last parameter is 'IEnumerable<DiskAttachmentResult> diskAttachmentResults' so real implementation need more invest?
                    //IEnumerable<T> is the base interface for collections in the System.Collections.Generic namespace such as List<T>, Dictionary<TKey,?TValue>, 

                    //update the stored prepa name as 'request.MachineName' ii need modify:when create prep_machine can comment and hard code as "i-tvgizngz" when initialized?
                prepvm_name = request.MachineName;
                return new List<MachineCreationResult> {result};                
            }
            else
            {
                //real VM for catalog
                string image_id = provisioningSettings.MasterImage.Id;
                LocalTrace("CreateMachines master image is {0}", image_id);
                //instance_type or cpu&memory can b achieved from ps script.
                string login_mode = "passwd";
                string login_passwd = "Citrix@111";
                InventoryItemReference network_ref = provisioningSettings.Networks.First();
                string network = network_ref.Id;
                LocalTrace("network is {0}", network); 
                string zone = "pekdemo1";

                //create identity disk first call the socket to load data on the server and get the id of identity disk
                IPAddress ip = IPAddress.Parse("100.100.24.36");  
                Socket clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);  
                try  
                {  
                    clientSocket.Connect(new IPEndPoint(ip, 23456)); //config port 
                    LocalTrace("connect to svr {0}","ok" );  
                }  
                catch  
                {  
                    LocalTrace("connect to svr {0}", "fail");  
                    //return;  
                }

                //change stream to byte find the diskattach for identity one firstly 
                //MachineCreationRequest request
                IList<DiskAttachmentRequest> RealVMDiskList = request.DiskAttachmentRequests;

                // public const string MachineIdentityRole = "DiskRole.Identity"; not same as the text in chm
                foreach (DiskAttachmentRequest Diskrst in RealVMDiskList)
                {
                    LocalTrace("Disk role is {0}", Diskrst.Role);
                    if(string.Equals(Diskrst.Role, "DiskRole.Identity"))  //  hit the id disk
                    {
                        Stream stream = Diskrst.DiskData;

                            byte[] bytes = new byte[stream.Length+1];  //maybe construct data,then copy to new arry[lenth+1]?
                            stream.Read(bytes, 0, bytes.Length); 
                            bytes[stream.Length] = (byte)'\n';

                            LocalTrace("hit the id disk sent data is {0} bytes", (bytes.Length).ToString());

                            string startstr = "QCDiskStart\n";
                            byte[] startbyteArray = System.Text.Encoding.Default.GetBytes (startstr);       

                            string endtstr = "QCDiskEnd\n";
                            byte[] endbyteArray = System.Text.Encoding.Default.GetBytes (endtstr);                             

                            // set the location of the stream
                            stream.Seek(0, SeekOrigin.Begin);
                            LocalTrace("stream changed into byte[] {0}", "already");

                        //send the data, better block-type rcv
                        clientSocket.Send(startbyteArray);
                         LocalTrace("startbyteArray sent {0}", "already");
                        clientSocket.Send(bytes);   
                         LocalTrace("socket sent data {0}", "already");
                        clientSocket.Send(endbyteArray);   
                        LocalTrace("endbyteArray sent {0}", "already");
                                              
                        //receive response from server expected as vol-vnwfrjid no need to encoder/decoder 
                        byte[] volumeid = new byte[100];
                        //string strIDDisk;

                        //use while to rcv until rcv not 0 bytes.
                        int byteCount = 0;
                        while(byteCount == 0)
                        {
                            byteCount = clientSocket.Receive(volumeid, clientSocket.Available,SocketFlags.None); 
                            if (byteCount > 0)
                            {
                                strIDDisk = System.Text.Encoding.Default.GetString (volumeid);
                                LocalTrace("rcv valid volumeid and volumeid is {0}", strIDDisk);
                                LocalTrace("rcv valid volumeid size is {0}", byteCount.ToString());
                            }
                            else
                            {
                                LocalTrace("rcv 0 byte {0}", "already");
                            }                                                    
                        }                        
                    }
                    else        //may need to consider data disk future
                    {
                        //check some string on OS disk
                        LocalTrace("OS disk and volumeid is{0}", Diskrst.DiskId);
                        LocalTrace("OS disk and volumename is{0}", Diskrst.DiskName);
                    }
                }
                
                //now we get the id volume id from server call SDK to create VM  when debugging can comment out the segment test volume worker firstly.maybe crash the enviroment by wrong code?
                Instance.RunInstancesInput input = new Instance.RunInstancesInput();
                String[] volumes = { strIDDisk };
                String[] networks = { network };
                input.setImageId(image_id);
                input.setCpu(1);
                input.setMemory(1024);
                input.setInstanceName(request.MachineName);
                input.setVolumes(volumes);
                input.setLoginMode("passwd");
                input.setLoginPassword(login_passwd);
                input.setVxnets(networks); 
                input.setHostname(request.MachineName);
                //input.setZone("pekdemo1");
                input.setCount(1);
                //String[] vxnets  paul:where to ensue the network in catalog? should use the one in profilesetting or creation request?   

                Instance.RunInstancesOutput output = new Instance.RunInstancesOutput();
                LocalTrace("start creation {0}", "VM");
                output = intance.RunInstances(input);

                // Print the return code.
                LocalTrace("CreateMachines() retcode is {0}", (output.getRetCode()).ToString());
                LocalTrace("The created VM id is {0}", output.getInstances());

                //add VM to inventory tree
                vms.Add(output.getInstances(), request.MachineName);

                //construct the DiskAttachmentresult
                var diskAttachmentResults = new List<DiskAttachmentResult>();
                
                // Add the data disk attachment results for each disk attachrequest
                foreach (DiskAttachmentRequest Disk_Request in RealVMDiskList)
                {
                    if(string.Equals(Disk_Request.Role, "DiskRole.Identity"))  //identity disk hit maybe string.Empty is not ok for 3rd parameter
                    {
                        diskAttachmentResults.Add(new DiskAttachmentResult(string.Empty, string.Empty,string.Empty, strIDDisk, Disk_Request.AttachmentIndex));
                    }
                    else        //simplized: if not identity disk then is os disk
                    {
                        diskAttachmentResults.Add(new DiskAttachmentResult(string.Empty, string.Empty,string.Empty, image_id, Disk_Request.AttachmentIndex));
                    }
                }
              

                //construct the machinecreation result
                MachineCreationResult result = new MachineCreationResult(
                    request.MachineName,    //name should be defined by MCS such as TEST01,02 
                    output.getInstances(),
                    diskAttachmentResults); //diskattachmentresult should have 2 attach
                    //both attach request and attach result is list  last parameter is 'IEnumerable<DiskAttachmentResult> diskAttachmentResults' so real implementation need more invest?
                    //IEnumerable<T> is the base interface for collections in the System.Collections.Generic namespace such as List<T>, Dictionary<TKey,?TValue>, 
                    //take Azure as reference to construct result.

                    //update the stored prepa name as 'request.MachineName' ii need modify:when create prep_machine can comment and hard code as "i-tvgizngz" when initialized?
                return new List<MachineCreationResult> {result};                  
                
            }
        }

        public void DeleteMachines(ConnectionSettings connectionSettings, IList<string> machineIds)
        {
            //in prototype should also be called to delete the prep VM and in proto just trace and no other action
            string machineId = machineIds.First();
            logger.TraceMsg("Received request to delete machine {0}", machineId);
            LocalTrace("Received request to delete machine {0}", machineId);

            //real deletion
                Instance.TerminateInstancesInput input = new Instance.TerminateInstancesInput();
                String[] instances = { machineId };
                input.setInstances(instances);

            Instance.TerminateInstancesOutput output = new Instance.TerminateInstancesOutput();
            LocalTrace("start delete {0}", "VM");
            output = intance.TerminateInstances(input);
            
            // Print the return code.
            LocalTrace("DeleteMachines() retcode is {0}", (output.getRetCode()).ToString());
            /*
            InfrastructureEmulator emulator = new InfrastructureEmulator(Path.GetTempPath());
            emulator.DeleteVirtualMachine(machineId);
            logger.TraceMsg("Deleted machine {0}", machineId);
            */
        }
    }
}
