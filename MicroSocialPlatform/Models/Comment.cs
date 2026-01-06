using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class Comment
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "The comment mustn't be empty!")]
        [StringLength(1000, ErrorMessage = "Max. 1000 characters")]
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }

        // un comentariu apartine unui singur user
        public string UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // un comentariu apartine unui singur post
        public int PostId { get; set; }
        public virtual Post? Post { get; set; }
    }
}
