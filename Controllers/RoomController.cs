using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.IO;
using System.Threading.Tasks;
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

        [HttpPost("Create")]
        public IActionResult CreateRoom()
        {
            var status = RoomService.CreateRoom(_context, Request, _env);
            JObject json = new JObject();
            JArray array = new JArray();
            array.Add(status);
            json["status"] = array;
            return Content(json.ToString());
        }

        [HttpPost("Get")]
        public IActionResult GetRoomById()
        {
            Room room = RoomService.GetRoomById(_context, Request);
            return Content(JsonConvert.SerializeObject(room));
        }

        [HttpPost("Get/ByUserId")]
        public IActionResult GetRoomByUserId()
        {
            var rooms = RoomService.GetRoomByUserId(_context, Request);
            return Content(JsonConvert.SerializeObject(rooms));
        }
        [HttpPost("Get/Image")]
        public async Task<IActionResult> GetImage()
        {
            var imgName = Request.Form["imgName"];
            var rootPath = _env.ContentRootPath;
            var path = Path.Combine(rootPath, "Images");
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
