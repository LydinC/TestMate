using TestMate.Common.DataTransferObjects.Developers;
using TestMate.Common.Models.Developers;

namespace TestMate.WEB.Services.Interfaces
{
    public interface IDevelopersService
    {
        Task<IEnumerable<Developer>> GetAllDeveloperDetails();
        Task<Developer> GetDeveloperDetails(string username);

        Task<Developer> RegisterDeveloper(Developer newDeveloper);

        Task<DeveloperLoginResultDTO> Login(DeveloperLoginDTO developerLoginDTO);

        Task<Developer> UpdateDeveloperDetails(string username, Developer updatedDeveloperDetails);

    }
}
