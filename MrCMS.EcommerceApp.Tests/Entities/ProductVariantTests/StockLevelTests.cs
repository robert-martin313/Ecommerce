﻿using FluentAssertions;
using MrCMS.Web.Apps.Ecommerce.Entities;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using Xunit;

namespace MrCMS.EcommerceApp.Tests.Entities.ProductVariantTests
{
    public class StockLevelTests
    {
        [Fact]
        public void ProductVariant_InStock_ShouldBeTrueIfStockRemainingIsNull()
        {
            var productVariant = new ProductVariant();

            var inStock = productVariant.InStock;

            inStock.Should().BeTrue();
        }

        [Fact]
        public void ProductVariant_InStock_ShouldBeFalseIfStockRemainingIs0()
        {
            var productVariant = new ProductVariant { StockRemaining = 0 };

            var inStock = productVariant.InStock;

            inStock.Should().BeFalse();
        }

        [Fact]
        public void ProductVariant_InStock_ShouldBeTrueIfStockRemainingIsGreaterThan0()
        {
            var productVariant = new ProductVariant { StockRemaining = 1 };

            var inStock = productVariant.InStock;

            inStock.Should().BeTrue();
        }

        [Fact]
        public void ProductVariant_CanBuy_ReturnFalseForZeroAmount()
        {
            var productVariant = new ProductVariant();

            var canBuy = productVariant.CanBuy(0);

            canBuy.Should().BeFalse();
        }

        [Fact]
        public void ProductVariant_CanBuy_ReturnTrueForPositiveAmountWhenStockRemainingIsNull()
        {
            var productVariant = new ProductVariant { StockRemaining = null };

            var canBuy = productVariant.CanBuy(1);

            canBuy.Should().BeTrue();
        }

        [Fact]
        public void ProductVariant_CanBuy_ReturnFalseForPositiveAmountWhenStockRemainingIsZero()
        {
            var productVariant = new ProductVariant { StockRemaining = 0 };

            var canBuy = productVariant.CanBuy(1);

            canBuy.Should().BeFalse();
        }

        [Fact]
        public void ProductVariant_CanBuy_ShouldReturnFalseWhenThereIsStockButLessThanRequested()
        {
            var productVariant = new ProductVariant { StockRemaining = 1 };

            var canBuy = productVariant.CanBuy(2);

            canBuy.Should().BeFalse();
        }

        [Fact]
        public void ProductVariant_CanBuy_ShouldReturnTrueWhenThereIsMoreStockThanRequested()
        {
            var productVariant = new ProductVariant { StockRemaining = 2 };

            var canBuy = productVariant.CanBuy(1);

            canBuy.Should().BeTrue();
        } 
    }
}