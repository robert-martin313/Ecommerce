using System;
using System.IO;
using System.Net;
using System.Text;
using System.Web;
using MrCMS.Entities.Documents.Media;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Installation.Models;

namespace MrCMS.Web.Apps.Ecommerce.Installation.Services
{
    public class SetupEcommerceMedia : ISetupEcommerceMedia
    {
        private readonly IFileService _fileService;
        private readonly IDocumentService _documentService;

        public SetupEcommerceMedia(IFileService fileService, IDocumentService documentService)
        {
            _fileService = fileService;
            _documentService = documentService;
        }

        public MediaModel Setup()
        {
            var model = new MediaModel();

            var defaultMediaCategory = _documentService.GetDocumentByUrl<MediaCategory>("default");
            try
            {
                var memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(EcommerceInstalInfo.LogoImageUrl));
                model.Logo = _fileService.AddFile(memoryStream, "logo.png", "image/png", memoryStream.Length, defaultMediaCategory);

                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(EcommerceInstalInfo.Slide1ImageUrl));
                model.SliderImage1 = _fileService.AddFile(memoryStream, "slide1.jpg", "image/jpeg", memoryStream.Length, defaultMediaCategory);

                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(EcommerceInstalInfo.Slide2ImageUrl));
                model.SliderImage2 = _fileService.AddFile(memoryStream, "slide2.jpg", "image/jpeg", memoryStream.Length, defaultMediaCategory);

                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(EcommerceInstalInfo.DeliveryIcon));
                model.DeliveryIcon = _fileService.AddFile(memoryStream, "delivery.gif", "image/gif", memoryStream.Length, defaultMediaCategory);

                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(EcommerceInstalInfo.ReturnIcon));
                model.ReturnIcon = _fileService.AddFile(memoryStream, "return.gif", "image/gif", memoryStream.Length, defaultMediaCategory);

                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(EcommerceInstalInfo.LocationIcon));
                model.LocationIcon = _fileService.AddFile(memoryStream, "location.gif", "image/gif", memoryStream.Length, defaultMediaCategory);

                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(EcommerceInstalInfo.ContactIcon));
                model.ContactIcon = _fileService.AddFile(memoryStream, "contact.gif", "image/gif", memoryStream.Length, defaultMediaCategory);

                //featured category images
                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(FeaturedCategoriesInfo.Category1ImageUrl));
                model.FeaturedCategory1 = _fileService.AddFile(memoryStream, "cat1.jpg", "image/jpeg", memoryStream.Length, defaultMediaCategory);

                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(FeaturedCategoriesInfo.Category2ImageUrl));
                model.FeaturedCategory2 = _fileService.AddFile(memoryStream, "cat2.jpg", "image/jpeg", memoryStream.Length, defaultMediaCategory);
                
                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(FeaturedCategoriesInfo.Category3ImageUrl));
                model.FeaturedCategory3 = _fileService.AddFile(memoryStream, "cat3.jpg", "image/jpeg", memoryStream.Length, defaultMediaCategory);

                memoryStream = new MemoryStream(EcommerceInstallHelper.GetFileFromUrl(FeaturedCategoriesInfo.Category4ImageUrl));
                model.FeaturedCategory4 = _fileService.AddFile(memoryStream, "cat4.jpg", "image/jpeg", memoryStream.Length, defaultMediaCategory);

            }
            catch (Exception ex) { }

            var imgPath = HttpContext.Current.Server.MapPath(EcommerceInstalInfo.AwatingImageUrl);
            var fileStream = new FileStream(imgPath, FileMode.Open);
            model.AwatiginImage = _fileService.AddFile(fileStream, Path.GetFileName(imgPath), "image/jpeg", fileStream.Length, defaultMediaCategory);

            return model;
        }
    }

    public static class EcommerceInstallHelper
    {
        public static byte[] GetFileFromUrl(string url)
        {
            if (String.IsNullOrWhiteSpace(url))
                return null;
            return new WebClient().DownloadData(url);
        }
    }
}