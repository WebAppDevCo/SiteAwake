using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Threading.Tasks;
using System.Net.Mail;

namespace SiteAwake.WakeUpCallProcessor.Infrastructure
{
    public interface IEmailManager : IDisposable
    {
        Task<bool> SendEmail(string recipient, string subject, string body, string cc, IEnumerable<Attachment> attachments);
    }
    
    public class EmailManager : IEmailManager
    {
        public async Task<bool> SendEmail(string recipient, string subject, string body, string cc, IEnumerable<Attachment> attachments)
        {
            try
            {
                using (SmtpClient client = new SmtpClient())
                {
                    using (MailMessage message = new MailMessage())
                    {
                        message.To.Add(new MailAddress(recipient));
                        if (!string.IsNullOrEmpty(cc))
                        {
                            message.CC.Add(cc.Trim().Replace(" ", ",").Replace(";", ","));
                        }
                        message.Body = body;
                        message.Subject = subject;

                        if (attachments != null)
                        {
                            foreach (Attachment attachment in attachments)
                            {
                                message.Attachments.Add(attachment);
                            }
                        }                       

                        message.IsBodyHtml = true;

                        await client.SendMailAsync(message);

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.ToString());
                Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                return false;
            }
        }

        #region IDisposable

        private bool _disposed;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                }
            }
            _disposed = true;
        }

        #endregion IDisposable
    }
}