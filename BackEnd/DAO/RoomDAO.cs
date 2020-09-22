using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BackEnd.Models;
using BackEnd.DBContext;

namespace BackEnd.DAO
{
    public class RoomDAO
    {
        public static string Create(RoomDBContext context, Room room)
        {
            try
            {
                context.Room.Add(room);
                context.SaveChanges();
                return "Create successful";
            }
            catch (Exception e)
            {
                return "Error Can't create Room";
            }
        }
        public static Room GetLastRoom(RoomDBContext context)
        {
            return context.Room.OrderByDescending(x=> x.RoomId).First();
        }
        public static Room Get(RoomDBContext context, int Id)
        {
            return context.Room.Where(x => x.RoomId == Id).FirstOrDefault<Room>();
        }

        public static List<Room> GetRoomsByUserId(RoomDBContext context, string userId)
        {
            var roomIds = context.RoomUserLink.Where(x => x.UserId == userId).Select(x => x.RoomId).ToList();
            return context.Room
                .Where(x => roomIds.Contains(x.RoomId))
                .ToList();
        }
        public static void UpdateRoom(RoomDBContext context, Room room)
        {
            context.Room.Update(room);
            context.SaveChanges();
        }
    }
}
