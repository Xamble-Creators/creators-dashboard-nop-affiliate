using System.Collections.Generic;
using Nop.Core;

namespace Nop.Plugin.BizApp.Authentication
{
    /// <summary>
    /// Represents plugin constants
    /// </summary>
    public class AuthenticationDefaults
    {
        /// <summary>
        /// Gets the plugin system name
        /// </summary>
        public static string SystemName => "BizApp.Authentication";

        /// <summary>
        /// Gets a default period (in seconds) before the request times out
        /// </summary>
        public static int RequestTimeout => 15;

    }
}