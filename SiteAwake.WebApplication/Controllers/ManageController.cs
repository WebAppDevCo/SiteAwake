using System;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using SiteAwake.WebApplication.Models;
using SiteAwake.Domain.Entities;
using SiteAwake.Domain.Infrastructure;
using SiteAwake.WebApplication.Infrastructure;

namespace SiteAwake.WebApplication.Controllers
{
    [Authorize]
    public class ManageController : Controller
    {
        private IContext Context;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private int _timeZoneOffsetMinutes = 0;

        public ManageController()
        {
        }

        public ManageController(IContext context)//, ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            this.Context = context;
            //UserManager = userManager;
            //SignInManager = signInManager;

            _timeZoneOffsetMinutes = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["timeZoneOffsetMinutes"]);
        }


        public ApplicationSignInManager SignInManager
        {
            get
            {
                return _signInManager ?? HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
            private set 
            { 
                _signInManager = value; 
            }
        }

        public ApplicationUserManager UserManager
        {
            get
            {
                return _userManager ?? HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
            private set
            {
                _userManager = value;
            }
        }

        //
        // GET: /Manage/Index
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.SetTwoFactorSuccess ? "Your two-factor authentication provider has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.AddPhoneSuccess ? "Your phone number was added."
                : message == ManageMessageId.RemovePhoneSuccess ? "Your phone number was removed."
                : "";

            var userId = User.Identity.GetUserId();
            var email = await UserManager.GetEmailAsync(userId);

            var account = Context.Accounts.Single(x => x.Email == email);
            var siteMetaData = account.SiteMetadatas.ToList()[0];
            var communication = siteMetaData.Communications.Any() ? siteMetaData.Communications.OrderByDescending(x => x.Created).First() : new Communication();

            var model = new IndexViewModel
            {
                Email = email,
                HasPassword = HasPassword(),
                Url = siteMetaData.Url,
                LastWakeUpCall = communication.Created,
                Enabled = account.Enabled,
                WebsiteStatus = communication.Status == "OK" ? "Up" : "Down",
                Subscribed = account.Subscribed,
                Cancelled = account.Cancelled,
                DaysLeftInTrial = (account.Subscribed ? 0 : 30 - Convert.ToInt32((DateTime.Now.AddMinutes(_timeZoneOffsetMinutes) - account.Created).TotalDays)),
                AlertsEnabled = siteMetaData.AlertsEnabled
            };
            return View(model);
        }
              
     
        //
        // GET: /Manage/ChangePassword
        public ActionResult ChangePassword()
        {
            return View();
        }

        //
        // POST: /Manage/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var result = await UserManager.ChangePasswordAsync(User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await UserManager.FindByIdAsync(User.Identity.GetUserId());
                if (user != null)
                {
                    await SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }
                return RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }
            AddErrors(result);
            return View(model);
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult CancelService()
        {
            //if (AuthenticationManager.User != null && AuthenticationManager.User.Identity.IsAuthenticated)
            //{
            //    Response.Redirect("~/Account", true);
            //    return null;
            //}

            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> CancelService(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
            {
                // Don't reveal that the user does not exist or is not confirmed
                //return View("ForgotPasswordConfirmation");
                ModelState.AddModelError("", "Invalid login attempt.");
                return View(model);
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await SignInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:

                    var account = Context.Accounts.Single(x => x.Email.Equals(model.Email, StringComparison.OrdinalIgnoreCase));

                    account.Cancelled = true;
                    account.Modified = DateTime.Now.AddMinutes(_timeZoneOffsetMinutes);

                    await Context.SaveChangesAsync();

                    return RedirectToAction("Index", "Account", new { Message = ManageMessageId.ServiceCancelSuccess });

                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && _userManager != null)
            {
                _userManager.Dispose();
                _userManager = null;
            }

            base.Dispose(disposing);
        }

#region Helpers
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
            }
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private bool HasPassword()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }
            return false;
        }

        private bool HasPhoneNumber()
        {
            var user = UserManager.FindById(User.Identity.GetUserId());
            if (user != null)
            {
                return user.PhoneNumber != null;
            }
            return false;
        }
        
#endregion
    }
}