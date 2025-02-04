using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using Nop.Core;
using Nop.Core.Domain.Seo;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Domain
{
    public class PlaceOrderRequest
    {
        public PlaceOrderRequest()
        {
            CartItems = new List<CartItem>();
        }

        public int SalesPageRecordId { get; set; }

        public int CustomerId { get; set; }

        public string AgentPId { get; set; }

        public string Postage { get; set; }

        public string FullName { get; set; }

        public string Email { get; set; }

        public int? CountryId { get; set; }

        public int? StateProvinceId { get; set; }

        public string Address1 { get; set; }

        public string City { get; set; }

        public string Address2 { get; set; }

        public string ZipPostalCode { get; set; }

        public string PhoneNumber { get; set; }

        public IList<CartItem> CartItems { get; set; }

        #region Nested Class

        public class CartItem
        {
            public string ProductSku { get; set; }

            public int Quantity { get; set; }
        }

        #endregion
    }
}