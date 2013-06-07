﻿using System.Web.Mvc;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Services.Categories;
using MrCMS.Web.Apps.Ecommerce.Services.Products;
using MrCMS.Web.Apps.Ecommerce.Services.Tax;
using MrCMS.Website.Controllers;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using MrCMS.Models;
using MrCMS.Entities.Documents.Media;
using System.IO;
using System.Web;
using MrCMS.Web.Apps.Ecommerce.Services.Geographic;
using System;
using MrCMS.Website;
using MrCMS.Entities.Multisite;
namespace MrCMS.Web.Apps.Ecommerce.Areas.Admin.Controllers
{
    public class ProductController : MrCMSAppAdminController<EcommerceApp>
    {
        private readonly IProductService _productService;
        private readonly IDocumentService _documentService;
        private readonly ICategoryService _categoryService;
        private readonly ITaxRateManager _taxRateManager;
        private readonly IProductOptionManager _productOptionManager;
        private readonly IFileService _fileService;
        private readonly IImportExportManager _importExportManager;

        public ProductController(IProductService productService, IDocumentService documentService, ICategoryService categoryService, ITaxRateManager taxRateManager,
            IProductOptionManager productOptionManager, IFileService fileService, IImportExportManager importExportManager)
        {
            _productService = productService;
            _documentService = documentService;
            _categoryService = categoryService;
            _taxRateManager = taxRateManager;
            _productOptionManager = productOptionManager;
            _fileService = fileService;
            _importExportManager = importExportManager;
        }

        /// <summary>
        /// Lists products
        /// </summary>
        /// <param name="q">query string to filter by</param>
        /// <param name="p">page number</param>
        /// <returns></returns>
        public ViewResult Index(string q = null, int p = 1)
        {
            if (_documentService.GetUniquePage<ProductSearch>() == null)
                return View();
            var searchResult = _productService.Search(q, p);
            return View(searchResult);
        }

        [HttpGet]
        public PartialViewResult MakeMultiVariant(Product product)
        {
            return PartialView(new MakeMultivariantModel { ProductId = product.Id });
        }

        [HttpPost]
        public RedirectToRouteResult MakeMultiVariant(MakeMultivariantModel model)
        {
            _productService.MakeMultiVariant(model);

            return RedirectToAction("Edit", "Webpage", new { id = model.ProductId });
        }

        [HttpGet]
        public PartialViewResult Categories(Product product)
        {
            return PartialView(product);
        }

        [HttpGet]
        public PartialViewResult AddCategory(Product product, string query, int page = 1)
        {
            ViewData["product"] = product;
            var categories = _categoryService.GetCategories(product, query, page);
            return PartialView(categories);
        }

