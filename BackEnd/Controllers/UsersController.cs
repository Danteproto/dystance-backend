
using System.Threading.Tasks;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using BackEnd.Requests;
using BackEnd.Ultilities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.StaticFiles;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Linq;
using System.Collections.Generic;

namespace BackEnd.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IMapper _mapper;
        private readonly ITeacherService _teacherService;
        private readonly IStudentService _studentService;

        public UsersController(IUserService userService, IAuthService authService, IMapper mapper, ITeacherService teacherService, IStudentService studentService)
        {
            _userService = userService;
            _authService = authService;
            _mapper = mapper;
            _teacherService = teacherService;
            _studentService = studentService;
        }

        [AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Authenticate()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userService.Authenticate(_mapper.Map<AuthenticateRequest>(reqForm));
        }

        [AllowAnonymous]
        [HttpPost("refreshToken")]
        public IActionResult RefreshToken()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return _userService.RefreshToken(_mapper.Map<RefreshTokenRequestz>(reqForm).RefreshToken);
        }

        [HttpGet]
        [HttpGet("getAll")]
        public async Task<IActionResult> GetAll()
        {
            var users = await _userService.GetAll();
            return Ok(users);
        }

        [HttpGet("info")]
        public async Task<IActionResult> GetUserInfoById(string id)
        {
            return await _userService.GetById(id);
        }

        [AllowAnonymous]
        [HttpPost("register")]

        public async Task<IActionResult> Register()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userService.Register(_mapper.Map<RegisterRequest>(reqForm));
        }

        [AllowAnonymous]
        [HttpPost("resendEmail")]
        public async Task<IActionResult> ResendEmail()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userService.ResendEmail(_mapper.Map<ResendEmailRequest>(reqForm));
        }


        [AllowAnonymous]
        [HttpGet("confirmEmail")]
        public async Task<string> ConfirmEmail(string token, string email)
        {
            return await _userService.ConfirmEmail(token, email);
        }

        [AllowAnonymous]
        [HttpPost("google")]
        public async Task<IActionResult> Google()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _authService.Google(_mapper.Map<GoogleLoginRequest>(reqForm));

        }

        [AllowAnonymous]
        [HttpPost("google/updateInfo")]
        public async Task<IActionResult> GoogleUpdateInfo()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _authService.GoogleUpdateInfo(_mapper.Map<GoogleLoginRequest>(reqForm));
        }


        [HttpGet("currentUser")]
        public async Task<IActionResult> GetCurrentUser()
        {
            return await _userService.GetCurrentUser();
        }

        [AllowAnonymous]
        [HttpPost("resetPassword/send")]
        public async Task<IActionResult> ResetPasswordSend()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userService.ResetPasswordSend(_mapper.Map<ResetPasswordRequest>(reqForm));
        }

        [AllowAnonymous]
        [HttpPost("resetPassword/verify")]
        public async Task<IActionResult> ResetPasswordVerify()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userService.ResetPasswordVerify(_mapper.Map<ResetPasswordVerify>(reqForm));
        }

        [AllowAnonymous]
        [HttpPost("resetPassword/update")]
        public async Task<IActionResult> ResetPasswordUpdate()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userService.ResetPasswordUpdate(_mapper.Map<ResetPasswordUpdate>(reqForm));
        }

        [HttpPost("updateProfile")]
        public async Task<IActionResult> UpdateProfile()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userService.UpdateProfile(_mapper.Map<UpdateProfileRequest>(reqForm));
        }
        
        [AllowAnonymous]
        [HttpGet("getAvatar")]
        public async Task<IActionResult> GetAvatar(string userName, string fileName, string realName)
        {
            realName = realName == null ? "default" : realName;
            var file = _userService.getAvatar(realName, userName, fileName);
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
            Response.Headers.Add("Content-Disposition", $"attachment; filename={realName}");
            return File(file, contentType);
        }

        [HttpPost("chat/add")]
        public async Task<IActionResult> PrivateMessage()
        {
            return await _userService.PrivateMessage(Request);
        }

        [HttpGet("chat/get")]
        public IActionResult GetPrivateMessage(string id1, string id2)
        {
            var pmList = _userService.GetPrivateMessage(id1, id2);
            return Content(JsonConvert.SerializeObject(pmList, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            }));
        }
        [HttpGet("chat/getLast")]
        public IActionResult GetLastPm(string id1, string id2)
        {
            var lastPm = _userService.GetLastPm(id1, id2);
            return Content(JsonConvert.SerializeObject(lastPm, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            }));
        }

        [HttpGet("chat/preview")]
        public IActionResult GetPreview(string id)
        {
            var previews = _userService.GetPreview(id);
            return Content(JsonConvert.SerializeObject(previews, new JsonSerializerSettings
            {
                ContractResolver = new DefaultContractResolver
                {
                    NamingStrategy = new CamelCaseNamingStrategy()
                },
                Formatting = Formatting.Indented
            }));
        }
        
        [AllowAnonymous]
        [HttpGet("chat/getFile")]
        public async Task<IActionResult> PMFile(string Id, string fileName, int type, string realName)
        {
            var file = _userService.GetPMFile(Id, fileName, type);
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
            Response.Headers.Add("Content-Disposition", $"attachment; filename={realName}");
            return File(file, contentType);
        }

        [HttpPost("log")]
        public async Task<IActionResult> Log()
        {
            var reqForm = Extensions.DictionaryToPascal(Request.Form.GetFormParameters());
            return await _userService.Log(_mapper.Map<LogRequest>(reqForm));
        }

        [HttpGet("logs/getByRoom")]
        public async Task<IActionResult> GetLogByRoom(string roomId)
        {
            return await _userService.GetLogByRoom(roomId);
        }

        [HttpGet("teachers")]
        public async Task<IActionResult> GetAllTeacher()
        {
            return await _teacherService.GetTeacher();
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
        [HttpPost("teachers/delete")]
        public async Task<IActionResult> DeleteTeacher([FromBody] List<string> model)
        {
            return await _teacherService.DeleteTeacher(model);
        }
        [HttpGet("students")]
        public async Task<IActionResult> GetAllStudent()
        {
            return await _studentService.GetStudent();
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
        [HttpPost("students/delete")]
        public async Task<IActionResult> DeleteStudent([FromBody] List<string> model)
        {
            return await _studentService.DeleteStudent(model);
        }
        [HttpPost("accounts")]
        public async Task<IActionResult> AddSemester()
        {
            return await _userService.AddAccount(Request);
        }
        //private string ipAddress()
        //{
        //    if (Request.Headers.ContainsKey("X-Forwarded-For"))
        //        return Request.Headers["X-Forwarded-For"];
        //    else
        //        return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
        //}
        
        [HttpGet("reports/attendance")]
        public async Task<IActionResult> GetAttendanceReports(string id, string semesterId)
        {
            return await _userService.GetAttendanceReports(id, semesterId);
        }

        [HttpPost("reports/attendance/update")]
        public async Task<IActionResult> UpdateAttendanceReports([FromBody] UpdateAttendanceStudentRequest model)
        {   
            return await _userService.UpdateAttendanceReports( model);
        }

        [HttpGet("reports/attendance/export")]
        public async Task<IActionResult> ExportAttendanceReports(string roomId)
        {
            var fileInfo =  await _userService.ExportAttendance(roomId);
            var bytes = System.IO.File.ReadAllBytes(fileInfo.FullName);

            const string contentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
            HttpContext.Response.ContentType = contentType;
            HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", "Content-Disposition");

            var fileContentResult = new FileContentResult(bytes, contentType)
            {
                FileDownloadName = fileInfo.Name
            };

            System.IO.File.Delete(fileInfo.FullName);

            return fileContentResult;
        }
    }
}
