﻿using Microsoft.AspNetCore.Mvc.Rendering;

namespace JGBugTracker.Models.ViewModels
{
    public class ProjectMembersViewModel
    {
        public Project? Project { get; set; }

        public MultiSelectList? UsersList { get; set; }

        public List<string>? SelectedUsers { get; set; }
    }
}
