using BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.Context
{
    public class Seed 
    {
        public static async Task SeedData(UserDbContext context, UserManager<AppUser> userManager)
        {
            if (!userManager.Users.Any())
            {
                var users = new List<AppUser>
                {
                    new AppUser{
                        DisplayName="Minh",
                        UserName="minh",
                        Email="minh@test.com"
                    },
                    new AppUser{
                        DisplayName="Dat",
                        UserName="dat",
                        Email="dat@test.com"
                    },
                    new AppUser{
                        DisplayName="Tu",
                        UserName="tu",
                        Email="tu@test.com"
                    },
                    new AppUser{
                        DisplayName="Hoang",
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
        }
    }
}
