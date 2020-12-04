using BackEnd.Models;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace BackEnd.Models
{
    public class AppUser : IdentityUser
    { 
        public string RealName { get; set; }
        public DateTime DOB { get; set; }

        [JsonIgnore]
        public List<RefreshToken> RefreshTokens { get; set; }

        public string Avatar { get; set; }

    }
}
