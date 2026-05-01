using MA_Sys.API.Dto.Turmas;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class TurmaService
    {
        private readonly AppDbContext _context;

        public TurmaService(AppDbContext context)
        {
            _context = context;
        }

        public List<TurmaResponseDto> List(int academiaId)
        {
            // Execute database query first
            var turmas = _context.Set<Turma>()
                .AsNoTracking()
                .Where(t => t.AcademiaId == academiaId)
                .Include(t => t.Professor)
                .Include(t => t.Alunos)
                .ThenInclude(ta => ta.Aluno)
                .OrderBy(t => t.Nome)
                .ToList();

            // Then do client-side processing
            return turmas
                .Select(t => new TurmaResponseDto
                {
                    Id = t.Id,
                    Nome = t.Nome,
                    Descricao = t.Descricao,
                    ProfessorId = t.ProfessorId,
                    ProfessorNome = t.Professor?.Nome,
                    DiasSemana = string.IsNullOrWhiteSpace(t.DiasSemana)
                        ? new List<string>()
                        : t.DiasSemana.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(d => d.Trim()).ToList(),
                    Ativo = t.Ativo,
                    Alunos = t.Alunos
                        .OrderBy(a => a.Aluno!.Nome)
                        .Select(a => new TurmaAlunoDto
                        {
                            AlunoId = a.AlunoId,
                            Nome = a.Aluno!.Nome ?? string.Empty
                        })
                        .ToList()
                })
                .ToList();
        }

        public TurmaResponseDto Add(TurmaCreateUpdateDto dto, int academiaId)
        {
            var turma = new Turma
            {
                AcademiaId = academiaId,
                Nome = dto.Nome.Trim(),
                Descricao = dto.Descricao?.Trim(),
                ProfessorId = ValidarProfessor(dto.ProfessorId, academiaId),
                DiasSemana = string.Join(",", dto.DiasSemana.Distinct(StringComparer.OrdinalIgnoreCase)),
                Ativo = dto.Ativo
            };

            _context.Set<Turma>().Add(turma);
            _context.SaveChanges();

            SincronizarAlunos(turma.Id, dto.AlunoIds);
            return GetById(turma.Id, academiaId)!;
        }

        public TurmaResponseDto Update(int id, TurmaCreateUpdateDto dto, int academiaId)
        {
            var turma = _context.Set<Turma>()
                .Include(t => t.Alunos)
                .FirstOrDefault(t => t.Id == id && t.AcademiaId == academiaId);

            if (turma == null)
            {
                throw new InvalidOperationException("Turma nao encontrada.");
            }

            turma.Nome = dto.Nome.Trim();
            turma.Descricao = dto.Descricao?.Trim();
            turma.ProfessorId = ValidarProfessor(dto.ProfessorId, academiaId);
            turma.DiasSemana = string.Join(",", dto.DiasSemana.Distinct(StringComparer.OrdinalIgnoreCase));
            turma.Ativo = dto.Ativo;

            SincronizarAlunos(turma.Id, dto.AlunoIds);
            _context.SaveChanges();
            return GetById(turma.Id, academiaId)!;
        }

        public TurmaResponseDto? GetById(int id, int academiaId)
        {
            return List(academiaId).FirstOrDefault(t => t.Id == id);
        }

        public void Delete(int id, int academiaId)
        {
            var turma = _context.Set<Turma>()
                .Include(t => t.Alunos)
                .FirstOrDefault(t => t.Id == id && t.AcademiaId == academiaId);

            if (turma == null)
            {
                throw new InvalidOperationException("Turma nao encontrada.");
            }

            if (turma.Alunos.Count > 0)
            {
                _context.Set<TurmaAluno>().RemoveRange(turma.Alunos);
            }

            _context.Set<Turma>().Remove(turma);
            _context.SaveChanges();
        }

        private void SincronizarAlunos(int turmaId, IEnumerable<int> alunoIds)
        {
            var ids = alunoIds.Distinct().ToList();
            var atuais = _context.Set<TurmaAluno>().Where(ta => ta.TurmaId == turmaId).ToList();

            var remover = atuais.Where(a => !ids.Contains(a.AlunoId)).ToList();
            if (remover.Count > 0)
            {
                _context.Set<TurmaAluno>().RemoveRange(remover);
            }

            var existentes = atuais.Select(a => a.AlunoId).ToHashSet();
            var novos = ids.Where(id => !existentes.Contains(id))
                .Select(id => new TurmaAluno { TurmaId = turmaId, AlunoId = id });

            _context.Set<TurmaAluno>().AddRange(novos);
        }

        private int? ValidarProfessor(int? professorId, int academiaId)
        {
            if (!professorId.HasValue || professorId.Value <= 0)
            {
                return null;
            }

            var professorExiste = _context.Professores
                .Any(p => p.Id == professorId.Value && p.AcademiaId == academiaId);

            if (!professorExiste)
            {
                throw new InvalidOperationException("Professor nao encontrado para esta turma.");
            }

            return professorId.Value;
        }
    }
}
