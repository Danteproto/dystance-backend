using System;
using System.Collections.Generic;
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
    public class SemestersController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISemesterService _semesterService;
        private readonly ITeacherService _teacherService;
        private readonly IStudentService _studentService;

        public SemestersController(IMapper mapper, ISemesterService semesterService, ITeacherService teacherService, IStudentService studentService)
        {
            _mapper = mapper;
            _semesterService = semesterService;
            _teacherService = teacherService;
            _studentService = studentService;
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
        [HttpGet("classes/get")]
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
        public async Task<IActionResult> UpdateClass([FromBody] List<ClassRequest> models)
        {
            return await _semesterService.UpdateClass(models);
        }
        [HttpPost("classes/delete")]
        public async Task<IActionResult> DeleteClass([FromBody] List<string> ids)
        {
            return await _semesterService.DeleteClass(ids);
        }
        [HttpGet("teachers/get")]
        public async Task<IActionResult> GetAllTeacher(string semesterId)
        {
            return await _teacherService.GetTeacherBySemesterId(semesterId);
        }
        
        [HttpPost("teachers/add")]
        public async Task<IActionResult> AddTeacher([FromBody] TeacherRequest model)
        {
            return await _teacherService.AddTeacher(model);
        }
        [HttpPost("teachers/update")]
        public async Task<IActionResult> UpdateTeacher([FromBody] List<TeacherRequest> model)
        {
            return await _teacherService.UpdateTeacher(model);
        }
        [HttpDelete("teachers/delete")]
        public async Task<IActionResult> DeleteTeacher([FromBody] List<string> model)
        {
            return await _teacherService.DeleteTeacher(model);
        }
        [HttpGet("students/get")]
        public async Task<IActionResult> GetAllStudent(string semesterId)
        {
            return await _studentService.GetStudentBySemesterId(semesterId);
        }
        [HttpPost("students/add")]
        public async Task<IActionResult> AddStudent([FromBody] TeacherRequest model)
        {
            return await _studentService.AddStudent(model);
        }
        [HttpPost("students/update")]
        public async Task<IActionResult> UpdateStudent([FromBody] List<TeacherRequest> model)
        {
            return await _studentService.UpdateStudent(model);
        }
        [HttpDelete("students/delete")]
        public async Task<IActionResult> DeleteStudent([FromBody] List<string> model)
        {
            return await _studentService.DeleteStudent(model);
        }
    }
}
