using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Responses
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class UserInfoResponse
    {

        public string Id { get; set; }

        public string UserName { get; set; }
        public string RealName { get; set; }
        public string Email { get; set; }
        public string Dob { get; set; }
        public string Avatar { get; set; }
        public string Role { get; set; }


    }
}
