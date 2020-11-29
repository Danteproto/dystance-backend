using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Requests;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpPost("accounts/import")]
        public async Task<IActionResult> AddExcel()
        {
            return await _adminService.AddAccount(Request);
        }
        [HttpGet("accounts")]
        public async Task<IActionResult> GetAllTeacher()
        {
            return await _adminService.GetAccounts();
        }
        [HttpPost("accounts/add")]
        public async Task<IActionResult> AddAccount([FromBody] AdminRequest model)
        {
            return await _adminService.AddAccountAdmin(model);
        }
        [HttpPost("accounts/update")]
        public async Task<IActionResult> UpdateAccount([FromBody] List<AdminRequest> model)
        {
            return await _adminService.UpdateAccountAdmin(model);
        }
        [HttpPost("accounts/delete")]
        public async Task<IActionResult> DeleteAccount([FromBody] List<string> model)
        {
            return await _adminService.DeleteManageAccounts(model);
        }
    }
}
