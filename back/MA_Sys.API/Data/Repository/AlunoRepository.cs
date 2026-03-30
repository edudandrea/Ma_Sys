using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class AlunoRepository : BaseRepository<Aluno>, IAlunoRepository
    {
        public AlunoRepository(AppDbContext context) : base(context) { }

        public List<Aluno> GetByAcademia(int academiaId)
        {
            return _context.Alunos
                .Where(a => a.AcademiaId == academiaId)
                .ToList();
        }


        public Aluno GetById(int id, int academiaId)
        {
            return _context.Alunos
                .FirstOrDefault(a => a.Id == id || a.AcademiaId == academiaId);
        }


    }

}
