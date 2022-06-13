using Microsoft.AspNetCore.Mvc.Rendering;

namespace JGBugTracker.Models.ViewModels
{
    public class AssignDeveloperViewModel
    {
        public SelectList? Developers { get; set; }
        public Ticket? Ticket { get; set; }
    }
}
