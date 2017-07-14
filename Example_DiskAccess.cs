using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;

namespace ExampleProvisioningPlugin
{
    public partial class ExampleService : ISynchronousDiskAccess        // For createmachine the data to populate on disk is 'stream': public Stream DiskData { get; }
    {
        public Stream ReadDisk(ConnectionSettings connectionSettings, DiskAccessRequest diskAccessRequest)
        {
            // The demonstration plug-in has a hard-coded sample disk containing some valid image preparation results for
            // MCS. This allows the demonstration plug-in to be used for image preparation.
            logger.TraceMsg("In ReadDisk - will return hard-coded demonstration set of preparation results.");
            LocalTrace("In ReadDisk - will return hard-coded demonstration set of preparation results. {0}", "ok");
            Assembly assembly = Assembly.GetExecutingAssembly();
            Stream compressedDiskData = assembly.GetManifestResourceStream("ExampleProvisioningPlugin.ImagePrepResultsDisk.raw.cmp");

            if (compressedDiskData == null)
            {
                //throw new MachineCreationException("Citrix.ExampleService.DiskReadFailure");
                LocalTrace("Citrix.ExampleService.DiskReadFailure. {0}", "ok");
            }

            logger.TraceMsg("Obtained compressed binary resource of size {0} bytes.", compressedDiskData.Length);
            LocalTrace("Obtained compressed binary resource of size {0} bytes.", "ok");

            MemoryStream outputStream = new MemoryStream();

            // The disk data is around 12Mb, and is compressed in the embedded resource, so we need to deflate it here.
            using (DeflateStream decompressionStream = new DeflateStream(compressedDiskData, CompressionMode.Decompress))
            {
                decompressionStream.CopyTo(outputStream);
            }

            // Seek back to the start once finished.
            outputStream.Seek(0L, SeekOrigin.Begin);

            logger.TraceMsg("Image preparation results disk size is {0} bytes.", outputStream.Length);
            LocalTrace("Image preparation results disk size is {0} bytes.", "ok");

            return outputStream;
        }

        public void WriteDisk(ConnectionSettings connectionSettings, DiskAccessRequest diskAccessRequest, Stream contents)  //not need to implement per PAUL
        {
            throw new NotImplementedException();
        }

        public void DeleteDisk(ConnectionSettings connectionSettings, DiskImageSpecification diskImageToDelete)
        {
            throw new NotImplementedException();
        }

        public DiskImageSpecification DetachDisk(ConnectionSettings connectionSettings, DiskAccessRequest diskToDetach,     //not need to implement per PAUL
            bool deletingMachine)
        {
            throw new NotImplementedException();
        }
    }
}
