using BackEnd.Context;
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
                var repeatOccurence = int.Parse(room.RepeatOccurence);
                var repeatDays = room.RepeatDays.Split(",");

                for (int i = 0; i < repeatDays.Length; i++)
                {
                    repeatDays[i] = repeatDays[i].Replace("\"", "").Trim();
                }

                int x = 0;
                DateTime startDate = room.StartDate;
                //repeat week
                while (x < repeatOccurence)
                {
                    foreach (DateTime day in DateTimeUtil.EachDay(startDate, startDate.AddDays(7)))
                    {
                        if (repeatDays.Contains(day.DayOfWeek.ToString().ToLower()))
                        {
                            if (day.Date <= model.EndDate && day.Date >= model.StartDate)
                            {
                                var roomResult = new TimetableResponse
                                {
                                    RoomId = room.RoomId.ToString(),
                                    EventType = 0,
                                    Title = room.RoomName,
                                    CreatorId = room.CreatorId,
                                    Description = room.Description,
                                    StartDate = day.ToString("yyyy-MM-dd") + "T" + room.StartHour,
                                    EndDate = day.ToString("yyyy-MM-dd") + "T" + room.EndHour
                                };
                                resultList.Add(roomResult);
                            }
                        }
                    }
                    x++;
                    startDate = startDate.AddDays(7);
                }
            }
            return new OkObjectResult(resultList);
        }
    }
}
