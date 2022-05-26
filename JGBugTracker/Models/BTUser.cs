using Microsoft.AspNetCore.Identity;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace JGBugTracker.Models
{
    public class BTUser : IdentityUser
    {
        [Required]
        [Display(Name = "First Name")]
        [StringLength(40, ErrorMessage = "The {0} must be a minimum of {2} characters and a max of {1}.", MinimumLength = 2)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [Display(Name = "Last Name")]
        [StringLength(40, ErrorMessage = "The {0} must be a minimum of {2} characters and a max of {1}.", MinimumLength = 2)]
        public string LastName { get; set; } = string.Empty;

        [NotMapped]
        [DisplayName("Full Name")]
        public string? FullName { get { return $"{FirstName} {LastName}"; } }

        [NotMapped]
        [DataType(DataType.Upload)]
        // TODO: Add Custom Attributes
        public IFormFile? AvatarFormFile { get; set; }

        [DisplayName("Avatar")]
        public string? AvatarName { get; set; }
        public byte[]? AvatarData { get; set; }

        [DisplayName("File Extension")]
        public string? AvatarContentType { get; set; }
        public int CompanyId { get; set; }

        // Navigational Properties
        public virtual Company? Company { get; set; }

        public virtual ICollection<Project>? Projects { get; set; } = new HashSet<Project>();

    }
}
