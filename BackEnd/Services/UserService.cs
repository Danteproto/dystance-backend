using BackEnd.Context;
using BackEnd.Models;
using BackEnd.Errors;
using BackEnd.Interfaces;
using BackEnd.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using AutoMapper;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Diagnostics;
using BackEnd.Ultilities;
using BackEnd.Responses;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using EmailService;
using BackEnd.Requests;

namespace BackEnd.Services
{
    public interface IUserService
    {
        public Task<IActionResult> Authenticate(AuthenticateRequest model);
        IActionResult RefreshToken(string token);
        bool RevokeToken(string token, string ipAddress);
        IEnumerable<User> GetAll();
        IActionResult GetById(string id);

        public Task<IActionResult> Register(RegisterRequest model);
        public Task<IActionResult> ResendEmail(ResendEmailRequest req);
        public Task<string> ConfirmEmail(string token, string email);
        public Task<IActionResult> GetCurrentUser();
        public IEnumerable<AppUser> GetUsers();
        public Task<IActionResult> ResetPassword(ResetPasswordRequest model);
        public Task<string> ResetPasswordHandler(ResetPasswordModel model);
        public Task<IActionResult> ChangePassword(ChangePasswordRequest model);


    }

    public class UserService : IUserService
    {
        private UserDbContext _context;
        private readonly IMapper _mapper;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;
        private readonly IUrlHelperFactory _urlHelperFactory;
        private readonly IActionContextAccessor _actionContextAccessor;
        private readonly IEmailSender _emailSender;
        private readonly IJwtGenerator _jwtGenerator;
        private readonly IUserAccessor _userAccessor;
        public UserService(
            UserDbContext context,
            IMapper mapper,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEmailSender emailSender,
            IJwtGenerator jwtGenerator,
            IUserAccessor userAccessor)
        {
            _context = context;
            _mapper = mapper;
            _signInManager = signInManager;
            _userManager = userManager;
            _urlHelperFactory = urlHelperFactory;
            _actionContextAccessor = actionContextAccessor;
            _emailSender = emailSender;
            _jwtGenerator = jwtGenerator;
            _userAccessor = userAccessor;
        }

        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            AppUser appUser = null;
            if (String.IsNullOrWhiteSpace(model.Username))
            {
                if (EmailUtil.CheckIfValid(model.Email))
                {
                    appUser = await _userManager.FindByEmailAsync(model.Email);
                }
                else
                {
                    return new BadRequestObjectResult(new { type = "0", message = "Email not found!" });
                }
            }
            else
            {
                appUser = await _userManager.FindByNameAsync(model.Username);
            }
            // return null if user not found
            if (appUser == null)
            {
                return new BadRequestObjectResult(new { type = "0", message = "Username not found!" });
            }

            if (!appUser.EmailConfirmed)
            {
                return new BadRequestObjectResult(new { type = "1", message = "You must confirm your email before login" });
            }

            var result = await _signInManager.CheckPasswordSignInAsync(appUser, model.Password, false);



            if (result.Succeeded)
            {
                // authentication successful so generate jwt and refresh tokens
                User user = _mapper.Map<User>(appUser);

                var jwtToken = _jwtGenerator.generateJwtToken(user);
                var refreshToken = _jwtGenerator.generateRefreshToken();

                // save refresh token
                appUser.RefreshTokens.Add(refreshToken);
                _context.Update(appUser);
                _context.SaveChanges();

                var response = new AuthenticateResponse(user, jwtToken.token, refreshToken.Token, _jwtGenerator.toSeconds(DateTime.Parse(jwtToken.ExpireDate), DateTime.Parse(jwtToken.StartDate)));

                return new OkObjectResult(response);
            }
            else
            {
                return new BadRequestObjectResult(new { type = "0", message = "Password is not correct" });
            }
        }

        public IActionResult RefreshToken(string token)
        {
            try
            {
                var appUser = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

                // return 500 if no user found with token
                if (appUser == null) return new ObjectResult(new { message = "Token not found"})
                {
                    StatusCode = 500
                };


                var refreshToken = appUser.RefreshTokens.Single(x => x.Token == token);

                // return 500 if token is no longer active
                if (!refreshToken.IsActive) return new ObjectResult(new { message = "Token is no longer active" })
                {
                    StatusCode = 500
                };

                // replace old refresh token with a new one and save
                var newRefreshToken = _jwtGenerator.generateRefreshToken();
                refreshToken.Revoked = DateTime.UtcNow;
                //refreshToken.RevokedByIp = ipAddress;
                refreshToken.ReplacedByToken = newRefreshToken.Token;
                appUser.RefreshTokens.Add(newRefreshToken);
                _context.Update(appUser);
                _context.SaveChanges();

                // generate new jwt
                User user = _mapper.Map<User>(appUser);
                var jwtToken = _jwtGenerator.generateJwtToken(user);

                return new OkObjectResult(new AuthenticateResponse(user, jwtToken.token, newRefreshToken.Token, _jwtGenerator.toSeconds(DateTime.Parse(jwtToken.ExpireDate), DateTime.Parse(jwtToken.StartDate))));
            }
            catch (Exception ex)
            {
                return new UnauthorizedObjectResult(new { message = ex.Message });
            }
        }

