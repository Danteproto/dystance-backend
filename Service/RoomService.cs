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
                    RoomName = request.Form["Name"],
                    CreatorId = Convert.ToInt32(request.Form["CreatorId"]),
                    Description = request.Form["Description"],
                    Image = imgName + extension,
                    StartDate = Convert.ToDateTime(request.Form["StartDate"]),
                    EndDate = Convert.ToDateTime(request.Form["EndDate"]),
                    StartHour = TimeSpan.Parse(request.Form["StartHour"]),
                    EndHour = TimeSpan.Parse(request.Form["EndHour"])
                };

                var result = RoomDAO.Create(context, room);
                var roomId = RoomDAO.GetLastRoom(context);
                var roomUserLink = new RoomUserLink
                {
                    UserId = Convert.ToInt32(request.Form["CreatorId"]),
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
                RoomId = Convert.ToInt32(request.Form["RoomId"]),
                UserId = Convert.ToInt32(request.Form["UserId"]),
                Content = request.Form["Content"],
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
