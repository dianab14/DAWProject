using System.ComponentModel.DataAnnotations;

namespace MicroSocialPlatform.Models
{
    public class Group
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "This group must have a name!")]
        [StringLength(50,ErrorMessage = "Max. 50 characters") ]
        public string Name { get; set; }

        [Required(ErrorMessage = "This group must have a description!")]
        [StringLength(300, ErrorMessage = "Max. 300 characters")]
        public string Description { get; set; }

        /// un group este creat de un user (moderator)
        public string ModeratorId { get; set; }
        // proprietatea de navigare catre ApplicationUser (adminul grupului)
        public virtual ApplicationUser? Moderator { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // relatia many-to-many dintre ApplicationUser si Group
        public virtual ICollection<GroupMembership> GroupMembers { get; set; } = new List<GroupMembership>();
        public virtual ICollection<GroupMessage> GroupMessages { get; set; } = new List<GroupMessage>();

    }
}
