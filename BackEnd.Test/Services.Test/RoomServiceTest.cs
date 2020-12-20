using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using Xunit;

namespace BackEnd.Test.Services.Test
{
    public class RoomServiceTest
    {
        private ConnectionFactory factory;
        private RoomDBContext _context;
        private Mock<IWebHostEnvironment> _env;
        public RoomServiceTest()
        {
            //setting up context
            factory = new ConnectionFactory();
            _context = factory.CreateRoomDbContextForInMemory();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _context.SaveChanges();
            var room = new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            };

            RoomDAO.Create(_context, room);

            var chats = new List<RoomChat>();

            chats.Add(new RoomChat() { Id = 1, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 2, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 3, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });

            foreach (var chat in chats)
            {
                RoomChatDAO.Create(_context, chat);
            }

            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 2, UserId = "testUser1" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser2" });

            RoomUserLinkDAO.Create(_context, links);


            _env = new Mock<IWebHostEnvironment>();
            _env.Setup(f => f.ContentRootPath).Returns("");
        }

        [Fact]
        public async void DeleteRoomSuccessful()
        {
            var result = await RoomService.DeleteRoom(_context, 1, _env.Object);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var room = RoomDAO.Get(_context, 1);

            Assert.Null(room);
        }
        [Fact]
        public async void DeleteRoomFail()
        {
            var result = await RoomService.DeleteRoom(_context, 2, _env.Object);
            Assert.Equal((int)HttpStatusCode.BadRequest, ((BadRequestObjectResult)result).StatusCode);

        }
        [Fact]
        public async void CreateRoomChatWithImage()
        {
            var request = new Mock<HttpRequest>();

            var formFile = new Mock<IFormFile>();
            var PhysicalFile = new FileInfo(@"../../../File/software-762486_1920.jpg");
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
                { "chatType", "1" },
                { "roomId","1"},
                { "userId","TestUser"},
                { "content",""}
            }, files);
            request.Setup(req => req.Form).Returns(formCol);

            var result = await RoomService.CreateRoomChat(_context, request.Object, _env.Object);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void CreateRoomChatWithFile()
        {
            var request = new Mock<HttpRequest>();

            var formFile = new Mock<IFormFile>();
            var PhysicalFile = new FileInfo(@"../../../File/software-762486_1920.jpg");
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
                { "chatType", "2" },
                { "roomId","1"},
                { "userId","TestUser"},
                { "content",""}
            }, files);
            request.Setup(req => req.Form).Returns(formCol);

            var result = await RoomService.CreateRoomChat(_context, request.Object, _env.Object);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void CreateRoomChat()
        {
            var request = new Mock<HttpRequest>();

            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "chatType", "0" },
                { "roomId","1"},
                { "userId","TestUser"},
                { "content",""}
            });
            request.Setup(req => req.Form).Returns(formCol);

            var result = await RoomService.CreateRoomChat(_context, request.Object, _env.Object);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void CreateRoomChatFail()
        {
            var request = new Mock<HttpRequest>();

            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "chatType", "3" },
                { "roomId","1"},
                { "userId","TestUser"},
                { "content",""}
            });
            request.Setup(req => req.Form).Returns(formCol);

            var result = await RoomService.CreateRoomChat(_context, request.Object, _env.Object);
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public void GetUserByRoom()
        {
            var list = RoomService.GetUsersByRoom(_context, 1);
            Assert.Equal(3, list.Count);
        }
        [Fact]
        public void GetChatByROomId()
        {
            var chats = new List<RoomChat>();

            chats.Add(new RoomChat() { Id = 1, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 2, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 3, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });

            foreach (var chat in chats)
            {
                RoomChatDAO.Create(_context, chat);
            }
            var rChats = RoomService.GetChatByRoomId(_context, 1);
            Assert.Equal(3, rChats.Count);
        }
        [Fact]
        public void GetLastChat()
        {

            var result = RoomService.GetLastChat(_context, 1);
            Assert.Equal(3, result.Id);
            Assert.Equal(1, result.RoomId);
            Assert.Equal("testUser", result.UserId);
            Assert.Equal("testChat", result.Content);
            Assert.Equal(1, result.Type);
        }
        [Fact]
        public void CreateGroup()
        {
            var request = new Mock<HttpRequest>();

            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                {"name","GroupTest" },
                {"teacherId","testTeacher" },
                {"roomId","1" },
                { "userIds","[\"testUser1\",\"testUser2\"]"}
            });
            request.Setup(req => req.Form).Returns(formCol);

            var group = RoomService.CreateGroup(_context, request.Object);
            Assert.Equal("GroupTest", group.Name);
            Assert.Equal(2, group.UserIds.Count);
            Assert.Equal(group.StartTime, group.EndTime);
        }
        [Fact]
        public void UpdateGroup()
        {
            var request = new Mock<HttpRequest>();

            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                {"name","GroupTest" },
                {"teacherId","testTeacher" },
                {"roomId","1" },
                { "userIds","[\"testUser1\",\"testUser2\"]"}
            });
            request.Setup(req => req.Form).Returns(formCol);

            var group = RoomService.CreateGroup(_context, request.Object);
            formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                {"groupId",group.GroupId.ToString() },
                { "userIds","[\"testUser\",\"testUser1\",\"testUser3\"]"}
            });
            request.Setup(req => req.Form).Returns(formCol);
            group = RoomService.GroupUpdate(_context, request.Object);
            Assert.Equal("GroupTest", group.Name);
            Assert.Equal(3, group.UserIds.Count);
        }
        [Fact]
        public void GetGroupByRoomId()
        {
            var request = new Mock<HttpRequest>();

            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                {"name","GroupTest" },
                {"teacherId","testTeacher" },
                {"roomId","1" },
                { "userIds","[\"testUser1\",\"testUser2\"]"}
            });
            request.Setup(req => req.Form).Returns(formCol);

            var group = RoomService.CreateGroup(_context, request.Object);
            Assert.Equal("GroupTest", group.Name);
            Assert.Equal(2, group.UserIds.Count);
            Assert.Equal(group.StartTime, group.EndTime);

            var groups = RoomService.GetGroupByRoomId(_context, 1);
            Assert.Single(groups);
            Assert.Equal("GroupTest", groups[0].Name);
            Assert.Equal(2, groups[0].UserIds.Count);

        }
        [Fact]
        public async void ResetGroup()
        {
            var request = new Mock<HttpRequest>();

            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                {"name","GroupTest" },
                {"teacherId","testTeacher" },
                {"roomId","1" },
                { "userIds","[\"testUser1\",\"testUser2\"]"}
            });
            request.Setup(req => req.Form).Returns(formCol);

            var group = RoomService.CreateGroup(_context, request.Object);
            Assert.Equal("GroupTest", group.Name);
            Assert.Equal(2, group.UserIds.Count);
            Assert.Equal(group.StartTime, group.EndTime);
            var result = await RoomService.ResetGroup(_context, 1, _env.Object);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

        }
        [Fact]
        public async void SerGroupTIme()
        {
            var request = new Mock<HttpRequest>();

            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                {"name","GroupTest" },
                {"teacherId","testTeacher" },
                {"roomId","1" },
                { "userIds","[\"testUser1\",\"testUser2\"]"}
            });
            request.Setup(req => req.Form).Returns(formCol);

            var group = RoomService.CreateGroup(_context, request.Object);
            Assert.Equal("GroupTest", group.Name);
            Assert.Equal(2, group.UserIds.Count);
            Assert.Equal(group.StartTime, group.EndTime);
            var now = DateTime.Now;
            formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "roomId", "1" },
                { "startTime",now.ToString() },
                { "duration", "15" }
            });
            request.Setup(req => req.Form).Returns(formCol);
            var result = await RoomService.SetGroupTime(_context, request.Object);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var groups = RoomService.GetGroupByRoomId(_context, 1);
            Assert.Single(groups);
            Assert.Equal("GroupTest", groups[0].Name);
            Assert.Equal(2, groups[0].UserIds.Count);
            Assert.Equal(groups[0].EndTime.AddMinutes(-15), groups[0].StartTime);
        }
        [Fact]
        public async void GetRoomBySemetser()
        {
            var result = await RoomService.GetRoomsBySemesterId(_context, 1);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
    }
}
