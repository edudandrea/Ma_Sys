using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.AcademiasDto;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class AcademiaService
    {
        private readonly IAcademiaRepository _repo;

                                

        public AcademiaService(IAcademiaRepository repo)
                                
        {
            _repo = repo;
                        
        }

        public List<AcademiaResponseDto> List()
        {
            return _repo.Query()
                .Select(a => new AcademiaResponseDto            
            {
                Id = a.Id,
                Nome = a.Nome,
                Email = a.Email,
                Telefone = a.Telefone,
                Ativo = a.Ativo
            }).ToList();
        }

        public List<AcademiaResponseDto> Get(AcademiaFiltroDto filtro, int academiaId)
        {
            var query = _repo.Query()
            .Where(a => a.Id == academiaId);         

            if (filtro.Id.HasValue)
                query = query.Where(a => a.Id == filtro.Id);

            if (!string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(a => a.Nome.Contains(filtro.Nome));

            return query.Select(a => new AcademiaResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Telefone = a.Telefone,
                Ativo = a.Ativo,

            }).ToList();
        }

        public AcademiaDto GetById(int id, int academiaId)
        {
            return _repo.Query()
                .Where(a => a.Id == id && a.Id == academiaId)
                .Select(a => new AcademiaDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    Telefone = a.Telefone,
                    Email = a.Email,

                    Cidade = a.Cidade,
                    Estado = a.Estado,

                    DataCadastro = a.DataCadastro,

                    Ativo = a.Ativo
                })
                .FirstOrDefault();
        }

        public void Add(AcademiaCreateDto dto)
        {
            var academia = new Academia
            {
                Nome = dto.Nome,
                Telefone = dto.Telefone,
                Email = dto.Email,
                DataCadastro = DateTime.UtcNow,
                Ativo = true
            };

            _repo.Add(academia);
            _repo.Save();

            
        }

        public void Update(int id, AcademiaUpdateDto dto)
        {
            var academia = _repo.Query()
                        .FirstOrDefault(a => a.Id == id);

            if (academia == null)
                throw new Exception("Academia não encontrado");

            academia.Nome = dto.Nome?.Trim();
            academia.Telefone = dto.Telefone;
            academia.Cidade = dto.Cidade;
            academia.Estado = dto.Estado;
            academia.RedeSocial = dto.RedeSocial;
            academia.Email = dto.Email;
            academia.Responsavel = dto.Responsavel;

            _repo.Save();
        }

        public void Delete(int id, int academiaId)
        {
            var academia = _repo.GetById(id, academiaId);

            if (academia == null)
                throw new Exception("Academia não encontrado");

            _repo.Delete(academia);
            _repo.Save();
        }

        public void UpdateStatus(int id, int academiaId, bool ativo)
        {
            var academia = _repo.GetById(id, academiaId);

            if (academia == null)
                throw new Exception("Academia não encontrado");

            academia.Ativo = ativo;

            _repo.Update(academia);
            _repo.Save();
        }


    }
}