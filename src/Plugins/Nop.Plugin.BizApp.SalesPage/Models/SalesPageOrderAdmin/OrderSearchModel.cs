using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Areas.Admin.Models.Common;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrderAdmin
{
    /// <summary>
    /// Represents an order search model
    /// </summary>
    public partial record OrderSearchModel : BaseSearchModel
    {
        #region Ctor

        public OrderSearchModel()
        {
            PaymentStatusIds = new List<int>();
            AvailablePaymentStatuses = new List<SelectListItem>();
        }

        #endregion

        #region Properties

        [NopResourceDisplayName("Admin.Orders.List.StartDate")]
        [UIHint("DateNullable")]
        public DateTime? StartDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.EndDate")]
        [UIHint("DateNullable")]
        public DateTime? EndDate { get; set; }

        [NopResourceDisplayName("Admin.Orders.List.PaymentStatus")]
        public IList<int> PaymentStatusIds { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.Email")]
        public string Email { get; set; }

        [NopResourceDisplayName("Admin.Address.Fields.PhoneNumber")]
        public string Phone { get; set; }

        [NopResourceDisplayName("Plugins.BizApp.SalesPage.FullName")]
        public string CustomerName { get; set; }

        [NopResourceDisplayName("Account.CustomerOrders.OrderNumber")]
        public string CustomOrderNumber { get; set; }

        public IList<SelectListItem> AvailablePaymentStatuses { get; set; }

        #endregion
    }
}