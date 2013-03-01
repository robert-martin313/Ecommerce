﻿using System;
using System.Collections.Generic;
using MrCMS.Entities.Documents;
using MrCMS.Web.Apps.Ecommerce.Metadata;
using MrCMS.Web.Apps.Ecommerce.Pages;
using Xunit;
using FluentAssertions;

namespace MrCMS.EcommerceApp.Tests.Metadata
{
    public class ProductContainerMetadataTests
    {
        [Fact]
        public void ProductContainerMetaData_ChildrenListType_ShouldBeWhiteList()
        {
            var metadata = GetMetadata();

            metadata.ChildrenListType.Should().Be(ChildrenListType.WhiteList);
        }

        [Fact]
        public void ProductContainerMetaData_ChildrenList_ShouldBeJustProducts()
        {
            var metadata = GetMetadata();

            metadata.ChildrenList.Should().BeEquivalentTo(new List<Type> {typeof (Product)});
        }

        [Fact]
        public void ProductContainerMetaData_ShowChildrenInAdminNav_ShouldBeFalse()
        {
            var metadata = GetMetadata();

            metadata.ShowChildrenInAdminNav.Should().BeFalse();
        }

        private static ProductContainerMetadata GetMetadata()
        {
            return new ProductContainerMetadata();
        }
    }
}