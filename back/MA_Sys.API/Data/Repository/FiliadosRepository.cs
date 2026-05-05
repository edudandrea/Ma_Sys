using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.interfaces;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Models;
using MA_SYS.Api.Data;

namespace MA_Sys.API.Data.Repository
{
    public class FiliadosRepository : BaseRepository<Filiados>, IFiliadosRepository
    {
        public FiliadosRepository(AppDbContext context) :base (context){}
    }
}