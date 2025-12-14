using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? ProfileImagePath { get; set; }

        [StringLength(100, ErrorMessage = "Max. 100 characters")]
        public string? Description { get; set; }

        public bool IsPrivate { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;


        // // un user poate avea multiple postari, comentarii, reactii
        // public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        // public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        // public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

        // // un user poate avea atat persoane care-l urmaresc cat si persoane pe care le urmareste
        // public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        // public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();

        // // un user poate sa faca parte din mai multe grupuri
        // public virtual ICollection<Member> GroupMemberships { get; set; } = new List<Member>();
    }
}
