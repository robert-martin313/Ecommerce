﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using MrCMS.Entities;
using MrCMS.Web.Apps.Ecommerce.Entities.Tax;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Web.Apps.Ecommerce.Pages;
using MrCMS.Web.Apps.Ecommerce.Settings;
using MrCMS.Website;
using System.Linq;
using NHibernate;
using MrCMS.Web.Apps.Ecommerce.Helpers;

namespace MrCMS.Web.Apps.Ecommerce.Entities.Products
{
    public class ProductVariant : SiteEntity, IBuyableItem
    {
        public ProductVariant()
        {
            AttributeValues = new List<ProductAttributeValue>();
        }
        [DisplayName("Price Pre Tax")]
        public virtual decimal PricePreTax
        {
            get
            {
                return Math.Round(MrCMSApplication.Get<TaxSettings>().LoadedPricesIncludeTax
                                      ? BasePrice / ((TaxRatePercentage + 100) / 100)
                                      : BasePrice, 2, MidpointRounding.AwayFromZero);
            }
        }

        public virtual decimal Weight { get; set; }
        public virtual string Name { get { return Product.Name; } }
        public virtual string EditUrl { get { return Product.EditUrl; } }

        [DisplayName("Price")]
        public virtual decimal BasePrice { get; set; }

        [DisplayName("Previous Price")]
        public virtual decimal? PreviousPrice { get; set; }

        public virtual decimal ReducedBy
        {
            get
            {
                return PreviousPrice != null
                           ? PreviousPrice.Value > PricePreTax
                                 ? PreviousPrice.Value - PricePreTax
                                 : 0
                           : 0;
            }
        }

        public virtual decimal ReducedByPercentage
        {
            get
            {
                return PreviousPrice != null
                           ? ReducedBy / PreviousPrice.Value
                           : 0;
            }
        }

        public virtual decimal Price
        {
            get
            {
                return Math.Round(TaxRate == null ? PricePreTax : PricePreTax * TaxRate.Multiplier, 2,
                                  MidpointRounding.AwayFromZero);
            }
        }

        public virtual decimal GetPrice(int quantity)
        {
            if (this.GetPriceBreaks().Any())
            {
                List<PriceBreak> priceBreaks = this.GetPriceBreaks().Where(x => quantity >= x.Quantity).OrderBy(x => x.Price).ToList();
                if (priceBreaks.Any())
                    return priceBreaks.First().GetPrice() * quantity;
            }

            return Price * quantity;
        }

        public virtual decimal GetSaving(int quantity)
        {
            if (PreviousPrice != 0)
                return ((PreviousPrice * quantity) - GetPrice(quantity)).Value;
            else
                return (Price * quantity) - GetPrice(quantity);
        }

        public virtual decimal GetTax(int quantity)
        {
            return Math.Round(GetPrice(quantity) - GetPricePreTax(quantity), 2, MidpointRounding.AwayFromZero);
        }

        public virtual decimal GetPricePreTax(int quantity)
        {
            return Math.Round(GetPrice(quantity) / ((TaxRatePercentage + 100) / 100), 2, MidpointRounding.AwayFromZero);
        }

        public virtual decimal GetUnitPrice()
        {
            return Price;
        }

        public virtual decimal GetPriceIncludingPriceBreaks(int quantity)
        {
            if (this.GetPriceBreaks().Any())
            {
                List<PriceBreak> priceBreaks = this.GetPriceBreaks().Where(x => quantity >= x.Quantity).OrderBy(x => x.Price).ToList();
                if (priceBreaks.Any())
                    return Math.Round(MrCMSApplication.Get<TaxSettings>().LoadedPricesIncludeTax
                                      ? priceBreaks.First().Price
                                      : TaxRate != null
                                            ? priceBreaks.First().Price * (TaxRate.Multiplier)
                                            : priceBreaks.First().Price, 2, MidpointRounding.AwayFromZero);
                else
                    return Price;
            }
            else
                return Price;
        }


        [DisplayName("Tax Rate")]
        public virtual TaxRate TaxRate { get; set; }

        public virtual decimal Tax { get { return Price - PricePreTax; } }
        public virtual string SKU { get; set; }
        public virtual decimal TaxRatePercentage
        {
            get
            {
                return TaxRate == null
                           ? 0
                           : TaxRate.Percentage;
            }
        }

        public virtual bool CanBuy(int quantity)
        {
            return quantity > 0 && (!StockRemaining.HasValue || StockRemaining >= quantity);
        }

        public virtual ProductAvailability Availability
        {
            get
            {
                if (AvailableOn.HasValue && AvailableOn <= DateTime.UtcNow)
                    return ProductAvailability.Available;
                return ProductAvailability.PreOrder;
            }
        }

        [DisplayName("Available On")]
        public virtual DateTime? AvailableOn { get; set; }

        public virtual bool InStock
        {
            get { return !StockRemaining.HasValue || StockRemaining > 0; }
        }
        [DisplayName("Stock Remaining")]
        public virtual int? StockRemaining { get; set; }

        public virtual Product Product { get; set; }

        public virtual IList<ProductAttributeValue> AttributeValues { get; set; }

    }
}