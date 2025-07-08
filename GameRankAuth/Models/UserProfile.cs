using GameRankAuth.Interfaces;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using GameRankAuth.Models;
namespace GameRankAuth.Models
{
    public class UserProfile : Profile
    {
        public UserProfile() 
        {
            CreateMap<IdentityUser, UserDto>();
            CreateMap<RegisterRequest, IdentityUser>().ForMember(dest => dest.UserName, opt => opt.MapFrom(src => src.UserName));
        }
    }
}
