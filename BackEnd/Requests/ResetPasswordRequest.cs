using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class ResetPasswordRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
