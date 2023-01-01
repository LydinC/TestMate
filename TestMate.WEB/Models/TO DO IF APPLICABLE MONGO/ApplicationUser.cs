using AspNetCore.Identity.MongoDbCore.Models;
using MongoDbGenericRepository.Attributes;

namespace TestMate.WEB.Models
{
    [CollectionName("Developers")]
    public class ApplicationUser : MongoIdentityUser<Guid>
    {

    }
}
