using System.Collections.Generic;
using Nop.Web.Framework.Models;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrder
{
    public record ProductModel : BaseNopModel
    {
        public ProductModel()
        {
            Attachments = new List<string>();
        }

        #region Properties

        public int ProductId { get; set; }

        public string ProductSku { get; set; }

        public string ProductName { get; set; }

        public string PriceStr { get; set; }

        public decimal Price { get; set; }

        public int Weight { get; set; }

        public int StockQuantity { get; set; }

        public IList<string> Attachments { get; set; }

        #endregion
    }
}