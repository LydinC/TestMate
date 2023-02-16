using AutoMapper;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.DataTransferObjects.Devices;
using TestMate.Common.Models.Developers;
using TestMate.Common.Models.Devices;

namespace TestMate.API.Profiles
{
    public class DevicesProfile : Profile
    {
        public DevicesProfile()
        {
            CreateMap<DevicesConnectDTO, Device>();
        }
    }
}
