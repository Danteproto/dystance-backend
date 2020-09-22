using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class RoomUserLink
    {
        [Key]
        public int RoomUserId { get; set; }
        [Required]
        public int RoomId { get; set; }
        [Required]
        public string UserId { get; set; }
    }
}
