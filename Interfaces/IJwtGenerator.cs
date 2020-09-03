using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Security
{
    public interface IJwtGenerator
    {
        string CreateToken(AppUser user);
    }
}
