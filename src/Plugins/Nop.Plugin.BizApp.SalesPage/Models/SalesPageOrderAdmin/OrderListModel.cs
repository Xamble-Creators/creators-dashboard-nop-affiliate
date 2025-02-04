using Nop.Web.Framework.Models;

namespace Nop.Plugin.BizApp.SalesPage.Models.SalesPageOrderAdmin
{
    /// <summary>
    /// Represents an order list model
    /// </summary>
    public partial record OrderListModel : BasePagedListModel<OrderModel>
    {
    }
}