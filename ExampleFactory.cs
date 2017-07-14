using System;
using System.Collections.Generic;
using System.Linq;
using System.AddIn;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using Citrix.ManagedMachineAPI;

namespace ExampleProvisioningPlugin
{
    [AddIn("ExampleProvisioningFactory", Publisher = "Citrix, Inc.", Version = "1.0.0.0")]
    public class ExampleProvisioningFactory : IMachineCreationFactory
    {
        public ICitrixMachineCreation CreateService(ILogProvider logger)
        {
            return new ExampleService(logger);
        }

        public string LocalizedName(string localeName)
        {
            return "Example Provisioning Plugin";
        }

        public string LocalizedDescription(string localeName)
        {
            // Example readable description, currently ignoring the locale.
            return "A minimal demonstration of the Citrix Provisioning SDK";
        }

        public string ExampleConnectionAddress(string localeName)
        {
            // Ignoring locale here. This should be a readable "hint" to the user as to
            // how the connection address should be formatted.
            return "http://172.27.10.10/api";
        }

        public bool ValidateConnectionAddressFormat(string connectionAddress)
        {
            // No validation performed in this template. Add code here to check the syntax of a candidate
            // REST API endpoint address.
            return true;
        }

        public string FactoryFor
        {
            // Just boilerplate here...
            get { return this.GetType().Name; }
        }

        public string Label
        {
            // Arbitrary "unique" identifier for this plugin.
            get { return "CitrixExample"; }
        }
    }
}
