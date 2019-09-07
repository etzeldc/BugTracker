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
using System.Text;
using System.Net.Mail;
using System.Threading.Tasks;

namespace BugTracker.Helpers
{
    public class TicketHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private UserRolesHelper roleHelper = new UserRolesHelper();
        private ProjectHelper projectHelper = new ProjectHelper();

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
            var ticket = db.Tickets.Find(ticketId);
            return ticket.AssignedToUserId == userId;
        }

        public List<Ticket> MyTickets(string userId, string roleName)
        {
            var myTickets = new List<Ticket>();
            var user = db.Users.Find(userId);
            switch (roleName)
            {
                case "Developer":
                    myTickets = db.Tickets.AsNoTracking().Where(t => t.AssignedToUserId == userId).ToList();
                    break;
                case "Submitter":
                    myTickets = db.Tickets.AsNoTracking().Where(t => t.OwnerUserId == userId).ToList();
                    break;
                case "Project Manager":
                    //myTickets = db.Users.AsNoTracking().Where(u => u.Id == userId).Projects.SelectMany(t => t.Tickets).ToList();

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
            return db.Projects.AsNoTracking().FirstOrDefault(p => p.Id == projectId).Tickets;
        }

        public ICollection<Ticket> UnassignedTicketsOnProject(int projectId)
        {
            var newTickets = new List<Ticket>();
            foreach (var ticket in TicketsOnProject(projectId))
            {
                if (ticket.AssignedToUserId == null)
                {
                    newTickets.Add(ticket);
                }
            }
            return newTickets;
        }

        public ICollection<Ticket> UserUnassignedTickets(string userId)
        {
            var unTickets = new List<Ticket>();
            foreach (var project in db.Users.Find(userId).Projects)
            {
                foreach (var ticket in UnassignedTicketsOnProject(project.Id))
                {
                    unTickets.Add(ticket);
                }
            }
            return unTickets;
        }

        public ICollection<Ticket> AllUnassignedTickets()
        {
            var allUnTickets = new List<Ticket>();
            foreach (var ticket in db.Tickets.AsNoTracking().ToList())
            {
                if (ticket.AssignedToUserId == null)
                {
                    allUnTickets.Add(ticket);
                }
            }
            return allUnTickets;
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

        public static string MakeReadable(string property, string value)
        {
            switch (property)
            {
                case "TicketStatusId":
                    return db.TicketStatuses.Find(Convert.ToInt32(value)).Name;
                case "TicketPriorityId":
                    return db.TicketPriorities.Find(Convert.ToInt32(value)).Name;
                case "TicketTypeId":
                    return db.TicketTypes.Find(Convert.ToInt32(value)).Name;
                case "AssignedToUserId":
                case "OwnerUserId":
                    if (!string.IsNullOrEmpty(value))
                        return db.Users.Find(value).DisplayName;
                    return "Unassigned";
                default:
                    return value;
            }
        }

        #region Assignment Notifications
        public async Task CreateAssignmentNotification(Ticket oldTicket, Ticket newTicket)
        {
            var noChange = (oldTicket.AssignedToUserId == newTicket.AssignedToUserId);
            var assignment = (string.IsNullOrEmpty(oldTicket.AssignedToUserId));
            var unassignment = (string.IsNullOrEmpty(newTicket.AssignedToUserId));

            if (noChange)
                return;

            if (assignment)
            {
                await GenerateAssignmentNotification(newTicket);
            }
            else if (unassignment)
            {
                await GenerateUnAssignmentNotification(oldTicket);
            }
            else
            {
                await GenerateAssignmentNotification(newTicket);
                await GenerateUnAssignmentNotification(oldTicket);
            }
        }

        private async Task GenerateUnAssignmentNotification(Ticket oldTicket)
        {
            var notification = new TicketNotification
            {
                Created = DateTime.Now,
                Subject = $"removed you from ticket {oldTicket.Title}",
                Read = false,
                RecipientId = oldTicket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"You are no longer assigned to {oldTicket.Title}.",
                TicketId = oldTicket.Id,
            };

            db.TicketNotifications.Add(notification);
            db.SaveChanges();
            var newNotification = db.TicketNotifications.Include("Sender").Include("Recipient").FirstOrDefault(t => t.Id == notification.Id);
            await GenerateNotificationEmail(newNotification);
        }

        private async Task GenerateAssignmentNotification(Ticket newTicket)
        {
            var notification = new TicketNotification
            {
                Created = DateTime.Now,
                Subject = $"assigned you to a new ticket",
                Read = false,
                RecipientId = newTicket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"You have been assigned to ticket {newTicket.Title}.",
                TicketId = newTicket.Id
            };
            db.TicketNotifications.Add(notification);
            db.SaveChanges();
            var newNotification = db.TicketNotifications.Include("Sender").Include("Recipient").FirstOrDefault(t => t.Id == notification.Id);
            await GenerateNotificationEmail(newNotification);
        }

        private async Task GenerateNotificationEmail(TicketNotification notification)
        {
            var emailFrom = $"{notification.Sender.Email}<{WebConfigurationManager.AppSettings["emailfrom"]}>";
            var email = new MailMessage(emailFrom, notification.Recipient.Email)
            {
                Subject = notification.Subject,
                Body = notification.NotificationBody,
                IsBodyHtml = true
            };
            var svc = new PersonalEmail();
            await svc.SendAsync(email);
        }
       
        #endregion

        #region Comment Notifications
        public void GenerateCommentNotification(Ticket ticket, TicketComment ticketComment)
        {
            var subComment = (ticketComment.AuthorId == ticket.OwnerUserId);
            var devComment = (ticketComment.AuthorId == ticket.AssignedToUserId);
            var pmComment = (ticketComment.AuthorId == projectHelper.UsersInRoleOnProject(ticket.ProjectId, "Project Manager").FirstOrDefault());
            var adComment = (ticketComment.AuthorId == roleHelper.UsersInRole("Admin").FirstOrDefault().Id);
            if (subComment)
            {
                GenerateSubCommentNotification(ticket, ticketComment);
            }
            else if (devComment)
            {
                GenerateDevCommentNotification(ticket, ticketComment);
            }
            else if (pmComment)
            {
                GeneratePMCommentNotification(ticket, ticketComment);
            }
            else if (adComment)
            {
                GenerateAdCommentNotification(ticket, ticketComment);
            }
        }
        public void GenerateDevCommentNotification(Ticket ticket, TicketComment ticketComment)
        {
            var authorName = db.Users.Find(ticketComment.AuthorId).DisplayName;
            var subNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = ticket.OwnerUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            var pmNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = projectHelper.UsersInRoleOnProject(ticket.ProjectId, "Project Manager").FirstOrDefault(),
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            db.TicketNotifications.Add(subNotification);
            db.TicketNotifications.Add(pmNotification);
            db.SaveChanges();
        }
        public void GeneratePMCommentNotification(Ticket ticket, TicketComment ticketComment)
        {
            var authorName = db.Users.Find(ticketComment.AuthorId).DisplayName;
            var subNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = ticket.OwnerUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            var devNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = ticket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            db.TicketNotifications.Add(subNotification);
            db.TicketNotifications.Add(devNotification);
            db.SaveChanges();
        }
        public void GenerateSubCommentNotification(Ticket ticket, TicketComment ticketComment)
        {
            var authorName = db.Users.Find(ticketComment.AuthorId).DisplayName;
            var devNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = ticket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            var pmNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = projectHelper.UsersInRoleOnProject(ticket.ProjectId, "Project Manager").FirstOrDefault(),
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            db.TicketNotifications.Add(devNotification);
            db.TicketNotifications.Add(pmNotification);
            db.SaveChanges();
        }
        public void GenerateAdCommentNotification(Ticket ticket, TicketComment ticketComment)
        {
            var authorName = db.Users.Find(ticketComment.AuthorId).DisplayName;
            var subNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = ticket.OwnerUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            var devNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = ticket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            var pmNotification = new TicketNotification
            {
                TicketId = ticket.Id,
                Created = DateTime.Now,
                Subject = $"commented on {ticket.Title}.",
                Read = false,
                RecipientId = projectHelper.UsersInRoleOnProject(ticket.ProjectId, "Project Manager").FirstOrDefault(),
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"{authorName} commented: {ticketComment.CommentBody}"
            };
            db.TicketNotifications.Add(subNotification);
            db.TicketNotifications.Add(devNotification);
            db.TicketNotifications.Add(pmNotification);
            db.SaveChanges();
        }
        #endregion

        #region Attachment Notifications
        public static void CreateAttachmentNotification(Ticket ticket)
        {
            var notification = new TicketNotification
            {
                Created = DateTime.Now,
                Subject = $"added an attachment to your ticket.",
                Read = false,
                RecipientId = ticket.AssignedToUserId,
                SenderId = HttpContext.Current.User.Identity.GetUserId(),
                NotificationBody = $"There is a new attachment on ticket {ticket.Title}.",
                TicketId = ticket.Id
            };
            db.TicketNotifications.Add(notification);
            db.SaveChanges();
        }
        #endregion

        #region History
        public void CreateHistoryRecord(Ticket oldTicket, Ticket newTicket)
        {
            foreach (var property in newTicket.GetType().GetProperties())
            {
                //checking to see if current property is relevant
                var trackedProperties = WebConfigurationManager.AppSettings["TrackHistoryProperties"].Split(',').ToList();
                if (!trackedProperties.Contains(property.Name))
                    continue;

                //capturing old and new property values
                var oldValue = (property.GetValue(oldTicket, null) ?? "").ToString();
                var newValue = (property.GetValue(newTicket, null) ?? "").ToString();

                if (oldValue != newValue)
                {
                    var newHistory = new TicketHistory
                    {
                        UserId = HttpContext.Current.User.Identity.GetUserId(),
                        Updated = newTicket.Updated.GetValueOrDefault(),
                        PropertyName = property.Name,
                        OldValue = MakeReadable(property.Name, oldValue.ToString()),
                        NewValue = MakeReadable(property.Name, newValue.ToString()),
                        TicketId = newTicket.Id
                    };
                    db.TicketHistories.Add(newHistory);
                }
            }
            db.SaveChanges();
        }
        #endregion

        #region Change Notification
        public void CreateChangeNotification(Ticket oldTicket, Ticket newTicket)
        {
            var messageBody = new StringBuilder();
            foreach (var property in WebConfigurationManager.AppSettings["TrackedTicketProperties"].Split(','))
            {
                var oldValue = MakeReadable(property, oldTicket.GetType().GetProperty(property).GetValue(oldTicket, null).ToString());
                var newValue = MakeReadable(property, newTicket.GetType().GetProperty(property).GetValue(newTicket, null).ToString());
                if (oldValue != newValue)
                {
                    messageBody.AppendLine($"");
                    messageBody.AppendLine($"The {property} of {newTicket.Title} was changed from \"{oldValue.ToString()}\" to \"{newValue.ToString()}\".");
                }
            }

            if (!string.IsNullOrEmpty(messageBody.ToString()))
            {
                var notification = new TicketNotification
                {
                    TicketId = newTicket.Id,
                    Created = DateTime.Now,
                    Subject = $"made a change to ticket {newTicket.Title}",
                    Read = false,
                    RecipientId = newTicket.AssignedToUserId,
                    SenderId = HttpContext.Current.User.Identity.GetUserId(),
                    NotificationBody = messageBody.ToString()
                };

                db.TicketNotifications.Add(notification);
                db.SaveChanges();
            }
        }

        #endregion

        #region Dashboard Notification
        public static int GetNewUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.AsNoTracking().Where(t => t.RecipientId == userId && !t.Read).Count();
        }
        public static int GetReadUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.AsNoTracking().Where(t => t.RecipientId == userId && t.Read).Count();
        }
        public static int GetAllUserNotificationCount()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.AsNoTracking().Where(t => t.RecipientId == userId).Count();
        }
        public static List<TicketNotification> GetUnreadNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.AsNoTracking().Where(t => t.RecipientId == userId && !t.Read).ToList();
        }
        public static List<TicketNotification> GetReadNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.AsNoTracking().Where(t => t.RecipientId == userId && t.Read).ToList();
        }
        public static List<TicketNotification> GetAllUserNotifications()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketNotifications.AsNoTracking().Where(t => t.RecipientId == userId).ToList();
        }
        public static List<TicketHistory> GetAllUserHistories()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();
            return db.TicketHistories.AsNoTracking().Where(t => t.UserId == userId).ToList();
        }
        #endregion


    }
}