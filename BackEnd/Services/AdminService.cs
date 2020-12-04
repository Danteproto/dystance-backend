using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Responses;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IAdminService
    {
        public Task<IActionResult> AddAccount(HttpRequest request);
        public Task<IActionResult> GetAccounts();
        public Task<IActionResult> AddAccountAdmin(AdminRequest model);
        public Task<IActionResult> UpdateAccountAdmin(List<AdminRequest> model);
        public Task<IActionResult> DeleteManageAccounts(List<string> model);

    }
    public class AdminService : IAdminService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly UserDbContext _userContext;
        private readonly ILogDAO _logDAO;
        private readonly IPrivateMessageDAO _privateMessageDAO;
        private readonly IAttendanceDAO _attendanceDAO;

        public AdminService(UserManager<AppUser> userManager,
            UserDbContext usercontext,
            ILogDAO logDAO,
            IPrivateMessageDAO privateMessageDAO,
            IAttendanceDAO attendanceDAO)
        {
            _userManager = userManager;
            _userContext = usercontext;
            _logDAO = logDAO;
            _privateMessageDAO = privateMessageDAO;
            _attendanceDAO = attendanceDAO;
        }


        public async Task<IActionResult> AddAccount(HttpRequest request)
        {
            var response = new List<AdminInfoResponse>();
            var dict = new Dictionary<String, object>();
            var errors = new List<Error>();
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

                                if (await _userManager.FindByEmailAsync(reader.GetValue(3).ToString()) != null)
                                {
                                    errors.Add(new Error
                                    {
                                        Type = 1,
                                        Message = "Email " + reader.GetValue(3).ToString() + " already exists",
                                    });
                                    continue;
                                }
                                if (await _userManager.FindByNameAsync(reader.GetValue(1).ToString()) != null)
                                {
                                    errors.Add(new Error
                                    {
                                        Type = 2,
                                        Message = "Employee Code " + reader.GetValue(1).ToString() + " already exists",
                                    });
                                    continue;
                                }

                                var user = new AppUser
                                {
                                    UserName = reader.GetValue(1).ToString(),
                                    RealName = reader.GetValue(2).ToString(),
                                    Email = reader.GetValue(3).ToString(),
                                    DOB = Convert.ToDateTime(reader.GetValue(4).ToString()),
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
                                response.Add(new AdminInfoResponse
                                {
                                    Id = user.Id,
                                    Code = user.UserName,
                                    RealName = user.RealName,
                                    Email = user.Email,
                                    Dob = user.DOB.ToString("yyyy-MM-dd"),
                                    Role = "quality assurance"
                                });
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
                                    DOB = Convert.ToDateTime(reader.GetValue(4).ToString()),
                                    Avatar = "default.png"
                                };

                                //Create user
                                var result = await _userManager.CreateAsync(user, "123@123a");

                                //roleManager.AddUserToRole
                                await _userManager.AddToRoleAsync(user, "academic management");

                                //debug
                                response.Add(new AdminInfoResponse
                                {
                                    Id = user.Id,
                                    Code = user.UserName,
                                    RealName = user.RealName,
                                    Email = user.Email,
                                    Dob = user.DOB.ToString("yyyy-MM-dd"),
                                    Role = "academic management"
                                });
                            }

                        }

                    } while (reader.NextResult());
                }


            }

            dict.Add("success", response);
            dict.Add("failed", errors);

            return new OkObjectResult(dict);
        }

        public async Task<IActionResult> GetAccounts()
        {
            var listUserId = await _userManager.Users.ToListAsync();

            var listAdmin = new List<AdminInfoResponse>();

            foreach (var user in listUserId)
            {
                if (await _userManager.IsInRoleAsync(user, "quality assurance"))
                {
                    listAdmin.Add(new AdminInfoResponse
                    {
                        Id = user.Id,
                        Code = user.UserName,
                        Email = user.Email,
                        RealName = user.RealName,
                        Dob = Convert.ToDateTime(user.DOB).ToString("yyyy-MM-dd"),
                        Role = "quality assurance"
                    });

                }
                if (await _userManager.IsInRoleAsync(user, "academic management"))
                {
                    listAdmin.Add(new AdminInfoResponse
                    {
                        Id = user.Id,
                        Code = user.UserName,
                        Email = user.Email,
                        RealName = user.RealName,
                        Dob = Convert.ToDateTime(user.DOB).ToString("yyyy-MM-dd"),
                        Role = "academic management"
                    });

                }
            }

            return new OkObjectResult(listAdmin);
        }

        public async Task<IActionResult> AddAccountAdmin(AdminRequest model)
        {

            var appUser = await _userManager.FindByNameAsync(model.Code);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = 2, message = "Employee code already exists" });
            }

            appUser = await _userManager.FindByEmailAsync(model.Email);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = 1, message = "Email already exists" });
            }

            var registerUser = new AppUser
            {
                Email = model.Email,
                UserName = model.Code,
                RealName = model.RealName,
                DOB = Convert.ToDateTime(model.Dob),
                Avatar = "default.png"
            };

            var result = await _userManager.CreateAsync(registerUser, "123@123a");
            if (!result.Succeeded)
            {
                var internalErr = new ObjectResult(new { type = 2, error = result.Errors.ToList()[0].Description })
                {
                    StatusCode = 500
                };
                return internalErr;
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(registerUser);
            await _userManager.ConfirmEmailAsync(registerUser, token);


            await _userManager.AddToRoleAsync(registerUser, model.Role);


            return new OkObjectResult(new AdminInfoResponse
            {
                Id = registerUser.Id,
                Code = registerUser.UserName,
                Email = registerUser.Email,
                RealName = registerUser.RealName,
                Dob = registerUser.DOB.ToString("yyyy-MM-dd"),
                Role = model.Role,
            });

        }

        public async Task<IActionResult> UpdateAccountAdmin(List<AdminRequest> adminList)
        {
            var dict = new Dictionary<String, object>();
            var response = new List<AdminInfoResponse>();
            var errors = new List<Error>();

            foreach (var req in adminList)
            {
                var user = await _userManager.FindByIdAsync(req.Id);
                if (user == null)
                {
                    continue;
                }
                try
                {
                    if (await _userManager.IsInRoleAsync(user, "academic management") || await _userManager.IsInRoleAsync(user, "quality assurance"))
                    {
                        if (!(user.UserName == req.Code && user.RealName == req.RealName && user.Email == req.Email && DateTime.Compare(user.DOB, Convert.ToDateTime(req.Dob))==0 && await _userManager.IsInRoleAsync(user, req.Role)))
                        {
                            //Update Profile
                            if (await _userManager.FindByEmailAsync(req.Email) != null && user.Email != req.Email)
                            {
                                errors.Add(new Error
                                {
                                    Type = 1,
                                    Message = "Email " + req.Email + " already exists",
                                });
                                continue;
                            }
                            if (await _userManager.FindByNameAsync(req.Code) != null && user.UserName != req.Code)
                            {
                                errors.Add(new Error
                                {
                                    Type = 2,
                                    Message = "Employee Code " + req.Code + " already exists",
                                });
                                continue;
                            }

                            user.UserName = req.Code;
                            user.RealName = req.RealName;
                            user.Email = req.Email;
                            user.DOB = Convert.ToDateTime(req.Dob);
                            if (!await _userManager.IsInRoleAsync(user, req.Role))
                            {
                                await _userManager.RemoveFromRoleAsync(user, (await _userManager.GetRolesAsync(user))[0]);
                                await _userManager.AddToRoleAsync(user, req.Role);
                            }
                        }


                        var resultUpdate = await _userManager.UpdateAsync(user);
                        if (resultUpdate.Succeeded)
                        {
                            response.Add(new AdminInfoResponse
                            {
                                Id = user.Id,
                                Code = user.UserName,
                                RealName = user.RealName,
                                Email = user.Email,
                                Dob = user.DOB.ToString("yyyy-MM-dd"),
                                Role = req.Role
                            });
                        }
                        else
                        {

                            return new ObjectResult(new { type = 3, code = resultUpdate.Errors.ToList()[0].Code, description = resultUpdate.Errors.ToList()[0].Description })
                            {
                                StatusCode = 500
                            };
                        }
                    }
                }
                catch (Exception ex)
                {
                    return new ObjectResult(new { type = 4, message = ex.Message })
                    {
                        StatusCode = 500
                    };
                }

            }
            dict.Add("success", response);
            dict.Add("failed", errors);

            return new OkObjectResult(dict);
        }

        public async Task<IActionResult> DeleteManageAccounts(List<string> model)
        {
            var dict = new Dictionary<String, object>();
            var response = new List<String>();
            var errors = new List<Error>();

            foreach (string id in model)
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user == null)
                {
                    errors.Add(new Error
                    {
                        Type = 1,
                        Message = "This id " + id + " don't exist",
                    });
                    continue;
                }
                if (!(await _userManager.IsInRoleAsync(user, "quality assurance") || await _userManager.IsInRoleAsync(user, "academic management")))
                {
                    errors.Add(new Error
                    {
                        Type = 2,
                        Message = "The user " + user.UserName + " is not a quality assurance or academic management",
                    });
                    continue;
                }
                
                var listLog = await (from logs in _userContext.UserLog
                                     where logs.UserId.Contains(id)
                                     select logs).ToListAsync();

                await _logDAO.DeleteLogs(listLog);

                var listPrivateMessages = await (from messages in _userContext.PrivateMessages
                                                 where messages.SenderId.Contains(id) || messages.ReceiverId.Contains(id)
                                                 select messages).ToListAsync();

                await _privateMessageDAO.DeletePrivateMessages(listPrivateMessages);

                var listAttendances = await (from attendances in _userContext.AttendanceReports
                                             where attendances.UserId.Contains(id)
                                             select attendances).ToListAsync();

                await _attendanceDAO.DeleteAttendance(listAttendances);

                await _userManager.DeleteAsync(user);

                response.Add(id);
            }



            dict.Add("success", response);
            dict.Add("failed", errors);

            return new OkObjectResult(dict);
        }
    }
}
