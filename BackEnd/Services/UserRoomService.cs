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
                var room = await _roomDbContext.Room.Where(r => r.RoomId == roomId && !r.Group).FirstOrDefaultAsync();
                if (room != null)
                {
                    rooms.Add(room);
                }
            }


            var resultList = new List<TimetableResponse>();

            foreach (var room in rooms)
            {
                var roomTimetables = TimetableDAO.GetByRoomId(_roomDbContext, room.RoomId);

                foreach(var timetable in roomTimetables)
                {
                    if(timetable.Date >= model.StartDate && timetable.Date <= model.EndDate)
                    resultList.Add(new TimetableResponse
                    {
                        RoomId = room.RoomId.ToString(),
                        EventType = 0,
                        Title = room.Subject +"-" +room.ClassName,
                        TeacherId = room.CreatorId,
                        StartDate = timetable.Date.ToString("yyyy-MM-dd") + "T" + timetable.StartTime,
                        EndDate = timetable.Date.ToString("yyyy-MM-dd") + "T" + timetable.EndTime,
                    });
                }
            }
            return new OkObjectResult(resultList);
        }
    }
}
