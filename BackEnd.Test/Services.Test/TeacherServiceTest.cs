using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Responses;
using BackEnd.Security;
using BackEnd.Services;
using EmailService;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BackEnd.Test.Services.Test
{
    public class TeacherServiceTest
    {
        private Mock<FakeUserManager> fakeUserManager;

        private UserDbContext _userContext;

        private RoomDBContext _roomContext;

        private ConnectionFactory factory;

        private Mock<TeacherService> _teacherService;

        private IQueryable<AppUser> users;

        private Mock<IEmailSender> mockEmailSender;

        private Mock<UserAccessor> mockUserAccessor;

        private Mock<IHttpContextAccessor> mockHttpContextAccessor;

        private Mock<IUrlHelperFactory> mockUrlHelperFactory;

        private Mock<IActionContextAccessor> actionContextAccessor;

        private Mock<IUrlHelper> mockUrlHelper;

        public TeacherServiceTest()
        {
            //setting up context
            factory = new ConnectionFactory();
            _userContext = factory.CreateUserDbContextForInMemory();
            _userContext.Database.EnsureDeleted();
            _userContext.Database.EnsureCreated();
            _userContext.SaveChanges();

            _roomContext = factory.CreateRoomDbContextForInMemory();
            _roomContext.Database.EnsureDeleted();
            _roomContext.Database.EnsureCreated();
            _roomContext.SaveChanges();

            //mocking user manager
            fakeUserManager = new Mock<FakeUserManager>();

            fakeUserManager.Setup(x => x.DeleteAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);
            fakeUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            fakeUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

            //MockLogDAO
            var logDAO = new Mock<ILogDAO>();
            logDAO.Setup(x => x.CreateLog(It.IsAny<UsersLog>())).ReturnsAsync("success");
            logDAO.Setup(x => x.DeleteLogs(It.IsAny<List<UsersLog>>())).ReturnsAsync("success");
            logDAO.Setup(x => x.GetLogsByRoomId(It.IsAny<string>())).ReturnsAsync(new List<UsersLog>());

            //MockLogDAO
            var attendanceDAO = new Mock<IAttendanceDAO>();
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
            mockHttpContextAccessor = new Mock<IHttpContextAccessor>();
            var HttpContext = new DefaultHttpContext();
            mockHttpContextAccessor.Setup(_ => _.HttpContext).Returns(HttpContext);

            //Mocking EmailSender
            mockEmailSender = new Mock<IEmailSender>();

            //Mocking UserAccessor
            mockUserAccessor = new Mock<UserAccessor>(mockHttpContextAccessor.Object);

            //Mocking UrlHelper
            mockUrlHelperFactory = new Mock<IUrlHelperFactory>();

            //Mockikng ActionContextAccessor
            actionContextAccessor = new Mock<IActionContextAccessor>();

            //MockPMAO
            var privateDAO = new Mock<IPrivateMessageDAO>();
            privateDAO.Setup(x => x.DeletePrivateMessages(It.IsAny<List<PrivateMessage>>())).ReturnsAsync(new OkObjectResult(""));

            _teacherService = new Mock<TeacherService>(
                _userContext,
                fakeUserManager.Object,
                _roomContext,
                logDAO.Object,
                attendanceDAO.Object,
                privateDAO.Object,
                mockEmailSender.Object,
                mockUrlHelperFactory.Object,
                actionContextAccessor.Object);
        }

        [Fact]
        public async void GetTeacher_Returns_Correctly()
        {
            //Arrange
            var user = new AppUser
            {
                Id = "1",
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var user2 = new AppUser
            {
                Id = "2",
                UserName = "Test2",
                Email = "Test2@gmail",
                Avatar = "default.png",
                RealName = "Test 2",
                DOB = new DateTime(2020, 02, 29)
            };

            var user3 = new AppUser
            {
                Id = "3",
                UserName = "Test3",
                Email = "Test3@gmail",
                Avatar = "default.png",
                RealName = "Test 3",
                DOB = new DateTime(2020, 07, 01)
            };

            var teacherResponses = new List<TeacherInfoResponse>()
            {
                new TeacherInfoResponse{
                    Id = user.Id,
                    Code = user.UserName,
                    Email = user.Email,
                    RealName = user.RealName,
                    Dob = String.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(user.DOB))
                },
                new TeacherInfoResponse{
                    Id = user2.Id,
                    Code = user2.UserName,
                    Email = user2.Email,
                    RealName = user2.RealName,
                    Dob = String.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(user2.DOB))
                },
                new TeacherInfoResponse{
                    Id = user3.Id,
                    Code = user3.UserName,
                    Email = user3.Email,
                    RealName = user3.RealName,
                    Dob = String.Format("{0:yyyy-MM-dd}", Convert.ToDateTime(user3.DOB))
                }
            };

            fakeUserManager.Setup(x => x.GetUsersInRoleAsync(It.IsAny<string>())).ReturnsAsync(new List<AppUser>() { user, user2, user3 });

            //Act
            var result = await _teacherService.Object.GetTeacher();
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as List<TeacherInfoResponse>;


            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.NotEmpty(model);
            Assert.IsType<OkObjectResult>(okObjectResult);

            //check teacherresponse 1
            Assert.Equal(teacherResponses[0].Code, model[0].Code);
            Assert.Equal(teacherResponses[0].RealName, model[0].RealName);
            Assert.Equal(teacherResponses[0].Code, model[0].Code);
            Assert.Equal(teacherResponses[0].Email, model[0].Email);
            Assert.Equal(teacherResponses[0].Dob, model[0].Dob);
            Assert.Equal(teacherResponses[0].Id, model[0].Id);

            //check teacherresponse 2
            Assert.Equal(teacherResponses[1].Code, model[1].Code);
            Assert.Equal(teacherResponses[1].RealName, model[1].RealName);
            Assert.Equal(teacherResponses[1].Code, model[1].Code);
            Assert.Equal(teacherResponses[1].Email, model[1].Email);
            Assert.Equal(teacherResponses[1].Dob, model[1].Dob);
            Assert.Equal(teacherResponses[1].Id, model[1].Id);

            //check teacherresponse 3
            Assert.Equal(teacherResponses[2].Code, model[2].Code);
            Assert.Equal(teacherResponses[2].RealName, model[2].RealName);
            Assert.Equal(teacherResponses[2].Code, model[2].Code);
            Assert.Equal(teacherResponses[2].Email, model[2].Email);
            Assert.Equal(teacherResponses[2].Dob, model[2].Dob);
            Assert.Equal(teacherResponses[2].Id, model[2].Id);

        }


        [Fact]
        public async void AddTeacher_Returns_Correctly()
        {
            //Arrange
            var user = new TeacherRequest
            {
                Id = "1",
                Code = "Test1",
                Email = "Test1@gmail",
                RealName = "Test 1",
                Dob = "2020-09-29"
            };

            var teacherResponses = new List<TeacherInfoResponse>()
            {
                new TeacherInfoResponse{
                    Id = user.Id,
                    Code = user.Code,
                    Email = user.Email,
                    RealName = user.RealName,
                    Dob = user.Dob
                },
            };

            // create url helper mock
            Type t = typeof(UserServiceTest);
            var httpContext = new Mock<HttpContext>().Object;
            actionContextAccessor.Setup(x => x.ActionContext).Returns(new ActionContext(
                httpContext,
                new Microsoft.AspNetCore.Routing.RouteData(),
                new ControllerActionDescriptor()
                {
                    MethodInfo = t.GetMethod(nameof(TeacherServiceTest.AddTeacher_Returns_Correctly))
                }));


            mockUrlHelper = new Mock<IUrlHelper>();

            mockUrlHelperFactory.Setup(x => x.GetUrlHelper(It.IsAny<ActionContext>())).Returns(mockUrlHelper.Object);
            mockUrlHelper.SetupGet(h => h.ActionContext).Returns(actionContextAccessor.Object.ActionContext);

            UrlActionContext actual = null;

            mockUrlHelper.Setup(h => h.Action(It.IsAny<UrlActionContext>()))
                .Callback((UrlActionContext context) => actual = context);

            fakeUserManager.Setup(x => x.GenerateEmailConfirmationTokenAsync(It.IsAny<AppUser>())).ReturnsAsync("");
            mockEmailSender.Setup(x => x.SendEmailAsync(It.IsAny<Message>())).Returns(() => Task.FromResult(""));




            //Act
            var result = await _teacherService.Object.AddTeacher(user);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as TeacherInfoResponse;

            var list = new List<TeacherInfoResponse>();

            list.Add(model);

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.NotEmpty(list);
            Assert.IsType<OkObjectResult>(okObjectResult);

            //check teacherresponse 1
            Assert.Equal(teacherResponses[0].Code, model.Code);
            Assert.Equal(teacherResponses[0].RealName, model.RealName);
            Assert.Equal(teacherResponses[0].Code, model.Code);
            Assert.Equal(teacherResponses[0].Email, model.Email);
            Assert.Equal(teacherResponses[0].Dob, model.Dob);
        }

        [Fact]
        public async void DeleteTeacher_Returns_Correctly()
        {
            //Arrange
            var user = new AppUser
            {
                Id = "1",
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var user2 = new AppUser
            {
                Id = "2",
                UserName = "Test2",
                Email = "Test2@gmail",
                Avatar = "default.png",
                RealName = "Test 2",
                DOB = new DateTime(2020, 02, 29)
            };

            var user3 = new AppUser
            {
                Id = "3",
                UserName = "Test3",
                Email = "Test3@gmail",
                Avatar = "default.png",
                RealName = "Test 3",
                DOB = new DateTime(2020, 07, 01)
            };


            List<string> list = new List<string>() { user.Id,user2.Id,user3.Id};
            fakeUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
            fakeUserManager.Setup(x => x.FindByIdAsync(user2.Id)).ReturnsAsync(user2);
            fakeUserManager.Setup(x => x.FindByIdAsync(user3.Id)).ReturnsAsync(user3);
            fakeUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "teacher")).ReturnsAsync(true);



            //Act
            var result = await _teacherService.Object.DeleteTeacher(list);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as Dictionary<String, object>;
            var keys = model.Keys.ToArray();
            var values = model.Values.ToArray();
            var emptyList = new List<string>();

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.Equal(2, model.Count);
            Assert.Equal("success", keys[0].ToString());
            Assert.Equal("failed", keys[1].ToString());
            Assert.Equal(list, values[0]);
            Assert.Equal(emptyList ,values[1]);

        }


    }
}
