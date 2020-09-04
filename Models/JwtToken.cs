using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class JwtToken
    {
        public JwtToken(string token, string expireDate, string startDate)
        {
            this.token = token;
            ExpireDate = expireDate;
            StartDate = startDate;
        }

        public string token { get; set; }
        public string StartDate { get; set; }
        public string ExpireDate { get; set; }
    }
}
