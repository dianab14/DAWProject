using Elfie.Serialization;
using MicroSocialPlatform.Data;
using MicroSocialPlatform.Migrations;
using MicroSocialPlatform.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using static System.Net.Mime.MediaTypeNames;

namespace MicroSocialPlatform.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider serviceProvider)
        {
            using (var context = new ApplicationDbContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<ApplicationDbContext>>()))
            {
                // Verificam daca in baza de date exista cel putin un rol
                // insemnand ca a fost rulat codul
                // Acesta metoda trebuie sa se execute o singura data
                if (context.Roles.Any())
                {
                    return; // baza de date contine deja roluri
                }

                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea
                context.Roles.AddRange(

                new IdentityRole
                {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7210",
                    Name = "Admin",
                    NormalizedName = "Admin".ToUpper()
                },


                new IdentityRole
                {
                    Id = "2c5e174e-3b0e-446f-86af-483d56fd7211",
                    Name = "User",
                    NormalizedName = "User".ToUpper()
                }
                
                );

                // o noua instanta pe care o vom utiliza pentru crearea parolelor utilizatorilor
                // parolele sunt de tip hash
                var hasher = new PasswordHasher<ApplicationUser>();

                // creeam 1 admin si 4 useri normali
                context.Users.AddRange(
                new ApplicationUser
                {

                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb0",
                    // primary key
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    FirstName = "Admin", 
                    LastName = "One",
                    Description = "Delighted to be the first account registered on this platform!!❤️",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "Admin1!")
                },
                new ApplicationUser
                {

                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb1",
                    // primary key
                    UserName = "user1@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER1@TEST.COM",
                    Email = "user1@test.com",
                    FirstName = "User",
                    LastName = "One",
                    IsPrivate = true,
                    Description = "Not accepting requests",
                    NormalizedUserName = "USER1@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User1!")
                },
                new ApplicationUser
                {

                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb2",
                    // primary key
                    UserName = "user2@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER2@TEST.COM",
                    Email = "user2@test.com",
                    FirstName = "User",
                    LastName = "Two",
                    NormalizedUserName = "USER2@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User2!")
                },
                new ApplicationUser
                {

                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb3",
                    // primary key
                    UserName = "user3@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER3@TEST.COM",
                    Email = "user3@test.com",
                    FirstName = "User",
                    LastName = "Three",
                    IsPrivate = false,
                    Description = "I am a menace to society",
                    NormalizedUserName = "USER3@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User3!")
                },
                new ApplicationUser
                {

                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb4",
                    // primary key
                    UserName = "user4@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "USER4@TEST.COM",
                    Email = "user4@test.com",
                    FirstName = "User",
                    LastName = "Four",
                    NormalizedUserName = "USER4@TEST.COM",
                    PasswordHash = hasher.HashPassword(null, "User4!")
                }
                );

                // asocierea userilor cu rolurile corespunzatoare
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {

                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7210",


                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },

                new IdentityUserRole<string>

                {

                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",


                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },

                new IdentityUserRole<string>

                {

                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",


                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb2"
                },
                new IdentityUserRole<string>

                {

                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",


                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3"
                },
                new IdentityUserRole<string>

                {

                    RoleId = "2c5e174e-3b0e-446f-86af-483d56fd7211",


                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb4"
                }
                );

                // ===== GROUPS =====
                context.Groups.AddRange(
                    new Group
                    {
                        Id = 1,
                        Name = "ASP.NET Core Devs",
                        Description = "Discussions about ASP.NET Core, MVC, Identity & EF Core",
                        ModeratorId = "8e445865-a24d-4543-a6c6-9443d048cdb0" // admin
                    },
                    new Group
                    {
                        Id = 2,
                        Name = "Star Wars",
                        Description = "R2D2 beep boop",
                        ModeratorId = "8e445865-a24d-4543-a6c6-9443d048cdb1" // user1
                    },
                    new Group
                    {
                        Id = 3,
                        Name = "Uncle jokes",
                        Description = "Group for fathers",
                        ModeratorId = "8e445865-a24d-4543-a6c6-9443d048cdb1" // user1
                    },
                    new Group
                    {
                        Id = 4,
                        Name = "Student Life",
                        Description = "Projects, deadlines, exams and student survival tips",
                        ModeratorId = "8e445865-a24d-4543-a6c6-9443d048cdb2" // user2
                    }
                );

                context.Posts.AddRange(
                new Post
                {
                    Id = 1,
                    Content = "Just got into something new. Feels great 😌",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },
                new Post
                {
                    Id = 2,
                    Content = "Anyone else struggling with EF Core migrations or is it just me?",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },
                new Post
                {
                    Id = 3,
                    Content = "Private profiles are underrated. Peace > drama.",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },
                new Post
                {
                    Id = 4,
                    Content = "MicroSocialPlatform is finally coming together 🚀",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb0"
                },
                new Post
                {
                    Id = 5,
                    VideoPath = "0b671b6a-336c-4bd1-9b57-43a3a443ad55.mp4",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },
                new Post
                {
                    Id = 6,
                    ImagePath = "90362412-9bf8-4476-8ed5-1a5a43d326c8.jpg",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb1"
                },
                new Post
                {
                    Id = 7,
                    Content = "I want to be like Dr. Doofenshmirtz. He's a role model to me.",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb3"
                }
            );

                context.SaveChanges();
            }
        }
    }
}
