using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MicroSocialPlatform.Models
{
    public class ApplicationUser : IdentityUser
    {
        public bool IsPrivate { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        [StringLength(50, ErrorMessage = "Max. 50 characters")]
        public string FirstName { get; set; } = null!;

        [Required]
        [StringLength(50, ErrorMessage = "Max. 50 characters")]
        public string LastName { get; set; } = null!;

        [Required]
        public string ProfileImagePath { get; set; } = "/images/profiles/default-profile.png";

        [Required]
        [StringLength(100, ErrorMessage = "Max. 100 characters")]
        public string Description { get; set; } = "Hello, I am using MicroSocialPlatform! :)";

        public bool IsDeleted { get; set; } = false; // pentru soft delete

        // 1–M
        // un user poate avea multiple postari, comentarii, mesaje in grupuri
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<GroupMessage> GroupMessages { get; set; } = new List<GroupMessage>();

        // grupuri create (moderator)
        public virtual ICollection<Group> OwnedGroups { get; set; } = new List<Group>();

        // M–N prin entități asociative
        // un user poate sa faca parte din mai multe grupuri, si sa dea reactii la mai multe postari
        public virtual ICollection<GroupMembership> GroupMemberships { get; set; } = new List<GroupMembership>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

        // follow system
        // un user poate avea atat persoane care-l urmaresc cat si persoane pe care le urmareste
        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();

        // variabila in care vom retine rolurile existente in baza de date
        // pentru popularea unui dropdown list
        [NotMapped]
        public IEnumerable<SelectListItem>? AllRoles { get; set; }

    }
}
