using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JGBugTracker.Models
{
    public class TicketPriority
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Ticket Priority Name")]
        public string? Name { get; set; }
    }
}
