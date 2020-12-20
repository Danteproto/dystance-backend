using AutoMapper;
using BackEnd.Attributes;
using BackEnd.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Controllers
{
    [ApiKey]
    [Route("api/[controller]")]
    [ApiController]
    public class PublicController : Controller
    {
        private readonly ISemesterService _semesterService;
        private readonly IPublicService _publicService;

        public PublicController(IMapper mapper, ISemesterService semesterService, IPublicService publicService)
        {
            _semesterService = semesterService;
            _publicService = publicService;
        }
        [HttpGet("semesters")]
        public async Task<IActionResult> GetSemester()
        {
            return await _semesterService.GetSemesters();
        }
        [HttpGet("classes")]
        public async Task<IActionResult> GetClasses(int semesterId)
        {
            return await _publicService.GetSemesterClass(semesterId);
        }
        [HttpGet("logs")]
        public async Task<IActionResult> GetLog(int roomId)
        {
            return await _publicService.GetLogByRoom(roomId);
        }
        [HttpGet("attendance")]
        public async Task<IActionResult> GetAttendance(int roomId)
        {
            return await _publicService.GetAttendanceByRoom(roomId);
        }
    }
}
