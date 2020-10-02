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
        public static async Task SeedData(UserDbContext context, UserManager<AppUser> userManager)
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

            context.SaveChanges();

            //upload default avatar to server
            string path = $"Files/Users/Images";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }

            if (Directory.Exists(path))
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
    }
}
