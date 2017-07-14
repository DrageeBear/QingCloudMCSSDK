using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;

//will not  implement the IMachineHardwareProfiler interface so that MCS will not call it for snapshot input for mastering can compile success with this? or add some stub code for compile?

/*
namespace ExampleProvisioningPlugin
{
    public partial class ExampleService : ISynchronousReplication
    {
        public bool SupportsRemoteCopy { get; private set; }

        public DiskImageSpecification LocalCopyDisk(ConnectionSettings connectionSettings, HostingSettings hostingSettings,
            Action<int> progressCallback, DiskImageSpecification sourceDisk, DiskImageSpecification destinationDisk)
        {
            throw new NotImplementedException();
        }

        public DiskImageSpecification RemoteCopyDisk(ConnectionSettings sourceConnectionSettings, HostingSettings sourceHostingSettings,
            ConnectionSettings destinationConnectionSettings, HostingSettings destinationHostingSettings,
            Action<int> progressCallback, DiskImageSpecification sourceDisk, DiskImageSpecification destinationDisk)
        {
            throw new NotImplementedException();
        }
    }
}
*/
