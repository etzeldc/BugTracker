using BugTracker.Helpers;
using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    //[Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();

        // GET Admin
        public ActionResult ManageUserRoles()
        {
            var users = db.Users.Select(u => new UserProfileViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvartarUrl,
                Email = u.Email
            }).ToList();
            ViewBag.RoleName = new SelectList(db.Roles.ToList(), "Name", "Name");
            return View(users);
        }

        //
        // Get

        //public ActionResult ManageUserRole(string userId)
        //{
        //    //Loading a DropDownList with Role information??
        //    //New SelectList("data pushed into control", 
        //    //               "column used to push selection into control",
        //    //               "column we show the user inside control",
        //    //               "if role is occupied, show that instead of nothing")
        //    var currentRole = roleHelper.ListUserRoles(userId).FirstOrDefault();
        //    ViewBag.UserId = userId;
        //    ViewBag.Roles = new SelectList(db.Roles.ToList(), "Name", "Name", currentRole);
        //    return View();
        //}

        //
        //// POST
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ManageUserRole(string userId, string userRole)
        //{
        //    foreach (var role in roleHelper.ListUserRoles(userId))
        //    {
        //        roleHelper.RemoveUserFromRole(userId, userRole);
        //    }
        //    //If user 
        //    if (!string.IsNullOrEmpty(userRole))
        //    {
        //        roleHelper.AddUserToRole(userId, userRole);
        //    }
        //    return RedirectToAction("UserIndex");
        //}

        // GET
        //public ActionResult ManageRoles()
        //{
        //    var users = db.Users.Select(u => new UserProfileViewModel
        //    {
        //        Id = u.Id
        //    }).ToList();
        //    ViewBag.Users = new MultiSelectList(db.Users.ToList(), "Id", "DisplayName"); //Look into that Full Name thing
        //    ViewBag.RoleName = new SelectList(db.Roles.ToList(), "Name", "Name");
        //    return View(users);
        //}

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageUserRoles(string userId, string roleName)
        {
                foreach (var role in roleHelper.ListUserRoles(userId))
                {
                    roleHelper.RemoveUserFromRole(userId, role);
                }
                if (!string.IsNullOrEmpty(roleName))
                {

                roleHelper.AddUserToRole(userId, roleName);
                }
            return RedirectToAction("ManageUserRoles");
        }

        private ProjectHelper projectHelper = new ProjectHelper();

        //GET
        public ActionResult ManageUserProjects()
        {
            var users = db.Users.Select(u => new UserProfileViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvartarUrl,
                Email = u.Email
            }).ToList();
            ViewBag.Project = new SelectList(db.Projects.ToList(), "Name", "Name");
            return View(users);
        }

        ////POST
        //[HttpPost]
        //[ValidateAntiForgeryToken]
        //public ActionResult ManageUserProjects(string userId, int project) //INT RIGHT?
        //{
        //    foreach (var projects in projectHelper.ListUserProjects(userId))
        //    {
        //        if (!string.IsNullOrEmpty(project)) //SAME STUFF, RIGHT? BUT NEEDS TO BE INT, NOT STRING.
        //        {

        //            //add new role
        //            projectHelper.AddUserToProject(userId, project);
        //        }
        //    return RedirectToAction("ManageUserProjects");
        //}
    }
}