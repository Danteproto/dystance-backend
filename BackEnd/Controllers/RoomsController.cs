using BackEnd.Context;
using BackEnd.DBContext;
using BackEnd.Models;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
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
        private readonly RoomDBContext _roomContext;
        private readonly UserDbContext _userContext;
        private readonly IWebHostEnvironment _env;

        public RoomsController(RoomDBContext roomContext, UserDbContext userContext, IWebHostEnvironment env)
        {
            _roomContext = roomContext;
            _userContext = userContext;
            _env = env;
            _contractResolver = new DefaultContractResolver
            {
                NamingStrategy = new CamelCaseNamingStrategy()
            };
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateRoom()
        {
            return await RoomService.CreateRoom(_roomContext, Request);
        }
        [HttpGet("delete")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            return await RoomService.DeleteRoom(_roomContext, id, _env);
        }

        [HttpGet("getById")]
        public IActionResult GetRoomById(int Id)
        {
            Room room = RoomService.GetRoomById(_roomContext, Id);
            return Content(JsonConvert.SerializeObject(room, new JsonSerializerSettings
            {
                ContractResolver = _contractResolver,
                Formatting = Formatting.Indented
            }));
        }

        [HttpGet("getByUserId")]
        public IActionResult GetRoomByUserId(string Id)
        {
            var rooms = RoomService.GetRoomByUserId(_roomContext, Id);
            return Content(JsonConvert.SerializeObject(rooms, new JsonSerializerSettings
            {
                ContractResolver = _contractResolver,
                Formatting = Formatting.Indented
            }));
        }
        [HttpGet("getImage")]
        public async Task<IActionResult> GetImage(int roomId, string imgName)
        {
            var rootPath = _env.ContentRootPath;
            var path = Path.Combine(rootPath, $"Files/{roomId}/Images");
            var imgPath = Path.Combine(path, imgName);
            var image = System.IO.File.OpenRead(imgPath);
            return File(image, "image/jpeg");
        }
        [HttpPost("chat/add")]
        public async Task<IActionResult> CreateChat()
        {
            return await RoomService.CreateRoomChat(_roomContext, Request, _env);
        }
        [HttpGet("chat/get")]
        public IActionResult GetChat(int id)
        {
            var roomChats = RoomService.GetChatByRoomId(_roomContext, id);
            return Content(JsonConvert.SerializeObject(roomChats, new JsonSerializerSettings
            {
                ContractResolver = _contractResolver,
                Formatting = Formatting.Indented
            }));
        }
        [HttpGet("chat/getLast")]
        public IActionResult GetLastChat(int id)
        {
            var roomChat = RoomService.GetLastChat(_roomContext, id);
            return Content(JsonConvert.SerializeObject(roomChat, new JsonSerializerSettings
            {
                ContractResolver = _contractResolver,
                Formatting = Formatting.Indented
            }));
        }
        [HttpGet("chat/getFile")]
        public async Task<IActionResult> GetFile(string fileName, int type, int roomId,string realName)
        {

            var rootPath = _env.ContentRootPath;
            string path = "";
            if (type == (int)RoomService.ChatType.Image)
            {
                path = Path.Combine(rootPath, $"Files/{roomId}/Chat/Images");
            }
            else if (type == (int)RoomService.ChatType.File)
            {
                path = Path.Combine(rootPath, $"Files/{roomId}/Chat/Files");
            }
            var filePath = Path.Combine(path, fileName);
            var file = System.IO.File.OpenRead(filePath);
            string contentType;
            new FileExtensionContentTypeProvider().TryGetContentType(fileName, out contentType);
            Response.Headers.Add("Content-Disposition", $"attachment; filename={realName}");
            return File(file, contentType);
        }

        [HttpPost("invite")]
        public async Task<IActionResult> InviteToRoom()
        {
            return await RoomService.Invite(_roomContext, _userContext, Request);
        }
    }
}
