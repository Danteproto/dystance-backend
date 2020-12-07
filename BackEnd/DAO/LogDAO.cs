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
        public Task<IList<UsersLog>> GetLogsByRoomId(string roomid);
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
            try
            {
                await _context.UserLog.AddAsync(usersLog);
                await _context.SaveChangesAsync();
                return "Success at creating log " + usersLog.ToString();
            }
            catch(Exception ex)
            {
                return "Error at log " + usersLog.ToString();
            }
        }

        public async Task<string> DeleteLogs(List<UsersLog> usersLogs)
        {
            try
            {
                _context.UserLog.RemoveRange(usersLogs);
                 await _context.SaveChangesAsync();
                return "Success";
            }catch(Exception ex)
            {
                return "Error ";
            }
           

        }

        public async Task<IList<UsersLog>> GetLogsByRoomId(string roomid)
        {
            var logLists = await (from logs in _context.UserLog
                                   where logs.RoomId.Contains(roomid)
                                   select logs).ToListAsync();
            return logLists;

        }

    }
}
