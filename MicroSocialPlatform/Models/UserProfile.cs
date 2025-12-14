using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class UserProfile
    {
        [Key]
        public int Id { get; set; }
        [Required] 
        public string UserId { get; set; }
        [Required]
        public ApplicationUser User { get; set; }
        public string? FullName { get; set; }

        [StringLength(100, ErrorMessage = "Max. 100 characters")]
        public string? Description { get; set; }
        public string? ProfileImagePath { get; set; }
    }
}
