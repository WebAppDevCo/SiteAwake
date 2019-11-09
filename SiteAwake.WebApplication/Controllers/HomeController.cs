using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using SiteAwake.WebApplication.Infrastructure;
using SiteAwake.WebApplication.Models;

namespace SiteAwake.WebApplication.Controllers
{
    public class HomeController : Controller
    {
        private IEmailManager EmailManager;

        public HomeController (IEmailManager emailManager)
        {
            this.EmailManager = emailManager;
        }

        public ActionResult Index()
        {
            ViewBag.Title = "Home Page";

            return View();
        }

        [HttpGet]
        public HttpStatusCodeResult LogTermsAndConditionsClicked()
        {
            Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Info, "Terms and Conditions Clicked", null);

            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }

        [HttpGet]
        public HttpStatusCodeResult LogDashboardExampleClicked()
        {
            Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Info, "Dashboard Example Clicked", null);

            return new HttpStatusCodeResult(System.Net.HttpStatusCode.OK);
        }

        [HttpPost]
        public async Task<JsonResult> SendMessage(Inquiry inquiry)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    string body = "<p style='font-family:Arial;'>A SiteAwake.net contact request has been generated.</p>";

                    body += "<table border='0' cellpadding='3' cellspacing='3' style='font-family:Arial;'>";
                    body += MapRow("Name:", !string.IsNullOrEmpty(inquiry.Name) ? inquiry.Name.Trim() : string.Empty);
                    body += MapRow("Phone:", !string.IsNullOrEmpty(inquiry.Phone) ? inquiry.Phone.Trim() : string.Empty);
                    body += MapRow("Email:", inquiry.Email.Trim());
                    body += MapRow("Message:", inquiry.Message.Trim());
                    body += "</table>";

                    body += "<p style='font-family:Arial;'><a href='https://siteawake.net'>Visit SiteAwake.net</a></p>";
                    
                    var success = EmailManager.SendEmail(System.Configuration.ConfigurationManager.AppSettings["inquiryRecipient"], "SiteAwake Contact Request", body, null, null).Result;
                    
                    //if (success)
                    //{
                        EmailManager.SendEmail(inquiry.Email.Trim(), "SiteAwake Contact Request", body, null, null);
                    //}

                    return success;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    return false;
                }
            });

            await task;

            return new JsonResult() { Data = task.Result };
        }

        private string MapRow(string name, string value)
        {
            return string.Format("<tr><td valign=\"top\">{0}</td><td valign=\"top\">{1}</td></tr>", name, value);
        }
    }
}
