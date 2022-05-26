﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JGBugTracker.Models
{
    public class NotificationType
    {
        public int Id { get; set; }
        [Required]
        [DisplayName("Type Name")]
        public string? Name { get; set; }
    }
}
