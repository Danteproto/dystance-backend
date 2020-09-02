using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Web;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly RoomDBContext _context;
        private readonly IWebHostEnvironment _env;

        public RoomController(RoomDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }
        [HttpGet]
        public string get()
        {
            return _env.WebRootPath;
        }
        [HttpPost("Create")]
        public string CreateRoom()
        {
            RoomService.CreateRoom(_context, Request, _env);
            return "create succesful";
        }
        [HttpPost("Get")]
        public IActionResult GetRoomById(int Id)
        {
            Room room = RoomService.GetRoomById(_context, Id);
            return Content(JsonConvert.SerializeObject(room));
        }

        [HttpPost("Get/ByUserId")]
        public IActionResult GetRoomByUserId(int Id)
        {
            var rooms = RoomService.GetRoomByUserId(_context, Id);
            return Content(JsonConvert.SerializeObject(rooms));
        }

    }
}
