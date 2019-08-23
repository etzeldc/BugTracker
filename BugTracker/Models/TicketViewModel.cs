using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace BugTracker.Models
{
    public class TicketViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Submitter { get; set; }
        public string Description { get; set; }
        public string CurrentAssignee { get; set; }
        public int ProjectId { get; set; }
        public int CurrentPriorityId { get; set; }
        public int CurrentStatusId { get; set; }
        public int CurrentTypeId { get; set; }
        public DateTime Created { get; set; }
        public DateTime? Updated { get; set; }
        public IEnumerable<SelectListItem> Priorities { get; set; }
        public IEnumerable<SelectListItem> Statuses { get; set; }
        public IEnumerable<SelectListItem> Types { get; set; }
        public IEnumerable<SelectListItem> Assignees { get; set; }
    }
}