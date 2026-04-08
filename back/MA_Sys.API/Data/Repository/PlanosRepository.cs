using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class PlanosRepository : BaseRepository<Plano>, IPlanosRepository
    {
        public PlanosRepository(AppDbContext context) : base (context){}

        public List<Plano> GetByAcademia(int academiaId)
        {
            return _context.Planos
                .Where(a => a.AcademiaId == academiaId)
                .ToList();
        }
    }
}