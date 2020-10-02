using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class UpdateProfileRequest
    {
        public string Dob { get; set; }
        public string RealName { get; set; }
        public IFormFile Avatar { get; set; }
        public string CurrentPassword { get; set; }
        public string NewPassword { get; set; }

    }
}
