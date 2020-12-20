using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ResetPasswordVerify
    {
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
