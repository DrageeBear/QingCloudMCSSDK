using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using Citrix.ManagedMachineAPI;
using PluginUtilities.Exceptions;

namespace ExampleProvisioningPlugin
{
    public partial class ExampleService : ISynchronousLifecycle
    {
        public bool SupportsBatchedResetRequests
        {
            // In current versions of the SDK, all calls are unbatched (a separate call is made for each operation),
            // so we may as well return false here. A return value of true is valid, but wouldn't actually change the
            // behaviour in terms of the pattern of calls to the plugin.
            get { return false; }
        }

        public bool SupportsBatchedUpdateRequests
        {
            // In current versions of the SDK, all calls are unbatched (a separate call is made for each operation),
            // so we may as well return false here. A return value of true is valid, but wouldn't actually change the
            // behaviour in terms of the pattern of calls to the plugin.
            get { return false; }
        }

        public IList<MachineLifecycleResult> ResetMachines(ConnectionSettings connectionSettings, HostingSettings hostingSettings, IList<MachineLifecycleRequest> machineRequests,
            DiskImageSpecification masterImage)
        {
            MachineLifecycleRequest request = machineRequests.First();
            logger.TraceMsg("Received request to reset machine {0}. Treating as no-op under emulation.", request.MachineId);

            IManagedMachine machine = GetMachineDetails(connectionSettings, request.MachineId);

            if (machine == null)
            {
                throw new NoSuchManagedMachineException();
            }
            else if (machine.State != MachineState.PoweredOff)
            {
                // Plug-ins should only expect to receive reset or update requests while the machine is powered off.
                // Throw an exception when this is found not to be the case.
                throw new MachineCreationException("Citrix.ExampleService.MachinePoweredOnForReset", new string[] { request.MachineId });
            }

            MachineLifecycleResult result = new MachineLifecycleResult(request.MachineId, null);
            return new List<MachineLifecycleResult>() { result };
        }

        public IList<MachineLifecycleResult> UpdateMachines(ConnectionSettings connectionSettings, HostingSettings hostingSettings, IList<MachineLifecycleRequest> machineRequests,
            DiskImageSpecification newMasterImage)
        {
            MachineLifecycleRequest request = machineRequests.First();
            logger.TraceMsg("Received request to update machine {0}. Treating as no-op under emulation.", request.MachineId);

            IManagedMachine machine = GetMachineDetails(connectionSettings, request.MachineId);

            if (machine == null)
            {
                throw new NoSuchManagedMachineException();
            }
            else if (machine.State != MachineState.PoweredOff)
            {
                // Plug-ins should only expect to receive reset or update requests while the machine is powered off.
                // Throw an exception when this is found not to be the case.
                throw new MachineCreationException("Citrix.ExampleService.MachinePoweredOnForUpdate", new string[] { request.MachineId });
            }

            MachineLifecycleResult result = new MachineLifecycleResult(request.MachineId, null);
            return new List<MachineLifecycleResult>() { result };
        }
    }
}
