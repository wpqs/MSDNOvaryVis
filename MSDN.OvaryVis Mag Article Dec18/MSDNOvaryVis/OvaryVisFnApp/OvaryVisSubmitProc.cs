using System;
using System.Threading.Tasks;

using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;


namespace OvaryVisFnApp
{
    public static class OvaryVisSubmitProc
    {
        [FunctionName("OvaryVisSubmitProc")]
        public static async Task Run([ServiceBusTrigger("dimsubmission", Connection = "AzureWebJobsServiceBus")]string myQueueItem, TraceWriter log)
        {
            log.Info(string.Format("passed {0}", myQueueItem ?? "[null]"));
            try
            {
                if (myQueueItem?.Length > 0)
                {
                    var config = new ConfigurationBuilder().AddEnvironmentVariables().Build();
                    if ((config == null) || (config["ConnectionStrings:DefaultConnection"] == null))
                        log.Info(string.Format("OvaryVisSbSubmitProc: error config={0} or DefaultConnection not set", (config == null) ? "null" : "not null"));
                    else
                    {
                        var conn = config["ConnectionStrings:DefaultConnection"];
                        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
                        optionsBuilder.UseSqlServer(conn);
                        var res = await FormSubmittedProc(config, new ApplicationDbContext(optionsBuilder.Options), myQueueItem);

                        log.Info(string.Format("result={0}", res ?? "[null]"));
                    }
                }
            }
            catch (Exception e)
            {
                log.Info(e.Message);
            }
            log.Info("done");
        }

        private static async Task<string> FormSubmittedProc(IConfigurationRoot config, ApplicationDbContext dbContext, string queueItem)
        {
            string rc = "FormSubmittedProc: ";

            if (queueItem == null)
                rc += "queueItem is null";
            else
            {
                var record = await dbContext.OvaryVis.SingleOrDefaultAsync(a => a.Id == queueItem);
                if (record == null)
                    rc += string.Format("record not found: Id={0}", queueItem);
                else
                {
                    rc += string.Format("record Id={0} found, ", record.Id);
                    if ((record.ResultVis != -1) || (record.ResultVis == -99))
                        rc += string.Format("already processed: result={0}", record.ResultVis);
                    else
                    {
                        if (await Server.IsRunning(config) == false)
                            rc += "server not running, wait for job to be resubmitted";
                        else
                        {
                            record.ResultVis = await Server.GetResultAsync(record.D1mm, record.D2mm, record.D3mm);
                            if (record.ResultVis < 0)
                                rc += string.Format("server running result={0} - error", record.ResultVis);
                            else
                                rc += string.Format("server running result={0} - success (ovary {1})", record.ResultVis, (record.ResultVis == 1) ? "found" : "not found");
                        }
                        record.StatusMsg = rc;
                        dbContext.Update(record);
                        await dbContext.SaveChangesAsync();
                    }
                }
            }
            return rc;
        }
    }
}
