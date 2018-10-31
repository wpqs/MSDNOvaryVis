using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceBus.Messaging;
using System.Threading.Tasks;

namespace OvaryVisFnApp
{
    public static class OvaryVisSubmitProc
    {
        [FunctionName("OvaryVisSubmitProc")]
        public static async Task Run([ServiceBusTrigger("dimsubmission", AccessRights.Manage, Connection = "AzureWebJobsServiceBus")]string myQueueItem, TraceWriter log, ExecutionContext exeContext)
        {
            if (myQueueItem?.Length > 0)
            {
                var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
                if ((config == null) || (config["ConnectionStrings:DefaultConnection"] == null))
                    log.Info(string.Format("OvaryVisSbSubmitProc: error config={0} or DefaultConnection not set", (config == null) ? "null" : "not null"));
                else
                {
                    var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                    optionsBuilder.UseSqlServer(config["ConnectionStrings:DefaultConnection"]);

                    log.Info(await FormSubmittedProc(config, new ApplicationDbContext(optionsBuilder.Options), myQueueItem));
                }
            }
            log.Info("done");
        }

        private static async Task<string> FormSubmittedProc(IConfigurationRoot config, ApplicationDbContext dbContext, string queueItem)
        {
            string rc = "FormSubmittedProc: ";

            var record = await dbContext.OvaryVis.SingleOrDefaultAsync(a => a.Id == queueItem);
            if (record == null)
                rc += string.Format("record not found: Id={0}", queueItem);
            else
                 rc += string.Format("record Id={0} found, ", record.Id);

            return rc;
        }
    }
}
