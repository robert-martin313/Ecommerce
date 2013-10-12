﻿using System.Collections.Generic;
using MarketplaceWebServiceOrders.Model;
using MrCMS.Web.Apps.Amazon.Entities.Orders;
using MrCMS.Web.Apps.Amazon.Helpers;
using MrCMS.Web.Apps.Amazon.Models;
using MrCMS.Web.Apps.Amazon.Services.Logs;
using System.Linq;
using MrCMS.Web.Apps.Amazon.Services.Orders.Events;

namespace MrCMS.Web.Apps.Amazon.Services.Orders.Sync
{
    public interface IUpdateAmazonOrder
    {
        AmazonOrder UpdateOrder(Order order);
        AmazonOrder UpdateOrder(AmazonOrderSyncData amazonOrderSyncData);
    }
    public class UpdateAmazonOrder : IUpdateAmazonOrder
    {
        private readonly IEnumerable<IPerformAmazonUpdates> _amazonUpdates;
        private readonly IEnumerable<IOnAmazonOrderPlaced> _onAmazonOrderPlaceds;
        private readonly IImportAmazonOrderService _importAmazonOrderService;
        private readonly IAmazonLogService _amazonLogService;
        private readonly IAmazonOrderService _amazonOrderService;

        public UpdateAmazonOrder(IEnumerable<IPerformAmazonUpdates> amazonUpdates,
                                 IEnumerable<IOnAmazonOrderPlaced> onAmazonOrderPlaceds,
                                 IImportAmazonOrderService importAmazonOrderService, IAmazonLogService amazonLogService,
                                 IAmazonOrderService amazonOrderService)
        {
            _amazonUpdates = amazonUpdates;
            _onAmazonOrderPlaceds = onAmazonOrderPlaceds;
            _importAmazonOrderService = importAmazonOrderService;
            _amazonLogService = amazonLogService;
            _amazonOrderService = amazonOrderService;
        }

        public AmazonOrder UpdateOrder(Order order)
        {
            switch (order.OrderStatus)
            {
                case OrderStatusEnum.Unshipped:
                case OrderStatusEnum.PartiallyShipped:
                case OrderStatusEnum.Shipped:
                case OrderStatusEnum.Canceled:
                    break;
                default:
                    return null;
            }
            if (!_importAmazonOrderService.IsCurrencyValid(order))
            {
                _amazonLogService.Add(AmazonLogType.Orders, AmazonLogStatus.Stage, null, null, AmazonApiSection.Orders,
                                      null, null, null, null,
                                      string.Format("Amazon Order #{0} uses different currency than current MrCMS Site.", order.AmazonOrderId));
                return null;
            }
            var amazonOrder = _amazonOrderService.GetByAmazonOrderId(order.AmazonOrderId);
            if (amazonOrder == null && order.OrderStatus == OrderStatusEnum.Canceled)
                return null;
            var newOrder = false;
            if (amazonOrder == null)
            {
                amazonOrder = new AmazonOrder();
                newOrder = true;
            }

            foreach (var update in _amazonUpdates.OrderBy(updates => updates.Order))
                update.Update(amazonOrder, order);

            _amazonOrderService.SaveOrUpdate(amazonOrder);

            if (newOrder)
                foreach (var orderPlaced in _onAmazonOrderPlaceds)
                    orderPlaced.OnAmazonOrderPlaced(amazonOrder);

            return amazonOrder;
        }

        public AmazonOrder UpdateOrder(AmazonOrderSyncData amazonOrderSyncData)
        {
            var amazonOrder = amazonOrderSyncData.AmazonOrder ?? new AmazonOrder();
            var order = AmazonAppHelper.DeserializeFromJson<Order>(amazonOrderSyncData.Data);

            foreach (var update in _amazonUpdates.OrderBy(updates => updates.Order))
                update.Update(amazonOrder, order);

            if (amazonOrder.Id > 0)
                amazonOrder = _amazonOrderService.Get(amazonOrder.Id);
            _amazonOrderService.SaveOrUpdate(amazonOrder);

            if (amazonOrderSyncData.Operation == SyncAmazonOrderOperation.Add)
            {
                foreach (var orderPlaced in _onAmazonOrderPlaceds)
                    orderPlaced.OnAmazonOrderPlaced(amazonOrder);
            }

            return amazonOrder;
        }
    }
}