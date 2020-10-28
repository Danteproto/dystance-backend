using BackEnd.Context;
using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using EmailService;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Primitives;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Ultilities;
using Newtonsoft.Json.Schema;
using BackEnd.Responses;

namespace BackEnd.Services
{
    public class RoomService
    {
        public enum ChatType
        {
            Text,
            Image,
            File
        }

        public async static Task<IActionResult> CreateRoom(RoomDBContext context, HttpRequest request, IWebHostEnvironment _env)
        {

            Room room = new Room
            {
                RoomName = request.Form["name"],
                CreatorId = request.Form["creatorId"],
                Description = request.Form["description"],
                StartDate = Convert.ToDateTime(request.Form["startDate"]),
                EndDate = Convert.ToDateTime(request.Form["endDate"]),
                RepeatOccurrence = request.Form["repeatOccurrence"],
            };


            var result = await RoomDAO.Create(context, room);
            var lastRoom = RoomDAO.GetLastRoom(context);

            var timetables = JsonConvert.DeserializeObject<List<Timetable>>(request.Form["roomTimes"]);
            timetables.ForEach(timetable => timetable.RoomId = lastRoom.RoomId);
            result = await TimetableDAO.Create(context, timetables);

            var roomUserLink = new RoomUserLink
            {
                UserId = request.Form["CreatorId"],
                RoomId = lastRoom.RoomId,
            };

            string imgPath;
            string imgName = "";
            string extension = "";
            IFormFile img = null;

            imgName = "default";
            extension = ".png";
            img = Ultilities.Extensions.GetRoomDefaultAvatar(_env);


            lastRoom.Image = $"api/rooms/getImage?roomId={lastRoom.RoomId}&imgName={imgName + extension}";

            RoomDAO.UpdateRoom(context, lastRoom);
            await RoomUserLinkDAO.Create(context, roomUserLink);

            return result;
        }
        public async static Task<IActionResult> DeleteRoom(RoomDBContext context, int roomId, IWebHostEnvironment env)
        {
            var room = RoomDAO.Get(context, roomId);
            var roomUserLinks = RoomUserLinkDAO.GetRoomLink(context, roomId);
            var roomChats = RoomChatDAO.GetChatByRoomId(context, roomId);
            var roomDeadlines = context.Deadline.Where(dl => dl.RoomId == roomId).ToList();
            var roomTimetables = TimetableDAO.GetByRoomId(context, roomId);

            var result = await RoomUserLinkDAO.Delete(context, roomUserLinks);
            result = await RoomChatDAO.DeleteRoomChat(context, roomChats);
            result = await TimetableDAO.DeleteTimeTable(context, roomTimetables);
            context.Deadline.RemoveRange(roomDeadlines);
            context.SaveChanges();

            var path = Path.Combine(env.ContentRootPath, $"Files/{roomId}");
            if (Directory.Exists(path))
            {
                Directory.Delete(path, true);
            }

            result = await RoomDAO.Delete(context, room);
            return result;
        }
        public static Room GetRoomById(RoomDBContext context, int Id)
        {
            return RoomDAO.Get(context, Id);
        }
        public static async Task<IActionResult> CreateRoomChat(RoomDBContext context, HttpRequest request, IWebHostEnvironment env)
        {
            var type = (ChatType)Convert.ToInt32(request.Form["chatType"]);
            var roomId = Convert.ToInt32(request.Form["roomId"]);
            switch (type)
            {
                case ChatType.Text:
                    {
                        var roomChat = new RoomChat
                        {
                            RoomId = roomId,
                            UserId = request.Form["userId"],
                            Content = request.Form["content"],
                            Date = DateTime.Now,
                            Type = (int)ChatType.Text
                        };
                        return await RoomChatDAO.Create(context, roomChat);
                    }
                case ChatType.Image:
                    {
                        var img = request.Form.Files[0];
                        var extension = Path.GetExtension(img.FileName);

                        var imgName = Convert.ToBase64String(
                                System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                            );
                        var path = Path.Combine(env.ContentRootPath, $"Files/{roomId}/Chat/Images");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }

                        var imgPath = Path.Combine(path, imgName + extension);
                        if (img.Length > 0)
                        {
                            using var fileStream = new FileStream(imgPath, FileMode.Create);
                            img.CopyTo(fileStream);
                        }
                        var roomChat = new RoomChat
                        {
                            RoomId = roomId,
                            UserId = request.Form["userId"],
                            Content = $"api/rooms/chat/getFile?fileName={imgName + extension}&roomId={roomId}&type={(int)ChatType.Image}&realName={Path.GetFileName(img.FileName)}",
                            Date = DateTime.Now,
                            Type = (int)ChatType.Image,
                            FileName = Path.GetFileName(img.FileName)
                        };
                        return await RoomChatDAO.Create(context, roomChat);
                    }
                case ChatType.File:
                    {
                        var file = request.Form.Files[0];
                        var extension = Path.GetExtension(file.FileName);
                        var fileName = Convert.ToBase64String(
                                System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                            );
                        var path = Path.Combine(env.ContentRootPath, $"Files/{roomId}/Chat/Files");
                        if (!Directory.Exists(path))
                        {
                            Directory.CreateDirectory(path);
                        }
                        var filePath = Path.Combine(path, fileName + extension);
                        if (file.Length > 0)
                        {
                            using var fileStream = new FileStream(filePath, FileMode.Create);
                            file.CopyTo(fileStream);
                        }
                        var roomChat = new RoomChat
                        {
                            RoomId = roomId,
                            UserId = request.Form["userId"],
                            Content = $"api/rooms/chat/getFile?fileName={fileName + extension}&roomId={roomId}&type={(int)ChatType.File}&realName={Path.GetFileName(file.FileName)}",
                            Date = DateTime.Now,
                            Type = (int)ChatType.File,
                            FileName = Path.GetFileName(file.FileName)
                        };
                        return await RoomChatDAO.Create(context, roomChat);
                    }
                default:
                    return new ObjectResult(new { message = "wrong chat type" })
                    {
                        StatusCode = 500,
                    };
            }

        }

