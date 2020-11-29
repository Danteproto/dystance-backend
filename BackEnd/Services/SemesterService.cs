using BackEnd.Context;
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


        public SemesterService(UserManager<AppUser> userManager
            , UserDbContext userContext
            , RoomDBContext roomContext
            , IWebHostEnvironment env
            )
        {
            _userManager = userManager;
            _userContext = userContext;
            _roomContext = roomContext;
            _env = env;
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
            SemesterDAO.Create(_roomContext, semester);
            semester = SemesterDAO.GetLast(_roomContext);
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
                                    if (reader.GetValue(0) == null && reader.GetValue(1) == null)
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
                                            throw new Exception(" Teacher Accounts in this semester doesn't exist yet. Please import the accounts first");
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
                                        for (int i = 0; i < count; i++)
                                        {
                                            reader.Read();
                                            var student = _userManager.FindByNameAsync(reader.GetValue(1).ToString()).Result;
                                            if (student == null)
                                            {
                                                throw new Exception(" Students Accounts in this semester doesn't exist yet. Please import the accounts first");
                                            }
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
                                    var room = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, className, subject, semester.Id);
                                    var schedule = new Timetable
                                    {
                                        RoomId = room.RoomId,
                                        Date = Convert.ToDateTime(reader.GetValue(0).ToString()),
                                        StartTime = Convert.ToDateTime(reader.GetValue(1).ToString()).TimeOfDay,
                                        EndTime = Convert.ToDateTime(reader.GetValue(2).ToString()).TimeOfDay,
                                    };
                                    var result = TimetableDAO.Create(_roomContext, schedule);
                                }

                            }
                        } while (reader.NextResult());
                    }
                    catch (Exception e)
                    {
                        await DeleteSemester(new List<string>() { semester.Id.ToString() });
                        return new BadRequestObjectResult(new { message = e.Message });
                    }
                }


                return new OkObjectResult(semester);
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
                        await RoomService.DeleteRoom(_roomContext, room.RoomId, _env);
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
                                        if ((reader.GetValue(0) == null && reader.GetValue(1) == null))
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
                                                throw new Exception(" Teacher Accounts in this semester doesn't exist yet. Please import the accounts first");
                                            }
                                            if (room.CreatorId != teacher.Id)
                                            {
                                                var delLink = RoomUserLinkDAO.GetRoomUserLink(_roomContext, room.RoomId, room.CreatorId);
                                                RoomUserLinkDAO.Delete(_roomContext, delLink);
                                                room.CreatorId = teacher.Id;
                                                RoomDAO.UpdateRoom(_roomContext, room);
                                                var link = new RoomUserLink
                                                {
                                                    UserId = teacher.Id,
                                                    RoomId = room.RoomId,
                                                };
                                                RoomUserLinkDAO.Create(_roomContext, link);
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
                                                    throw new Exception($" Student Accounts {reader.GetValue(1).ToString()} in this semester doesn't exist yet. Please import the accounts first");
                                                }
                                                listUserIds.Add(student.Id);
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

                                            RoomUserLinkDAO.Create(_roomContext, addLinks);
                                            RoomUserLinkDAO.Delete(_roomContext, deleteLinks);
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
                                        var room = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, className, subject, semester.Id);
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
                                        var result = TimetableDAO.Create(_roomContext, schedule);

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
            return new OkObjectResult(semester);

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
                    return new BadRequestObjectResult(new { message = "Cannot add overlapping schedule" });
                }
                var result = TimetableDAO.Create(_roomContext, schedule).Result;
                schedule = TimetableDAO.GetLast(_roomContext);
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
            return new BadRequestObjectResult(new { message = "Class not exist!" });
        }
        public async Task<IActionResult> UpdateSchedule(int semesterId, List<ScheduleRequest> models)
        {
            List<Timetable> updateSchedules = new List<Timetable>();
            List<ScheduleRequest> success = new List<ScheduleRequest>();
            List<string> fail = new List<string>();
            foreach (var model in models)
            {
                var room = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, model.@class, model.subject, semesterId);

                var schedule = new Timetable
                {
                    Id = Convert.ToInt32(model.id),
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
                    fail.Add(model.id);
                    continue;
                }
                else
                {
                    foreach(var item in extSchedules)
                    {
                        if(item.Id == schedule.Id)
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
                var students = links.Where(link => link.UserId != @class.CreatorId).Select(link => link.UserId).ToList();
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
                return new BadRequestObjectResult(new { message = "class already exist!" });
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
            var failed = new List<string>();
            foreach (var model in models)
            {
                var @class = RoomDAO.Get(_roomContext, Convert.ToInt32(model.id));
                var extClass = RoomDAO.GetRoomByClassSubjectSemester(_roomContext, model.@class, model.subject, semesterId);
                if (extClass != null && extClass.RoomId != Convert.ToInt32(model.id))
                {
                        failed.Add($"Class cannot upddate: Update to already exited class!");
                        continue;
                }
                @class.Subject = model.subject;
                @class.CreatorId = model.teacher;
                @class.ClassName = model.@class;
                RoomDAO.UpdateRoom(_roomContext, @class);
                var listUserIds = model.students;
                listUserIds.Add(model.teacher);
                var extLinkUserIds = RoomUserLinkDAO.GetRoomLink(_roomContext, @class.RoomId)
                                    .Select(x => x.UserId).ToList();
                var deleteUserIds = extLinkUserIds.Except(listUserIds).ToList();
                var addUserIds = listUserIds.Except(extLinkUserIds).ToList();

                var deleteLinks = deleteUserIds.Select(item => RoomUserLinkDAO.GetRoomUserLink(_roomContext, @class.RoomId, item)).ToList();

                var addLinks = addUserIds.Select(item => new RoomUserLink
                {
                    UserId = item,
                    RoomId = @class.RoomId
                }).ToList();

                RoomUserLinkDAO.Create(_roomContext, addLinks);
                RoomUserLinkDAO.Delete(_roomContext, deleteLinks);

                result.Add(new ClassResponse
                {
                    id = @class.RoomId.ToString(),
                    subject = @class.Subject,
                    @class = @class.ClassName,
                    teacher = @class.CreatorId,
                    students = model.students.Where(x => x != @class.CreatorId).ToList()
                });
            }
            return new OkObjectResult(new
            {
                success = result,
                failed = failed
            });
        }

        public async Task<IActionResult> DeleteClass(List<string> models)
        {
            var delRoom = new List<Room>();
            foreach (var id in models)
            {
                RoomService.DeleteRoom(_roomContext, Convert.ToInt32(id), _env);
            }
            return new OkObjectResult(new { message = "delete succesful" });
        }

        public async Task<IActionResult> GetSemesters()
        {
            var semetsers = SemesterDAO.GetAll(_roomContext);
            return new OkObjectResult(semetsers);
        }
    }
}