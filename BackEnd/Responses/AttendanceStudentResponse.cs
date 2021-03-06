﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class AttendanceStudentResponse
    {
        public string Id { get; set; }
        public string Class { get; set; }
        public string Subject { get; set; }
        public string Date { get; set; }
        public string StartTime { get; set; }
        public string EndTime { get; set; }
        public string Teacher { get; set; }
        public string Status { get; set; }
    }
}
