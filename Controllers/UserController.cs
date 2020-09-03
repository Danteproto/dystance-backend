using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Context;
using API.Models;
using BackEnd.Errors;
using BackEnd.Interfaces;
using BackEnd.Models;
using BackEnd.Security;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly UserDbContext _context;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IConfiguration _configuration;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IUserService _userService;

        public UserController(UserDbContext context, SignInManager<AppUser> signInManager, UserManager<AppUser> userManager, IConfiguration configuration, IUserService userService, IJwtGenerator jwtGenerator)
        {
            _context = context;
            _signInManager = signInManager;
            _userManager = userManager;
            _configuration = configuration;
            _jwtGenerator = jwtGenerator;



            _userService = new UserService(_context, _userManager, _signInManager, _jwtGenerator);
        }

        [HttpPost]
        [Route("login")]
        [AllowAnonymous]
        public Task<IActionResult> Login([FromBody] LoginModel model)
        {

            return _userService.Login(model);
        }

        [HttpPost]
        [Route("register")]
        [AllowAnonymous]
        public Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            return _userService.Register(model);
        }


    }
}