        public static List<RoomResponse> GetRoomByUserId(RoomDBContext context, string userId)
        {
            var rooms = RoomDAO.GetRoomsByUserId(context, userId);
            var response = new List<RoomResponse>();
            foreach(var room in rooms)
            {
                response.Add(new RoomResponse {
                    RoomId = room.RoomId,
                    RoomName = room.RoomName,
                    CreatorId = room.CreatorId,
                    Image = room.Image,
                    Description = room.Description,
                    StartDate = room.StartDate,
                    EndDate = room.EndDate,
                    RepeatOccurrence = room.RepeatOccurrence,
                    RoomTimes = JsonConvert.SerializeObject(TimetableDAO.GetByRoomId(context, room.RoomId)
                                .Select(item => new {item.DayOfWeek ,item.StartTime, item.EndTime }))
                });
            }
            return response;
        }
        public static List<RoomChat> GetChatByRoomId(RoomDBContext context, int roomId)
        {
            return RoomChatDAO.GetChatByRoomId(context, roomId);
        }

        public static RoomChat GetLastChat(RoomDBContext context, int roomId)
        {
            return RoomChatDAO.GetLastChat(context, roomId);
        }

        public static async Task<IActionResult> Invite(
            RoomDBContext roomContext, UserDbContext userContext,
            HttpRequest request, IEmailSender emailSender)
        {
            var roomId = Convert.ToInt32(request.Form["roomId"]);
            var room = RoomDAO.Get(roomContext, roomId);
            var message = request.Form["message"].ToString();
            string emailLists = request.Form["emailList"];
            var list = emailLists.Split(",").ToList();
            var userIds = userContext.Users.Where(user => list.Any(x => user.Email == x)).Select(user => user.Id).ToList();
            var roomUserLinks = userIds.Select(userId => new RoomUserLink
            {
                RoomId = roomId,
                UserId = userId
            }).ToList();
            var existLink = roomContext.RoomUserLink.Where(link => link.RoomId == roomId).ToList();
            if (existLink.Count != 0)
            {
                roomUserLinks = roomUserLinks.Where(link => !existLink.Any(x => x.UserId == link.UserId)).ToList();
            }
            var result = await RoomUserLinkDAO.Create(roomContext, roomUserLinks);
            roomUserLinks.ForEach(async link =>
            {
                var email = userContext.Users.Where(user => user.Id == link.UserId).Select(user => user.Email).FirstOrDefault().ToString();

                var mailMessage = new Message(new string[] { email }, "Invite To Class", message == "" ? $"You have been invite to class {room.RoomName}" : message, null);

                await emailSender.SendEmailAsync(mailMessage);
            });
            return result;
        }

