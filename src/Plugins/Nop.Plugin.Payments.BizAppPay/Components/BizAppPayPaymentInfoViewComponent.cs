using Microsoft.AspNetCore.Mvc;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Payments.BizAppPay.Components
{
    public class BizAppPayPaymentComponent : NopViewComponent
    {
        public IViewComponentResult Invoke()
        {
            return View("~/Plugins/Payments.BizAppPay/Views/PaymentInfo.cshtml");
        }
    }
}
