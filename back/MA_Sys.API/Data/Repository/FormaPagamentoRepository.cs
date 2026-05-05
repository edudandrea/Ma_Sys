using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Models;
using MA_SYS.Api.Data;

namespace MA_Sys.API.Data.Repository
{
     public class FormaPagamentoRepository : BaseRepository<FormaPagamento>, IFormaPagamentoRepository
    {
        public FormaPagamentoRepository(AppDbContext context) : base(context) { }

        public List<FormaPagamento> GetByAcademia(int academiaId)
        {
            return _context.FormaPagamentos
                .Where(fp => fp.AcademiaId == academiaId)
                .ToList();
        }
    }
}
