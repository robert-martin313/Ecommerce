﻿using System.ComponentModel;
using MrCMS.Settings;

namespace MrCMS.Web.Apps.Ecommerce.Settings
{
    public class TaxSettings : SiteSettingsBase
    {
        [DisplayName("Loaded Prices Include Tax")]
        public bool LoadedPricesIncludeTax { get; set; }

        [DisplayName("Shipping Rates Include Tax")]
        public bool ShippingRateIncludesTax { get; set; }

        public override bool RenderInSettings
        {
            get { return false; }
        }
    }
}