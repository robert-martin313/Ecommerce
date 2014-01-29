﻿using System;
using System.Collections.Generic;
using MrCMS.Entities.Messaging;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Web.Apps.Ecommerce.Entities.Products;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Web.Apps.Ecommerce.MessageTemplates
{
    public class ProductBackInStockMessageTemplate : MessageTemplate, IMessageTemplate<ProductVariant>
    {
        public override MessageTemplate GetInitialTemplate(ISession session)
        {
            return new ProductBackInStockMessageTemplate
                       {
                           FromName = CurrentRequestData.CurrentSite.Name,
                           ToAddress = "{Email}",
                           ToName = "Customer",
                           Bcc = String.Empty,
                           Cc = String.Empty,
                           Subject = String.Format("{0} - Product Back In Stock", CurrentRequestData.CurrentSite.Name),
                           Body = "<p>The product {Name} is back in stock.</p><p>{ProductUrl}</p>",
                           IsHtml = true
                       };
        }

        public override List<string> GetTokens(IMessageTemplateParser messageTemplateParser)
        {
            return messageTemplateParser.GetAllTokens<ProductVariant>();
        }
    }
}