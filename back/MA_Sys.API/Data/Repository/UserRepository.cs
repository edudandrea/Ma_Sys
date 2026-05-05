using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Data.Repository
{
    public class UserRepository : BaseRepository<Users>, IUserRepository
    {
        public UserRepository(AppDbContext context) : base(context) {}

        public Users? GetByLogin(string login)
        {
            return _context.User
                                .Include(u => u.Academia)
                                .Include(u => u.Federacao)
                                .FirstOrDefault(u => u.Login == login);
        }

        public bool HasAnyUser()
        {
            return _context.User.Any();
        }
    }
}
