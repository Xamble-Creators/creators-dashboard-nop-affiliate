using System.Collections;
using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class CreateOrderRequest : ApiRequest, IAuthorizedRequest
    {
        [JsonProperty(propertyName:"salespageid")]
        public int SalesPageId { get; set; }

        [JsonProperty(propertyName:"agentpid")]
        public string AgentPId { get; set; }

        [JsonProperty(propertyName:"url")]
        public string Url { get; set; }

        [JsonProperty(propertyName:"orderinfo")]
        public IList<OrderData> OrderInfo { get; set; }

        [JsonProperty(propertyName:"productinfo")]
        public IList<ProductData> ProductInfo { get; set; }

        [JsonProperty(propertyName:"paymentmethod")]
        public string PaymentMethod { get; set; }


        /// <summary>
        /// Gets the request path
        /// </summary>
        public override string Path => "createorder";

        /// <summary>
        /// Gets the request method
        /// </summary>
        public override string Method => HttpMethods.Post;

        #region Nested Class

        public class OrderData
        {
            [JsonProperty(propertyName: "sporderid")]
            public int OrderId { get; set; }

            [JsonProperty(propertyName: "customername")]
            public string CustomerName { get; set; }

            [JsonProperty(propertyName: "address")]
            public string Address { get; set; }

            [JsonProperty(propertyName: "hpno")]
            public string PhoneNumber { get; set; }

            [JsonProperty(propertyName: "email")]
            public string Email { get; set; }

            [JsonProperty(propertyName: "totalprice")]
            public decimal TotalPrice { get; set; }

            [JsonProperty(propertyName: "totalagentprice")]
            public decimal TotalAgentPrice { get; set; }

            [JsonProperty(propertyName: "totalweight")]
            public int TotalWeight { get; set; }

            [JsonProperty(propertyName: "discountcode")]
            public string DiscountCode { get; set; }

            [JsonProperty(propertyName: "postageid")]
            public string PostageId { get; set; }

            [JsonProperty(propertyName: "postageprice")]
            public decimal PostagePrice { get; set; }
        }

        public class ProductData
        {

            [JsonProperty(propertyName: "productsku")]
            public string ProductSku { get; set; }

            [JsonProperty(propertyName: "quantity")]
            public int Quantity { get; set; }
        }

        #endregion
    }
}