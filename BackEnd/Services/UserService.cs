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
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using static BackEnd.Constant.Log;
using BackEnd.DAO;
using System.Text;
using System.Text.RegularExpressions;
using ExcelDataReader;
using BackEnd.DBContext;
using BackEnd.Constant;
using OfficeOpenXml;
using OfficeOpenXml.Style;

namespace BackEnd.Services
{
    public interface IUserService
    {
        public enum MessageType
        {
            Text,
            Image,
            File
        }

        public Task<IActionResult> Authenticate(AuthenticateRequest model);
        IActionResult RefreshToken(string token);
        bool RevokeToken(string token, string ipAddress);
        Task<IEnumerable<UserInfoResponse>> GetAll();
        Task<IActionResult> GetById(string id);
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
        public Task<IActionResult> PrivateMessage(HttpRequest request);
        public List<PrivateMessage> GetPrivateMessage(string id1, string id2);
        public List<PrivateMessage> GetPreview(string id);
        public PrivateMessage GetLastPm(string id1, string id2);
        public FileStream GetPMFile(string userId, string fileName, int type);
        public Task<IActionResult> Log(LogRequest model);
        public Task<IActionResult> GetLogByRoom(string roomid);
        public Task<IActionResult> AddAccount(HttpRequest request);
        public Task<IActionResult> GetAttendanceReports(string userId, string semesterId);
        public Task<IActionResult> UpdateAttendanceReports(UpdateAttendanceStudentRequest model);
        public Task<FileInfo> ExportAttendance(string roomId);
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
        private readonly ILogDAO _logDAO;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly RoomDBContext _roomDBContext;
        private readonly IAttendanceDAO _attendanceDAO;

