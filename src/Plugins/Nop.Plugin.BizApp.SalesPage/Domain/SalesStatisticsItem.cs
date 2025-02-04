using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Domain
{
    public class SalesStatisticsItem
    {
        public decimal OrderTotal { get; set; }

        public decimal Profit { get; set; }
    }
}