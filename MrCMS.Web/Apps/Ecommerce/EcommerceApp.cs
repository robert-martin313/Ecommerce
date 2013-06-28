﻿using System.Collections.Generic;
using System.Web.Mvc;
using MrCMS.Apps;
using MrCMS.Entities.Documents.Layout;
using MrCMS.Entities.Multisite;
using MrCMS.Installation;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Web.Apps.Ecommerce.Areas.Admin.Controllers;
using MrCMS.Web.Apps.Ecommerce.Entities.Discounts;
using MrCMS.Web.Apps.Ecommerce.Pages;
using NHibernate;
using Ninject;
using MrCMS.Helpers;

namespace MrCMS.Web.Apps.Ecommerce
{
    public class EcommerceApp : MrCMSApp
    {
        public override string AppName
        {
            get { return "Ecommerce"; }
        }

        protected override void RegisterServices(IKernel kernel)
        {

        }

        public override System.Collections.Generic.IEnumerable<System.Type> BaseTypes
        {
            get
            {
                yield return typeof(DiscountLimitation);
                yield return typeof(DiscountApplication);
            }
        }
        protected override void RegisterApp(MrCMSAppRegistrationContext context)
        {
            context.MapAreaRoute("Admin controllers", "Admin", "Admin/Apps/Ecommerce/{controller}/{action}/{id}",
                                 new { controller = "Home", action = "Index", id = UrlParameter.Optional },
                                 new[] { typeof(ProductController).Namespace });
            context.MapRoute("Product Variant - GetPriceBreaksForProductVariant", "Apps/Ecommerce/ProductVariant/GetPriceBreaksForProductVariant", new { controller = "ProductVariant", action = "GetPriceBreaksForProductVariant" });
            context.MapRoute("Product Search - Results", "Apps/Ecommerce/ProductSearch/Results", new { controller = "ProductSearch", action = "Results" });
            context.MapRoute("Category - Results", "Apps/Ecommerce/Category/Results", new { controller = "Category", action = "Results" });
            context.MapRoute("Cart - Details", "Apps/Ecommerce/Cart/Details", new { controller = "Cart", action = "Details" });
            context.MapRoute("Cart - Update Quantity", "Apps/Ecommerce/Cart/UpdateQuantity", new { controller = "Cart", action = "UpdateQuantity" });
            context.MapRoute("Cart - Basic Details", "Apps/Ecommerce/Cart/BasicDetails", new { controller = "Cart", action = "BasicDetails" });
            context.MapRoute("Cart - Delivery Details", "Apps/Ecommerce/Cart/DeliveryDetails", new { controller = "Cart", action = "DeliveryDetails" });
            context.MapRoute("Cart - Order Email", "Apps/Ecommerce/Cart/OrderEmail", new { controller = "Cart", action = "OrderEmail" });
            context.MapRoute("Cart - Enter Order Email", "Apps/Ecommerce/Cart/EnterOrderEmail", new { controller = "Cart", action = "EnterOrderEmail" });
            context.MapRoute("Cart - Order Placed", "Apps/Ecommerce/Cart/OrderPlaced", new { controller = "Cart", action = "OrderPlaced" });
            context.MapRoute("Cart - Payment Details", "Apps/Ecommerce/Cart/PaymentDetails", new { controller = "Cart", action = "PaymentDetails" });
            context.MapRoute("Cart - Set Delivery Details", "Apps/Ecommerce/Cart/SetDeliveryDetails", new { controller = "Cart", action = "SetDeliveryDetails" });
            context.MapRoute("Cart - Shipping Methods", "Apps/Ecommerce/Cart/ShippingMethods", new { controller = "Cart", action = "ShippingMethods" });
            context.MapRoute("Cart - Add Shipping Method", "Apps/Ecommerce/Cart/AddShippingMethod", new { controller = "Cart", action = "AddShippingMethod" });
            context.MapRoute("Cart - Cart Panel", "Apps/Ecommerce/Cart/CartPanel", new { controller = "Cart", action = "CartPanel" });
            context.MapRoute("Cart - Add to Cart", "Apps/Ecommerce/Cart/AddToCart", new { controller = "Cart", action = "AddToCart" });
            context.MapRoute("Cart - Edit Cart Item", "Apps/Ecommerce/Cart/EditCartItem", new { controller = "Cart", action = "EditCartItem" });
            context.MapRoute("Cart - Delete Cart Item", "Apps/Ecommerce/Cart/DeleteCartItem", new { controller = "Cart", action = "DeleteCartItem" });
            context.MapRoute("Cart - Discount Code", "Apps/Ecommerce/Cart/DiscountCode", new { controller = "Cart", action = "DiscountCode" });
            context.MapRoute("Cart - Add Discount Code", "Apps/Ecommerce/Cart/AddDiscountCode", new { controller = "Cart", action = "AddDiscountCode" });
            context.MapRoute("Cart - Edit Discount Code", "Apps/Ecommerce/Cart/EditDiscountCode", new { controller = "Cart", action = "EditDiscountCode" });
            context.MapRoute("Cart - Is Discount Code Valid", "Apps/Ecommerce/Cart/IsDiscountCodeValid", new { controller = "Cart", action = "IsDiscountCodeValid" });
            context.MapRoute("User Login", "Apps/Ecommerce/UserLogin/UserLogin", new { controller = "UserLogin", action = "UserLogin" });
            context.MapRoute("User Login Details", "Apps/Ecommerce/UserLogin/UserLoginDetails", new { controller = "UserLogin", action = "UserLoginDetails" });
            context.MapRoute("User Login POST", "Apps/Ecommerce/UserLogin/Login", new { controller = "UserLogin", action = "Login" });
            context.MapRoute("User Registration", "Apps/Ecommerce/UserRegistration/UserRegistration", new { controller = "UserRegistration", action = "UserRegistration" });
            context.MapRoute("User Registration Details", "Apps/Ecommerce/UserRegistration/UserRegistrationDetails", new { controller = "UserRegistration", action = "UserRegistrationDetails" });
            context.MapRoute("User Register", "Apps/Ecommerce/UserRegistration/Register", new { controller = "UserRegistration", action = "Register" });
            context.MapRoute("User Account", "Apps/Ecommerce/UserAccount/UserAccount", new { controller = "UserAccount", action = "UserAccount" });
            context.MapRoute("User Account Details", "Apps/Ecommerce/UserAccount/UserAccountDetails", new { controller = "UserAccount", action = "UserAccountDetails" });
            context.MapRoute("User Account Orders", "Apps/Ecommerce/UserAccount/UserAccountOrders", new { controller = "UserAccount", action = "UserAccountOrders" });
            context.MapRoute("User Update Account", "Apps/Ecommerce/UserAccount/UpdateAccount", new { controller = "UserAccount", action = "UpdateAccount" });
        }

