using BugTracker.Helpers;
using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Admin, Project Manager")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

        // GET Admin
        public ActionResult AssignRoles()
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
        public ActionResult AssignRoles(string userId, string roleName)
        {
            foreach (var role in roleHelper.ListUserRoles(userId))
            {
                roleHelper.RemoveUserFromRole(userId, role);
            }
            if (!string.IsNullOrEmpty(roleName))
            {

                roleHelper.AddUserToRole(userId, roleName);
            }
            return RedirectToAction("AssignRoles");
        }

        //GET
        public ActionResult AssignProjects()
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
            ViewBag.ProjectIds = new MultiSelectList(db.Projects.ToList(), "Id", "Name");
            return View(users);
        }

        ////POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignProjects(string userId, List<int> projectIds)
        {
            //foreach (var project in projectHelper.ListUserProjects(userId))
            //{
            //    projectHelper.RemoveUserFromProject(userId, project.Id);
            //}
            if (projectIds != null)
            {
                foreach (var projectId in projectIds)
                {
                    projectHelper.AddUserToProject(userId, projectId);

                }
            }
            return RedirectToAction("AssignProjects");
        }

        ///POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ManageProjectUsers(int projectId, List<string> ProjectManagers, List<string> Developers, List<string> Submitters)
        {
            foreach (var user in projectHelper.UsersNotOnProject(projectId).ToList())
            {
                projectHelper.RemoveUserFromProject(user.Id, projectId);
            }
            if (ProjectManagers != null)
            {
                foreach (var projectManagerId in ProjectManagers)
                {
                    projectHelper.AddUserToProject(projectManagerId, projectId);
                }
            }
            if (Developers != null)
            {
                foreach (var developerId in Developers)
                {
                    projectHelper.AddUserToProject(developerId, projectId);
                }
            }
            if (Submitters != null)
            {
                foreach (var submitterId in Submitters)
                {
                    projectHelper.AddUserToProject(submitterId, projectId);
                }
            }
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }


        //[HttpPost]
        //[ValidateAntiForgeryToken]
        public ActionResult RemoveProjectUser(string userId, int projectId)
        {
            projectHelper.RemoveUserFromProject(userId, projectId);
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }
    }
}