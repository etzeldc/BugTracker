using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Validation;

namespace BugTracker.Helpers
{
    public class ProjectHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();

        public List<string> UsersInRoleOnProject(int projectId, string roleName)
        {
            var people = new List<string>();

            foreach (var user in UsersOnProject(projectId).ToList())
            {
                if (roleHelper.IsUserInRole(user.Id, roleName))
                {
                    people.Add(user.Id);
                }
            }

            return people;
        }
        public bool IsUserOnProject(string userId, int projectId)
        {
            var project = db.Projects.Find(projectId);
            var flag = project.Users.Any(u => u.Id == userId);
            return (flag);
        }

        public ICollection<Project>ListUserProjects(string userId)
        {
            ApplicationUser user = db.Users.Find(userId);

            var projects = user.Projects.ToList();
            return (projects);
        }

        public void AddUserToProject(string userId, int projectId)
        {
            if (!IsUserOnProject(userId, projectId))
            {
                Project proj = db.Projects.Find(projectId);
                var newUser = db.Users.Find(userId);

                proj.Users.Add(newUser);
                db.SaveChanges();
            }
        }

        public void RemoveUserFromProject(string userId, int projectId)
        {
            if (IsUserOnProject(userId, projectId))
            {
                Project proj = db.Projects.Find(projectId);
                var delUser = db.Users.Find(userId);

                proj.Users.Remove(delUser);
                db.Entry(proj).State = EntityState.Modified;
                GenerateProjectUnassignmentNotification(userId, projectId);
                db.SaveChanges();
            }
        }

        public ICollection<ApplicationUser>UsersOnProject(int projectId)
        {
            return db.Projects.Find(projectId).Users;
        }

        public ICollection<ApplicationUser> UsersNotOnProject(int projectId)
        {
            return db.Users.Where(u => u.Projects.All(p => p.Id != projectId)).ToList();
        }

        public ICollection<Ticket> TicketsOnProject(int projectId)
        {
            return db.Projects.Find(projectId).Tickets.ToList();
        }

        public ICollection<Project> UnassignedProjects()
        {
            var newProjects = new List<Project>();
            var allProjects = db.Projects.ToList();
            foreach (var project in allProjects)
            {
                if (UsersInRoleOnProject(project.Id, "Admin").Count() == 0 &&
                    UsersInRoleOnProject(project.Id, "Project Manager").Count() == 0 ||
                    UsersInRoleOnProject(project.Id, "Developer").Count() == 0 ||
                    UsersInRoleOnProject(project.Id, "Submitter").Count() == 0)
                {
                    newProjects.Add(project);
                }
            }
            return newProjects;
        }

        public void GenerateProjectAssignmentNotification(Project project, List<string> users)
        {
            foreach (var user in users)
            {
                var notification = new ProjectNotification
                {
                    Created = DateTime.Now,
                    Subject = $"assigned you to a project.",
                    Read = false,
                    RecipientId = user,
                    SenderId = HttpContext.Current.User.Identity.GetUserId(),
                    NotificationBody = $"You have been assigned to {project.Name}.",
                    ProjectId = project.Id
                };
                db.ProjectNotifications.Add(notification);
            }
            db.SaveChanges();
        }
        public void GenerateProjectUnassignmentNotification(string userId, int projectId)
        {
            var project = db.Projects.Find(projectId);
            var notification = new ProjectNotification
            {
                Created = DateTime.Now,
                Subject = $"has removed you from a project.",
                Read = false,
                RecipientId = userId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"You have been removed from {project.Name}.",
                ProjectId = projectId
            };
            db.ProjectNotifications.Add(notification);
            db.SaveChanges();
        }

        #region Dashboard Notification
        public static int GetNewUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.ProjectNotifications.AsNoTracking().Where(t => t.RecipientId == userId && !t.Read).Count();
        }
        public static int GetReadUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.ProjectNotifications.AsNoTracking().Where(t => t.RecipientId == userId && t.Read).Count();
        }
        public static int GetAllUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.ProjectNotifications.AsNoTracking().Where(t => t.RecipientId == userId).Count();
        }
        public static List<ProjectNotification> GetUnreadNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.ProjectNotifications.AsNoTracking().Where(t => t.RecipientId == userId && !t.Read).ToList();
        }
        public static List<ProjectNotification> GetReadNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.ProjectNotifications.AsNoTracking().Where(t => t.RecipientId == userId && t.Read).ToList();
        }
        public static List<ProjectNotification> GetAllUserNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.ProjectNotifications.AsNoTracking().Where(t => t.RecipientId == userId).ToList();
        }
        #endregion

    }
}