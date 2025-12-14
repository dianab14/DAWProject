namespace MicroSocialPlatform.Models
{
    public class Post
    {
        public int Id { get; set; }

        public string? Content { get; set; }
        public string? ImagePath { get; set; }
        public string? VideoPath { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public string UserId { get; set; }
        public ApplicationUser User { get; set; }

        // un post poate avea multiple comentarii si reactii
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();
    }
}
