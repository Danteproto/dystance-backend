using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Requests;
using BackEnd.Services;
using BackEnd.Ultilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Authorize]
    [Route("api")]
    [ApiController]
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

        [HttpPost("kickout")]
        //Timetable Req
        public async Task<IActionResult> KickOutMember()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userRoomService.KickOutMember(_mapper.Map<KickOutMemberRequest>(reqForm));
        }

    }
}
