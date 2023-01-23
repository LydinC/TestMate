using AutoMapper;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.Models.Developers;

namespace TestMate.API.Profiles
{
    public class DeveloperProfile : Profile
    {
        public DeveloperProfile()
        {
            CreateMap<DeveloperRegisterDTO, Developer>();

        }
    }
}
