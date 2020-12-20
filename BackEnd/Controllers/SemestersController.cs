using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Services;
using BackEnd.Ultilities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    [ExcludeFromCodeCoverage]
    public class SemestersController : ControllerBase
    {
        private readonly ISemesterService _semesterService;

        public SemestersController(IMapper mapper, ISemesterService semesterService, ITeacherService teacherService, IStudentService studentService)
        {
            _semesterService = semesterService;
        }

        [HttpGet]
        public async Task<IActionResult> GetSemester()
        {
            return await _semesterService.GetSemesters();
        }

        [HttpPost("add")]
        public async Task<IActionResult> AddSemester()
        {
            return await _semesterService.AddSemester(Request);
        }

        [HttpPost("update")]
        public async Task<IActionResult> UpdateSemester()
        {
            return await _semesterService.UpdateSemester(Request);
        }
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteSemester([FromBody] List<string> ids)
        {
            return await _semesterService.DeleteSemester(ids);
        }
        [HttpGet("schedules")]
        public async Task<IActionResult> GetSemesterSchedules(int semesterId)
        {
            return await _semesterService.GetSemesterSchedule(semesterId);
        }
        [HttpPost("schedules/add")]
        public async Task<IActionResult> AddSchedules(int semesterId, [FromBody] ScheduleRequest model)
        {
            return await _semesterService.AddSchedule(semesterId, model);
        }
        [HttpPost("schedules/update")]
        public async Task<IActionResult> UpdateSchedules(int semesterId, [FromBody] List<ScheduleRequest> models)
        {
            return await _semesterService.UpdateSchedule(semesterId, models);
        }
        [HttpPost("schedules/delete")]
        public async Task<IActionResult> DeleteSchedules([FromBody] List<string> models)
        {
            return await _semesterService.DeleteSchedule(models);
        }
        [HttpGet("classes")]
        public async Task<IActionResult> GetClassBySemester(int semesterId)
        {
            return await _semesterService.GetSemesterClass(semesterId);
        }
        [HttpPost("classes/add")]
        public async Task<IActionResult> AddClass(int semesterId, [FromBody] ClassRequest model)
        {
            return await _semesterService.AddClass(semesterId, model);
        }
        [HttpPost("classes/update")]
        public async Task<IActionResult> UpdateClass(int semesterId, [FromBody] List<ClassRequest> models)
        {
            return await _semesterService.UpdateClass(semesterId ,models);
        }
        [HttpPost("classes/delete")]
        public async Task<IActionResult> DeleteClass([FromBody] List<string> ids)
        {
            return await _semesterService.DeleteClass(ids);
        }
        
    }
}
