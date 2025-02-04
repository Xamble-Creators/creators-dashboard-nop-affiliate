using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageAdmin;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.BizApp.SalesPage.Validators
{
    public class SalesPageRecordValidator : BaseNopValidator<SalesPageRecordModel>
    {
        #region Ctor

        public SalesPageRecordValidator(ILocalizationService localizationService)
        {
            RuleFor(model => model.PageName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.PageName.Required"));

            RuleFor(model => model.UrlSlug)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.UrlSlug.Required"));

            RuleFor(model => model.PageHtmlContent)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.PageHtmlContent.Required"));

            RuleFor(model => model.SelectedProductSkus)
                .Must(products => products != null && products.Count > 0)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.ProductIds.Required"));
        }

        #endregion
    }
}