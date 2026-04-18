using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Matriculas;
using MA_Sys.API.Models;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class MatriculaService
    {
        private readonly IMatriculaRepository _repo;
        private readonly IAlunoRepository _alunoRepo;
        private readonly IPlanosRepository _planoRepo;
        private readonly IFormaPagamentoRepository _formaPgtoRepo;
        private readonly IPagamentoRepository _pagamentoRepo;

        public MatriculaService(IMatriculaRepository repo,
                                IAlunoRepository alunoRepo,
                                IPlanosRepository planoRepo,
                                IFormaPagamentoRepository formaPgtoRepo,
                                IPagamentoRepository pagamentoRepo)
        {
            _repo = repo;
            _alunoRepo = alunoRepo;
            _planoRepo = planoRepo;
            _formaPgtoRepo = formaPgtoRepo;
            _pagamentoRepo = pagamentoRepo;
        }

        private List<Pagamentos> GerarPagamentos(Matricula matricula, Plano plano, FormaPagamento formaPgto, int formaPgtoId)
        {
            var lista = new List<Pagamentos>();
            var valorParcela = plano.Valor / formaPgto.Parcelas;

            for (int i = 0; i < formaPgto.Parcelas; i++)
            {
                var vencimento = matricula.DataInicio.AddDays(formaPgto.Dias * i);
                lista.Add(new Pagamentos
                {
                    AcademiaId = matricula.AcademiaId,
                    AlunoId = matricula.AlunoId,
                    PlanoId = matricula.PlanoId,
                    DataVencimento = vencimento,
                    Valor = valorParcela,
                    Status = "Pendente",
                    FormaPagamentoId = formaPgtoId
                });
            }
            return lista;
        }

        public List<MatriculasResponseDto> List(int academiaId)
        {
            var modalidade = _repo.Query();

            modalidade = modalidade.Where(m => m.AcademiaId == academiaId);

            return modalidade.Select(a => new MatriculasResponseDto
            {
                Id = a.Id,
                AlunoId = a.AlunoId,
                PlanoId = a.PlanoId,
                DataInicio = a.DataInicio,
                DataFim = a.DataFim,
                Status = a.Status

            }).ToList();
        }

        public List<MatriculasResponseDto> Get(string role, MatriculasFiltro filtro, int? academiaId)
        {
            Console.WriteLine($"ROLE: {role}");
            Console.WriteLine($"ACADEMIA ID NO GET: {academiaId}");

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (!academiaId.HasValue)
                    throw new UnauthorizedAccessException("Usuário sem vínculo com academia");
            }

            var query = _repo.Query().AsNoTracking();

            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                query = query.Where(m => m.AcademiaId == academiaId);
            }
            
            var result = query.Select(m => new MatriculasResponseDto
            {
                Id = m.Id,
                AlunoId = m.AlunoId,

                AlunoNome = _alunoRepo.Query()
                    .Where(a => a.Id == m.AlunoId)
                    .Select(a => a.Nome)
                    .FirstOrDefault(),

                AcademiaId = m.AcademiaId,
                PlanoId = m.PlanoId,
                DataInicio = m.DataInicio,
                DataFim = m.DataFim,
                Status = m.Status
            });

            // 🔥 FILTRO POR NOME (APÓS PROJEÇÃO)
            if (filtro != null && !string.IsNullOrEmpty(filtro.AlunoNome))
            {
                result = result.Where(m => m.AlunoNome.Contains(filtro.AlunoNome));
            }

            return result.ToList();
        }


        public async Task<Matricula> Add(MatriculasCreateDto dto, int? academiaId, string role)
        {
            if (!string.Equals(role, "Admin", StringComparison.OrdinalIgnoreCase))
            {
                if (!academiaId.HasValue)
                    throw new UnauthorizedAccessException("Usuário sem vínculo com academia");
            }

            var plano = _planoRepo.Query().FirstOrDefault(p => p.Id == dto.PlanoId);
            if (plano == null)
                throw new Exception("Plano não encontrado");

            var dataInicio = dto.DataInicio ?? DateTime.Now;

            var matricula = new Matricula
            {
                AlunoId = dto.AlunoId,
                PlanoId = dto.PlanoId,
                AcademiaId = academiaId.Value, // 🔥 ESSENCIAL
                DataInicio = dataInicio,
                DataFim = dataInicio.AddMonths(plano.DuracaoMeses),
                Status = "Ativa"
            };

            _repo.Add(matricula);
            _repo.Save();

            return matricula;
        }

    }
}
