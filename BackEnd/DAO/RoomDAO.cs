using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BackEnd.Models;
using BackEnd.DBContext;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.DAO
{
    public class RoomDAO
    {
        public static async Task<IActionResult> Create(RoomDBContext context, Room room)
        {
            try
            {
                context.Room.Add(room);
                context.SaveChanges();
                return new ObjectResult(new { message = "success" })
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
        public static async Task<IActionResult> Delete(RoomDBContext context, Room room)
        {
            try
            {
                context.Room.Remove(room);
                context.SaveChanges();
                return new ObjectResult(new { message = "Delete room finish" })
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
        public static Room GetLastRoom(RoomDBContext context)
        {
            return context.Room.OrderByDescending(x => x.RoomId).First();
        }
        public static Room Get(RoomDBContext context, int Id)
        {
            return context.Room.Where(x => x.RoomId == Id).FirstOrDefault<Room>();
        }

        public static List<Room> GetRoomsByUserId(RoomDBContext context, string userId)
        {
            var roomIds = context.RoomUserLink.Where(x => x.UserId == userId).Select(x => x.RoomId).ToList();
            return context.Room
                .Where(x => roomIds.Contains(x.RoomId) && !x.Group)
                .ToList();
        }
        public static IActionResult UpdateRoom(RoomDBContext context, Room room)
        {
            try
            {
                context.Room.Update(room);
                context.SaveChanges();
                return new ObjectResult(new { message = "Successful" })
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

        public static List<Room> GetGroupByRoom(RoomDBContext context, int Id)
        {
            return context.Room.Where(room => room.MainRoomId == Id && room.Group).ToList();
        }
        public static Room GetRoomByClassAndSubject(RoomDBContext context, string className, string subject)
        {
            return context.Room.Where(room => room.Subject == subject && room.ClassName == className).FirstOrDefault();
        }
        public static List<Room> GetRoomBySemester(RoomDBContext context, int semesterId)
        {
            return context.Room.Where(room => room.SemesterId == semesterId).ToList();
        }
        public static Room GetRoomByClassSubjectSemester(RoomDBContext context, string className, string subject, int semesterId)
        {
            return context.Room.Where(room => room.Subject == subject && room.ClassName == className && room.SemesterId == semesterId).FirstOrDefault();
        }
    }
}

