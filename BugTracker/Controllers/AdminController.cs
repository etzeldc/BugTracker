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
        public ActionResult UserIndex()
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

            return View(users);
        }

        //
        // Get

        public ActionResult ManageUserRole(string userId)
        {
            //Loading a DropDownList with Role information??
            //New SelectList("data pushed into control", 
            //               "column used to push selection into control",
            //               "column we show the user inside control",
            //               "if role is occupied, show that instead of nothing")
            var currentRole = roleHelper.ListUserRoles(userId).FirstOrDefault();
            ViewBag.UserId = userId;
            ViewBag.Roles = new SelectList(db.Roles.ToList(), "Name", "Name", currentRole);
            return View();
        }

        //
        //// POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageUserRole(string userId, string roles)
        {
            foreach (var role in roleHelper.ListUserRoles(userId))
            {
                roleHelper.RemoveUserFromRole(userId, role);
            }
            //If user 
            if (!string.IsNullOrEmpty(roles))
            {
                roleHelper.AddUserToRole(userId, roles);
            }
            return RedirectToAction("UserIndex");
        }

    }
}