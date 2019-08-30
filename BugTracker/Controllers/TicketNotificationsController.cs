using System;
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
    public class TicketNotificationsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketHelper ticketHelper = new TicketHelper();
        private ProjectHelper projectHelper = new ProjectHelper();
        // GET: TicketNotifications
        public ActionResult Index()
        {
            var ticketNotifications = db.TicketNotifications.Include(t => t.Recipient).Include(t => t.Sender).Include(t => t.Ticket);
            return View(ticketNotifications.ToList());
        }

        // GET: TicketNotifications/Create
        public ActionResult Create()
        {
            ViewBag.RecipientId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.SenderId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "OwnerUserId");
            return View();
        }

        // POST: TicketNotifications/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "Id,TicketId,RecipientId,SenderId,Created,Subject,NotificationBody,Read")] TicketNotification ticketNotification)
        {
            if (ModelState.IsValid)
            {
                db.TicketNotifications.Add(ticketNotification);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.RecipientId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.RecipientId);
            ViewBag.SenderId = new SelectList(db.Users, "Id", "FirstName", ticketNotification.SenderId);
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "OwnerUserId", ticketNotification.TicketId);
            return View(ticketNotification);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult MarkAsRead(int? notificationId)
        {
            var notification = db.TicketNotifications.Find(notificationId);
            notification.Read = true;
            db.SaveChanges();
            if (User.Identity.GetUserId() == notification.Ticket.AssignedToUserId || User.Identity.GetUserId() == notification.Ticket.OwnerUserId || User.Identity.GetUserId() == projectHelper.UsersInRoleOnProject(notification.Ticket.ProjectId, "Project Manager").FirstOrDefault() || User.IsInRole("Admin"))
            {
                return RedirectToAction("Details", "Tickets", new { notification.Ticket.Id });
            }
            else
            {
                return RedirectToAction("Index", "TicketNotifications");
            }
        }

        public ActionResult MarkAllAsRead(string returnUrl)
        {
            foreach (var notification in db.TicketNotifications)
            {
                notification.Read = true;
            }
            db.SaveChanges();
            return RedirectToAction("Index", "TicketNotifications");
        }

        public ActionResult NotificationsPartial()
        {
            return PartialView();
        }
    }
}
