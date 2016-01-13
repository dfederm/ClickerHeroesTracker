// <copyright file="AccountController.cs" company="Clicker Heroes Tracker">
// Copyright (c) Clicker Heroes Tracker. All rights reserved.
// </copyright>

namespace ClickerHeroesTrackerWebsite.Controllers
{
    using System;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Web;
    using System.Web.Mvc;
    using ClickerHeroesTrackerWebsite.Models;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;
    using Models.Account;
    using Models.Settings;

    /// <summary>
    /// The Account controller is where users register and log in.
    /// </summary>
    [Authorize]
    public class AccountController : Controller
    {
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private readonly IUserSettingsProvider userSettingsProvider;

        private ApplicationSignInManager signInManager;
        private ApplicationUserManager userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="AccountController"/> class.
        /// </summary>
        /// <param name="userSettingsProvider">The user settings provider</param>
        public AccountController(IUserSettingsProvider userSettingsProvider)
        {
            this.userSettingsProvider = userSettingsProvider;
        }

        private ApplicationSignInManager SignInManager
        {
            get
            {
                return this.signInManager ?? this.HttpContext.GetOwinContext().Get<ApplicationSignInManager>();
            }
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                return this.userManager ?? this.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();
            }
        }

        private IAuthenticationManager AuthenticationManager
        {
            get
            {
                return this.HttpContext.GetOwinContext().Authentication;
            }
        }

        /// <summary>
        /// GET: /Account/Login
        /// </summary>
        /// <param name="returnUrl">The url to redirect back to after login</param>
        /// <returns>The login view</returns>
        [AllowAnonymous]
        public ActionResult Login(string returnUrl)
        {
            this.ViewBag.ReturnUrl = returnUrl;
            return this.View();
        }

        /// <summary>
        /// POST: /Account/Login
        /// </summary>
        /// <param name="model">The user-submitted login data</param>
        /// <param name="returnUrl">The url to redirect back to after login</param>
        /// <returns>A redirect result if successful, and error view otherwise</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Login(LoginViewModel model, string returnUrl)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var userName = model.UserName;

            // Allow the user to log in with their email address too.
            // We already check that usernames are only "word" chars (\w+), so this check is sufficient.
            if (userName.Contains("@"))
            {
                var user = await this.UserManager.FindByEmailAsync(userName);
                userName = user.UserName;
            }

