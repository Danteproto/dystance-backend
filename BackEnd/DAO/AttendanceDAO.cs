using BackEnd.Context;
using BackEnd.Models;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.DAO
{
    public interface IAttendanceDAO
    {
        public Task<IActionResult> UpdateAttendance(List<AttendanceReports> attendances);
    }
    public class AttendanceDAO : IAttendanceDAO
    {
        private readonly UserDbContext _context;

        public AttendanceDAO(UserDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> UpdateAttendance(List<AttendanceReports> attendances)
        {
            try
            {
                _context.AttendanceReports.UpdateRange(attendances);
                await _context.SaveChangesAsync();
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

    }
}
