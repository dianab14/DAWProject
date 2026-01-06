using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class GroupMessage
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The comment mustn't be empty!")]
        [StringLength(1000, ErrorMessage = "Max. 1000 characters")]
        public string Content { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public string UserId { get; set; }
        public ApplicationUser? User { get; set; }

        public int GroupId { get; set; }
        public Group? Group { get; set; }
    }
}
