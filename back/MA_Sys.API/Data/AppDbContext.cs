using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;
using SQLitePCL;
using System.Security.Claims;


namespace MA_SYS.Api.Data
{
    public class AppDbContext : DbContext
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AppDbContext(DbContextOptions<AppDbContext> options,
                            IHttpContextAccessor httpContextAccessor) : base(options)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public DbSet<Users> User { get; set; }
        public DbSet<Aluno> Alunos { get; set; }
        public DbSet<Plano> Planos { get; set; }
        public DbSet<Aula> Aulas { get; set; }
        public DbSet<Mensalidade> Mensalidades { get; set; }
        public DbSet<Professor> Professores { get; set; }
        public DbSet<Academia> Academias { get; set; }
        public DbSet<Modalidade> Modalidades { get; internal set; }

        private int GetAcademiaId()
        {
            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst("AcademiaID");
            return claim != null ? int.Parse(claim.Value) : -1;
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Aluno>()
                        .HasQueryFilter(a => a.AcademiaId == GetAcademiaId() || a.AcademiaId == 0);

            modelBuilder.Entity<Professor>()
                        .HasQueryFilter(p => p.AcademiaId == GetAcademiaId());

            modelBuilder.Entity<Modalidade>()
                        .HasQueryFilter(m => m.AcademiaId == GetAcademiaId());

        }
    }
}