        protected override void OnInstallation(ISession session, InstallModel model, Site site)
        {
            var currentSite = new CurrentSite(site);
            var configurationProvider = new ConfigurationProvider(new SettingService(session), currentSite);
            var siteSettings = configurationProvider.GetSiteSettings<SiteSettings>();
            var documentService = new DocumentService(session, siteSettings, currentSite);

            var productSearch = new ProductSearch
                                    {
                                        Name = "Product Search",
                                        UrlSegment = "products",
                                        RevealInNavigation = true
                                    };
            var categoryContainer = new CategoryContainer
                                        {
                                            Name = "Categories",
                                            UrlSegment = "categories",
                                            RevealInNavigation = true
                                        };
            documentService.AddDocument(productSearch);
            documentService.PublishNow(productSearch);
            documentService.AddDocument(categoryContainer);
            documentService.PublishNow(categoryContainer);

            var layout = new Layout
                             {
                                 Name = "Ecommerce Layout",
                                 UrlSegment = "~/Apps/Ecommerce/Views/Shared/_EcommerceLayout.cshtml",
                                 LayoutAreas = new List<LayoutArea>()
                             };
            var areas = new List<LayoutArea>
                                     {
                                         new LayoutArea {AreaName = "Header", Layout = layout},
                                         new LayoutArea {AreaName = "After Content", Layout = layout},
                                         new LayoutArea {AreaName = "Footer", Layout = layout}
                                     };
            documentService.AddDocument(layout);
            var layoutAreaService = new LayoutAreaService(session);
            foreach (var area in areas)
                layoutAreaService.SaveArea(area);
            siteSettings.DefaultLayoutId = layout.Id;
            configurationProvider.SaveSettings(siteSettings);
        }
    }
}