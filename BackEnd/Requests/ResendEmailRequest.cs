using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class ResendEmailRequest
    {
        public string UserName { get; set; }
        public string Email { get; set; }
    }
}
