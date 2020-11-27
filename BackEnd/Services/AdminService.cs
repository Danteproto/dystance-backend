using BackEnd.Models;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IAdminService
    {
        public Task<IActionResult> AddAccount(HttpRequest request);

    }
    public class AdminService : IAdminService
    {
        private readonly UserManager<AppUser> _userManager;

        public AdminService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<IActionResult> AddAccount(HttpRequest request)
        {
            var appUsers = new List<AppUser>();
            var file = request.Form.Files[0];
            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read()) //Each row of the file
                        {
                            if (reader.Name == "Quality Assurance") // "Quality Assurance" SHEET
                            {
                                if (reader.GetValue(0).ToString() == "No")
                                {
                                    continue;
                                }

                                if (await _userManager.FindByNameAsync(reader.GetValue(1).ToString()) != null ||
                                   await _userManager.FindByEmailAsync(reader.GetValue(3).ToString()) != null)
                                {
                                    continue;
                                }

                                var user = new AppUser
                                {
                                    UserName = reader.GetValue(1).ToString(),
                                    RealName = reader.GetValue(2).ToString(),
                                    Email = reader.GetValue(3).ToString(),
                                    DOB = reader.GetValue(4).ToString(),
                                    Avatar = "default.png"
                                };


                                //Create user
                                var result = await _userManager.CreateAsync(user, "123@123a");

                                //Confirm email
                                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                await _userManager.ConfirmEmailAsync(user, token);

                                //roleManager.AddUserToRole
                                await _userManager.AddToRoleAsync(user, "quality assurance");

                                //debug
                                if (result != IdentityResult.Success)
                                {
                                    appUsers.Add(user);
                                }
                            }

                            if (reader.Name == "Academic Management") // "Academic Management" SHEET
                            {
                                if (reader.GetValue(0).ToString() == "No")
                                {
                                    continue;
                                }

                                if (await _userManager.FindByNameAsync(reader.GetValue(1).ToString()) != null ||
                                    await _userManager.FindByEmailAsync(reader.GetValue(3).ToString()) != null)
                                {
                                    continue;
                                }

                                var user = new AppUser
                                {
                                    UserName = reader.GetValue(1).ToString(),
                                    RealName = reader.GetValue(2).ToString(),
                                    Email = reader.GetValue(3).ToString(),
                                    DOB = reader.GetValue(4).ToString(),
                                    Avatar = "default.png"
                                };

                                //Create user
                                var result = await _userManager.CreateAsync(user, "123@123a");

                                //roleManager.AddUserToRole
                                await _userManager.AddToRoleAsync(user, "academic management");

                                //debug
                                if (result != IdentityResult.Success)
                                {
                                    appUsers.Add(user);
                                }
                            }

                        }

                    } while (reader.NextResult());
                }


            }
            return new OkObjectResult("Successful");

        }

    }
}
