using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.DashboardDto;
using SQLitePCL;

namespace MA_Sys.API.Services
{
    public class DashboardService
    {
        private readonly IProfessorRepository _professorRepo;
        private readonly IAcademiaRepository _academiaRepo;
        private readonly IAlunoRepository _alunoRepo;
        public DashboardService(IAlunoRepository alunoRepo, IAcademiaRepository academiaRepo, IProfessorRepository professorRepo)
        {
            _alunoRepo = alunoRepo;
            _academiaRepo = academiaRepo;
            _professorRepo = professorRepo;
        }   
        public DashboardDto GetDashboard()
        {
            var totalAcademias = _academiaRepo.Query().Count();
            var totalAlunos = _alunoRepo.Query().Count();
            var totalProfessores = _professorRepo.Query().Count();

            return new DashboardDto
            {
                TotalAcademias = totalAcademias,
                TotalAlunos = totalAlunos,
                TotalProfessores = totalProfessores
            };

        }
    }
}