﻿using FakeItEasy;
using MrCMS.Web.Apps.Ecommerce.Entities.Cart;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Web.Apps.Ecommerce.Services.Cart;
using Xunit;
using FluentAssertions;
using MrCMS.Helpers;

namespace MrCMS.EcommerceApp.Tests.Services
{
    public class CartManagerTests : InMemoryDatabaseTest
    {
        private CartModel _cartModel;
        private ProductVariant _productVariant = new ProductVariant();
        private CartManager _cartManager;
        private ICartSessionManager _cartSessionManager;

        public CartManagerTests()
        {
            _cartModel = new CartModel();
            Session.Transact(session => session.SaveOrUpdate(_productVariant));
            _cartSessionManager = A.Fake<ICartSessionManager>();
            _cartManager = new CartManager(_cartModel, Session, _cartSessionManager);
        }
        [Fact]
        public void CartManager_AddToCart_AddsAnItemToTheCart()
        {
            _cartManager.AddToCart(_productVariant, 1);

            _cartModel.Items.Should().HaveCount(1);
        }

        [Fact]
        public void CartManager_AddToCart_ShouldIncreaseAmountIfItAlreadyExists()
        {
            _cartModel.Items.Add(new CartItem { Item = _productVariant, Quantity = 1 });

            _cartManager.AddToCart(_productVariant, 1);

            _cartModel.Items.Should().HaveCount(1);
        }

        [Fact]
        public void CartManager_AddToCart_ShouldPersistToDb()
        {
            _cartManager.AddToCart(_productVariant, 1);

            Session.QueryOver<CartItem>().RowCount().Should().Be(1);
        }

        [Fact]
        public void CartManager_AddToCart_WithExistingItemShouldOnlyHave1DbRecord()
        {
            _cartModel.Items.Add(new CartItem { Item = _productVariant, Quantity = 1 });

            _cartManager.AddToCart(_productVariant, 1);

            Session.QueryOver<CartItem>().RowCount().Should().Be(1);
        }

        [Fact]
        public void CartManager_Delete_ShouldRemoveCartItemFromModel()
        {
            var cartItem = new CartItem { Item = _productVariant, Quantity = 1 };
            _cartModel.Items.Add(cartItem);

            _cartManager.Delete(cartItem);

            _cartModel.Items.Should().HaveCount(0);
        }

        [Fact]
        public void CartManager_Delete_ShouldRemoveCartItemFromDb()
        {
            var cartItem = new CartItem { Item = _productVariant, Quantity = 1 };
            Session.Transact(session => session.Save(cartItem));
            _cartModel.Items.Add(cartItem);

            _cartManager.Delete(cartItem);

            Session.QueryOver<CartItem>().RowCount().Should().Be(0);
        }

        [Fact]
        public void CartManager_UpdateQuantity_ShouldUpdateQuantityValueOfItem()
        {
            var cartItem = new CartItem { Item = _productVariant, Quantity = 1 };
            Session.Transact(session => session.Save(cartItem));
            _cartModel.Items.Add(cartItem);

            _cartManager.UpdateQuantity(cartItem, 2);

            cartItem.Quantity.Should().Be(2);
        }

        [Fact]
        public void CartManager_EmptyBasket_RemovesItemsFromModel()
        {
            var cartItem = new CartItem { Item = _productVariant, Quantity = 1 };
            _cartModel.Items.Add(cartItem);

            _cartManager.EmptyBasket();

            _cartModel.Items.Should().BeEmpty();
        }

        [Fact]
        public void CartManager_EmptyBasket_RemovesItemsFromDB()
        {
            var cartItem = new CartItem { Item = _productVariant, Quantity = 1 };
            _cartModel.Items.Add(cartItem);

            _cartManager.EmptyBasket();

            Session.QueryOver<CartItem>().RowCount().Should().Be(0);
        }
    }
}