using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class AcademiaRepository : BaseRepository<Academia>, IAcademiaRepository
    {
        public AcademiaRepository(AppDbContext context) :base (context){}
        
    }
}