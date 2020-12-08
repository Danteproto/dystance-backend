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
    public class ScheduleDAOTest
    {
        private ConnectionFactory factory;
        private RoomDBContext roomContext;
        public ScheduleDAOTest()
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
        public async void ScheduleCreateSuccessfully()
        {
            //Arrange
            var schedule = new Timetable()
            {
                Id = 1,
                RoomId = 1,
                Date = DateTime.Now.Date,
                StartTime = TimeSpan.Parse("8:00"),
                EndTime = TimeSpan.Parse("10:00"),
            };
            // Act
            var result = await TimetableDAO.Create(roomContext, schedule);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void ScheduleCreateFail()
        {
            //Arrange
            var schedule = new Timetable()
            {
                Id = 1,
                RoomId = 1,
                Date = DateTime.Now.Date,
                StartTime = TimeSpan.Parse("8:00"),
                EndTime = TimeSpan.Parse("10:00"),
            };
            // Act
            var result = await TimetableDAO.Create(roomContext, schedule);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            result = await TimetableDAO.Create(roomContext, schedule);

            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void ScheduleListCreateSuccessfully()
        {
            var schedules = new List<Timetable>();
            //Arrange
            schedules.Add(new Timetable() { Id = 1, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 2, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 3, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            // Act
            var result = await TimetableDAO.Create(roomContext, schedules);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void GetByRoomId()
        {
            var schedules = new List<Timetable>();
            //Arrange
            schedules.Add(new Timetable() { Id = 1, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 2, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 3, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            // Act
            var result = await TimetableDAO.Create(roomContext, schedules);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultList = TimetableDAO.GetByRoomId(roomContext, 1);
            Assert.Equal(3, resultList.Count);
        }
        [Fact]
        public async void GetByRoomAndDate()
        {
            var schedules = new List<Timetable>();
            //Arrange
            schedules.Add(new Timetable() { Id = 1, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 2, RoomId = 1, Date = DateTime.Now.AddDays(1).Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 3, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            // Act
            var result = await TimetableDAO.Create(roomContext, schedules);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultList = TimetableDAO.GetByRoomAndDate(roomContext, 1, DateTime.Now.Date);
            Assert.Equal(2, resultList.Count);
        }
        [Fact]
        public async void UpdateSchedule()
        {
            //Arrange
            var schedule = new Timetable()
            {
                Id = 1,
                RoomId = 1,
                Date = DateTime.Now.Date,
                StartTime = TimeSpan.Parse("8:00"),
                EndTime = TimeSpan.Parse("10:00"),
            };
            // Act
            var result = await TimetableDAO.Create(roomContext, schedule);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            schedule.Date = DateTime.Now.AddDays(1).Date;
            schedule.StartTime = TimeSpan.Parse("9:00");
            schedule.EndTime = TimeSpan.Parse("12:00");

            result = TimetableDAO.Update(roomContext, schedule);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            var resultSchedule = TimetableDAO.GetById(roomContext, 1);
            Assert.Equal(1, resultSchedule.RoomId);
            Assert.Equal(DateTime.Now.AddDays(1).Date, resultSchedule.Date);
            Assert.Equal(TimeSpan.Parse("9:00"), resultSchedule.StartTime);
            Assert.Equal(TimeSpan.Parse("12:00"), resultSchedule.EndTime);
        }
        [Fact]
        public async void ScheduleUpdateList()
        {
            var schedules = new List<Timetable>();
            //Arrange
            schedules.Add(new Timetable() { Id = 1, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 2, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 3, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            // Act
            var result = await TimetableDAO.Create(roomContext, schedules);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            schedules[0].Date = DateTime.Now.AddDays(1).Date;
            schedules[0].StartTime = TimeSpan.Parse("9:00");
            schedules[0].EndTime = TimeSpan.Parse("10:00");

            result = await TimetableDAO.Update(roomContext, schedules);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultSchedule = TimetableDAO.GetById(roomContext, 1);
            Assert.Equal(1, resultSchedule.RoomId);
            Assert.Equal(DateTime.Now.AddDays(1).Date, resultSchedule.Date);
            Assert.Equal(TimeSpan.Parse("9:00"), resultSchedule.StartTime);
            Assert.Equal(TimeSpan.Parse("10:00"), resultSchedule.EndTime);
        }
        [Fact]
        public async void ScheduleDeleteList()
        {
            var schedules = new List<Timetable>();
            //Arrange
            schedules.Add(new Timetable() { Id = 1, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 2, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            schedules.Add(new Timetable() { Id = 3, RoomId = 1, Date = DateTime.Now.Date, StartTime = TimeSpan.Parse("8:00"), EndTime = TimeSpan.Parse("10:00"), });
            // Act
            var result = await TimetableDAO.Create(roomContext, schedules);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            schedules.RemoveAt(2);

            result = await TimetableDAO.DeleteTimeTable(roomContext, schedules);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            var resultSchedule = TimetableDAO.GetById(roomContext, 1);
            Assert.Null(resultSchedule);
            resultSchedule = TimetableDAO.GetById(roomContext, 3);
            Assert.NotNull(resultSchedule);
        }
    }
}
