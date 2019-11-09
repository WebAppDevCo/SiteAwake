using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Threading.Tasks;
using SiteAwake.WebApplication.Infrastructure;
using SiteAwake.WebApplication.Models;
using SiteAwake.Domain.Entities;
using SiteAwake.Domain.Infrastructure;
using System.Configuration;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin.Security;
using System.Data;
using System.Data.SqlClient;

namespace SiteAwake.WebApplication.Controllers
{
    [Authorize]
    public class AccountController : Controller
    {
        private IContext Context;
        private IEmailManager EmailManager;
        private ApplicationSignInManager _signInManager;
        private ApplicationUserManager _userManager;
        private int _timeZoneOffsetMinutes = 0;

        #region Properties

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

        #endregion

        public AccountController(IContext context, IEmailManager emailManager)//, ApplicationUserManager userManager, ApplicationSignInManager signInManager)
        {
            this.Context = context;
            this.EmailManager = emailManager;
            //UserManager = userManager;
            //SignInManager = signInManager;

            _timeZoneOffsetMinutes = Convert.ToInt32(System.Configuration.ConfigurationManager.AppSettings["timeZoneOffsetMinutes"]);
        }

        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            ViewBag.StatusMessage =
                message == ManageMessageId.EditSettingsSuccess ? "Your settings have been changed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : message == ManageMessageId.ServiceCancelSuccess ? "Your service has been cancelled."
                : "";

            var userId = User.Identity.GetUserId();
            var email = await UserManager.GetEmailAsync(userId);

            var account = Context.Accounts.Single(x => x.Email == email);
            var siteMetaData = account.SiteMetadatas.ToList()[0];
            var communication = siteMetaData.Communications.Any() ? siteMetaData.Communications.OrderByDescending(x => x.Created).First() : new Communication();

            var model = new IndexViewModel
            {
                Email = email,
                HasPassword = true,
                Url = siteMetaData.Url,
                LastWakeUpCall = communication.Created,
                Enabled = account.Enabled,
                WebsiteStatus = communication.Status == "OK" ? "Up" : "Down",
                Subscribed = account.Subscribed,
                Cancelled = account.Cancelled,
                Interval = siteMetaData.Interval,
                DaysLeftInTrial = (account.Subscribed ? 0 : 30 - Convert.ToInt32((DateTime.Now.AddMinutes(_timeZoneOffsetMinutes) - account.Created).TotalDays)),
                AlertsEnabled = siteMetaData.AlertsEnabled,
                Uptime = new TimeSpan(0, Context.Database.SqlQuery<int>("dbo.GetUptimeMinutes @SiteMetadataId", new SqlParameter("SiteMetadataId", siteMetaData.Id)).First(), 0),
                MillisecondsElapsed = communication.MillisecondsElapsed
            };
            return View(model);
        }

