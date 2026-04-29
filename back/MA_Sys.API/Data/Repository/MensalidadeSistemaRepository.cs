using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Models;
using MA_SYS.Api.Data;

namespace MA_Sys.API.Data.Repository
{
    public class MensalidadeSistemaRepository : BaseRepository<MensalidadeSistema>, IMensalidadeSistemaRepository
    {
        public MensalidadeSistemaRepository(AppDbContext context) : base(context)
        {
        }
    }
}
