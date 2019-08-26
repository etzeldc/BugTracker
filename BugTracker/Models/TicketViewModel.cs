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
        public int TicketPriorityId { get; set; }
        public int TicketStatusId { get; set; }
        public int TicketTypeId { get; set; }
        public DateTime Created { get; set; }
        public IEnumerable<SelectListItem> CurrentPriority { get; set; }
        public IEnumerable<SelectListItem> CurrentStatus { get; set; }
        public IEnumerable<SelectListItem> CurrentType { get; set; }
    }
}