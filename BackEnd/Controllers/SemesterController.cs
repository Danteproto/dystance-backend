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
    [Route("api/[controller]")]
    [ApiController]
    public class SemesterController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly ISemesterService _semesterService;

        public SemesterController(IMapper mapper, ISemesterService semesterService)
        {
            _mapper = mapper;
            _semesterService = semesterService;
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
    }
}
