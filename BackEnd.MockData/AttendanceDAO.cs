using BackEnd.DAO;
using BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BackEnd.MockData
{
    public class AttendanceDAO : IAttendanceDAO
    {
        public async Task<IActionResult> UpdateAttendance(List<AttendanceReports> attendances) => await Task.FromResult(new OkObjectResult(attendances));
    }
}
