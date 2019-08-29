using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.Identity;

namespace BugTracker.Helpers
{
    public static class DecisionHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private static UserRolesHelper roleHelper = new UserRolesHelper();
        private static ProjectHelper projectHelper = new ProjectHelper();

        public static bool TicketEditable(Ticket ticket)
        {

            var userId = HttpContext.Current.User.Identity.GetUserId();
            var myRole = roleHelper.ListUserRoles(userId).FirstOrDefault();

            switch (myRole)
            {
                case "Developer":
                    return ticket.AssignedToUserId == userId;
                case "Submitter":
                    return ticket.OwnerUserId == userId;
                case "Project Manager":
                    var myProjects = projectHelper.ListUserProjects(userId);
                    foreach (var project in myProjects)
                    {
                        foreach (var projTick in project.Tickets)
                        {
                            if (projTick.Id == ticket.Id)
                            {
                                return true;
                            }
                        }
                    }
                    return false;
                case "Admin":
                    return true;
                default:
                    return false;
            }
        }
    }
}