using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class ModalidadeRepository : BaseRepository<Modalidade>, IModalidadeRepository
    {
        public ModalidadeRepository(AppDbContext context) : base (context){}

        public List<Modalidade> GetByAcademia(int academiaId)
        {
            return _context.Modalidades
                .Where(a => a.AcademiaId == academiaId)
                .ToList();
        }
    }
}