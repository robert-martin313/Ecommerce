﻿using System.Collections.Generic;
using System.Web.Mvc;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.Shipping;
using MrCMS.Web.Apps.Ecommerce.Entities.Users;
using MrCMS.Website;

namespace MrCMS.Web.Apps.Ecommerce.Services.Shipping
{
    public interface IShippingMethodManager
    {
        IList<ShippingMethod> GetAll();
        ShippingMethod Get(int id);
        ShippingMethod GetByName(string name);
        List<SelectListItem> GetOptions();
        void Add(ShippingMethod ShippingMethod);
        void Update(ShippingMethod ShippingMethod);
        void Delete(ShippingMethod ShippingMethod);
        void UpdateDisplayOrder(IList<SortItem> options);
    }

      

}