            // This doesn't count login failures towards account lockout
            // To enable password failures to trigger account lockout, change to shouldLockout: true
            var result = await this.SignInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, shouldLockout: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return this.RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return this.View("Lockout");
                case SignInStatus.Failure:
                default:
                    this.ModelState.AddModelError(string.Empty, "Invalid login attempt.");
                    return this.View(model);
            }
        }

        /// <summary>
        /// GET: /Account/Register
        /// </summary>
        /// <returns>The register view</returns>
        [AllowAnonymous]
        public ActionResult Register()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Account/Register
        /// </summary>
        /// <param name="model">The user-submitted registration data</param>
        /// <returns>A redirect result if successful, and error view otherwise</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Register(RegisterViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var user = new ApplicationUser { UserName = model.UserName, Email = model.Email };
                var result = await this.UserManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    // Save the user's initial settings
                    var userSettings = this.userSettingsProvider.Get(user.Id);
                    userSettings.TimeZone = TimeZoneInfo.GetSystemTimeZones().FirstOrDefault(tz => tz.GetUtcOffset(DateTime.UtcNow).TotalMinutes == -model.TimezoneOffset);

                    await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);

                    return this.RedirectToAction("Index", "Home");
                }

                this.AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        /// <summary>
        /// GET: /Account/ConfirmEmail
        /// </summary>
        /// <param name="userId">The user id</param>
        /// <param name="code">The confirmation code sent to the user</param>
        /// <returns>A view with the status</returns>
        [AllowAnonymous]
        public async Task<ActionResult> ConfirmEmail(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return this.View("Error");
            }

            var result = await this.UserManager.ConfirmEmailAsync(userId, code);
            return this.View(result.Succeeded ? "ConfirmEmail" : "Error");
        }

        /// <summary>
        /// GET: /Account/ForgotPassword
        /// </summary>
        /// <returns>The forgot password view</returns>
        [AllowAnonymous]
        public ActionResult ForgotPassword()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Account/ForgotPassword
        /// </summary>
        /// <param name="model">The user-submitted forgot password data</param>
        /// <returns>The forgot password confirmation view</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                // If we ever use email confirmation, change to this
                // if (user == null || !(await UserManager.IsEmailConfirmedAsync(user.Id)))
                var user = await this.UserManager.FindByEmailAsync(model.Email);
                if (user == null)
                {
                    // Don't reveal that the user does not exist or is not confirmed
                    return this.View("ForgotPasswordConfirmation");
                }

                // For more information on how to enable account confirmation and password reset please visit http://go.microsoft.com/fwlink/?LinkID=320771
                // Send an email with this link
                string code = await this.UserManager.GeneratePasswordResetTokenAsync(user.Id);
                var callbackUrl = this.Url.Action("ResetPassword", "Account", new { userId = user.Id, code = code }, protocol: this.Request.Url.Scheme);
                await this.UserManager.SendEmailAsync(user.Id, "Reset Password", "Please reset your password by clicking <a href=\"" + callbackUrl + "\">here</a>");
                return this.RedirectToAction("ForgotPasswordConfirmation", "Account");
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        /// <summary>
        /// GET: /Account/ForgotPasswordConfirmation
        /// </summary>
        /// <returns>The forgot password confirmation view</returns>
        [AllowAnonymous]
        public ActionResult ForgotPasswordConfirmation()
        {
            return this.View();
        }

        /// <summary>
        /// GET: /Account/ResetPassword
        /// </summary>
        /// <param name="code">The confirmation code</param>
        /// <returns>The reset password view</returns>
        [AllowAnonymous]
        public ActionResult ResetPassword(string code)
        {
            return code == null ? this.View("Error") : this.View();
        }

        /// <summary>
        /// POST: /Account/ResetPassword
        /// </summary>
        /// <param name="model">The user-submitted reset password view</param>
        /// <returns>A redirection to the reset password confirmation view</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var user = await this.UserManager.FindByNameAsync(model.UserName);
            if (user == null)
            {
                // Don't reveal that the user does not exist
                return this.RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            var result = await this.UserManager.ResetPasswordAsync(user.Id, model.Code, model.Password);
            if (result.Succeeded)
            {
                return this.RedirectToAction("ResetPasswordConfirmation", "Account");
            }

            this.AddErrors(result);
            return this.View();
        }

        /// <summary>
        /// GET: /Account/ResetPasswordConfirmation
        /// </summary>
        /// <returns>The reset password confirmation view</returns>
        [AllowAnonymous]
        public ActionResult ResetPasswordConfirmation()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Account/ExternalLogin
        /// </summary>
        /// <param name="provider">The external login provider</param>
        /// <param name="returnUrl">The url to redirect back to</param>
        /// <returns>The challenge result</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ExternalLogin(string provider, string returnUrl)
        {
            // Request a redirect to the external login provider
            return new ChallengeResult(provider, this.Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl }));
        }

        /// <summary>
        /// GET: /Account/ExternalLoginCallback
        /// </summary>
        /// <param name="returnUrl">The url to redirect back to</param>
        /// <returns>A redirection result if the challenge succeeded, an error view otherwise</returns>
        [AllowAnonymous]
        public async Task<ActionResult> ExternalLoginCallback(string returnUrl)
        {
            var loginInfo = await this.AuthenticationManager.GetExternalLoginInfoAsync();
            if (loginInfo == null)
            {
                return this.RedirectToAction("Login");
            }

            // Sign in the user with this external login provider if the user already has a login
            var result = await this.SignInManager.ExternalSignInAsync(loginInfo, isPersistent: false);
            switch (result)
            {
                case SignInStatus.Success:
                    return this.RedirectToLocal(returnUrl);
                case SignInStatus.LockedOut:
                    return this.View("Lockout");
                case SignInStatus.RequiresVerification:
                    return this.RedirectToAction("SendCode", new { ReturnUrl = returnUrl, RememberMe = false });
                case SignInStatus.Failure:
                default:
                    // If the user does not have an account, then prompt the user to create an account
                    this.ViewBag.ReturnUrl = returnUrl;
                    this.ViewBag.LoginProvider = loginInfo.Login.LoginProvider;
                    return this.View("ExternalLoginConfirmation", new ExternalLoginConfirmationViewModel { UserName = loginInfo.DefaultUserName });
            }
        }

        /// <summary>
        /// POST: /Account/ExternalLoginConfirmation
        /// </summary>
        /// <param name="model">The user-submitted external login confirmation data</param>
        /// <param name="returnUrl">The url to redirect back to</param>
        /// <returns>A redirection result if the challenge succeeded, an error view otherwise</returns>
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ExternalLoginConfirmation(ExternalLoginConfirmationViewModel model, string returnUrl)
        {
            if (this.User.Identity.IsAuthenticated)
            {
                return this.RedirectToAction("Index", "Manage");
            }

            if (this.ModelState.IsValid)
            {
                // Get the information about the user from the external login provider
                var info = await this.AuthenticationManager.GetExternalLoginInfoAsync();
                if (info == null)
                {
                    return this.View("ExternalLoginFailure");
                }

                var user = new ApplicationUser { UserName = model.UserName, Email = info.Email };
                var result = await this.UserManager.CreateAsync(user);
                if (result.Succeeded)
                {
                    result = await this.UserManager.AddLoginAsync(user.Id, info.Login);
                    if (result.Succeeded)
                    {
                        await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                        return this.RedirectToLocal(returnUrl);
                    }
                }

                this.AddErrors(result);
            }

            this.ViewBag.ReturnUrl = returnUrl;
            return this.View(model);
        }

        /// <summary>
        /// POST: /Account/LogOff
        /// </summary>
        /// <returns>A redirection to the homepage</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LogOff()
        {
            this.AuthenticationManager.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
            return this.RedirectToAction("Index", "Home");
        }

        /// <summary>
        /// GET: /Account/ExternalLoginFailure
        /// </summary>
        /// <returns>An external login failure view</returns>
        [AllowAnonymous]
        public ActionResult ExternalLoginFailure()
        {
            return this.View();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (this.userManager != null)
                {
                    this.userManager.Dispose();
                    this.userManager = null;
                }

                if (this.signInManager != null)
                {
                    this.signInManager.Dispose();
                    this.signInManager = null;
                }
            }

            base.Dispose(disposing);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error);
            }
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (this.Url.IsLocalUrl(returnUrl))
            {
                return this.Redirect(returnUrl);
            }

            return this.RedirectToAction("Index", "Home");
        }

        internal class ChallengeResult : HttpUnauthorizedResult
        {
            public ChallengeResult(string provider, string redirectUri)
                : this(provider, redirectUri, null)
            {
            }

            public ChallengeResult(string provider, string redirectUri, string userId)
            {
                this.LoginProvider = provider;
                this.RedirectUri = redirectUri;
                this.UserId = userId;
            }

            public string LoginProvider { get; set; }

            public string RedirectUri { get; set; }

            public string UserId { get; set; }

            public override void ExecuteResult(ControllerContext context)
            {
                var properties = new AuthenticationProperties { RedirectUri = this.RedirectUri };
                if (this.UserId != null)
                {
                    properties.Dictionary[XsrfKey] = this.UserId;
                }

                context.HttpContext.GetOwinContext().Authentication.Challenge(properties, this.LoginProvider);
            }
        }
    }
}