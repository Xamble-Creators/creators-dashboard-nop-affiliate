using System.Linq;
using FluentValidation;
using Microsoft.IdentityModel.Tokens;
using Nop.Core;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageAdmin;
using Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder;
using Nop.Services.Directory;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.BizApp.SalesPage.Validators
{
    public class SalesPageOrderValidator : BaseNopValidator<SalesPageOrderModel>
    {
        #region Ctor

        public SalesPageOrderValidator(ILocalizationService localizationService, ICountryService countryService,
            IStateProvinceService stateProvinceService)
        {
            RuleFor(model => model.FullName)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.FullName.Required"))
                .MinimumLength(5)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.FullName.MinLength"));

            RuleFor(model => model.Email)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Address.Fields.Email.Required"));

            RuleFor(model => model.PhoneNumber)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Address.Fields.PhoneNumber.Required"));

            RuleFor(model => model.Address1)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Address.Fields.Address1.Required"));

            RuleFor(model => model.CountryId)
                .GreaterThan(0)
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Address.Fields.Country.Required"));

            RuleFor(model => model.StateProvinceId)
                .GreaterThan(0)
                .WhenAwait(async x => (await stateProvinceService.GetStateProvincesByCountryIdAsync(x.CountryId ?? 0)).Any())
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Address.Fields.StateProvince.Required"));

            RuleFor(model => model.ZipPostalCode)
                .NotEmpty()
                .WithMessageAwait(localizationService.GetResourceAsync("Admin.Address.Fields.ZipPostalCode.Required"))
                .Length(5)
                .WhenAwait(async x => x.CountryId == (await countryService.GetCountryByTwoLetterIsoCodeAsync(SalesPageDefaults.MalaysiaTwoLetterIsoCode)).Id)
                .WithMessageAwait(localizationService.GetResourceAsync("Plugins.BizApp.SalesPage.ZipPostalCode.MinLength"));
        }

        #endregion
    }
}