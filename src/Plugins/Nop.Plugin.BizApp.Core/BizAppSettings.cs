using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Plugin.BizApp.Core
{
    public class BizAppSettings : ISettings
    {
        public string BizAppApiUrl { get; set; }
    }
}