        public static IActionResult WhiteboardUploadImage(HttpRequest request, IWebHostEnvironment env)
        {
            Console.WriteLine(request.Form);
            var roomId = Convert.ToInt32(request.Form["whiteboardId"]);
            string rawdata = request.Form["imagedata"];
            var imagedata = Convert.FromBase64String(rawdata.Split(",")[1]);
            var path = Path.Combine(env.ContentRootPath, $"Files/{roomId}/Whiteboard");
            var imgName = Convert.ToBase64String(
                System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())) + ".png";
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
            var imgPath = Path.Combine(path, imgName);
            File.WriteAllBytes(imgPath, imagedata);
            return new OkObjectResult(JsonConvert.SerializeObject(new
            {
                imageUrl = $"api/rooms/whiteboard/img?id={roomId}&imgName={imgName}"
            }));
        }

        public async static Task<IActionResult> UpdateRoom(RoomDBContext context, HttpRequest request, IWebHostEnvironment _env)
        {
            var room = RoomDAO.Get(context, Convert.ToInt32(request.Form["roomId"]));
            if (room == null)
            {
                return new BadRequestObjectResult(new { type = 0, message = "Not found room" });
            }
            string imgPath;
            string imgName = "";
            string extension = "";
            IFormFile img = null;
            //if avatar is empty, use default

            if (request.Form.Files.Any())
            {
                img = request.Form.Files[0];
                extension = Path.GetExtension(img.FileName);

                imgName = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                    );
                var path = Path.Combine(_env.ContentRootPath, $"Files/{room.RoomId}/Images");
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }

                imgPath = Path.Combine(path, imgName + extension);
                if (img.Length > 0)
                {
                    using var fileStream = new FileStream(imgPath, FileMode.Create);
                    img.CopyTo(fileStream);
                }
                room.Image = $"api/rooms/getImage?roomId={room.RoomId}&imgName={imgName + extension}";
            }

            room.RoomName = request.Form["name"].Any() ? request.Form["name"].ToString() : room.RoomName;
            room.CreatorId = request.Form["creatorId"].Any() ? request.Form["creatorId"].ToString() : room.CreatorId;
            room.Description = request.Form["description"].Any() ? request.Form["description"].ToString() : room.Description;
            room.StartDate = request.Form["startDate"].Any() ? Convert.ToDateTime(request.Form["startDate"]) : room.StartDate;
            room.EndDate = request.Form["endDate"].Any() ? Convert.ToDateTime(request.Form["endDate"]) : room.EndDate;
            room.RepeatOccurrence = request.Form["repeatOccurrence"].Any() ? request.Form["repeatOccurrence"].ToString() : room.RepeatOccurrence;
            var result = RoomDAO.UpdateRoom(context, room);

            if (request.Form["roomTimes"].Any())
            {
                var timetables = TimetableDAO.GetByRoomId(context, room.RoomId);
                await TimetableDAO.DeleteTimeTable(context, timetables);
                timetables = JsonConvert.DeserializeObject<List<Timetable>>(request.Form["roomTimes"]);
                timetables.ForEach(timetable => timetable.RoomId = room.RoomId);
                result = await TimetableDAO.Create(context, timetables);
            }

            return result;
        }

    }
}
