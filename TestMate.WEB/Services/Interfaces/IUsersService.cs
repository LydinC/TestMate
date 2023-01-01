using TestMate.Common.Models.Users;

namespace TestMate.WEB.Services.Interfaces
{
    public interface IUsersService
    {
        Task<IEnumerable<User>> GetAllUserDetails();
        Task<User> GetUserDetails(string username);

    }
}
