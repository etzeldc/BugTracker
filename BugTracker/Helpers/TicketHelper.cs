using System;
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
                Subject = $"assigned you to a new ticket",
                Read = false,
                RecipientId = newTicket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"You have been assigned to ticket {newTicket.Title}.",
                TicketId = newTicket.Id,
            };

            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }

        public static void CreateHistory(Ticket oldTicket, Ticket newTicket)
        {
            foreach (var property in WebConfigurationManager.AppSettings["TrackTicketHistory"].Split(','))
            {
                var oldValue = oldTicket.GetType().GetProperty(property).GetValue(oldTicket, null).ToString();
                var newValue = newTicket.GetType().GetProperty(property).GetValue(newTicket, null).ToString();
                if (oldValue != newValue)
                    GenerateHistory(property, oldValue, newValue, newTicket.AssignedToUserId, newTicket.Id);
            }
        }

        private static void GenerateHistory(string property, object oldValue, object newValue, string assignedToUserId, int id)
        {
            throw new NotImplementedException();
        }




        public static void CreateChangeNotification(Ticket oldTicket, Ticket newTicket)
        {
            var changes = new List<string>();
            foreach (var property in WebConfigurationManager.AppSettings["TrackedTicketProperties"].Split(','))
            {
                var oldValue = oldTicket.GetType().GetProperty(property).GetValue(oldTicket, null).ToString();
                var newValue = newTicket.GetType().GetProperty(property).GetValue(newTicket, null).ToString();
                if (oldValue != newValue)
                    changes.Add(property);
            }
            if (changes.Count() > 0)
                GenerateChangeNotification(changes, newTicket);

        }

        private static void GenerateChangeNotification(List<string> changes, Ticket newTicket)
        {
            var output = "";
            if (changes.Count() == 1)
            {
                output = string.Join("", changes).Replace("Id", "");
            }
            else if (changes.Count() == 2)
            {
                output = string.Join(" and ", changes).Replace("Id", "");
            }
            else if (changes.Count() > 2)
            {
                output = string.Join(", ", changes.Take(changes.Count() - 1));
                output = output + string.Concat(", and ", changes.Last());
                output = output.Replace("Id", "");
            }
            var notification = new TicketNotification
            {
                Created = DateTime.Now,
                Subject = $"made a change to ticket {newTicket.Title}",
                Read = false,
                RecipientId = newTicket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"The {output} of {newTicket.Title} has changed.",
                TicketId = newTicket.Id,
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
            return db.TicketNotifications.AsNoTracking().Where(t => t.RecipientId == userId && !t.Read).ToList();
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