using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class UpdateAttendanceStudentRequest
    {
        public string Id { get; set; }
        public IEnumerable<AttendanceStudent> Students { get; set; }

    }
}
