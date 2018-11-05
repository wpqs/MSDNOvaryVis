using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace OvaryVisFnApp
{
    public static class OvaryVisMonitor
    {
        private static IQueueClient _queueClient = null;
        private static readonly object _accesslock = new object();

        private static void SetupServiceBus(string connection, string queueName)
        {
            lock (_accesslock)
            {
                if (_queueClient == null)
                    _queueClient = new QueueClient(connection, queueName);
            }
        }

        [FunctionName("OvaryVisMonitor")]
        public static async Task Run([TimerTrigger("0 */1 * * * *")]TimerInfo myTimer, TraceWriter log, Microsoft.Azure.WebJobs.ExecutionContext exeContext)
        {
            using (Mutex mutex = new Mutex(true, "MSDNOvaryVisMonitorMutuex", out bool doRun))
            {
                if (doRun == false)
                    log.Info(string.Format("Monitor: previous run not completed at {0} - wait for next invocation", DateTime.UtcNow.ToString("HH:mm:ss")));
                else
                {
                    log.Info(string.Format("Monitor: run starts at {0}", DateTime.UtcNow.ToString("HH:mm:ss")));
                    var config = new ConfigurationBuilder().SetBasePath(exeContext?.FunctionAppDirectory)
                                                   .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                                                   .AddEnvironmentVariables().Build();
                    if ((config == null) || (config["ConnectionStrings:DefaultConnection"] == null))
                        log.Info(string.Format("Error:config={0} or DefaultConnection not set", (config == null) ? "null" : "not null"));
                    else
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        optionsBuilder.UseSqlServer(config["ConnectionStrings:DefaultConnection"]);
                        ApplicationDbContext dbContext = new ApplicationDbContext(optionsBuilder.Options);

                        var statusMsg = "Monitor: ";
                        DateTime expiry = DateTime.UtcNow.AddMinutes(-10);
                        var pendingJobs = await dbContext.OvaryVis.Where(a => (a.ResultVis == -1) && (a.JobSubmitted > expiry)).ToListAsync();
                        if (pendingJobs.Count > 0)
                        {
                            if (await Server.IsRunning(config) == false)
                                statusMsg += await Server.StartAsync(config);
                            else
                            {
                                statusMsg += "server running ";

                                SetupServiceBus(config["AzureWebJobsServiceBus"], config["AzureServiceBusQueueName"]);
                                int cnt = 0;
                                foreach (var job in pendingJobs)
                                {
                                    var message = new Message(Encoding.UTF8.GetBytes(job.Id));
                                    await _queueClient.SendAsync(message);
                                    cnt++;
                                }
                                statusMsg += string.Format("{0} of {1} jobs resubmitted", cnt, pendingJobs.Count);
                            }
                        }
                        else
                        {
                            if (await Server.IsRunning(config) == false)
                                statusMsg += "server stopped";
                            else
                            {
                                var recentJobs = await dbContext.OvaryVis.Where(a => a.JobSubmitted > expiry).ToListAsync();
                                if (recentJobs.Count > 0)
                                    statusMsg += string.Format("server running, {0} recent job", recentJobs.Count);
                                else
                                    statusMsg += await Server.StopAsync(config);
                            }
                        }
                        log.Info(statusMsg);
                    }
                    log.Info(string.Format("Monitor: run end at {0}", DateTime.UtcNow.ToString("HH:mm:ss")));
                }
            }
        }
    }
}
