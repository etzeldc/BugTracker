﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using BugTracker.Models;
using BugTracker.Helpers;
using Microsoft.AspNet.Identity;

namespace BugTracker.Controllers
{
    [RequireHttps]
    [Authorize(Roles = "Admin, Project Manager")]

    public class ProjectsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

        // GET: Projects
        [Authorize]

        public ActionResult Index()
        {
            return View(db.Projects.ToList());
        }

        [OverrideAuthorization]
        [Authorize(Roles = "Admin, Project Manager, Developer, Submitter")]
        // GET: Projects/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }

            var allProjectManagers = roleHelper.UsersInRole("Project Manager");
            //var currentProjectManagers = projectHelper.UsersInRoleOnProject(project.Id, "Project Manager");
            ViewBag.ProjectManagers = new MultiSelectList(allProjectManagers, "Id", "FullName");

            var allDevelopers = roleHelper.UsersInRole("Developer");
            //var currentDevelopers = projectHelper.UsersInRoleOnProject(project.Id, "Developer");
            ViewBag.Developers = new MultiSelectList(allDevelopers, "Id", "FullName");

            var allSubmitters = roleHelper.UsersInRole("Submitter");
            //var currentSubmitters = projectHelper.UsersInRoleOnProject(project.Id, "Submitter");
            ViewBag.Submitters = new MultiSelectList(allSubmitters, "Id", "FullName");

            var adminAndProjectManagers = allProjectManagers.Concat(roleHelper.UsersInRole("Admin"));
            //roleHelper.UsersInRole("Project Manager");
            ViewBag.Admins = new MultiSelectList(adminAndProjectManagers, "Id", "FullName");

            return View(project);
        }

        // GET: Projects/Create
        public ActionResult Create()
        {
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,Name,Description")] Project project)
        {
            if (ModelState.IsValid)
            {
                project.Created = DateTime.Now;
                db.Projects.Add(project);
                db.SaveChanges();
                return RedirectToAction("Details", "Projects", new { Id = project.Id });
            }

            return View(project);
        }

        public ActionResult CreateProjectPartial()
        {
            return PartialView();
        }

        // GET: Projects/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,Name,Description,Created")] Project project)
        {
            if (ModelState.IsValid)
            {
                //project.Updated = DateTime.Now;
                db.Entry(project).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            return View(project);
        }

        // GET: Projects/Delete/5
        [Authorize(Roles = "Admin")]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Project project = db.Projects.Find(id);
            if (project == null)
            {
                return HttpNotFound();
            }
            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Project project = db.Projects.Find(id);
            db.Projects.Remove(project);
            db.SaveChanges();
            return RedirectToAction("Index", "Home");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsRead(int? notificationId)
        {
            var notification = db.ProjectNotifications.Find(notificationId);
            notification.Read = true;
            var user = db.Users.Find(User.Identity.GetUserId());
            db.SaveChanges();
            if (notification.Project.Users.Contains(user))
            {
                return RedirectToAction("Details", "Projects", new { notification.Project.Id });
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }
        public ActionResult MarkAllAsRead(string returnUrl)
        {
            foreach (var notification in db.ProjectNotifications)
            {
                notification.Read = true;
            }
            db.SaveChanges();
            return RedirectToLocal(returnUrl);
        }

        private ActionResult RedirectToLocal(string returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            return RedirectToAction("Index", "Home");
        }
    }
}