        [HttpPost]
        public JsonResult AddCategory(Product product, int categoryId)
        {
            try
            {
                _productService.AddCategory(product, categoryId);
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [AcceptVerbs(HttpVerbs.Get)]
        public JsonResult SearchCategories(Product product, string term)
        {
            if (string.IsNullOrWhiteSpace(term))
                return Json(_categoryService.GetCategories(product, String.Empty, 1).Select(x => new { Name = x.Name, CategoryID = x.Id }).Take(10).ToList());

            return Json(_categoryService.GetCategories(product, term, 1).Select(x=>new { Name=x.Name, CategoryID=x.Id }).Take(10).ToList(), JsonRequestBehavior.AllowGet);
        }


        [HttpGet]
        public PartialViewResult RemoveCategory(Product product, int categoryId)
        {
            ViewData["category"] = _documentService.GetDocument<Category>(categoryId);
            return PartialView(product);
        }

        [HttpPost]
        [ActionName("RemoveCategory")]
        public RedirectToRouteResult RemoveCategory_POST(Product product, int categoryId)
        {
            _productService.RemoveCategory(product, categoryId);

            return RedirectToAction("Edit", "Webpage", new { id = product.Id });
        }

        [HttpGet]
        public PartialViewResult AddSpecification(Product product)
        {
            ViewData["product"] = product;
            List<ProductSpecificationAttribute> attributes=_productOptionManager.ListSpecificationAttributes().ToList().Where(x=>x.Options.Count()>0 
                && product.SpecificationValues.Where(v=>v.Option.Id==x.Id).Count()==0).ToList();

            ViewData["specification-attributes"] = new SelectList(attributes, "Id", "Name");
            ViewData["specification-attributes-options"] = new SelectList(attributes.Count() > 0 ? attributes.First().Options : new List<ProductSpecificationAttributeOption>(), "Id", "Name");
            return PartialView(new ProductSpecificationValue() { Product=product });
        }

        [HttpGet]
        public JsonResult GetSpecificationAttributeOptions(int specificationAttributeId = 0)
        {
            try
            {
                List<SelectListItem> options = new List<SelectListItem>();
                foreach (var item in _productOptionManager.GetSpecificationAttribute(specificationAttributeId).Options.OrderBy(x => x.DisplayOrder).ToList())
                {
                    options.Add(new SelectListItem() { Selected=false, Text=item.Name, Value=item.Id.ToString() });
                }
                return Json(options);
            }
            catch
            {
                return Json(false);
            }
        }

        [HttpPost]
        public JsonResult AddSpecification(string Value, int Option=0,int ProductId=0)
        {
            try
            {
                _productOptionManager.SetSpecificationValue(_productService.Get(ProductId), _productOptionManager.GetSpecificationAttribute(Option), Value);
                return Json(true);
            }
            catch
            {
                return Json(false);
            }
        }

        [HttpGet]
        public PartialViewResult RemoveSpecification(Product product, int specificationValueId)
        {
            ViewData["specification-value"] = _productOptionManager.GetSpecificationValue(specificationValueId);
            return PartialView(product);
        }

        [HttpPost]
        [ActionName("RemoveSpecification")]
        public RedirectToRouteResult RemoveSpecification_POST(Product product, int specificationValueId)
        {
            _productOptionManager.DeleteSpecificationValue(_productOptionManager.GetSpecificationValue(specificationValueId));

            return RedirectToAction("Edit", "Webpage", new { id = product.Id });
        }

        [HttpGet]
        public ActionResult SortSpecifications(int productId = 0)
        {
            if (productId != 0)
            {
                Product product = _productService.Get(productId);
                if (product != null)
                {
                    var sortItems = product.SpecificationValues.OrderBy(x => x.DisplayOrder)
                                .Select(
                                    arg => new SortItem { Order = arg.DisplayOrder, Id = arg.Id, Name = arg.Option.Name })
                                .ToList();
                    ViewBag.Product = product;
                    return View(sortItems);
                }
            }
            return RedirectToAction("Edit", "Webpage", new { id = productId });
        }

        [HttpPost]
        public ActionResult SortSpecifications(List<SortItem> items, int productId = 0)
        {
            if (productId != 0)
            {
                if (items != null && items.Count > 0)
                {
                    _productOptionManager.UpdateSpecificationValueDisplayOrder(items);
                }
                return RedirectToAction("Edit", "Webpage", new { id = productId });
            }
            return RedirectToAction("Edit", "Webpage", new { id = productId });
        }

        public PartialViewResult PricingButtons(Product product)
        {
            return PartialView(product);
        }

        public PartialViewResult PricingInfo(Product product)
        {
            ViewData["tax-rate-options"] = _taxRateManager.GetOptions(product.TaxRate);
            return PartialView(product);
        }

        public PartialViewResult Specifications(Product product)
        {
            return PartialView(product);
        }

        public PartialViewResult Images(Product product)
        {
            return PartialView(product);
        }

        public ActionResult Thumbnails()
        {
            return PartialView();
        }

        [HttpGet]
        public ActionResult SortImages(Product product)
        {
            ViewBag.Product = product;
            var sortItems =
            _fileService.GetFiles(product.Gallery).OrderBy(arg => arg.display_order)
                                .Select(
                                    arg => new ImageSortItem { Order = arg.display_order, Id = arg.Id, Name = arg.name, ImageUrl = arg.url, IsImage = arg.is_image })
                                .ToList();

            return View(sortItems);
        }

        [HttpPost]
        public ActionResult SortImages(int productId, List<SortItem> items)
        {
            _fileService.SetOrders(items);
            return RedirectToAction("Edit", "Webpage", new { id = productId });
        }


        [HttpGet]
        public ActionResult SortOptions(Product product)
        {
            if (product != null)
            {
                var sortItems = product.AttributeOptions.OrderBy(x => x.DisplayOrder)
                            .Select(
                                arg => new SortItem { Order = arg.DisplayOrder, Id = arg.Id, Name = arg.Name })
                            .ToList();
                ViewBag.Product = product;
                return View(sortItems);
            }
            return RedirectToAction("Edit", "Webpage", new { id = product.Id });
        }

        [HttpPost]
        public ActionResult SortOptions(List<SortItem> items, Product product)
        {
            if (product != null)
            {
                if (items != null && items.Count > 0)
                {
                    _productOptionManager.UpdateAttributeOptionDisplayOrder(items);
                }
            }
            return RedirectToAction("Edit", "Webpage", new { id = product.Id });
        }

        [HttpGet]
        public PartialViewResult ManageOptions(Product product)
        {
            MakeMultivariantModel model = new MakeMultivariantModel();
            model.ProductId = product.Id;
            if (product.AttributeOptions.Count() > 0)
            {
                try
                {
                    model.Option1Id = product.AttributeOptions[0].Id;
                    model.Option1 = product.AttributeOptions[0].Name;
                    if (product.AttributeOptions[1] != null)
                    {
                        model.Option2Id = product.AttributeOptions[1].Id;
                        model.Option2 = product.AttributeOptions[1].Name;
                    }
                    if (product.AttributeOptions[2] != null)
                    {
                        model.Option3Id = product.AttributeOptions[2].Id;
                        model.Option3 = product.AttributeOptions[2].Name;
                    }
                }
                catch (Exception)
                {

                }
            }
            return PartialView(model);
        }

        [HttpPost]
        public RedirectToRouteResult ManageOptions(MakeMultivariantModel model)
        {
            var product = _documentService.GetDocument<Product>(model.ProductId);
            if (!string.IsNullOrWhiteSpace(model.Option1))
            {
                _productOptionManager.UpdateAttributeOption(model.Option1, model.Option1Id,product);
            }
            else
            {
                if (model.Option1Id != 0)
                {
                    ProductAttributeOption option = _productOptionManager.GetAttributeOption(model.Option1Id);
                    foreach (var item in option.Values.Where(x => x.ProductAttributeOption.Id == option.Id).ToList())
                    {
                        option.Values.Remove(item);
                    }
                    product.AttributeOptions.Remove(option);
                }
            }

            if (!string.IsNullOrWhiteSpace(model.Option2))
            {
                _productOptionManager.UpdateAttributeOption(model.Option2, model.Option2Id, product);
            }
            else
            {
                if (model.Option2Id != 0)
                {
                    ProductAttributeOption option = _productOptionManager.GetAttributeOption(model.Option2Id);
                    foreach (var item in option.Values.Where(x => x.ProductAttributeOption.Id == option.Id).ToList())
                    {
                        option.Values.Remove(item);
                    }
                    _productOptionManager.UpdateAttributeOption(option);
                    product.AttributeOptions.Remove(option);
                }
            }
            if (!string.IsNullOrWhiteSpace(model.Option3))
            {
                _productOptionManager.UpdateAttributeOption(model.Option3, model.Option3Id, product);
            }
            else
            {
                if (model.Option3Id != 0)
                {
                    ProductAttributeOption option = _productOptionManager.GetAttributeOption(model.Option3Id);
                    foreach (var item in option.Values.Where(x => x.ProductAttributeOption.Id == option.Id).ToList())
                    {
                        option.Values.Remove(item);
                    }
                    product.AttributeOptions.Remove(option);
                }
            }
            _documentService.SaveDocument(product);
   
            return RedirectToAction("Edit", "Webpage", new { id = model.ProductId });
        }

        public JsonResult IsUniqueSKU(string sku, int id)
        {
            if (id != 0)
            {
                if(_productService.AnyExistingProductWithSKU(sku,id))
                    return Json("There is already an SKU stored with that value.", JsonRequestBehavior.AllowGet);
                else
                    return Json(true, JsonRequestBehavior.AllowGet);
            }
            return Json(String.Empty);
        }

        public ViewResult ImportExport()
        {
            return View();
        }

        public FileResult ExportProducts()
        {
            try
            {
                byte[] file=_importExportManager.ExportProductsToExcel();
                ViewBag.ExportStatus = "Products successfully exported.";
                return File(file, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "MrCMS-ExportProducts-" + DateTime.UtcNow + ".xlsx");
            }
            catch (Exception)
            {
                ViewBag.ExportStatus = "Products exporting has failed. Please try again and contact system administration if error continues to appear.";
                return null;
            }
        }

        [HttpPost]
        public ViewResult ImportProducts(HttpPostedFileBase document)
        {
            if (document != null && document.ContentLength > 0 && document.ContentType == "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
            {
                ViewBag.Messages = _importExportManager.ImportProductsFromExcel(document);
            }
            else
            {
                ViewBag.ImportStatus = "Please choose non-empty Excel (.xslx) file before uploading.";
            }
            return View("ImportExport");
        }

        [HttpGet]
        public PartialViewResult MakeSingleVariant(Product product)
        {
            return PartialView(product);
        }

        [HttpPost]
        [ActionName("MakeSingleVariant")]
        public RedirectToRouteResult MakeSingleVariant_POST(Product product)
        {
            _productService.MakeSingleVariant(product);

            return RedirectToAction("Edit", "Webpage", new { id = product.Id });
        }

        [HttpGet]
        public JsonResult SearchProducts(string term)
        {
            if (!string.IsNullOrWhiteSpace(term))
                return Json(_productService.Search(term).Select(x => new { Name = x.Name, ProductID=x.Id }).Take(15).ToList());

            return Json(String.Empty, JsonRequestBehavior.AllowGet);
        }
    }
}