using BackEnd.Models;
using BackEnd.Requests;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface ISemesterService
    {
        public Task<IActionResult> AddSemester(HttpRequest request);
    }


    public class SemesterService : ISemesterService
    {
        private readonly UserManager<AppUser> _userManager;

        public SemesterService(UserManager<AppUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> AddSemester(HttpRequest request)
        {
            //doc file excel
            var appUsers = new List<AppUser>();

            var file = request.Form.Files[0];
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read()) //Each row of the file
                        {
                            if (reader.Name == "Students") // "STUDENTS" SHEET
                            {
                                if (reader.GetValue(0).ToString() == "No")
                                {
                                    continue;
                                }

                                var user = new AppUser
                                {
                                    UserName = reader.GetValue(1).ToString(),
                                    RealName = reader.GetValue(2).ToString(),
                                    Email = reader.GetValue(3).ToString(),
                                    DOB = reader.GetValue(4).ToString()
                                };

                                //roleManager.AddUserToRole

                                //Create user
                                var result = await _userManager.CreateAsync(user, "123@123a");

                                if (result != IdentityResult.Success)
                                {
                                    appUsers.Add(user);
                                }
                            }

                            if (reader.Name == "Teachers") // "STUDENTS" SHEET
                            {
                                if (reader.GetValue(0).ToString() == "No")
                                {
                                    continue;
                                }

                                var user = new AppUser
                                {
                                    UserName = reader.GetValue(1).ToString(),
                                    RealName = reader.GetValue(2).ToString(),
                                    Email = reader.GetValue(3).ToString(),
                                    DOB = reader.GetValue(4).ToString()
                                };

                                //roleManager.AddUserToRole

                                //Create user
                                var result = await _userManager.CreateAsync(user, "123@123a");

                                if (result != IdentityResult.Success)
                                {
                                    appUsers.Add(user);
                                }
                            }


                            if (reader.Name == "Classes") // "CLASSES" SHEET
                            {

                            }


                            if (reader.Name == "Schedules") // "SCHEDULES" SHEET
                            {

                            }

                        }
                    } while (reader.NextResult());
                }


                return new OkObjectResult("");
            }

        }


    }
}