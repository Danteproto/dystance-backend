using BackEnd.DAO;
using BackEnd.DBContext;
using BackEnd.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;

namespace BackEnd.Service
{
    

    public class RoomService
    {
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
                    CreatorId = Convert.ToInt32(request.Form["creatorId"]),
                    Description = request.Form["description"],
                    Image = imgName + extension,
                    StartDate = Convert.ToDateTime(request.Form["startDate"]),
                    EndDate = Convert.ToDateTime(request.Form["endDate"]),
                    StartHour = TimeSpan.Parse(request.Form["startHour"]),
                    EndHour = TimeSpan.Parse(request.Form["endHour"])
                };

                var result = RoomDAO.Create(context, room);
                var roomId = RoomDAO.GetLastRoom(context);
                var roomUserLink = new RoomUserLink
                {
                    UserId = Convert.ToInt32(request.Form["creatorId"]),
                    RoomId = roomId,
                };

                RoomUserLinkDAO.Create(context, roomUserLink);

                var path = Path.Combine(env.ContentRootPath, $"Images/{roomId}");

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

        public static void CreateRoomChat(RoomDBContext context, HttpRequest request)
        {
            var roomChat = new RoomChat
            {
                RoomId = Convert.ToInt32(request.Form["roomId"]),
                UserId = Convert.ToInt32(request.Form["userId"]),
                Content = request.Form["content"],
                Date = DateTime.Now
            };
            RoomChatDAO.Create(context, roomChat);
        }

        public static List<Room> GetRoomByUserId(RoomDBContext context,int userId)
        {
            return RoomDAO.GetRoomsByUserId(context, userId);
        }

    }
}