        public bool RevokeToken(string token, string ipAddress)
        {
            var appUser = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

            // return false if no user found with token
            if (appUser == null) return false;

            var refreshToken = appUser.RefreshTokens.Single(x => x.Token == token);

            // return false if token is not active
            if (!refreshToken.IsActive) return false;

            // revoke token and save
            refreshToken.Revoked = DateTime.UtcNow;
            refreshToken.RevokedByIp = ipAddress;
            _context.Update(appUser);
            _context.SaveChanges();

            return true;
        }

        public IEnumerable<User> GetAll()
        {
            return _mapper.Map<List<AppUser>, List<User>>(_context.Users.ToList());

        }

        public IActionResult GetById(string id)
        {
            var user = _context.Users.Find(id);
            if (user == null)
            {
                return new NotFoundObjectResult("");
            }

            var response = new UserInfoResponse
            {
                Id = user.Id,
                RealName = user.RealName,
                Email = user.Email,
                DOB = user.DOB,
                Avatar = ""
            };
            return new OkObjectResult(response);
        }


        public async Task<IActionResult> Register(RegisterRequest userModel)
        {

            var appUser = await _userManager.FindByNameAsync(userModel.Username);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = "1", message = "Username already exists" });
            }

            appUser = await _userManager.FindByEmailAsync(userModel.Email);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = "0", message = "Email already exists" });
            }

            var registerUser = new AppUser
            {
                Email = userModel.Email,
                UserName = userModel.Username,
                RealName = userModel.RealName,
                DOB = userModel.DOB
            };

            var result = await _userManager.CreateAsync(registerUser, userModel.Password);
            if (!result.Succeeded)
            {
                var internalErr = new ObjectResult(new { error = result.Errors.ToList()[0].Description })
                {
                    StatusCode = 500
                };
                return internalErr;
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(registerUser);
            var confirmationLink = urlHelper.Action("ConfirmEmail", "Users", new { token, email = registerUser.Email }, "https");
            var message = new Message(new string[] { registerUser.Email }, "Confirmation email link", confirmationLink, null);
            await _emailSender.SendEmailAsync(message);
            //await _userManager.AddToRoleAsync(user, "Visitor");


            return new OkObjectResult(new RegisterResponse
            {
                Email = userModel.Email,
                UserName = userModel.Username,
                Token = token,
                TokenLink = confirmationLink
            });

        }

        public async Task<IActionResult> ResendEmail(ResendEmailRequest req)
        {
            AppUser user;
            if (!String.IsNullOrEmpty(req.Email))
            {
                user = await _userManager.FindByEmailAsync(req.Email);
            }
            else
            {
                user = await _userManager.FindByNameAsync(req.UserName);
            }


            if (user == null)
            {
                var internalErr = new ObjectResult(new { error = "User not found" })
                {
                    StatusCode = 500
                };
                return internalErr;
            }

            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = urlHelper.Action("ConfirmEmail", "Users", new { token, email = user.Email }, "https");
            var message = new Message(new string[] { user.Email }, "Confirmation email link", confirmationLink, null);
            await _emailSender.SendEmailAsync(message);

            return new OkObjectResult(new { message = "Successful" });
        }


        public async Task<string> ConfirmEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null)
                return "User not found";

            var result = await _userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                return "Email Confirmed";
            }
            else
            {
                return "Error";
            }

        }

        public async Task<IActionResult> GetCurrentUser()
        {
            var appUser = await _userManager.FindByIdAsync(_userAccessor.GetCurrentUserId());

            if (appUser != null)
            {
                return new OkObjectResult(new
                {
                    appUser.RealName,
                    appUser.DOB,
                    appUser.Email,
                    appUser.UserName
                });
            }

            return new NotFoundObjectResult("");
        }

        public IEnumerable<AppUser> GetUsers()
        {
            return _context.Users;
        }


        public async Task<IActionResult> ResetPassword(ResetPasswordRequest model)
        {
            AppUser user;
            //Find the user by email or by username
            if (!String.IsNullOrEmpty(model.Email))
            {
                user = await _userManager.FindByEmailAsync(model.Email);
            }
            else
            {
                user = await _userManager.FindByNameAsync(model.UserName);
            }

            if (user == null)
            {
                return new BadRequestObjectResult(new { type = "0", message = "Missing fields" });
            }

            //Generate url 
            var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);


            //Send email
            var resetLink = urlHelper.Action("ResetPasswordHandler", "Users", new { token, email = user.Email }, "https");
            var message = new Message(new string[] { user.Email }, "Reset password link", resetLink, null);
            await _emailSender.SendEmailAsync(message);

            return new OkObjectResult("Email sent successfully");
        }

        public async Task<string> ResetPasswordHandler(ResetPasswordModel model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return "User not found.";
            }

            var resetPassResult = await _userManager.ResetPasswordAsync(user, model.Token, model.Password);

            if (!resetPassResult.Succeeded)
            {
                return "Error changing password!";
            }

            return "Password reset successfully";
        }

        public async Task<IActionResult> ChangePassword(ChangePasswordRequest model)
        {

            var user = await _userManager.FindByIdAsync(_userAccessor.GetCurrentUserId());

            //Change password
            var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);

            if (result.Succeeded)
            {
                return new OkObjectResult(new { message = "Password changed successfully" });
            }
            else
            {
                return new ObjectResult(new { type = "0", code = result.Errors.ToList()[0].Code, description = result.Errors.ToList()[0].Description })
                {
                    StatusCode = 500
                };
            }
        }


    }
}
