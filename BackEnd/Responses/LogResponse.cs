using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class LogResponse
    {
        public string DateTime { get; set; }
        public string LogType { get; set; }
        public string RoomId { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }


    }
}
