using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JGBugTracker.Models
{
    public class TicketStatus
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Ticket Status Name")]
        public string? Name { get; set; }
    }
}
