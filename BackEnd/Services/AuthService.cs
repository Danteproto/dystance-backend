using BackEnd.Errors;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Security;
using EmailService;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IAuthService
    {
        Task<IActionResult> Google( GoogleLoginRequest userView);
        Task<IActionResult> GoogleUpdateInfo( GoogleLoginRequest userView);

    }

    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly IJwtGenerator _jwtGenerator;

        public AuthService(UserManager<AppUser> userManager, IJwtGenerator jwtGenerator)
        {
            _userManager = userManager;
            _jwtGenerator = jwtGenerator;
        }

        public async Task<IActionResult> Google(GoogleLoginRequest userView)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(userView.TokenId, new GoogleJsonWebSignature.ValidationSettings()).Result;
                var user = await _userManager.FindByEmailAsync(payload.Email);

                if (user == null)
                {
                    return new NotFoundObjectResult(new { type = 0 ,message = "You need to update your information before proceed", googleName = payload.Name, email = payload.Email });
                }
                if (!user.EmailConfirmed)
                {
                    return new BadRequestObjectResult(new { type = 1, message = "You must confirm your email before login" });
                }
                var token = _jwtGenerator.VerifyAndReturnToken(user);

                return new OkObjectResult(token);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Expired"))
                {
                    var internalErr = new ObjectResult(new { type = 2, error = ex.Message })
                    {
                        StatusCode = 500
                    };
                    return internalErr;
                }
                else
                {
                    var internalErr = new ObjectResult(new { type = 3, error = ex.Message })
                    {
                        StatusCode = 500
                    };
                    return internalErr;
                }
            }

        }


        public async Task<IActionResult> GoogleUpdateInfo(GoogleLoginRequest userView)
        {
            var user = new AppUser
            {
                UserName = userView.UserName,
                Email = userView.Email,
                RealName = userView.RealName,
                DOB = userView.Dob
            };

            var appUser = await _userManager.FindByNameAsync(userView.UserName);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = 1, message = "Username already exists" });
            }

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                appUser = await _userManager.FindByEmailAsync(userView.Email);
                //confirm email
                string token = await _userManager.GenerateEmailConfirmationTokenAsync(appUser);
                await _userManager.ConfirmEmailAsync(appUser, token);

                var userToken = _jwtGenerator.VerifyAndReturnToken(user);

                return new OkObjectResult(userToken);
            }

            return new NotFoundObjectResult("");
        }



    }
}
