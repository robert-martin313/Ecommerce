﻿using System;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;
using MrCMS.ACL.Rules;
using MrCMS.Entities;
using MrCMS.Entities.Documents.Web;
using MrCMS.Helpers;
using MrCMS.Services;
using MrCMS.Website;
using MrCMS.Website.Controllers;
using NHibernate;
using System.Linq;

namespace MrCMS.Web.Areas.Admin.Controllers
{
    [MrCMSACLRule(typeof(AdminBarACL), AdminBarACL.Show, ReturnEmptyResult = true)]
    public class InPageAdminController : MrCMSAdminController
    {
        private readonly ISession _session;

        public InPageAdminController(ISession session)
        {
            _session = session;
        }

        public ActionResult InPageEditor(Webpage page)
        {
            return PartialView("InPageEditor", page);
        }

        [HttpPost]
        [ValidateInput(false)]
        public JsonResult SaveBodyContent(string content, int id, string type, string property)
        {
            var types = TypeHelper.GetAllConcreteTypesAssignableFrom<SystemEntity>();
            var entityType = types.FirstOrDefault(t => t.Name == type);
            if (entityType == null)
                return Json(new SaveResult(false, string.Format("Could not find entity type '{0}'", type)));
            var entity = _session.Get(entityType, id);
            if (entity == null)
                return
                    Json(new SaveResult(false,
                                        string.Format("Could not find entity of type '{0}' with id {1}", type, id)));
            var propertyInfo = entityType.GetProperties().FirstOrDefault(info => info.Name == property);
            if (propertyInfo == null)
                return
                    Json(new SaveResult(false,
                                        string.Format("Could not find entity of type '{0}' with id {1}", type, id)));
            if (string.IsNullOrWhiteSpace(content) &&
                propertyInfo.GetCustomAttributes(typeof(RequiredAttribute), false).Any())
            {
                return
                    Json(new SaveResult(false,
                                        string.Format("Could not edit '{0}' as it is required", property)));
            }

            propertyInfo.SetValue(entity, content, null);
            _session.Transact(session => session.SaveOrUpdate(entity));

            return Json(new SaveResult());
        }

        public class SaveResult
        {
            public SaveResult()
            {
                success = true;
                message = string.Empty;
            }

            public SaveResult(bool success, string message)
            {
                this.success = success;
                this.message = message;
            }

            public bool success { get; set; }
            public string message { get; set; }
        }


        [ValidateInput(false)]
        public string GetUnformattedBodyContent(int id, string type, string property)
        {
            var types = TypeHelper.GetAllConcreteTypesAssignableFrom<SystemEntity>();
            var entityType = types.FirstOrDefault(t => t.Name == type);
            if (entityType == null)
                return string.Empty;
            var entity = _session.Get(entityType, id);
            if (entity == null)
                return string.Empty;
            var propertyInfo = entityType.GetProperties().FirstOrDefault(info => info.Name == property);
            if (propertyInfo == null)
                return string.Empty;
            return Convert.ToString(propertyInfo.GetValue(entity, null));
        }

        [ValidateInput(false)]
        public string GetFormattedBodyContent(int id, string type, string property)
        {
            var types = TypeHelper.GetAllConcreteTypesAssignableFrom<SystemEntity>();
            var entityType = types.FirstOrDefault(t => t.Name == type);
            if (entityType == null)
                return string.Empty;
            var entity = _session.Get(entityType, id);
            if (entity == null)
                return string.Empty;
            var propertyInfo = entityType.GetProperties().FirstOrDefault(info => info.Name == property);
            if (propertyInfo == null)
                return string.Empty;
            var content = Convert.ToString(propertyInfo.GetValue(entity, null));

            if (entity is Webpage)
                CurrentRequestData.CurrentPage = entity as Webpage;
            var htmlHelper = MrCMSHtmlHelper.GetHtmlHelper(this);
            return htmlHelper.ParseShortcodes(content).ToHtmlString();
        }

    }
}