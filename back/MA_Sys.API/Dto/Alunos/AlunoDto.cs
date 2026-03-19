using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;

namespace MA_SYS.Api.Dto
{
    public class AlunoDto
    {
        public int Id { get; set; }
        public int AcademiaId { get; set; }
        public string? Nome { get; set; }
        public string? CPF { get; set; }
        public string? Endereco { get; set; }
        public string? Bairro { get; set; }
        public string? Cidade { get; set; }
        public string? Estado { get; set; }
        public string? CEP { get; set; }
        public string? Telefone { get; set; }
        public string? Email { get; set; }
        public string? RedeSocial { get; set; }
        public int Sexo { get; set; }
        public int ModalidadeId { get; set; }
        public string? Graduacao { get; set; }
        public DateTime DataNascimento { get; set; }
        public DateTime DataCadastro { get; set; }
        public bool AlunoAtivo { get; set; }
    }
}