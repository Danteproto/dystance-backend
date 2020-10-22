using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class UpdateDeadlineRequest
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string DeadlineTime { get; set; }
        public string DeadlineDate { get; set; }
        public string Description { get; set; }
        public string RoomId { get; set; }
    }
}
