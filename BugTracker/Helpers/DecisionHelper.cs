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

        //public static bool TicketEditable()
        //{

        //}

        //public static bool TicketTypeEditable()
        //{

        //}

        //public static bool TicketStatusEditable()
        //{

        //}
    }
}