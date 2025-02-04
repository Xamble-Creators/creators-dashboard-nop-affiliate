using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class ProductListResponse : ApiResponse
    {
        #region Properties

        public IList<ProductData> Products { get; set; }

        #endregion

        #region Nested classes

        public class ProductData
        {
            public string Id { get; set; }

            [JsonProperty(PropertyName = "hqpid")]
            public string HqPId { get; set; }

            [JsonProperty(PropertyName = "agentpid")]
            public string AgentPId { get; set; }

            public string AgentGroup { get; set; }

            public string ProductSku { get; set; }

            public int StockBalance { get; set; }

            public string ProductName { get; set; }

            public string Attachment1 { get; set; }

            public string Attachment2 { get; set; }

            public string Attachment3 { get; set; }

            public string Attachment4 { get; set; }

            public decimal SellingPrice { get; set; }

            public decimal AgentPrice { get; set; }

            public int Weight { get; set; }
        }

        #endregion
    }
}