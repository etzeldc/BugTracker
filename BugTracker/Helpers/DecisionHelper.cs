using BugTracker.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BugTracker.Enumerations;
using Microsoft.AspNet.Identity;

namespace BugTracker.Helpers
{
    public static class DecisionHelper
    {
        private static ApplicationDbContext db = new ApplicationDbContext();
        private static UserRolesHelper roleHelper = new UserRolesHelper();
        private static ProjectHelper projectHelper = new ProjectHelper();

        public static bool TicketDetailViewable()
        {
            var userId = HttpContext.Current.User.Identity.GetUserId();

            var roleName = roleHelper.ListUserRoles(userId).FirstOrDefault();
            var systemRole = (SystemRole)Enum.Parse(typeof(SystemRole), roleName);

            switch (systemRole)
            {
                case SystemRole.Admin:
                    break;
                case SystemRole.ProjectManager:
                    break;
                case SystemRole.Developer:
                    break;
                case SystemRole.Submitter:
                    break;
            }
            return true;
        }

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

        //public static bool TicketTypeEditable()
        //{

        //}

        //public static bool TicketStatusEditable()
        //{

        //}
    }
}