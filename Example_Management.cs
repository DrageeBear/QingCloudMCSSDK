using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using Citrix.ManagedMachineAPI;
using QingCloudSDK.service;


namespace ExampleProvisioningPlugin
{
    public partial class ExampleService : ISynchronousManagement
    {
        private static ILogProvider staticLogProvider = null;
        public int pollCount = 0; //seems multi call on GetMachineDetails will affect  pollCount concurrently. need to consider.
        public IManagedMachine GetMachineDetails(ConnectionSettings connectionSettings, string machineId)
        {
            
            logger.TraceMsg("Looking up details for machine {0}", machineId);
            StreamWriter sw = new StreamWriter(@"C:\plugin_sw\demotrace.txt", true);    //to check whether multi traces printed due to jam?

            //InfrastructureEmulator emulator = new InfrastructureEmulator(Path.GetTempPath());
            //return emulator.GetVirtualMachine(machineId);
            if ( string.Equals(machineId, "i-tvgizngz") )       //check if query for prep-machine
            {            
            sw.WriteLine("new Looking up details for machine {0}", machineId);
            string index = pollCount.ToString();
            sw.WriteLine("new Looking up details count is {0}", index);
                sw.WriteLine("hit {0}", "i-tvgizngz");
            
            
                if (pollCount > 0)
                {
                    pollCount = 0;
                    sw.WriteLine("i-tvgizngz off {0}", "now");
                sw.Close();
                return new ManagedMachine(
                        machineId,
                        new List<string>(),
                        prepvm_name,    //need to meet the requested? can be null? can have a test 
                        new List<string>(),
                        MachineState.PoweredOff,
                        string.Empty);
                }
                else
                {
                /*
                    if (pollCount == 0) //here check when pollCount is 0 the first time?or multi  time with multi call?
                   {
                    sw.WriteLine("pllcount is {0}", "0");
                        pollCount++;
                    sw.Close();
                    return new ManagedMachine(
                            machineId,
                            new List<string>(),
                            prepvm_name,    //need to meet the requested?
                            new List<string>(),
                            MachineState.PoweredOn,
                            string.Empty);
                    }
                    */
                    //else
                    //{
                    pollCount++;
                    sw.WriteLine("i-tvgizngz on {0}", "now");
                    sw.Close();
                    return new ManagedMachine(
                            machineId,
                            new List<string>(),
                            prepvm_name,    //need to meet the requested?
                            new List<string>(),
                            MachineState.PoweredOn,
                            string.Empty);
                    //}

                }
            }
            else        //  get real VM details
            {
                sw.WriteLine("GetMachineDetails on real VM");
                sw.Close();   
                return new ManagedMachine(
                machineId,
                new List<string>(),
                prepvm_name,    //need to meet the requested?
                new List<string>(),
                MachineState.PoweredOn,
                string.Empty);                
            }
                  
        }

        public bool SupportsBatchedManagementRequests
        { 
            // In current versions of the SDK, all calls are unbatched (a separate call is made for each operation),
            // so we may as well return false here. A return value of true is valid, but wouldn't actually change the
            // behaviour in terms of the pattern of calls to the plugin.
            get { return false; }        
        }
        public IList<SupportedPowerAction> SupportedPowerActions
        {
            get
            {
                // Let's assume that the plugin can support a basic set of power operations. Feel free to add
                // more from the SupportedPowerAction enum where relevant.
                List<SupportedPowerAction> result = new List<SupportedPowerAction>();
                result.Add(SupportedPowerAction.PowerOn);
                result.Add(SupportedPowerAction.PowerOff);
                result.Add(SupportedPowerAction.Shutdown);
                result.Add(SupportedPowerAction.Reset);
                return result;
            }
        }

        public void PowerOnMachines(ConnectionSettings connectionSettings, IList<string> machineIds)
        {
        //For the hardcode prep VM API to power on will return false and ignore it(check whether prep VM, if yes no failure handle)
        //for other VM need throw out exception. need to implement when API call is available.
            LocalTrace("new PowerOnMachines() get called for {0}", "ok");
            // Print the first instance ID.
            //Console.WriteLine(output.getInstance_id()[0]);
            
                        foreach (string id in machineIds)
                        {
                            LocalTrace("Power On Machine {0}", id);     //Power On Machine i-tvgizngz
                        }


                        //String[] instances = {"i-tvgizngz"};
                        String[] instances = { machineIds.First() };
                        
                        Instance.StartInstancesInput input = new Instance.StartInstancesInput();
                        input.setInstances(instances);
                        //input.setZone("demo1");
                        Instance.StartInstancesOutput output = new Instance.StartInstancesOutput();
                        output = intance.StartInstances(input);

                        // Print the return code.
                        //Console.WriteLine(output.getRetCode());
                        LocalTrace("PowerOnMachines() retcode is {0}", (output.getRetCode()).ToString());

            
            // Print the first instance ID.
            //Console.WriteLine(output.getVolomeId()[0]);
            //LocalTrace("PowerOnMachines() retcode is {0}", (output.getRetCode()).ToString());
            /*
            InfrastructureEmulator emulator = new InfrastructureEmulator(Path.GetTempPath());
            foreach (string id in machineIds)
            {
                IManagedMachine machine = emulator.GetVirtualMachine(id);

                emulator.PowerOnVirtualMachine(id);

                // Demonstration: The image preparation process expects a virtual machine to run for a short time and
                // then shut itself down. Image preparation machines have names starting with "Preparation". If we
                // intercept a request to power on a Preparation machine, then we'll set up an alarm to shut the machine
                // back down again after a minute. This is because the emulator does not provision real virtual machines,
                // so we need to simulate the concept of a machine that runs and then shuts down on its own.
                if (machine != null &&
                    machine.ManagedMachineName.StartsWith("Preparation", StringComparison.InvariantCultureIgnoreCase))
                {
                    logger.TraceMsg("Detected power-on for an image preparation machine ('{0}'). Setting up auto-shutdown.", machine.ManagedMachineName);
                    staticLogProvider = logger;
                    Thread imagePrepThread = new Thread(AutoShutdownMachine);
                    imagePrepThread.Start(machine.ManagedMachineId);
                }
            }
        */
        }

