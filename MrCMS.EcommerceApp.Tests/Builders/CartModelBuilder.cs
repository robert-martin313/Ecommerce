﻿using System.Collections.Generic;
using FakeItEasy;
using MrCMS.EcommerceApp.Tests.TestableModels;
using MrCMS.Web.Apps.Ecommerce.Entities.Cart;
using MrCMS.Web.Apps.Ecommerce.Entities.Geographic;
using MrCMS.Web.Apps.Ecommerce.Entities.Users;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Ecommerce.Services.Shipping;

namespace MrCMS.EcommerceApp.Tests.Builders
{
    public class CartModelBuilder
    {
        private decimal? _totalPreShipping;
        private decimal? _weight;
        private string _shippingAddressCountry;
        private readonly List<CartItem> _items = new List<CartItem>();
        private readonly HashSet<IShippingMethod> _availableShippingMethods = new HashSet<IShippingMethod>
                                                                            {
                                                                                A.Fake<IShippingMethod>()
                                                                            };

        private decimal? _shippableCalculationTotal;

        public CartModelBuilder WithWeight(decimal weight)
        {
            _weight = weight;
            return this;
        }

        public CartModelBuilder WithTotalPreShipping(decimal totalPreShipping)
        {
            _totalPreShipping = totalPreShipping;
            return this;
        }

        public CartModelBuilder WithShippableCalculationTotal(decimal shippableCalculationTotal)
        {
            _shippableCalculationTotal = shippableCalculationTotal;
            return this;
        }

        public CartModelBuilder WithShippingAddressCountry(string code)
        {
            _shippingAddressCountry = code;
            return this;
        }

        public CartModelBuilder WithItems(params CartItem[] items)
        {
            _items.Clear();
            items.ForEach(info => _items.Add(info));
            return this;
        }

        public CartModel Build()
        {
            return new TestableCartModel(_weight, _totalPreShipping)
                       {
                           ShippingAddress = new Address { CountryCode = _shippingAddressCountry },
                           Items = _items,
                           PotentiallyAvailableShippingMethods = _availableShippingMethods
                       };
        }
    }
}