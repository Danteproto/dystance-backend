using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class Deadline
    {
        [Key]
        public int DeadlineId { get; set; }
        public TimeSpan DeadlineTime { get; set; }
        public DateTime DeadlineDate { get; set; }
        public string Description { get; set; }
        public string CreatorId { get; set; }
        public int RoomId { get; set; }

    }
}
