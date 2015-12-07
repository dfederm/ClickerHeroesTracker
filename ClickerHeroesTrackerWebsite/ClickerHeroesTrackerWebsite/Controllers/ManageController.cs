// <copyright file="ManageController.cs" company="Clicker Heroes Tracker">
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
    using ClickerHeroesTrackerWebsite.Models.Settings;
    using Microsoft.AspNet.Identity;
    using Microsoft.AspNet.Identity.Owin;
    using Microsoft.Owin.Security;

    /// <summary>
    /// The manage controller allows users to manage their settings.
    /// </summary>
    [Authorize]
    public class ManageController : Controller
    {
        // Used for XSRF protection when adding external logins
        private const string XsrfKey = "XsrfId";

        private readonly IUserSettingsProvider userSettingsProvider;

        private ApplicationSignInManager signInManager;
        private ApplicationUserManager userManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="ManageController"/> class.
        /// </summary>
        public ManageController(IUserSettingsProvider userSettingsProvider)
        {
            this.userSettingsProvider = userSettingsProvider;
        }

        /// <summary>
        /// Represents a status message to the user
        /// </summary>
        /// <remarks>
        /// Although public, this enum is not intended to be used outside the <see cref="ManageController"/>
        /// </remarks>
        public enum ManageMessageId
        {
            /// <summary>
            /// The user's password was successfully changed.
            /// </summary>
            ChangePasswordSuccess,

            /// <summary>
            /// The user's password was successfully set.
            /// </summary>
            SetPasswordSuccess,

            /// <summary>
            /// The user's external login was successfully removed.
            /// </summary>
            RemoveLoginSuccess,

            /// <summary>
            /// An error happened
            /// </summary>
            Error
        }

        private ApplicationSignInManager SignInManager
        {
            get
            {
                return this.signInManager ?? (this.signInManager = this.HttpContext.GetOwinContext().Get<ApplicationSignInManager>());
            }
        }

        private ApplicationUserManager UserManager
        {
            get
            {
                return this.userManager ?? (this.userManager = this.HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>());
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
        /// GET: /Manage/Index
        /// </summary>
        /// <param name="message">The status of the user's last operation</param>
        /// <returns>The user's settings view</returns>
        public async Task<ActionResult> Index(ManageMessageId? message)
        {
            this.ViewBag.StatusMessage =
                message == ManageMessageId.ChangePasswordSuccess ? "Your password has been changed."
                : message == ManageMessageId.SetPasswordSuccess ? "Your password has been set."
                : message == ManageMessageId.Error ? "An error has occurred."
                : string.Empty;

            var userId = this.User.Identity.GetUserId();

            var userSettings = this.userSettingsProvider.Get(userId);

            return await this.GetIndexResult(userId, userSettings);
        }

        /// <summary>
        /// The user submits a change to their settings
        /// </summary>
        /// <param name="indexViewModel">The user-submitted settings data</param>
        /// <returns>The user's settings view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Index(IndexViewModel indexViewModel)
        {
            this.ViewBag.StatusMessage = "Changes Saved";

            var userId = this.User.Identity.GetUserId();

            var userSettings = this.userSettingsProvider.Get(userId);

            userSettings.TimeZone = TimeZoneInfo.FindSystemTimeZoneById(indexViewModel.TimeZoneId);
            userSettings.AreUploadsPublic = indexViewModel.AreUploadsPublic;
            userSettings.UseReducedSolomonFormula = indexViewModel.SolomonFormula.Equals("Log", StringComparison.OrdinalIgnoreCase);
            userSettings.PlayStyle = indexViewModel.PlayStyle.SafeParseEnum<PlayStyle>();

            return await this.GetIndexResult(userId, userSettings);
        }

        /// <summary>
        /// POST: /Manage/RemoveLogin
        /// </summary>
        /// <param name="loginProvider">The external login provider name</param>
        /// <param name="providerKey">The login provider key</param>
        /// <returns>A redirection to the manage logins page</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> RemoveLogin(string loginProvider, string providerKey)
        {
            ManageMessageId? message;
            var result = await this.UserManager.RemoveLoginAsync(this.User.Identity.GetUserId(), new UserLoginInfo(loginProvider, providerKey));
            if (result.Succeeded)
            {
                var user = await this.UserManager.FindByIdAsync(this.User.Identity.GetUserId());
                if (user != null)
                {
                    await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                message = ManageMessageId.RemoveLoginSuccess;
            }
            else
            {
                message = ManageMessageId.Error;
            }

            return this.RedirectToAction("ManageLogins", new { Message = message });
        }

        /// <summary>
        /// GET: /Manage/ChangePassword
        /// </summary>
        /// <returns>The change password view</returns>
        public ActionResult ChangePassword()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Manage/ChangePassword
        /// </summary>
        /// <param name="model">The user-submitted change password data</param>
        /// <returns>A redirect to the settings page or an error view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (!this.ModelState.IsValid)
            {
                return this.View(model);
            }

            var result = await this.UserManager.ChangePasswordAsync(this.User.Identity.GetUserId(), model.OldPassword, model.NewPassword);
            if (result.Succeeded)
            {
                var user = await this.UserManager.FindByIdAsync(this.User.Identity.GetUserId());
                if (user != null)
                {
                    await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                }

                return this.RedirectToAction("Index", new { Message = ManageMessageId.ChangePasswordSuccess });
            }

            this.AddErrors(result);
            return this.View(model);
        }

        /// <summary>
        /// GET: /Manage/SetPassword
        /// </summary>
        /// <returns>The set password view</returns>
        public ActionResult SetPassword()
        {
            return this.View();
        }

        /// <summary>
        /// POST: /Manage/SetPassword
        /// </summary>
        /// <param name="model">The user-submitted password data</param>
        /// <returns>A redirect to the settings page or an error view</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SetPassword(SetPasswordViewModel model)
        {
            if (this.ModelState.IsValid)
            {
                var result = await this.UserManager.AddPasswordAsync(this.User.Identity.GetUserId(), model.NewPassword);
                if (result.Succeeded)
                {
                    var user = await this.UserManager.FindByIdAsync(this.User.Identity.GetUserId());
                    if (user != null)
                    {
                        await this.SignInManager.SignInAsync(user, isPersistent: false, rememberBrowser: false);
                    }

                    return this.RedirectToAction("Index", new { Message = ManageMessageId.SetPasswordSuccess });
                }

                this.AddErrors(result);
            }

            // If we got this far, something failed, redisplay form
            return this.View(model);
        }

        /// <summary>
        /// GET: /Manage/ManageLogins
        /// </summary>
        /// <param name="message">The status of the user's last operation</param>
        /// <returns>The user's settings view</returns>
        public async Task<ActionResult> ManageLogins(ManageMessageId? message)
        {
            this.ViewBag.StatusMessage =
                message == ManageMessageId.RemoveLoginSuccess ? "The external login was removed."
                : message == ManageMessageId.Error ? "An error has occurred."
                : string.Empty;
            var user = await this.UserManager.FindByIdAsync(this.User.Identity.GetUserId());
            if (user == null)
            {
                return this.View("Error");
            }

            var userLogins = await this.UserManager.GetLoginsAsync(this.User.Identity.GetUserId());
            var otherLogins = this.AuthenticationManager
                .GetExternalAuthenticationTypes()
                .Where(auth => userLogins.All(ul => !string.Equals(auth.AuthenticationType, ul.LoginProvider, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            this.ViewBag.ShowRemoveButton = user.PasswordHash != null || userLogins.Count > 1;
            return this.View(new ManageLoginsViewModel
            {
                CurrentLogins = userLogins,
                OtherLogins = otherLogins
            });
        }

        /// <summary>
        /// POST: /Manage/LinkLogin
        /// </summary>
        /// <param name="provider">The external login provider name</param>
        /// <returns>A challenge result for the link</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult LinkLogin(string provider)
        {
            // Request a redirect to the external login provider to link a login for the current user
            return new AccountController.ChallengeResult(provider, this.Url.Action("LinkLoginCallback", "Manage"), this.User.Identity.GetUserId());
        }

        /// <summary>
        /// GET: /Manage/LinkLoginCallback
        /// </summary>
        /// <returns>A redirect to the external login manage page</returns>
        public async Task<ActionResult> LinkLoginCallback()
        {
            var loginInfo = await this.AuthenticationManager.GetExternalLoginInfoAsync(XsrfKey, this.User.Identity.GetUserId());
            if (loginInfo == null)
            {
                return this.RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
            }

            var result = await this.UserManager.AddLoginAsync(this.User.Identity.GetUserId(), loginInfo.Login);
            return result.Succeeded ? this.RedirectToAction("ManageLogins") : this.RedirectToAction("ManageLogins", new { Message = ManageMessageId.Error });
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

        private async Task<ActionResult> GetIndexResult(string userId, IUserSettings userSettings)
        {
            var model = new IndexViewModel
            {
                HasPassword = this.HasPassword(),
                Logins = await this.UserManager.GetLoginsAsync(userId),
                TimeZoneId = userSettings.TimeZone.Id,
                AreUploadsPublic = userSettings.AreUploadsPublic,
                SolomonFormula = userSettings.UseReducedSolomonFormula ? "Log" : "Ln",
                PlayStyle = userSettings.PlayStyle.ToString()
            };

            return this.View(model);
        }

        private void AddErrors(IdentityResult result)
        {
            foreach (var error in result.Errors)
            {
                this.ModelState.AddModelError(string.Empty, error);
            }
        }

        private bool HasPassword()
        {
            var user = this.UserManager.FindById(this.User.Identity.GetUserId());
            if (user != null)
            {
                return user.PasswordHash != null;
            }

            return false;
        }
    }
}