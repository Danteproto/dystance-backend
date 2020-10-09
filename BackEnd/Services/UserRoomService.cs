using BackEnd.Context;
using BackEnd.DBContext;
using BackEnd.Interfaces;
using BackEnd.Models;
using BackEnd.Requests;
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
    public interface IUserRoomService {
        public Task<IActionResult> GetTimeTable(TimetableRequest model);
        public Task<IActionResult> KickOutMember(KickOutMemberRequest model);


    }

    public class UserRoomService: IUserRoomService
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

            var list = new List<Room>();

            foreach (var roomId in userLinks)
            {
                //get room
                var room = await _roomDbContext.Room.FirstOrDefaultAsync(r => r.RoomId == roomId
                && r.StartDate >= model.StartDate
                && r.EndDate <= model.EndDate);

                list.Add(room);
            }
            return new OkObjectResult(list);
        }

        public async Task<IActionResult> KickOutMember(KickOutMemberRequest model)
        {
            var appUser = await _userManager.FindByIdAsync(_userAccessor.GetCurrentUserId());
            var room = await _roomDbContext.Room.FirstOrDefaultAsync(r => r.RoomId.ToString() == model.RoomId);

            if (room == null)
            {
                return new BadRequestObjectResult(new { type = 1, message = "Not Found Room!" });
            }


            if (room.CreatorId == model.UserId)
            {
                return new BadRequestObjectResult(new { type = 0, message = "Can not kick out creator!" });
            }

            //check if creator
            if(appUser.Id != room.CreatorId)
            {
                return new BadRequestObjectResult(new { type = 2, message = "Not Creator Of The Room!" });
            }

            //Find kick member
            var kickMember = await _roomDbContext.RoomUserLink.FirstOrDefaultAsync(r => r.UserId == model.UserId && r.RoomId.ToString() == model.RoomId);
            if(kickMember == null)
            {
                return new BadRequestObjectResult(new { type = 3, message = "Not Found The Member In The Room!" });
            }

            //kick
            var result = _roomDbContext.RoomUserLink.Remove(kickMember);
            await _roomDbContext.SaveChangesAsync();
            if(result.State == EntityState.Detached)
            {
                return new OkObjectResult(result);
            }
            else
            {
                return new BadRequestObjectResult(new { type = 4, message = "Fail To Kick The Member Out Of The Room!" });
            }
            
        }




    }
}
