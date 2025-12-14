namespace MicroSocialPlatform.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Text { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // un comentariu apartine unui singur user
        public string UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // un comentariu apartine unui singur post
        public int PostId { get; set; }
        public virtual Post? Post { get; set; }
    }
}
