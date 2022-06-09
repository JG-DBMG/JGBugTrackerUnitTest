using JGBugTracker.Extensions;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JGBugTracker.Models
{
    public class Project
    {
        public int Id { get; set; }
        public int CompanyId { get; set; }
        [Required]
        [DisplayName("Project Name")]
        [StringLength(240, ErrorMessage = "The {0} must be at least {2} at most {1} characters long.", MinimumLength = 2)]
        public string? Name { get; set; }
        [Required]
        [DisplayName("Project Description")]
        [StringLength(2000, ErrorMessage = "The {0} must be at least {2} at most {1} characters long.", MinimumLength = 2)]
        public string? Description { get; set; }
        [DataType(DataType.Date)]
        [DisplayName("Date Created")]
        public DateTime Created { get; set; }
        [DataType(DataType.Date)]
        [DisplayName("Project Start Date")]
        public DateTime? StartDate { get; set; }
        [DataType(DataType.Date)]
        [DisplayName("Project End Date")]
        public DateTime? EndDate { get; set; }
        public int ProjectPriorityId { get; set; }
        [NotMapped]
        [DataType(DataType.Upload)]
        [MaxFileSize(1024 * 1024)]  
        public IFormFile? ImageFormFile { get; set; }
        [DisplayName("File Name")]
        public string? ImageFileName { get; set; }
        [DisplayName("Project Image")]
        public byte[]? ImageFileData { get; set; }
        [DisplayName("File Extension")]
        public string? ImageFileContentType { get; set; }

        public bool Archived { get; set; }

        // Navigational properties ORM object relational mapping
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }

        //Navigational collections
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();
    }
}
