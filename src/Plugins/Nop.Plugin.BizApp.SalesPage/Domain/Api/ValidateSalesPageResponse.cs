using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using Nop.Plugin.BizApp.Core.Domain.Api;

namespace Nop.Plugin.BizApp.SalesPage.Domain.Api
{
    public class ValidateSalesPageResponse : ApiResponse
    {
        [JsonProperty(propertyName: "agentbankinfo")]
        public IList<AgentBankData> AgentBankInfo { get; set; }

        #region Nested Class

        public class AgentBankData
        {
            [JsonProperty(propertyName: "bizappay_secretkey")]
            public string BizAppPaySecretKey { get; set; }

            [JsonProperty(propertyName: "bizappay_categorycode")]
            public string BizAppPayCategoryCode { get; set; }

            [JsonProperty(propertyName: "bizappay_email")]
            public string BizAppPayEmail { get; set; }
        }

        #endregion
    }
}