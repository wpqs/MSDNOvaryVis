using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.ServiceBus.Messaging;
using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace OvaryVisFnApp
{
    public static class OvaryVisSubmitProc
    {
        [FunctionName("OvaryVisSubmitProc")]
        public static async Task Run([ServiceBusTrigger("dimsubmission", AccessRights.Manage, Connection = "AzureWebJobsServiceBus")]string myQueueItem, TraceWriter log) 
        {
            log.Info(string.Format("passed {0}", myQueueItem ?? "[null]"));
            try
            {
                FunctionsAssemblyResolver.RedirectAssembly();

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
            catch(Exception e)
            {
                log.Info(e.Message);
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
            {
                rc += string.Format("record Id={0} found, ", record.Id);
                record.StatusMsg = rc;
                dbContext.Update(record);
                await dbContext.SaveChangesAsync();
            }
            return rc;
        }
    }

    public class FunctionsAssemblyResolver  //acknowledgement: fix suggested by Igne B https://stackoverflow.com/questions/50342416/azure-function-ef-core-cant-load-componentmodel-annotations-4-2-0-0/50770897#50770897
    {  
        public static void RedirectAssembly()
        {
            var list = AppDomain.CurrentDomain.GetAssemblies().OrderByDescending(a => a.FullName).Select(a => a.FullName).ToList();
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
        }
        private static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            var requestedAssembly = new AssemblyName(args.Name);
            Assembly assembly = null;
            AppDomain.CurrentDomain.AssemblyResolve -= CurrentDomain_AssemblyResolve;
 
            assembly = Assembly.Load(requestedAssembly.Name);
   
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            return assembly;
        }
    }
}
