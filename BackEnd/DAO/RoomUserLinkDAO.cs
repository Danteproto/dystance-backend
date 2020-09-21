using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using BackEnd.Models;
using BackEnd.DBContext;
using Microsoft.AspNetCore.Mvc;
using EmailService;

namespace BackEnd.DAO
{
    public class RoomUserLinkDAO
    {
        public static async Task<IActionResult> Create(RoomDBContext context, RoomUserLink roomUserLink)
        {
            try
            {
                await context.RoomUserLink.AddAsync(roomUserLink);
                context.SaveChanges();
                return new ObjectResult("success")
                {
                    StatusCode = 200,
                };
            }
            catch (Exception e)
            {
                return new ObjectResult(e.Message)
                {
                    StatusCode = 500,
                };
            }
        }

        public static async Task<IActionResult> Create(RoomDBContext context, List<RoomUserLink> roomUserLinks)
        {
            try
            {
                await context.RoomUserLink.AddRangeAsync(roomUserLinks);
                context.SaveChanges();
                return new ObjectResult(new { message = "success" })
                {
                    StatusCode = 200,
                };
            }
            catch (Exception e)
            {
                return new ObjectResult(new { message = e.Message})
                {
                    StatusCode = 500,
                };
            }
        }
    }
}
