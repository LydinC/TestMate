using AutoMapper;
using TestMate.Common.DataTransferObjects.Devices;
using TestMate.Common.DataTransferObjects.Users;
using TestMate.Common.Models.Users;

namespace TestMate.API.Profiles
{
    public class UserProfile : Profile
    {
        public UserProfile()
        {
            CreateMap<UserRegisterDTO, User>();

        }
    }
}
