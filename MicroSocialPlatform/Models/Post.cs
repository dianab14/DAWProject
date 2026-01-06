using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class Post
    {
        [Key]
        public int Id { get; set; }

        [StringLength(1000, ErrorMessage = "Max. 1000 characters")]
        public string? Content { get; set; }
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // un post poate avea multiple comentarii si reactii
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}
