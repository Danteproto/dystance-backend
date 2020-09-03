using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    public class RegisterResponse
    {
        public string Username { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string TokenLink { get; set; }

    }
}
