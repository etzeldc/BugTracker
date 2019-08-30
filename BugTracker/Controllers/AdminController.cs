using BugTracker.Helpers;
using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

        // GET USER INDEX
        public ActionResult UserIndex()
        {
            var users = db.Users.Select(u => new UserProfileViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvatarUrl,
                Email = u.Email
            }).ToList();
            ViewBag.RoleName = new SelectList(db.Roles.ToList(), "Name", "Name");
            return View(users);
        }


        // POST USER INDEX
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UserIndex(string userId, string roleName)
        {
            foreach (var role in roleHelper.ListUserRoles(userId))
            {
                roleHelper.RemoveUserFromRole(userId, role);
            }
            if (!string.IsNullOrEmpty(roleName))
            {

                roleHelper.AddUserToRole(userId, roleName);
            }
            return RedirectToAction("UserIndex");
        }

        // GET ASSIGN PROJECTS
        public ActionResult AssignProjects()
        {
            var users = db.Users.Select(u => new UserProfileViewModel
            {
                Id = u.Id,
                FirstName = u.FirstName,
                LastName = u.LastName,
                DisplayName = u.DisplayName,
                AvatarUrl = u.AvatarUrl,
                Email = u.Email
            }).ToList();
            ViewBag.ProjectIds = new MultiSelectList(db.Projects.ToList(), "Id", "Name");
            return View(users);
        }

        // POST ASSIGN PROJECTS
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult AssignProjects(string userId, List<int> projectIds)
        {
            if (projectIds != null)
            {
                foreach (var projectId in projectIds)
                {
                    projectHelper.AddUserToProject(userId, projectId);
                }
            }
            return RedirectToAction("AssignProjects");
        }

        // POST ASSIGN USERS PROJECTS
        [HttpPost]
        [ValidateAntiForgeryToken]
        [OverrideAuthorization]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult ManageProjectUsers(int projectId, List<string>Admins, List<string> ProjectManagers, List<string> Developers, List<string> Submitters)
        {
            var newUsers = new List<string>();
            var project = db.Projects.AsNoTracking().FirstOrDefault(p => p.Id == projectId);
            foreach (var user in projectHelper.UsersNotOnProject(projectId).ToList())
            {
                projectHelper.RemoveUserFromProject(user.Id, projectId);
            }
            if (Admins != null)
            {
                foreach (var adminId in Admins)
                {
                    projectHelper.AddUserToProject(adminId, projectId);
                    newUsers.Add(adminId);

                }
            }
            if (ProjectManagers != null)
            {
                foreach (var projectManagerId in ProjectManagers)
                {
                    projectHelper.AddUserToProject(projectManagerId, projectId);
                    newUsers.Add(projectManagerId);
                }
            }
            if (Developers != null)
            {
                foreach (var developerId in Developers)
                {
                    projectHelper.AddUserToProject(developerId, projectId);
                    newUsers.Add(developerId);
                }
            }
            if (Submitters != null)
            {
                foreach (var submitterId in Submitters)
                {
                    projectHelper.AddUserToProject(submitterId, projectId);
                    newUsers.Add(submitterId);
                }
            }
            projectHelper.GenerateProjectAssignmentNotification(project, newUsers);
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }


        // POST REMOVE USERS FROM PROJECT

        [OverrideAuthorization]
        [Authorize(Roles = "Admin, Project Manager")]
        public ActionResult RemoveProjectUser(string userId, int projectId)
        {
            projectHelper.RemoveUserFromProject(userId, projectId);
            return RedirectToAction("Details", "Projects", new { id = projectId });
        }
    }
}