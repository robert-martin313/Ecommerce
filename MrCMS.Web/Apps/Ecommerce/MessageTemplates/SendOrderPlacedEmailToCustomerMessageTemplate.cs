﻿using System;
using System.Collections.Generic;
using MrCMS.Entities.Messaging;
using MrCMS.Entities.People;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Settings;
using MrCMS.Website;
using NHibernate;
using MrCMS.Web.Apps.Ecommerce.Entities.Orders;

namespace MrCMS.Web.Apps.Ecommerce.MessageTemplates
{
    [FriendlyClassName("Send Order Placed Email To Customer Message Template")]
    public class SendOrderPlacedEmailToCustomerMessageTemplate : MessageTemplate, IMessageTemplate<Order>
    {
        public override MessageTemplate GetInitialTemplate(ISession session)
        {
            var fromName = CurrentRequestData.CurrentSite.Name;

            return new SendOrderPlacedEmailToCustomerMessageTemplate
            {
                FromName = fromName,
                ToAddress = "{OrderEmail}",
                ToName = "{UserName}",
                Bcc = String.Empty,
                Cc = String.Empty,
                Subject = String.Format("{0} - Order Placed", fromName),
                Body = "Your order (ID:{Id}) has been placed.",
                IsHtml = false
            };
        }

        public override List<string> GetTokens(IMessageTemplateParser messageTemplateParser)
        {
            return messageTemplateParser.GetAllTokens<Order>();
        }
    }
}