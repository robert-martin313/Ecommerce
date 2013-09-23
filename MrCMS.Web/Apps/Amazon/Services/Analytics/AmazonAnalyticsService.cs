﻿using System;
using System.Collections.Generic;
using System.Linq;
using MrCMS.Web.Apps.Amazon.Entities.Analytics;
using MrCMS.Web.Apps.Amazon.Entities.Listings;
using MrCMS.Web.Apps.Amazon.Entities.Orders;
using MrCMS.Web.Apps.Amazon.Helpers;
using MrCMS.Web.Apps.Amazon.Models;
using MrCMS.Web.Apps.Amazon.Settings;
using MrCMS.Website;
using NHibernate;
using NHibernate.Criterion;

namespace MrCMS.Web.Apps.Amazon.Services.Analytics
{
    public class AmazonAnalyticsService : IAmazonAnalyticsService
    {
        private readonly ISession _session;
        private readonly IAmazonApiUsageService _amazonApiUsageService;
        private readonly AmazonAppSettings _amazonAppSettings;
        private readonly AmazonSellerSettings _amazonSellerSettings;

        public AmazonAnalyticsService(IAmazonApiUsageService amazonApiUsageService, 
            ISession session, 
            AmazonAppSettings amazonAppSettings,
            AmazonSellerSettings amazonSellerSettings)
        {
            _amazonApiUsageService = amazonApiUsageService;
            _session = session;
            _amazonAppSettings = amazonAppSettings;
            _amazonSellerSettings = amazonSellerSettings;
        }

        public void TrackNewApiCall(AmazonApiSection? apiSection, string apiOperation)
        {
            var amazonApiUsage = _amazonApiUsageService.GetForToday(apiSection, apiOperation) ?? new AmazonApiUsage()
                {
                    NoOfCalls = 0,
                    Day = CurrentRequestData.Now.Date,
                    ApiSection = apiSection,
                    ApiOperation = apiOperation,
                    Site = CurrentRequestData.CurrentSite
                };
               
            amazonApiUsage.NoOfCalls += 1;

            _amazonApiUsageService.Save(amazonApiUsage);
        }

        public object GetRevenue(DateTime from, DateTime to)
        {
            var orders = _session.QueryOver<AmazonOrder>()
                                 .Where(item => item.PurchaseDate >= from && item.PurchaseDate <= to).Cacheable().List()
                                 .GroupBy(c => c.PurchaseDate.Value).Select(k => new { Date = k.Key, Sum = k.Sum(t => t.OrderTotalAmount) })
                                 .ToDictionary(t => t.Date, t => t.Sum).ToList();

            return SetChartModel(from, to, orders);
        }

        public object GetProductsSold(DateTime from, DateTime to)
        {
            var orders = _session.QueryOver<AmazonOrder>()
                                 .Where(item => item.PurchaseDate >= from && item.PurchaseDate <= to).Cacheable().List()
                                 .GroupBy(c => c.PurchaseDate.Value).Select(k => new { Date = k.Key, Count = k.Sum(t => t.NumberOfItemsShipped) + k.Sum(t => t.NumberOfItemsUnshipped) })
                                 .ToDictionary(t => t.Date, t => Decimal.Parse(t.Count.ToString())).ToList();

            return SetChartModel(from, to, orders);
        }

        private static object SetChartModel(DateTime @from, DateTime to, List<KeyValuePair<DateTime, decimal>> items)
        {
            var data = new List<decimal>();
            var labels = new List<string>();
            var ts = to - @from;
            if (ts.Days <= 7)
            {
                foreach (var order in items)
                {
                    labels.Add(order.Key.ToString("dd/MM"));
                    data.Add(order.Value);
                }
            }
            else
            {
                var factor = (ts.Days/7);
                var current = 0;
                for (var i = 0; i < 7; i++)
                {
                    if (items.Count < i + 1) continue;

                    var oldDate = items[i].Key;
                    var currentDate = items[i].Key.AddDays(current);
                    labels.Add(currentDate.ToString("dd/MM"));
                    data.Add(i == 0
                                 ? items.Where(x => x.Key.Date == currentDate.Date).Sum(x => x.Value)
                                 : items.Where(x => oldDate.Date <= x.Key && x.Key <= currentDate.Date)
                                        .Sum(x => x.Value));
                    current += factor;
                }
            }

