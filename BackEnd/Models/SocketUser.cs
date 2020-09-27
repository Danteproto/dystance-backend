using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class SocketUser
    {
        public string UserId { get; set; }
        public DateTime LastPing { get; set; }
    }
}
