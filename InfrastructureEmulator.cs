using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using Citrix.ManagedMachineAPI;

namespace ExampleProvisioningPlugin
{
    /// <summary>
    /// A class that emulates the operation of hypervisor or cloud hosting infrastructure using files on disk.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class helps with the implementation of a minimal example provisioning plug-in, allowing more of the functionality
    /// to be demonstrated from the XenDesktop or XenApp provisioning environment.
    /// </para>
    /// <para>
    /// The Citrix Provisioning SDK allows plug-ins to be written to create virtual machine instances on arbitrary hosting
    /// infrastructure, such as a hypervisor, hyper-converged platform, or public or private cloud. Since the choice of
    /// target infrastructure is down to each individual implementation, it is impossible for us to know in the generic
    /// example code how all of the plug-in's operations must be implemented.
    /// </para>
    /// <para>
    /// This class is used to effectively fake the provisioning operations using files on the local disk drive.
    /// </para>
    /// <para>
    /// For example, the operation of creating a VM is achieved by simply creating a file on disk. Disk files are sufficiently
    /// persistent that it is possible to emulate the creation and lifecycle of VMs without relying on transient
    /// in-memory dummy data.
    /// </para>
    /// </remarks>
    public class InfrastructureEmulator
    {
        /// <summary>
        /// The root path on the local disk drive.
        /// </summary>
        private readonly string rootPath;

        /// <summary>
        /// Create a new instance of the emulator.
        /// </summary>
        /// <param name="rootPath">The root path for the emulator, such as "C:\temp\myhypervisor\emulation". The root
        /// path characterizes the entire emulation. Multiple instances of this class are functionally equivalent if
        /// they use the same root path. The instance doesn't store any in-memory data at all.</param>
        public InfrastructureEmulator(string rootPath)
        {
            this.rootPath = rootPath;
        }

        /// <summary>
        /// Creates a new emulated virtual machine (disk file) and returns its ID.
        /// </summary>
        /// <param name="machineCreationRequest">The machine creation request that was passed into the provisioning API.</param>
        /// <returns>A string ID for the newly-created machine.</returns>
        public string CreateVirtualMachine(MachineCreationRequest machineCreationRequest)
        {
            Guid id = Guid.NewGuid();
            using (FileStream f = File.Create(MachinePathFromId(id.ToString())))
            {
                var details = new Dictionary<string, string>();
                details.Add("MachineName", machineCreationRequest.MachineName);
                details.Add("PowerState", MachineState.PoweredOff.ToString());
                details.Add("Id", id.ToString());
                WriteDetails(f, details);
                return id.ToString();
            }
        }

