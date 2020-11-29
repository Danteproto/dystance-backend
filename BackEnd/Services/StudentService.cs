﻿using BackEnd.Context;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Ultilities;
using Microsoft.AspNetCore.Hosting;
using BackEnd.DAO;

namespace BackEnd.Services
{
    public interface IStudentService
    {
        public Task<IActionResult> GetStudent();
        public Task<IActionResult> AddStudent(TeacherRequest model);
        public Task<IActionResult> UpdateStudent(List<TeacherRequest> model);
        public Task<IActionResult> DeleteStudent(List<string> model);
    }
    public class StudentService : IStudentService
    {
        private UserDbContext _context;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly RoomDBContext _roomContext;
        private readonly IWebHostEnvironment _env;
        private readonly UserManager<AppUser> _userManager;


        public StudentService(
            UserDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            RoomDBContext roomcontext,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _roomContext = roomcontext;
            _env = env;
        }


        public async Task<IActionResult> GetStudent()
        {
            var listUserId = await _userManager.Users.ToListAsync();

            var listStudent = new List<TeacherInfoResponse>();


            foreach (var user in listUserId)
            {
                if (await _userManager.IsInRoleAsync(user, "Student"))
                {
                    listStudent.Add(new TeacherInfoResponse
                    {
                        Id = user.Id,
                        Code = user.UserName,
                        Email = user.Email,
                        RealName = user.RealName,
                        Dob = String.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(user.DOB))
                    });

                }
            }

            return new OkObjectResult(listStudent);
        }


        public async Task<IActionResult> AddStudent(TeacherRequest model)
        {

            var appUser = await _userManager.FindByNameAsync(model.Code);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = 2, message = "Student code already exists" });
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
                DOB = model.Dob,
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
            await _userManager.AddToRoleAsync(registerUser, "Student");

            return new OkObjectResult(new TeacherInfoResponse
            {
                Id = registerUser.Id,
                Code = registerUser.UserName,
                Email = registerUser.Email,
                RealName = registerUser.RealName,
                Dob = registerUser.DOB
            });

        }

        public async Task<IActionResult> UpdateStudent(List<TeacherRequest> teacherList)
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
                    //Update thong tin user
                    if (await _userManager.IsInRoleAsync(user, "Student"))
                    {
                        //Update Profile

                        if (!(user.UserName == req.Code && user.RealName == req.RealName && user.Email == req.Email && user.DOB == req.Dob))
                        {
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
                                    Message = "Student Code " + req.Code + " already exists",
                                });
                                continue;
                            }

                            user.UserName = req.Code;
                            user.RealName = req.RealName;
                            user.DOB = req.Dob;
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
                                    Dob = user.DOB
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

        public async Task<IActionResult> DeleteStudent(List<string> teacherIdList)
        {
            int i = 0;
            foreach (string id in teacherIdList)
            {
                var user = await _userManager.FindByIdAsync(id);

                if (user != null && await _userManager.IsInRoleAsync(user, "Student"))
                {
                    await _userManager.DeleteAsync(user);
                    i++;
                }
            }

            if (i == 0)
            {
                return new OkObjectResult(new { response = "No Students Were Deleted " });
            }

            return new OkObjectResult(new { response = "Deleted Students Successfully" });
        }



    }
}