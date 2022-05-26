using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JGBugTracker.Models
{
    public class TicketType
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Ticket Type Name")]
        public string? Name { get; set; }
    }
}
