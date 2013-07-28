﻿using System;
using System.Collections.Generic;
using System.Reflection;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Linq;

namespace MrCMS.Website.Routing
{
    public class AppRouteConstraint : IRouteConstraint
    {
        private readonly string _appName;
        private readonly string _area;

        public AppRouteConstraint(string appName, string area)
        {
            _appName = appName;
            _area = area;
        }

        public bool Match(HttpContextBase httpContext, Route route, string parameterName, RouteValueDictionary values, RouteDirection routeDirection)
        {
            string controllerName = Convert.ToString(values["controller"]);
            string actionName = Convert.ToString(values["action"]);
            if (routeDirection == RouteDirection.IncomingRequest)
            {
                var mrCMSControllerFactory = ControllerBuilder.Current.GetControllerFactory() as MrCMSControllerFactory;
                if (mrCMSControllerFactory != null)
                {
                    bool isAdmin = !string.IsNullOrWhiteSpace(_area) && _area.Equals("Admin", StringComparison.OrdinalIgnoreCase);
                    bool isValidControllerType = mrCMSControllerFactory.IsValidControllerType(_appName, controllerName, isAdmin);
                    return isValidControllerType;
                }
                return false;
            }

            Type controllerType =
                MrCMSControllerFactory.AppAdminControllers[_appName].FirstOrDefault(
                    type => type.Name.Equals(controllerName + "Controller", StringComparison.OrdinalIgnoreCase));

            if (controllerType == null)
                return false;

            // get controller's methods
            return
                controllerType.GetMethods()
                              .Where(q => q.IsPublic && typeof (ActionResult).IsAssignableFrom(q.ReturnType))
                              .Any(info => info.Name.Equals(actionName, StringComparison.OrdinalIgnoreCase));

        }
    }
}