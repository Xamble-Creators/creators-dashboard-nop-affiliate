using System.Collections.Generic;

namespace Nop.Plugin.Payments.BizAppPay.Domain.Api
{
    public class TokenResponse : ApiResponse
    {
        public string Token { get; set; }
    }
}