using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class RoomChat
    {
        [Key]
        public int ChatId { get; set; }
        [Required]
        public int RoomId { get; set; }
        [Required]
        public int UserId { get; set; }
        [Required]
        public DateTime Date { get; set; }
        [Required]
        public string Content { get; set; }
    }
}
