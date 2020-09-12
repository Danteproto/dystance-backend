using BackEnd.Errors;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Security;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public interface IAuthService
    {
        Task<AppUser> Authenticate(Google.Apis.Auth.GoogleJsonWebSignature.Payload payload);
        Task<IActionResult> Google([FromBody] GoogleLoginRequest userView);
        Task<IActionResult> GoogleUpdateInfo([FromBody] GoogleLoginRequest userView);

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
        public async Task<AppUser> Authenticate(Google.Apis.Auth.GoogleJsonWebSignature.Payload payload)
        {
            await Task.Delay(1);
            return await this.FindUserOrAdd(payload);
        }

        private async Task<AppUser> FindUserOrAdd(Google.Apis.Auth.GoogleJsonWebSignature.Payload payload)
        {
            return await _userManager.FindByEmailAsync(payload.Email);
            
            
        }

        public async Task<IActionResult> Google([FromBody] GoogleLoginRequest userView)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(userView.TokenId, new GoogleJsonWebSignature.ValidationSettings()).Result;
                var user = await Authenticate(payload);

                if (user == null)
                {
                    return new NotFoundObjectResult(new { message = "You need to update your information before proceed", googleName = payload.Name, email = payload.Email });
                }

                var token = _jwtGenerator.VerifyAndReturnToken(user);

                return new OkObjectResult(token);
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("Expired"))
                {
                    var internalErr = new ObjectResult(new { type = "0", error = ex.Message })
                    {
                        StatusCode = 500
                    };
                    return internalErr;
                }
                else
                {
                    var internalErr = new ObjectResult(new { type = "1", error = ex.Message })
                    {
                        StatusCode = 500
                    };
                    return internalErr;
                }
            }

        }


        public async Task<IActionResult> GoogleUpdateInfo([FromBody] GoogleLoginRequest userView)
        {
            var user = new AppUser
            {
                UserName = userView.UserName,
                Email = userView.Email,
                RealName = userView.RealName,
                DOB = userView.DOB
            };

            var appUser = await _userManager.FindByNameAsync(userView.UserName);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = "1", message = "Username already exists" });
            }

            var result = await _userManager.CreateAsync(user);
            if (result.Succeeded)
            {
                appUser = await _userManager.FindByEmailAsync(userView.Email);

                var token = _jwtGenerator.VerifyAndReturnToken(appUser);

                return  new OkObjectResult(token);
            }

            return new NotFoundObjectResult("");
        }



    }
}
