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
    public class TicketCommentsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        private TicketHelper ticketHelper = new TicketHelper();


        // GET: TicketComments/Create
        public ActionResult Create()
        {
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName");
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "OwnerUserId");
            return View();
        }

        // POST: TicketComments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public ActionResult Create([Bind(Include = "TicketId,CommentBody")] TicketComment ticketComment, int ticketId)
        {
            if (ModelState.IsValid)
            {
                var newTicket = db.Tickets.Find(ticketId);
                ticketComment.AuthorId = User.Identity.GetUserId();
                ticketComment.Created = DateTime.Now;
                db.TicketComments.Add(ticketComment);
                db.SaveChanges();
                ticketHelper.GenerateCommentNotification(newTicket, ticketComment);
                return RedirectToAction("Details", "Tickets", new { id = ticketId });
            }

            ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketComment.AuthorId);
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "OwnerUserId", ticketComment.TicketId);
            return View(ticketComment);
        }

        public ActionResult CommentsPartial()
        {
            return PartialView();
        }
        public ActionResult CommentEditPartial()
        {
            return PartialView();
        }

        // GET: TicketComments/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            TicketComment ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketComment.AuthorId);
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "OwnerUserId", ticketComment.TicketId);
            return View(ticketComment);
        }

        // POST: TicketComments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see https://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "Id,TicketId,AuthorId,CommentBody,Created")] TicketComment ticketComment, int pk, int fk)
        {
            var newComment = db.TicketComments.Find(pk);
            if (ModelState.IsValid)
            {
                newComment.CommentBody = ticketComment.CommentBody;
                db.SaveChanges();
                return RedirectToAction("Details", "Tickets", new { id = fk });
            }
            ViewBag.AuthorId = new SelectList(db.Users, "Id", "FirstName", ticketComment.AuthorId);
            ViewBag.TicketId = new SelectList(db.Tickets, "Id", "OwnerUserId", ticketComment.TicketId);
            return View(ticketComment);
        }

        // GET: TicketComments/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            var ticketComment = db.TicketComments.Find(id);
            if (ticketComment == null)
            {
                return HttpNotFound();
            }
            return View(ticketComment);
        }

        // POST: TicketComments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int pk)
        {
            var ticketComment = db.TicketComments.Find(pk);
            var ticketId = ticketComment.TicketId;
            db.TicketComments.Remove(ticketComment);
            db.SaveChanges();
            return RedirectToAction("Details", "Tickets", new { id = ticketId });
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
