using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Citrix.MachineCreationAPI;
using Citrix.ManagedMachineAPI;
using PluginUtilities.Exceptions;
using QingCloudSDK.service;
using QingCloudSDK.config;

namespace ExampleProvisioningPlugin
{
    /// <summary>
    /// This single class implements the entire plugin, but is split into partial classes to improve the
    /// clarity and functional separation between the many distinct interfaces that the class must support.
    /// </summary>
    public partial class ExampleService : ICitrixMachineCreation
    {
        /// <summary>
        /// Use this object to issue diagnostic traces from any plugin operation.
        /// </summary>
        private readonly ILogProvider logger; 
        public Config config;       //for SDK use
        public Instance intance;
        

        //define some config item in config file to be defined by Qing: implement in product. In propo hard code them.
        

        public ExampleService(ILogProvider logger)
        {
            this.logger = logger;
            //create config object for SDK call hard code here need to read config file in product and check config file modified or not if yes read and mofify the config object
            config = new Config("BMJUICBALGOIFGIHQQYJ", "kparYToinhe7Usx9CTu30RJHTkcjfChlooLYM9EA");
            config.setHost("api.pekdemo.com");
            config.setPort("80");
            config.setProtocol("http"); 

            //create instance service object
            intance = new Instance(config, "pekdemo1");
                
            GatherInventory();  
            //build inventory tree and use describeXXX in GatherInventory() to build. Also need to update tree via call GatherInventory() periodically. 
            //Or on some point say creating image via consol, private network added or deleted,public ip,all the above triggered manullly and need to notify plugin.say after op->ps script->plugin?
        }

        public void LocalTrace(string format, string arg)
        {
            //when run again need to clear the previous trace in the file
            StreamWriter sw = new StreamWriter(@"C:\plugin_sw\demotrace.txt",true); 
            //true mean append C:\Program Files\Common Files\Citrix\HCLPlugins\CitrixMachineCreation\v1.0.0.0\ProvisioningExample\demotrace.txt
            sw.WriteLine(format, new object[] {arg});    
            sw.Close();  
        }   
        
        //config file contain customer config which can be updated any time so each time calling API need to read the file and update the config content with relevant value.
        //In propo hard code them.
        public void ReadConfigFile(string format, string arg)
        {
            //when run again need to clear the previous trace in the file
            StreamWriter sw = new StreamWriter(@"C:\plugin_sw\demotrace.txt",true); 
            //true C:\Program Files\Common Files\Citrix\HCLPlugins\CitrixMachineCreation\v1.0.0.0\ProvisioningExample\demotrace.txt
            sw.WriteLine(format, new object[] {arg});    
            sw.Close();  
            /*            
            host: "api.qcdemo.com"
            port: 80
            protocol: "http"
            zone: 'demo1'
            */
        }            
        

        public void ValidateConnectionSettings(ConnectionSettings connectionSettings)
        {
            // For prototype juest leave it as stub besides a trace for call. QIng can choose to implement or not. since API will check the one in config file?
            LocalTrace("ValidateConnectionSettings called ({0})", "ok");
            // The Provisioning SDK currently only supports a single "default" connection
            // credential, consisting of a username and password pair (although these can also
            // be treated as API/secret keys).
            /*
            UserCredential defaultCredential = connectionSettings.Credentials["Default"];

            // We support multiple service addresses, but REST APIs endpoints are typically just a
            // single address, so let's assume that pattern here.
            string endpoint = connectionSettings.ServiceAddresses[0];

            // Currently just issue a trace log (avoiding the password), but this is where you would
            // add code to verify the correct address and credentials.
            logger.TraceMsg(
                "Connecting to Example API endpoint {0} as user {1}",
                endpoint,
                defaultCredential.UserName);

            // Demonstrate the use of some specific validation failures that might suitably be thrown here.

            if (!endpoint.StartsWith("https://", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new HostingInfrastructureAddressNotFoundException("The only supported endpoint type is 'https'.");
            }

            if (!endpoint.EndsWith("127.0.0.1", StringComparison.InvariantCultureIgnoreCase))
            {
                throw new HostingInfrastructureCommunicationsFailureException("Cannot communicate with hosting infrastructure.");
            }

            if (defaultCredential.UserName != "root")
            {
                throw new InvalidCredentialsException("The only permitted user for the example plug-in is 'root'.");
            }
            */
        }

        public StorageModel PluginStorageModel
        {
            // Let's assume that storage repositories are modelled explicitly in the inventory
            // tree, and can be selected as storage targets for provisioning. (See the API documentation
            // for more details about the choice of storage model).
            get { return StorageModel.ImplicitManagedByHostingInfrastructure; }
        }
        public string prepvm_name;
    }
}
