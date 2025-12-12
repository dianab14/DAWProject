using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class ApplicationUser
    {
        [Required(ErrorMessage = "Username is mandatory")]
        [MaxLength(50)]
        public string Username { get; set; }

        [StringLength(100, ErrorMessage = "Max. 100 characters")]
        public string? Description { get; set; }

        public bool IsPrivate { get; set; } = false;

        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();


        public virtual ICollection<Member> Members { get; set; } = new List<Member>();
    }
}
