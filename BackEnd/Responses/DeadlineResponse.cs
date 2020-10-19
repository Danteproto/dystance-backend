using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    public class DeadlineResponse
    {
        public string Title { get; set; }
        public string DeadlineTime { get; set; }
        public string DeadlineDate { get; set; }
        public string Description { get; set; }
        public string RoomId { get; set; }
        public string RemainingTime { get; set; }

    }
}
