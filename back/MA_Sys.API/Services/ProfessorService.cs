using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Dto.ProfessoresDto;
using MA_SYS.Api.Models;
using Microsoft.AspNetCore.Identity;
using SQLitePCL;

namespace MA_Sys.API.Services
{
    public class ProfessorService
    {
        private readonly IProfessorRepository _repo;

        public ProfessorService(IProfessorRepository repo)
        {
            _repo = repo;
        }

        public List<ProfessorResponseDto> List(int academiaId)
        {
            var prof = _repo.Query();
            prof = prof.Where(p => p.AcademiaId == academiaId);

            return prof.Select(p => new ProfessorResponseDto
            {
                Id = p.Id,
                AcademiaId = p.AcademiaId,
                Nome = p.Nome,
                Email = p.Email,
                Telefone = p.Telefone,
                Graduacao = p.Graduacao,
                ModalidadeId = p.ModalidadeId,
                Ativo = p.Ativo

            }).ToList();
        }

        public List<ProfessorResponseDto> Get(string role, ProfessorFiltroDto filtro, int? academiaId)
        {
            var query = _repo.Query();

            Console.WriteLine($"ROLE RAW: '{role}'");
            Console.WriteLine("ROLE NO SERVICE: '" + role + "'");

            var isAdmin = role?.Trim().ToLower() == "admin";

            if (!isAdmin && academiaId.HasValue)
            {
                query = query.Where(a => a.AcademiaId == academiaId);
            }

            if (filtro.Id.HasValue)
                query = query.Where(p => p.Id == filtro.Id);

            if (!string.IsNullOrEmpty(filtro.Nome))
                query = query.Where(p => p.Nome.Contains(filtro.Nome));

            if (!string.IsNullOrEmpty(filtro.Email))
                query = query.Where(p => p.Email.Contains(filtro.Email));

            return query.Select(p => new ProfessorResponseDto
            {
                Id = p.Id,
                AcademiaId = p.AcademiaId,
                ModalidadeId = p.ModalidadeId,
                Nome = p.Nome,
                Email = p.Email,
                Telefone = p.Telefone,
                Graduacao = p.Graduacao,
                Ativo = p.Ativo,
            }).ToList();
        }

        public void Add(ProfessorCreateDto dto, int? academiaId, string role)
        {
            int academiaFinal;

            if (role == "Admin")
            {
                if (dto.AcademiaId <= 0)
                    throw new Exception("Admin deve informar a academia");

                academiaFinal = dto.AcademiaId;
            }
            else
            {
                if (!academiaId.HasValue || academiaId <= 0)
                    throw new Exception("Usuário sem academia válida");

                academiaFinal = academiaId.Value;
            }

            var prof = new Professor
            {
                Nome = dto.Nome,
                Graduacao = dto.Graduacao,
                ModalidadeId = dto.ModalidadeId,
                Telefone = dto.Telefone,
                Email = dto.Email,
                AcademiaId = academiaFinal,
                Ativo = true
            };

            _repo.Add(prof);
            _repo.Save();
        }

        public void Update(int id, ProfessorUpdateDto dto)
        {
            var prof = _repo.Query()
                        .FirstOrDefault(a => a.Id == id);

            if (prof == null)
                throw new Exception("Professor não encontrado");

            prof.Nome = dto.Nome?.Trim();
            prof.Email = dto.Email?.Trim();
            prof.Telefone = dto.Telefone;
            prof.Email = dto.Email;
            prof.ModalidadeId = dto.ModalidadeId;
            prof.Graduacao = dto.Graduacao;

            _repo.Save();
        }


        public void Delete(int id, int academiaId)
        {
            var prof = _repo.GetById(id, academiaId);

            if (prof == null)
                throw new Exception("Professor não encontrado");

            _repo.Delete(prof);
            _repo.Save();
        }

        public void UpdateStatus(int id, int academiaId, bool ativo)
        {
            var prof = _repo.GetById(id, academiaId);

            if (prof == null)
                throw new Exception("Professor não encontrado");

            prof.Ativo = ativo;

            _repo.Update(prof);
            _repo.Save();
        }

    }
}