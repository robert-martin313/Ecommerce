﻿using System;
using System.Web.Routing;
using MrCMS.Entities.Documents.Web;
using MrCMS.Services;
using MrCMS.Website;
using System.Linq;

namespace MrCMS.Helpers
{
    public class UniquePageHelper
    {
        public static string GetUrl<T>(object queryString = null) where T : Webpage, IUniquePage
        {
            return Get<T>(queryString, arg => "/" + arg.LiveUrlSegment);
        }
        public static string GetAbsoluteUrl<T>(object queryString = null) where T : Webpage, IUniquePage
        {
            return Get<T>(queryString, arg => arg.AbsoluteUrl);
        }

        private static string Get<T>(object queryString, Func<T, string> selector) where T : Webpage, IUniquePage
        {
            var documentService = MrCMSApplication.Get<IDocumentService>();

            var processPage = documentService.GetUniquePage<T>();
            var url = processPage != null ? selector(processPage) : "/";
            if (queryString != null && processPage != null)
            {
                var dictionary = new RouteValueDictionary(queryString);
                url += string.Format("?{0}",
                                     string.Join("&",
                                                 dictionary.Select(
                                                     pair => string.Format("{0}={1}", pair.Key, pair.Value))));
            }
            return url;
        }
    }
}