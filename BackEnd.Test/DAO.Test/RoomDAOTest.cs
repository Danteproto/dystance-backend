using BackEnd.Context;
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
    public class RoomDAOTest
    {
        private ConnectionFactory factory;
        private RoomDBContext roomContext;
        private UserDbContext userContext;
        public RoomDAOTest()
        {
            factory = new ConnectionFactory();
            roomContext = factory.CreateRoomDbContextForInMemory();
            roomContext.Database.EnsureDeleted();
            roomContext.Database.EnsureCreated();
            roomContext.SaveChanges();
        }

        //Test create
        [Fact]
        public async Task RoomCreateSuccessfully()
        {
            //Arrange
            var room = new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            };
            // Act
            var result = await RoomDAO.Create(roomContext, room);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }

        [Fact]
        public async Task RoomCreateFail()
        {
            var result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });
            result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });
            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async Task RoomUpdateSuccessfully()
        {
            //Arrange
            var room = new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            };
            // Act
            var result = await RoomDAO.Create(roomContext, room);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            room.Subject = "testSubject2";
            room.ClassName = "testName2";
            result = RoomDAO.UpdateRoom(roomContext, room);
            var resultRoom = RoomDAO.Get(roomContext, 1);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            Assert.Equal(resultRoom.RoomId, room.RoomId);
            Assert.Equal(resultRoom.Subject, room.Subject);
            Assert.Equal(resultRoom.ClassName, room.ClassName);
            Assert.Equal(resultRoom.CreatorId, room.CreatorId);
            Assert.Equal(resultRoom.SemesterId, room.SemesterId);
        }
        [Fact]
        public async Task RoomUpdateFail()
        {
            //Arrange
            var room = new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            };
            // Act
            var result = await RoomDAO.Create(roomContext, room);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            room.RoomId = 2;
            room.Subject = "testSubject2";
            room.ClassName = "testName2";
            result = RoomDAO.UpdateRoom(roomContext, room);
            var resultRoom = RoomDAO.Get(roomContext, 1);
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.Equal(2, resultRoom.RoomId);
            Assert.Equal(resultRoom.Subject, room.Subject);
            Assert.Equal(resultRoom.ClassName, room.ClassName);
            Assert.Equal(resultRoom.CreatorId, room.CreatorId);
            Assert.Equal(resultRoom.SemesterId, room.SemesterId);
        }

        [Fact]
        public async Task DeleteSuccessfull()
        {
            var room = new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            };
            // Act
            var result = await RoomDAO.Create(roomContext, room);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await RoomDAO.Delete(roomContext, room);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            room = RoomDAO.Get(roomContext, 1);

            Assert.Null(room);

        }

        [Fact]
        public async Task GetLastRoom()
        {
            var result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var room = RoomDAO.GetLastRoom(roomContext);

            Assert.NotNull(room);
            Assert.Equal(1, room.RoomId);
            Assert.Equal("testSubject", room.Subject);
            Assert.Equal("testName", room.ClassName);
            Assert.Equal("testUser", room.CreatorId);
            Assert.Equal(1, room.SemesterId);

            result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 2,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            room = RoomDAO.GetLastRoom(roomContext);

            Assert.NotNull(room);
            Assert.Equal(2, room.RoomId);
            Assert.Equal("testSubject", room.Subject);
            Assert.Equal("testName", room.ClassName);
            Assert.Equal("testUser", room.CreatorId);
            Assert.Equal(1, room.SemesterId);
        }

        [Fact]
        public async Task GetRoom()
        {
            var result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 2,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var room = RoomDAO.Get(roomContext, 1);

            Assert.NotNull(room);
            Assert.Equal(1, room.RoomId);
            Assert.Equal("testSubject", room.Subject);
            Assert.Equal("testName", room.ClassName);
            Assert.Equal("testUser", room.CreatorId);
            Assert.Equal(1, room.SemesterId);

            room = RoomDAO.Get(roomContext, 2);

            Assert.NotNull(room);
            Assert.Equal(2, room.RoomId);
            Assert.Equal("testSubject", room.Subject);
            Assert.Equal("testName", room.ClassName);
            Assert.Equal("testUser", room.CreatorId);
            Assert.Equal(1, room.SemesterId);
        }
        [Fact]
        public async Task GetRoomBySemester()
        {
            var result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 2,
                Subject = "testSubject2",
                ClassName = "testName2",
                CreatorId = "testUser",
                SemesterId = 1
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 3,
                Subject = "testSubject2",
                ClassName = "testName2",
                CreatorId = "testUser",
                SemesterId = 2
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var room = RoomDAO.GetRoomBySemester(roomContext, 1);

            Assert.NotNull(room);
            Assert.Equal(2, room.Count);

           
        }
        [Fact]
        public async Task GetRoomByClassAndSubject()
        {
            var result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 2,
                Subject = "testSubject2",
                ClassName = "testName2",
                CreatorId = "testUser",
                SemesterId = 1
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var room = RoomDAO.GetRoomByClassAndSubject(roomContext, "testName", "testSubject");

            Assert.NotNull(room);
            Assert.Equal(1, room.RoomId);
            Assert.Equal("testSubject", room.Subject);
            Assert.Equal("testName", room.ClassName);
            Assert.Equal("testUser", room.CreatorId);
            Assert.Equal(1, room.SemesterId);

            room = RoomDAO.GetRoomByClassAndSubject(roomContext, "testName2", "testSubject2");


            Assert.NotNull(room);
            Assert.Equal(2, room.RoomId);
            Assert.Equal("testSubject2", room.Subject);
            Assert.Equal("testName2", room.ClassName);
            Assert.Equal("testUser", room.CreatorId);
            Assert.Equal(1, room.SemesterId);
        }
        [Fact]
        public async Task GetRoomByClassSubjectSemetser()
        {
            var result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 1,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 1
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await RoomDAO.Create(roomContext, new Room()
            {
                RoomId = 2,
                Subject = "testSubject",
                ClassName = "testName",
                CreatorId = "testUser",
                SemesterId = 2
            });

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var room = RoomDAO.GetRoomByClassSubjectSemester(roomContext, "testName", "testSubject", 1);

            Assert.NotNull(room);
            Assert.Equal(1, room.RoomId);
            Assert.Equal("testSubject", room.Subject);
            Assert.Equal("testName", room.ClassName);
            Assert.Equal("testUser", room.CreatorId);
            Assert.Equal(1, room.SemesterId);

            room = RoomDAO.GetRoomByClassSubjectSemester(roomContext, "testName", "testSubject",2);


            Assert.NotNull(room);
            Assert.Equal(2, room.RoomId);
            Assert.Equal("testSubject", room.Subject);
            Assert.Equal("testName", room.ClassName);
            Assert.Equal("testUser", room.CreatorId);
            Assert.Equal(2, room.SemesterId);
        }
    }
}
