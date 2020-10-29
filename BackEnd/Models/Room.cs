using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class Room
    {
        [Key]
        public int RoomId { get; set; }
        [Required]
        public string RoomName { get; set; }
        [Required]
        public string CreatorId { get; set; }
        public string Image { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? RepeatOccurrence { get; set; }
        public bool Group { get; set; }
        public int? MainRoomId { get; set; }
    }
}
