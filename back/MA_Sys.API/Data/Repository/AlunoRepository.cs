using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository
{
    public class AlunoRepository : IAlunoRepository
    {
        private readonly AppDbContext _context;

        public AlunoRepository(AppDbContext context)
        {
            _context = context;
        }

        public List<Aluno> GetByAcademia(int academiaId)
        {
            return _context.Alunos
                .Where(a => a.AcademiaId == academiaId)
                .ToList();
        }

        public IQueryable<Aluno> Query()
        {
            return _context.Alunos;
        }

        public Aluno GetById(int id, int academiaId)
        {
            return _context.Alunos
                .FirstOrDefault(a => a.Id == id || a.AcademiaId == academiaId);
        }

        public void Add(Aluno aluno)
        {
            _context.Alunos.Add(aluno);
        }

        public void Update(Aluno aluno)
        {
            _context.Alunos.Update(aluno);
        }

        public void Delete(Aluno aluno)
        {
            _context.Alunos.Remove(aluno);
        }

        public void Save()
        {
            _context.SaveChanges();
        }
    }

}
