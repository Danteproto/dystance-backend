using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Interfaces;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Responses;
using BackEnd.Ultilities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing.Patterns;
using Microsoft.EntityFrameworkCore;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IUserRoomService
    {
        public Task<IActionResult> GetTimeTable(TimetableRequest model);
    }

    public class UserRoomService : IUserRoomService
    {
        private readonly RoomDBContext _roomDbContext;
        private readonly IUserAccessor _userAccessor;
        private readonly UserManager<AppUser> _userManager;

        public UserRoomService(RoomDBContext roomDbContext, IUserAccessor userAccessor, UserManager<AppUser> userManager)
        {
            _roomDbContext = roomDbContext;
            _userAccessor = userAccessor;
            _userManager = userManager;
        }

        public async Task<IActionResult> GetTimeTable(TimetableRequest model)
        {
            var appUser = await _userManager.FindByIdAsync(_userAccessor.GetCurrentUserId());

            var userLinks = (from userRooms in _roomDbContext.RoomUserLink
                             where userRooms.UserId == appUser.Id
                             select userRooms.RoomId).ToList();

            //Toàn bộ room người dùng này tham gia vào
            var rooms = new List<Room>();

            foreach (var roomId in userLinks)
            {
                //get room
                var room = await _roomDbContext.Room.FirstOrDefaultAsync(r => r.RoomId == roomId);

                rooms.Add(room);
            }


            var resultList = new List<TimetableResponse>();

            foreach (var room in rooms)
            {
                var deadlines = _roomDbContext.Deadline
                    .Where(dl => dl.DeadlineDate >= model.StartDate 
                                && dl.DeadlineDate <= model.EndDate 
                                && dl.RoomId == room.RoomId)
                    .ToList();

                foreach (var deadline in deadlines)
                {
                    var deadlineResult = new TimetableResponse
                    {
                        RoomId = room.RoomId.ToString(),
                        EventType = 1,
                        Title = deadline.Title,
                        CreatorId = deadline.CreatorId,
                        Description = deadline.Description,
                        StartDate = deadline.DeadlineDate.ToString("yyyy-MM-dd") + "T" + deadline.DeadlineTime,
                        EndDate = deadline.DeadlineDate.ToString("yyyy-MM-dd") + "T" + deadline.DeadlineTime,
                    };
                    resultList.Add(deadlineResult);
                }

                var repeatOccurence = int.Parse(room.RepeatOccurrence);
                var roomTimetables = TimetableDAO.GetByRoomId(_roomDbContext, room.RoomId);

                DateTime startDate = room.StartDate;
                //repeat week
                while (startDate <= room.EndDate)
                {
                    foreach (DateTime day in DateTimeUtil.EachDay(startDate, startDate.AddDays(7)))
                    {
                        if (roomTimetables.Any(item => item.DayOfWeek == day.DayOfWeek.ToString().ToLower()))
                        {
                            if (day.Date <= room.EndDate && day.Date >= room.StartDate)
                            {
                                var timetable = roomTimetables.Where(item => item.DayOfWeek == day.DayOfWeek.ToString().ToLower()).First();
                                var roomResult = new TimetableResponse
                                {
                                    RoomId = room.RoomId.ToString(),
                                    EventType = 0,
                                    Title = room.RoomName,
                                    CreatorId = room.CreatorId,
                                    Description = room.Description,
                                    StartDate = day.ToString("yyyy-MM-dd") + "T" + timetable.StartTime,
                                    EndDate = day.ToString("yyyy-MM-dd") + "T" + timetable.EndTime,
                                };
                                resultList.Add(roomResult);
                            }
                        }
                        if (day.DayOfWeek == DayOfWeek.Sunday)
                        {
                            //indicates weekend, jump skip weeks
                            startDate = startDate.AddDays(7 * repeatOccurence);

                            //if not start of a week (monday), decrease days to monday
                            while (startDate.DayOfWeek != DayOfWeek.Monday)
                            {
                                startDate = startDate.AddDays(-1);
                            }
                            break;
                        }
                    }
                }
            }
            return new OkObjectResult(resultList);
        }
    }
}
