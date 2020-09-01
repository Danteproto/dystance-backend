using Microsoft.AspNetCore.Identity;
using System;

namespace API.Models
{
    public class AppUser : IdentityUser
    { 
        public string DisplayName { get; set; }
    }
}
