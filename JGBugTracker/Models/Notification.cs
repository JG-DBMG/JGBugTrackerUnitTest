﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace JGBugTracker.Models
{
    public class Notification
    {
        // Primary Key
        public int Id { get; set; }

        public int? ProjectId { get; set; }
        public int? ProjectTicket { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Message { get; set; }

        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }

        [Required]
        public string? Sender { get; set; }

        [Required]
        public string? RecipientId { get; set; }
        public int NotificationTypeId { get; set; }

        [DisplayName("Has Been Viewed")]
        public bool Viewed { get; set; }

        // Navigation Properties

        public virtual NotificationType? NotificationType { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Invitor { get; set; }
        public virtual BTUser? Invitee { get; set; }
    }
}