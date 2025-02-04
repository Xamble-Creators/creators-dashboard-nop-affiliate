using System.Collections.Generic;
using Nop.Core;

namespace Nop.Plugin.BizApp.Core
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class BizAppDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "BizApp.Core";

        /// <summary>
        /// Gets a default period (in seconds) before the request times out
        /// </summary>
        public static int RequestTimeout => 15;


        public static string ConfigurationRouteName => "Plugin.BizApp.Core.Configure";

        public static string BizAppTokenAttribute => "bizapp.token";
        public static string BizAppRefreshTokenAttribute => "bizapp.refreshtoken";
        public static string BizAppTokenExpiryAttribute => "bizapp.tokenexpiry";

    }
}