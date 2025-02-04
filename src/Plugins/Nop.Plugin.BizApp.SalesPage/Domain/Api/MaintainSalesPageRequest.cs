using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class MaintainSalesPageRequest : ApiRequest, IAuthorizedRequest
    {
        public static string ActionNew = "NEW";
        public static string ActionUpdate = "UPDATE";
        public static string ActionDelete = "DELETE";

        public MaintainSalesPageRequest()
        {
            Products = new List<ProductData>();
        }

        [JsonProperty(propertyName: "salespageid")]
        public int SalesPageId { get; set; }

        [JsonProperty(propertyName: "salespagename")]
        public string SalesPageName { get; set; }

        [JsonProperty(propertyName: "url")]
        public string Url { get; set; }

        [JsonProperty(propertyName: "action")]
        public string Action { get; set; }

        [JsonProperty(propertyName: "products")]
        public IList<ProductData> Products { get; set; }

        [JsonProperty(propertyName: "option_link_affiliate")]
        public string OptionLinkAffiliate { get; set; }

        [JsonProperty(propertyName: "option_splitpayment")]
        public string OptionSplitPayment { get; set; }

        [JsonProperty(propertyName: "option_notify_sms")]
        public string OptionNotifySms { get; set; }

        [JsonProperty(propertyName: "option_notify_whatsapp")]
        public string OptionNotifyWhatsApp { get; set; }

        [JsonProperty(propertyName: "option_notify_email")]
        public string OptionNotifyEmail { get; set; }

        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "salespage";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;

        #region Nested Class

        public class ProductData
        {
            [JsonProperty(propertyName: "productsku")]
            public string ProductSku { get; set; }
        }

        #endregion
    }
}