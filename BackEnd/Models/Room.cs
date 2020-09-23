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
        [Required]
        public string Description { get; set; }
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; }
        [Required]
        [DataType (dataType: DataType.Time)]
        public TimeSpan StartHour { get; set; }
        [Required]
        [DataType(dataType: DataType.Time)]
        public TimeSpan EndHour { get; set; }
    }
}
