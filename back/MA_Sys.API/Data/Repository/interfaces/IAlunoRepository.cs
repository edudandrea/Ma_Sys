using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Data.Repository.interfaces
{
    public interface IAlunoRepository
    {
        List<Aluno> GetByAcademia(int academiaId);
        Aluno GetById(int id, int academiaId);
        void Add(Aluno aluno);
        void Update(Aluno aluno);
        void Delete (Aluno aluno);
        void Save();
    }
}