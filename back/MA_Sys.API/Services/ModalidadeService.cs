using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.ModalidadesDto;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class ModalidadeService
    {
        private readonly IModalidadeRepository _repo;

        public ModalidadeService(IModalidadeRepository repo)
        {
            _repo = repo;
        }

        public List<ModalidadeResponseDto> List(int academiaId)
        {
           var modalidade = _repo.Query();
           modalidade = modalidade.Where(m => m.AcademiaId == academiaId);

           return modalidade.Select(a => new ModalidadeResponseDto
           {
               Id = a.Id,
               NomeModalidade = a.NomeModalidade,
               Ativo = a.Ativo
           }).ToList();
        }

        public List<ModalidadeResponseDto> Get(ModalidadeFiltroDto filtro, int academiaId)
        {
            var query = _repo.Query();
            query = query.Where(m => m.AcademiaId == academiaId);

            if (!string.IsNullOrEmpty(filtro.NomeModalidade))
                query = query.Where(m => m.NomeModalidade.Contains(filtro.NomeModalidade));                 

            return query.Select(m => new ModalidadeResponseDto
            {
                Id = m.Id,
                NomeModalidade = m.NomeModalidade,                
                Ativo = m.Ativo,

            }).ToList();
        }

        public void Add(ModalidadeCreateDto dto, int academiaId)
        {
            var modalidade = new Modalidade
            {                
                NomeModalidade = dto.NomeModalidade,
                AcademiaId = academiaId,                
                Ativo = true
            };

            _repo.Add(modalidade);
            _repo.Save();
        }

        public void Update (int id, ModalidadeUpdateDto dto)
        {
            var modalidade = _repo.Query()
                        .FirstOrDefault(a => a.Id == id);

            if (modalidade == null)
                throw new Exception("Modalidade não encontrado");

            modalidade.NomeModalidade = dto.NomeModalidade?.Trim();
            modalidade.Ativo = dto.Ativo;

            _repo.Save();
        }

        public void UpdateStatus(int id, int academiaId, bool ativo)
        {
            var modalidade = _repo.GetById(id, academiaId);

            if (modalidade == null)
                throw new Exception("Modalidade não encontrado");

            modalidade.Ativo = ativo;

            _repo.Update(modalidade);
            _repo.Save();
        }

    }
}