        public enum MessageType
        {
            Text,
            Image,
            File
        }
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
            IWebHostEnvironment env,
            ILogDAO logDAO,
            RoleManager<AppRole> roleManager,
            RoomDBContext roomDBContext,
            IAttendanceDAO attendanceDAO)
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
            _logDAO = logDAO;
            _roleManager = roleManager;
            _roomDBContext = roomDBContext;
            _attendanceDAO = attendanceDAO;
        }

        public async Task<IActionResult> Authenticate(AuthenticateRequest model)
        {
            AppUser appUser = null;
            if (String.IsNullOrWhiteSpace(model.UserName))
            {
                if (EmailUtil.CheckIfValid(model.Email))
                {
                    appUser = await _userManager.FindByEmailAsync(model.Email);
                    if (appUser == null)
                    {
                        return new BadRequestObjectResult(new { type = 0, message = "Email not found!" });
                    }
                }
                else
                {
                    return new BadRequestObjectResult(new { type = 3, message = "Wrong format email!" });
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


        public async Task<IEnumerable<UserInfoResponse>> GetAll()
        {
            //return _context.Users;
            var users = _userManager.Users.ToList();
            var returnList = new List<UserInfoResponse>();
            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                returnList.Add(new UserInfoResponse
                {
                    Id = user.Id,
                    RealName = user.RealName,
                    UserName = user.UserName,
                    Avatar = $"api/users/getAvatar?fileName={user.Avatar}&realName=&userName={user.UserName}",
                    Dob = user.DOB.ToString("yyyy-MM-dd"),
                    Email = user.Email,
                    Role = roles.Any() ? roles[0] : ""
                });
            }

            return returnList;
        }

        public async Task<IActionResult> GetById(string id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return new NotFoundObjectResult("");
            }

            var roles = await _userManager.GetRolesAsync(user);

            var response = new UserInfoResponse
            {
                Id = user.Id,
                RealName = user.RealName,
                Email = user.Email,
                Dob = user.DOB.ToString("yyyy-MM-dd"),
                Avatar = $"api/users/getAvatar?fileName={user.Avatar}&realName=&userName={user.UserName}",
                UserName = user.UserName,
                Role = roles[0]
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
                DOB = Convert.ToDateTime(userModel.Dob),
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
            var content = String.Format(EmailTemplate.HTML_CONTENT, registerUser.Email, registerUser.UserName, "123@123a", confirmationLink);

            var message = new Message(new string[] { registerUser.Email }, "Your Account On DYSTANCE", content, null);
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
            var content = String.Format(EmailTemplate.HTML_CONTENT, user.Email, user.UserName, "123@123a", confirmationLink);

            var message = new Message(new string[] { user.Email }, "Your Account On DYSTANCE", content, null);
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

            var roles = await _userManager.GetRolesAsync(appUser);

            if (appUser != null)
            {
                var response = new UserInfoResponse
                {
                    Id = appUser.Id,
                    RealName = appUser.RealName,
                    Email = appUser.Email,
                    Dob = appUser.DOB.ToString("yyyy-MM-dd"),
                    Avatar = $"api/users/getAvatar?fileName={appUser.Avatar}&realName=&userName={appUser.UserName}",
                    UserName = appUser.UserName,
                    Role = roles[0]
                };

                return new OkObjectResult(response);
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
            var roles = await _userManager.GetRolesAsync(user);
            try
            {
                //Update Profile
                user.RealName = model.RealName ?? user.RealName;
                if (!String.IsNullOrEmpty(model.Dob))
                {
                    user.DOB = Convert.ToDateTime(model.Dob);
                }

                string imgPath;
                string imgName = "";
                string extension = "";

                IFormFile img = null;
                //if avatar is empty, use default
                if (model.Avatar != null)
                {
                    deleteAvatar(user.UserName, user.Avatar);
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
                                        Dob = user.DOB.ToString("yyyy-MM-dd"),
                                        Avatar = $"api/users/getAvatar?fileName={user.Avatar}&realName={Path.GetFileName(user.Avatar)}&userName={user.UserName}",
                                        Role = roles[0]
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
                                         Dob = user.DOB.ToString("yyyy-MM-dd"),
                                         Avatar = $"api/users/getAvatar?fileName={user.Avatar}&realName={Path.GetFileName(user.Avatar)}&userName={user.UserName}",
                                         Role = roles[0]
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

        public void deleteAvatar(string userName, string fileName)
        {
            if (fileName != "default.png")
            {
                string path;
                var rootPath = _env.ContentRootPath;
                path = Path.Combine(rootPath, $"Files/Users/{userName}/Images");
                var filePath = Path.Combine(path, fileName);
                File.Delete(filePath);
            }
        }

        public async Task<IActionResult> PrivateMessage(HttpRequest request)
        {
            var type = (MessageType)Convert.ToInt32(request.Form["chatType"]);
            try
            {
                switch (type)
                {
                    case MessageType.Text:
                        {
                            var pm = new PrivateMessage
                            {
                                SenderId = request.Form["senderId"],
                                ReceiverId = request.Form["receiverId"],
                                Date = DateTime.Now,
                                Content = request.Form["content"],
                                Type = (int)type
                            };
                            await _context.PrivateMessages.AddAsync(pm);
                            break;
                        }
                    case MessageType.Image:
                        {
                            var img = request.Form.Files[0];
                            var extension = Path.GetExtension(img.FileName);

                            var imgName = Convert.ToBase64String(
                                    System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                                );
                            var path = Path.Combine(_env.ContentRootPath, $"Files/Users/{request.Form["senderId"]}/PM/Images");
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }

                            var imgPath = Path.Combine(path, imgName + extension);
                            if (img.Length > 0)
                            {
                                using var fileStream = new FileStream(imgPath, FileMode.Create);
                                img.CopyTo(fileStream);
                            }
                            var pm = new PrivateMessage
                            {
                                SenderId = request.Form["senderId"],
                                ReceiverId = request.Form["receiverId"],
                                Date = DateTime.Now,
                                Content = $"api/users/chat/getFile?id={request.Form["senderId"]}&fileName={imgName + extension}&type={(int)type}&realName={Path.GetFileName(img.FileName)}",
                                Type = (int)type,
                                FileName = Path.GetFileName(img.FileName)
                            };
                            await _context.PrivateMessages.AddAsync(pm);
                            break;
                        }
                    case MessageType.File:
                        {
                            var file = request.Form.Files[0];
                            var extension = Path.GetExtension(file.FileName);
                            var fileName = Convert.ToBase64String(
                                    System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                                );
                            var path = Path.Combine(_env.ContentRootPath, $"Files/Users/{request.Form["senderId"]}/PM/Files");
                            if (!Directory.Exists(path))
                            {
                                Directory.CreateDirectory(path);
                            }
                            var filePath = Path.Combine(path, fileName + extension);
                            if (file.Length > 0)
                            {
                                using var fileStream = new FileStream(filePath, FileMode.Create);
                                file.CopyTo(fileStream);
                            }
                            var pm = new PrivateMessage
                            {
                                SenderId = request.Form["senderId"],
                                ReceiverId = request.Form["receiverId"],
                                Date = DateTime.Now,
                                Content = $"api/users/chat/getFile?id={request.Form["senderId"]}&fileName={fileName + extension}&type={(int)type}&realName={Path.GetFileName(file.FileName)}",
                                Type = (int)type,
                                FileName = Path.GetFileName(file.FileName)
                            };
                            await _context.PrivateMessages.AddAsync(pm);
                            break;
                        }
                }
                _context.SaveChanges();
                return new OkObjectResult(new { message = "successful" });
            }
            catch (Exception e)
            {
                return new ObjectResult(new { message = "add message fail" })
                {
                    StatusCode = 500
                };
            }
        }

        public List<PrivateMessage> GetPreview(string id)
        {
            List<string> recIds = _context.PrivateMessages.Where(pm => pm.SenderId == id).Select(pm => pm.ReceiverId).ToList();
            List<string> senIds = _context.PrivateMessages.Where(pm => pm.ReceiverId == id).Select(pm => pm.SenderId).ToList();
            List<string> ids = recIds.Concat(senIds).Distinct().ToList();
            return ids.Select(item => GetPrivateMessage(item, id).Last()).ToList();
        }

        public List<PrivateMessage> GetPrivateMessage(string id1, string id2)
        {
            return _context.PrivateMessages
                .Where(pm => (pm.SenderId == id1 && pm.ReceiverId == id2) || (pm.SenderId == id2 && pm.ReceiverId == id1))
                .OrderBy(pm => pm.Date)
                .ToList();

        }

        public FileStream GetPMFile(string userId, string fileName, int type)
        {
            var rootPath = _env.ContentRootPath;
            string path = "";
            if (type == (int)MessageType.Image)
            {
                path = Path.Combine(rootPath, $"Files/Users/{userId}/PM/Images");
            }
            else if (type == (int)MessageType.File)
            {
                path = Path.Combine(rootPath, $"Files/Users/{userId}/PM/Files");
            }
            var filePath = Path.Combine(path, fileName);
            return System.IO.File.OpenRead(filePath);
        }

        public PrivateMessage GetLastPm(string id1, string id2)
        {
            return _context.PrivateMessages
                .Where(pm => (pm.SenderId == id1 && pm.ReceiverId == id2) || (pm.SenderId == id2 && pm.ReceiverId == id1))
                .OrderBy(pm => pm.Date)
                .Last();
        }

        public async Task<IActionResult> Log(LogRequest model)
        {
            //var user = await _userManager.FindByIdAsync(model.UserId);
            var result = new StringBuilder();
            var file = model.Log;
            using (var stream = file.OpenReadStream())
            {
                using (var reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        string line = await reader.ReadLineAsync();
                        var words = new Regex("\\s+").Split(line);
                        string logType = words[1].Trim();
                        LogType type;
                        if (Enum.TryParse(logType, out type))
                            switch (type)
                            {
                                case LogType.ATTENDANCE_JOIN:
                                    UsersLog attendanceJoin = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Joined room"
                                    };
                                    if (CheckLogExist(attendanceJoin))
                                    {
                                        var schedules = TimetableDAO.GetByRoomAndDate(_roomDBContext, Convert.ToInt32(attendanceJoin.RoomId), attendanceJoin.DateTime);
                                        foreach (var schedule in schedules)
                                        {
                                            var attendances = new List<AttendanceReports>();
                                            var attendance = _attendanceDAO.GetAttendanceByScheduleUserId(schedule.Id, attendanceJoin.UserId);
                                            if (attendance != null)
                                            {
                                                if (attendanceJoin.DateTime.TimeOfDay < schedule.StartTime.Add(TimeSpan.FromMinutes(10)))
                                                {
                                                    attendance.Status = "present";
                                                    attendances.Add(attendance);
                                                }
                                                else
                                                {
                                                    if (attendance.Status != "present")
                                                    {
                                                        attendance.Status = "absent";
                                                        attendances.Add(attendance);
                                                    }
                                                }
                                            }
                                            await _attendanceDAO.UpdateAttendance(attendances);
                                        }
                                        result.Append(await _logDAO.CreateLog(attendanceJoin) + '\n');
                                    }
                                    break;
                                case LogType.ATTENDANCE_LEAVE:
                                    UsersLog attendanceLeave = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Left room"
                                    };
                                    if (CheckLogExist(attendanceLeave))
                                    {
                                        var schedules = TimetableDAO.GetByRoomAndDate(_roomDBContext, Convert.ToInt32(attendanceLeave.RoomId), attendanceLeave.DateTime);
                                        foreach (var schedule in schedules)
                                        {
                                            var attendances = new List<AttendanceReports>();
                                            if (attendanceLeave.DateTime.TimeOfDay < schedule.EndTime.Add(TimeSpan.FromMinutes(-10)))
                                            {
                                                var attendance = _attendanceDAO.GetAttendanceByScheduleUserId(schedule.Id, attendanceLeave.UserId);
                                                if (attendance != null)
                                                {
                                                    attendance.Status = "absent";
                                                    attendances.Add(attendance);
                                                }
                                            }
                                            await _attendanceDAO.UpdateAttendance(attendances);
                                        }
                                        result.Append(await _logDAO.CreateLog(attendanceLeave) + '\n');
                                    }
                                    break;
                                case LogType.ROOM_CHAT_TEXT:
                                    UsersLog roomChatText = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Sent message"
                                    };
                                    if (CheckLogExist(roomChatText))
                                    {
                                        result.Append(await _logDAO.CreateLog(roomChatText) + '\n');
                                    }
                                    break;
                                case LogType.ROOM_CHAT_IMAGE:
                                    UsersLog roomChatImage = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Sent image " + words[words.Length - 1]
                                    };
                                    if (CheckLogExist(roomChatImage))
                                    {
                                        result.Append(await _logDAO.CreateLog(roomChatImage) + '\n');
                                    }
                                    break;
                                case LogType.ROOM_CHAT_FILE:
                                    UsersLog roomChatFile = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Sent file " + words[words.Length - 1]
                                    };
                                    if (CheckLogExist(roomChatFile))
                                    {
                                        result.Append(await _logDAO.CreateLog(roomChatFile) + '\n');
                                    }
                                    break;
                                case LogType.WHITEBOARD_ALLOW:
                                    UsersLog whiteboardAllow = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Gained whiteboard permissions"
                                    };
                                    if (CheckLogExist(whiteboardAllow))
                                    {
                                        result.Append(await _logDAO.CreateLog(whiteboardAllow) + '\n');
                                    }
                                    break;
                                case LogType.WHITEBOARD_DISABLE:
                                    UsersLog whiteboardDisable = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Lost whiteboard permissions"
                                    };
                                    if (CheckLogExist(whiteboardDisable))
                                    {
                                        result.Append(await _logDAO.CreateLog(whiteboardDisable) + '\n');
                                    }
                                    break;
                                case LogType.GROUP_CREATE:
                                    UsersLog groupCreate = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Created " + words[words.Length - 2] + " groups"
                                    };
                                    if (CheckLogExist(groupCreate))
                                    {
                                        result.Append(await _logDAO.CreateLog(groupCreate) + '\n');
                                    }
                                    break;
                                case LogType.GROUP_DELETE:
                                    UsersLog groupDelete = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Deleted " + words[words.Length - 2] + " groups"
                                    };
                                    if (CheckLogExist(groupDelete))
                                    {
                                        result.Append(await _logDAO.CreateLog(groupDelete) + '\n');
                                    }
                                    break;
                                case LogType.GROUP_START:
                                    UsersLog groupStart = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Started groups"
                                    };
                                    if (CheckLogExist(groupStart))
                                    {
                                        result.Append(await _logDAO.CreateLog(groupStart) + '\n');
                                    }
                                    break;
                                case LogType.GROUP_STOP:
                                    UsersLog groupStop = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Stopped groups"
                                    };
                                    if (CheckLogExist(groupStop))
                                    {
                                        result.Append(await _logDAO.CreateLog(groupStop) + '\n');
                                    }
                                    break;
                                case LogType.GOT_KICKED:
                                    UsersLog gotKicked = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Got kicked"
                                    };
                                    if (CheckLogExist(gotKicked))
                                    {
                                        result.Append(await _logDAO.CreateLog(gotKicked) + '\n');
                                    }
                                    break;
                                case LogType.GOT_MUTED:
                                    UsersLog gotMuted = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Got mute"
                                    };
                                    if (CheckLogExist(gotMuted))
                                    {
                                        result.Append(await _logDAO.CreateLog(gotMuted) + '\n');
                                    }
                                    break;
                                case LogType.KICK:
                                    UsersLog kick = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Kicked user " + new Regex("(?<=user).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(kick))
                                    {
                                        result.Append(await _logDAO.CreateLog(kick) + '\n');
                                    }
                                    break;
                                case LogType.MUTE:
                                    UsersLog mute = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Muted user " + new Regex("(?<=user).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(mute))
                                    {
                                        result.Append(await _logDAO.CreateLog(mute) + '\n');
                                    }
                                    break;
                                case LogType.TOGGLE_WHITEBOARD:
                                    UsersLog toggleWhiteboard = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Toggled whiteboard usage for " + new Regex("(?<=for).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(toggleWhiteboard))
                                    {
                                        result.Append(await _logDAO.CreateLog(toggleWhiteboard) + '\n');
                                    }
                                    break;
                                case LogType.REMOTE_CONTROL_PERMISSION:
                                    UsersLog remoteControlPermission = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Asked to remote control " + new Regex("(?<=control).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(remoteControlPermission))
                                    {
                                        result.Append(await _logDAO.CreateLog(remoteControlPermission) + '\n');
                                    }
                                    break;
                                case LogType.REMOTE_CONTROL_ACCEPT:
                                    UsersLog remoteControlAccept = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Accepted remote control request from " + new Regex("(?<=from).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(remoteControlAccept))
                                    {
                                        result.Append(await _logDAO.CreateLog(remoteControlAccept) + '\n');
                                    }
                                    break;
                                case LogType.REMOTE_CONTROL_REJECT:
                                    UsersLog remoteControlReject = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Rejected remote control request from " + new Regex("(?<=from).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(remoteControlReject))
                                    {
                                        result.Append(await _logDAO.CreateLog(remoteControlReject) + '\n');
                                    }
                                    break;
                                case LogType.REMOTE_CONTROL_STOP:
                                    UsersLog remoteControlStop = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Stopped remote-controlling " + new Regex("(?<=controlling).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(remoteControlStop))
                                    {
                                        result.Append(await _logDAO.CreateLog(remoteControlStop) + '\n');
                                    }
                                    break;
                                case LogType.GROUP_JOIN:
                                    UsersLog groupJoin = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Joined group " + new Regex("(?<=group).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(groupJoin))
                                    {
                                        result.Append(await _logDAO.CreateLog(groupJoin) + '\n');
                                    }
                                    break;
                                case LogType.GROUP_LEAVE:
                                    UsersLog groupLeave = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Left group " + new Regex("(?<=group).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(groupLeave))
                                    {
                                        result.Append(await _logDAO.CreateLog(groupLeave) + '\n');
                                    }
                                    break;
                                case LogType.PRIVATE_CHAT_TEXT:
                                    UsersLog privateChatText = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Sent message to " + new Regex("(?<=to).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(privateChatText))
                                    {
                                        result.Append(await _logDAO.CreateLog(privateChatText) + '\n');
                                    }
                                    break;
                                case LogType.PRIVATE_CHAT_IMAGE:
                                    UsersLog privateChatImage = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Sent image " + new Regex("(?<=image).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(privateChatImage))
                                    {
                                        result.Append(await _logDAO.CreateLog(privateChatImage) + '\n');
                                    }
                                    break;
                                case LogType.PRIVATE_CHAT_FILE:
                                    UsersLog privateChatFile = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Sent file " + new Regex("(?<=file).*").Match(line).ToString().Trim()
                                    };
                                    if (CheckLogExist(privateChatFile))
                                    {
                                        result.Append(await _logDAO.CreateLog(privateChatFile) + '\n');
                                    }
                                    break;
                                case LogType.SCREEN_SHARE_START:
                                    UsersLog screenShareStart = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Started screen sharing"
                                    };
                                    if (CheckLogExist(screenShareStart))
                                    {
                                        result.Append(await _logDAO.CreateLog(screenShareStart) + '\n');
                                    }
                                    break;
                                case LogType.SCREEN_SHARE_STOP:
                                    UsersLog screenShareStop = new UsersLog
                                    {
                                        DateTime = DateTimeUtil.GetDateTimeFromString(words[0]),
                                        LogType = words[1],
                                        RoomId = words[2],
                                        UserId = words[3],
                                        Description = "Stopped screen sharing"
                                    };
                                    if (CheckLogExist(screenShareStop))
                                    {
                                        result.Append(await _logDAO.CreateLog(screenShareStop) + '\n');
                                    }
                                    break;
                            }
                    }
                }
            }

            if (!result.ToString().Contains("Error"))
            {
                return new OkObjectResult(new { message = "Log Sent Successfully" });
            }
            else
            {
                return new BadRequestObjectResult(new { message = "Log sent successfully with some errors" });

            }

        }

        public async Task<IActionResult> GetLogByRoom(string roomid)
        {

            var roomLists = await _logDAO.GetLogsByRoomId(roomid);

            var list = new List<LogResponse>();


            foreach (var rooms in roomLists)
            {
                list.Add(new LogResponse()
                {
                    DateTime = String.Format("{0:s}", rooms.DateTime),
                    LogType = rooms.LogType,
                    RoomId = rooms.RoomId,
                    UserId = rooms.UserId,
                    Description = rooms.Description
                });
            }


            return new OkObjectResult(list);
        }

        public bool CheckLogExist(UsersLog model)
        {
            var logLists = (from logs in _context.UserLog
                            where logs.RoomId.Contains(model.RoomId) && logs.UserId.Contains(model.UserId)
                                  && logs.DateTime.CompareTo(model.DateTime) == 0 && logs.LogType.Contains(model.LogType)
                                  && logs.Description.Contains(model.Description)
                            select logs).ToList();

            if (logLists.Count != 0)
            {
                return false;
            }

            return true;
        }

        public async Task<IActionResult> AddAccount(HttpRequest request)
        {
            var response = new List<TeacherInfoResponse>();
            var dict = new Dictionary<String, object>();
            var errors = new List<Error>();
            var file = request.Form.Files[0];
            using (var stream = file.OpenReadStream())
            {
                using (var reader = ExcelReaderFactory.CreateReader(stream))
                {
                    do
                    {
                        while (reader.Read()) //Each row of the file
                        {
                            if (reader.Name == "Students") // "STUDENTS" SHEET
                            {
                                if (reader.GetValue(0).ToString() == "No" || reader.GetValue(0) == null)
                                {
                                    continue;
                                }
                                if (await _userManager.FindByNameAsync(reader.GetValue(1).ToString()) != null ||
                                   await _userManager.FindByEmailAsync(reader.GetValue(3).ToString()) != null)
                                {
                                    errors.Add(new Error
                                    {
                                        Type = 1,
                                        Message = "Email " + reader.GetValue(3).ToString() + " already exists",
                                    });
                                    continue;
                                }
                                if (await _userManager.FindByNameAsync(reader.GetValue(1).ToString()) != null)
                                {
                                    errors.Add(new Error
                                    {
                                        Type = 2,
                                        Message = "Student Code " + reader.GetValue(1).ToString() + " already exists",
                                    });
                                    continue;
                                }


                                var user = new AppUser
                                {
                                    UserName = reader.GetValue(1).ToString(),
                                    RealName = reader.GetValue(2).ToString(),
                                    Email = reader.GetValue(3).ToString(),
                                    DOB = Convert.ToDateTime(reader.GetValue(4).ToString()),
                                    Avatar = "default.png"
                                };


                                //Create user
                                var result = await _userManager.CreateAsync(user, "123@123a");

                                //Confirm email
                                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                var confirmationLink = urlHelper.Action("ConfirmEmail", "Users", new { token, email = user.Email }, "https");
                                var content = String.Format(EmailTemplate.HTML_CONTENT, reader.GetValue(3).ToString(), reader.GetValue(1).ToString(), "123@123a", confirmationLink);

                                var message = new Message(new string[] { user.Email }, "Your Account On DYSTANCE", content, null);
                                await _emailSender.SendEmailAsync(message);

                                //roleManager.AddUserToRole
                                await _userManager.AddToRoleAsync(user, "Student");

                                //debug
                                response.Add(new TeacherInfoResponse
                                {
                                    Id = user.Id,
                                    Code = user.UserName,
                                    RealName = user.RealName,
                                    Email = user.Email,
                                    Dob = user.DOB.ToString("yyyy-MM-dd")
                                });
                            }

                            else if (reader.Name == "Teachers") // "TEACHERS" SHEET
                            {
                                if (reader.GetValue(0).ToString() == "No" || reader.GetValue(0) == null)
                                {
                                    continue;
                                }

                                if (await _userManager.FindByEmailAsync(reader.GetValue(3).ToString()) != null)
                                {
                                    errors.Add(new Error
                                    {
                                        Type = 1,
                                        Message = "Email " + reader.GetValue(3).ToString() + " already exists",
                                    });
                                    continue;
                                }
                                if (await _userManager.FindByNameAsync(reader.GetValue(1).ToString()) != null)
                                {
                                    errors.Add(new Error
                                    {
                                        Type = 2,
                                        Message = "Employee Code " + reader.GetValue(1).ToString() + " already exists",
                                    });
                                    continue;
                                }

                                var user = new AppUser
                                {
                                    UserName = reader.GetValue(1).ToString(),
                                    RealName = reader.GetValue(2).ToString(),
                                    Email = reader.GetValue(3).ToString(),
                                    DOB = Convert.ToDateTime(reader.GetValue(4).ToString()),
                                    Avatar = "default.png"
                                };

                                //Create user
                                var result = await _userManager.CreateAsync(user, "123@123a");

                                //Confirm email
                                var urlHelper = _urlHelperFactory.GetUrlHelper(_actionContextAccessor.ActionContext);
                                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                                var confirmationLink = urlHelper.Action("ConfirmEmail", "Users", new { token, email = user.Email }, "https");
                                var content = String.Format(EmailTemplate.HTML_CONTENT, reader.GetValue(3).ToString(), reader.GetValue(1).ToString(), "123@123a", confirmationLink);

                                var message = new Message(new string[] { user.Email }, "Your Account On DYSTANCE", content, null);
                                await _emailSender.SendEmailAsync(message);

                                //roleManager.AddUserToRole
                                await _userManager.AddToRoleAsync(user, "Teacher");

                                //debug
                                response.Add(new TeacherInfoResponse
                                {
                                    Id = user.Id,
                                    Code = user.UserName,
                                    RealName = user.RealName,
                                    Email = user.Email,
                                    Dob = user.DOB.ToString("yyyy-MM-dd")
                                });
                            }
                            else
                            {
                                return new BadRequestObjectResult(new { message = "Teacher and student import error: Wrong file format" });

                            }

                        }

                    } while (reader.NextResult());
                }
            }
            dict.Add("success", response);
            dict.Add("failed", errors);

            return new OkObjectResult(dict);

        }
        public async Task<IActionResult> GetAttendanceReports(string userId, string semesterId)
        {
            var currentUser = await _userManager.FindByIdAsync(userId);

            if (await _userManager.IsInRoleAsync(currentUser, "student"))
            {
                var attendanceList = new List<AttendanceStudentResponse>();

                var attendances = (from att in _context.AttendanceReports
                                   where att.UserId == userId
                                   select att).ToList();

                foreach (var attendance in attendances)
                {
                    var timetable = await _roomDBContext.TimeTable.FirstOrDefaultAsync(t => t.Id == attendance.TimeTableId);
                    var room = await _roomDBContext.Room.FirstOrDefaultAsync(r => r.RoomId == timetable.RoomId && r.SemesterId.ToString().Contains(semesterId));

                    if (room != null)
                    {
                        attendanceList.Add(new AttendanceStudentResponse
                        {
                            Id = timetable.Id.ToString(),
                            Class = room.ClassName,
                            Subject = room.Subject,
                            Date = String.Format("{0:yyyy-MM-dd}", timetable.Date),
                            StartTime = timetable.StartTime.ToString(@"hh\:mm"),
                            EndTime = timetable.EndTime.ToString(@"hh\:mm"),
                            Teacher = room.CreatorId,
                            Status = attendance.Status
                        });
                    }
                }

                return new OkObjectResult(attendanceList);
            }

            if (await _userManager.IsInRoleAsync(currentUser, "teacher"))
            {
                var attendanceList = new List<AttendanceTeacherResponse>();

                var roomList = await (from rooms in _roomDBContext.Room
                                      where rooms.SemesterId.ToString().Contains(semesterId) && rooms.CreatorId.Contains(userId)
                                      select rooms.RoomId).ToListAsync();

                var timetableList = new List<Timetable>();

                foreach (int roomId in roomList)
                {
                    var timetable = await (from timetables in _roomDBContext.TimeTable
                                           where timetables.RoomId == roomId
                                           select timetables).ToListAsync();

                    timetableList.AddRange(timetable);
                }

                foreach (Timetable timetable in timetableList)
                {
                    //var attendance = await _context.AttendanceReports.FirstOrDefaultAsync(x => x.UserId == userId && x.TimeTableId == timetable.Id);
                    var room = await _roomDBContext.Room.FirstOrDefaultAsync(x => x.RoomId == timetable.RoomId);
                    var listStudent = new List<AttendanceStudent>();

                    var listStudentId = await (from attendances in _context.AttendanceReports
                                               where attendances.TimeTableId == timetable.Id
                                               select new AttendanceStudent
                                               {
                                                   Id = attendances.UserId,
                                                   Status = attendances.Status
                                               }).ToListAsync();

                    listStudent.AddRange(listStudentId);


                    attendanceList.Add(new AttendanceTeacherResponse
                    {
                        Id = timetable.Id.ToString(),
                        Class = room.ClassName,
                        Subject = room.Subject,
                        Date = String.Format("{0:yyyy-MM-dd}", timetable.Date),
                        StartTime = timetable.StartTime.ToString(@"hh\:mm"),
                        EndTime = timetable.EndTime.ToString(@"hh\:mm"),
                        Teacher = userId,
                        Students = listStudent
                    });

                }
                return new OkObjectResult(attendanceList);
            }

            if (await _userManager.IsInRoleAsync(currentUser, "quality assurance") || await _userManager.IsInRoleAsync(currentUser, "academic management"))
            {
                var attendanceList = new List<AttendanceTeacherResponse>();

                var roomList = await (from rooms in _roomDBContext.Room
                                      where rooms.SemesterId.ToString().Contains(semesterId)
                                      select rooms.RoomId).ToListAsync();

                var timetableList = new List<Timetable>();

                foreach (int roomId in roomList)
                {
                    var timetable = await (from timetables in _roomDBContext.TimeTable
                                           where timetables.RoomId == roomId
                                           select timetables).ToListAsync();

                    timetableList.AddRange(timetable);
                }

                foreach (Timetable timetable in timetableList)
                {
                    //var attendance = await _context.AttendanceReports.FirstOrDefaultAsync(x => x.UserId == userId && x.TimeTableId == timetable.Id);
                    var room = await _roomDBContext.Room.FirstOrDefaultAsync(x => x.RoomId == timetable.RoomId);
                    var listStudent = new List<AttendanceStudent>();

                    var listStudentId = await (from attendances in _context.AttendanceReports
                                               where attendances.TimeTableId == timetable.Id
                                               select new AttendanceStudent
                                               {
                                                   Id = attendances.UserId,
                                                   Status = attendances.Status
                                               }).ToListAsync();

                    listStudent.AddRange(listStudentId);


                    attendanceList.Add(new AttendanceTeacherResponse
                    {
                        Id = timetable.Id.ToString(),
                        Class = room.ClassName,
                        Subject = room.Subject,
                        Date = String.Format("{0:yyyy-MM-dd}", timetable.Date),
                        StartTime = timetable.StartTime.ToString(@"hh\:mm"),
                        EndTime = timetable.EndTime.ToString(@"hh\:mm"),
                        Teacher = room.CreatorId,
                        Students = listStudent
                    });

                }

                return new OkObjectResult(attendanceList);

            }

            return new OkObjectResult("");
        }

        public async Task<IActionResult> UpdateAttendanceReports(UpdateAttendanceStudentRequest model)
        {
            var currentUser = await _userManager.FindByIdAsync(_userAccessor.GetCurrentUserId());
            var listStudent = new List<AttendanceStudent>();

            if (await _userManager.IsInRoleAsync(currentUser, "academic management") || await _userManager.IsInRoleAsync(currentUser, "teacher"))
            {

                var studentsList = model.Students.ToList();
                var attendanceList = new List<AttendanceReports>();



                foreach (AttendanceStudent student in studentsList)
                {
                    var updateAttendance = await _context.AttendanceReports.FirstOrDefaultAsync(x => x.UserId == student.Id && x.TimeTableId.ToString() == model.Id);

                    if (updateAttendance != null)
                    {
                        updateAttendance.Status = student.Status;
                        attendanceList.Add(updateAttendance);
                        listStudent.Add(new AttendanceStudent
                        {
                            Id = student.Id,
                            Status = student.Status,
                        });
                    }
                }

                await _attendanceDAO.UpdateAttendance(attendanceList);

                return new ObjectResult(new UpdateAttendanceStudentRequest
                {
                    Id = model.Id,
                    Students = listStudent
                });
            }
            else
            {
                return new ObjectResult(new { message = "Forbidden request" })
                {
                    StatusCode = 403,
                };
            }

        }

        public async Task<FileInfo> ExportAttendance(string roomId)
        {
            var room = _roomDBContext.Room.FirstOrDefault(x => x.RoomId.ToString() == roomId);
            var stream = new MemoryStream();
            FileInfo fileInfo = new FileInfo(room.Subject + "-" + room.ClassName + ".xlsx");
            using (ExcelPackage excel = new ExcelPackage(stream))
            {

                var listTimetable = await (from timetables in _roomDBContext.TimeTable
                                           where timetables.RoomId.ToString().Contains(roomId)
                                           select timetables).ToListAsync();


                foreach (var timetable in listTimetable)
                {
                    var startMinutes = String.Format(timetable.StartTime.Minutes == 0 ? "{0:00}" : "{0:##}", timetable.StartTime.Minutes);
                    var endMinutes = String.Format(timetable.EndTime.Minutes == 0 ? "{0:00}" : "{0:##}", timetable.EndTime.Minutes);    
                    var workSheet = excel.Workbook.Worksheets.Add(String.Format("{0} {1}h{2}-{3}h{4}", timetable.Date.ToString("dd-MM-yyyy"), timetable.StartTime.Hours, startMinutes, timetable.EndTime.Hours, endMinutes));

                    //Header of table  
                    // 
                    workSheet.Row(1).Height = 20;
                    workSheet.Row(1).Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    workSheet.Row(1).Style.Font.Bold = true;
                    workSheet.Cells[1, 1].Value = "Student Code";
                    workSheet.Cells[1, 2].Value = "Real Name";
                    workSheet.Cells[1, 3].Value = "Email";
                    workSheet.Cells[1, 4].Value = "Status";

                    //Get students from slot
                    //
                    var listStudents = await (from students in _context.AttendanceReports
                                              where students.TimeTableId == timetable.Id
                                              select students).ToListAsync();


                    //Body of table  
                    //
                    int recordIndex = 2;
                    foreach (var student in listStudents)
                    {
                        var user = await _userManager.FindByIdAsync(student.UserId);
                        workSheet.Cells[recordIndex, 1].Value = user.UserName;
                        workSheet.Cells[recordIndex, 2].Value = user.RealName;
                        workSheet.Cells[recordIndex, 3].Value = user.Email;
                        workSheet.Cells[recordIndex, 4].Value = student.Status;
                        recordIndex++;
                    }


                    //Fit
                    workSheet.Column(1).AutoFit();
                    workSheet.Column(2).AutoFit();
                    workSheet.Column(3).AutoFit();
                    workSheet.Column(4).AutoFit();

                    
                }
                excel.SaveAs(fileInfo);
            }

            return fileInfo;
        }


    }
}
