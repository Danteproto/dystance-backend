using BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BackEnd.Models
{
    public class AppUser : IdentityUser
    { 
        public string DisplayName { get; set; }
        [JsonIgnore]
        public string Password { get; set; }

        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }
    }
}
