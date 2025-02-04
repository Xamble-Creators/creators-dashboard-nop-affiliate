using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Orders;
using Nop.Core.Http;
using Nop.Data;
using Nop.Plugin.BizApp.Core.Domain.Api.Auth;
using Nop.Plugin.BizApp.Core.Services;
using Nop.Plugin.BizApp.SalesPage.Domain;
using Nop.Plugin.BizApp.SalesPage.Domain.Api;
using Nop.Services.Catalog;
using Nop.Services.Customers;
using Nop.Services.Events;
using Nop.Services.Orders;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.BizApp.SalesPage.Services
{
    public partial class OrderPaidEventConsumer : IConsumer<OrderPaidEvent>
    {
        private readonly BizAppHttpClient _bizAppHttpClient;
        private readonly ICustomerService _customerService;
        private readonly ILogger<CustomerRegisteredEventConsumer> _logger;
        private readonly IOrderService _orderService;
        private readonly SalesPageOrderService _salesPageOrderService;
        private readonly SalesPageRecordService _salesPageRecordService;

        public OrderPaidEventConsumer(BizAppHttpClient bizAppHttpClient,
            ICustomerService customerService,
            ILogger<CustomerRegisteredEventConsumer> logger,
            IOrderService orderService,
            SalesPageOrderService salesPageOrderService,
            SalesPageRecordService salesPageRecordService)
        {
            _bizAppHttpClient = bizAppHttpClient;
            _customerService = customerService;
            _logger = logger;
            _orderService = orderService;
            _salesPageOrderService = salesPageOrderService;
            _salesPageRecordService = salesPageRecordService;
        }

        public virtual async Task HandleEventAsync(OrderPaidEvent eventMessage)
        {
            try
            {
                var salesPageOrder = await _salesPageOrderService.GetOrderByOrderIdAsync(eventMessage.Order.Id);
                var salesPageRecord = await _salesPageRecordService.GetRecordByIdAsync(salesPageOrder.SalesPageRecordId);
                var customer = await _customerService.GetCustomerByIdAsync(salesPageRecord.CustomerId);

                var updatePaymentStatusResponse = await _bizAppHttpClient.RequestAsync<UpdatePaymentStatusRequest, UpdatePaymentStatusResponse>(new UpdatePaymentStatusRequest()
                {
                    Customer = customer,
                    SalesPageId = salesPageRecord.Id,
                    CollectionId = salesPageOrder.CollectionId,
                    BillId = salesPageOrder.BillId,
                    BizAppOrderId = salesPageOrder.BizAppOrderId,
                    PaymentAmount = eventMessage.Order.OrderTotal.ToString("#.##"),
                    PaymentStatus = "paid",
                    PaymentUrl = salesPageOrder.PaymentUrl,
                    FpxKey = salesPageOrder.FpxKey
                });

                if (!updatePaymentStatusResponse.Success)
                {
                    await _orderService.InsertOrderNoteAsync(new OrderNote()
                    {
                        OrderId = eventMessage.Order.Id,
                        CreatedOnUtc = DateTime.UtcNow,
                        Note = $"Failed to update BizApp payment status with response {JsonConvert.SerializeObject(updatePaymentStatusResponse)}"
                    });
                    return;
                }

                await _orderService.InsertOrderNoteAsync(new OrderNote()
                {
                    OrderId = eventMessage.Order.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    Note = "Successfully update BizApp payment status"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to update payment status for order");

                await _orderService.InsertOrderNoteAsync(new OrderNote()
                {
                    OrderId = eventMessage.Order.Id,
                    CreatedOnUtc = DateTime.UtcNow,
                    Note = $"Failed to update BizApp payment status with error {JsonConvert.SerializeObject(ex.Message)}"
                });
            }
        }
    }
}