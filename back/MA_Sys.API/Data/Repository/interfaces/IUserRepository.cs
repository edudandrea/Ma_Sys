using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IUserRepository
    {

        Users GetByLogin(string login);
        List<Users> GetByAcademia(int academiaId);
        Users GetById(int id);
        IQueryable<Users> Query(int academiaId);
        void Add(Users user);
        void Update(Users user);
        void Delete (Users user);        
        void Save();
    }
}