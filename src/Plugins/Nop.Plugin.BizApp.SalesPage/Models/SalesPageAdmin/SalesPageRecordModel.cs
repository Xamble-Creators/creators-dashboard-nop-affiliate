using System;
using System.Collections;
using System.Collections.Generic;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageAdmin
{
    public record SalesPageRecordModel : BaseNopEntityModel
    {
        public SalesPageRecordModel()
        {
            SelectedProductSkus = new List<string>();
            AvailableProductSkus = new List<SelectListItem>();

        }

        #region Properties

        public string ImageKey { get; set; }

        public string CustomerName { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.PageName")]
        public string PageName { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.UrlSlug")]
        public string UrlSlug { get; set; }

        public string StoreUrl { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.PageHtmlContent")]
        public string PageHtmlContent { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.PaymentType")]
        public string PaymentType { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.OtherSettings")]
        public bool EnableLinkAffiliate { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.OtherSettings")]
        public bool ActivateSplitPaymentToAffiliate { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.NotificationToCustomer")]
        public bool NotificationToCustomerViaSms { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.NotificationToCustomer")]
        public bool NotificationToCustomerViaWhatsApp { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.NotificationToCustomer")]
        public bool NotificationToCustomerViaEmail { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.ProductIds")]
        public IList<string> SelectedProductSkus { get; set; }

        public IList<SelectListItem> AvailableProductSkus { get; set; }

        public DateTime CreatedOn { get; set; }

        #endregion
    }
}