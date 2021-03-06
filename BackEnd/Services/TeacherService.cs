﻿using BackEnd.Context;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Responses;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Ultilities;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using EmailService;
using BackEnd.Requests;
using Newtonsoft.Json;
using BackEnd.DAO;
using Nancy.Validation;
using BackEnd.Constant;

namespace BackEnd.Services
{
    public interface ITeacherService
    {
        public Task<IActionResult> GetTeacher();
        public Task<IActionResult> AddTeacher(TeacherRequest model);
        public Task<IActionResult> UpdateTeacher(List<TeacherRequest> model);
        public Task<IActionResult> DeleteTeacher(List<string> model);
    }
    public class TeacherService : ITeacherService
    {
        private readonly UserManager<AppUser> _userManager;
        private UserDbContext _userContext;
        private RoomDBContext _roomContext;
        private readonly ILogDAO _logDAO;
        private readonly IPrivateMessageDAO _privateMessageDAO;
        private readonly IAttendanceDAO _attendanceDAO;
        private readonly IEmailSender _emailSender;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;

        public TeacherService(
            UserDbContext usercontext,
            UserManager<AppUser> userManager,
            RoomDBContext roomcontext,
            ILogDAO logDAO,
            IAttendanceDAO attendanceDAO,
            IPrivateMessageDAO privateMessageDAO,
            IEmailSender emailSender,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor)
        {
            _userContext = usercontext;
            _userManager = userManager;
            _roomContext = roomcontext;
            _logDAO = logDAO;
            _attendanceDAO = attendanceDAO;
            _privateMessageDAO = privateMessageDAO;
            _emailSender = emailSender;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
        }

        public async Task<IActionResult> GetTeacher()
        {
            var listUserId = await _userManager.GetUsersInRoleAsync("teacher");
            var listTeacher = new List<TeacherInfoResponse>();
            if (listUserId != null)
            {
                foreach (var user in listUserId)
                {

                    listTeacher.Add(new TeacherInfoResponse
                    {
                        Id = user.Id,
                        Code = user.UserName,
                        Email = user.Email,
                        RealName = user.RealName,
                        Dob = String.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(user.DOB))
                    });
                }
            }

            return new OkObjectResult(listTeacher);
        }

        public async Task<IActionResult> AddTeacher(TeacherRequest model)
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
                var internalErr = new ObjectResult(new { type = 2, message = result.Errors.ToList()[0].Description })
                {
                    StatusCode = 500
                };
                return internalErr;
            }

            await _userManager.AddToRoleAsync(registerUser, "Teacher");

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(registerUser);
            var confirmationLink = urlHelper.Action("ConfirmEmail", "Users", new { token, email = registerUser.Email }, "https");
            var content = String.Format(EmailTemplate.HTML_CONTENT, model.Email, model.Code, "123@123a", confirmationLink);

            var message = new Message(new string[] { registerUser.Email }, "Your Account On DYSTANCE", content, null);
            await _emailSender.SendEmailAsync(message);

            return new OkObjectResult(new TeacherInfoResponse
            {
                Id = registerUser.Id,
                Code = registerUser.UserName,
                Email = registerUser.Email,
                RealName = registerUser.RealName,
                Dob = registerUser.DOB.ToString("yyyy-MM-dd"),
            });

        }

        public async Task<IActionResult> UpdateTeacher(List<TeacherRequest> teacherList)
        {
            var dict = new Dictionary<String, object>();
            var response = new List<TeacherInfoResponse>();
            var errors = new List<Error>();
            foreach (var req in teacherList)
            {
                var user = await _userManager.FindByIdAsync(req.Id);
                if (user == null)
                {
                    continue;
                }
                try
                {
                    if (await _userManager.IsInRoleAsync(user, "teacher"))
                    {
                        if (!(user.UserName == req.Code && user.RealName == req.RealName && user.Email == req.Email && DateTime.Compare(user.DOB, Convert.ToDateTime(req.Dob)) == 0))
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
                            user.DOB = Convert.ToDateTime(req.Dob);
                            user.Email = req.Email;



                            var resultUpdate = await _userManager.UpdateAsync(user);
                            if (resultUpdate.Succeeded)
                            {
                                response.Add(new TeacherInfoResponse
                                {
                                    Id = user.Id,
                                    Code = user.UserName,
                                    RealName = user.RealName,
                                    Email = user.Email,
                                    Dob = user.DOB.ToString("yyyy-MM-dd")
                                });
                            }
                            else
                            {

                                return new ObjectResult(new { type = 3, code = resultUpdate.Errors.ToList()[0].Code, message = resultUpdate.Errors.ToList()[0].Description })
                                {
                                    StatusCode = 500
                                };
                            }

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

        public async Task<IActionResult> DeleteTeacher(List<string> teacherIdList)
        {
            var dict = new Dictionary<String, object>();
            var response = new List<String>();
            var errors = new List<Error>();

            foreach (string id in teacherIdList)
            {
                var user = await _userManager.FindByIdAsync(id);
                var room = await _roomContext.RoomUserLink.FirstOrDefaultAsync(x => x.UserId == id);

                if (user == null)
                {
                    errors.Add(new Error
                    {
                        Type = 1,
                        Message = "This id " + id + " don't exist",
                    });
                    continue;
                }
                if (!await _userManager.IsInRoleAsync(user, "teacher"))
                {
                    errors.Add(new Error
                    {
                        Type = 2,
                        Message = "The user " + user.UserName + " is not a teacher",
                    });
                    continue;
                }
                if (room != null)
                {
                    errors.Add(new Error
                    {
                        Type = 3,
                        Message = "Teacher " + user.UserName + " is still linked to a classroom",
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
