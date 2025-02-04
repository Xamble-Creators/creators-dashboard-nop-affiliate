using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Caching;

namespace Nop.Plugin.BizApp.SalesPage
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class SalesPageDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "BizApp.SalesPage";

        public static string PaymentTypeBizAppPayDescription => "Fpx / CC (BIZAPPAY)";
        public static string BizAppPay => "BIZAPPAY";
        public static string BizAppPaySystemName => "Payments.BizAppPay";
        public static string MalaysiaTwoLetterIsoCode => "MY";

        public static string PaymentTypeAttribute => "salespage.paymenttype";
        public static string EnableLinkAffiliateAttribute => "salespage.enablelinkaffiliate";
        public static string ActivateSplitPaymentToAffiliateAttribute => "salespage.activatesplitpaymenttoaffiliate";
        public static string NotificationToCustomerViaSmsAttribute => "salespage.notificationtocustomerviasms";
        public static string NotificationToCustomerViaWhatsAppAttribute => "salespage.notificationtocustomerviawhatsapp";
        public static string NotificationToCustomerViaEmailAttribute => "salespage.notificationtocustomerviaemail";
        public static string ProductSkusAttribute => "salespage.productskus";

        public static string BizAppTokenAttribute => "salespage.bizapp.token";
        public static string BizAppRefreshTokenAttribute => "salespage.bizapp.refreshtoken";
        public static string BizAppTokenExpiryAttribute => "salespage.bizapp.tokenexpiry";
        public static string BizAppSalesPageLimitAttribute => "salespage.bizapp.salespagelimit";

        public static string RouteAgentPId => "agentpid";
        public static string RouteId => "id";

        public static string BizAppUsersRoleName => "BizAppUsers";
        public static string BizAppAdminsRoleName => "BizAppAdmins";

        public static CacheKey ProductsKeyCache => new("PLUGINS_BIZAPP_SALESPAGE_PRODUCTS.{0}.{1}") { 
             CacheTime = 1
        };

        public static CacheKey SalesPageVisitKeyCache => new("PLUGINS_BIZAPP_SALESPAGE_VISIT.{0}.{1}");

        public static CacheKey SalesPageByImageKeyKeyCache => new("PLUGINS_BIZAPP_SALESPAGE_BY_IMAGE_KEY.{0}");
    }
}