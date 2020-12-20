using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using BackEnd.Models;
using BackEnd.DAO;
using BackEnd.Context;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.EntityFrameworkCore;
using System.Linq;

namespace BackEnd.Test.DAO.Test
{
    public class AttendanceDAOTest
    {
        private ConnectionFactory factory;
        private UserDbContext context;
        private readonly AttendanceDAO attendanceDAO;
        public AttendanceDAOTest()
        {
            factory = new ConnectionFactory();
            context = factory.CreateUserDbContextForInMemory();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SaveChanges();
            attendanceDAO = new AttendanceDAO(context);
        }

        [Fact]
        public async Task TesCreateOneAttendanceSuccessfully()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "2" });
            
            //Act
            var result = await attendanceDAO.CreateAttendance(listToUpdate);

            //Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            Assert.Single(context.AttendanceReports.ToList());
        }

        [Fact]
        public async Task TesCreateAttendancesSuccessfully()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 2, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 3, Status = "future", TimeTableId = 1, UserId = "2" });
            
            //Act
            var result = await attendanceDAO.CreateAttendance(listToUpdate);
            
            //Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            Assert.Equal( 3 , context.AttendanceReports.ToList().Count);
        }

        [Fact]
        public async Task TesCreateAttendancesFail()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "2" });

            //Act
            var result = await attendanceDAO.CreateAttendance(listToUpdate);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.Empty(context.AttendanceReports.ToList());
        }

        [Fact]
        public async Task TestUpdateAttendanceStatusSuccessfully()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 2, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 3, Status = "future", TimeTableId = 1, UserId = "2" });
            await context.AttendanceReports.AddRangeAsync(listToUpdate);
            await context.SaveChangesAsync();

            var listUpdate = await context.AttendanceReports.ToListAsync();
            foreach(var rp in listUpdate)
            {
                rp.Status = "present";
            }

            //Act
            var result = await attendanceDAO.UpdateAttendance(listUpdate);

            //Assert
            Assert.Equal((int)HttpStatusCode.OK,((ObjectResult)result).StatusCode);
            foreach (var rp in listUpdate)
            {
                Assert.Equal("present", rp.Status);
            }
        }

        [Fact]
        public async Task TestUpdateAttendanceTimetableIdAndStatusSuccessfully()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 2, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 3, Status = "future", TimeTableId = 1, UserId = "2" });
            await context.AttendanceReports.AddRangeAsync(listToUpdate);
            await context.SaveChangesAsync();

            var listUpdate = await context.AttendanceReports.ToListAsync();
            foreach (var rp in listUpdate)
            {
                rp.TimeTableId = 2;
            }

            foreach (var rp in listUpdate)
            {
                rp.Status = "present";
            }

            //Act
            var result = await attendanceDAO.UpdateAttendance(listUpdate);

            //Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            foreach (var rp in listUpdate)
            {
                Assert.Equal(2, rp.TimeTableId);
            }
            foreach (var rp in listUpdate)
            {
                Assert.Equal("present", rp.Status);
            }
        }

        [Fact]
        public async Task TestUpdateAttendanceTimetableIdSuccessfully()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 2, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 3, Status = "future", TimeTableId = 1, UserId = "2" });
            await context.AttendanceReports.AddRangeAsync(listToUpdate);
            await context.SaveChangesAsync();

            var listUpdate = await context.AttendanceReports.ToListAsync();
            foreach (var rp in listUpdate)
            {
                rp.TimeTableId = 2;
            }

            //Act
            var result = await attendanceDAO.UpdateAttendance(listUpdate);

            //Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            foreach (var rp in listUpdate)
            {
                Assert.Equal(2, rp.TimeTableId);
            }
        }

        [Fact]
        public async Task TestUpdateAttendanceFail_NoData()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            AttendanceReports attendance = new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "1" };
            listToUpdate.Add(attendance);
            //await context.AttendanceReports.AddAsync(attendance);
            //await context.SaveChangesAsync();

            attendance.AttendanceId = 2;
            attendance.Status = "present";
            attendance.TimeTableId = 2;
            attendance.UserId = "2";
            

            //Act
            var result = await attendanceDAO.UpdateAttendance(listToUpdate);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.Equal(2 , attendance.AttendanceId);
            Assert.Equal("present", attendance.Status);
            Assert.Equal(2 , attendance.TimeTableId);
            Assert.Equal("2" , attendance.UserId);
        }

        [Fact]
        public async Task TestUpdateAttendanceFail()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            AttendanceReports attendance = new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "1" };
            listToUpdate.Add(attendance);
            await context.AttendanceReports.AddAsync(attendance);
            await context.SaveChangesAsync();

            attendance.AttendanceId = 2;
            attendance.Status = "present";
            attendance.TimeTableId = 2;
            attendance.UserId = "2";


            //Act
            var result = await attendanceDAO.UpdateAttendance(listToUpdate);

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.Equal(2, attendance.AttendanceId);
            Assert.Equal("present", attendance.Status);
            Assert.Equal(2, attendance.TimeTableId);
            Assert.Equal("2", attendance.UserId);
        }

        [Fact]
        public async Task TestGetAttendanceByScheduleSuccessfully()
        {
            //Arrange
            var listToGet = new List<AttendanceReports>();
            AttendanceReports attendance = new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "1" };
            AttendanceReports attendance1 = new AttendanceReports() { AttendanceId = 2, Status = "future", TimeTableId = 1, UserId = "2" };

            listToGet.Add(attendance);
            listToGet.Add(attendance1);

            await context.AttendanceReports.AddRangeAsync(listToGet);
            await context.SaveChangesAsync();

            //Act
            var result = attendanceDAO.GetAttendanceBySchedule(1);

            //Assert
            Assert.Equal(listToGet, result);
        }

        [Fact]
        public void TestGetAttendanceByScheduleFail()
        {
            //Arrange
            
            //Act
            var result = attendanceDAO.GetAttendanceBySchedule(1);

            //Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task TestGetAttendanceByScheduleUserIdSuccessfully()
        {
            //Arrange
            AttendanceReports attendance = new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "1" };
            await context.AttendanceReports.AddAsync(attendance);
            await context.SaveChangesAsync();

            //Act
            var result = attendanceDAO.GetAttendanceByScheduleUserId(1,"1");

            //Assert
            Assert.Equal(attendance, result);
        }

        [Fact]
        public void TestGetAttendanceByScheduleUserIdFail()
        {
            //Arrange

            //Act
            var result = attendanceDAO.GetAttendanceByScheduleUserId(1, "1");

            //Assert
            Assert.Null( result);
        }

        [Fact]
        public async Task TestDeleteSuccessfully()
        {
            //Arrange
            var listToDelete = new List<AttendanceReports>();
            AttendanceReports attendance = new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "1" };
            AttendanceReports attendance1 = new AttendanceReports() { AttendanceId = 2, Status = "future", TimeTableId = 1, UserId = "2" };

            listToDelete.Add(attendance);
            listToDelete.Add(attendance1);

            await context.AttendanceReports.AddRangeAsync(listToDelete);
            await context.SaveChangesAsync();

            //Act
            var result = await attendanceDAO.DeleteAttendance(listToDelete);
            var listAfterDelete = await context.AttendanceReports.ToListAsync();

            //Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            Assert.Empty(listAfterDelete);
        }

        [Fact]
        public async Task TestDeleteFail()
        {
            //Arrange
            var listToDelete = new List<AttendanceReports>();
            var listToDB = new List<AttendanceReports>();
            AttendanceReports attendance = new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "1" };
            AttendanceReports attendance1 = new AttendanceReports() { AttendanceId = 2, Status = "future", TimeTableId = 1, UserId = "2" };

            listToDelete.Add(attendance);
            listToDB.Add(attendance1);

            await context.AttendanceReports.AddRangeAsync(listToDB);
            await context.SaveChangesAsync();

            //Act
            var result = await attendanceDAO.DeleteAttendance(listToDelete);
            var listAfterDelete = await context.AttendanceReports.ToListAsync();

            //Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
            Assert.NotEmpty(listAfterDelete);
        }

    }
}
