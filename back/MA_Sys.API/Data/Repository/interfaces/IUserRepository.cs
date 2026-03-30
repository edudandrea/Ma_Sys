using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IUserRepository : IBaseRepository<Users>
    {
        Users GetByLogin(string login);
        
    }
}