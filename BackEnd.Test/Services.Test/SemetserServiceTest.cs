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

namespace BackEnd.Test.Services.Test
{
    public class SemetserServiceTest
    {
        private Mock<SemesterService> _SemetserService { get; }
        private UserDbContext userContext;
        private RoomDBContext roomContext;
        private ConnectionFactory factory;

        public SemetserServiceTest()
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

            IAttendanceDAO attendanceDAO =(IAttendanceDAO) new AttendanceDAO(userContext);
            ILogDAO logDAO = (ILogDAO)new LogDAO(userContext);
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
            _SemetserService = new Mock<SemesterService>(fakeUserManager, userContext, roomContext, mockEnvironment, attendanceDAO,logDAO);
        }

        [Fact]
        public async void CreateSemester()
        {
            var request = new Mock<HttpRequest>();


            var formFile = new Mock<IFormFile>();
            var PhysicalFile = new FileInfo(@"../../../File/Semester-TestSuccess.xlsx");
            var memory = new MemoryStream();
            var writer = new StreamWriter(memory);
            writer.Write(PhysicalFile.OpenRead());
            writer.Flush();
            memory.Position = 0;
            var fileName = PhysicalFile.Name;

            formFile.Setup(_ => _.FileName).Returns(fileName);
            formFile.Setup(_ => _.Length).Returns(memory.Length);
            formFile.Setup(_ => _.OpenReadStream()).Returns(memory);
            formFile.Verify();
            var file = formFile.Object;

            var files = new FormFileCollection();
            files.Add(file);
            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "name", "SemesterTest" }
            },files);
            request.Setup(req => req.Form).Returns(formCol);
            await _SemetserService.Object.AddSemester(request.Object);
        }
    }
}
