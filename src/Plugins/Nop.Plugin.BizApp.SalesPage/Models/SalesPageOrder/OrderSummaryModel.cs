using System;
using System.Collections.Generic;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Common;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder
{
    public record OrderSummaryModel : BaseNopModel
    {
        public OrderSummaryModel()
        {
            Products = new List<OrderItemModel>();
            ShippingAddress = new AddressModel();
        }

        #region Properties

        public int Id { get; set; }

        public DateTime CreatedOn { get; set; }

        public string SalesPageUrl { get; set; }

        public string CustomOrderNumber { get; set; }

        public string BizAppOrderId { get; set; }

        public AddressModel ShippingAddress { get; set; }

        public string ShippingRateComputationMethodSystemName { get; set; }

        public string OrderShipping { get; set; }

        public string SubtotalPrice { get; set; }

        public string Tax { get; set; }

        public string Shipping { get; set; }

        public string ShippingTax { get; set; }

        public string OrderTotal { get; set; }

        public IList<OrderItemModel> Products { get; set; }

        #endregion

        #region Nested Class

        public class OrderItemModel
        {
            public string ProductSku { get; set; }

            public string ProductName { get; set; }

            public int Quantity { get; set; }

            public string Price { get; set; }

            public string TotalPrice { get; set; }
        }

        #endregion
    }
}