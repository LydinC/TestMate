using TestMate.Common.DataTransferObjects.TestRequests;
using TestMate.Common.Models.TestRequests;
using AutoMapper;
namespace TestMate.API.Profiles
{
    public class TestRequestProfile : Profile
    {
        public TestRequestProfile() {
            CreateMap<TestRequestCreateDTO, TestRequest>();
        }
    }
}
