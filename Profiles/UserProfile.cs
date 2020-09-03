using AutoMapper;
using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Profiles
{
    public class UserProfile: Profile
    {
        public UserProfile()
        {
            CreateMap<AppUser, User>();
        }

    }
}
