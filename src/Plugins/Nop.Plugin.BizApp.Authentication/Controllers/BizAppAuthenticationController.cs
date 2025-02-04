using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Plugin.BizApp.Authentication.Helpers;
using Nop.Plugin.BizApp.Authentication.Models;
using Nop.Plugin.BizApp.Authentication.Services;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.BizApp.Authentication.Controllers
{
    [Area(AreaNames.Admin)]
    [AuthorizeAdmin(true)]
    [AutoValidateAntiforgeryToken]
    public class BizAppAuthenticationController : BasePluginController
    {
        #region Fields

        private readonly ILocalizationService _localizationService;
        private readonly AuthenticationService _authenticationService;
        private readonly ICustomerRegistrationService _customerRegistrationService;
        private readonly ICustomerService _customerService;
        private readonly IWorkContext _workContext;
        private readonly IPermissionService _permissionService;

        #endregion

        #region Ctor

        public BizAppAuthenticationController(
            ILocalizationService localizationService,
            AuthenticationService authenticationService,
            ICustomerRegistrationService customerRegistrationService,
            ICustomerService customerService,
            IWorkContext workContext,
            IPermissionService permissionService)
        {
            _localizationService = localizationService;
            _authenticationService = authenticationService;
            _customerRegistrationService = customerRegistrationService;
            _customerService = customerService;
            _workContext = workContext;
            _permissionService = permissionService;
        }

        #endregion

        #region Methods

        #region Configuration

        public async Task<IActionResult> Login()
        {

            if (await _customerService.IsRegisteredAsync(await _workContext.GetCurrentCustomerAsync()))
            {
                if (!await _permissionService.AuthorizeAsync(StandardPermissionProvider.AccessAdminPanel))
                    return RedirectToAction("Logout", "Customer");
                else
                    return new RedirectResult("/Admin");
            }

            return View("~/Plugins/BizApp.Authentication/Views/Login.cshtml", new LoginModel());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (!ModelState.IsValid)
                return View("~/Plugins/BizApp.Authentication/Views/Login.cshtml", model);

            var loginResult = await _authenticationService.ValidateCustomerAsync(model.Username, model.Password, model.Domain);
            switch (loginResult)
            {
                case CustomerLoginResults.Successful:
                    {
                        var customer = await _customerService.GetCustomerByUsernameAsync(UsernameHelper.CombineUsernameAndDomain(model.Username, model.Domain));

                        return await _customerRegistrationService.SignInCustomerAsync(customer, "/Admin", true);
                    }
                case CustomerLoginResults.MultiFactorAuthenticationRequired:
                    ModelState.AddModelError("", "Not implemented");
                    break;
                case CustomerLoginResults.CustomerNotExist:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.CustomerNotExist"));
                    break;
                case CustomerLoginResults.Deleted:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.Deleted"));
                    break;
                case CustomerLoginResults.NotActive:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotActive"));
                    break;
                case CustomerLoginResults.NotRegistered:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.NotRegistered"));
                    break;
                case CustomerLoginResults.LockedOut:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials.LockedOut"));
                    break;
                case CustomerLoginResults.WrongPassword:
                default:
                    ModelState.AddModelError("", await _localizationService.GetResourceAsync("Account.Login.WrongCredentials"));
                    break;
            }

            //If we got this far, something failed, redisplay form
            return View("~/Plugins/BizApp.Authentication/Views/Login.cshtml", model);
        }

        #endregion

        #endregion
    }
}