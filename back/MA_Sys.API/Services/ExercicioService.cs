using MA_Sys.API.Dto.Treinos;
using MA_SYS.Api.Data;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class ExercicioService
    {
        private readonly AppDbContext _context;

        public ExercicioService(AppDbContext context)
        {
            _context = context;
        }

        public List<ExercicioResponseDto> List(int academiaId)
        {
            return _context.Set<Exercicio>()
                .AsNoTracking()
                .Where(e => e.AcademiaId == academiaId)
                .OrderBy(e => e.Nome)
                .Select(e => new ExercicioResponseDto
                {
                    Id = e.Id,
                    Nome = e.Nome,
                    GrupoMuscular = e.GrupoMuscular,
                    Descricao = e.Descricao,
                    Ativo = e.Ativo
                })
                .ToList();
        }

        public ExercicioResponseDto Add(ExercicioCreateUpdateDto dto, int academiaId)
        {
            var entity = new Exercicio
            {
                AcademiaId = academiaId,
                Nome = dto.Nome.Trim(),
                GrupoMuscular = dto.GrupoMuscular?.Trim(),
                Descricao = dto.Descricao?.Trim(),
                Ativo = dto.Ativo
            };

            _context.Set<Exercicio>().Add(entity);
            _context.SaveChanges();
            return List(academiaId).First(e => e.Id == entity.Id);
        }

        public ExercicioResponseDto Update(int id, ExercicioCreateUpdateDto dto, int academiaId)
        {
            var entity = _context.Set<Exercicio>().FirstOrDefault(e => e.Id == id && e.AcademiaId == academiaId);
            if (entity == null)
            {
                throw new InvalidOperationException("Exercicio nao encontrado.");
            }

            entity.Nome = dto.Nome.Trim();
            entity.GrupoMuscular = dto.GrupoMuscular?.Trim();
            entity.Descricao = dto.Descricao?.Trim();
            entity.Ativo = dto.Ativo;
            _context.SaveChanges();
            return List(academiaId).First(e => e.Id == entity.Id);
        }

        public void Delete(int id, int academiaId)
        {
            var entity = _context.Set<Exercicio>().FirstOrDefault(e => e.Id == id && e.AcademiaId == academiaId);
            if (entity == null)
            {
                throw new InvalidOperationException("Exercicio nao encontrado.");
            }

            var vinculadoEmTreino = _context.Set<TreinoExercicio>().Any(te => te.ExercicioId == id);
            if (vinculadoEmTreino)
            {
                throw new InvalidOperationException("Nao e possivel excluir um exercicio vinculado a treinos.");
            }

            _context.Set<Exercicio>().Remove(entity);
            _context.SaveChanges();
        }
    }
}
