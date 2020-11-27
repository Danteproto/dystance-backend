using BackEnd.DBContext;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DAO
{
    public class TimetableDAO
    {
        public static async Task<IActionResult> Create(RoomDBContext context, List<Timetable> timetables)
        {
            try
            {
                await context.TimeTable.AddRangeAsync(timetables);
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
        public static async Task<IActionResult> Create(RoomDBContext context, Timetable timetable)
        {
            try
            {
                await context.TimeTable.AddAsync(timetable);
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
        public static List<Timetable> GetByRoomId(RoomDBContext context, int roomId)
        {
            return context.TimeTable.Where(timetable => timetable.RoomId == roomId).ToList();
        }
        public static Timetable GetById(RoomDBContext context, int id)
        {
            return context.TimeTable.Where(timetable => timetable.Id == id).FirstOrDefault();
        }
        public static Timetable GetLast(RoomDBContext context)
        {
            return context.TimeTable.OrderByDescending(x => x.Id).FirstOrDefault();
        }
        public static async Task<IActionResult> Update(RoomDBContext context, List<Timetable> timetables)
        {
            try
            {
                context.TimeTable.UpdateRange(timetables);
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
        public static async Task<IActionResult> Update(RoomDBContext context, Timetable timetable)
        {
            try
            {
                context.TimeTable.Update(timetable);
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
        public static async Task<IActionResult> DeleteTimeTable(RoomDBContext context, List<Timetable> timetables)
        {
            try
            {
                context.TimeTable.RemoveRange(timetables);
                context.SaveChanges();
                return new ObjectResult(new { message = "delete success!" })
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

    }
}
