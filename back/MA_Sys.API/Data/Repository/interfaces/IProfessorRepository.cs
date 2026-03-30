using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IProfessorRepository : IBaseRepository<Professor>
    {
        List<Professor> GetByAcademia(int academiaId);
    }
}