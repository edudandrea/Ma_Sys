namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IBaseRepository<T>
    {
        IQueryable<T> Query();        
        T GetById(int id, int academiaId);
        void Add(T entity);
        void Update(T entity);
        void Delete(T entity);
        void Save();
    }
}