using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using Xunit;

namespace BackEnd.Test.DAO.Test
{
    public class RoomChatDAOTest
    {
        private ConnectionFactory factory;
        private RoomDBContext roomContext;
        public RoomChatDAOTest()
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
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(999999)]
        public async void ChatCreateSuccessfully(int roomid)
        {
            //Arrange
            var chat = new RoomChat()
            {
                Id = 1,
                RoomId = roomid,
                UserId = "testUser",
                Date = DateTime.Now,
                Content = "testChat",
                Type = 1,
            };
            // Act
            var result = await RoomChatDAO.Create(roomContext, chat);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(999999)]
        public async void ChatCreateFail(int roomid)
        {
            //Arrange
            var chat = new RoomChat()
            {
                Id = 1,
                RoomId = roomid,
                UserId = "testUser",
                Date = DateTime.Now,
                Content = "testChat",
                Type = 1,
            };
            // Act
            var result = await RoomChatDAO.Create(roomContext, chat);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await RoomChatDAO.Create(roomContext, chat);

            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(999999)]
        public async void GetChatByRoomId(int roomId)
        {
            var chats = new List<RoomChat>();
            //Arrange
            chats.Add(new RoomChat() { Id = 1, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 2, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 3, RoomId = 1, UserId = "testUser", Date = DateTime.Now, Content = "testChat", Type = 1, });

            // Act
            foreach (var chat in chats)
            {
                await RoomChatDAO.Create(roomContext, chat);
            }

            var result = RoomChatDAO.GetChatByRoomId(roomContext, roomId);
            if(roomId == 1) { 
                Assert.Equal(3, result.Count);
            }
            else
            {
                Assert.Empty(result);
            }

        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(999999)]
        public async void GetLastChat(int roomId)
        {
            var now = DateTime.Now;
            var chats = new List<RoomChat>();
            //Arrange
            chats.Add(new RoomChat() { Id = 1, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 2, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 3, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });

            // Act
            foreach (var chat in chats)
            {
                await RoomChatDAO.Create(roomContext, chat);
            }

            var result = RoomChatDAO.GetLastChat(roomContext, roomId);
            
            Assert.Equal(3, result.Id);
            Assert.Equal(roomId, result.RoomId);
            Assert.Equal("testUser", result.UserId);
            Assert.Equal("testChat", result.Content);
            Assert.Equal(1, result.Type);         
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(999999)]
        public async void DeleteChat_Successful(int roomId)
        {
            var now = DateTime.Now;
            var chats = new List<RoomChat>();
            //Arrange
            chats.Add(new RoomChat() { Id = 1, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 2, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 3, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });

            // Act
            foreach (var chat in chats)
            {
                await RoomChatDAO.Create(roomContext, chat);
            }

            var result = RoomChatDAO.DeleteRoomChat(roomContext, chats);
            var resultChat = RoomChatDAO.GetChatByRoomId(roomContext, roomId);
            Assert.Empty(resultChat);
        }

        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(999999)]
        public async void DeleteChat_FAIL(int roomId)
        {
            var now = DateTime.Now;
            var chats = new List<RoomChat>();
            var chats2 = new List<RoomChat>();
            //Arrange
            chats.Add(new RoomChat() { Id = 1, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });
            chats.Add(new RoomChat() { Id = 2, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });
            chats2.Add(new RoomChat() { Id = 3, RoomId = roomId, UserId = "testUser", Date = now, Content = "testChat", Type = 1, });

            // Act
            foreach (var chat in chats)
            {
                await RoomChatDAO.Create(roomContext, chat);
            }

            var result = RoomChatDAO.DeleteRoomChat(roomContext, chats2);
            var resultChat = RoomChatDAO.GetChatByRoomId(roomContext, roomId);
            Assert.NotEmpty(resultChat);
        }
    }
}
