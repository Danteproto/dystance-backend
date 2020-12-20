using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Xunit;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Microsoft.Extensions.Primitives;
using BackEnd.Requests;
using BackEnd.Responses;

namespace BackEnd.Test.Services.Test
{
    public class SemesterServiceTest
    {
        private Mock<SemesterService> _SemetserService { get; }
        private UserDbContext userContext;
        private RoomDBContext roomContext;
        private Mock<AttendanceDAO> attendanceDAO;
        private Mock<LogDAO> logDAO;
        private ConnectionFactory factory;

        public SemesterServiceTest()
        {
            factory = new ConnectionFactory();
            userContext = factory.CreateUserDbContextForInMemory();
            userContext.Database.EnsureDeleted();
            userContext.Database.EnsureCreated();
            userContext.SaveChanges();

            roomContext = factory.CreateRoomDbContextForInMemory();
            roomContext.Database.EnsureDeleted();
            roomContext.Database.EnsureCreated();
            roomContext.SaveChanges();

            attendanceDAO = new Mock<AttendanceDAO>(userContext);
            logDAO = new Mock<LogDAO>(userContext);
            var iAttendanceDao = attendanceDAO.As<IAttendanceDAO>();
            var iLogDAO = logDAO.As<ILogDAO>();
            //mocking user manager
            var users = new List<AppUser>
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

            var fakeUserManager = new Mock<FakeUserManager>();
            fakeUserManager.Setup(x => x.Users).Returns(users.AsQueryable());

            fakeUserManager.Setup(x => x.DeleteAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);
            fakeUserManager.Setup(x => x.CreateAsync(It.IsAny<AppUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
            fakeUserManager.Setup(x => x.UpdateAsync(It.IsAny<AppUser>()))
            .ReturnsAsync(IdentityResult.Success);

            var mockEnvironment = new Mock<IWebHostEnvironment>();
            //...Setup the mock as needed
            mockEnvironment
                .Setup(m => m.EnvironmentName)
                .Returns("Hosting:UnitTestEnvironment");
            mockEnvironment.Setup(m => m.ContentRootPath).Returns("");
            _SemetserService = new Mock<SemesterService>(fakeUserManager.Object, userContext, roomContext, mockEnvironment.Object, iAttendanceDao.Object, iLogDAO.Object);
            var semester = new Semester()
            {
                Id = 1,
                Name = "testSemester",
                File = "TestFile.xsl",
                LastUpdated = DateTime.Now
            };
            SemesterDAO.Create(roomContext, semester);

            var room = new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            };
            RoomDAO.Create(roomContext, room);

            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 2, UserId = "testUser2" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser1" });

            // Act
            RoomUserLinkDAO.Create(roomContext, links);

            var schedule = new Timetable()
            {
                Id = 1,
                RoomId = 1,
                Date = DateTime.Now.Date,
                StartTime = TimeSpan.Parse("8:00"),
                EndTime = TimeSpan.Parse("10:00"),
            };
            // Act
            TimetableDAO.Create(roomContext, schedule);
        }

        [Fact]
        public async void UpdateSemester()
        {
            var request = new Mock<HttpRequest>();
            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "name", "SemesterTest" },
                {"id","1" }
            });
            request.Setup(req => req.Form).Returns(formCol);
            var result = await _SemetserService.Object.UpdateSemester(request.Object);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var resultSemester = SemesterDAO.GetById(roomContext, 1);
            Assert.Equal("SemesterTest", resultSemester.Name);
        }
        [Fact]
        public async void DeleteSemester()
        {
            var ids = new List<string>();
            ids.Add("1");
            var result = await _SemetserService.Object.DeleteSemester(ids);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var semester = SemesterDAO.GetById(roomContext, 1);
            Assert.Null(semester);
        }
        [Fact]
        public async void AddScheduleFailNoClass()
        {
            var model = new ScheduleRequest()
            {
                date = DateTime.Now.Date.ToString(),
                startTime = "8:00",
                endTime = "10:00",
                subject = "testSubject123",
                @class = "testName"
            };
            var result = await _SemetserService.Object.AddSchedule(1, model);
            Assert.Equal((int)HttpStatusCode.BadRequest, ((ObjectResult)result).StatusCode);

        }
        [Fact]
        public async void AddscheduleFailOverlap()
        {
            var model = new ScheduleRequest()
            {
                date = DateTime.Now.Date.ToString(),
                startTime = "8:00",
                endTime = "10:00",
                subject = "testSubject",
                @class = "testName"
            };
            var result = await _SemetserService.Object.AddSchedule(1, model);
            Assert.Equal((int)HttpStatusCode.BadRequest, ((ObjectResult)result).StatusCode);

        }
        [Fact]
        public async void Addschedule()
        {
            var model = new ScheduleRequest()
            {
                date = DateTime.Now.Date.ToString(),
                startTime = "11:00",
                endTime = "13:00",
                subject = "testSubject",
                @class = "testName"
            };
            var result = await _SemetserService.Object.AddSchedule(1, model);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

        }
        [Fact]
        public async void UpdateSchedule()
        {
            var models = new List<ScheduleRequest>();
            models.Add(new ScheduleRequest()
            {
                id = "1",
                date = DateTime.Now.Date.ToString(),
                startTime = "11:00",
                endTime = "13:00",
                subject = "testSubject",
                @class = "testName"
            });
            var result = await _SemetserService.Object.UpdateSchedule(1, models);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var schedule = TimetableDAO.GetById(roomContext, 1);
            Assert.Equal(schedule.StartTime.ToString(), "11:00:00");
            Assert.Equal(schedule.EndTime.ToString(), "13:00:00");

        }
        [Fact]
        public async void UpdateScheduleFailClassNotExist()
        {
            var models = new List<ScheduleRequest>();
            models.Add(new ScheduleRequest()
            {
                id = "1",
                date = DateTime.Now.Date.ToString(),
                startTime = "11:00",
                endTime = "13:00",
                subject = "testSubjectasd",
                @class = "testName"
            });
            var result = await _SemetserService.Object.UpdateSchedule(1, models);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var schedule = TimetableDAO.GetById(roomContext, 1);
            Assert.NotEqual(schedule.StartTime.ToString(), "11:00:00");
            Assert.NotEqual(schedule.EndTime.ToString(), "13:00:00");

        }
        [Fact]
        public async void UpdateScheduleFailOverlap()
        {
            var models = new List<ScheduleRequest>();
            models.Add(new ScheduleRequest()
            {
                id = "1",
                date = DateTime.Now.Date.ToString(),
                startTime = "8:30",
                endTime = "10:30",
                subject = "testSubjectasd",
                @class = "testName"
            });
            var result = await _SemetserService.Object.UpdateSchedule(1, models);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var schedule = TimetableDAO.GetById(roomContext, 1);
            Assert.NotEqual(schedule.StartTime.ToString(), "8:30:00");
            Assert.NotEqual(schedule.EndTime.ToString(), "10:30:00");

        }
        [Fact]
        public async void DeleteSchedule()
        {
            var models = new List<string>();
            models.Add("1");
            var result = await _SemetserService.Object.DeleteSchedule(models);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var schedule = TimetableDAO.GetById(roomContext, 1);
            Assert.Null(schedule);

        }
        [Fact]
        public async void AddClass()
        {
            var model = new ClassRequest()
            {
                @class = "testName1",
                subject="testSubject1",
                teacher="testUser",
                students = new List<string> { "testUser1", "TestUser2", "TestUser3" }
            };
            var result = await _SemetserService.Object.AddClass(1,model);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var response = ((ObjectResult)result).Value as ClassResponse;
            Assert.Equal(response.@class, "testName1");
            Assert.Equal(response.subject, "testSubject1");

        }
        [Fact]
        public async void AddClassFailClassAlreadyExist()
        {
            var model = new ClassRequest()
            {
                @class = "testName",
                subject = "testSubject",
                teacher = "testUser",
                students = new List<string> { "testUser1", "TestUser2", "TestUser3" }
            };
            var result = await _SemetserService.Object.AddClass(1, model);
            Assert.Equal((int)HttpStatusCode.BadRequest, ((ObjectResult)result).StatusCode);

        }
        [Fact]
        public async void UpdateClassFailClassAlreadyExist()
        {
            var model = new ClassRequest()
            {
                @class = "testName1",
                subject = "testSubject1",
                teacher = "testUser",
                students = new List<string> { "testUser1", "TestUser2", "TestUser3" }
            };
            var result = await _SemetserService.Object.AddClass(1, model);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var models = new List<ClassRequest>();
            models.Add( new ClassRequest()
            {
                id = "2",
                @class = "testName",
                subject = "testSubject",
                teacher = "testUser",
                students = new List<string> { "testUser1", "TestUser2", "TestUser3" }
            });
            result = await _SemetserService.Object.UpdateClass(1, models);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var response = RoomDAO.Get(roomContext, 2);
            Assert.Equal(response.ClassName, "testName1");
            Assert.Equal(response.Subject, "testSubject1");

        }
        [Fact]
        public async void UpdateClassSuccessful()
        {
            var model = new ClassRequest()
            {
                @class = "testName1",
                subject = "testSubject1",
                teacher = "testUser",
                students = new List<string> { "testUser1", "TestUser2", "TestUser3" }
            };
            var result = await _SemetserService.Object.AddClass(1, model);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var models = new List<ClassRequest>();
            models.Add(new ClassRequest()
            {
                id = "2",
                @class = "testName2",
                subject = "testSubject3",
                teacher = "testUser",
                students = new List<string> { "testUser1", "TestUser2", "TestUser3" }
            });
            result = await _SemetserService.Object.UpdateClass(1, models);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var response = RoomDAO.Get(roomContext, 2);
            Assert.Equal(response.ClassName, "testName2");
            Assert.Equal(response.Subject, "testSubject3");

        }
        [Fact]
        public async void DeleteClass()
        {
            var models = new List<string>();
            models.Add("1");
            var result = await _SemetserService.Object.DeleteClass(models);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
    }
}
