﻿using System.Linq;
using System.Text;
using MrCMS.Services;
using MrCMS.Web.Apps.Core.Widgets;
using MrCMS.Web.Apps.Ecommerce.Installation.Models;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services;
using MrCMS.Web.Apps.Ecommerce.Services.Categories;
using MrCMS.Web.Apps.Ecommerce.Widgets;

namespace MrCMS.Web.Apps.Ecommerce.Installation.Services
{
    public class SetupEcommerceWidgets : ISetupEcommerceWidgets
    {
        private readonly IWidgetService _widgetService;
        private readonly IDocumentService _documentService;

        public SetupEcommerceWidgets(IWidgetService widgetService, IDocumentService documentService)
        {
            _widgetService = widgetService;
            _documentService = documentService;
        }

        public void Setup(PageModel pageModel, MediaModel mediaModel, LayoutModel layoutModel)
        {
            var layout = layoutModel.HomeLayout;
            var beforeContent = layoutModel.EcommerceLayout.LayoutAreas.FirstOrDefault(x => x.AreaName == "Before Content");
            var teaser1Area = layout.LayoutAreas.FirstOrDefault(x => x.AreaName == "Teaser1");
            var teaser2Area = layout.LayoutAreas.FirstOrDefault(x => x.AreaName == "Teaser2");
            var teaser3Area = layout.LayoutAreas.FirstOrDefault(x => x.AreaName == "Teaser3");
            var teaser4Area = layout.LayoutAreas.FirstOrDefault(x => x.AreaName == "Teaser4");

            var slider = new Slider
            {
                Image = mediaModel.SliderImage1.FileUrl,
                Image1 = mediaModel.SliderImage2.FileUrl,
                LayoutArea = beforeContent,
                Webpage = pageModel.HomePage
            };
            _widgetService.AddWidget(slider);

            var featuredProducts = new FeaturedProducts
            {
                LayoutArea = beforeContent,
                Webpage = pageModel.HomePage,
                ListOfFeaturedProducts = GetFeaturedProducts(),
                Name = "Featured Products"
            };
            _widgetService.AddWidget(featuredProducts);

            var featuredCategories = new FeaturedCategories
            {
                LayoutArea = beforeContent,
                Webpage = pageModel.HomePage,
                ListOfFeaturedCategories = GetFeaturedCategories(),
                Name = "Featured Categories"
            };
            _widgetService.AddWidget(featuredCategories);

            var teaser1 = new TextWidget()
            {
                LayoutArea = teaser1Area,
                Webpage = pageModel.HomePage,
                Text = string.Format(@"<div class=""padding-bottom-10""><span><img src=""{0}"" /> </span></div><h3><a href=""#"">FREE delivery on orders over &pound;50. </a></h3><p>Orders placed Monday to Friday before 2pm will generally be picked and packed for immediate despatch. Please note that orders placed over the weekend or on public holidays will be processed on the next working day.</p>", mediaModel.DeliveryIcon.FileUrl)
            };
            _widgetService.AddWidget(teaser1);

            var teaser2 = new TextWidget()
            {
                LayoutArea = teaser2Area,
                Webpage = pageModel.HomePage,
                Text = string.Format(@"<div class=""padding-bottom-10""><span><img src=""{0}"" /> </span></div><h3><a href=""#"">7 day no question returns.</a></h3><p>We offer a 28 Day Money Back Guarantee. If for any reason you are not completely delighted with your purchase you may download a Returns Form and return it within 28 days of receipt for a full refund or exchange.</p>", mediaModel.ReturnIcon.FileUrl)
            };
            _widgetService.AddWidget(teaser2);

            var teaser3 = new TextWidget()
            {
                LayoutArea = teaser3Area,
                Webpage = pageModel.HomePage,
                Text = string.Format(@"<div class=""padding-bottom-10""><span><img src=""{0}"" /> </span></div><h3><a href=""#"">Store locations.</a></h3><p>Use our store locator to find a store near you as well as information like opening times, addresses, maps and a list of facilities available at every store.</p>", mediaModel.LocationIcon.FileUrl)
            };
            _widgetService.AddWidget(teaser3);

            var teaser4 = new TextWidget()
            {
                LayoutArea = teaser4Area,
                Webpage = pageModel.HomePage,
                Text = string.Format(@"<div class=""padding-bottom-10""><span><img src=""{0}"" /> </span></div><h3><a href=""#"">Contact us.</a></h3><p>Our customer service team is always willing to answer your proposal concerning Samsung Service. Your message will be promptly handled under the direct supervision of our executive management.</p>", mediaModel.ContactIcon.FileUrl)
            };
            _widgetService.AddWidget(teaser4);

            var recentlyViewed = new RecentlyViewedItems
            {
                Webpage = pageModel.ProductSearch,
                LayoutArea = layoutModel.SearchLayout.LayoutAreas.FirstOrDefault(x => x.AreaName == "After Filters"),
                Name = "Recently Viewed"
            };
            _widgetService.AddWidget(recentlyViewed);
        }

        private string GetFeaturedProducts()
        {
            var product1 = _documentService.GetDocumentByUrl<Product>(FeaturedProductsInfo.Product1Url);
            var product2 = _documentService.GetDocumentByUrl<Product>(FeaturedProductsInfo.Product2Url);
            var product3 = _documentService.GetDocumentByUrl<Product>(FeaturedProductsInfo.Product3Url);
            var product4 = _documentService.GetDocumentByUrl<Product>(FeaturedProductsInfo.Product4Url);
            var ids = new StringBuilder();
            if (product1 != null)
                ids.Append(product1.Id + ",");
            if (product2 != null)
                ids.Append(product2.Id + ",");
            if (product3 != null)
                ids.Append(product3.Id + ",");
            if (product4 != null)
                ids.Append(product4.Id + ",");

            return ids.ToString();
        }

        private string GetFeaturedCategories()
        {
            var cat1 = _documentService.GetDocumentByUrl<Category>(FeaturedCategoriesInfo.Category1Url);
            var cat2 = _documentService.GetDocumentByUrl<Category>(FeaturedCategoriesInfo.Category2Url);
            var cat3 = _documentService.GetDocumentByUrl<Category>(FeaturedCategoriesInfo.Category3Url);
            var cat4 = _documentService.GetDocumentByUrl<Category>(FeaturedCategoriesInfo.Category4Url);
            var ids = new StringBuilder();
            if (cat1 != null)
                ids.Append(cat1.Id + ",");
            if (cat2 != null)
                ids.Append(cat2.Id + ",");
            if (cat3 != null)
                ids.Append(cat3.Id + ",");
            if (cat4 != null)
                ids.Append(cat4.Id + ",");

            return ids.ToString();
        }
    }
}