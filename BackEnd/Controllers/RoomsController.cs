using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Web;

namespace BackEnd.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RoomsController : ControllerBase
    {
        private readonly DefaultContractResolver _contractResolver;
        private readonly RoomDBContext _context;
        private readonly IWebHostEnvironment _env;

        public RoomsController(RoomDBContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
            _contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
        }

        [HttpPost("create")]
        public IActionResult CreateRoom()
        {
            var status = RoomService.CreateRoom(_context, Request, _env);
            var obj = JObject.FromObject(new
            {
                status,
            });
            return Content(obj.ToString());
        }

        [HttpGet("getById")]
        public IActionResult GetRoomById(int Id)
        {
            Room room = RoomService.GetRoomById(_context, Id);
            return Content(JsonConvert.SerializeObject(room, new JsonSerializerSettings
            {
                ContractResolver = _contractResolver,
                Formatting = Formatting.Indented
            }));
        }

        [HttpGet("getByUserId")]
        public IActionResult GetRoomByUserId(int Id)
        {
            var rooms = RoomService.GetRoomByUserId(_context, Id);
            return Content(JsonConvert.SerializeObject(rooms, new JsonSerializerSettings
            {
                ContractResolver = _contractResolver,
                Formatting = Formatting.Indented
            }));
        }
        [HttpGet("Get/Image/Room")]
        public async Task<IActionResult> GetImage(int roomId, string imgName)
        {
            var rootPath = _env.ContentRootPath;
            var path = Path.Combine(rootPath, $"Images/{roomId}");
            var imgPath = Path.Combine(path, imgName);
            var image = System.IO.File.OpenRead(imgPath);
            return File(image, "image/jpeg");
        }
        [HttpPost("Chat")]
        public IActionResult CreateChat()
        {
            RoomService.CreateRoomChat(_context, Request);
            return Content("Chat add successful");
        }
    }
}
