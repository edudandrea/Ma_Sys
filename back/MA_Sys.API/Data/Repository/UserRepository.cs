using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class UserRepository : BaseRepository<Users>, IUserRepository
    {
        
        public UserRepository(AppDbContext context) : base (context){}
       
        
        public Users GetByLogin(string login)
        {
            return _context.User.FirstOrDefault(u => u.Login == login);
        }
        
    }
}