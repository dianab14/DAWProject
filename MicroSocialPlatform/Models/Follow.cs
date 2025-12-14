namespace MicroSocialPlatform.Models
{
    public class Follow
    {
        public int Id { get; set; }

        public string FollowerId { get; set; }
        public virtual ApplicationUser? Follower { get; set; }

        public string FollowedId { get; set; }
        public virtual ApplicationUser? Followed { get; set; }

        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
        public string Status { get; set; } = "Pending";
    }
}
