using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IAlunoRepository : IBaseRepository<Aluno>
    {
        List<Aluno> GetByAcademia(int academiaId);
        
    }
}