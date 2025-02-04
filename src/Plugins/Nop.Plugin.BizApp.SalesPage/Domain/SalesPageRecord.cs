using System;
using Nop.Core;
using Nop.Core.Domain.Seo;

namespace Nop.Plugin.BizApp.SalesPage.Domain
{
    public class SalesPageRecord : BaseEntity, ISlugSupported
    {
        public int CustomerId { get; set; }

        public string PageName { get; set; }

        public string PageHtmlContent { get; set; }

        public string ImageKey { get; set; }

        public DateTime CreatedOnUtc { get; set; }
    }
}