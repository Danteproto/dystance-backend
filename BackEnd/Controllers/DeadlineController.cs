using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Requests;
using BackEnd.Services;
using BackEnd.Ultilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Route("api/rooms/deadline")]
    [ApiController]
    [Authorize]
    public class DeadlineController : ControllerBase
    {
        private readonly IDeadlineService _deadlineService;
        private readonly IMapper _mapper;

        public DeadlineController(IDeadlineService deadlineService, IMapper mapper)
        {
            _deadlineService = deadlineService;
            _mapper = mapper;

        }
        //createDeadline
        [HttpPost("create")]
        public async Task<IActionResult> CreateDeadLine()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _deadlineService.CreateDeadline(_mapper.Map<DeadlineRequest>(reqForm));
        }

        //Get List Deadline By RoomId
        [HttpGet("get")]
        public async Task<IActionResult> GetDeadlineForRoom(string roomId)
        {
            return await _deadlineService.GetDeadlineForRoom(roomId);
        }
    }
}
