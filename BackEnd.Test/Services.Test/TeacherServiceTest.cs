using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Security;
using BackEnd.Services;
using EmailService;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;

namespace BackEnd.Test.Services.Test
{
    public class TeacherServiceTest
    {

        private Mock<FakeSignInManager> fakeSignInManager;

        private Mock<FakeUserManager> fakeUserManager;

        private UserDbContext _userContext;
        private RoomDBContext _roomContext;

        private Mock<JwtGenerator> _jwtGenerator;

        private ConnectionFactory factory;

        private Mock<TeacherService> _teacherService;

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
            var attendanceDAO = new Mock<AttendanceDAO>();
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
                _roomContext, logDAO.Object, attendanceDAO, new EmailSender(emailConfig));
        }


        public void GetTeacher_Returns_Correctly()
        {

        }

    }
}
