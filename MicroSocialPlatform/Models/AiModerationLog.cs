using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class AiModerationLog
    {
        [Key]
        public int Id { get; set; }

        public string Content { get; set; } = null!; // textul analizat

        public bool IsAppropriate { get; set; }

        public double Confidence { get; set; }

        public string ContentType { get; set; } = null!;
        // "Post" / "Comment"

        public string? UserId { get; set; }

        public DateTime CheckedAt { get; set; } = DateTime.UtcNow;
    }
}
