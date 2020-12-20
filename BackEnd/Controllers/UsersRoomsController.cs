using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Requests;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Authorize]
    [Route("api/users")]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class UsersRoomsController : ControllerBase
    {
        private readonly IUserRoomService _userRoomService;
        private readonly IMapper _mapper;

        public UsersRoomsController(IUserRoomService userRoomService , IMapper mapper)
        {
            _userRoomService = userRoomService;
            _mapper = mapper;
        }

        [HttpGet("timetable")]
        //Timetable Req
        public async Task<IActionResult> Timetable(string startDate, string endDate)
        {
            return await _userRoomService.GetTimeTable(new TimetableRequest
            {
                StartDate = DateTime.Parse(startDate),
                EndDate = DateTime.Parse(endDate)
            });
        }
    }
}
