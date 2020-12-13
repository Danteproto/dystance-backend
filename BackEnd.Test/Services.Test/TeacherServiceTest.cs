using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Requests;
using BackEnd.Responses;
using BackEnd.Security;
using BackEnd.Services;
using EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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


            _teacherService = new Mock<TeacherService>(_userContext, fakeUserManager.Object,
                _roomContext, logDAO.Object, attendanceDAO.Object, new EmailSender(emailConfig), new UrlHelperFactory(), new ActionContextAccessor());
        }

        //[Fact]
        //public async void GetTeacher_Returns_Correctly()
        //{
        //    //Arrange
        //    var teacherResponses = new List<TeacherInfoResponse>()
        //    {
        //        new TeacherInfoResponse{
        //            Id = "TestId001",
        //            Code = "TestCode001",
        //            Email = "TestEmail001",
        //            RealName = "TestName001",
        //            Dob = "TestDob001"
        //        },
        //        new TeacherInfoResponse{
        //            Id = "TestId002",
        //            Code = "TestCode002",
        //            Email = "TestEmail002",
        //            RealName = "TestName002",
        //            Dob = "TestDob002"
        //        },
        //        new TeacherInfoResponse{
        //            Id = "TestId003",
        //            Code = "TestCode003",
        //            Email = "TestEmail003",
        //            RealName = "TestName003",
        //            Dob = "TestDob003"
        //        }
        //    };

        //    _teacherService.Setup(x => x.GetTeacher()).Returns(Task.FromResult<IActionResult>(new OkObjectResult(teacherResponses)));

        //    //Act
        //    var result = await _teacherService.Object.GetTeacher();




        //    //Assert
        //}


        //[Fact]
        //public async void AddTeacher(TeacherRequest teacherRequest)
        //{

        //}

    }
}
