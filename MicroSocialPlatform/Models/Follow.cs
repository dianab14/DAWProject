using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class Follow
    {
        [Key]
        public int Id { get; set; }

        public string FollowerId { get; set; }
        public virtual ApplicationUser? Follower { get; set; }

        public string FollowedId { get; set; }
        public virtual ApplicationUser? Followed { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;

        public DateTime? AcceptedAt { get; set; }
        public string Status { get; set; } = "Pending"; // doar Pending sau Accepted; Rejected = se sterge din DB cererea
    }
}
