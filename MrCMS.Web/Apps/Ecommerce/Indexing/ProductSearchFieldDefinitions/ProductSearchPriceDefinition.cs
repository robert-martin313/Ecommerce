using System;
using System.Collections.Generic;
using System.Linq;
using Lucene.Net.Documents;
using MrCMS.Entities;
using MrCMS.Helpers;
using MrCMS.Indexing;
using MrCMS.Indexing.Management;
using MrCMS.Tasks;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Web.Apps.Ecommerce.Entities.Tax;
using MrCMS.Web.Apps.Ecommerce.Helpers;
using MrCMS.Web.Apps.Ecommerce.Helpers.Pricing;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Settings;
using NHibernate;
using NHibernate.Transform;

namespace MrCMS.Web.Apps.Ecommerce.Indexing.ProductSearchFieldDefinitions
{
    public class ProductSearchPriceDefinition : DecimalFieldDefinition<ProductSearchIndex, Product>
    {
        private readonly TaxSettings _taxSettings;
        private readonly ISession _session;

        public ProductSearchPriceDefinition(ILuceneSettingsService luceneSettingsService, TaxSettings taxSettings, ISession session)
            : base(luceneSettingsService, "price", index: Field.Index.NOT_ANALYZED)
        {
            _taxSettings = taxSettings;
            _session = session;
        }

        protected override IEnumerable<decimal> GetValues(Product obj)
        {
            return GetPrices(obj);
        }
        public class PriceList
        {
            public decimal BasePrice { get; set; }
            public decimal Price { get { return BasePrice.ProductPriceIncludingTax(TaxRatePercentage); } }
            public int ProductId { get; set; }
            public int? TaxRateId { get; set; }

            public void SetTaxRate(IList<TaxRate> possibleTaxRates)
            {
                TaxRate = possibleTaxRates.FirstOrDefault(rate => rate.Id == TaxRateId);
            }

            public TaxRate TaxRate { get; set; }

            public decimal TaxRatePercentage
            {
                get { return TaxRate.GetTaxRatePercentage(); }
            }
        }
        protected override Dictionary<Product, IEnumerable<decimal>> GetValues(List<Product> objs)
        {
            PriceList list = null;
            var priceLists = _session.QueryOver<ProductVariant>()
                .SelectList(builder => builder
                    .Select(variant => variant.BasePrice).WithAlias(() => list.BasePrice)
                    .Select(variant => variant.TaxRate.Id).WithAlias(() => list.TaxRateId)
                    .Select(variant => variant.Product.Id).WithAlias(() => list.ProductId)
                )
                .TransformUsing(Transformers.AliasToBean<PriceList>())
                .List<PriceList>();

            var taxRates = _session.QueryOver<TaxRate>().Cacheable().List();

            foreach (var priceList in priceLists)
                priceList.SetTaxRate(taxRates);


            var groupedPrices = priceLists.GroupBy(skuList => skuList.ProductId)
                .ToDictionary(lists => lists.Key, lists => lists.Select(skuList => skuList.Price));

            return objs.ToDictionary(product => product,
                product => groupedPrices.ContainsKey(product.Id) ? groupedPrices[product.Id] : Enumerable.Empty<decimal>());
        }

        public IEnumerable<decimal> GetPrices(Product entity)
        {
            return entity.Variants.Select(pv => pv.Price);
        }

        public override Dictionary<Type, Func<SystemEntity, IEnumerable<LuceneAction>>> GetRelatedEntities()
        {
            return new Dictionary<Type, Func<SystemEntity, IEnumerable<LuceneAction>>>
                   {
                       {typeof(ProductVariant),GetVariantActions},
                       {typeof(TaxRate),GetTaxRateActions}
                   };
        }

        private IEnumerable<LuceneAction> GetTaxRateActions(SystemEntity entity)
        {
            if (!_taxSettings.TaxesEnabled || _taxSettings.LoadedPricesIncludeTax)
                yield break;

            var rate = entity as TaxRate;
            if (rate == null)
                yield break;

            var variants = _session.QueryOver<ProductVariant>().Where(variant => variant.TaxRate.Id == rate.Id && variant.Product != null).List().ToList();

            if (rate.IsDefault)
            {
                variants.AddRange(
                    _session.QueryOver<ProductVariant>()
                        .Where(variant => variant.TaxRate == null && variant.Product != null)
                        .List());
            }
            foreach (var variant in variants)
            {
                yield return GetAction(variant.Product);
            }
        }

        private IEnumerable<LuceneAction> GetVariantActions(SystemEntity entity)
        {
            var variant = entity as ProductVariant;
            if (variant != null && variant.Product != null)
                yield return GetAction(variant.Product);
        }

        private static LuceneAction GetAction(Product product)
        {
            return new LuceneAction
            {
                Entity = product.Unproxy(),
                Operation = LuceneOperation.Update,
                IndexDefinition = IndexingHelper.Get<ProductSearchIndex>()
            };
        }
    }
}