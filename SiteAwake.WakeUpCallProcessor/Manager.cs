using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SiteAwake.Domain.Entities;
using SiteAwake.Domain.Infrastructure;
using System.Net.Http;
using System.Net;
using System.Diagnostics;
using SiteAwake.WakeUpCallProcessor.Infrastructure;

namespace SiteAwake.WakeUpCallProcessor
{
    public interface IManager
    {
        void Run();
    }

    public class Manager : IManager
    {
        private readonly IWorker Worker;
        private readonly IContext Context;
       
        private List<SiteMetadata> _siteMetadatas;
        private HttpClient _client;

        public Manager (IWorker worker, IContext context)
        {
            Worker = worker;
            Context = context;
            
            ServicePointManager.DefaultConnectionLimit = 10000;
            ServicePointManager.UseNagleAlgorithm = false;
            ServicePointManager.Expect100Continue = false;
            ServicePointManager.CheckCertificateRevocationList = false;
            ServicePointManager.ReusePort = true;
        }

        public void Run()
        {
            var accountProcessCheckOffestSeconds = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["accountProcessCheckOffestSeconds"]);
            var accountProcessCheckOffestSlippageSeconds = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["accountProcessCheckOffestSlippageSeconds"]);
            var startTime = DateTime.Now;
            var accountProcessCheckTime = DateTime.Now;

            _siteMetadatas = Context.SiteMetadatas.Where(x => x.Account.Enabled && x.Account.Verified && !x.Account.Cancelled).ToList();
            
            _client = new HttpClient();
            _client.Timeout = new TimeSpan(0, 0, 60);

            //reset flags
            foreach (var siteMetadata in _siteMetadatas)
            {
                siteMetadata.Processing = false;
                siteMetadata.LastWakeUpCall = null;
            }

            while (1 != 0)
            {       
                try
                {
                    //process siteMetadatas based on interval
                    for (int i = _siteMetadatas.Count() - 1; i >= 0; i--)
                    {
                        try
                        { 
                            var siteMetadata = _siteMetadatas[i];

                            var currentTime = DateTime.Now;

                            if (DateTime.Compare((siteMetadata.LastWakeUpCall ?? DateTime.MinValue).AddMinutes(siteMetadata.Interval), currentTime) <= 0 & !siteMetadata.Processing)
                            {   
                                var task = Task.Run(() =>
                                {
                                    if (!siteMetadata.Processing)
                                    {
                                        siteMetadata.LastWakeUpCall = DateTime.Now;
                                        siteMetadata.Processing = true;
                                        Console.WriteLine((siteMetadata.LastWakeUpCall ?? DateTime.MinValue).ToString("MM/dd/yyyy HH:mm:ss") + " " + siteMetadata.Url);
                                        Worker.CheckWebsite(siteMetadata, _client, Context);
                                    }
                                });
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                            Console.WriteLine(ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                    Console.WriteLine(ex.ToString());
                }

                try
                { 
                    //process account changes every n minutes as long as websites are not being checked
                    if (DateTime.Compare(DateTime.Now.AddSeconds(accountProcessCheckOffestSeconds), accountProcessCheckTime) >= 0)
                    {
                        accountProcessCheckTime = DateTime.Now;

                        var task = Task.Run(() =>
                        {
                            //check last n minutes to make sure nothing gets out of sync / exact timing off and missed
                            Worker.ProcessAccountChanges(_siteMetadatas, accountProcessCheckTime.AddSeconds(accountProcessCheckOffestSlippageSeconds), Context);
                        });
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                    Console.WriteLine(ex.ToString());
                }

                Thread.Sleep(1);
            }
        }
    }
}
