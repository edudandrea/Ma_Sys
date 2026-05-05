using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.FormaPagamentos;
using MA_Sys.API.Models;
using MA_Sys.API.Security;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class FormaPagamentoService
    {
        private readonly IFormaPagamentoRepository _repo;
        private readonly IAcademiaRepository _academiaRepo;

        public FormaPagamentoService(IFormaPagamentoRepository repo, IAcademiaRepository academiaRepo)
        {
            _repo = repo;
            _academiaRepo = academiaRepo;
        }

        public List<FormaPagamentoResponseDto> List(int academiaId)
        {
            return _repo.Query()
                .Include(m => m.Academia)
                .Where(m => m.AcademiaId == academiaId)
                .ToList()
                .Select(a => MapToResponse(a))
                .ToList();
        }

        public List<FormaPagamentoResponseDto> Get(string role, FormaPagamentoFiltroDto filtro, int? academiaId, int? userId)
        {
            IQueryable<FormaPagamento> query = _repo.Query().AsNoTracking().Include(m => m.Academia);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar formas de pagamento.");

                query = query.Where(m => m.OwnerUserId == userId.Value);
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para acessar formas de pagamento.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id);

                query = query.Where(m => m.AcademiaId.HasValue && academiaIds.Contains(m.AcademiaId.Value));
            }
            else
            {
                if (academiaId == null)
                    throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode acessar formas de pagamento.");

                query = query.Where(m => m.AcademiaId == academiaId);
            }

            if (filtro != null && !string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(m => m.Nome.Contains(filtro.Nome));

            return query.ToList().Select(m => MapToResponse(m)).ToList();
        }

        public void Add(FormaPagamentoCreateDto dto, int? academiaId, string role, int? userId)
        {
            int? academiaDestino = RoleScope.IsAcademia(role) ? academiaId ?? 0 : dto.AcademiaId;
            int? ownerUserId = null;

            if (!RoleScope.IsAdmin(role) && !RoleScope.IsSuperAdmin(role) && !RoleScope.IsFederacao(role) && academiaId == null)
                throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode criar formas de pagamento.");

            if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para cadastrar formas de pagamento.");

                academiaDestino = null;
                ownerUserId = userId.Value;
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para cadastrar formas de pagamento.");

                if (!academiaDestino.HasValue)
                    throw new InvalidOperationException("Informe uma academia para cadastrar a forma de pagamento.");

                var academiaPertenceAoUsuario = _academiaRepo.Query()
                    .Any(a => a.Id == academiaDestino.Value && a.OwnerUserId == userId.Value);

                if (!academiaPertenceAoUsuario)
                    throw new UnauthorizedAccessException("Usuario nao pode cadastrar formas de pagamento fora do seu escopo.");
            }
            else if (!RoleScope.IsSuperAdmin(role) && !academiaDestino.HasValue)
            {
                throw new UnauthorizedAccessException("Usuario sem vinculo com academia nao pode criar formas de pagamento.");
            }
            else if (RoleScope.IsSuperAdmin(role) && !academiaDestino.HasValue)
            {
                throw new InvalidOperationException("Informe uma academia para cadastrar a forma de pagamento.");
            }

            var formaPagamento = new FormaPagamento
            {
                Nome = dto.Nome,
                AcademiaId = academiaDestino,
                OwnerUserId = ownerUserId,
                Ativo = true,
                Taxa = dto.Taxa,
                Parcelas = dto.Parcelas,
                Dias = dto.Dias
            };

            _repo.Add(formaPagamento);
            _repo.Save();
        }

        public void Update(int id, FormaPagamentoUpdateDto dto, string role, int? academiaId, int? userId)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar forma de pagamento.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar forma de pagamento.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id)
                    .ToList();

                query = query.Where(a => a.AcademiaId.HasValue && academiaIds.Contains(a.AcademiaId.Value));
            }
            else
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            var formaPagamento = query.FirstOrDefault();

            if (formaPagamento == null)
                throw new Exception("Forma de pagamento nao encontrada");

            formaPagamento.Nome = dto.Nome;
            formaPagamento.Ativo = dto.Ativo;
            formaPagamento.Taxa = dto.Taxa;
            formaPagamento.Parcelas = dto.Parcelas;
            formaPagamento.Dias = dto.Dias;

            _repo.Save();
        }

        public void UpdateStatus(int id, string role, int? academiaId, int? userId, bool ativo)
        {
            var query = _repo.Query().Where(a => a.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsFederacao(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar status da forma de pagamento.");

                query = query.Where(a => a.OwnerUserId == userId.Value);
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario invalido para alterar status da forma de pagamento.");

                var academiaIds = _academiaRepo.Query()
                    .Where(a => a.OwnerUserId == userId.Value)
                    .Select(a => a.Id)
                    .ToList();

                query = query.Where(a => a.AcademiaId.HasValue && academiaIds.Contains(a.AcademiaId.Value));
            }
            else
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            var formaPagamento = query.FirstOrDefault();

            if (formaPagamento == null)
                throw new Exception("Forma de pagamento nao encontrada");

            formaPagamento.Ativo = ativo;

            _repo.Update(formaPagamento);
            _repo.Save();
        }

        private static FormaPagamentoResponseDto MapToResponse(FormaPagamento formaPagamento)
        {
            return new FormaPagamentoResponseDto
            {
                Id = formaPagamento.Id,
                AcademiaId = formaPagamento.AcademiaId,
                OwnerUserId = formaPagamento.OwnerUserId,
                AcademiaNome = formaPagamento.Academia?.Nome,
                Escopo = formaPagamento.OwnerUserId.HasValue ? "Federacao" : "Academia",
                Nome = formaPagamento.Nome,
                Ativo = formaPagamento.Ativo,
                Taxa = formaPagamento.Taxa,
                Parcelas = formaPagamento.Parcelas,
                Dias = formaPagamento.Dias
            };
        }
    }
}
