using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class DeadlineRequest
    {
        public string Title { get; set; }
        public string DeadlineTime { get; set; }
        public string DeadlineDate { get; set; }
        public string Description { get; set; }
        public string RoomId { get; set; }
    }
}
