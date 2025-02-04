using FluentValidation;
using Nop.Plugin.BizApp.Authentication.Models;
using Nop.Plugin.BizApp.Core.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.BizApp.Authentication.Validators
{
    public class LoginValidator : BaseNopValidator<LoginModel>
    {
        #region Ctor

        public LoginValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.Username)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Login.Fields.Username.Required"));

            RuleFor(model => model.Password)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Account.Login.Fields.Password.Required"));
        }

        #endregion
    }
}