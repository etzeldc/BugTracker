﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BugTracker.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Web.Configuration;

namespace BugTracker.Helpers
{
    public class TicketHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();

        public List<string> UsersInRoleOnTicket(int ticketId, string roleName)
        {
            var people = new List<string>();

            foreach (var user in UsersOnTicket(ticketId).ToList())
            {
                if (roleHelper.IsUserInRole(user.Id, roleName))
                {
                    people.Add(user.Id);
                }
            }

            return people;
        }

        public bool IsUserOnTicket(string userId, int ticketId)
        {
            var ticket = db.Projects.Find(ticketId);
            var flag = ticket.Users.Any(u => u.Id == userId);
            return (flag);
        }

        public List<Ticket> MyTickets(string userId, string roleName)
        {
            var myTickets = new List<Ticket>();
            switch (roleName)
            {
                case "Developer":
                    myTickets = db.Tickets.Where(t => t.AssignedToUserId == userId).ToList();
                    break;
                case "Submitter":
                    myTickets = db.Tickets.Where(t => t.OwnerUserId == userId).ToList();
                    break;
                case "Project Manager":
                    myTickets = db.Users.Find(userId).Projects.SelectMany(t => t.Tickets).ToList();
                    break;
                case "Admin":
                    myTickets = db.Tickets.ToList();
                    break;
            }
            return myTickets;
        }

        public void AddUserToTicket(string userId, int ticketId)
        {
            if (!IsUserOnTicket(userId, ticketId))
            {
                Ticket tick = db.Tickets.Find(ticketId);
                var newUser = db.Users.Find(userId);

                tick.Project.Users.Add(newUser);
                db.SaveChanges();
            }
        }

        public void RemoveUserFromTicket(string userId, int ticketId)
        {
            if (IsUserOnTicket(userId, ticketId))
            {
                Ticket tick = db.Tickets.Find(ticketId);
                var delUser = db.Users.Find(userId);

                tick.Project.Users.Remove(delUser);
                db.Entry(tick).State = EntityState.Modified;
                db.SaveChanges();
            }
        }

        public ICollection<ApplicationUser> UsersOnTicket(int ticketId)
        {
            return db.Tickets.Find(ticketId).Project.Users;
        }

        public ICollection<Ticket> TicketsOnProject(int projectId)
        {
            return db.Projects.Find(projectId).Tickets;
        }

        public string TicketsByPriority(string priorityName)
        {
            switch (priorityName)
            {
                case "Immediate":
                    return "text-red";
                case "High":
                    return "text-orange";
                case "Medium":
                    return "text-yellow";
                case "Low":
                    return "text-green";
                case "None":
                    return "text-blue";
                default:
                    return "";
            }
        }

        public string TicketAssignee(int ticketId)
        {
            return db.Tickets.Find(ticketId).AssignedToUserId;
        }

        public static void CreateAssignmentNotification(Ticket oldTicket, Ticket newTicket)
        {
            var noChange = (oldTicket.AssignedToUserId == newTicket.AssignedToUserId);
            var assignment = (string.IsNullOrEmpty(oldTicket.AssignedToUserId));
            var unassignment = (string.IsNullOrEmpty(newTicket.AssignedToUserId));

            if (noChange)
                return;

            if (assignment)
                GenerateAssignmentNotification(oldTicket, newTicket);
            else if (unassignment)
                GenerateUnAssignmentNotification(oldTicket, newTicket);
            else
            {
                GenerateAssignmentNotification(oldTicket, newTicket);
                GenerateUnAssignmentNotification(oldTicket, newTicket);
            }
        }

        private static void GenerateUnAssignmentNotification(Ticket oldTicket, Ticket newTicket)
        {
            return;
        }

        private static void GenerateAssignmentNotification(Ticket oldTicket, Ticket newTicket)
        {
            var notification = new TicketNotification
            {
                Created = DateTime.Now,
                Subject = $"You have been assigned to Ticket {newTicket.Id} on {DateTime.Now}",
                Read = false,
                RecipientId = newTicket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"Please acknowledge that you have read this notification by marking it as read",
                TicketId = newTicket.Id,
            };

            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }

        public static void CreateChangeNotification(Ticket oldTicket, Ticket newTicket)
        {
            foreach (var property in WebConfigurationManager.AppSettings["TrackedTicketProperties"].Split(','))
            {
                var oldValue = oldTicket.GetType().GetProperty(property).GetValue(oldTicket, null);
                var newValue = newTicket.GetType().GetProperty(property).GetValue(property, null);
                if (oldValue != newValue)
                    GenerateChangeNotification(property, oldValue, newValue, newTicket.AssignedToUserId, newTicket.Id);
            }

        }

        private static void GenerateChangeNotification(string property, object oldValue, object newValue, string recipientId, int ticketId)
        {
            var notification = new TicketNotification
            {
                Created = DateTime.Now,
                Subject = $"A change has occurred in the Ticket {ticketId} on {DateTime.Now}",
                Read = false,
                RecipientId = recipientId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"A change to property {property} was made at {DateTime.Now}. The old value was {oldValue.ToString()}, and the new value is {newValue.ToString()}.",
                TicketId = ticketId,
            };

            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }

        public static int GetNewUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.Where(t => t.RecipientId == userId && !t.Read).Count();
        }

        public static int GetReadUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.Where(t => t.RecipientId == userId && t.Read).Count();
        }
        public static int GetAllUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.Where(t => t.RecipientId == userId).Count();
        }

        public static List<TicketNotification> GetUnreadNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.Where(t => t.RecipientId == userId && !t.Read).ToList();
        }
        public static List<TicketNotification> GetReadNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.Where(t => t.RecipientId == userId && t.Read).ToList();
        }

        public static List<TicketNotification> GetAllUserNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.Where(t => t.RecipientId == userId).ToList();
        }
    }
}