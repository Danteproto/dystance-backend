using AutoMapper.Mappers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class UsersLog
    {
        [Key]
        public int UsersLogId { get; set; }
        public DateTime DateTime { get; set; }
        public string LogType { get; set; }
        public string RoomId { get; set; }
        public string UserId { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return String.Format("{0} {1} {2} {3} {4} {5}", UsersLogId, LogType, DateTime, RoomId, UserId, Description);
        }
    }
}
