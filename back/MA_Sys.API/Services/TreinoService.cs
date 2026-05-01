using MA_Sys.API.Dto.Treinos;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class TreinoService
    {
        private readonly AppDbContext _context;

        public TreinoService(AppDbContext context)
        {
            _context = context;
        }

        public List<TreinoResponseDto> List(int academiaId)
        {
            return _context.Set<Treino>()
                .AsNoTracking()
                .Where(t => t.AcademiaId == academiaId)
                .Include(t => t.Professor)
                .Include(t => t.Exercicios)
                .ThenInclude(te => te.Exercicio)
                .Join(_context.Alunos.AsNoTracking(), t => t.AlunoId, a => a.Id, (t, a) => new TreinoResponseDto
                {
                    Id = t.Id,
                    AlunoId = t.AlunoId,
                    AlunoNome = a.Nome ?? string.Empty,
                    ProfessorId = t.ProfessorId,
                    ProfessorNome = t.Professor != null ? t.Professor.Nome : null,
                    Nome = t.Nome,
                    Objetivo = t.Objetivo,
                    Observacoes = t.Observacoes,
                    Ativo = t.Ativo,
                    Exercicios = t.Exercicios
                        .OrderBy(e => e.Ordem)
                        .Select(e => new TreinoExercicioDto
                        {
                            ExercicioId = e.ExercicioId,
                            ExercicioNome = e.Exercicio!.Nome,
                            Ordem = e.Ordem,
                            Series = e.Series,
                            Repeticoes = e.Repeticoes,
                            Descanso = e.Descanso,
                            Observacoes = e.Observacoes
                        }).ToList()
                })
                .OrderBy(t => t.AlunoNome)
                .ThenBy(t => t.Nome)
                .ToList();
        }

        public TreinoResponseDto Add(TreinoCreateUpdateDto dto, int academiaId)
        {
            ValidarAluno(dto.AlunoId, academiaId);
            var professorId = ValidarProfessor(dto.ProfessorId, academiaId);

            var treino = new Treino
            {
                AcademiaId = academiaId,
                AlunoId = dto.AlunoId,
                ProfessorId = professorId,
                Nome = dto.Nome.Trim(),
                Objetivo = dto.Objetivo?.Trim(),
                Observacoes = dto.Observacoes?.Trim(),
                Ativo = dto.Ativo
            };

            _context.Set<Treino>().Add(treino);
            _context.SaveChanges();
            SincronizarExercicios(treino.Id, dto.Exercicios);
            _context.SaveChanges();
            return List(academiaId).First(t => t.Id == treino.Id);
        }

        public TreinoResponseDto Update(int id, TreinoCreateUpdateDto dto, int academiaId)
        {
            ValidarAluno(dto.AlunoId, academiaId);
            var professorId = ValidarProfessor(dto.ProfessorId, academiaId);

            var treino = _context.Set<Treino>()
                .Include(t => t.Exercicios)
                .FirstOrDefault(t => t.Id == id && t.AcademiaId == academiaId);

            if (treino == null)
            {
                throw new InvalidOperationException("Treino nao encontrado.");
            }

            treino.AlunoId = dto.AlunoId;
            treino.ProfessorId = professorId;
            treino.Nome = dto.Nome.Trim();
            treino.Objetivo = dto.Objetivo?.Trim();
            treino.Observacoes = dto.Observacoes?.Trim();
            treino.Ativo = dto.Ativo;

            SincronizarExercicios(treino.Id, dto.Exercicios);
            _context.SaveChanges();
            return List(academiaId).First(t => t.Id == treino.Id);
        }

        public void Delete(int id, int academiaId)
        {
            var treino = _context.Set<Treino>()
                .Include(t => t.Exercicios)
                .FirstOrDefault(t => t.Id == id && t.AcademiaId == academiaId);

            if (treino == null)
            {
                throw new InvalidOperationException("Treino nao encontrado.");
            }

            if (treino.Exercicios.Count > 0)
            {
                _context.Set<TreinoExercicio>().RemoveRange(treino.Exercicios);
            }

            _context.Set<Treino>().Remove(treino);
            _context.SaveChanges();
        }

        private void ValidarAluno(int alunoId, int academiaId)
        {
            var alunoExiste = _context.Alunos.Any(a => a.Id == alunoId && a.AcademiaId == academiaId);
            if (!alunoExiste)
            {
                throw new InvalidOperationException("Aluno nao encontrado para este treino.");
            }
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
                throw new InvalidOperationException("Professor nao encontrado para este treino.");
            }

            return professorId.Value;
        }

        private void SincronizarExercicios(int treinoId, IEnumerable<TreinoExercicioDto> exercicios)
        {
            var atuais = _context.Set<TreinoExercicio>().Where(e => e.TreinoId == treinoId).ToList();
            if (atuais.Count > 0)
            {
                _context.Set<TreinoExercicio>().RemoveRange(atuais);
            }

            var novos = exercicios
                .OrderBy(e => e.Ordem)
                .Select(e => new TreinoExercicio
                {
                    TreinoId = treinoId,
                    ExercicioId = e.ExercicioId,
                    Ordem = e.Ordem,
                    Series = e.Series,
                    Repeticoes = e.Repeticoes,
                    Descanso = e.Descanso,
                    Observacoes = e.Observacoes
                });

            _context.Set<TreinoExercicio>().AddRange(novos);
        }
    }
}