        /// <summary>
        /// Gets the details of an existing virtual machine instance with a given ID.
        /// </summary>
        /// <param name="id">The ID of the machine, which will be the ID that was returned from a previous call
        /// to <see cref="CreateVirtualMachine"/>.</param>
        /// <returns>Details of the machine, or null if the machine cannot be located on the hosting infrastructure
        /// (which effectively means that the disk file cannot be found).</returns>
        public IManagedMachine GetVirtualMachine(string id)
        {
            string path = MachinePathFromId(id);
            if (File.Exists(path))
            {
                using (FileStream f = File.Open(path, FileMode.Open))
                {
                    var details = ReadDetails(f);

                    MachineState powerState;

                    if (!Enum.TryParse(details["PowerState"], out powerState))
                    {
                        powerState = MachineState.Unknown;
                    }

                    return new ManagedMachine(
                        id,
                        new List<string>(),
                        details["MachineName"],
                        new List<string>(),
                        powerState,
                        string.Empty);
                }
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Gets a full inventory of all virtual machine instances that have been created on this emulator.
        /// </summary>
        /// <returns>A non-null (but possibly empty) list of populated virtual machine details, including up-to-date
        /// power states.</returns>
        public IList<IManagedMachine> GetAllVirtualMachines()
        {
            var result = new List<IManagedMachine>();

            foreach (string file in Directory.GetFiles(rootPath, "emu-*.vm"))
            {
                using (FileStream fs = File.Open(file, FileMode.Open, FileAccess.Read))
                {
                    var details = ReadDetails(fs);

                    MachineState powerState;

                    if (!Enum.TryParse(details["PowerState"], out powerState))
                    {
                        powerState = MachineState.Unknown;
                    }

                    result.Add(new ManagedMachine(
                        details["Id"],
                        new List<string>(),
                        details["MachineName"],
                        new List<string>(),
                        powerState,
                        string.Empty));
                }
            }

            return result;
        }

        /// <summary>
        /// Deletes a single virtual machine instance from the hosting infrastructure (which is equivalent to deleting
        /// a file from disk).
        /// </summary>
        /// <param name="id">The ID of the machine, which will have been returned from an earlier successful call to
        /// <see cref="CreateVirtualMachine"/>.</param>
        /// <exception cref="MachineCreationException">Thrown if we try to delete a machine that does not exist.</exception>
        public void DeleteVirtualMachine(string id)
        {
            string path = MachinePathFromId(id);
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            else
            {
                throw new MachineCreationException("Example.FailedToDeleteInstance", new string[] {id});
            }
        }

        /// <summary>
        /// Set the power state of the given existing virtual machine to "on".
        /// </summary>
        /// <param name="id">The ID of the machine, which will have been returned from an earlier successful call to
        /// <see cref="CreateVirtualMachine"/>.</param>
        public void PowerOnVirtualMachine(string id)
        {
            string path = MachinePathFromId(id);
            if (File.Exists(path))
            {
                IDictionary<string, string> details;

                using (FileStream f = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    details = ReadDetails(f);
                }

                details["PowerState"] = MachineState.PoweredOn.ToString();

                using (FileStream f = File.Open(path, FileMode.Truncate, FileAccess.ReadWrite))
                {
                    WriteDetails(f, details);
                }
            }
            else
            {
                throw new MachineCreationException("Example.FailedToPowerOnInstance", new string[] { id });
            }
        }

        /// <summary>
        /// Set the power state of the given existing virtual machine to "off".
        /// </summary>
        /// <param name="id">The ID of the machine, which will have been returned from an earlier successful call to
        /// <see cref="CreateVirtualMachine"/>.</param>
        public void PowerOffVirtualMachine(string id)
        {
            string path = MachinePathFromId(id);
            if (File.Exists(path))
            {
                IDictionary<string, string> details;

                using (FileStream f = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    details = ReadDetails(f);
                }

                details["PowerState"] = MachineState.PoweredOff.ToString();

                using (FileStream f = File.Open(path, FileMode.Truncate, FileAccess.ReadWrite))
                {
                    WriteDetails(f, details);
                }
            }
            else
            {
                throw new MachineCreationException("Example.FailedToPowerOffInstance", new string[] { id });
            }
        }

        /// <summary>
        /// Simple algorithm to derive a local, fully-qualified disk file name from an arbitrary string ID.
        /// </summary>
        /// <param name="id">The ID string from which to derive the name.</param>
        /// <returns>A fully-qualified local pathname.</returns>
        private string MachinePathFromId(string id)
        {
            return Path.Combine(rootPath, "emu-" + id + ".vm");
        }

        /// <summary>
        /// Populates a set of key-value pairs into the given virtual machine's file stream.
        /// </summary>
        /// <remarks>
        /// This method outputs the text data format that is expected by <see cref="ReadDetails"/>.
        /// </remarks>
        /// <param name="virtualMachine">The open filestream representing the emulated virtual machine.</param>
        /// <param name="details">Non-null dictionary representing the details that we want to store for the
        /// file.</param>
        private void WriteDetails(FileStream virtualMachine, IDictionary<string, string> details)
        {
            using (StreamWriter writer = new StreamWriter(virtualMachine))
            {
                foreach (KeyValuePair<string, string> kvp in details)
                {
                    writer.WriteLine(kvp.Key + "=" + kvp.Value);
                }
            }
        }

        /// <summary>
        /// Reads key-value pairs from the give filestream and returns them as a dictionary.
        /// </summary>
        /// <param name="virtualMachine">The open filestream from which to read details of the virtual machine
        /// instance. The stream is assumed to be organized with exactly one key value pair per line of the text
        /// file. Each line should be of the form "key=value". Whitespace is allowed and will be trimmed, hence
        /// "key = value" is equivalent to "key=value". This format is obeyed by <see cref="WriteDetails"/>.</param>
        /// <returns>The populated dictionary.</returns>
        private IDictionary<string, string> ReadDetails(FileStream virtualMachine)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();

            using (StreamReader reader = new StreamReader(virtualMachine))
            {
                string line = reader.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    string[] keyValuePair = line.Split('=');
                    string key = keyValuePair[0].Trim();
                    string value = keyValuePair[1].Trim();
                    result.Add(key, value);
                    line = reader.ReadLine();
                }
            }

            return result;
        }
    }
}
