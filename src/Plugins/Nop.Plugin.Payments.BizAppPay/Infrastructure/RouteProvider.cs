using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Payments.BizAppPay.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        /// <summary>
        /// Register routes
        /// </summary>
        /// <param name="routeBuilder">Route builder</param>
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            //IPN
            endpointRouteBuilder.MapControllerRoute(null,
                 "Plugins/BizAppPay/Webhook/{customOrderNumber}",
                 new { controller = "PaymentBizAppPay", action = "Webhook" });

            //After checkout, for redirection
            endpointRouteBuilder.MapControllerRoute(null,
                 "Plugins/BizAppPay/PaymentRequest/{customOrderNumber}",
                 new { controller = "PaymentBizAppPay", action = "PaymentRequest" });

            //For callback
            endpointRouteBuilder.MapControllerRoute(null,
                 "Plugins/BizAppPay/PaymentComplete/{customOrderNumber}",
                 new { controller = "PaymentBizAppPay", action = "PaymentComplete" });

            //Cancel
            endpointRouteBuilder.MapControllerRoute(null,
                "Plugins/BizAppPay/CancelOrder",
                 new { controller = "PaymentBizAppPay", action = "CancelOrder" });
        }

        /// <summary>
        /// Gets a priority of route provider
        /// </summary>
        public int Priority => -1;
    }
}