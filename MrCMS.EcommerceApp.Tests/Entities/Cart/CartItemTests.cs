﻿using FakeItEasy;
using FluentAssertions;
using MrCMS.Web.Apps.Ecommerce.Entities.Cart;
using MrCMS.Web.Apps.Ecommerce.Entities.Discounts;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using Xunit;

namespace MrCMS.EcommerceApp.Tests.Entities.Cart
{
    public class CartItemTests
    {
        private ProductVariant _productVariant;

        public CartItemTests()
        {
            _productVariant = A.Fake<ProductVariant>();
        }

        [Fact]
        public void CartItem_PricePreTax_ShouldBeTheResultOfGetPricePreTax()
        {
            A.CallTo(() => _productVariant.GetPricePreTax(2)).Returns(20);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 2 };

            var pricePreTax = cartItem.PricePreTax;

            pricePreTax.Should().Be(20);
        }


        [Fact]
        public void CartItem_Price_ShouldBeTheResultOfGetPrice()
        {
            A.CallTo(() => _productVariant.GetPrice(2)).Returns(20);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 2 };

            var price = cartItem.Price;

            price.Should().Be(20);
        }

        [Fact]
        public void CartItem_Saving_ShouldBeTheResultOfGetSaving()
        {
            A.CallTo(() => _productVariant.GetSaving(2)).Returns(20);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 2 };

            var saving = cartItem.Saving;

            saving.Should().Be(20);
        }

        [Fact]
        public void CartItem_Tax_ShouldBeTheResultOfGetTax()
        {
            A.CallTo(() => _productVariant.GetTax(2)).Returns(20);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 2 };

            var tax = cartItem.Tax;

            tax.Should().Be(20);
        }

        [Fact]
        public void CartItem_CurrentlyAvailable_ShouldBeFalseIfStockLevelsAreTooLow()
        {
            A.CallTo(() => _productVariant.CanBuy(5)).Returns(false);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 2 };

            var currentlyAvailable = cartItem.CurrentlyAvailable;

            currentlyAvailable.Should().BeFalse();
        }

        [Fact]
        public void CartItem_CurrentlyAvailable_ShouldBeTrueIfProductIsAvailableForQuantity()
        {
            A.CallTo(() => _productVariant.CanBuy(2)).Returns(true);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 2 };

            var currentlyAvailable = cartItem.CurrentlyAvailable;

            currentlyAvailable.Should().BeTrue();
        }

        [Fact]
        public void CartItem_CurrentlyAvailable_ShouldBeTrueIfStockLevelsAreHighEnough()
        {
            A.CallTo(() => _productVariant.CanBuy(2)).Returns(true);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 2 };

            var currentlyAvailable = cartItem.CurrentlyAvailable;

            currentlyAvailable.Should().BeTrue();
        }

        [Fact]
        public void CartItem_TaxRatePercentage_ShouldReturnTheTaxRateInPercentage()
        {
            A.CallTo(() => _productVariant.TaxRatePercentage).Returns(20);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 2 };

            var taxRatePercentage = cartItem.TaxRatePercentage;

            taxRatePercentage.Should().Be(20);
        }

        [Fact]
        public void CartItem_Weight_ShouldBeWeightTimesQuantity()
        {
            A.CallTo(() => _productVariant.Weight).Returns(123);
            var cartItem = new CartItem { Item = _productVariant, Quantity = 3 };

            var weight = cartItem.Weight;

            weight.Should().Be(369);
        }

        [Fact]
        public void CartItem_GetDiscountAmount_IfNullDiscountIsPassedShouldBeZero()
        {
            var cartItem = new CartItem { Item = _productVariant, Quantity = 3 };

            var discountAmount = cartItem.GetDiscountAmount(null, null);

            discountAmount.Should().Be(0);
        }

        [Fact]
        public void CartItem_GetDiscountAmount_IfValidDiscountIsPassedShouldReturnResultOfDiscountGetAmount()
        {
            var discount = A.Fake<Discount>();
            var cartItem = new CartItem { Item = _productVariant, Quantity = 3 };
            A.CallTo(() => discount.GetDiscount(cartItem, "test")).Returns(10m);

            var discountAmount = cartItem.GetDiscountAmount(discount, "test");

            discountAmount.Should().Be(10);
        }
    }
}