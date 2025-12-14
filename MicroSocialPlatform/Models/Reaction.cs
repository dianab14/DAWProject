namespace MicroSocialPlatform.Models
{
    public class Reaction
    {
        public int Id { get; set; }
        public string Type { get; set; } = "Like"; // ex: "like", "dislike"

        // o reactie apartine unui singur user
        public string UserId { get; set; }
        public virtual ApplicationUser? User { get; set; }

        // o reactie apartine unui singur post
        public int PostId { get; set; }
        public virtual Post? Post { get; set; }
    }
}
