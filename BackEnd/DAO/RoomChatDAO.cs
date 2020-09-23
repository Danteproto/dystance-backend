using BackEnd.DBContext;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DAO
{
    public class RoomChatDAO
    {
        public static async Task<IActionResult> Create(RoomDBContext context, RoomChat roomChat)
        {
            try
            {
                context.RoomChat.Add(roomChat);
                context.SaveChanges();
                return new ObjectResult(new { message = "Add success!" })
                {
                    StatusCode = 200,
                };
            }
            catch (Exception e)
            {
                return new ObjectResult(new { message = e.Message })
                {
                    StatusCode = 500,
                };
            }
        }
        public static List<RoomChat> GetChatByRoomId(RoomDBContext context, int roomId)
        {
            return context.RoomChat.Where(x => x.RoomId == roomId).ToList();
        }
        public static RoomChat GetLastChat(RoomDBContext context, int roomId)
        {
            return context.RoomChat.Where(x => x.RoomId == roomId).OrderByDescending(x => x.Id).First();
        }
    }
}
