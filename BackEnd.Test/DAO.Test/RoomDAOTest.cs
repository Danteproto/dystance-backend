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
        private RoomDBContext context;
        public RoomDAOTest()
        {
            factory = new ConnectionFactory();
            context = factory.CreateRoomDbContextForInMemory();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SaveChanges();
        }

        //Test create
        [Fact]
        public async Task RoomCreateSuccessfully()
        {
            //Arrange
            var room = new Room()
            {
                ClassName="testName"
            };



            // Act
            var result = await RoomDAO.Create(context, room);

            // Assert
            Assert.Equal((int)HttpStatusCode.NotFound, ((ObjectResult)result).StatusCode);
        }

    }
}
