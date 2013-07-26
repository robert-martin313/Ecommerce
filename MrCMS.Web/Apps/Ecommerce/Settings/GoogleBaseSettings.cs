﻿using System.ComponentModel;
using MrCMS.Settings;
using MrCMS.Web.Apps.Ecommerce.Models;

namespace MrCMS.Web.Apps.Ecommerce.Settings
{
    public class GoogleBaseSettings : SiteSettingsBase
    {
        [DisplayName("Default Category")]
        public string DefaultCategory{ get; set; }

        [DisplayName("Default Condition")]
        public ProductCondition DefaultCondition { get; set; }

        public override bool RenderInSettings
        {
            get { return false; }
        }
    }
}