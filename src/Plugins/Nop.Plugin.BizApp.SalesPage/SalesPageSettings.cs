using System.Collections.Generic;
using Nop.Core.Configuration;

namespace Nop.Plugin.BizApp.SalesPage
{
    public class SalesPageSettings : ISettings
    {
        public int AllowedFileCountPerSalesPage { get; set; }

        public long FileSizeLimitInBytes { get; set; }
    }
}