﻿using MrCMS.Web.Apps.Ecommerce.Models;

namespace MrCMS.Web.Apps.Ecommerce.Services.Cart
{
    public interface IGetCart
    {
        CartModel GetCart();
        MrCMS.Web.Apps.Ecommerce.Pages.Cart GetSiteCart();
    }
}