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
    public class SemesterDAOTest
    {
        private ConnectionFactory factory;
        private RoomDBContext roomContext;
        public SemesterDAOTest()
        {
            factory = new ConnectionFactory();
            roomContext = factory.CreateRoomDbContextForInMemory();
            roomContext.Database.EnsureDeleted();
            roomContext.Database.EnsureCreated();
            roomContext.SaveChanges();
        }

        [Fact]
        public async void SemesterCreateSuccessfully()
        {
            //Arrange
            var semester = new Semester()
            {
                Id = 1,
                Name = "testSemester",
                File = "TestFile.xsl",
                LastUpdated = DateTime.Now
            };
            // Act
            var result = await SemesterDAO.Create(roomContext, semester);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void SemesterCreateFail()
        {
            //Arrange
            var semester = new Semester()
            {
                Id = 1,
                Name = "testSemester",
                File = "TestFile.xsl",
                LastUpdated = DateTime.Now
            };
            // Act
            var result = await SemesterDAO.Create(roomContext, semester);
            // Assert
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);
            result = await SemesterDAO.Create(roomContext, semester);
            // Assert
            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)result).StatusCode);
        }
        [Fact]
        public async void GetAll()
        {
            //Arrange
            var semesters = new List<Semester>();
            semesters.Add(new Semester() { Id = 1, Name = "testSemester", File = "TestFile.xsl", LastUpdated = DateTime.Now });
            semesters.Add(new Semester() { Id = 2, Name = "testSemester2", File = "TestFile.xsl", LastUpdated = DateTime.Now });
            semesters.Add(new Semester() { Id = 3, Name = "testSemester3", File = "TestFile.xsl", LastUpdated = DateTime.Now });


            // Act
            foreach (var semester in semesters)
            {
                var result = await SemesterDAO.Create(roomContext, semester);
                Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            }
            var resultSemester = SemesterDAO.GetAll(roomContext);
            Assert.Equal(3, resultSemester.Count);
        }
        [Fact]
        public async void GetLast()
        {
            var now = DateTime.Now;
            //Arrange
            var semesters = new List<Semester>();
            semesters.Add(new Semester() { Id = 1, Name = "testSemester", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 2, Name = "testSemester2", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 3, Name = "testSemester3", File = "TestFile.xsl", LastUpdated = now });


            // Act
            foreach (var semester in semesters)
            {
                var result = await SemesterDAO.Create(roomContext, semester);
                Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            }
            var resultSemester = SemesterDAO.GetLast(roomContext);
            Assert.Equal(3, resultSemester.Id);
            Assert.Equal("testSemester3", resultSemester.Name);
            Assert.Equal("TestFile.xsl", resultSemester.File);
            Assert.Equal(now, resultSemester.LastUpdated);
        }
        [Fact]
        public async void GetById()
        {
            var now = DateTime.Now;
            //Arrange
            var semesters = new List<Semester>();
            semesters.Add(new Semester() { Id = 1, Name = "testSemester", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 2, Name = "testSemester2", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 3, Name = "testSemester3", File = "TestFile.xsl", LastUpdated = now });


            // Act
            foreach (var semester in semesters)
            {
                var result = await SemesterDAO.Create(roomContext, semester);
                Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            }
            var resultSemester = SemesterDAO.GetById(roomContext,3);
            Assert.Equal(3, resultSemester.Id);
            Assert.Equal("testSemester3", resultSemester.Name);
            Assert.Equal("TestFile.xsl", resultSemester.File);
            Assert.Equal(now, resultSemester.LastUpdated);

            resultSemester = SemesterDAO.GetById(roomContext, 2);
            Assert.Equal(2, resultSemester.Id);
            Assert.Equal("testSemester2", resultSemester.Name);
            Assert.Equal("TestFile.xsl", resultSemester.File);
            Assert.Equal(now, resultSemester.LastUpdated);

            resultSemester = SemesterDAO.GetById(roomContext, 1);
            Assert.Equal(1, resultSemester.Id);
            Assert.Equal("testSemester", resultSemester.Name);
            Assert.Equal("TestFile.xsl", resultSemester.File);
            Assert.Equal(now, resultSemester.LastUpdated);
        }
        [Fact]
        public async void UpdateSuccessfully()
        {
            var now = DateTime.Now;
            //Arrange
            var semesters = new List<Semester>();
            semesters.Add(new Semester() { Id = 1, Name = "testSemester", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 2, Name = "testSemester2", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 3, Name = "testSemester3", File = "TestFile.xsl", LastUpdated = now });


            // Act
            foreach (var semester in semesters)
            {
                var result = await SemesterDAO.Create(roomContext, semester);
                Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            }
            semesters[2].LastUpdated = DateTime.Now.AddMinutes(10);
            semesters[2].Name = "TestSemesterUpdate";
            semesters[2].File = "TestSemesterUpdateFile.xsl";

            var updateResult = await SemesterDAO.Update(roomContext, semesters[2]);
            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)updateResult).StatusCode);

            var resultSemester = SemesterDAO.GetById(roomContext, 3);
            Assert.Equal(3, resultSemester.Id);
            Assert.Equal("TestSemesterUpdate", resultSemester.Name);
            Assert.Equal("TestSemesterUpdateFile.xsl", resultSemester.File);
            Assert.NotEqual(now, resultSemester.LastUpdated);

        }
        [Fact]
        public async void SemetserDeleteSuccessfully()
        {
            var now = DateTime.Now;
            //Arrange
            var semesters = new List<Semester>();
            semesters.Add(new Semester() { Id = 1, Name = "testSemester", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 2, Name = "testSemester2", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 3, Name = "testSemester3", File = "TestFile.xsl", LastUpdated = now });


            // Act
            foreach (var semester in semesters)
            {
                var result = await SemesterDAO.Create(roomContext, semester);
                Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            }
            var delResult = await SemesterDAO.Delete(roomContext, semesters[2]);

            Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)delResult).StatusCode);

            var resultSemester = SemesterDAO.GetById(roomContext, 3);
            Assert.Null(resultSemester);
        }
        [Fact]
        public async void SemetserDeleteFail()
        {
            var now = DateTime.Now;
            //Arrange
            var semesters = new List<Semester>();
            semesters.Add(new Semester() { Id = 1, Name = "testSemester", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 2, Name = "testSemester2", File = "TestFile.xsl", LastUpdated = now });
            semesters.Add(new Semester() { Id = 3, Name = "testSemester3", File = "TestFile.xsl", LastUpdated = now });


            // Act
            foreach (var semester in semesters)
            {
                var result = await SemesterDAO.Create(roomContext, semester);
                Assert.Equal((int)HttpStatusCode.OK, ((ObjectResult)result).StatusCode);

            }
            var delResult = await SemesterDAO.Delete(roomContext, new Semester { Id = 4, Name = "testSemester3", File = "TestFile.xsl", LastUpdated = now });

            Assert.Equal((int)HttpStatusCode.InternalServerError, ((ObjectResult)delResult).StatusCode);

        }
    }
}
