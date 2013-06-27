﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Web.Mvc;
using MrCMS.Entities;
using MrCMS.Helpers;
using MrCMS.Web.Apps.Ecommerce.Entities.Cart;
using MrCMS.Web.Apps.Ecommerce.Models;
using MrCMS.Web.Apps.Ecommerce.Entities.Orders;
using System.ComponentModel.DataAnnotations;

namespace MrCMS.Web.Apps.Ecommerce.Entities.Discounts
{
    public class Discount : SiteEntity
    {
        public Discount()
        {
            Orders = new List<Order>();
        }

        [Required]
        public virtual string Name { get; set; }
        [Required]
        [StringLength(30, ErrorMessage = "Minimum length for code is {2} characters.", MinimumLength = 3)]
        public virtual string Code { get; set; }
        public virtual IList<Order> Orders { get; set; }

        public virtual string ValidTimePeriod
        {
            get
            {
                if (ValidFrom == null && ValidUntil == null)
                    return "Forever";
                else if (ValidFrom != null && ValidUntil == null)
                    return "From " + ValidFrom;
                else if (ValidFrom == null && ValidUntil != null)
                    return "Until " + ValidUntil;
                else
                    return ValidFrom + " - " + ValidUntil;
            }
        }

        [DisplayName("Valid From")]
        public virtual DateTime? ValidFrom { get; set; }
        [DisplayName("Valid Until")]
        public virtual DateTime? ValidUntil { get; set; }

        public virtual DiscountLimitation Limitation { get; set; }
        public virtual DiscountApplication Application { get; set; }

        public virtual List<SelectListItem> LimitationOptions
        {
            get
            {
                return TypeHelper.GetAllConcreteMappedClassesAssignableFrom<DiscountLimitation>()
                                 .BuildSelectItemList(type => type.Name.BreakUpString(), type => type.FullName,
                                                      type =>
                                                      Limitation != null && Limitation.Unproxy().GetType() == type,
                                                      "No limitations");
            }
        }
        public virtual List<SelectListItem> ApplicationOptions
        {
            get
            {
                return TypeHelper.GetAllConcreteMappedClassesAssignableFrom<DiscountApplication>()
                                 .BuildSelectItemList(type => type.Name.BreakUpString(), type => type.FullName,
                                                      type =>
                                                      Application != null && Application.Unproxy().GetType() == type,
                                                      emptyItemText: null);
            }
        }

        public virtual decimal GetDiscount(CartModel cartModel)
        {
            if (!IsCodeValid(cartModel.DiscountCode))
                return 0m;

            if (Limitation != null)
                if (!Limitation.IsCartValid(cartModel))
                    return 0m;

            return Application == null ? 0m : Application.GetDiscount(cartModel);
        }

        public virtual decimal GetDiscount(CartItem cartItem, string discountCode)
        {
            if (!IsCodeValid(discountCode))
                return 0m;

            if (Limitation != null)
                if (!Limitation.IsItemValid(cartItem))
                    return 0m;

            return Application == null ? 0m : Application.GetDiscount(cartItem);
        }

        public virtual bool IsCodeValid(string discountCode)
        {
            if (!string.Equals(Code, discountCode, StringComparison.OrdinalIgnoreCase))
                return false;

            if (ValidFrom.HasValue && ValidFrom > DateTime.UtcNow)
                return false;

            if (ValidUntil.HasValue && ValidUntil < DateTime.UtcNow)
                return false;

            return true;
        }
    }
}