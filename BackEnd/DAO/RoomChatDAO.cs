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
            context.RoomChat.Add(roomChat);
            context.SaveChanges();
        }

        public List<RoomChat> GetChatByRoomId(RoomDBContext context, int roomId, int scale)
        {
            return context.RoomChat.Where(x => x.RoomId == roomId).Take(50 *scale).ToList();
        }
    }
}
