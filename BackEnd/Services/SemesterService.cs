﻿using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Responses;
using ExcelDataReader;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface ISemesterService
    {
        public Task<IActionResult> GetSemesters();
        public Task<IActionResult> AddSemester(HttpRequest request);
        public Task<IActionResult> UpdateSemester(HttpRequest request);
        public Task<IActionResult> DeleteSemester(List<string> ids);
        public Task<IActionResult> GetSemesterSchedule(int id);
        public Task<IActionResult> AddSchedule(int semesterId, ScheduleRequest model);
        public Task<IActionResult> UpdateSchedule(int semesterId, List<ScheduleRequest> model);
        public Task<IActionResult> DeleteSchedule(List<string> models);
        public Task<IActionResult> GetSemesterClass(int id);
        public Task<IActionResult> AddClass(int semesterId, ClassRequest model);
        public Task<IActionResult> UpdateClass(int semesterId, List<ClassRequest> models);
        public Task<IActionResult> DeleteClass(List<string> models);
    }


    public class SemesterService : ISemesterService
    {
        private readonly UserManager<AppUser> _userManager;
        private UserDbContext _userContext;
        private RoomDBContext _roomContext;
        private readonly IWebHostEnvironment _env;
        private readonly IAttendanceDAO _attendanceDAO;
        private readonly ILogDAO _logDAO;


        public SemesterService(UserManager<AppUser> userManager
            , UserDbContext userContext
            , RoomDBContext roomContext
            , IWebHostEnvironment env
            , IAttendanceDAO attendanceDAO
            , ILogDAO logDao)
        {
            _userManager = userManager;
            _userContext = userContext;
            _roomContext = roomContext;
            _env = env;
            _attendanceDAO = attendanceDAO;
            _logDAO = logDao;
        }

        public async Task<IActionResult> AddSemester(HttpRequest request)
        {
            //doc file excel
            var appUsers = new List<AppUser>();
            var file = request.Form.Files[0];
            var semester = new Semester
            {
                Name = request.Form["name"],
                LastUpdated = DateTime.Now,
                File = file.FileName
            };
            var fail = new List<object>();
            await SemesterDAO.Create(_roomContext, semester);
            semester = SemesterDAO.GetLast(_roomContext);
            System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {

                        do
                        {
                            while (reader.Read()) //Each row of the file
                            {

                                if (reader.Name == "Classes") // "CLASSES" SHEET
                                {
                                    var room = new Room();
                                    if (reader.GetValue(0) == null)
                                    {
                                        continue;
                                    }
                                    if (reader.GetValue(0).ToString() == "Subject")
                                    {
                                        room.Subject = reader.GetValue(1).ToString();
                                        reader.Read();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    if (reader.GetValue(0).ToString() == "Class")
                                    {
                                        room.ClassName = reader.GetValue(1).ToString();
                                        reader.Read();
                                    }
                                    else
                                    {
                                        continue;
                                    }
                                    var extRoom = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, room.ClassName, room.Subject, semester.Id);
                                    if (extRoom != null)
                                    {
                                        fail.Add(new { message = $"Class {room.Subject}-{room.ClassName} already exist" });
                                        continue;
                                    }
                                    room.StartDate = DateTime.Now;
                                    room.EndDate = DateTime.Now;
                                    room.SemesterId = semester.Id;
                                    var result = RoomDAO.Create(_roomContext, room);
                                    room = RoomDAO.GetLastRoom(_roomContext);
                                    room.Image = $"api/rooms/getImage?roomId={room.RoomId}&imgName=default.png";
                                    RoomDAO.UpdateRoom(_roomContext, room);
                                    if (reader.GetValue(0).ToString() == "Teacher")
                                    {
                                        var teacher = await _userManager.FindByNameAsync(reader.GetValue(1).ToString());
                                        if (teacher == null)
                                        {
                                            fail.Add(new { message = $"Class {room.Subject}-{room.ClassName} error: Teacher {reader.GetValue(1)} not exist " });
                                            await RoomService.DeleteRoom(_roomContext, room.RoomId, _env);
                                            continue;
                                        }
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
                                        var links = new List<RoomUserLink>();
                                        var flag = true;
                                        for (int i = 0; i < count; i++)
                                        {
                                            reader.Read();
                                            var student = _userManager.FindByNameAsync(reader.GetValue(1).ToString()).Result;
                                            if (student == null)
                                            {
                                                fail.Add(new { message = $"Class {room.Subject}-{room.ClassName} error: Student {reader.GetValue(1)} not exist " });
                                                await RoomService.DeleteRoom(_roomContext, room.RoomId, _env);
                                                flag = false;
                                                break;
                                            }
                                            if (!links.Any(item => item.UserId == student.Id))
                                            {
                                                links.Add(new RoomUserLink
                                                {
                                                    UserId = student.Id,
                                                    RoomId = room.RoomId,
                                                });
                                            }
                                            else
                                            {
                                                fail.Add(new { message = $"Class {room.Subject}-{room.ClassName} error: Student {reader.GetValue(1)} duplicate " });
                                            }

                                        }
                                        if (flag)
                                        {
                                            result = RoomUserLinkDAO.Create(_roomContext, links);
                                        }
                                    }
                                }


                                else if (reader.Name == "Schedules") // "SCHEDULES" SHEET
                                {
                                    if (reader.GetValue(0).ToString() == "Date")
                                    {
                                        continue;
                                    }
                                    var subject = reader.GetValue(3).ToString();
                                    var className = reader.GetValue(4).ToString();
                                    var room = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, className, subject, semester.Id);
                                    if (room == null)
                                    {
                                        fail.Add(new { message = $"Schedule for Class {subject}-{className} at date {reader.GetValue(0)} error: Class doesn't exist " });
                                        continue;
                                    }
                                    var schedule = new Timetable
                                    {
                                        RoomId = room.RoomId,
                                        Date = Convert.ToDateTime(reader.GetValue(0).ToString()),
                                        StartTime = Convert.ToDateTime(reader.GetValue(1).ToString()).TimeOfDay,
                                        EndTime = Convert.ToDateTime(reader.GetValue(2).ToString()).TimeOfDay,
                                    };
                                    var userIds = RoomUserLinkDAO.GetRoomLink(_roomContext, schedule.RoomId)
                                    .Select(item => item.UserId);
                                    var flag = false;
                                    foreach (var id in userIds)
                                    {
                                        var roomsList = RoomDAO.GetRoomsByUserId(_roomContext, id);
                                        foreach (var r in roomsList)
                                        {
                                            var rSchedules = TimetableDAO.GetByRoomId(_roomContext, r.RoomId);
                                            if(rSchedules.Count == 0)
                                            {
                                                continue;
                                            }
                                            if (rSchedules.Any(item => (item.Date == schedule.Date) &&
                                             ((item.StartTime <= schedule.EndTime && schedule.EndTime <= item.EndTime) ||
                                             (item.EndTime >= schedule.StartTime && schedule.StartTime >= item.StartTime) ||
                                             (item.StartTime > schedule.StartTime && item.EndTime < schedule.EndTime))))
                                            {
                                                flag = true;
                                            }
                                        }
                                    }
                                    if (!flag)
                                    {
                                        var result = await TimetableDAO.Create(_roomContext, schedule);
                                        schedule = TimetableDAO.GetLast(_roomContext);
                                        var attendances = new List<AttendanceReports>();
                                        userIds = userIds.Where(item => item != room.CreatorId).ToList();
                                        if (schedule != null)
                                        {
                                            foreach (var userId in userIds)
                                            {
                                                attendances.Add(new AttendanceReports
                                                {
                                                    UserId = userId,
                                                    Status = "future",
                                                    TimeTableId = schedule.Id,
                                                });
                                            }
                                            await _attendanceDAO.CreateAttendance(attendances);
                                        }
                                    }
                                    else{
                                        fail.Add(new { message = $"Schedule for Class {subject}-{className} at date {schedule.Date} error: User schedule overlapping" });
                                    }
                                }
                                else
                                {
                                    throw new Exception("Semester import error: Wrong file format");
                                }

                            }
                        } while (reader.NextResult());
                    }

                }

                return new OkObjectResult(new
                {
                    success = semester,
                    failed = fail
                });
            }
            catch (Exception e)
            {
                await DeleteSemester(new List<string>() { semester.Id.ToString() });
                return new BadRequestObjectResult(new { message = e.Message });
            }

        }

        public async Task<IActionResult> DeleteSemester(List<string> ids)
        {
            foreach (var id in ids)
            {
                var semester = SemesterDAO.GetById(_roomContext, Convert.ToInt32(id));
                if (semester != null)
                {
                    var rooms = RoomDAO.GetRoomBySemester(_roomContext, semester.Id);
                    foreach (var room in rooms)
                    {
                        var roomUserLinks = RoomUserLinkDAO.GetRoomLink(_roomContext, room.RoomId);
                        var roomChats = RoomChatDAO.GetChatByRoomId(_roomContext, room.RoomId);
                        var roomTimetables = TimetableDAO.GetByRoomId(_roomContext, room.RoomId);
                        var groups = RoomDAO.GetGroupByRoom(_roomContext, room.RoomId);
                        var log = _userContext.UserLog.Where(item => item.RoomId.Contains(room.RoomId.ToString())).ToList();
                        foreach (var schedule in roomTimetables)
                        {
                            var attendances = _attendanceDAO.GetAttendanceBySchedule(schedule.Id);
                            await _attendanceDAO.DeleteAttendance(attendances);
                        }
                        await _logDAO.DeleteLogs(log);
                        await RoomChatDAO.DeleteRoomChat(_roomContext, roomChats);
                        await RoomUserLinkDAO.Delete(_roomContext, roomUserLinks);
                        await TimetableDAO.DeleteTimeTable(_roomContext, roomTimetables);
                        foreach (var group in groups)
                        {
                            await RoomService.DeleteRoom(_roomContext, group.RoomId, _env);
                        }

                        var path = Path.Combine(_env.ContentRootPath, $"Files/{room.RoomId}");
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path, true);
                        }

                        await RoomDAO.Delete(_roomContext, room);
                    }
                    var result = SemesterDAO.Delete(_roomContext, semester);
                }
            }
            return new OkObjectResult(new { message = "delete success" });
        }

        public async Task<IActionResult> UpdateSemester(HttpRequest request)
        {
            var semester = SemesterDAO.GetById(_roomContext, Convert.ToInt32(request.Form["id"]));
            semester.Name = request.Form["name"];
            semester.LastUpdated = DateTime.Now;
            var fail = new List<object>();
            if (request.Form.Files.Any())
            {
                var appUsers = new List<AppUser>();
                var file = request.Form.Files[0];
                semester.File = file.FileName;
                System.Text.Encoding.RegisterProvider(System.Text.CodePagesEncodingProvider.Instance);
                using (var stream = file.OpenReadStream())
                {
                    using (var reader = ExcelReaderFactory.CreateReader(stream))
                    {
                        try
                        {
                            do
                            {
                                while (reader.Read()) //Each row of the file
                                {
                                    if (reader.Name == "Classes") // "CLASSES" SHEET
                                    {
                                        var room = new Room();
                                        if (reader.GetValue(0) == null)
                                        {
                                            continue;
                                        }
                                        if (reader.GetValue(0).ToString() == "Subject")
                                        {
                                            room.Subject = reader.GetValue(1).ToString();
                                            reader.Read();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        if (reader.GetValue(0).ToString() == "Class")
                                        {
                                            room.ClassName = reader.GetValue(1).ToString();
                                            reader.Read();
                                        }
                                        else
                                        {
                                            continue;
                                        }
                                        var extRoom = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, room.ClassName, room.Subject, semester.Id);
                                        if (extRoom == null)
                                        {
                                            room.StartDate = DateTime.Now;
                                            room.EndDate = DateTime.Now;
                                            room.SemesterId = semester.Id;
                                            var result = RoomDAO.Create(_roomContext, room);
                                            room = RoomDAO.GetLastRoom(_roomContext);
                                        }
                                        else
                                        {
                                            room = extRoom;
                                        }
                                        var links = RoomUserLinkDAO.GetRoomLink(_roomContext, room.RoomId);
                                        if (reader.GetValue(0).ToString() == "Teacher")
                                        {
                                            var teacher = await _userManager.FindByNameAsync(reader.GetValue(1).ToString());
                                            if (teacher == null)
                                            {
                                                fail.Add(new { message = $"Class {room.Subject}-{room.ClassName} error: Teacher {reader.GetValue(1)} not exist " });
                                            }
                                            else if (room.CreatorId != teacher.Id)
                                            {
                                                var delLink = RoomUserLinkDAO.GetRoomUserLink(_roomContext, room.RoomId, room.CreatorId);
                                                await RoomUserLinkDAO.Delete(_roomContext, delLink);
                                                room.CreatorId = teacher.Id;
                                                RoomDAO.UpdateRoom(_roomContext, room);
                                                var link = new RoomUserLink
                                                {
                                                    UserId = teacher.Id,
                                                    RoomId = room.RoomId,
                                                };
                                                await RoomUserLinkDAO.Create(_roomContext, link);
                                            }

                                            reader.Read();
                                        }
                                        if (reader.GetValue(0).ToString() == "Num of students")
                                        {
                                            var count = Convert.ToInt32(reader.GetValue(1).ToString());
                                            var listUserIds = new List<string>();
                                            for (int i = 0; i < count; i++)
                                            {
                                                reader.Read();
                                                var student = _userManager.FindByNameAsync(reader.GetValue(1).ToString()).Result;
                                                if (student == null)
                                                {
                                                    fail.Add(new { message = $"Class {room.Subject}-{room.ClassName} error: Student {reader.GetValue(1)} not exist " });
                                                    continue;
                                                }
                                                if (!listUserIds.Contains(student.Id))
                                                {
                                                    listUserIds.Add(student.Id);
                                                }
                                                else
                                                {
                                                    fail.Add(new { message = $"Class {room.Subject}-{room.ClassName} error: Student {reader.GetValue(1)} duplicate " });
                                                }
                                            }
                                            listUserIds.Add(room.CreatorId);
                                            var extLinkUserIds = RoomUserLinkDAO.GetRoomLink(_roomContext, room.RoomId)
                                                                .Select(x => x.UserId).ToList();
                                            var deleteUserIds = extLinkUserIds.Except(listUserIds).ToList();
                                            var addUserIds = listUserIds.Except(extLinkUserIds).ToList();

                                            var deleteLinks = deleteUserIds.Select(item => RoomUserLinkDAO.GetRoomUserLink(_roomContext, room.RoomId, item)).ToList();

                                            var addLinks = addUserIds.Select(item => new RoomUserLink
                                            {
                                                UserId = item,
                                                RoomId = room.RoomId
                                            }).ToList();

                                            await RoomUserLinkDAO.Create(_roomContext, addLinks);
                                            await RoomUserLinkDAO.Delete(_roomContext, deleteLinks);
                                        }
                                    }


                                    else if (reader.Name == "Schedules") // "SCHEDULES" SHEET
                                    {
                                        if (reader.GetValue(0).ToString() == "Date")
                                        {
                                            continue;
                                        }
                                        var subject = reader.GetValue(3).ToString();
                                        var className = reader.GetValue(4).ToString();
                                        var room = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, className, subject, semester.Id);
                                        if (room == null)
                                        {
                                            fail.Add(new { message = $"Schedule for Class {subject}-{className} at date {reader.GetValue(0)} error: Class doesn't exist " });
                                            continue;
                                        }
                                        var extSchedules = TimetableDAO.GetByRoomId(_roomContext, room.RoomId);
                                        var schedule = new Timetable
                                        {
                                            RoomId = room.RoomId,
                                            Date = Convert.ToDateTime(reader.GetValue(0).ToString()),
                                            StartTime = Convert.ToDateTime(reader.GetValue(1).ToString()).TimeOfDay,
                                            EndTime = Convert.ToDateTime(reader.GetValue(2).ToString()).TimeOfDay,
                                        };
                                        if (extSchedules.Any(x => x.RoomId == schedule.RoomId &&
                                                             x.Date == schedule.Date &&
                                                             x.StartTime == schedule.StartTime &&
                                                             x.EndTime == schedule.EndTime))
                                        {
                                            continue;
                                        }
                                        var userIds = RoomUserLinkDAO.GetRoomLink(_roomContext, schedule.RoomId)
                                        .Select(item => item.UserId);
                                        var flag = false;
                                        foreach (var id in userIds)
                                        {
                                            var roomsList = RoomDAO.GetRoomsByUserId(_roomContext, id);
                                            foreach (var r in roomsList)
                                            {
                                                var rSchedules = TimetableDAO.GetByRoomId(_roomContext, r.RoomId);
                                                if (rSchedules.Any(item => (item.Date == schedule.Date) &&
                                                 ((item.StartTime <= schedule.EndTime && schedule.EndTime <= item.EndTime) ||
                                                 (item.EndTime >= schedule.StartTime && schedule.StartTime >= item.StartTime) ||
                                                 (item.StartTime > schedule.StartTime && item.EndTime < schedule.EndTime))))
                                                {
                                                    var user = await _userManager.FindByIdAsync(id);
                                                    fail.Add(new { message = $"Schedule for Class {subject}-{className} at date {schedule.Date} error: User {user.UserName} schedule overlapping" });
                                                    flag = true;
                                                }
                                            }
                                        }
                                        if (!flag)
                                        {
                                            var result = TimetableDAO.Create(_roomContext, schedule);
                                            schedule = TimetableDAO.GetLast(_roomContext);
                                            var attendances = new List<AttendanceReports>();
                                            userIds = userIds.Where(item => item != room.CreatorId).ToList();
                                            foreach (var userId in userIds)
                                            {
                                                attendances.Add(new AttendanceReports
                                                {
                                                    UserId = userId,
                                                    Status = "future",
                                                    TimeTableId = schedule.Id,
                                                });
                                            }
                                            await _attendanceDAO.CreateAttendance(attendances);
                                        }


                                    }
                                    else
                                    {
                                        throw new Exception("Semester import error: Wrong file format");
                                    }

                                }
                            } while (reader.NextResult());
                        }
                        catch (Exception e)
                        {
                            return new BadRequestObjectResult(new { message = e.Message });
                        }
                    }

                }
            }
            var finalResult = SemesterDAO.Update(_roomContext, semester);
            return new OkObjectResult(new
            {
                success = semester,
                failed = fail
            });

        }

        public async Task<IActionResult> GetSemesterSchedule(int id)
        {
            var rooms = RoomDAO.GetRoomBySemester(_roomContext, id);
            List<ScheduleResponse> results = new List<ScheduleResponse>();
            foreach (var room in rooms)
            {
                var schedules = TimetableDAO.GetByRoomId(_roomContext, room.RoomId);
                foreach (var schedule in schedules)
                {
                    results.Add(new ScheduleResponse
                    {
                        id = schedule.Id.ToString(),
                        date = schedule.Date.ToString("yyyy-MM-dd"),
                        startTime = schedule.StartTime.ToString(),
                        endTime = schedule.EndTime.ToString(),
                        subject = room.Subject,
                        @class = room.ClassName
                    });
                }
            }
            return new OkObjectResult(results);
        }

        public async Task<IActionResult> AddSchedule(int semesterId, ScheduleRequest model)
        {
            var room = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, model.@class, model.subject, semesterId);
            if (room != null)
            {
                var schedule = new Timetable
                {
                    RoomId = room.RoomId,
                    Date = Convert.ToDateTime(model.date),
                    StartTime = Convert.ToDateTime(model.startTime).TimeOfDay,
                    EndTime = Convert.ToDateTime(model.endTime).TimeOfDay,
                };
                var extSchedules = TimetableDAO.GetByRoomId(_roomContext, room.RoomId);
                if (extSchedules.Any(item => (item.Date == schedule.Date) &&
                     ((item.StartTime <= schedule.EndTime && schedule.EndTime <= item.EndTime) ||
                     (item.EndTime >= schedule.StartTime && schedule.StartTime >= item.StartTime) ||
                     (item.StartTime > schedule.StartTime && item.EndTime < schedule.EndTime))))
                {
                    return new BadRequestObjectResult(new { message = $"Add schedule for {room.Subject}-{room.ClassName} at date {schedule.Date} {schedule.StartTime} error: Cannot add overlapping schedule" });
                }
                var userIds = RoomUserLinkDAO.GetRoomLink(_roomContext, schedule.RoomId)
                .Select(item => item.UserId);
                foreach (var id in userIds)
                {
                    var user = await _userManager.FindByIdAsync(id);
                    var roomsList = RoomDAO.GetRoomsByUserId(_roomContext, id);
                    foreach (var r in roomsList)
                    {
                        var rSchedules = TimetableDAO.GetByRoomId(_roomContext, r.RoomId);
                        if (rSchedules.Any(item => (item.Date == schedule.Date) &&
                         ((item.StartTime <= schedule.EndTime && schedule.EndTime <= item.EndTime) ||
                         (item.EndTime >= schedule.StartTime && schedule.StartTime >= item.StartTime) ||
                         (item.StartTime > schedule.StartTime && item.EndTime < schedule.EndTime))))
                        {
                            return new BadRequestObjectResult(new { message = $"Add schedule for {room.Subject}-{room.ClassName} at date {schedule.Date} {schedule.StartTime} error: User Schedule overlapping!" });
                        }
                    }
                }
                var result = TimetableDAO.Create(_roomContext, schedule).Result;
                schedule = TimetableDAO.GetLast(_roomContext);
                userIds = userIds.Where(item => item != room.CreatorId).ToList();
                var attendances = new List<AttendanceReports>();
                foreach (var userId in userIds)
                {
                    attendances.Add(new AttendanceReports
                    {
                        UserId = userId,
                        Status = "future",
                        TimeTableId = schedule.Id,
                    });
                }
                await _attendanceDAO.CreateAttendance(attendances);
                return new OkObjectResult(new ScheduleResponse
                {
                    id = schedule.Id.ToString(),
                    date = schedule.Date.ToString("yyyy-MM-dd"),
                    startTime = schedule.StartTime.ToString(),
                    endTime = schedule.EndTime.ToString(),
                    subject = room.Subject,
                    @class = room.ClassName
                });
            }
            return new BadRequestObjectResult(new { message = "Class doesn't exist!" });
        }
        public async Task<IActionResult> UpdateSchedule(int semesterId, List<ScheduleRequest> models)
        {
            List<Timetable> updateSchedules = new List<Timetable>();
            List<ScheduleRequest> success = new List<ScheduleRequest>();
            List<object> fail = new List<object>();
            foreach (var model in models)
            {
                var room = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, model.@class, model.subject, semesterId);
                if (room == null)
                {
                    fail.Add(new { message = $"Schedule update for {model.subject}-{model.@class} error: Class doesn't exist " });
                    continue;
                }
                var schedule = new Timetable
                {
                    Id = Convert.ToInt32(model.id),
                    RoomId = room.RoomId,
                    Date = Convert.ToDateTime(model.date),
                    StartTime = Convert.ToDateTime(model.startTime).TimeOfDay,
                    EndTime = Convert.ToDateTime(model.endTime).TimeOfDay,
                };
                var extSchedules = TimetableDAO.GetByRoomId(_roomContext, room.RoomId);
                if (extSchedules.Any(item => (item.Id != schedule.Id) && (item.Date == schedule.Date) &&
                     ((item.StartTime <= schedule.EndTime && schedule.EndTime <= item.EndTime) ||
                     (item.EndTime >= schedule.StartTime && schedule.StartTime >= item.StartTime) ||
                     (item.StartTime > schedule.StartTime && item.EndTime < schedule.EndTime))))
                {
                    fail.Add(new { message = $"Schedule update for {room.Subject}-{room.ClassName} at date {schedule.Date} {schedule.StartTime} error: Schedule overlaping" });
                    continue;
                }
                else
                {
                    var flag = false;
                    var userIds = RoomUserLinkDAO.GetRoomLink(_roomContext, schedule.RoomId)
                    .Select(item => item.UserId);
                    foreach (var id in userIds)
                    {
                        var user = await _userManager.FindByIdAsync(id);
                        var roomsList = RoomDAO.GetRoomsByUserId(_roomContext, id);
                        foreach (var r in roomsList)
                        {
                            var rSchedules = TimetableDAO.GetByRoomId(_roomContext, r.RoomId);
                            if (rSchedules.Any(item => (item.Id != schedule.Id) && (item.Date == schedule.Date) &&
                             ((item.StartTime <= schedule.EndTime && schedule.EndTime <= item.EndTime) ||
                             (item.EndTime >= schedule.StartTime && schedule.StartTime >= item.StartTime) ||
                             (item.StartTime > schedule.StartTime && item.EndTime < schedule.EndTime))))
                            {
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        fail.Add(new { message = $"Schedule update for {room.Subject}-{room.ClassName} at date {schedule.Date} {schedule.StartTime} error: User schedule overlaping" });
                        continue;
                    }
                    foreach (var item in extSchedules)
                    {
                        if (item.Id == schedule.Id)
                        {
                            item.RoomId = schedule.RoomId;
                            item.StartTime = schedule.StartTime;
                            item.EndTime = schedule.EndTime;
                            item.Date = schedule.Date;
                        }
                    }
                    var result = TimetableDAO.Update(_roomContext, extSchedules);
                    success.Add(model);
                }

            }

            return new OkObjectResult(new
            {
                success = success,
                failed = fail
            });
        }

        public async Task<IActionResult> DeleteSchedule(List<string> models)
        {
            List<Timetable> delSchedules = new List<Timetable>();
            foreach (var id in models)
            {
                var schedule = TimetableDAO.GetById(_roomContext, Convert.ToInt32(id));
                var attendances = _attendanceDAO.GetAttendanceBySchedule(schedule.Id);
                await _attendanceDAO.DeleteAttendance(attendances);
                if (schedule != null)
                {
                    delSchedules.Add(schedule);
                }
            }
            var result = await TimetableDAO.DeleteTimeTable(_roomContext, delSchedules);
            return result;
        }

        public async Task<IActionResult> GetSemesterClass(int id)
        {
            var classes = RoomDAO.GetRoomBySemester(_roomContext, id);
            var result = new List<ClassResponse>();
            foreach (var @class in classes)
            {
                var links = RoomUserLinkDAO.GetRoomLink(_roomContext, @class.RoomId);
                var students = links.Where(link => link.UserId != @class.CreatorId).OrderByDescending(x => x.UserId).Select(link => link.UserId).ToList();
                result.Add(new ClassResponse
                {
                    id = @class.RoomId.ToString(),
                    subject = @class.Subject,
                    @class = @class.ClassName,
                    teacher = @class.CreatorId,
                    students = students
                });
            }
            return new OkObjectResult(result);
        }

        public async Task<IActionResult> AddClass(int semesterId, ClassRequest model)
        {
            var extClass = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, model.@class, model.subject, semesterId);
            if (extClass != null)
            {
                return new BadRequestObjectResult(new { message = "Class already exist!" });
            }
            var @class = new Room
            {
                ClassName = model.@class,
                Subject = model.subject,
                SemesterId = semesterId,
                CreatorId = model.teacher,
                StartDate = DateTime.Now,
                EndDate = DateTime.Now,
            };
            var result = RoomDAO.Create(_roomContext, @class);
            @class = RoomDAO.GetLastRoom(_roomContext);
            @class.Image = $"api/rooms/getImage?roomId={@class.RoomId}&imgName=default.png";
            RoomDAO.UpdateRoom(_roomContext, @class);
            var teacherLink = new RoomUserLink
            {
                UserId = model.teacher,
                RoomId = @class.RoomId
            };
            result = RoomUserLinkDAO.Create(_roomContext, teacherLink);
            if (model.students != null && model.students.Count != 0)
            {
                foreach (var id in model.students)
                {
                    var link = new RoomUserLink
                    {
                        UserId = id,
                        RoomId = @class.RoomId,
                    };
                    result = RoomUserLinkDAO.Create(_roomContext, link);
                }
            }
            var links = RoomUserLinkDAO.GetRoomLink(_roomContext, @class.RoomId);
            var students = links.Where(link => link.UserId != @class.CreatorId).Select(link => link.UserId).ToList();
            return new OkObjectResult(new ClassResponse
            {
                id = @class.RoomId.ToString(),
                subject = @class.Subject,
                @class = @class.ClassName,
                teacher = @class.CreatorId,
                students = students
            });
        }

        public async Task<IActionResult> UpdateClass(int semesterId, List<ClassRequest> models)
        {
            var result = new List<ClassResponse>();
            var failed = new List<object>();
            foreach (var model in models)
            {
                var @class = RoomDAO.Get(_roomContext, Convert.ToInt32(model.id));
                var extClass = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, model.@class, model.subject, semesterId);
                if (extClass != null && extClass.RoomId != Convert.ToInt32(model.id))
                {
                    failed.Add(new { message = $"Cannot Update Class: Class Already Exists!" });
                    continue;
                }

                var listUserIds = model.students;
                listUserIds.Add(model.teacher);
                var extLinkUserIds = RoomUserLinkDAO.GetRoomLink(_roomContext, @class.RoomId)
                                    .Select(x => x.UserId).ToList();
                var deleteUserIds = extLinkUserIds.Except(listUserIds).ToList();
                var addUserIds = listUserIds.Except(extLinkUserIds).ToList();

                var schedules = TimetableDAO.GetByRoomId(_roomContext, @class.RoomId);
                var flag = false;
                foreach (var id in addUserIds)
                {
                    var user = await _userManager.FindByIdAsync(id);
                    var roomsList = RoomDAO.GetRoomsByUserId(_roomContext, id).Where(item => item.RoomId != @class.RoomId);
                    foreach (var r in roomsList)
                    {
                        foreach (var schedule in schedules)
                        {
                            var rSchedules = TimetableDAO.GetByRoomId(_roomContext, r.RoomId);
                            if (rSchedules.Any(item => (item.Id != schedule.Id) && (item.Date == schedule.Date) &&
                             ((item.StartTime <= schedule.EndTime && schedule.EndTime <= item.EndTime) ||
                             (item.EndTime >= schedule.StartTime && schedule.StartTime >= item.StartTime) ||
                             (item.StartTime > schedule.StartTime && item.EndTime < schedule.EndTime))))
                            {
                                flag = true;
                            }
                        }
                    }
                    if (flag)
                    {
                        failed.Add(new { message = $"Cannot Update Class: User {user.UserName} Schedule Overlapping!" });
                    }
                }

                if (!flag)
                {
                    var deleteLinks = deleteUserIds.Select(item => RoomUserLinkDAO.GetRoomUserLink(_roomContext, @class.RoomId, item)).ToList();

                    var addLinks = addUserIds.Select(item => new RoomUserLink
                    {
                        UserId = item,
                        RoomId = @class.RoomId
                    }).ToList();
                    var addAttendance = new List<AttendanceReports>();
                    var delAttendance = new List<AttendanceReports>();
                    foreach (var schedule in schedules)
                    {
                        foreach (var userId in addUserIds)
                        {
                            if (userId != @class.CreatorId)
                            {
                                addAttendance.Add(new AttendanceReports
                                {
                                    UserId = userId,
                                    TimeTableId = schedule.Id,
                                    Status = "future"
                                });
                            }
                        }
                        foreach (var userId in deleteUserIds)
                        {
                            var attendance = _attendanceDAO.GetAttendanceByScheduleUserId(schedule.Id, userId);
                            if (attendance != null && schedule.Date > DateTime.Now)
                            {
                                delAttendance.Add(attendance);
                            }
                        }
                    }
                    await _attendanceDAO.DeleteAttendance(delAttendance);
                    await _attendanceDAO.CreateAttendance(addAttendance);
                    await RoomUserLinkDAO.Create(_roomContext, addLinks);
                    await RoomUserLinkDAO.Delete(_roomContext, deleteLinks);
                    @class.Subject = model.subject;
                    @class.CreatorId = model.teacher;
                    @class.ClassName = model.@class;
                    RoomDAO.UpdateRoom(_roomContext, @class);
                    result.Add(new ClassResponse
                    {
                        id = @class.RoomId.ToString(),
                        subject = @class.Subject,
                        @class = @class.ClassName,
                        teacher = @class.CreatorId,
                        students = model.students.Where(x => x != @class.CreatorId).ToList()
                    });
                }
            }
            return new OkObjectResult(new
            {
                success = result,
                failed = failed
            });
        }

        public async Task<IActionResult> DeleteClass(List<string> models)
        {
            var delRoom = new List<string>();
            var failed = new List<string>();
            foreach (var id in models)
            {
                var room = RoomDAO.Get(_roomContext, Convert.ToInt32(id));
                if (room != null)
                {
                    var roomUserLinks = RoomUserLinkDAO.GetRoomLink(_roomContext, room.RoomId);
                    var links = roomUserLinks.Where(link => link.UserId != room.CreatorId).Select(link => link.UserId).ToList();
                    var roomChats = RoomChatDAO.GetChatByRoomId(_roomContext, room.RoomId);
                    var roomTimetables = TimetableDAO.GetByRoomId(_roomContext, room.RoomId);
                    var groups = RoomDAO.GetGroupByRoom(_roomContext, room.RoomId);
                    var log = _userContext.UserLog.Where(item => item.RoomId == id).ToList();
                    if (links.Count > 0 || roomTimetables.Count > 0)
                    {
                        failed.Add(id);
                        continue;
                    }
                    await RoomUserLinkDAO.Delete(_roomContext, roomUserLinks);
                    await _logDAO.DeleteLogs(log);
                    await RoomChatDAO.DeleteRoomChat(_roomContext, roomChats);
                    foreach (var group in groups)
                    {
                        await RoomService.DeleteRoom(_roomContext, group.RoomId, _env);
                    }

                    var path = Path.Combine(_env.ContentRootPath, $"Files/{room.RoomId}");
                    if (Directory.Exists(path))
                    {
                        Directory.Delete(path, true);
                    }

                    await RoomDAO.Delete(_roomContext, room);
                    delRoom.Add(id);
                }
            }
            return new OkObjectResult(new
            {
                success = delRoom,
                failed = failed
            });
        }

        public async Task<IActionResult> GetSemesters()
        {
            var semetsers = SemesterDAO.GetAll(_roomContext);
            return new OkObjectResult(semetsers);
        }
    }
}