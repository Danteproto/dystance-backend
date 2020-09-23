using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace BackEnd.Service
{
    public class RoomService
    {
        public enum ChatType
        {
            Text,
            Image,
            File
        }
        public static string CreateRoom(RoomDBContext context, HttpRequest request, IWebHostEnvironment env)
        {
            try
            {
                var img = request.Form.Files[0];
                var extension = Path.GetExtension(img.FileName);

                var imgName = Convert.ToBase64String(
                        System.Text.Encoding.UTF8.GetBytes(DateTime.Now.ToString())
                    );

                Room room = new Room
                {
                    RoomName = request.Form["name"],
                    CreatorId = request.Form["creatorId"],
                    Description = request.Form["description"],
                    StartDate = Convert.ToDateTime(request.Form["startDate"]),
                    EndDate = Convert.ToDateTime(request.Form["endDate"]),
                    StartHour = TimeSpan.Parse(request.Form["startHour"]),
                    EndHour = TimeSpan.Parse(request.Form["endHour"])
                };

                var result = RoomDAO.Create(context, room);
                room = RoomDAO.GetLastRoom(context);
                room.Image = $"api/rooms/getImage?roomId={room.RoomId}&imgName={imgName + extension}";
                var roomUserLink = new RoomUserLink
                {
                    UserId = request.Form["creatorId"],
                    RoomId = room.RoomId,
                };
                RoomDAO.UpdateRoom(context, room);
                RoomUserLinkDAO.Create(context, roomUserLink);

                var path = Path.Combine(env.ContentRootPath, $"Files/{room.RoomId}/Images");

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

                return result;
            }
            catch (Exception e)
            {
                Console.Error.WriteLine(e.StackTrace);
                return "Error Can't create Room";
            }
        }
        public static Room GetRoomById(RoomDBContext context, int Id)
        {
            return RoomDAO.Get(context, Id);
        }

        public static void CreateRoomUserLink(RoomDBContext context, RoomUserLink roomUserLink)
        {
            RoomUserLinkDAO.Create(context, roomUserLink);
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

        public static List<Room> GetRoomByUserId(RoomDBContext context, string userId)
        {
            return RoomDAO.GetRoomsByUserId(context, userId);
        }

        public static List<RoomChat> GetChatByRoomId(RoomDBContext context, int roomId)
        {
            return RoomChatDAO.GetChatByRoomId(context, roomId);
        }

        public static RoomChat GetLastChat(RoomDBContext context, int roomId)
        {
            return RoomChatDAO.GetLastChat(context, roomId);
        }

    }
}
