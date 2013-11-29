﻿using System.Web.Mvc;
using MrCMS.Web.Apps.Ecommerce.Entities.BackInStockNotification;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Website.Controllers;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services.Products;
using MrCMS.Web.Apps.Ecommerce.Services.Analytics;

namespace MrCMS.Web.Apps.Ecommerce.Controllers
{
    public class ProductController : MrCMSAppUIController<EcommerceApp>
    {
        private readonly ITrackingService _trackingService;
        private readonly IProductUiService _productUiService;
        private readonly IBackInStockNotificationService _backInStockNotificationService;

        public ProductController(ITrackingService trackingService, IProductUiService productUiService, IBackInStockNotificationService backInStockNotificationService)
        {
            _trackingService = trackingService;
            _productUiService = productUiService;
            _backInStockNotificationService = backInStockNotificationService;
        }

        public ViewResult Show(Product page, int? variant)
        {
            _trackingService.AddItemToRecentlyViewedItemsCookie(page.Id);
            var variantToShow = _productUiService.GetVariantToShow(page, variant);
            ViewData["selected-variant"] = variantToShow;
            ViewData["back-in-stock"] = _productUiService.UserNotifiedOfBackInStock(variantToShow,
                                                                                    TempData["back-in-stock"] is bool &&
                                                                                    (bool) TempData["back-in-stock"]);
            return View(page);
        }
        public PartialViewResult VariantDetails(ProductVariant productVariant)
        {
            ViewData["back-in-stock"] = _productUiService.UserNotifiedOfBackInStock(productVariant,
                                                                                    TempData["back-in-stock"] is bool &&
                                                                                    (bool) TempData["back-in-stock"]);
            return PartialView(productVariant);
        }

        [HttpPost]
        public RedirectResult BackInStock(BackInStockNotificationRequest request)
        {
            _backInStockNotificationService.SaveRequest(request);
            TempData["back-in-stock"] = true;
            if (request.ProductVariant != null && request.ProductVariant.Product != null)
                return
                    Redirect(string.Format("~/{0}?variant={1}", request.ProductVariant.Product.LiveUrlSegment,
                                           request.ProductVariant.Id));
            return Redirect(Referrer.ToString());
        }
    }
}