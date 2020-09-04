using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class JwtToken
    {
        public JwtToken(string token, string expireDate)
        {
            this.token = token;
            ExpireDate = expireDate;
        }

        public string token { get; set; }
        public string ExpireDate { get; set; }
    }
}