        //
        // GET: /Account/Login
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            if (AuthenticationManager.User != null && AuthenticationManager.User.Identity.IsAuthenticated)
            {
                Response.Redirect("~/Account", true);
                return null;
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
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
                    return RedirectToLocal(returnUrl);
                //case SignInStatus.LockedOut:
                //    return View("Lockout");
                //case SignInStatus.RequiresVerification:
                //    return RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = model.RememberMe });
                //case SignInStatus.Failure:
                default:
                    ModelState.AddModelError("", "Invalid login attempt.");
                    return View(model);
            }
        }
        
        //
        // POST: /Account/LogOff
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> Create(SignUpViewModel signUpVM)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        signUpVM.ErrorMessage = "Invalid model";
                        return signUpVM;
                    }

                    signUpVM.Email = signUpVM.Email.Trim().ToLower();
                    signUpVM.Url = signUpVM.Url.Trim().ToLower();

                    var websiteUrl = ConfigurationManager.AppSettings["websiteUrl"].TrimEnd(new char['/']);

                    var accountValidation = new AccountValidation();

                    var validationMessage = accountValidation.IsPasswordValid(signUpVM.Password);

                    if (!validationMessage.IsValid)
                    {
                        signUpVM.ErrorMessage = validationMessage.ErrorMessage;
                        return signUpVM;
                    }

                    validationMessage = accountValidation.IsWakeUpIntervalValid(signUpVM.WakeUpIntervalMinutes);

                    if (!validationMessage.IsValid)
                    {
                        signUpVM.ErrorMessage = validationMessage.ErrorMessage;
                        return signUpVM;
                    }

                    validationMessage = accountValidation.IsUserNameValid(Context, 0, signUpVM.Email);

                    if (!validationMessage.IsValid)
                    {
                        signUpVM.ErrorMessage = validationMessage.ErrorMessage;
                        return signUpVM;
                    }

                    validationMessage = accountValidation.IsUrlValid(Context, 0, signUpVM.Url);

                    if (!validationMessage.IsValid)
                    {
                        signUpVM.ErrorMessage = validationMessage.ErrorMessage;
                        return signUpVM;
                    }

                    //create aspnet user
                    var user = new ApplicationUser { UserName = signUpVM.Email.ToLower(), Email = signUpVM.Email.ToLower() };
                    var result = UserManager.CreateAsync(user, signUpVM.Password).Result;
                    var callbackUrl = string.Empty;
                    if (result.Succeeded)
                    {
                        //SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = UserManager.GenerateEmailConfirmationTokenAsync(user.Id).Result;
                        callbackUrl = Url.Action("VerifyEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                        // await UserManager.SendEmailAsync(user.Id, "Confirm your account", "Please confirm your account by clicking <a href=\"" + callbackUrl + "\">here</a>");

                        //return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        var errors = string.Empty;
                        foreach (string error in result.Errors)
                        {
                            errors += error + "<br/>";
                        }

                        signUpVM.ErrorMessage = errors;
                        return signUpVM;
                    }

                    Context.Accounts.Add(
                        new Account()
                        {
                            Created = DateTime.Now.AddMinutes(_timeZoneOffsetMinutes),
                            Email = signUpVM.Email.ToLower(),
                            Enabled = false,
                            Subscribed = false,
                            Cancelled = false,
                            Verified = false,
                            SiteMetadatas = new List<SiteMetadata>()
                            {
                                new SiteMetadata()
                                {
                                    Created = DateTime.Now.AddMinutes(_timeZoneOffsetMinutes),
                                    AlertsEnabled = false,
                                    AlertSent = false,
                                    Interval = signUpVM.WakeUpIntervalMinutes,
                                    Url = signUpVM.Url.ToLower()
                                }
                            }
                        });

                    Context.SaveChanges();

                    //send verification email
                    try
                    {
                        string body = "<p style='font-family:Arial;'>Thank you for signing up with SiteAwake.</p>";

                        body += "<p style='font-family:Arial;'>Please confirm your account by clicking <a href='" + callbackUrl + "'>here</a></p>";

                        body += "<p style='font-family:Arial;'>Thank you!<br/><a href='" + websiteUrl + "'>SiteAwake.net</a></p>";

                        EmailManager.SendEmail(signUpVM.Email.Trim(), "SiteAwake Email Verification", body, null, null);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                        //return false;
                    }

                    return signUpVM;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);

                    signUpVM.ErrorMessage = "An unspecified error has occurred.";

                    return signUpVM;
                }
            });

            await task;

            return new JsonResult() { Data = task.Result };
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<JsonResult> ResendVerification(SignUpViewModel signUpVM)
        {
            var task = Task.Run(() =>
            {
                try
                {
                    if (!ModelState.IsValid)
                    {
                        signUpVM.ErrorMessage = "Invalid model";
                        return signUpVM;
                    }

                    signUpVM.Email = signUpVM.Email.Trim().ToLower();
                    
                    //get aspnet user
                    var user = UserManager.FindByEmail(signUpVM.Email);
                  
                    var callbackUrl = string.Empty;
                    if (user != null && !user.EmailConfirmed)
                    {
                        // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                        // Send an email with this link
                        string code = UserManager.GenerateEmailConfirmationTokenAsync(user.Id).Result;
                        callbackUrl = Url.Action("VerifyEmail", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);
                    }
                    else
                    {
                        signUpVM.ErrorMessage = "An unspecified error occurred.";
                        return signUpVM;
                    }

                    var websiteUrl = ConfigurationManager.AppSettings["websiteUrl"].TrimEnd(new char['/']);

                    //send verification email
                    try
                    {
                        string body = "<p style='font-family:Arial;'>Thank you for signing up with SiteAwake.</p>";

                        body += "<p style='font-family:Arial;'>Please confirm your account by clicking <a href='" + callbackUrl + "'>here</a></p>";

                        body += "<p style='font-family:Arial;'>Thank you!<br/><a href='" + websiteUrl + "'>SiteAwake.net</a></p>";

                        EmailManager.SendEmail(signUpVM.Email.Trim(), "SiteAwake Email Verification", body, null, null);
                    }
                    catch (Exception ex)
                    {
                        System.Diagnostics.Debug.WriteLine(ex.ToString());
                        Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                        //return false;
                    }

                    return signUpVM;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);

                    signUpVM.ErrorMessage = "An unspecified error has occurred.";

                    return signUpVM;
                }
            });

            await task;

            return new JsonResult() { Data = task.Result };
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> VerifyEmail(string userId, string code)
        {
            SignUpViewModel signUp = new SignUpViewModel();

            var task = Task.Run(() =>
            {
                try
                {
                    if (userId == null || code == null)
                    {
                        signUp.ErrorMessage = "The verification code is not valid.";
                        return signUp;
                    }

                    //confirm the email and set the account flags to make active
                    var result = UserManager.ConfirmEmailAsync(userId, code).Result;

                    if (result.Succeeded)
                    {
                        var user = UserManager.FindByIdAsync(userId).Result;
                        var account = Context.Accounts.Single(x => x.Email.Equals(user.Email, StringComparison.OrdinalIgnoreCase));

                        account.Verified = true;
                        account.Enabled = true;
                        account.Modified = DateTime.Now.AddMinutes(_timeZoneOffsetMinutes);

                        Context.SaveChanges();
                        
                        //message is handled in the UI and this is a placeholder
                        signUp.StatusMessage = "Your email has been verified. Click the \"Log In\" link at the top of the page to log into your account.";
                    }
                    else
                    {
                        signUp.ErrorMessage = "The verification code is not valid.";
                    }

                    return signUp;
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);

                    signUp.ErrorMessage = "An unspecified error has occurred.";

                    return signUp;
                }
            });

            await task;

            return View(signUp);
        }

        //
        // GET: /Account/ForgotPassword
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return View();
        }

        //
        // POST: /Account/ForgotPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var websiteUrl = ConfigurationManager.AppSettings["websiteUrl"].TrimEnd(new char['/']);

                var user = await UserManager.FindByNameAsync(model.Email);
                if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: Request.Url.Scheme);		
                // await UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                // return RedirectToAction("ForgotPasswordConfirmation", "Account");

                //send verification email
                try
                {
                    string body = "<p style='font-family:Arial;'>Please reset your password by clicking <a href='" + callbackUrl + "'>here</a></p>";

                    body += "<p style='font-family:Arial;'>Thank you!<br/><a href='" + websiteUrl + "'>SiteAwake.net</a></p>";

                    await EmailManager.SendEmail(model.Email.Trim(), "SiteAwake Reset Password", body, null, null);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.ToString());
                    Logger.Log(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType, Logger.EventType.Error, ex.Message, ex);
                    //return false;
                }

                return RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        //
        // GET: /Account/ForgotPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/ResetPassword
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? View("Error") : View();
        }

        //
        // POST: /Account/ResetPassword
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }
            var user = await UserManager.FindByNameAsync(model.Email);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            var result = await UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return RedirectToAction("ResetPasswordConfirmation", "Account");
            }
            AddErrors(result);
            return View();
        }

        //
        // GET: /Account/ResetPasswordConfirmation
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        //
        // GET: /Account/EditSettings
        public async Task<ActionResult> EditSettings()
        {
            var userId = User.Identity.GetUserId();
            var email = await UserManager.GetEmailAsync(userId);

            var account = Context.Accounts.Single(x => x.Email == email);
            var siteMetaData = account.SiteMetadatas.ToList()[0];
            var communication = siteMetaData.Communications.Any() ? siteMetaData.Communications.OrderByDescending(x => x.Created).First() : new Communication();

            var model = new IndexViewModel
            {
                Email = email,
                HasPassword = true,
                Url = siteMetaData.Url,
                LastWakeUpCall = communication.Created,
                Enabled = account.Enabled,
                WebsiteStatus = communication.Status == "OK" ? "Up" : "Down",
                Subscribed = account.Subscribed,
                Cancelled = account.Cancelled,
                Interval = siteMetaData.Interval,
                DaysLeftInTrial = (account.Subscribed ? 0 : 30 - Convert.ToInt32((DateTime.Now.AddMinutes(_timeZoneOffsetMinutes) - account.Created).TotalDays)),
                AlertsEnabled = siteMetaData.AlertsEnabled
            };
            return View(model);
        }

        //
        // POST: /Account/EditSettings
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> EditSettings(IndexViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            if (model.Interval < 1 || model.Interval > 60)
            {
                ModelState.AddModelError("", "Interval must be from 1 to 60 minutes");
                return View(model);
            }

            if (string.IsNullOrEmpty(model.Url))
            {
                ModelState.AddModelError("", "Url is required");
                return View(model);
            }
                       

            var userId = User.Identity.GetUserId();
            var email = await UserManager.GetEmailAsync(userId);

            var account = Context.Accounts.Single(x => x.Email == email);
            var siteMetaData = account.SiteMetadatas.ToList()[0];

            AccountValidation accountValidation = new AccountValidation();
            AccountValidation.ValidationMessage validationMessage = accountValidation.IsUrlValid(Context, account.Id, model.Url);

            if (!validationMessage.IsValid)
            {
                ModelState.AddModelError("", validationMessage.ErrorMessage);
                return View(model);
            }
            
            siteMetaData.Url = model.Url.ToLower();
            siteMetaData.Interval = model.Interval;
            siteMetaData.Modified = DateTime.Now.AddMinutes(_timeZoneOffsetMinutes);
            siteMetaData.AlertsEnabled = model.AlertsEnabled;
            siteMetaData.Processing = false;
            siteMetaData.AlertSent = false;

            account.Enabled = model.Enabled;
            account.Modified = DateTime.Now.AddMinutes(_timeZoneOffsetMinutes);

            await Context.SaveChangesAsync();
            
            return RedirectToAction("Index", "Account", new { Message = ManageMessageId.EditSettingsSuccess });
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Account");
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return HttpContext.GetOwinContext().Authentication;
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
    }
}