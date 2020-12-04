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
        public async Task TestUpdateAttendanceSuccessfully()
        {
            //Arrange
            var listToUpdate = new List<AttendanceReports>();
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 2, Status = "future", TimeTableId = 1, UserId = "2" });
            listToUpdate.Add(new AttendanceReports() { AttendanceId = 3, Status = "future", TimeTableId = 1, UserId = "2" });
            await context.AddRangeAsync(listToUpdate);

            var listUpdate = new List<AttendanceReports>();
            listUpdate.Add(new AttendanceReports() { AttendanceId = 1, Status = "attended", TimeTableId = 1, UserId = "2" });
            listUpdate.Add(new AttendanceReports() { AttendanceId = 2, Status = "attended", TimeTableId = 1, UserId = "2" });
            listUpdate.Add(new AttendanceReports() { AttendanceId = 3, Status = "attended", TimeTableId = 1, UserId = "2" });

            //Act
            var result = await attendanceDAO.UpdateAttendance(listUpdate);

            //Assert
            Assert.Equal((int)HttpStatusCode.OK,((ObjectResult)result).StatusCode);
        }
    }
}
