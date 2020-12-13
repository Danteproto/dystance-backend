using AutoMapper;
using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Security;
using BackEnd.Services;
using BackEnd.Stores;
using EmailService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using SignInResult = Microsoft.AspNetCore.Identity.SignInResult;

namespace BackEnd.Test
{
    public class UserServiceTest : IClassFixture<TestFixture<Startup>>
    {
        private Mock<UserService> _userService { get; }

        private Mock<FakeSignInManager> fakeSignInManager;

        private Mock<FakeUserManager> fakeUserManager;

        private UserDbContext context;

        private Mock<JwtGenerator> _jwtGenerator;

        IEnumerable<AppUser> users;

        private ConnectionFactory factory;

        private Mock<IAttendanceDAO> attendanceDAO;

        private Mock<ILogDAO>logDAO;

        private RoomDBContext roomContext;

        public UserServiceTest(TestFixture<Startup> fixture)
        {
            //setting up context
            factory = new ConnectionFactory();
            context = factory.CreateUserDbContextForInMemory();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SaveChanges();

            //setting up room context
            factory = new ConnectionFactory();
            roomContext = factory.CreateRoomDbContextForInMemory();
            roomContext.Database.EnsureDeleted();
            roomContext.Database.EnsureCreated();
            roomContext.SaveChanges();


            //mocking user manager
            users = new List<AppUser>
            {
                new AppUser
                {
                    UserName = "abcxyz",
                    Email = "abcxyz@gmail.com"
                },
                new AppUser
                {
                    UserName = "dat",
                    Email = "dat@gmail.com"
                },
                new AppUser
                {
                    UserName = "minh",
                    Email = "minh@gmail.com"
                },
                new AppUser
                {
                    UserName = "hoang",
                    Email = "hoang@gmail.com"
                },
                new AppUser
                {
                    UserName = "tu",
                    Email = "tu@gmail.com"
                },
            }.AsQueryable();

            fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(x => x.Users).Returns(users.AsQueryable());

            fakeUserManager.Setup(x => x.DeleteAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);
            fakeUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
            fakeUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

            //mocking mapper

            var mapper = (IMapper)fixture.Server.Host.Services.GetService(typeof(IMapper));
            //var mockMapper = new MapperConfiguration(cfg =>
            //{
            //    cfg.AddProfile(new UserProfile()); //your automapperprofile 
            //});
            //var mapper = mockMapper.CreateMapper();


            //mocking IEmailService
            var emailConfig = new EmailConfiguration
            {
                From = "dystancemailservice@gmail.com",
                SmtpServer = "smtp.gmail.com",
                Port = 465,
                UserName = "dystancemailservice@gmail.com"
            };
            var appSettings = new AppSettings
            {
                Secret = "THIS IS USED TO SIGN AND VERIFY JWT TOKENS, REPLACE IT WITH YOUR OWN SECRET, IT CAN BE ANY STRING"
            };
            IOptions<AppSettings> options = Options.Create(appSettings);



            //mocking HttpContextAccessor
            var mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var HttpContext = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(HttpContext);


            //mocking signinmanager
            fakeSignInManager = new Mock<FakeSignInManager>();
            fakeSignInManager.Setup(
                    x => x.PasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>()))
                .ReturnsAsync(SignInResult.Success);
           
            _jwtGenerator = new Mock<JwtGenerator>(options, mapper, context);

            //mocking attendanceDAO
            attendanceDAO = new Mock<IAttendanceDAO>();
            attendanceDAO.Setup(x => x.UpdateAttendance(It.IsAny<List<AttendanceReports>>())).ReturnsAsync(new ObjectResult(new { message = "success" })
            {
                StatusCode = 200,
            });

            attendanceDAO.Setup(x => x.CreateAttendance(It.IsAny<List<AttendanceReports>>())).ReturnsAsync(new ObjectResult(new { message = "success" })
            {
                StatusCode = 200,
            });

            attendanceDAO.Setup(x => x.DeleteAttendance(It.IsAny<List<AttendanceReports>>())).ReturnsAsync(new ObjectResult(new { message = "success" })
            {
                StatusCode = 200,
            });


            //MockLogDAO
            logDAO = new Mock<ILogDAO>();
            logDAO.Setup(x => x.CreateLog(It.IsAny<UsersLog>())).ReturnsAsync("success");
            logDAO.Setup(x => x.DeleteLogs(It.IsAny<List<UsersLog>>())).ReturnsAsync("success");
            logDAO.Setup(x => x.GetLogsByRoomId(It.IsAny<string>())).ReturnsAsync(new List<UsersLog>());

