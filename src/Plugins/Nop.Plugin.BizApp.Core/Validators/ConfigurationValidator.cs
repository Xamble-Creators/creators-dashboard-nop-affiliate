using FluentValidation;
using Nop.Plugin.BizApp.Core.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.BizApp.Core.Validators
{
    /// <summary>
    /// Represents configuration model validator
    /// </summary>
    public class ConfigurationValidator : BaseNopValidator<ConfigurationModel>
    {
        #region Ctor

        public ConfigurationValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.BizAppApiUrl)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.BizApp.Core.BizAppApiUrl.Required"))
                .When(model => !string.IsNullOrEmpty(model.BizAppApiUrl));
        }

        #endregion
    }
}