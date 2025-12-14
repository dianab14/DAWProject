namespace MicroSocialPlatform.Models
{
    public class Group
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Owner = utilizatorul care detine grupul
        public string OwnerId { get; set; }
        public ApplicationUser Owner { get; set; }
    }
}