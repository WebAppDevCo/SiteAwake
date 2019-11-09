using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using SiteAwake.Domain.Entities;
using SiteAwake.Domain.Infrastructure;
using SiteAwake.WakeUpCallProcessor.Infrastructure;

namespace SiteAwake.WakeUpCallProcessor
{
    public interface IWorker
    {
        Task<bool> CheckWebsite(SiteMetadata siteMetadata, HttpClient client, IContext context);
        bool ProcessAccountChanges(List<SiteMetadata> siteMetaData, DateTime startTime, IContext context);
    }

    public class Worker : IWorker
    {
        private readonly string WebsiteUrl;
        private readonly IEmailManager EmailManager;
        private List<Communication> _communications;
        
        public Worker(IEmailManager emailManager)
        {
            EmailManager = emailManager;
            WebsiteUrl = System.Configuration.ConfigurationManager.AppSettings["websiteUrl"];
            _communications = new List<Communication>();
        }

        public async Task<bool> CheckWebsite(SiteMetadata siteMetadata, HttpClient client, IContext context)
        {
            var stopWatch = Stopwatch.StartNew();

            try
            {   
                var response = await client.GetAsync(siteMetadata.Url, HttpCompletionOption.ResponseHeadersRead);

                Console.WriteLine(siteMetadata.Url + " " + response.IsSuccessStatusCode + " " + stopWatch.ElapsedMilliseconds);

                AddCommunication(siteMetadata.Id, null, siteMetadata.LastWakeUpCall ?? DateTime.Now, stopWatch.ElapsedMilliseconds, "OK");

                siteMetadata.AlertSent = false;
                siteMetadata.Processing = false;

                return response.IsSuccessStatusCode;
            }
            catch (HttpRequestException hex)
            {                
                Console.WriteLine(siteMetadata.Url + " " + hex.Message);

                AddCommunication(siteMetadata.Id, hex.Message, siteMetadata.LastWakeUpCall ?? DateTime.Now, stopWatch.ElapsedMilliseconds, "ERROR");

                if(siteMetadata.AlertsEnabled & !siteMetadata.AlertSent)
                {
                    siteMetadata.AlertSent = await EmailManager.SendEmail(siteMetadata.Account.Email, "SiteAwake Alert Generated", CreateEmailAlertBody(siteMetadata), null, null);
                }

                siteMetadata.Processing = false;

                return false;
            }
            catch (TaskCanceledException tce)
            {
                Console.WriteLine(siteMetadata.Url + " " + tce.Message);

                AddCommunication(siteMetadata.Id, tce.Message, siteMetadata.LastWakeUpCall ?? DateTime.Now, stopWatch.ElapsedMilliseconds, "ERROR");

                if (siteMetadata.AlertsEnabled & !siteMetadata.AlertSent)
                {
                    siteMetadata.AlertSent = await EmailManager.SendEmail(siteMetadata.Account.Email, "SiteAwake Alert Generated", CreateEmailAlertBody(siteMetadata), null, null);
                }

                siteMetadata.Processing = false;

                return false;
            }
            catch (Exception ex)
            {
                Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                Console.WriteLine(siteMetadata.Url + " " + ex.ToString());

                AddCommunication(siteMetadata.Id, ex.Message, siteMetadata.LastWakeUpCall ?? DateTime.Now, stopWatch.ElapsedMilliseconds, "ERROR");

                _communications.Add(new Communication()
                {
                    SiteMetadataId = siteMetadata.Id,
                    Created = DateTime.Now,
                    Message = ex.Message,
                    MillisecondsElapsed = stopWatch.ElapsedMilliseconds,
                    Status = "ERROR"
                });

                siteMetadata.Processing = false;

                return false;
            }
        }

