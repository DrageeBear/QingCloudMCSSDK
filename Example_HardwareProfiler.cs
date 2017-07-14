using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using Citrix.ManagedMachineAPI;

//will not  implement the IMachineHardwareProfiler interface so that MCS will not call it for snapshot input for mastering can compile success with this? or add some stub code for compile?
/*
namespace ExampleProvisioningPlugin
{
    public partial class ExampleService : IMachineHardwareProfiler
    {
        private readonly ExampleHardwareProfile demoHardwareProfile = new ExampleHardwareProfile()
        {
            NumberOfVirtualProcessors = 1,
            MemorySizeInMegabytes = 4096,
            FullProfile = "vCPU=1,RAM=4096,..."
        };

        public IHardwareProfile GatherHardwareProfile(ConnectionSettings connectionSettings, IInventoryItem sourceItem)
        {
            // Return hard-wired dummy data.
            // A real implementation should look up the inventory item, and catalog its properties (assuming that it's
            // something like a VM or a snapshot).
            return demoHardwareProfile;
        }

        public IHardwareProfile ReconstructHardwareProfile(string fullProfileString)
        {
            // Return hard-wired dummy data.
            // A real implementation should "deserialize" the profile string here.
            return demoHardwareProfile;
        }
    }

    internal class ExampleHardwareProfile : IHardwareProfile
    {
        public int NumberOfVirtualProcessors { get; internal set; }
        public int MemorySizeInMegabytes { get; internal set; }
        public string FullProfile { get; internal set; }
    }
}
*/