            //mocking UserStore
            var userStore = new Mock<IUserStore>();
            userStore.Setup(x => x.GenerateTokenAndSave(It.IsAny<String>())).Returns("");
            userStore.Setup(x => x.IsTokenValid(It.IsAny<String>())).Returns(true);

            var _env = new Mock<IWebHostEnvironment>();



            _userService = new Mock<UserService>(
                context, 
                mapper, 
                fakeSignInManager.Object, 
                fakeUserManager.Object,
                new UrlHelperFactory(), 
                new ActionContextAccessor(), 
                new EmailSender(emailConfig), 
                _jwtGenerator.Object, 
                new UserAccessor(mockHttpContextAccessor.Object),
                userStore,
                _env,
                logDAO,
                roomContext,
                attendanceDAO
                );

        }

        [Theory]
        [MemberData(nameof(AuthenticateTestCase_ForOK))]
        public async void TestAuthenticate_Return_OkResult(AuthenticateRequest model, string description)
        {
            var user = new AppUser
            {
                Email = model.Email,
                UserName = model.UserName,
                EmailConfirmed = true,
                RefreshTokens = new List<RefreshToken>(),
                Id = Guid.NewGuid().ToString()
            };
            fakeUserManager.Setup(x => x.FindByEmailAsync(user.Email)).Returns(Task.FromResult(users.FirstOrDefault<AppUser>(u => u.Email == user.Email) == null ? null : user));
            fakeUserManager.Setup(x => x.FindByNameAsync(user.UserName)).Returns(Task.FromResult(users.FirstOrDefault<AppUser>(u => u.UserName == user.UserName) == null ? null : user));
            fakeSignInManager.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<String>(), false)).ReturnsAsync(SignInResult.Success);
            context.Add(user);
            context.SaveChanges();


            var data = await _userService.Object.Authenticate(model);

            Assert.IsType<OkObjectResult>(data);

        }

        [Theory]
        [MemberData(nameof(AuthenticateTestCase_ForNotOK))]
        public async void TestAuthenticate_Return_NotOkResult(AuthenticateRequest model, string description)
        {
            var user = new AppUser
            {
                Email = model.Email,
                UserName = model.UserName,
                EmailConfirmed = true,
                RefreshTokens = new List<RefreshToken>(),
                Id = Guid.NewGuid().ToString()
            };

            var password = "P@ssw0rd";
            fakeUserManager.Setup(x => x.FindByEmailAsync(user.Email)).Returns(Task.FromResult(users.FirstOrDefault<AppUser>(u => u.Email == user.Email) == null ? null : user));
            fakeUserManager.Setup(x => x.FindByNameAsync(user.UserName)).Returns(Task.FromResult(users.FirstOrDefault<AppUser>(u => u.UserName == user.UserName) == null ? null : user));
            fakeSignInManager.Setup(x => x.CheckPasswordSignInAsync(It.IsAny<AppUser>(), It.IsAny<String>(), false)).ReturnsAsync(password.Equals(model.Password) == true ? SignInResult.Success : SignInResult.Failed);
            
            context.Add(user);
            context.SaveChanges();


            var data = await _userService.Object.Authenticate(model);

            Assert.IsType<BadRequestObjectResult>(data);

        }


        [Theory]
        [InlineData("TestRefreshToken", "9999-9-29")]
        public void TestRefreshToken_Return_OkResult(string token,string expiresDate)
        {
            var user = new AppUser
            {
                Email = "refreshToken@gmail",
                UserName = "refreshToken",
                EmailConfirmed = true,
                RefreshTokens = new List<RefreshToken>(),
                Id = Guid.NewGuid().ToString()
            };

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = "TestRefreshToken",
                Expires = DateTime.Parse(expiresDate)
            }) ;
            context.Add(user);
            context.SaveChanges();

            var data = _userService.Object.RefreshToken(token);

            Assert.IsType<OkObjectResult>(data);
        }

        [Theory]
        [InlineData("NotTestRefreshToken", "9999-9-29")]
        [InlineData("TestRefreshToken", "2019-9-29")]
        public void TestRefreshToken_Return_NotOkResult(string token, string expiresDate)
        {
            var user = new AppUser
            {
                Email = "refreshToken@gmail",
                UserName = "refreshToken",
                EmailConfirmed = true,
                RefreshTokens = new List<RefreshToken>(),
                Id = Guid.NewGuid().ToString()
            };

            user.RefreshTokens.Add(new RefreshToken
            {
                Token = "TestRefreshToken",
                Expires = DateTime.Parse(expiresDate)
            });
            context.Add(user);
            context.SaveChanges();

            var data = _userService.Object.RefreshToken(token);

            Assert.IsType<ObjectResult>(data);
            Assert.Equal(500, ((ObjectResult)data).StatusCode);
        }

        [Theory]
        [InlineData("1")]
        public void TestGetById_Return_OkResult(string id)
        { 
            var user = new AppUser
            {
                Email = "GetById@gmail",
                UserName = "GetById",
                EmailConfirmed = true,
                RefreshTokens = new List<RefreshToken>(),
                Id = "1"
            };

            context.Add(user);
            context.SaveChanges();

            var data = _userService.Object.GetById(id);

            Assert.IsType<OkObjectResult>(data);
        }

        [Theory]
        [InlineData("2")]
        public void TestGetById_Return_NotOkResult(string id)
        {
            var user = new AppUser
            {
                Email = "GetById@gmail",
                UserName = "GetById",
                EmailConfirmed = true,
                RefreshTokens = new List<RefreshToken>(),
                Id = "1"
            };

            context.Add(user);
            context.SaveChanges();

            var data = _userService.Object.GetById(id);

            Assert.IsType<NotFoundObjectResult>(data);
        }


        //[Theory]
        //[MemberData(nameof(RegisterTestCase_ForOK))]
        //public async void TestRegister_Return_OkResult(RegisterRequest registerRequest, bool expectedOutput, string description)
        //{
        //    var user = new AppUser
        //    {
        //        Email = registerRequest.Email,
        //        UserName = registerRequest.Username,
        //        RefreshTokens = new List<RefreshToken>(),
        //        Id = Guid.NewGuid().ToString()
        //    };

        //    fakeUserManager.Setup(x => x.FindByEmailAsync(user.Email)).Returns(Task.FromResult(users.FirstOrDefault<AppUser>(u => u.Email == user.Email) == null ? null : user));
        //    fakeUserManager.Setup(x => x.FindByNameAsync(user.UserName)).Returns(Task.FromResult(users.FirstOrDefault<AppUser>(u => u.UserName == user.UserName) == null ? null : user));

        //    fakeUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
        //    .ReturnsAsync(IdentityResult.Success).Callback<AppUser, string>((x, y) => context.Users.Add(new AppUser {
        //        Email = x.Email
        //        }));


        //    var data = await _userService.Object.Register(registerRequest);



        //    Assert.IsType<OkObjectResult>(data);
        //}










        #region ObjectĐểTruyềnVào Cho phần MemberData, để truyền vào object Request cho nhanh  
        public static IEnumerable<object[]> AuthenticateTestCase_ForOK
        {
            get
            {
                var Password = "Pa$$w0rd";
                var Username = "abcxyz";
                var Email = "abcxyz@gmail.com";

                var data = new List<ITheoryDatum>();

                data.Add(TheoryDatum.Factory(new AuthenticateRequest
                {
                    UserName = Username,
                    Password = Password
                }, "Email missing"));

                data.Add(TheoryDatum.Factory(new AuthenticateRequest
                {
                    Email = Email,
                    Password = Password
                }, "Username missing"));


                return data.ConvertAll(d => d.ToParameterArray());
            }
        }

        public static IEnumerable<object[]> AuthenticateTestCase_ForNotOK
        {
            get
            {
                var Password = "1";
                var Username = "a";
                var Email = "a@gmail.com";

                var data = new List<ITheoryDatum>();

                data.Add(TheoryDatum.Factory(new AuthenticateRequest
                {
                    UserName = Username,
                    Password = Password
                }, "Wrong Email"));

                data.Add(TheoryDatum.Factory(new AuthenticateRequest
                {
                    Email = Email,
                    Password = Password
                }, "Wrong Username"));
                data.Add(TheoryDatum.Factory(new AuthenticateRequest
                {
                    Email = Email,
                    Password = Password
                }, "Wrong Password"));

                return data.ConvertAll(d => d.ToParameterArray());
            }
        }

        public static IEnumerable<object[]> RegisterTestCase_ForOK
        {
            get
            {
                var Username = "dathaynha";
                var Email = "dathaynha@gmail.com";
                var Password = "dathaynha1";
                var RealName = "Ha Quoc Dat";
                var DOB = "29-09-1999";

               var data = new List<ITheoryDatum>();

                data.Add(TheoryDatum.Factory(new RegisterRequest
                {
                    UserName = Username,
                    Email = Email,
                    Password = Password,
                    RealName = RealName,
                    Dob = DOB,

                }, "Success"));

                

                return data.ConvertAll(d => d.ToParameterArray());
            }
        }
    }

    #endregion  
}
