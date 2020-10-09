using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class KickOutMemberRequest
    {
        public string UserId { get; set; }
        public string RoomId { get; set; }

    }
}
