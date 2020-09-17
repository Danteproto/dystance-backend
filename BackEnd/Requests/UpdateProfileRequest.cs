using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class UpdateProfileRequest
    {
        public string Email { get; set; }
        public string UserName { get; set; }
        public string Dob { get; set; }
        public string PhoneNumber { get; set; }
        public string RealName { get; set; }


    }
}
