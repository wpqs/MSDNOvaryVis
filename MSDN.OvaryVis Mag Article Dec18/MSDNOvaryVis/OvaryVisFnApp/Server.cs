using System.Collections.Generic;
using System.Threading.Tasks;

using System.Net.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Management.ContainerInstance.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ResourceManager.Fluent;

namespace OvaryVisFnApp
{
    public static class Server
    {
        private static readonly HttpClient _client = new HttpClient();      //see https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/

        private static string _classifierIP = "";
        public static string GetIP() { return _classifierIP; }

        public static async Task<bool> IsRunning(IConfigurationRoot config)
        {
            bool rc = false;

            var container = await GetContainerGroup(config);
            if (container != null)
            {
                _classifierIP = container.IPAddress;
                rc = true;
            }
            return rc;
        }

        public static async Task<string> StartAsync(IConfigurationRoot config)
        {
            string rc = "server start";

            if (await Server.IsRunning(config) == true)
                rc += " completed, already running";
            else
            {
                var azure = GetAzure(config);
                if (azure == null)
                    rc += " failed due to bad azure config";
                else
                {
                    var region = config["ClassifierRegion"];
                    var resourceGroupName = config["ClassifierResourceGroup"];
                    var containerName = config["ClassifierContainerName"];
                    var classifierImage = config["ClassifierImage"];
                    var cpus = config["ClassifierCpus"];
                    var memory = config["ClassifierMemoryGB"];
                    var dockeruser = config["DockerHubUserName"];
                    var dockerpwd = config["DockerHubPassword"];

                    if ((double.TryParse(cpus, out double cpuCount) == false) || (double.TryParse(memory, out double memoryGb) == false))
                        rc += " failed due to invalid settings";
                    else
                    {
                        var containerGroup = await azure.ContainerGroups
                            .Define(containerName).WithRegion(Region.Create(region)).WithExistingResourceGroup(resourceGroupName).WithLinux()
                            .WithPrivateImageRegistry("index.docker.io", dockeruser, dockerpwd)
                            .WithoutVolume()
                                .DefineContainerInstance(containerName).WithImage(classifierImage).WithExternalTcpPort(80).WithCpuCoreCount(cpuCount).WithMemorySizeInGB(memoryGb)
                                .Attach()
                                .WithRestartPolicy(ContainerGroupRestartPolicy.OnFailure)
                            .CreateAsync();
                        _classifierIP = containerGroup?.IPAddress ?? "";
                        if (_classifierIP.Length == 0)
                            rc += " failed as IP is bad";
                        else
                            rc += " completed ok";
                    }
                }
            }
            return rc;
        }

        public static async Task<string> StopAsync(IConfigurationRoot config)
        {
            string rc = "server stop";

            var azure = GetAzure(config);
            if (azure == null)
                rc += " failed due to bad azure config";
            else
            {
                _classifierIP = "";
                IContainerGroup containerGroup = await GetContainerGroup(config, azure);
                if (containerGroup == null)
                    rc += "completed already";
                else
                {
                    await azure.ContainerGroups.DeleteByIdAsync(containerGroup.Id);
                    rc += " completed ok";
                }
            }
            return rc;
        }

        public static async Task<int> GetResultAsync(int D1mm, int D2mm, int D3mm)
        {
            int rc = -99;

            var ip = Server.GetIP();
            if (ip != "")
            {
                var url = string.Format("http://{0}/ocpu/library/ovaryclassifier/R/OvaryVisFromDims", ip);
                var param = new List<KeyValuePair<string, string>>
                {
                    new KeyValuePair<string, string>("d1", D1mm.ToString()),
                    new KeyValuePair<string, string>("d2", D2mm.ToString()),
                    new KeyValuePair<string, string>("d3", D3mm.ToString())
                };

                using (var postResponse = await _client.PostAsync(url, new FormUrlEncodedContent(param)))
                {
                    if (postResponse?.IsSuccessStatusCode == true)
                    {
                        var getUrl = postResponse.Headers.Location.ToString() + "R/.val";
                        using (var getResponse = await _client.GetAsync(getUrl))
                        {
                            if (getResponse?.IsSuccessStatusCode == true)
                            {
                                var response = await getResponse.Content.ReadAsStringAsync();
                                if (response?.Length > 0)
                                {
                                    var start = response.IndexOf('"');
                                    var end = response.IndexOf('"', start + 1);
                                    if ((start >= 0) && (end >= 0) && (end > start))
                                    {
                                        var result = response.Substring(start + 1, end - start - 1);
                                        rc = (result == "visualised") ? 1 : 0;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return rc;
        }

        private static async Task<IContainerGroup> GetContainerGroup(IConfigurationRoot config, IAzure azure = null)
        {
            IContainerGroup rc = null;

            if (azure == null)
                azure = GetAzure(config);
            if (azure != null)
            {
                var classifierResoureGroup = config["ClassifierResourceGroup"];
                var classifierContainerName = config["ClassifierContainerName"];
                var list = await azure.ContainerGroups.ListByResourceGroupAsync(classifierResoureGroup);
                foreach (var container in list)
                {
                    if ((rc = await azure.ContainerGroups.GetByResourceGroupAsync(classifierResoureGroup, classifierContainerName)) != null)
                        break;
                }
            }
            return rc;
        }

        private static IAzure GetAzure(IConfigurationRoot config)
        {
            IAzure rc = null;
            //see AzureResCmds.txt for commands needed to create the Security Principal
            var classifierSecPrincipalId = config["SecPrincipalId"];      //ApplicationId as returned by $sp | Select DisplayName, ApplicationId
            var classifierSecPrincipalKey = config["SecPrincipalKey"];    //EEE as used in $password = ConvertTo-SecureString "EEE" -AsPlainText -Force
            var classifierTenantId = config["TenantId"];                  //TenantID as returned by Get-AzureRmSubscription -SubscriptionName MsdnOvaryVis

            AzureCredentials credentials = SdkContext.AzureCredentialsFactory
                     .FromServicePrincipal(classifierSecPrincipalId, classifierSecPrincipalKey, classifierTenantId, AzureEnvironment.AzureGlobalCloud);
            rc = Azure.Configure()
                    .WithLogLevel(HttpLoggingDelegatingHandler.Level.Basic)
                    .Authenticate(credentials)
                    .WithDefaultSubscription();

            return rc;
        }
    }
}
