using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace BackEnd.Models
{
    public class AuthenticateResponse
    {
        public string Id { get; set; }
    
        public string UserName { get; set; }
        public string JwtToken { get; set; }
        public string RefreshToken { get; set; }

        public string Expires { get; set; }

        public AuthenticateResponse(User user, string jwtToken, string refreshToken, string expires)
        {
            Id = user.Id;

            UserName = user.Username;
            JwtToken = jwtToken;
            RefreshToken = refreshToken;
            Expires = expires;
        }

    }
}
