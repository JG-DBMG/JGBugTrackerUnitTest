using Microsoft.AspNetCore.Mvc.Rendering;

namespace JGBugTracker.Models.ViewModels
{
    public class AssignProjectManagerViewModel
    {
        public Project? Project { get; set; }

        public SelectList? PMList { get; set; }

        public string? PMID { get; set; }

    }
}
