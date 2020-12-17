using BackEnd.Errors;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Security;
using BackEnd.Stores;
using EmailService;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Ultilities;

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
        private readonly IUserStore _userStore;
        private readonly IWebHostEnvironment _env;

        public AuthService(UserManager<AppUser> userManager, IJwtGenerator jwtGenerator, IUserStore userStore, IWebHostEnvironment env)
        {
            _userManager = userManager;
            _jwtGenerator = jwtGenerator;
            _userStore = userStore;
            _env = env;
        }
        public async Task<AppUser> Authenticate(GoogleJsonWebSignature.Payload payload)
        {
            await Task.Delay(1);
            return await this.FindUserOrAdd(payload);
        }

        private async Task<AppUser> FindUserOrAdd(GoogleJsonWebSignature.Payload payload)
        {
            return await _userManager.FindByEmailAsync(payload.Email);
        }

        public async Task<IActionResult> Google(GoogleLoginRequest userView)
        {
            try
            {
                var payload = GoogleJsonWebSignature.ValidateAsync(userView.TokenId, new GoogleJsonWebSignature.ValidationSettings()).Result;
                var user = await _userManager.FindByEmailAsync(payload.Email);
                _userStore.TokenIdVerified = true;

                if (user == null)
                {
                    return new NotFoundObjectResult(new { type = 0 ,message = "Account doesn't exist", googleName = payload.Name, email = payload.Email });
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

            if (!_userStore.TokenIdVerified)
            {
                return new BadRequestObjectResult(new { type = 0, message = "Google token not verified" });
            }

            string imgPath;
            string imgName = "";
            string extension = "";
            IFormFile img = null;
            //if avatar is empty, use default
            if (userView.Avatar != null)
            {
                img = userView.Avatar;
                extension = Path.GetExtension(img.FileName);

                imgName = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                    );
                var path = Path.Combine(_env.ContentRootPath, $"Files/Users/{userView.UserName}/Images");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                imgPath = Path.Combine(path, imgName + extension);
                if (img.Length > 0)
                {
                    using var fileStream = new FileStream(imgPath, FileMode.Create);
                    img.CopyTo(fileStream);
                }
            }
            else
            {
                imgName = "default";
                extension = ".png";
                img = Extensions.GetDefaultAvatar(_env);
            }

            var user = new AppUser
            {
                UserName = userView.UserName,
                Email = userView.Email,
                RealName = userView.RealName,
                DOB = Convert.ToDateTime(userView.Dob),
                Avatar = imgName + extension
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
