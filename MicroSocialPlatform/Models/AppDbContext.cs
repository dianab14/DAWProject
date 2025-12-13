using Microsoft.EntityFrameworkCore;
namespace MicroSocialPlatform.Models

{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
        public DbSet<ApplicationUser> Users { get; set; }
        //public DbSet<Post> Posts { get; set; }
        //public DbSet<Comment> Comments { get; set; }
        //public DbSet<Reaction> Reactions { get; set; }
        //public DbSet<Follow> Follows { get; set; }
        //public DbSet<Group> Groups { get; set; }
        //public DbSet<Member> Members { get; set; }
    }
}
