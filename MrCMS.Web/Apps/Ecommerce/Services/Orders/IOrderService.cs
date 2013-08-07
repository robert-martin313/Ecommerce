﻿using System;
using MrCMS.Entities.People;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Paging;
using MrCMS.Web.Apps.Ecommerce.Entities.Orders;

namespace MrCMS.Web.Apps.Ecommerce.Services.Orders
{
    public interface IOrderService
    {
        Order PlaceOrder(CartModel cartModel, Action<Order> postCreationActions);
        IPagedList<Order> GetPaged(int pageNum, int pageSize = 10);
        void Save(Order item);
        Order Get(int id);
        IPagedList<Order> GetOrdersByUser(User user, int pageNum, int pageSize = 10);
        void Cancel(Order order);
        void MarkAsShipped(Order order);
        void MarkAsPaid(Order order);
        void MarkAsVoided(Order order);
    }
}