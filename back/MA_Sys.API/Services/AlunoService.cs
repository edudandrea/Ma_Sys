using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_SYS.Api.Dto;
using MA_SYS.Api.Models;

namespace MA_Sys.API.Services
{
    public class AlunoService
    {
        private readonly IAlunoRepository _repo;

        public AlunoService(IAlunoRepository repo)
        {
            _repo = repo;
        }

        public List<AlunoDto> GetAlunos(int academiaId, AlunoDto dto)
        {
            var alunos = _repo.GetByAcademia(academiaId, dto);

            return alunos.Select(a => new AlunoDto
            {
                Id = a.Id,
                Nome = a.Nome,
                CPF = a.CPF,
                Telefone = a.Telefone,
                AlunoAtivo = a.AlunoAtivo

            }).ToList();
        }

        public Aluno Obter(int id, int academiaId)
        {
            return _repo.GetById(id, academiaId);
        }

        public void Criar(AlunoDto dto, int academiaId)
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

        public void Atualizar(Aluno aluno)
        {
            _repo.Update(aluno);
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