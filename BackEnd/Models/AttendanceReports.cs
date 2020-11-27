using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class AttendanceReports
    {
        [Key]
        public int AttendanceId { get; set; }
        public string UserId { get; set; }
        public string Status { get; set; }
        public int TimeTableId { get; set; }
    }
}
