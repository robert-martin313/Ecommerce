﻿using System.Web.Mvc;
using MrCMS.ACL.Rules;
using MrCMS.Entities.People;
using MrCMS.Models;
using MrCMS.Services;
using MrCMS.Web.Areas.Admin.Helpers;
using MrCMS.Web.Areas.Admin.ModelBinders;
using MrCMS.Web.Areas.Admin.Models;
using MrCMS.Web.Areas.Admin.Services;
using MrCMS.Website;
using MrCMS.Website.Binders;
using MrCMS.Website.Controllers;

namespace MrCMS.Web.Areas.Admin.Controllers
{
    public class UserController : MrCMSAdminController
    {
        private readonly IUserService _userService;
        private readonly IUserSearchService _userSearchService;
        private readonly IRoleService _roleService;
        private readonly IPasswordManagementService _passwordManagementService;
        private readonly IGetUserCultureOptions _getUserCultureOptions;
        private IGetUserEditTabsService _getUserEditTabsService;


        public UserController(IUserService userService, IUserSearchService userSearchService, IRoleService roleService, IPasswordManagementService passwordManagementService, IGetUserCultureOptions getUserCultureOptions, IGetUserEditTabsService getUserEditTabsService)
        {
            _userService = userService;
            _userSearchService = userSearchService;
            _roleService = roleService;
            _passwordManagementService = passwordManagementService;
            _getUserCultureOptions = getUserCultureOptions;
            _getUserEditTabsService = getUserEditTabsService;
        }

        [MrCMSACLRule(typeof(UserACL), UserACL.View)]
        public ActionResult Index(UserSearchQuery searchQuery)
        {
            ViewData["users"] = _userSearchService.GetUsersPaged(searchQuery);
            ViewData["roles"] = _userSearchService.GetAllRoleOptions();
            return View(searchQuery);
        }

        [HttpGet]
        [MrCMSACLRule(typeof(UserACL), UserACL.Add)]
        public PartialViewResult Add()
        {
            var model = new AddUserModel(); 
            ViewData["culture-options"] = _getUserCultureOptions.Get();
            return PartialView(model);
        }

        [HttpPost]
        [MrCMSACLRule(typeof(UserACL), UserACL.Add)]
        public ActionResult Add([IoCModelBinder(typeof(AddUserModelBinder))] User user)
        {
            _userService.AddUser(user);

            return RedirectToAction("Edit", new { id = user.Id });
        }

        [HttpGet]
        [ActionName("Edit")]
        [MrCMSACLRule(typeof(UserACL), UserACL.Edit)]
        public ActionResult Edit_Get(User user)
        {
            ViewData["AvailableRoles"] = _roleService.GetAllRoles();
            ViewData["OnlyAdmin"] = _roleService.IsOnlyAdmin(user);
            ViewData["culture-options"] = _getUserCultureOptions.Get();
            ViewData["edit-tabs"] = _getUserEditTabsService.GetEditTabs(user);
            return user == null
                       ? (ActionResult)RedirectToAction("Index")
                       : View(user);
        }

        [HttpPost]
        [MrCMSACLRule(typeof(UserACL), UserACL.Edit)]
        public ActionResult Edit([IoCModelBinder(typeof(EditUserModelBinder))] User user)
        {
            _userService.SaveUser(user);
            TempData.SuccessMessages().Add(string.Format("{0} successfully saved", user.Name));
            return RedirectToAction("Edit", "User", new { Id = user.Id });
        }

        [HttpGet]
        [ActionName("Delete")]
        [MrCMSACLRule(typeof(UserACL), UserACL.Delete)]
        public PartialViewResult Delete_Get(User user)
        {
            return PartialView(user);
        }

        [HttpPost]
        [MrCMSACLRule(typeof(UserACL), UserACL.Delete)]
        public RedirectToRouteResult Delete(User user)
        {
            _userService.DeleteUser(user);

            return RedirectToAction("Index");
        }

        [HttpGet]
        [MrCMSACLRule(typeof(UserACL), UserACL.SetPassword)]
        public ActionResult SetPassword(User user)
        {
            return PartialView(user);
        }

        [HttpPost]
        [MrCMSACLRule(typeof(UserACL), UserACL.SetPassword)]
        public ActionResult SetPassword(User user, string password)
        {
            _passwordManagementService.SetPassword(user, password, password);
            _userService.SaveUser(user);
            return RedirectToAction("Edit", new { user.Id });
        }

        public JsonResult IsUniqueEmail(string email, int? id)
        {
            if (_userService.IsUniqueEmail(email, id))
                return Json(true, JsonRequestBehavior.AllowGet);

            return Json("Email already registered.", JsonRequestBehavior.AllowGet);
        }
    }
}