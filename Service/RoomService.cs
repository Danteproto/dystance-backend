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
        public static void CreateRoom(RoomDBContext context, HttpRequest request, IWebHostEnvironment env)
        {
            var rootPath = env.ContentRootPath;
            var path = Path.Combine(rootPath, "Images");
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path); 
            }
            var img = request.Form.Files[0];
            var extension = Path.GetExtension(img.FileName);
            var imgName = DateTime.Now.ToString("MM_dd_yyyy_hh_mm_ss_tt");
            var imgPath = Path.Combine(path, imgName + extension);
            if (img.Length > 0)
            {
                using var fileStream = new FileStream(imgPath, FileMode.Create);
                img.CopyTo(fileStream);
            }
            Room room = new Room
            {
                RoomName = request.Form["Name"],
                CreatorId = Convert.ToInt32(request.Form["CreatorId"]),
                Description = request.Form["Description"],
                Image = imgName,
                StartDate = Convert.ToDateTime(request.Form["StartDate"]),
                EndDate = Convert.ToDateTime(request.Form["EndDate"]),
                StartHour = TimeSpan.Parse(request.Form["StartHour"]),
                EndHour = TimeSpan.Parse(request.Form["EndHour"])
            };
            RoomDAO.Create(context, room);
        }
        public static Room GetRoomById(RoomDBContext context, int Id)
        {
            return RoomDAO.Get(context, Id);
        }

        public static void CreateRoomUserLink(RoomDBContext context, RoomUserLink roomUserLink)
        {
            RoomUserLinkDAO.Create(context, roomUserLink);
        }

        public static void CreateRoomChat(RoomDBContext context, RoomChat roomChat)
        {
            RoomChatDAO.Create(context, roomChat);
        }

        public static List<Room> GetRoomByUserId(RoomDBContext context, int userId)
        {
            return RoomDAO.GetRoomsByUserId(context, userId);
        }

    }
}
