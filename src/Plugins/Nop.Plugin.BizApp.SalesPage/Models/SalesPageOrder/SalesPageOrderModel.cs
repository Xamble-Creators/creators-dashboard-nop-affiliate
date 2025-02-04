using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder
{
    public record SalesPageOrderModel : BaseNopModel
    {
        public SalesPageOrderModel()
        {
            AvailableCountries = new List<SelectListItem>();
            AvailableStates = new List<SelectListItem>();
            AvailablePostages = new List<PostageModel>();
            AvailableProducts = new List<ProductModel>();

        }

        #region Properties

        public string UrlSlug { get; set; }

        public string FailMessageKey { get; set; }

        public int SalesPageRecordId { get; set; }

        public string PageName { get; set; }

        public string PageHtmlContent { get; set; }

        public string AgentPId { get; set; }


        [NopResourceDisplayName("Plugins.BizApp.SalesPage.Postage")]
        public string SelectedPostage { get; set; }

        public IList<PostageModel> AvailablePostages { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.FullName")]
        public string FullName { get; set; }

        [DataType(DataType.EmailAddress)]
        [NopResourceDisplayName("Admin.Address.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.Country")]
        public int? CountryId { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.StateProvince")]
        public int? StateProvinceId { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.Address1")]
        public string Address1 { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.City")]
        public string City { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.Address2")]
        public string Address2 { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.ZipPostalCode")]
        public string ZipPostalCode { get; set; }

        [DataType(DataType.PhoneNumber)]
        [NopResourceDisplayName("Admin.Address.Fields.PhoneNumber")]
        public string PhoneNumber { get; set; }

        public IList<SelectListItem> AvailableCountries { get; set; }

        public IList<SelectListItem> AvailableStates { get; set; }

        public IList<ProductModel> AvailableProducts { get; set; }

        #endregion
    }
}