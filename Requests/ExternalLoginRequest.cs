using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    public class ExternalLoginRequest
    {
        public string provider { get; set; }
        public string returnUrl { get; set; }
    }
}
