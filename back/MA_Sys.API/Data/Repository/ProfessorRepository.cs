using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class ProfessorRepository : BaseRepository<Professor>, IProfessorRepository
    {
        public ProfessorRepository(AppDbContext context) : base (context){}

        public List<Professor> GetByAcademia(int academiaId)
        {
            return _context.Professores
                .Where(a => a.AcademiaId == academiaId)
                .ToList();
        }
    }
}