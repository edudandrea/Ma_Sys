using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class AcademiaRepository : IAcademiaRepository
    {
        private readonly AppDbContext _context;

        public AcademiaRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Academia> GetByAcademia(int academiaId)
        {
            return _context.Academias
                .Where(a => a.Id == academiaId)
                .ToList();
        }

        public IQueryable<Academia> Query()
        {
            return _context.Academias;
        }

        public Academia GetById(int id, int academiaId)
        {
            return _context.Academias
                .FirstOrDefault(a => a.Id == id || a.Id == academiaId);
        }

        public void Add(Academia academia)
        {
            _context.Academias.Add(academia);
        }

        public void Update(Academia academia)
        {
            _context.Academias.Update(academia);
        }

        public void Delete(Academia academia)
        {
            _context.Academias.Remove(academia);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }
}