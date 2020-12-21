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
        public Task<IActionResult> CreateAttendance(List<AttendanceReports> attendances);
        public Task<IActionResult> DeleteAttendance(List<AttendanceReports> attendances);
        public List<AttendanceReports> GetAttendanceBySchedule(int scheduleId);
        public AttendanceReports GetAttendanceByScheduleUserId(int scheduleId, string userId);
    }
    public class AttendanceDAO : IAttendanceDAO
    {
        private readonly UserDbContext _context;

        public AttendanceDAO(UserDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> CreateAttendance(List<AttendanceReports> attendances)
        {
            try
            {
                _context.AttendanceReports.AddRange(attendances);
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

        public async Task<IActionResult> UpdateAttendance(List<AttendanceReports> attendances)
        {
            try
            {
                _context.AttendanceReports.UpdateRange(attendances);
                await _context.SaveChangesAsync();
                return new ObjectResult(new { message = "Update success!" })
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

        public List<AttendanceReports> GetAttendanceBySchedule(int scheduleId)
        {
            return _context.AttendanceReports.Where(item => item.TimeTableId == scheduleId).ToList();
        }

        public async Task<IActionResult> DeleteAttendance(List<AttendanceReports> attendances)
        {
            try
            {
                _context.AttendanceReports.RemoveRange(attendances);
                await _context.SaveChangesAsync();
                return new ObjectResult(new { message = "Delete success!" })
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

        public AttendanceReports GetAttendanceByScheduleUserId(int scheduleId, string userId)
        {
            return _context.AttendanceReports.Where(item => item.TimeTableId == scheduleId && item.UserId == userId).FirstOrDefault();
        }
    }
}
