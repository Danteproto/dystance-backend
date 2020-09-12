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
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }

        public int Expires { get; set; }

        public AuthenticateResponse(User user, string jwtToken, string refreshToken, int expires)
        {
            Id = user.Id;

            UserName = user.Username;
            AccessToken = jwtToken;
            RefreshToken = refreshToken;
            Expires = expires;
        }

    }
}
