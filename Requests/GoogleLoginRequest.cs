using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class GoogleLoginRequest
    {
        public string TokenId { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string DOB { get; set; }
    }
}
