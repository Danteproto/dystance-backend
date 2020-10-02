using BackEnd.Context;
using BackEnd.Models;
using BackEnd.Interfaces;
using BackEnd.Security;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using BackEnd.Ultilities;
using BackEnd.Responses;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using EmailService;
using BackEnd.Requests;
using BackEnd.Stores;
using System.IO;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

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
        public Task<IActionResult> ResetPasswordSend(ResetPasswordRequest model);
        public Task<IActionResult> ResetPasswordVerify(ResetPasswordVerify model);
        public Task<IActionResult> ResetPasswordUpdate(ResetPasswordUpdate model);
        public Task<IActionResult> UpdateProfile(UpdateProfileRequest model);
        public FileStream getAvatar(string realName, string userName, string fileName);

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
        private readonly IUserStore _userStore;
        private readonly IWebHostEnvironment _env;

        public UserService(
            UserDbContext context,
            IMapper mapper,
            SignInManager<AppUser> signInManager,
            UserManager<AppUser> userManager,
            IUrlHelperFactory urlHelperFactory,
            IActionContextAccessor actionContextAccessor,
            IEmailSender emailSender,
            IJwtGenerator jwtGenerator,
            IUserAccessor userAccessor,
            IUserStore userStore,
            IWebHostEnvironment env)
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
            _userStore = userStore;
            _env = env;
        }

        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            AppUser appUser = null;
            if (String.IsNullOrWhiteSpace(model.UserName))
            {
                if (EmailUtil.CheckIfValid(model.Email))
                {
                    appUser = await _userManager.FindByEmailAsync(model.Email);
                }
                else
                {
                    return new BadRequestObjectResult(new { type = 0, message = "Email not found!" });
                }
            }
            else
            {
                appUser = await _userManager.FindByNameAsync(model.UserName);
            }
            // return null if user not found
            if (appUser == null)
            {
                return new BadRequestObjectResult(new { type = 2, message = "Username not found!" });
            }

            if (!appUser.EmailConfirmed)
            {
                return new BadRequestObjectResult(new { type = 1, message = "You must confirm your email before login" });
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
                return new BadRequestObjectResult(new { type = 4, message = "Password is not correct" });
            }
        }

        public IActionResult RefreshToken(string token)
        {
            try
            {
                var appUser = _context.Users.SingleOrDefault(u => u.RefreshTokens.Any(t => t.Token == token));

                // return 500 if no user found with token
                if (appUser == null) return new ObjectResult(new { message = "Token not found" })
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
                Dob = user.DOB,
                Avatar = $"api/users/getAvatar?fileName={user.Avatar}&realName=&userName={user.UserName}",
                UserName = user.UserName
            };
            return new OkObjectResult(response);
        }


        public async Task<IActionResult> Register(RegisterRequest userModel)
        {

            var appUser = await _userManager.FindByNameAsync(userModel.UserName);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = 1, message = "Username already exists" });
            }

            appUser = await _userManager.FindByEmailAsync(userModel.Email);
            if (appUser != null)
            {
                return new BadRequestObjectResult(new { type = 0, message = "Email already exists" });
            }

            string imgPath;
            string imgName = "";
            string extension = "";
            IFormFile img = null;
            //if avatar is empty, use default
            if (userModel.Avatar != null)
            {
                img = userModel.Avatar;
                extension = Path.GetExtension(img.FileName);

                imgName = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                    );
                var path = Path.Combine(_env.ContentRootPath, $"Files/Users/{userModel.UserName}/Images");
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

            var registerUser = new AppUser
            {
                Email = userModel.Email,
                UserName = userModel.UserName,
                RealName = userModel.RealName,
                DOB = userModel.Dob,
                Avatar = imgName + extension
            };

            var result = await _userManager.CreateAsync(registerUser, userModel.Password);
            if (!result.Succeeded)
            {
                var internalErr = new ObjectResult(new { type = 2, error = result.Errors.ToList()[0].Description })
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
                Email = registerUser.Email,
                UserName = registerUser.UserName,
                Avatar = $"api/users/getAvatar?fileName={imgName + extension}&realName={Path.GetFileName(img.FileName)}&userName={userModel.UserName}",
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
                var internalErr = new ObjectResult(new { type = 0, error = "User not found" })
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

        public async Task<IActionResult> ResetPasswordSend(ResetPasswordRequest model)
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
                return new BadRequestObjectResult(new { type = 0, message = "User does not exist" });
            }
            if (String.IsNullOrEmpty(user.PasswordHash))
            {
                return new BadRequestObjectResult(new { type = 2, message = "Google accounts are not allowed" });
            }
            //Generate token 
            //var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
            //var token = await _userManager.GenerateUserTokenAsync(user, "ResetPasswordTokenProvider", "PasswordReset");
            var token = _userStore.GenerateTokenAndSave(user.Email);

            //Send email
            //var resetLink = urlHelper.Action("ResetPasswordHandler", "Users", new { token, email = user.Email }, "https");
            var content = "Your token: " + token;
            var message = new Message(new string[] { user.Email }, "Reset password at Dystance", content, null);
            await _emailSender.SendEmailAsync(message);

            return new OkObjectResult(new { email = user.Email, message = "Email sent successfully" });
        }

        public async Task<IActionResult> ResetPasswordVerify(ResetPasswordVerify model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new BadRequestObjectResult(new { type = 0, message = "User does not exist" });
            }
            if (String.IsNullOrEmpty(user.PasswordHash))
            {
                return new BadRequestObjectResult(new { type = 2, message = "Google accounts are not allowed" });
            }

            //var result = await _userManager.VerifyUserTokenAsync(user, _userManager.Options.Tokens.PasswordResetTokenProvider, "PasswordReset", model.Token);
            var result = _userStore.IsTokenValid(model.Token);

            if (result)
            {
                return new OkObjectResult("");
            }

            return new BadRequestObjectResult(new { type = 1, message = "Wrong token or token expired" });
        }


        public async Task<IActionResult> ResetPasswordUpdate(ResetPasswordUpdate model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                return new BadRequestObjectResult(new { type = 0, message = "User does not exist" });
            }
            if (_userStore.IsResetPasswordTokenVerified)
            {
                _userStore.IsResetPasswordTokenVerified = false;
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, model.NewPassword);

                if (result.Succeeded)
                {
                    return new OkObjectResult("");
                }
            }
            return new BadRequestObjectResult(new { type = 1, message = "Token not verified or generated" });


        }

        public async Task<IActionResult> UpdateProfile(UpdateProfileRequest model)
        {

            var user = await _userManager.FindByIdAsync(_userAccessor.GetCurrentUserId());
            try
            {
                //Update Profile
                user.RealName = model.RealName ?? user.RealName;
                user.DOB = model.Dob ?? user.DOB;

                string imgPath;
                string imgName = "";
                string extension = "";

                IFormFile img = null;
                //if avatar is empty, use default
                if (model.Avatar != null)
                {
                    img = model.Avatar;
                    extension = Path.GetExtension(img.FileName);

                    imgName = Convert.ToBase64String(
                            System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                        );
                    var path = Path.Combine(_env.ContentRootPath, $"Files/Users/{user.UserName}/Images");
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
                    user.Avatar = imgName + extension;
                }


                var resultUpdate = await _userManager.UpdateAsync(user);


                if (!string.IsNullOrEmpty(model.NewPassword))
                {
                    var resultCheckpass = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
                    if (resultCheckpass)
                    {
                        //Change password
                        var resultPass = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);


                        if (resultPass.Succeeded)
                        {
                            user = await _userManager.FindByIdAsync(user.Id);
                            return new OkObjectResult(
                                    new UserInfoResponse
                                    {
                                        Id = user.Id,
                                        UserName = user.UserName,
                                        RealName = user.RealName,
                                        Email = user.Email,
                                        Dob = user.DOB,
                                        Avatar = $"api/users/getAvatar?fileName={user.Avatar}&realName={Path.GetFileName(user.Avatar)}&userName={user.UserName}",

                                    }
                            );
                        }
                        else
                        {

                            return new ObjectResult(new { type = 1, code = resultPass.Errors.ToList()[0].Code, description = resultPass.Errors.ToList()[0].Description })
                            {
                                StatusCode = 500
                            };
                        }
                    }
                    else
                    {

                        return new ObjectResult(new { type = 0, message = "Password mismatched" })
                        {
                            StatusCode = 500
                        };
                    }
                }
                if (resultUpdate.Succeeded)
                {
                    user = await _userManager.FindByIdAsync(user.Id);
                    return new OkObjectResult(
                                     new UserInfoResponse
                                     {
                                         Id = user.Id,
                                         UserName = user.UserName,
                                         RealName = user.RealName,
                                         Email = user.Email,
                                         Dob = user.DOB,
                                         Avatar = $"api/users/getAvatar?fileName={user.Avatar}&realName={Path.GetFileName(user.Avatar)}&userName={user.UserName}",

                                     }
                             );
                }
                else
                {

                    return new ObjectResult(new { type = 2, code = resultUpdate.Errors.ToList()[0].Code, description = resultUpdate.Errors.ToList()[0].Description })
                    {
                        StatusCode = 500
                    };
                }


            }
            catch (Exception ex)
            {
                return new ObjectResult(new { type = 3, message = ex.Message })
                {
                    StatusCode = 500
                };
            }
        }


        public FileStream getAvatar(string realName, string userName, string fileName)
        {
            string path;
            var rootPath = _env.ContentRootPath;
            path = fileName != "default.png" ? Path.Combine(rootPath, $"Files/Users/{userName}/Images") : Path.Combine(rootPath, $"Files/Users/Images");

            var filePath = Path.Combine(path, fileName);
            var file = File.OpenRead(filePath);

            return file;

        }
    }

}
