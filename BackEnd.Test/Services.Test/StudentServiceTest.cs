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

    public class StudentServiceTest
    {

        private Mock<FakeUserManager> fakeUserManager;

        private UserDbContext _userContext;

        private RoomDBContext _roomContext;

        private ConnectionFactory factory;

        private Mock<StudentService> _studentService;

        private IQueryable<AppUser> users;

        private Mock<IEmailSender> mockEmailSender;

        private Mock<UserAccessor> mockUserAccessor;

        private Mock<IHttpContextAccessor> mockHttpContextAccessor;

        private Mock<IUrlHelperFactory> mockUrlHelperFactory;

        private Mock<IActionContextAccessor> actionContextAccessor;

        private Mock<IUrlHelper> mockUrlHelper;

        public StudentServiceTest()
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

            _studentService = new Mock<StudentService>(
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
        public async void GetStudent_Returns_Correctly()
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
                }
            };

            fakeUserManager.Setup(x => x.GetUsersInRoleAsync(It.IsAny<string>())).ReturnsAsync(new List<AppUser>() { user, user2 });

            //Act
            var result = await _studentService.Object.GetStudent();
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

        }

        [Fact]
        public async void GetStudent_Returns_Fail()
        {
            //Arrange

            //fakeUserManager.Setup(x => x.GetUsersInRoleAsync(It.IsAny<string>())).ReturnsAsync(new List<AppUser>() { user, user2 });

            //Act
            var result = await _studentService.Object.GetStudent();
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as List<TeacherInfoResponse>;


            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.Empty(model);
            Assert.IsType<OkObjectResult>(okObjectResult);


        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public async void AddStudent_Returns_Correctly(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };


            var teacherRequest = new TeacherRequest
            {
                Id = user.Id,
                Code = user.UserName,
                Email = user.Email,
                RealName = user.RealName,
                Dob = "2020-09-29"
            };

            var teacherResponses = new List<TeacherInfoResponse>()
            {
                new TeacherInfoResponse{
                    Id = teacherRequest.Id,
                    Code = teacherRequest.Code,
                    Email = teacherRequest.Email,
                    RealName = teacherRequest.RealName,
                    Dob = teacherRequest.Dob
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
            var result = await _studentService.Object.AddStudent(teacherRequest);
            var okObjectResult = result as OkObjectResult;


            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
        }



        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public async void AddStudent_Returns_Fail_CodeExist(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };


            var teacherRequest = new TeacherRequest
            {
                Id = user.Id,
                Code = user.UserName,
                Email = user.Email,
                RealName = user.RealName,
                Dob = "2020-09-29"
            };

            var teacherResponses = new List<TeacherInfoResponse>()
            {
                new TeacherInfoResponse{
                    Id = teacherRequest.Id,
                    Code = teacherRequest.Code,
                    Email = teacherRequest.Email,
                    RealName = teacherRequest.RealName,
                    Dob = teacherRequest.Dob
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
            fakeUserManager.Setup(x => x.FindByNameAsync(teacherRequest.Code)).Returns(() => Task.FromResult(user));



            //Act
            var result = await _studentService.Object.AddStudent(teacherRequest);
            var okObjectResult = result as BadRequestObjectResult;


            //Assert
            Assert.Equal(400, okObjectResult.StatusCode);
            Assert.IsType<BadRequestObjectResult>(okObjectResult);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("4")]
        public async void AddStudent_Returns_Fail_EmailExist(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };


            var teacherRequest = new TeacherRequest
            {
                Id = user.Id,
                Code = user.UserName,
                Email = user.Email,
                RealName = user.RealName,
                Dob = "2020-09-29"
            };

            var teacherResponses = new List<TeacherInfoResponse>()
            {
                new TeacherInfoResponse{
                    Id = teacherRequest.Id,
                    Code = teacherRequest.Code,
                    Email = teacherRequest.Email,
                    RealName = teacherRequest.RealName,
                    Dob = teacherRequest.Dob
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
            fakeUserManager.Setup(x => x.FindByEmailAsync(teacherRequest.Email)).Returns(() => Task.FromResult(user));



            //Act
            var result = await _studentService.Object.AddStudent(teacherRequest);
            var okObjectResult = result as BadRequestObjectResult;


            //Assert
            Assert.Equal(400, okObjectResult.StatusCode);
            Assert.IsType<BadRequestObjectResult>(okObjectResult);
        }


        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("5")]
        public async void DeleteStudent_Returns_Correctly(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            List<string> list = new List<string>() { user.Id };
            fakeUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
            fakeUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "student")).ReturnsAsync(true);



            //Act
            var result = await _studentService.Object.DeleteStudent(list);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as Dictionary<String, object>;
            var keys = model.Keys.ToArray();
            var values = model.Values.ToArray();
            var emptyList = new List<string>();
            var listSuccess = (List<string>)values[0];

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.Equal(2, model.Count);
            Assert.Equal("success", keys[0].ToString());
            Assert.Equal("failed", keys[1].ToString());
            Assert.Equal(list, values[0]);

            //check value in dictionary
            Assert.Equal(user.Id, listSuccess[0]);
            Assert.Equal(emptyList, values[1]);

        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("5")]
        public async void DeleteStudent_Returns_Fail_NotFoundId(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            List<string> list = new List<string>() { user.Id, "4" };
            List<string> listcheckSuccess = new List<string>() { user.Id };

            fakeUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
            fakeUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "student")).ReturnsAsync(true);



            //Act
            var result = await _studentService.Object.DeleteStudent(list);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as Dictionary<String, object>;
            var keys = model.Keys.ToArray();
            var values = model.Values.ToArray();
            var emptyList = new List<string>() { "This id " + "4" + " don't exist" };
            var listSuccess = (List<string>)values[0];
            var listFailed = (List<Error>)values[1];

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.Equal(2, model.Count);
            Assert.Equal("success", keys[0].ToString());
            Assert.Equal("failed", keys[1].ToString());
            Assert.Equal(listcheckSuccess, values[0]);

            //check value in dictionary
            Assert.Equal(user.Id, listSuccess[0]);
            Assert.Equal(emptyList[0], listFailed[0].Message);

        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("5")]
        public async void DeleteStudent_Returns_Fail_NotStudent(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var user2 = new AppUser
            {
                Id = "4",
                UserName = "Test4",
                Email = "Test4@gmail",
                Avatar = "default.png",
                RealName = "Test 4",
                DOB = new DateTime(2020, 09, 29)
            };

            List<string> list = new List<string>() { user.Id, "4" };
            List<string> listcheckSuccess = new List<string>() { user.Id };

            fakeUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
            fakeUserManager.Setup(x => x.FindByIdAsync(user2.Id)).ReturnsAsync(user2);
            fakeUserManager.Setup(x => x.IsInRoleAsync(user, "student")).ReturnsAsync(true);


            //Act
            var result = await _studentService.Object.DeleteStudent(list);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as Dictionary<String, object>;
            var keys = model.Keys.ToArray();
            var values = model.Values.ToArray();
            var emptyList = new List<string>() { "The user " + user2.UserName + " is not a student" };
            var listSuccess = (List<string>)values[0];
            var listFailed = (List<Error>)values[1];

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.Equal(2, model.Count);
            Assert.Equal("success", keys[0].ToString());
            Assert.Equal("failed", keys[1].ToString());
            Assert.Equal(listcheckSuccess, values[0]);

            //check value in dictionary
            Assert.Equal(user.Id, listSuccess[0]);
            Assert.Equal(emptyList[0], listFailed[0].Message);

        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        [InlineData("5")]
        public async void DeleteStudent_Returns_Fail_LinkedToRoom(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var user2 = new AppUser
            {
                Id = "4",
                UserName = "Test4",
                Email = "Test4@gmail",
                Avatar = "default.png",
                RealName = "Test 4",
                DOB = new DateTime(2020, 09, 29)
            };

            var roomUserLink = new RoomUserLink
            {
                RoomUserId = 1,
                RoomId = 1,
                UserId = "4"
            };

            _roomContext.RoomUserLink.Add(roomUserLink);
            _roomContext.SaveChanges();

            List<string> list = new List<string>() { user.Id, "4" };
            List<string> listcheckSuccess = new List<string>() { user.Id };

            fakeUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(user);
            fakeUserManager.Setup(x => x.FindByIdAsync(user2.Id)).ReturnsAsync(user2);
            fakeUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "student")).ReturnsAsync(true);

            //Act
            var result = await _studentService.Object.DeleteStudent(list);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as Dictionary<String, object>;
            var keys = model.Keys.ToArray();
            var values = model.Values.ToArray();
            var emptyList = new List<string>() { "Student " + user2.UserName + " is still linked to a classroom" };
            var listSuccess = (List<string>)values[0];
            var listFailed = (List<Error>)values[1];

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.Equal(2, model.Count);
            Assert.Equal("success", keys[0].ToString());
            Assert.Equal("failed", keys[1].ToString());
            Assert.Equal(listcheckSuccess, values[0]);

            //check value in dictionary
            Assert.Equal(user.Id, listSuccess[0]);
            Assert.Equal(emptyList[0], listFailed[0].Message);

        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        public async void UpdateStudent_Returns_Correctly(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var userDB = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "TestDB 1",
                DOB = new DateTime(2020, 09, 29)
            };

            List<TeacherRequest> list = new List<TeacherRequest>() {
                new TeacherRequest{
                    Id = user.Id,
                    Code = user.UserName,
                    Email = user.Email,
                    RealName = user.RealName,
                    Dob = user.DOB.ToString("yyyy-MM-dd")
                }
                };

            List<TeacherInfoResponse> listres = new List<TeacherInfoResponse>() {
                new TeacherInfoResponse{
                    Id = user.Id,
                    Code = user.UserName,
                    Email = user.Email,
                    RealName = user.RealName,
                    Dob = user.DOB.ToString("yyyy-MM-dd")
                } };

            fakeUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(userDB);
            fakeUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "student")).ReturnsAsync(true);


            //Act
            var result = await _studentService.Object.UpdateStudent(list);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as Dictionary<String, object>;
            var keys = model.Keys.ToArray();
            var values = model.Values.ToArray();

            var listSuccess = (List<TeacherInfoResponse>)values[0];

            var listFailed = new List<Error>();

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.Equal(2, model.Count);
            Assert.Equal("success", keys[0].ToString());
            Assert.Equal("failed", keys[1].ToString());

            //check response 1
            Assert.Equal(listres[0].Id, listSuccess[0].Id);
            Assert.Equal(listres[0].Code, listSuccess[0].Code);
            Assert.Equal(listres[0].Email, listSuccess[0].Email);
            Assert.Equal(listres[0].RealName, listSuccess[0].RealName);
            Assert.Equal(listres[0].Dob, listSuccess[0].Dob);

            //check list failed = empty
            Assert.Equal(listFailed, values[1]);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        public async void UpdateStudent_Returns_Fail_EmailExist(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var userDB = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "TestDB 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var user2 = new AppUser
            {
                Id = "4",
                UserName = "Test4",
                Email = "Test4@gmail",
                Avatar = "default.png",
                RealName = "Test 4",
                DOB = new DateTime(2020, 09, 29)
            };

            List<TeacherRequest> list = new List<TeacherRequest>() {
                new TeacherRequest{
                    Id = user.Id,
                    Code = user.UserName,
                    Email = user.Email,
                    RealName = user.RealName,
                    Dob = user.DOB.ToString("yyyy-MM-dd")
                },
                new TeacherRequest{
                    Id = user2.Id,
                    Code = user2.UserName,
                    Email = user2.Email,
                    RealName = "Test",
                    Dob = user2.DOB.ToString("yyyy-MM-dd")
                }
                };

            List<TeacherInfoResponse> listres = new List<TeacherInfoResponse>() {
                new TeacherInfoResponse{
                    Id = user.Id,
                    Code = user.UserName,
                    Email = user.Email,
                    RealName = user.RealName,
                    Dob = user.DOB.ToString("yyyy-MM-dd")
                } };

            fakeUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(userDB);
            fakeUserManager.Setup(x => x.FindByIdAsync(user2.Id)).ReturnsAsync(user);
            fakeUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "student")).ReturnsAsync(true);
            fakeUserManager.Setup(x => x.FindByEmailAsync(user2.Email)).ReturnsAsync(user);

            //Act
            var result = await _studentService.Object.UpdateStudent(list);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as Dictionary<String, object>;
            var keys = model.Keys.ToArray();
            var values = model.Values.ToArray();

            var listSuccess = (List<TeacherInfoResponse>)values[0];

            var listFailed = (List<Error>)values[1];

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.Equal(2, model.Count);
            Assert.Equal("success", keys[0].ToString());
            Assert.Equal("failed", keys[1].ToString());

            //check response 1
            Assert.Equal(listres[0].Id, listSuccess[0].Id);
            Assert.Equal(listres[0].Code, listSuccess[0].Code);
            Assert.Equal(listres[0].Email, listSuccess[0].Email);
            Assert.Equal(listres[0].RealName, listSuccess[0].RealName);
            Assert.Equal(listres[0].Dob, listSuccess[0].Dob);

            //check list failed = empty
            //Assert.Equal(listFailed, values[1]);
            Assert.Equal("Email " + user2.Email + " already exists", listFailed[0].Message);
            Assert.Equal(1, listFailed[0].Type);
        }

        [Theory]
        [InlineData("1")]
        [InlineData("2")]
        [InlineData("3")]
        public async void UpdateStudent_Returns_Fail_CodeExist(string id)
        {
            //Arrange
            var user = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "Test 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var userDB = new AppUser
            {
                Id = id,
                UserName = "Test1",
                Email = "Test1@gmail",
                Avatar = "default.png",
                RealName = "TestDB 1",
                DOB = new DateTime(2020, 09, 29)
            };

            var user2 = new AppUser
            {
                Id = "4",
                UserName = "Test4",
                Email = "Test4@gmail",
                Avatar = "default.png",
                RealName = "Test 4",
                DOB = new DateTime(2020, 09, 29)
            };

            List<TeacherRequest> list = new List<TeacherRequest>() {
                new TeacherRequest{
                    Id = user.Id,
                    Code = user.UserName,
                    Email = user.Email,
                    RealName = user.RealName,
                    Dob = user.DOB.ToString("yyyy-MM-dd")
                },
                new TeacherRequest{
                    Id = user2.Id,
                    Code = user2.UserName,
                    Email = user2.Email,
                    RealName = "Test",
                    Dob = user2.DOB.ToString("yyyy-MM-dd")
                }
                };

            List<TeacherInfoResponse> listres = new List<TeacherInfoResponse>() {
                new TeacherInfoResponse{
                    Id = user.Id,
                    Code = user.UserName,
                    Email = user.Email,
                    RealName = user.RealName,
                    Dob = user.DOB.ToString("yyyy-MM-dd")
                } };

            fakeUserManager.Setup(x => x.FindByIdAsync(user.Id)).ReturnsAsync(userDB);
            fakeUserManager.Setup(x => x.FindByIdAsync(user2.Id)).ReturnsAsync(user);
            fakeUserManager.Setup(x => x.IsInRoleAsync(It.IsAny<AppUser>(), "student")).ReturnsAsync(true);
            fakeUserManager.Setup(x => x.FindByNameAsync(user2.UserName)).ReturnsAsync(user);

            //Act
            var result = await _studentService.Object.UpdateStudent(list);
            var okObjectResult = result as OkObjectResult;
            var model = okObjectResult.Value as Dictionary<String, object>;
            var keys = model.Keys.ToArray();
            var values = model.Values.ToArray();

            var listSuccess = (List<TeacherInfoResponse>)values[0];

            var listFailed = (List<Error>)values[1];

            //Assert
            Assert.Equal(200, okObjectResult.StatusCode);
            Assert.IsType<OkObjectResult>(okObjectResult);
            Assert.Equal(2, model.Count);
            Assert.Equal("success", keys[0].ToString());
            Assert.Equal("failed", keys[1].ToString());

            //check response 1
            Assert.Equal(listres[0].Id, listSuccess[0].Id);
            Assert.Equal(listres[0].Code, listSuccess[0].Code);
            Assert.Equal(listres[0].Email, listSuccess[0].Email);
            Assert.Equal(listres[0].RealName, listSuccess[0].RealName);
            Assert.Equal(listres[0].Dob, listSuccess[0].Dob);

            //check list failed = empty
            //Assert.Equal(listFailed, values[1]);
            Assert.Equal("Student Code " + user2.UserName + " already exists", listFailed[0].Message);
            Assert.Equal(2, listFailed[0].Type);
        }
    }
}
