using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.MensalidadesSistema;
using MA_Sys.API.Models;
using MA_Sys.API.Security;
using Microsoft.EntityFrameworkCore;

namespace MA_Sys.API.Services
{
    public class MensalidadeSistemaService
    {
        private readonly IMensalidadeSistemaRepository _repo;

        public MensalidadeSistemaService(IMensalidadeSistemaRepository repo)
        {
            _repo = repo;
        }

        public List<MensalidadeSistemaResponseDto> Listar(string role, int? userId)
        {
            var query = _repo.Query().AsNoTracking();

            if (RoleScope.IsSuperAdmin(role))
            {
            }
            else if (RoleScope.IsAdmin(role))
            {
                if (!userId.HasValue)
                    throw new UnauthorizedAccessException("Usuario administrador invalido.");

                query = query.Where(x => x.OwnerUserId == userId.Value);
            }
            else
            {
                throw new UnauthorizedAccessException("Perfil sem permissao para acessar mensalidades do sistema.");
            }

            return query
                .OrderByDescending(x => x.Ativo)
                .ThenBy(x => x.MesesUso)
                .ThenBy(x => x.Valor)
                .Select(x => new MensalidadeSistemaResponseDto
                {
                    Id = x.Id,
                    Valor = x.Valor,
                    PrazoPagamentoDias = x.PrazoPagamentoDias,
                    MesesUso = x.MesesUso,
                    Ativo = x.Ativo,
                    Descricao = x.Descricao,
                    DataCadastro = x.DataCadastro
                })
                .ToList();
        }

        public void Adicionar(MensalidadeSistemaCreateDto dto, string role, int? userId)
        {
            ValidarPermissao(role, userId);
            ValidarDto(dto.Valor, dto.PrazoPagamentoDias, dto.MesesUso);

            var mensalidade = new MensalidadeSistema
            {
                Valor = dto.Valor,
                PrazoPagamentoDias = dto.PrazoPagamentoDias,
                MesesUso = dto.MesesUso,
                Descricao = dto.Descricao?.Trim(),
                Ativo = true,
                DataCadastro = DateTime.UtcNow,
                OwnerUserId = RoleScope.IsAdmin(role) ? userId : null
            };

            _repo.Add(mensalidade);
            _repo.Save();
        }

        public void Atualizar(int id, MensalidadeSistemaUpdateDto dto, string role, int? userId)
        {
            ValidarPermissao(role, userId);
            ValidarDto(dto.Valor, dto.PrazoPagamentoDias, dto.MesesUso);

            var mensalidade = QueryPorEscopo(id, role, userId).FirstOrDefault();

            if (mensalidade == null)
                throw new InvalidOperationException("Mensalidade do sistema nao encontrada.");

            mensalidade.Valor = dto.Valor;
            mensalidade.PrazoPagamentoDias = dto.PrazoPagamentoDias;
            mensalidade.MesesUso = dto.MesesUso;
            mensalidade.Ativo = dto.Ativo;
            mensalidade.Descricao = dto.Descricao?.Trim();

            _repo.Update(mensalidade);
            _repo.Save();
        }

        public void AtualizarStatus(int id, bool ativo, string role, int? userId)
        {
            ValidarPermissao(role, userId);

            var mensalidade = QueryPorEscopo(id, role, userId).FirstOrDefault();

            if (mensalidade == null)
                throw new InvalidOperationException("Mensalidade do sistema nao encontrada.");

            mensalidade.Ativo = ativo;

            _repo.Update(mensalidade);
            _repo.Save();
        }

        private IQueryable<MensalidadeSistema> QueryPorEscopo(int id, string role, int? userId)
        {
            var query = _repo.Query().Where(x => x.Id == id);

            if (RoleScope.IsSuperAdmin(role))
            {
                return query;
            }

            return query.Where(x => x.OwnerUserId == userId);
        }

        private static void ValidarPermissao(string role, int? userId)
        {
            if (RoleScope.IsSuperAdmin(role))
                return;

            if (RoleScope.IsAdmin(role) && userId.HasValue)
                return;

            throw new UnauthorizedAccessException("Perfil sem permissao para gerenciar mensalidades do sistema.");
        }

        private static void ValidarDto(decimal valor, int prazoPagamentoDias, int mesesUso)
        {
            if (valor <= 0)
                throw new InvalidOperationException("Informe um valor maior que zero.");

            if (prazoPagamentoDias <= 0)
                throw new InvalidOperationException("Informe um prazo de pagamento maior que zero.");

            if (mesesUso <= 0)
                throw new InvalidOperationException("Informe a quantidade de meses de uso.");
        }
    }
}
