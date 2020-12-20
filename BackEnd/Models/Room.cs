using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class Room
    {
        public int RoomId { get; set; }
        public string Subject { get; set; }
        public string? ClassName { get; set; }
        public string? CreatorId { get; set; }
        public string? Image { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool Group { get; set; }
        public int? MainRoomId { get; set; }
        public int? SemesterId { get; set; }
    }
}
