using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Responses;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IPublicService
    {
        public Task<IActionResult> GetSemesterClass(int id);
        public Task<IActionResult> GetLogByRoom(int id);
        public Task<IActionResult> GetAttendanceByRoom(int id);
    }
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PublicService : IPublicService
    {
        private readonly RoomDBContext _roomContext;
        private readonly UserManager<AppUser> _userManager;
        private readonly ILogDAO _logDAO;
        private readonly UserDbContext _userContext;

        public PublicService(UserManager<AppUser> userManager
            ,RoomDBContext roomContext
            ,ILogDAO logDAO
            ,UserDbContext userContext)
        {
            _roomContext = roomContext;
            _userManager = userManager;
            _logDAO = logDAO;
            _userContext = userContext;
        }

        public async Task<IActionResult> GetAttendanceByRoom(int id)
        {
            var timetables = TimetableDAO.GetByRoomId(_roomContext, id);
            var attendanceList = new List<AttendanceTeacherResponse>();
            foreach (var timetable in timetables)
            {
                var room = await _roomContext.Room.FirstOrDefaultAsync(x => x.RoomId == timetable.RoomId);
                if(room == null)
                {
                    break;
                }
                var listStudent = new List<AttendanceStudent>();

                var listStudentId = await (from attendances in _userContext.AttendanceReports
                                           where attendances.TimeTableId == timetable.Id
                                           select new AttendanceStudent
                                           {
                                               Id = attendances.UserId,
                                               Status = attendances.Status
                                           }).ToListAsync();
                foreach(var student in listStudentId)
                {
                    student.Id = _userManager.FindByIdAsync(student.Id).Result.UserName;
                }
                listStudent.AddRange(listStudentId);
                attendanceList.Add(new AttendanceTeacherResponse
                {
                    Id = timetable.Id.ToString(),
                    Class = room.ClassName,
                    Subject = room.Subject,
                    Date = String.Format("{0:yyyy-MM-dd}", timetable.Date),
                    StartTime = timetable.StartTime.ToString(@"hh\:mm"),
                    EndTime = timetable.EndTime.ToString(@"hh\:mm"),
                    Teacher = _userManager.FindByIdAsync(room.CreatorId).Result.UserName,
                    Students = listStudent
                });
            }
            return new OkObjectResult(attendanceList);
        }

        public async Task<IActionResult> GetLogByRoom(int id)
        {
            var roomLists = await _logDAO.GetLogsByRoomId(id.ToString());

            var list = new List<LogResponse>();


            foreach (var rooms in roomLists)
            {
                var user = _userManager.FindByIdAsync(rooms.UserId).Result;
                list.Add(new LogResponse()
                {
                    DateTime = String.Format("{0:s}", rooms.DateTime),
                    LogType = rooms.LogType,
                    RoomId = rooms.RoomId,
                    UserId = user.UserName + "-" + user.RealName,
                    Description = rooms.Description
                });
            }


            return new OkObjectResult(list);
        }

        public async Task<IActionResult> GetSemesterClass(int id)
        {
            var classes = RoomDAO.GetRoomBySemester(_roomContext, id);
            var result = new List<ClassResponse>();
            foreach (var @class in classes)
            {
                var teacher = _userManager.FindByIdAsync(@class.CreatorId).Result;
                result.Add(new ClassResponse
                {
                    id = @class.RoomId.ToString(),
                    subject = @class.Subject,
                    @class = @class.ClassName,
                    teacher = teacher.UserName,
                });
            }
            return new OkObjectResult(result);
        }
    }
}
