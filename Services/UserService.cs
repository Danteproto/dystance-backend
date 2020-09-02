using API.Context;
using API.Models;
using BackEnd.Errors;
using BackEnd.Interfaces;
using BackEnd.Models;
using BackEnd.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace BackEnd.Services
{
    public class UserService : IUserService
    {
        private readonly UserDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IJwtGenerator _jwtGenerator;

        public UserService(UserDbContext context, UserManager<AppUser> userManager, SignInManager<AppUser> signInManager, IJwtGenerator jwtGenerator)
        {
            _context = context;
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtGenerator = jwtGenerator;
        }


        public async Task<IActionResult> Register(RegisterModel model)
        {

            if (await _context.Users.Where(x => x.Email == model.Email).AnyAsync())
            {
                throw new RestException(HttpStatusCode.BadRequest, new { Email = "Email already exists" });
            }

            if (await _context.Users.Where(x => x.UserName == model.Username).AnyAsync())
            {
                throw new RestException(HttpStatusCode.BadRequest, new { Username = "Username already exists" });
            }

            var user = new AppUser
            {
                DisplayName = model.DisplayName,
                Email = model.Email,
                UserName = model.Username
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var authenUser = new AuthenticatedUser
                {
                    DisplayName = user.DisplayName,
                    Token = _jwtGenerator.CreateToken(user),
                    Username = user.UserName,
                    Image = null
                };

                return new OkObjectResult(authenUser);
            }

            throw new Exception("Problem creating users");
        }


        public async Task<IActionResult> Login(LoginModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Username);
            if (user != null && await _userManager.CheckPasswordAsync(user, model.Password))
            {

                return new OkObjectResult(user);
            }
            else
            {
                throw new RestException(HttpStatusCode.BadRequest, new { User = "Not found" });
            }

        }



    }
}
