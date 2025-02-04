using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class OrderResponse : ApiResponse
    {
        #region Properties

        [JsonProperty(propertyName: "bizapporderid")]
        public string BizAppOrderId { get; set; }

        [JsonProperty(propertyName: "totalprice")]
        public decimal TotalPrice { get; set; }

        [JsonProperty(propertyName: "totalagentprice")]
        public decimal TotalAgentPrice { get; set; }

        [JsonProperty(propertyName: "agentcommission")]
        public decimal AgentCommission { get; set; }

        [JsonProperty(propertyName: "fpxkey")]
        public string FpxKey { get; set; }

        #endregion
    }
}