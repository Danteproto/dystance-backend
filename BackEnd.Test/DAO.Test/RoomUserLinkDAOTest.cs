using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Xunit;

namespace BackEnd.Test.DAO.Test
{
    public class RoomUserLinkDAOTest
    {
        private ConnectionFactory factory;
        private RoomDBContext roomContext;
        public RoomUserLinkDAOTest()
        {
            factory = new ConnectionFactory();
            roomContext = factory.CreateRoomDbContextForInMemory();
            roomContext.Database.EnsureDeleted();
            roomContext.Database.EnsureCreated();
            roomContext.SaveChanges();

            var room = new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            };
            RoomDAO.Create(roomContext, room);
        }
        [Fact]
        public async Task LinkCreateSuccessfully()
        {
            //Arrange
            var link = new RoomUserLink()
            {
                RoomId = 1,
                RoomUserId = 1,
                UserId = "testUser"
            };
            // Act
            var result = await RoomUserLinkDAO.Create(roomContext, link);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task ListLinkCreateSuccessfully()
        {
            //Arrange
            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 2, UserId = "testUser1" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser2" });

            // Act
            var result = await RoomUserLinkDAO.Create(roomContext, links);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async Task ListLinkCreateFail()
        {
            //Arrange
            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser1" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser2" });

            // Act
            var result = await RoomUserLinkDAO.Create(roomContext, links);
            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async Task LinkCreateFail()
        {
            //Arrange
            var link = new RoomUserLink()
            {
                RoomId = 1,
                RoomUserId = 1,
                UserId = "testUser"
            };
            // Act
            var result = await RoomUserLinkDAO.Create(roomContext, link);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await RoomUserLinkDAO.Create(roomContext, new RoomUserLink()
            {
                RoomId = 1,
                RoomUserId = 1,
                UserId = "testUser"
            });
            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void GetRoomLink()
        {
            //Arrange
            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 2, UserId = "testUser1" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser2" });

            var result = await RoomUserLinkDAO.Create(roomContext, links);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultLinks = RoomUserLinkDAO.GetRoomLink(roomContext, 1);

            Assert.Equal(3, resultLinks.Count);
        }
        [Fact]
        public async void GetRoomUserLink()
        {
            //Arrange
            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 2, UserId = "testUser1" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser2" });

            var result = await RoomUserLinkDAO.Create(roomContext, links);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultLink = RoomUserLinkDAO.GetRoomUserLink(roomContext, 1, "testUser");

            Assert.Equal(1, resultLink.RoomId);
            Assert.Equal(1, resultLink.RoomUserId);
            Assert.Equal("testUser", resultLink.UserId);

            resultLink = RoomUserLinkDAO.GetRoomUserLink(roomContext, 1, "testUser0");

            Assert.Null(resultLink);
        }
        [Fact]
        public async void DeleteLinkSuccessfully()
        {
            //Arrange
            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 2, UserId = "testUser1" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser2" });

            var result = await RoomUserLinkDAO.Create(roomContext, links);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultLinks = RoomUserLinkDAO.GetRoomLink(roomContext, 1);

            Assert.Equal(3, resultLinks.Count);

            result = await RoomUserLinkDAO.Delete(roomContext, links[1]);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            resultLinks = RoomUserLinkDAO.GetRoomLink(roomContext, 1);

            Assert.Equal(2, resultLinks.Count);
        }
        [Fact]
        public async void DeleteLinkFail()
        {
            //Arrange
            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 2, UserId = "testUser1" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser2" });

            var result = await RoomUserLinkDAO.Create(roomContext, links);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultLinks = RoomUserLinkDAO.GetRoomLink(roomContext, 1);

            Assert.Equal(3, resultLinks.Count);

            result = await RoomUserLinkDAO.Delete(roomContext, new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser12" });

            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);

            resultLinks = RoomUserLinkDAO.GetRoomLink(roomContext, 1);

            Assert.Equal(3, resultLinks.Count);
        }
        [Fact]
        public async void DeleteListLinkSuccessfully()
        {
            //Arrange
            var links = new List<RoomUserLink>();
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 1, UserId = "testUser" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 2, UserId = "testUser1" });
            links.Add(new RoomUserLink() { RoomId = 1, RoomUserId = 3, UserId = "testUser2" });

            var result = await RoomUserLinkDAO.Create(roomContext, links);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultLinks = RoomUserLinkDAO.GetRoomLink(roomContext, 1);

            Assert.Equal(3, resultLinks.Count);

            result = await RoomUserLinkDAO.Delete(roomContext, links);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            resultLinks = RoomUserLinkDAO.GetRoomLink(roomContext, 1);

            Assert.Empty(resultLinks);
        }
    }
}
