﻿using MrCMS.Web.Apps.Ecommerce.Models;

namespace MrCMS.Web.Apps.Ecommerce.Services.SagePay
{
    public interface ITransactionRegistrationBuilder
    {
        TransactionRegistration Build(CartModel cartModel);
    }
}