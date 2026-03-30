using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;

namespace MA_Sys.API.Services
{
    public class AdminService
    {
        private readonly IAcademiaRepository _acadrepo;
        private readonly IAlunoRepository _alunoRepo;

        public AdminService(IAcademiaRepository acadRepo, IAlunoRepository alunoRepo)
        {
            _acadrepo = acadRepo;
            _alunoRepo = alunoRepo;
        }

        public int Totalacademia()
        {
            return _acadrepo.Query().Count();
        }

        public int TotalAlunos()
        {
            return _alunoRepo.Query().Count();
        }
    }
}