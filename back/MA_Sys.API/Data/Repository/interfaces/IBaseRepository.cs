using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IBaseRepository<T>
    {
        IQueryable<T> Query();        
        T GetById(int id);
        void Add(T entity);
        void Update(T entity);
        void delete(T entity);
        void Save();
    }
}