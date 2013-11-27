﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using MrCMS.Entities.Documents.Web;
using MrCMS.Entities.Widget;
using MrCMS.Web.Apps.Core.Models;
using MrCMS.Website;
using NHibernate;

namespace MrCMS.Web.Apps.Core.Widgets
{
    public class Navigation : Widget
    {
        public override bool HasProperties
        {
            get { return false; }
        }

        public override object GetModel(ISession session)
        {
            var navigationRecords =
                GetPages(session, null).Where(webpage => webpage.Published).OrderBy(webpage => webpage.DisplayOrder)
                       .Select(webpage => new NavigationRecord
                       {
                           Text = MvcHtmlString.Create(webpage.Name),
                           Url = MvcHtmlString.Create("/" + webpage.LiveUrlSegment),
                           Children = GetPages(session, webpage)
                                            .Select(webpage1 =>
                                                    new NavigationRecord
                                                    {
                                                        Text = MvcHtmlString.Create(webpage1.Name),
                                                        Url = MvcHtmlString.Create("/" + webpage1.LiveUrlSegment)
                                                    }).ToList()
                       }).ToList();

            return new NavigationList(navigationRecords.ToList());
        }

        private IList<Webpage> GetPages(ISession session, Webpage parent)
        {
            var queryOver = session.QueryOver<Webpage>();
            queryOver = parent == null
                            ? queryOver.Where(webpage => webpage.Parent == null)
                            : queryOver.Where(webpage => webpage.Parent.Id == parent.Id);
            return queryOver.Where(
                webpage => webpage.RevealInNavigation && webpage.Site.Id == Site.Id).Cacheable()
                          .List();
        }
    }
}