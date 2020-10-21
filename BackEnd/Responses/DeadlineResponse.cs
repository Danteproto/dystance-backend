using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    public class DeadlineResponse
    {
        public int DeadlineId { get; set; }
        public string Title { get; set; }
        public string EndDate { get; set; }
        public string Description { get; set; }
        public string RoomId { get; set; }

    }
}
