﻿namespace EA.Weee.Web.Areas.Admin.Controllers
{
    using Api.Client;
    using Api.Client.Entities;
    using Base;
    using Core;
    using EA.Weee.Requests.Users;
    using Infrastructure;
    using Microsoft.Owin.Security;
    using Prsd.Core.Web.ApiClient;
    using Prsd.Core.Web.Mvc.Extensions;
    using Prsd.Core.Web.OAuth;
    using Prsd.Core.Web.OpenId;
    using Services;
    using System;
    using System.Linq;
    using System.Net.Mail;
    using System.Threading.Tasks;
    using System.Web.Mvc;
    using Thinktecture.IdentityModel.Client;
    using ViewModels;
    using Weee.Requests.Admin;
    using UserInfoClient = Thinktecture.IdentityModel.Client.UserInfoClient;

    public class AccountController : AdminController
    {
        private readonly Func<IWeeeClient> apiClient;
        private readonly IAuthenticationManager authenticationManager;
        private readonly Func<IOAuthClient> oauthClient;
        private readonly Func<IUserInfoClient> userInfoClient;
        private readonly IExternalRouteService externalRouteService;

        public AccountController(
            Func<IWeeeClient> apiClient,
            IAuthenticationManager authenticationManager,
            Func<IOAuthClient> oauthClient,
            Func<IUserInfoClient> userInfoClient,
            IExternalRouteService externalRouteService)
        {
            this.apiClient = apiClient;
            this.oauthClient = oauthClient;
            this.authenticationManager = authenticationManager;
            this.userInfoClient = userInfoClient;
            this.externalRouteService = externalRouteService;
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult Create()
        {
            return View(new InternalUserCreationViewModel());
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(InternalUserCreationViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var userCreationData = new UserCreationData
            {
                Email = model.Email,
                FirstName = model.Name,
                Surname = model.Surname,
                Password = model.Password,
                ConfirmPassword = model.ConfirmPassword,
                Claims = new[]
                {
                    Claims.CanAccessInternalArea
                },
                ActivationBaseUrl = externalRouteService.ActivateInternalUserAccountUrl,
            };

            try
            {
                using (var client = apiClient())
                {
                    var userId = await client.User.CreateUserAsync(userCreationData);
                    var signInResponse = await oauthClient().GetAccessTokenAsync(userCreationData.Email, userCreationData.Password);
                    authenticationManager.SignIn(signInResponse.GenerateUserIdentity());
                    var competentUserId = await client.SendAsync(signInResponse.AccessToken, new AddCompetentAuthorityUser(userId));
                }

                return RedirectToAction("AdminAccountActivationRequired");
            }
            catch (ApiBadRequestException ex)
            {
                this.HandleBadRequest(ex);

                if (ModelState.IsValid)
                {
                    throw;
                }
            }
            catch (SmtpException)
            {
                ViewBag.Errors = new[] { "The activation email was not sent, please try again later." };
            }

            return View(model);
        }

        [HttpGet]
        public ActionResult AdminAccountActivationRequired()
        {
            var email = User.GetEmailAddress();
            if (!string.IsNullOrEmpty(email))
            {
                ViewBag.UserEmailAddress = User.GetEmailAddress();
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> AdminAccountActivationRequired(FormCollection model)
        {
            var emailAddress = authenticationManager.User.GetEmailAddress();
            if (!string.IsNullOrEmpty(emailAddress))
            {
                ViewBag.UserEmailAddress = emailAddress;
            }

            using (var client = apiClient())
            {
                string accessToken = authenticationManager.User.GetAccessToken();

                string activationBaseUrl = externalRouteService.ActivateInternalUserAccountUrl;

                await client.User.ResendActivationEmail(accessToken, activationBaseUrl);
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> ActivateUserAccount(Guid id, string code)
        {
            using (var client = apiClient())
            {
                bool result =
                    await
                        client.User.ActivateUserAccountEmailAsync(new ActivatedUserAccountData
                        {
                            Id = id,
                            Code = code
                        });

                if (!result)
                {
                    return RedirectToAction("AdminAccountActivationRequired", "Account", new { area = "Admin" });
                }
            }

            return View();
        }

        [HttpGet]
        [AllowAnonymous]
        public ActionResult SignIn(string returnUrl)
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToLocal(returnUrl);
            }
            ViewBag.ReturnUrl = returnUrl;
            return View("SignIn");
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> SignIn(InternalLoginViewModel model, string returnUrl)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var response = await oauthClient().GetAccessTokenAsync(model.Email, model.Password);

            if (response.AccessToken != null)
            {
                var isInternalUser = await IsInternalUser(response.AccessToken);
                if (isInternalUser)
                {
                    authenticationManager.SignIn(new AuthenticationProperties { IsPersistent = model.RememberMe },
                        response.GenerateUserIdentity());
                    return RedirectToLocal(returnUrl);
                }
                ModelState.AddModelError(string.Empty, "Invalid login details");
                return View("SignIn", model);
            }

            ModelState.AddModelError(string.Empty, ParseLoginError(response.Error));

            return View("SignIn", model);
        }

        // POST: /Admin/Account/SignOut
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignOut()
        {
            authenticationManager.SignOut();

            return RedirectToAction("SignIn");
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            
            return RedirectToAction("ChooseActivity", "Home", new { area = "Admin" });
        }

        private async Task<bool> IsInternalUser(string accessToken)
        {
            var userInfo = await userInfoClient().GetUserInfoAsync(accessToken);

            return userInfo.Claims.Any(p => p.Item2 == Claims.CanAccessInternalArea);
        }

        private string ParseLoginError(string error)
        {
            switch (error)
            {
                case OAuth2Constants.Errors.AccessDenied:
                    return "Access denied";
                case OAuth2Constants.Errors.InvalidGrant:
                    return "Invalid credentials";
                case OAuth2Constants.Errors.Error:
                case OAuth2Constants.Errors.InvalidClient:
                case OAuth2Constants.Errors.InvalidRequest:
                case OAuth2Constants.Errors.InvalidScope:
                case OAuth2Constants.Errors.UnauthorizedClient:
                case OAuth2Constants.Errors.UnsupportedGrantType:
                case OAuth2Constants.Errors.UnsupportedResponseType:
                default:
                    return "Internal error";
            }
        }
    }
}