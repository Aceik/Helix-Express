using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aceik.HelixExpress.Runner
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.Out.WriteLine($"Beginning conversion");
            var configMap = new ExeConfigurationFileMap();
            configMap.ExeConfigFilename = args[0];
            var config = ConfigurationManager.OpenMappedExeConfiguration(configMap, ConfigurationUserLevel.None);
            if (config == null)
            {
                return;
            }
            var companyPrefix = config.AppSettings.Settings["CompanyPrefix"];

            if (companyPrefix == null || string.IsNullOrWhiteSpace(companyPrefix.Value))
            {
                Console.Out.WriteLine($"Failed --- Could not find configuration file location in the arguments");
                return;
            }

            var root = config.AppSettings.Settings["SolutionRootFolder"];
            var helixExpressFolder = config.AppSettings.Settings["HelixExpressTemplateFolder"];
            var originalHelixSolution = config.AppSettings.Settings["OriginalHelixSolution"];
            new SolutionCreator(companyPrefix.Value, root.Value, helixExpressFolder.Value, originalHelixSolution.Value).LoadTemplateSlnFile();
        }
    }
}
