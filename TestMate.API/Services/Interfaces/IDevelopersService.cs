using TestMate.Common.DataTransferObjects.APIResponse;
using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.Models.Developers;

namespace TestMate.API.Services.Interfaces
{
    public interface IDevelopersService
    {
        Task<APIResponse<DeveloperLoginResultDTO>> Login(DeveloperLoginDTO developerLoginDTO);

        Task<APIResponse<IEnumerable<Developer>>> GetAllDevelopers();

        Task<APIResponse<Developer>> GetDeveloper(string username);

        Task<APIResponse<DeveloperRegisterResultDTO>> Register(DeveloperRegisterDTO developerRegisterDTO);

    }
}
