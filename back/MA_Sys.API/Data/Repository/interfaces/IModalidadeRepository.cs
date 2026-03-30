using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IModalidadeRepository : IBaseRepository<Modalidade>
    {
        List<Modalidade> GetByAcademia(int academiaId);
    }
}