        public bool ProcessAccountChanges(List<SiteMetadata> siteMetaDataList, DateTime startTime, IContext context)
        {
            try
            {
                //grab new and updated db entries based on created date and modified date (this will work even though the old data is stored in the object until reload)
                var siteMetadatas = context.SiteMetadatas.Where(x => x.Account.Enabled && x.Account.Verified && !x.Account.Cancelled && (x.Created >= startTime || x.Modified >= startTime || x.Account.Created >= startTime || x.Account.Modified >= startTime)).ToList();

                if (siteMetadatas.Any())
                {
                    //insert / updated entities
                    foreach (var siteMetdata in siteMetadatas)
                    {   
                        if (siteMetaDataList.Any(x => x.Id == siteMetdata.Id))
                        {
                            //force the context to reload the new data since the collection still has the old data
                            context.Entry(siteMetdata).Reload();
                            siteMetdata.Processing = false;
                        }
                        else
                        {
                            siteMetdata.Processing = false;
                            siteMetaDataList.Add(siteMetdata);
                        }
                    }
                }

                //look for disabled accounts
                var disabledAccounts = context.Accounts.Where(x => !x.Enabled && x.Modified >= startTime).ToList();

                if (disabledAccounts.Any())
                {
                    foreach (var account in disabledAccounts)
                    {
                        if (siteMetaDataList.Any(x => x.AccountId == account.Id))
                        {
                            var sm = siteMetaDataList.Single(x => x.AccountId == account.Id);
                            siteMetaDataList.Remove(sm);
                        }
                    }
                }
                
                //look for cancelled accounts
                var cancelledAccounts = context.Accounts.Where(x => x.Cancelled && x.Modified >= startTime).ToList();

                if (cancelledAccounts.Any())
                {
                    foreach (var account in cancelledAccounts)
                    {
                        if (siteMetaDataList.Any(x => x.AccountId == account.Id))
                        {
                            var sm = siteMetaDataList.Single(x => x.AccountId == account.Id);
                            siteMetaDataList.Remove(sm);
                        }
                    }
                }

                //update subscription cancel flags based on trial period ending
                var subscriptionCutoff = DateTime.Now.AddDays(-30);
                var expiredAccounts = context.Accounts.Where(x => !x.Cancelled && !x.Subscribed && x.Created < subscriptionCutoff).ToList();

                if (expiredAccounts.Any())
                {
                    foreach (var account in expiredAccounts)
                    {
                        if (siteMetaDataList.Any(x => x.AccountId == account.Id))
                        {
                            var sm = siteMetaDataList.Single(x => x.AccountId == account.Id);
                            siteMetaDataList.Remove(sm);
                        }

                        account.Cancelled = true;
                        account.Modified = DateTime.Now;
                    }
                }
                
                //remove a communcation from the list after adding it to the context. 
                //This is to prevent clearing the list and potenitally removing something that was not picked up 
                //due to async website checking while the loop was running
                for (int i = _communications.Count() - 1; i >= 0; i--)
                {
                    context.Communications.Add(_communications[i]);
                    _communications.Remove(_communications[i]);
                }
                
                context.SaveChanges();
                
                return true;
            }
            catch (Exception ex)
            {
                Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                Console.WriteLine(ex.ToString());

                foreach (var siteMetdata in siteMetaDataList)
                {
                    siteMetdata.Processing = false;
                }

                return false;
            }
        }

        private void AddCommunication (long siteMetadataId, string message, DateTime wakeUpCall, long elapsedMilliseconds, string status)
        {
            _communications.Add(new Communication()
            {
                SiteMetadataId = siteMetadataId,
                Created = DateTime.Now,
                Message = message,
                MillisecondsElapsed = elapsedMilliseconds,
                Status = status,
                WakeUpCall = wakeUpCall
            });
        }

        private string CreateEmailAlertBody(SiteMetadata siteMetadata)
        {
            string body = "<p style='font-family:Arial;'>Your website may be down. <a href='" + siteMetadata.Url + "'>Go to Website</a>.</p>";
            body += "<p style='font-family:Arial;'>View your <a href='" + WebsiteUrl + "/Account'>Dashboard</a>.</p>";
            body += "<p style='font-family:Arial;'>Thank you!<br/><a href='" + WebsiteUrl + "'>SiteAwake.net</a></p>";

            return body;
        }
    }
}
