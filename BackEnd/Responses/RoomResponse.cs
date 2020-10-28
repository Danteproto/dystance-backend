using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    public class RoomResponse
    {
        public int RoomId { get; set; }
        public string RoomName { get; set; }
        public string CreatorId { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string RepeatOccurrence { get; set; }
        public string RoomTimes { get; set; }
    }
}
