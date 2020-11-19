using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    public class ScheduleResponse
    {
        public string id { get; set; }
        public string date { get; set; }
        public string startTime { get; set; }
        public string endTime { get; set; }
        public string subject { get; set; }
        public string @class { get; set; }
    }
}
