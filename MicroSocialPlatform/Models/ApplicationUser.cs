using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class ApplicationUser
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is mandatory")]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is mandatory")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is mandatory")]
        public string Password { get; set; } = string.Empty;

        [StringLength(100, ErrorMessage = "Max. 100 characters")]
        public string? Description { get; set; }

        public bool IsPrivate { get; set; } = false;

        //// un user poate avea multiple postari, comentarii, reactii
        //public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        //public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        //public virtual ICollection<Reaction> Reactions { get; set; } = new List<Reaction>();

        //// un user poate avea atat persoane care-l urmaresc cat si persoane pe care le urmareste
        //public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
        //public virtual ICollection<Follow> Following { get; set; } = new List<Follow>();

        //// un user poate sa faca parte din mai multe grupuri
        //public virtual ICollection<Member> GroupMemberships { get; set; } = new List<Member>();
    }
}
