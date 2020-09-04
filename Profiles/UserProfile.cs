using AutoMapper;
using BackEnd.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BackEnd.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<AppUser, User>();

            CreateMap<AppUser, RegisterRequest>().ForMember(dest =>
    dest.Username,
            opt => opt.MapFrom(src => src.UserName))
        .ForMember(dest =>
            dest.Email,
            opt => opt.MapFrom(src => src.Email));
        }

    }
}
