﻿using System.ComponentModel;

namespace MrCMS.Settings
{
    public class ThirdPartyAuthSettings : SystemSettingsBase
    {
        [DisplayName("Is Google Login Enabled?")]
        public bool GoogleEnabled { get; set; }
        [DisplayName("Google Client Id")]
        public string GoogleClientId { get; set; }
        [DisplayName("Google Secret")]
        public string GoogleClientSecret { get; set; }


        [DisplayName("Is Facebook Login Enabled?")]
        public bool FacebookEnabled { get; set; }
        [DisplayName("Facebook App Id")]
        public string FacebookAppId { get; set; }
        [DisplayName("Facebook App Secret")]
        public string FacebookAppSecret { get; set; }

        [DisplayName("Is LinkedIn Login Enabled?")]
        public bool LinkedInEnabled { get; set; }
        [DisplayName("LinkedIn Client Id")]
        public string LinkedInClientId { get; set; }
        [DisplayName("LinkedIn Client Secret")]
        public string LinkedInClientSecret { get; set; }
    }
}