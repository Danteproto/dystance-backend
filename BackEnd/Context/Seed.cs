using BackEnd.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Context
{
    public class Seed
    {
        private readonly IWebHostEnvironment _env;

        public Seed(IWebHostEnvironment env)
        {
            _env = env;
        }
        public static async Task SeedData(UserDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            if (!userManager.Users.Any())
            {
                var users = new List<AppUser>
                {
                    new AppUser{
                        RealName="Minh",
                        UserName="minh",
                        Email="minh@test.com"
                    },
                    new AppUser{
                        RealName="Dat",
                        UserName="dat",
                        Email="dat@test.com"
                    },
                    new AppUser{
                        RealName="Tu",
                        UserName="tu",
                        Email="tu@test.com"
                    },
                    new AppUser{
                        RealName="Hoang",
                        UserName="hoang",
                        Email="hoang@test.com"
                    }
                };
                foreach (var user in users)
                {
                    await userManager.CreateAsync(user, "Pa$$w0rd");
                }

            }

            if (await userManager.FindByNameAsync("admin") == null)
            {
                await userManager.CreateAsync(new AppUser
                {
                    UserName = "admin",
                    Email = "admin@gmail.com",
                    RealName ="Admin"
                }, "Capstone@123");
                var u = await userManager.FindByNameAsync("admin");
                await userManager.AddToRoleAsync(u, "admin");
                var token = await userManager.GenerateEmailConfirmationTokenAsync(u);
                await userManager.ConfirmEmailAsync(u, token);
            }
            
            if (await userManager.FindByNameAsync("academicManager") == null)
            {
                await userManager.CreateAsync(new AppUser
                {
                    UserName = "academicManager",
                    Email = "academicManager@gmail.com",
                    RealName = "Academic Manager"
                }, "Capstone@123");

                var u = await userManager.FindByNameAsync("academicManager");
                await userManager.AddToRoleAsync(u, "academic management");
                var token = await userManager.GenerateEmailConfirmationTokenAsync(u);
                await userManager.ConfirmEmailAsync(u, token);
            }
            
            if (await userManager.FindByNameAsync("qualityAssurance") == null)
            {
                await userManager.CreateAsync(new AppUser
                {
                    UserName = "qualityAssurance",
                    Email = "qualityAssurance@gmail.com",
                    RealName = "Quality Assurance"
                }, "Capstone@123");

                var u = await userManager.FindByNameAsync("qualityAssurance");
                await userManager.AddToRoleAsync(u, "quality assurance");
                var token = await userManager.GenerateEmailConfirmationTokenAsync(u);
                await userManager.ConfirmEmailAsync(u, token);
            }

            if (!roleManager.Roles.Any())
            {
                var roles = new List<AppRole>
                {
                    new AppRole{
                        Name="admin"
                    },
                    new AppRole{
                        Name="academic management"
                    },
                    new AppRole{
                        Name="student"
                    },
                    new AppRole{
                        Name="teacher"
                    },
                    new AppRole{
                        Name="quality assurance"
                    }
                };
                foreach (var role in roles)
                {
                    await roleManager.CreateAsync(role);
                }
            }


            //upload default avatar to server
            string path = $"Files/Users/Images";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (Directory.Exists(path))
            {
                if (!Directory.Exists(path + "/default.png"))
                {
                    using (WebClient webClient = new WebClient())
                    {
                        byte[] data = webClient.DownloadData("https://huyhoanhotel.com/wp-content/uploads/2016/05/765-default-avatar.png");

                        using (MemoryStream mem = new MemoryStream(data))
                        {
                            using (var yourImage = Image.FromStream(mem))
                            {
                                yourImage.Save(Path.Combine(path, "default.png"), ImageFormat.Png);
                            }
                        }

                    }
                }
            }

            //upload default avatar to server
            string roomPath = $"Files/Images";
            if (!Directory.Exists(roomPath))
            {
                Directory.CreateDirectory(roomPath);
            }

            if (Directory.Exists(roomPath))
            {
                if (!Directory.Exists(roomPath + "/default.png"))
                {
                    using (WebClient webClient = new WebClient())
                    {
                        byte[] data = webClient.DownloadData("https://image.freepik.com/free-vector/empty-classroom-interior-school-college-class_107791-631.jpg");

                        using (MemoryStream mem = new MemoryStream(data))
                        {
                            using (var yourImage = Image.FromStream(mem))
                            {
                                yourImage.Save(Path.Combine(roomPath, "default.png"), ImageFormat.Png);
                            }
                        }

                    }
                }
            }

        }
    }
}
