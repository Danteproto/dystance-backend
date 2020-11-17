using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Requests;
using ExcelDataReader;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface ISemesterService
    {
        public Task<IActionResult> AddSemester(HttpRequest request);
        public Task<IActionResult> UpdateSemester(HttpRequest request);
        public Task<IActionResult> DeleteSemester(List<string> ids);
    }


    public class SemesterService : ISemesterService
    {
        private readonly UserManager<AppUser> _userManager;
        private UserDbContext _userContext;
        private RoomDBContext _roomContext;

        public SemesterService(UserManager<AppUser> userManager, UserDbContext userContext, RoomDBContext roomContext)
        {
            _userManager = userManager;
            _userContext = userContext;
            _roomContext = roomContext;
        }

        public async Task<IActionResult> AddSemester(HttpRequest request)
        {
            //doc file excel
            var appUsers = new List<AppUser>();
            var file = request.Form.Files[0];
            var semester = new Semester
            {
                Name = request.Form["name"],
                LastUpdate = DateTime.Now,
                File = file.FileName
            };
            SemesterDAO.Create(_roomContext, semester);
            semester = SemesterDAO.GetLast(_roomContext);
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

                            if (reader.Name == "Teachers") // "TEACHERS" SHEET
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
                                var room = new Room();
                                if(reader.GetValue(0) == null && reader.GetValue(1) == null)
                                {
                                    continue;
                                }
                                if (reader.GetValue(0).ToString() == "Subject")
                                {
                                    room.Subject = reader.GetValue(1).ToString();
                                    reader.Read();
                                }
                                if (reader.GetValue(0).ToString() == "Class")
                                {
                                    room.ClassName = reader.GetValue(1).ToString();
                                    reader.Read();
                                }
                                room.StartDate = DateTime.Now;
                                room.EndDate = DateTime.Now;
                                var result = RoomDAO.Create(_roomContext, room);
                                room = RoomDAO.GetLastRoom(_roomContext);
                                if (reader.GetValue(0).ToString() == "Teacher")
                                {
                                    var teacher = await _userManager.FindByNameAsync(reader.GetValue(1).ToString());
                                    room.CreatorId = teacher.Id;
                                    RoomDAO.UpdateRoom(_roomContext, room);
                                    var link = new RoomUserLink
                                    {
                                        UserId = teacher.Id,
                                        RoomId = room.RoomId,
                                    };
                                    result = RoomUserLinkDAO.Create(_roomContext, link);
                                    reader.Read();
                                }
                                if (reader.GetValue(0).ToString() == "Num of students")
                                {
                                    var count = Convert.ToInt32(reader.GetValue(1).ToString());
                                    for (int i = 0; i < count; i++)
                                    {
                                        reader.Read();
                                        var student = _userManager.FindByNameAsync(reader.GetValue(1).ToString()).Result;
                                        var link = new RoomUserLink
                                        {
                                            UserId = student.Id,
                                            RoomId = room.RoomId,
                                        };
                                        result = RoomUserLinkDAO.Create(_roomContext, link);
                                    }
                                }
                            }


                            if (reader.Name == "Schedules") // "SCHEDULES" SHEET
                            {
                                if (reader.GetValue(0).ToString() == "Date")
                                {
                                    continue;
                                }
                                var subject = reader.GetValue(3).ToString();
                                var className = reader.GetValue(4).ToString();
                                var room = RoomDAO.GetRoomByClassAndSubject(_roomContext, className, subject);
                                var schedule = new Timetable
                                {
                                    RoomId = room.RoomId,
                                    Date = Convert.ToDateTime(reader.GetValue(0).ToString()),
                                    StartTime = Convert.ToDateTime(reader.GetValue(1).ToString()).TimeOfDay,
                                    EndTime = Convert.ToDateTime(reader.GetValue(2).ToString()).TimeOfDay,
                                    SemesterId = semester.Id
                                };
                                var result = TimetableDAO.Create(_roomContext, schedule);
                            }

                        }
                    } while (reader.NextResult());
                }


                return new OkObjectResult(semester);
            }

        }

        public Task<IActionResult> DeleteSemester(List<string> ids)
        {
            throw new NotImplementedException();
        }

        public Task<IActionResult> UpdateSemester(HttpRequest request)
        {
            throw new NotImplementedException();
        }

    }
}