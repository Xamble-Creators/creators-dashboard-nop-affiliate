using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Payments.BizAppPay.Models
{
    public partial record ConfigurationModel : BaseNopModel
    {
        public int ActiveStoreScopeConfiguration { get; set; }

        [NopResourceDisplayName("Plugins.Payments.BizAppPay.Fields.Endpoint")]
        public string Endpoint { get; set; }
        public bool Endpoint_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.BizAppPay.Fields.TenantId")]
        public string TenantId { get; set; }
        public bool TenantId_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.BizAppPay.Fields.Username")]
        public string Username { get; set; }
        public bool Username_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.BizAppPay.Fields.Password")]
        public string Password { get; set; }
        public bool Password_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.BizAppPay.Fields.SecretKey")]
        public string SecretKey { get; set; }
        public bool SecretKey_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.BizAppPay.Fields.AdditionalFee")]
        public decimal AdditionalFee { get; set; }
        public bool AdditionalFee_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.BizAppPay.Fields.AdditionalFeePercentage")]
        public bool AdditionalFeePercentage { get; set; }
        public bool AdditionalFeePercentage_OverrideForStore { get; set; }

        [NopResourceDisplayName("Plugins.Payments.BizAppPay.Fields.BypassChannelSelection")]
        public bool BypassChannelSelection { get; set; }
        public bool BypassChannelSelection_OverrideForStore { get; set; }
    }
}