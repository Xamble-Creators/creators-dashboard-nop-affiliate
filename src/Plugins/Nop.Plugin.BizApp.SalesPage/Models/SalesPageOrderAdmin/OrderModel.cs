using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Nop.Core.Domain.Tax;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrderAdmin
{
    /// <summary>
    /// Represents an order model
    /// </summary>
    public partial record OrderModel : BaseNopEntityModel
    {
        #region Ctor

        public OrderModel()
        {
        }

        #endregion

        #region Properties

        public override int Id { get; set; }
        public Guid OrderGuid { get; set; }
        public string CustomOrderNumber { get; set; }
        
        public string CustomerEmail { get; set; }
        public string CustomerFullName { get; set; }
        public string CustomerPhone { get; set; }
        public string OrderTotal { get; set; }

        public string PaymentStatus { get; set; }
        public DateTime CreatedOn { get; set; }

        #endregion
    }
}