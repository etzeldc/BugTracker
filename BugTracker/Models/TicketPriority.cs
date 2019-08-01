using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }

        //virtuakl nav
        public virtual ICollection<Ticket> Tickets { get; set; }
        public TicketPriority()
        {
            Tickets = new HashSet<Ticket>();
        }
    }
}