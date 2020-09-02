using BackEnd.DBContext;
using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DAO
{
    public class RoomChatDAO
    {
        public static void Create(RoomDBContext context, RoomChat roomChat)
        {
            roomChat.Date = DateTime.Now;
            context.RoomChat.Add(roomChat);
            context.SaveChanges();
        }
        public RoomChat Get(RoomDBContext context, int id)
        {
            return context.RoomChat.Where(x => x.ChatId == id).FirstOrDefault<RoomChat>();

        }

        public List<RoomChat> GetChatByRoomId(RoomDBContext context, int roomId)
        {
            return context.RoomChat.Where(x => x.RoomId == roomId).ToList();
        }
    }
}
