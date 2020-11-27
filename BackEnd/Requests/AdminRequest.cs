using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class AdminRequest
    {
        public string Id { get; set; }
        public string Code { get; set; }
        public string Email { get; set; }
        public string RealName { get; set; }
        public string Role { get; set; }
    }
}
