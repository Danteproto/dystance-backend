using BackEnd.Context;
using BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DAO
{
    public interface ILogDAO
    {
        public Task<string> CreateLog(UsersLog usersLog);
        public Task<string> DeleteLogs(List<UsersLog> usersLogs);
        public Task<IEnumerable<UsersLog>> GetLogsByRoomId(string roomid);
    }
    public class LogDAO: ILogDAO
    {
        private readonly UserDbContext _context;

        public LogDAO(UserDbContext context)
        {
            _context = context;
        }

        public async Task<string> CreateLog(UsersLog usersLog)
        {
            await _context.UserLog.AddAsync(usersLog);
            var no = await _context.SaveChangesAsync();
            if (no <= 0)
            {
                return "Error at log " + usersLog.ToString();
            }
            else
            {
                return "Success at creating log " + usersLog.ToString();               
            }
        }

        public async Task<string> DeleteLogs(List<UsersLog> usersLogs)
        {
            _context.UserLog.RemoveRange(usersLogs);
            var no = await _context.SaveChangesAsync();
            if (no <= 0)
            {
                return "Error ";
            }
            else
            {
                return "Success";
            }

        }

        public async Task<IEnumerable<UsersLog>> GetLogsByRoomId(string roomid)
        {
            var logLists = await (from logs in _context.UserLog
                                   where logs.RoomId.Contains(roomid)
                                   select logs).ToListAsync();
            return logLists;

        }

    }
}
