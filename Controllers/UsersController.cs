using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using BackEnd.Context;
using BackEnd.Models;
using BackEnd.Errors;
using BackEnd.Interfaces;
using BackEnd.Security;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Http;
using AutoMapper;
using EmailService;
using BackEnd.Responses;
using Google.Apis.Auth.OAuth2.Requests;
using BackEnd.Requests;
using System.Text.Json;
using Google.Apis.Auth;
using static Google.Apis.Auth.GoogleJsonWebSignature;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackEnd.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private IUserService _userService;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IMapper _mapper;
        private readonly IEmailSender _emailSender;
        private readonly IAuthService _authService;

        public UsersController(IUserService userService, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IMapper mapper, IEmailSender emailSender, IAuthService authService)
        {
            _userService = userService;
            _signInManager = signInManager;
            _userManager = userManager;
            _mapper = mapper;
            _emailSender = emailSender;
            _authService = authService;
        }

        [AllowAnonymous]
        [HttpPost("authenticate")]
        public async Task<IActionResult> Authenticate([FromBody] AuthenticateRequest model)
        {
            var response = await _userService.Authenticate(model);

            //setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        [Authorize]
        [HttpPost("refreshToken")]
        public IActionResult RefreshToken([FromBody] RefreshTokenRequestz tokenRequest)
        {
            var refreshToken = tokenRequest.RefreshToken;
            var response = _userService.RefreshToken(refreshToken);

            if (response == null)
                return Unauthorized(new { message = "Invalid token" });

            //setTokenCookie(response.RefreshToken);

            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public IActionResult RevokeToken([FromBody] RevokeTokenRequest model)
        {
            // accept token from request body or cookie
            var token = model.Token ?? Request.Cookies["refreshToken"];

            if (string.IsNullOrEmpty(token))
                return BadRequest(new { message = "Token is required" });

            var response = _userService.RevokeToken(token, ipAddress());

            if (!response)
                return NotFound(new { message = "Token not found" });

            return Ok(new { message = "Token revoked" });
        }

        [HttpGet]
        public IActionResult GetAll()
        {
            var users = _userService.GetAll();
            return Ok(users);
        }

        [Authorize]
        [HttpGet("info")]
        public IActionResult GetUserInfoById(string id)
        {
            var user = _userService.GetById(id);
            if (user == null) return NotFound();

            return Ok(new UserInfoResponse
            {
                Id = user.Id,
                UserName = user.UserName,
                RealName = user.RealName,
                Avatar = ""
            });
        }

        [HttpGet("{id}/refresh-tokens")]
        public IActionResult GetRefreshTokens(string id)
        {
            var user = _userService.GetById(id);
            if (user == null) return NotFound();

            return Ok(user.RefreshTokens);
        }


        [AllowAnonymous]
        [HttpPost("register")]

        public async Task<IActionResult> Register([FromBody] RegisterRequest userModel)
        {

            var appUser = await _userManager.FindByEmailAsync(userModel.Email);
            if (appUser != null)
            {
                throw new RestException(HttpStatusCode.BadRequest, new { type = "0", message = "Email already exists" });
            }

            appUser = await _userManager.FindByNameAsync(userModel.Username);
            if (appUser != null)
            {
                throw new RestException(HttpStatusCode.BadRequest, new { type = "1", message = "Username already exists" });
            }

            var user = new AppUser
            {
                Email = userModel.Email,
                UserName = userModel.Username,
                RealName = userModel.RealName,
                DOB = userModel.DOB
            };

            var result = await _userManager.CreateAsync(user, userModel.Password);
            if (!result.Succeeded)
            {
                throw new RestException(HttpStatusCode.InternalServerError, new { error = result.Errors.ToList()[0].Description });
            }


            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Users", new { token, email = user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Confirmation email link", confirmationLink, null);
            await _emailSender.SendEmailAsync(message);
            //await _userManager.AddToRoleAsync(user, "Visitor");


            return Ok(new RegisterResponse
            {
                Email = userModel.Email,
                UserName = userModel.Username,
                Token = token,
                TokenLink = confirmationLink
            });
        }

        [AllowAnonymous]
        [HttpGet("resendEmail")]
        public async Task<IActionResult> ResendEmail(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new RestException(HttpStatusCode.InternalServerError, new { error = "User not found" });

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Users", new { token, email = user.Email }, Request.Scheme);
            var message = new Message(new string[] { user.Email }, "Confirmation email link", confirmationLink, null);
            await _emailSender.SendEmailAsync(message);

            return Ok(new { message = "Successful" });
        }


        [AllowAnonymous]
        [HttpGet("confirmEmail")]
        public async Task<string> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                throw new RestException(HttpStatusCode.InternalServerError, new { error = "Error!" });

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return "Email confirmed!";
            }
            throw new RestException(HttpStatusCode.InternalServerError, new { error = "Error!" });
        }

        [AllowAnonymous]
        [HttpPost("google")]
        public async Task<IActionResult> Google([FromBody] GoogleLoginRequest userView)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(userView.TokenId, new GoogleJsonWebSignature.ValidationSettings()).Result;
                var user = await _authService.Authenticate(payload);

                if (user == null)
                {
                    return NotFound(new { message = "You need to update your information before proceed", googleName = payload.Name });
                }

                var token = _userService.VerifyAndReturnToken(user);

                return Ok(token);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Expired"))
                {
                    throw new RestException(HttpStatusCode.InternalServerError, new { type = "0", error = ex.Message });
                }
                else
                {
                    throw new RestException(HttpStatusCode.InternalServerError, new { type = "1", error = ex.Message });
                }
            }

        }

        [AllowAnonymous]
        [HttpPost("google/updateInfo")]
        public async Task<IActionResult> GoogleUpdateInfo([FromBody] GoogleLoginRequest userView)
        {
            Payload payload;
            try
            {
                payload = GoogleJsonWebSignature.ValidateAsync(userView.TokenId, new GoogleJsonWebSignature.ValidationSettings()).Result;


            }
            catch (Exception ex)
            {
                throw new RestException(HttpStatusCode.InternalServerError, new { message = ex.Message });
            }
            var user = new AppUser
            {
                UserName = userView.UserName,
                Email = payload.Email,
                RealName = userView.RealName,
                DOB = userView.DOB
            };

            var appUser = await _userManager.FindByNameAsync(userView.UserName);
            if (appUser != null)
            {
                throw new RestException(HttpStatusCode.BadRequest, new { type = "1", message = "Username already exists" });
            }

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                appUser = await _userManager.FindByEmailAsync(payload.Email);

                var token = _userService.VerifyAndReturnToken(appUser);

                return Ok(token);
            }

            return NotFound();
    }


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

    private void setTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTime.UtcNow.AddDays(7)
        };
        Response.Cookies.Append("refreshToken", token, cookieOptions);
    }

    private string ipAddress()
    {
        if (Request.Headers.ContainsKey("X-Forwarded-For"))
            return Request.Headers["X-Forwarded-For"];
        else
            return HttpContext.Connection.RemoteIpAddress.MapToIPv4().ToString();
    }





}
}
