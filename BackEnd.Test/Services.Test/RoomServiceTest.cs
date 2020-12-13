using BackEnd.DBContext;
using BackEnd.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Moq;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace BackEnd.Test.Services.Test
{
    public class RoomServiceTest
    {
        private ConnectionFactory factory;
        private RoomDBContext _context;
        public RoomServiceTest()
        {
            //setting up context
            factory = new ConnectionFactory();
            _context = factory.CreateRoomDbContextForInMemory();
            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
            _context.SaveChanges();
        }

        [Fact]
        public async void CreateRoom_Returns_Success()
        {
            //Arrange

            //---------------------------MOCKING REQUEST------------------------//
            var formFile = new Mock<IFormFile>();
            var files = new FormFileCollection();
            files.Add(formFile.Object);
            var formCol = new FormCollection(new Dictionary<string, Microsoft.Extensions.Primitives.StringValues>
            {
                { "chatType", "0" },
                {"", "" }
            }, files);
            var request = new Mock<HttpRequest>();
            request.Setup(req => req.Form).Returns(formCol);
            //------------------------------------------------------------------//


            var _env = new Mock<IWebHostEnvironment>();







            //Act
            var result = await RoomService.CreateRoomChat(_context, request.Object, _env.Object);




            //Assert
        }



    }
}
