using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IAcademiaRepository
    {
        List<Academia> GetByAcademia(int academiaId);
        Academia GetById(int id, int academiaId);
        IQueryable<Academia> Query();
        void Add(Academia academia);
        void Update(Academia academia);
        void Delete (Academia academia);        
        void Save();



    }
}