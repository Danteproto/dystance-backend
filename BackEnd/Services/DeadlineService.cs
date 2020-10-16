using BackEnd.DBContext;
using BackEnd.Interfaces;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Responses;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IDeadlineService
    {
        public Task<IActionResult> CreateDeadline(DeadlineRequest deadlineRequest);
        public Task<IActionResult> GetDeadlineForRoom(string roomid);
    }
    public class DeadlineService : IDeadlineService
    {
        private readonly RoomDBContext _roomDBContext;
        private readonly IUserAccessor _userAccessor;

        public DeadlineService(RoomDBContext roomDBContext, IUserAccessor userAccessor)
        {
            _roomDBContext = roomDBContext;
            _userAccessor = userAccessor;
        }
        public async Task<IActionResult> CreateDeadline(DeadlineRequest deadlineRequest)
        {
            //get current room by roomId
            var room = _roomDBContext.Room.FirstOrDefault(r => r.RoomId.ToString() == deadlineRequest.RoomId);
            if (room == null)
            {
                return new BadRequestObjectResult(new { type = 0, message = "Room not found !" });
            }

            //check trong khoang startdate end date
            if (DateTime.Compare(DateTime.Parse(deadlineRequest.DeadlineDate), room.EndDate) > 0)
            {
                return new BadRequestObjectResult(new { type = 1, message = "Deadline date has exceeded room's end date !" });
            }

            var deadline = new Deadline
            {
                CreatorId = _userAccessor.GetCurrentUserId(),
                DeadlineTime = TimeSpan.Parse(deadlineRequest.DeadlineTime),
                DeadlineDate = DateTime.Parse(deadlineRequest.DeadlineDate),
                Description = deadlineRequest.Description,
                RoomId = room.RoomId
            };

            await _roomDBContext.Deadline.AddAsync(deadline);
            var result = await _roomDBContext.SaveChangesAsync();

            if (result > 0)
            {
                return new OkObjectResult(deadline);
            }
            else
            {
                return new ObjectResult("Error adding deadline object")
                {
                    StatusCode = 500
                };
            }
        }

        //TODO: get all Deadline for a room
        public async Task<IActionResult> GetDeadlineForRoom(string roomId)
        {
            //get current room by roomId
            var room = await  _roomDBContext.Room.FirstOrDefaultAsync(r => r.RoomId.ToString() == roomId);
            if (room == null)
            {
                return new BadRequestObjectResult(new { type = 0, message = "Room not found !" });
            }

            var listDeadline = await _roomDBContext.Deadline.ToListAsync();
            var listResult = new ArrayList();

            foreach (var deadline in listDeadline)
            {
                if (deadline.RoomId == room.RoomId)
                {
                    var remaining = new DateTime(deadline.DeadlineDate.Year, deadline.DeadlineDate.Month, deadline.DeadlineDate.Day, deadline.DeadlineTime.Hours, deadline.DeadlineTime.Minutes, deadline.DeadlineTime.Seconds) - DateTime.Now;
                    listResult.Add(new DeadlineResponse 
                    {
                        DeadlineTime = deadline.DeadlineTime.ToString(),
                        DeadlineDate = deadline.DeadlineDate.ToShortDateString(),
                        Description = deadline.Description,
                        RoomId = deadline.RoomId.ToString(),
                        RemainingTime = String.Format("{0} days {1} hours {2} minutes {3} seconds", remaining.Days, remaining.Hours, remaining.Minutes, remaining.Seconds),
                    });
                }
            }

            return new OkObjectResult(listResult);
        }





    }
}
