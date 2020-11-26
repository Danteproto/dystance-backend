
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

        public UsersController(IUserService userService, IAuthService authService, IMapper mapper)
        {
            _userService = userService;
            _authService = authService;
            _mapper = mapper;
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

        //[HttpPost("revoke-token")]
        //public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
        //{
        //    // accept token from request body or cookie
        //    var token = model.Token ?? Request.Cookies["refreshToken"];

        //    if (string.IsNullOrEmpty(token))
        //        return BadRequest(new { message = "Token is required" });

        //    var response = _userService.RevokeToken(token, ipAddress());

        //    if (!response)
        //        return NotFound(new { message = "Token not found" });

        //    return Ok(new { message = "Token revoked" });
        //}

        [HttpGet]
        [HttpGet("getAll")]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();

            
            return Ok(users);
        }

        [HttpGet("info")]
        public IActionResult GetUserInfoById(string id)
        {
            return _userService.GetById(id);
        }

        //[HttpGet("{id}/refresh-tokens")]
        //public IActionResult GetRefreshTokens(string id)
        //{
        //    var user = _userService.GetById(id);
        //    if (user == null) return NotFound();

        //    return Ok(user.RefreshTokens);
        //}


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

        //[AllowAnonymous]
        //[HttpGet("autoComplete")]
        //public async Task<IActionResult> Autocomplete(string userName)
        //{
        //    return await _userService.AutoComplete(userName);
        //}

        //[AllowAnonymous]
        //[HttpGet("externalLoginServices")]
        //public async Task<IActionResult> GetExternalLoginServices()
        //{
        //    var services = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
        //    return Ok(services[0]);

        //}

        //[AllowAnonymous]
        //[HttpGet("externalLogin")]

        //public IActionResult GetExternalLogin(string returnUrl)
        //{

        //    var redirectUrl = Url.Action("ExternalLoginCallback", "Users", new { ReturnUrl = returnUrl });

        //    var properties = _signInManager.ConfigureExternalAuthenticationProperties("Google", redirectUrl);

        //    var challenge = new ChallengeResult("Google", properties);
        //    return challenge;

        //}

        //[AllowAnonymous]
        //[HttpGet("ExternalLoginCallback")]

        //public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        //{
        //    returnUrl = returnUrl ?? Url.Content("~/");

        //    AuthenticateRequest authenticateRequest = new AuthenticateRequest
        //    {
        //        ReturnUrl = returnUrl,
        //        ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
        //    };


        //    var info = await _signInManager.GetExternalLoginInfoAsync();

        //    var signInResult = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);

        //    if (signInResult.Succeeded)
        //    {
        //        return LocalRedirect(returnUrl);
        //    }
        //    else
        //    {
        //        var email = info.Principal.FindFirstValue(ClaimTypes.Email);

        //        if (email != null)
        //        {
        //            var user = await _userManager.FindByEmailAsync(email);

        //            if (user == null)
        //            {
        //                user = new AppUser
        //                {
        //                    Id = Guid.NewGuid().ToString(),
        //                    UserName = info.Principal.FindFirstValue(ClaimTypes.Email),
        //                    Email = info.Principal.FindFirstValue(ClaimTypes.Email)
        //                };

        //                return Ok(user);
        //            }

        //            await _userManager.AddLoginAsync(user, info);
        //            await _signInManager.SignInAsync(user, isPersistent: false);

        //            var result = info.AuthenticationTokens;

        //            var response = new AuthenticateResponse(new User
        //            { Username = user.UserName }

        //            , info.AuthenticationTokens.Single(f => f.Name == "access_token").Value,
        //                info.AuthenticationTokens.Single(f => f.Name == "refresh_token").Value,
        //                info.AuthenticationTokens.Single(f => f.Name == "expires_at").Value);

        //            return Ok(response);
        //        }
        //    }

        //    throw new RestException(HttpStatusCode.NotFound, new { message = "Email not found" });
        //}


        // helper methods

        //private void setTokenCookie(string token)
        //{
        //    var cookieOptions = new CookieOptions
        //    {
        //        HttpOnly = true,
        //        Expires = DateTime.UtcNow.AddDays(7)
        //    };
        //    Response.Cookies.Append("refreshToken", token, cookieOptions);
        //}

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
    }
}
