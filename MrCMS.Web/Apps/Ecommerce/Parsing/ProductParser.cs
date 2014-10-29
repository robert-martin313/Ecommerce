using System.Drawing;
using System.Text.RegularExpressions;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.NewsletterBuilder;
using MrCMS.Web.Apps.Ecommerce.Helpers;
using MrCMS.Web.Apps.Ecommerce.Pages;

namespace MrCMS.Web.Apps.Ecommerce.Parsing
{
    public class ProductParser : INewsletterItemParser<Product>
    {
        private static readonly Regex ImageRegex = new Regex(@"\[(?i)ProductImage\]");
        private static readonly Regex NameRegex = new Regex(@"\[(?i)ProductName\]");
        private static readonly Regex LinkRegex = new Regex(@"\[(?i)ProductUrl\]");
        private static readonly Regex PriceRegex = new Regex(@"\[(?i)ProductPrice\]");
        private static readonly Regex OldPriceRegex = new Regex(@"\[(?i)ProductOldPrice\]");
        private readonly INewsletterUrlHelper _newsletterUrlHelper;
        private readonly IFileService _fileService;
        private readonly IImageProcessor _imageProcessor;

        public ProductParser(INewsletterUrlHelper newsletterUrlHelper, IFileService fileService, IImageProcessor imageProcessor)
        {
            _newsletterUrlHelper = newsletterUrlHelper;
            _fileService = fileService;
            _imageProcessor = imageProcessor;
        }

        public string Parse(NewsletterTemplate template, Product item)
        {
            string output = template.ProductTemplate;
            var image = _imageProcessor.GetImage(item.DisplayImageUrl);
            output = ImageRegex.Replace(output,
                _newsletterUrlHelper.ToAbsolute(_fileService.GetFileLocation(image, new Size {Width = 150, Height = 150})));
            output = NameRegex.Replace(output, item.Name ?? string.Empty);
            output = LinkRegex.Replace(output, item.AbsoluteUrl);
            output = PriceRegex.Replace(output, GetPrice(item.DisplayPrice));
            output = OldPriceRegex.Replace(output, GetPrice(item.DisplayPreviousPrice));
            return output;
        }

        private static string GetPrice(decimal? price)
        {
            return price.HasValue
                ? string.Format("{0:�0.00}", price)
                : string.Empty;
        }
    }
}