using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class Timetable
    {
        [Key]
        public int Id { get; set; }
        public int RoomId { get; set; }
        [Required]
        [JsonProperty("dayOfWeek")]
        public string DayOfWeek { get; set; }
        [Required]
        [DataType(dataType: DataType.Time)]
        [JsonProperty("startTime")]
        public TimeSpan StartTime { get; set; }
        [Required]
        [DataType(dataType: DataType.Time)]
        [JsonProperty("endTime")]
        public TimeSpan EndTime { get; set; }
    }
}