        public void PowerOffMachines(ConnectionSettings connectionSettings, IList<string> machineIds)
        {
            LocalTrace("PowerOffMachines() get called {0}", "ok");

            // Print the first instance ID.
            //Console.WriteLine(output.getInstance_id()[0]);
            
                        foreach (string id in machineIds)
                        {
                            LocalTrace("Power Off  Machine {0}", id);     
                        }


                        //String[] instances = {"i-tvgizngz"};
                        String[] instances = { machineIds.First() };
            

                       //test code for poweroff
            
                      
                       Instance.StopInstancesInput input = new Instance.StopInstancesInput();
                       
                                   input.setInstances(instances);
                       
                                   Instance.StopInstancesOutput output = new Instance.StopInstancesOutput();
                       
                                   output = intance.StopInstances(input);
            
                                   LocalTrace("PoweroffMachines() retcode is {0}", (output.getRetCode()).ToString());
                                   
            
            
            /*
            InfrastructureEmulator emulator = new InfrastructureEmulator(Path.GetTempPath());
            foreach (string id in machineIds)
            {
                emulator.PowerOffVirtualMachine(id);
            }
            */
        }

        public void ShutdownMachines(ConnectionSettings connectionSettings, IList<string> machineIds)
        {
            PowerOffMachines(connectionSettings, machineIds);
        }

        public void SuspendMachines(ConnectionSettings connectionSettings, IList<string> machineIds)
        {
        
        LocalTrace("new SuspendMachines() get called for {0}", "ok");
            throw new NotImplementedException();
        }

        public void RestartMachines(ConnectionSettings connectionSettings, IList<string> machineIds)
        {

                   LocalTrace("RestartMachines() get called {0}", "ok");
                   
                   // Print the first instance ID.
                   //Console.WriteLine(output.getInstance_id()[0]);
                   
                               foreach (string id in machineIds)
                               {
                                   LocalTrace("Resatart Machine {0}", id);     
                               }
                   
                   
                               //String[] instances = {"i-tvgizngz"};
                               String[] instances = { machineIds.First() };

                   //test code for restart
        
        
                    Instance.RestartInstancesInput input = new Instance.RestartInstancesInput();
        
                    input.setInstances(instances);
        
                    Instance.RestartInstancesOutput output = new Instance.RestartInstancesOutput();
        
                    output = intance.RestartInstances(input);
        
                    LocalTrace("restare() retcode is {0}", (output.getRetCode()).ToString());


        /*
            InfrastructureEmulator emulator = new InfrastructureEmulator(Path.GetTempPath());
            foreach (string id in machineIds)
            {
                emulator.PowerOnVirtualMachine(id);
            }
            */
        }

        public void ResetMachines(ConnectionSettings connectionSettings, IList<string> machineIds)
        {
            LocalTrace("new ResetMachines() get called for {0}", "ok");
        
            InfrastructureEmulator emulator = new InfrastructureEmulator(Path.GetTempPath());
            foreach (string id in machineIds)
            {
                emulator.PowerOnVirtualMachine(id);
            }
        }

        private static void AutoShutdownMachine(Object machineId)
        {
        
        //LocalTrace("new AutoShutdownMachine() get called for {0}", "ok");
            if (staticLogProvider != null)
            {
                staticLogProvider.TraceMsg("Starting a one minute sleep for the image preparation machine.");
            }

            Thread.Sleep(1 * 60 * 1000);
            InfrastructureEmulator emulator = new InfrastructureEmulator(Path.GetTempPath());

            if (staticLogProvider != null)
            {
                staticLogProvider.TraceMsg("Image preparation timer has elapsed. The preparation machine will now be powered off.");
            }

            // Explicitly power off the machine as a way of emulating the self-shutdown that would happen in the
            // real world.
            emulator.PowerOffVirtualMachine((string) machineId);
        }
    }
}
