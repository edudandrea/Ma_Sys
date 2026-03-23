using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.AcademiasDto;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class AcademiaService
    {
        private readonly IAcademiaRepository _repo;

        private readonly IUserRepository _userRepo;                        

        public AcademiaService(IAcademiaRepository repo, IUserRepository userRepo)
                                
        {
            _repo = repo;
            _userRepo = userRepo;
            
        }

        public List<AcademiaResponseDto> Listar(int academiaId)
        {
            var academia = _repo.GetByAcademia(academiaId);

            return academia.Select(a => new AcademiaResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                Email = a.Email,
                Telefone = a.Telefone,
                Ativo = a.Ativo
            }).ToList();
        }

        public List<AcademiaResponseDto> Buscar(AcademiaFiltroDto filtro)
        {
            var query = _repo.Query();            

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

        public AcademiaDto ObterPorId(int id, int academiaId)
        {
            return _repo.Query()
                .Where(a => a.Id == id)
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

        public void Criar(AcademiaCreateDto dto)
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

            var user = new Users
            {
                UserName = dto.User,
                Password = dto.Password,
                AcademiaId = academia.Id
            };

            _userRepo.Add(user);
            _userRepo.Save();
        }

        public void Atualizar(int id, AcademiaUpdateDto dto)
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

        public void Excluir(int id, int academiaId)
        {
            var aluno = _repo.GetById(id, academiaId);

            if (aluno == null)
                throw new Exception("Aluno não encontrado");

            _repo.Delete(aluno);
            _repo.Save();
        }

        public void AlterarStatus(int id, int academiaId, bool ativo)
        {
            var academia = _repo.GetById(id, academiaId);

            if (academia == null)
                throw new Exception("Aluno não encontrado");

            academia.Ativo = ativo;

            _repo.Update(academia);
            _repo.Save();
        }


    }
}