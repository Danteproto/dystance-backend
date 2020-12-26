using BackEnd.Context;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DAO
{
    public interface IPrivateMessageDAO
    {
        public Task<IActionResult> CreatePrivateMessage(PrivateMessage privateMessage);
        public Task<IActionResult> DeletePrivateMessages(List<PrivateMessage> privateMessages);
    }
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class PrivateMessageDAO : IPrivateMessageDAO
    {
        private readonly UserDbContext _context;

        public PrivateMessageDAO(UserDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CreatePrivateMessage(PrivateMessage privateMessage)
        {
            try
            {

                await _context.PrivateMessages.AddAsync(privateMessage);
                await _context.SaveChangesAsync();
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

        public async Task<IActionResult> DeletePrivateMessages(List<PrivateMessage> privateMessages)
        {
            try
            {
                _context.PrivateMessages.RemoveRange(privateMessages);
                await _context.SaveChangesAsync();
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
