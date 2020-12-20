using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class GoogleLoginRequest
    {
        public string TokenId { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
        public string RealName { get; set; }
        public string Dob { get; set; }
        public IFormFile Avatar { get; set; }
    }
}
