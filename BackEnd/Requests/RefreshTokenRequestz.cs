using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Requests
{
    [System.Diagnostics.CodeAnalysis.ExcludeFromCodeCoverage]
    public class RefreshTokenRequestz
    {
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
    }
}