            return new
                {
                    Labels = labels,
                    Data = data
                };
        }

        public AmazonDashboardModel GetAmazonDashboardModel(DateTime? from, DateTime? to)
        {
            var model = new AmazonDashboardModel();
            if (from.HasValue)
                model.FilterFrom = @from.Value;
            if (to.HasValue)
                model.FilterUntil = to.Value;
            model.NoOfActiveListings = GetNumberOfActiveListings();
            model.NoOfApiCalls = GetNumberOfApiCalls(model.FilterFrom, model.FilterUntil);
            model.NoOfOrders =GetNumberOfOrders(model.FilterFrom, model.FilterUntil);
            model.NoOfUnshippedOrders =GetNumberUnshippedOrders(model.FilterFrom, model.FilterUntil);
            model.AverageOrderAmount = GetAverageOrderAmount(model.FilterFrom, model.FilterUntil);
            model.NoOfOrderedProducts = GetNumberOfOrderedProducts(model.FilterFrom, model.FilterUntil);
            model.NoOfShippedProducts = GetNumberOfShippedProducts(model.FilterFrom, model.FilterUntil);
            model.AppSettingsStatus = AmazonAppHelper.GetAmazonAppSettingsStatus(_amazonAppSettings);
            model.SellerSettingsStatus = AmazonAppHelper.GetAmazonSellerSettingsStatus(_amazonSellerSettings);
            return model;
        }

        private int GetNumberOfOrders(DateTime from, DateTime to)
        {
            return _session.QueryOver<AmazonOrder>()
                           .Where(item => item.PurchaseDate >= from && item.PurchaseDate <= to).Cacheable().RowCount();
        }

        private double GetAverageOrderAmount(DateTime from, DateTime to)
        {
            return _session.CreateCriteria(typeof(AmazonOrder))
                .Add(Restrictions.Between("PurchaseDate", from, to))
                .SetProjection(Projections.Avg("OrderTotalAmount")).SetCacheable(true).UniqueResult<double>();
        }

        private int GetNumberUnshippedOrders(DateTime from, DateTime to)
        {
            return _session.QueryOver<AmazonOrder>().Where(item => item.PurchaseDate >= from && item.PurchaseDate <= to &&
                               (item.Status==null || item.Status.Value==AmazonOrderStatus.Unshipped)).Cacheable().RowCount();
        }

        private decimal GetNumberOfOrderedProducts(DateTime from, DateTime to)
        {
            AmazonOrder amazonOrderAlias = null;
            AmazonOrderItem amazonOrderItemAlias = null;
            return _session.QueryOver(() => amazonOrderItemAlias)
                              .JoinAlias(() => amazonOrderItemAlias.AmazonOrder, () => amazonOrderAlias)
                              .Where(() => amazonOrderAlias.PurchaseDate >= from && amazonOrderAlias.PurchaseDate <= to)
                              .Cacheable().List().Sum(x => x.QuantityOrdered);
        }

        private decimal GetNumberOfShippedProducts(DateTime from, DateTime to)
        {
            AmazonOrder amazonOrderAlias = null;
            AmazonOrderItem amazonOrderItemAlias = null;
            return _session.QueryOver(() => amazonOrderItemAlias)
                              .JoinAlias(() => amazonOrderItemAlias.AmazonOrder, () => amazonOrderAlias)
                              .Where(() => amazonOrderAlias.PurchaseDate >= from && amazonOrderAlias.PurchaseDate <= to)
                              .Cacheable().List().Sum(x => x.QuantityShipped);
        }

        private int GetNumberOfActiveListings()
        {
            return _session.QueryOver<AmazonListing>()
                           .Where(item => item.Status==AmazonListingStatus.Active).Cacheable().RowCount();
        }

        private int GetNumberOfApiCalls(DateTime from, DateTime to)
        {
            return _session.CreateCriteria(typeof(AmazonApiUsage))
                .Add(Restrictions.Between("Day",from,to))
                .SetProjection(Projections.Sum("NoOfCalls")).SetCacheable(true).UniqueResult<int>();
        }
    }
}