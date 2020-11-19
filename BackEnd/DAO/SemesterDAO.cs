using BackEnd.DBContext;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DAO
{
    public class SemesterDAO
    {
        public static async Task<IActionResult> Create(RoomDBContext context, Semester semester)
        {
            try
            {
                context.Semester.Add(semester);
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
        public static List<Semester> GetAll(RoomDBContext context)
        {
            return context.Semester.ToList();
        }
        public static Semester GetLast(RoomDBContext context)
        {
            return context.Semester.OrderByDescending(x => x.Id).FirstOrDefault();
        }
        public static Semester GetById(RoomDBContext context, int id)
        {
            return context.Semester.Where(x => x.Id == id).FirstOrDefault();
        }
        public static async Task<IActionResult> Update (RoomDBContext context, Semester semester)
        {
            try
            {
                context.Semester.Update(semester);
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
        public static async Task<IActionResult> Delete(RoomDBContext context, Semester semester)
        {
            try
            {
                context.Semester.Remove(semester);
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
    }
}
