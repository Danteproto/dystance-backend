using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using static BackEnd.Constant.Log;

namespace BackEnd.Test.DAO.Test
{
    public class LogDAOTest
    {
        private ConnectionFactory factory;
        private UserDbContext context;
        private readonly LogDAO _logDAO;
        public LogDAOTest()
        {
            factory = new ConnectionFactory();
            context = factory.CreateUserDbContextForInMemory();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SaveChanges();
            _logDAO = new LogDAO(context);
        }


        //Test CreateLog
        [Theory]
        [InlineData(LogType.ATTENDANCE_JOIN)]
        [InlineData(LogType.ATTENDANCE_LEAVE)]
        [InlineData(LogType.PRIVATE_CHAT_IMAGE)]
        [InlineData(LogType.PRIVATE_CHAT_FILE)]
        [InlineData(LogType.PRIVATE_CHAT_MESSAGE)]
        [InlineData(LogType.ROOM_CHAT_IMAGE)]
        [InlineData(LogType.ROOM_CHAT_FILE)]
        [InlineData(LogType.ROOM_CHAT_TEXT)]
        [InlineData(LogType.KICK)]
        [InlineData(LogType.MUTE)]
        [InlineData(LogType.GOT_MUTED)]
        [InlineData(LogType.TOGGLE_WHITEBOARD)]
        [InlineData(LogType.REMOTE_CONTROL_PERMISSION)]
        [InlineData(LogType.REMOTE_CONTROL_ACCEPT)]
        [InlineData(LogType.REMOTE_CONTROL_REJECT)]
        [InlineData(LogType.REMOTE_CONTROL_STOP)]
        [InlineData(LogType.WHITEBOARD_ALLOW)]
        [InlineData(LogType.WHITEBOARD_DISABLE)]
        [InlineData(LogType.GROUP_CREATE)]
        [InlineData(LogType.GROUP_DELETE)]
        [InlineData(LogType.GROUP_START)]
        [InlineData(LogType.GROUP_STOP)]
        [InlineData(LogType.GROUP_JOIN)]
        [InlineData(LogType.GROUP_LEAVE)]
        [InlineData(LogType.PRIVATE_CHAT_TEXT)]
        [InlineData(LogType.SCREEN_SHARE_START)]
        [InlineData(LogType.SCREEN_SHARE_STOP)]
        public async Task TestCreateLogSuccess(LogType logType)
        {
            //Arrange
            var userLog = new UsersLog()
            {
                UserId = "1",
                DateTime = DateTime.Parse("2020-02-02"),
                Description = "test",
                LogType = logType.ToString(),
                RoomId = "1"
            };

            //Act
            var result = await _logDAO.CreateLog(userLog);
            var list = await context.UserLog.ToListAsync();
            var justAdded = list[0];


            //Assert
            Assert.Contains("Success", result);
            Assert.NotEmpty(list);
            Assert.Equal("1", justAdded.UserId);
            Assert.Equal(DateTime.Parse("2020-02-02"), justAdded.DateTime);
            Assert.Equal("test", justAdded.Description);
            Assert.Equal(logType.ToString(), justAdded.LogType);
            Assert.Equal("1", justAdded.RoomId);
        }

        [Fact]
        public async Task TestCreateLogFail()
        {
            //Arrange
            var userLog = new UsersLog()
            {
                UsersLogId = 1,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            };
            await _logDAO.CreateLog(userLog);

            var userLog2 = new UsersLog()
            {
                UsersLogId = 1,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            };
            //Act
            var result = await _logDAO.CreateLog(userLog2);
            var list = await context.UserLog.ToListAsync();


            //Assert
            Assert.Contains("Error", result);
            Assert.Single(list);
        }




        //Test DeleteLogs
        [Fact]
        public async Task TestDeleteLogsSuccess()
        {
            //Arrange
            var list = new List<UsersLog>();
            list.Add(new UsersLog()
            {
                UsersLogId = 1,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });
            list.Add(new UsersLog()
            {
                UsersLogId = 2,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });
            list.Add(new UsersLog()
            {
                UsersLogId = 3,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });

            context.UserLog.AddRange(list);
            await context.SaveChangesAsync();

            //Act
            var result = await _logDAO.DeleteLogs(list);
            var listAfter = await context.UserLog.ToListAsync();


            //Assert
            Assert.Empty(listAfter);
            Assert.Contains("Success", result);
        }

        [Fact]
        public async Task TestDeleteLogsFail()
        {
            //Arrange
            var list = new List<UsersLog>();
            list.Add(new UsersLog()
            {
                UsersLogId = 1,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });
            list.Add(new UsersLog()
            {
                UsersLogId = 2,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });
            list.Add(new UsersLog()
            {
                UsersLogId = 3,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });

            context.UserLog.AddRange(list);
            await context.SaveChangesAsync();

            var listToDel = new List<UsersLog>();
            listToDel.Add(new UsersLog()
            {
                UsersLogId = 4,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });

            //Act
            var result = await _logDAO.DeleteLogs(listToDel);
            var listAfter = await context.UserLog.ToListAsync();


            //Assert
            Assert.NotEmpty(listAfter);
            Assert.Equal(3, listAfter.Count);
            Assert.Contains("Error", result);
        }



        //Test GetLogsByRoomId
        [Fact]
        public async Task TestGetLogsByRoomIdSuccess()
        {
            //Arrange
            var list = new List<UsersLog>();
            list.Add(new UsersLog()
            {
                UsersLogId = 1,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });
            list.Add(new UsersLog()
            {
                UsersLogId = 2,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "2"
            });
            list.Add(new UsersLog()
            {
                UsersLogId = 3,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });

            context.UserLog.AddRange(list);
            await context.SaveChangesAsync();

            //Act
            var result = await _logDAO.GetLogsByRoomId("1");

            //Assert
            Assert.NotEmpty(result);
            Assert.Equal(2, result.Count);
            Assert.Equal(1, result[0].UsersLogId);
            Assert.Equal(3, result[1].UsersLogId);
        }

        [Fact]
        public async Task TestGetLogsByRoomIdFail()
        {
            //Arrange
            var list = new List<UsersLog>();
            list.Add(new UsersLog()
            {
                UsersLogId = 1,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });
            list.Add(new UsersLog()
            {
                UsersLogId = 2,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "2"
            });
            list.Add(new UsersLog()
            {
                UsersLogId = 3,
                Description = "test",
                LogType = LogType.ATTENDANCE_JOIN.ToString(),
                RoomId = "1"
            });

            context.UserLog.AddRange(list);
            await context.SaveChangesAsync();

            //Act
            var result = await _logDAO.GetLogsByRoomId("4");

            //Assert
            Assert.Empty(result);
        }
    }
}
