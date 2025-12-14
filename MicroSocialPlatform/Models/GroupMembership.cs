using System.ComponentModel.DataAnnotations.Schema;

namespace MicroSocialPlatform.Models
{
    public class GroupMembership
    {
        // tabelul asociativ pentru de relatia de join intre ApplicationUser si Group
        // un user poate fi membru in mai multe grupuri
        // iar un grup poate avea mai multi membri

        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        public string UserId { get; set; }
        public int GroupId { get; set; }
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        public virtual ApplicationUser? User { get; set; }
        public virtual Group? Group { get; set; }
    }
}
