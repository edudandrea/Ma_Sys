using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.Alunos;
using MA_SYS.Api.Data;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;
using SQLitePCL;

namespace MA_Sys.API.Services
{
    public class AlunoService
    {
        private readonly IAlunoRepository _repo;
        

        public AlunoService(IAlunoRepository repo)
        {
            _repo = repo;            
        }

        public List<AlunoResponseDto> Listar(int academiaId)
        {
           var alunos = _repo.GetByAcademia(academiaId);

           return alunos.Select(a => new AlunoResponseDto
           {
               Id = a.Id,
               Nome = a.Nome,
               CPF = a.CPF,
               Telefone = a.Telefone,
               Ativo = a.AlunoAtivo
           }).ToList();
        }

        public List<AlunoResponseDto> Buscar(AlunoFiltroDto filtro, int academiaId)
        {
            var query = _repo.Query();
            //query = query.Where(a => a.AcademiaId == academiaId);

            if (filtro.Id.HasValue)
                query = query.Where(a => a.Id == filtro.Id);

            if (!string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(a => a.Nome.Contains(filtro.Nome));

            if (!string.IsNullOrEmpty(filtro.CPF))
                query = query.Where(a => a.CPF.Contains(filtro.CPF));

            return query.Select(a => new AlunoResponseDto
            {
                Id = a.Id,
                Nome = a.Nome,
                CPF = a.CPF,
                Telefone = a.Telefone,
                Graduacao = a.Graduacao,
                Ativo = a.AlunoAtivo,

            }).ToList();
        }

        public AlunoDto ObterPorId(int id, int academiaId)
        {
            return _repo.Query()
            
                .Where(a => a.Id == id) 
                .Select(a => new AlunoDto
                {
                    Id = a.Id,
                    Nome = a.Nome,
                    CPF = a.CPF,
                    Telefone = a.Telefone,
                    Email = a.Email,
                    Endereco = a.Endereco,
                    Cidade = a.Cidade,
                    Estado = a.Estado,
                    CEP = a.CEP,
                    ModalidadeId = a.ModalidadeId,
                    Graduacao = a.Graduacao,
                    DataCadastro = a.DataCadastro,
                    DataNascimento = a.DataNascimento,
                    AlunoAtivo = a.AlunoAtivo
                })
                .FirstOrDefault();
        }

        public void Criar(AlunosCreateDto dto, int academiaId)
        {
            var aluno = new Aluno
            {
                Nome = dto.Nome,
                CPF = dto.CPF,
                ModalidadeId = dto.ModalidadeId,
                Telefone = dto.Telefone,
                Email = dto.Email,

                AcademiaId = academiaId,
                DataCadastro = DateTime.UtcNow,
                AlunoAtivo = true
            };

            _repo.Add(aluno);
            _repo.Save();
        }

        public void Atualizar(int id, AlunoUpdateDto dto, int academiaId)
        {
            var aluno = _repo.Query()
                        .FirstOrDefault(a => a.Id == id);

            if(aluno == null)
            throw new Exception("Aluno não encontrado");

            aluno.Nome = dto.Nome?.Trim();
            aluno.CPF = dto.CPF?.Trim();
            aluno.Telefone = dto.Telefone;
            aluno.Endereco = dto.Endereco;
            aluno.Cidade = dto.Cidade;
            aluno.Estado = dto.Estado;
            aluno.RedeSocial = dto.RedeSocial;
            aluno.Email = dto.Email;
            aluno.ModalidadeId = dto.ModalidadeId;
            aluno.Graduacao = dto.Graduacao;
            aluno.DataNascimento = dto.DataNascimento;

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
            var aluno = _repo.GetById(id, academiaId);

            if (aluno == null)
                throw new Exception("Aluno não encontrado");

            aluno.AlunoAtivo = ativo;

            _repo.Update(aluno);
            _repo.Save();
        }
    }
}