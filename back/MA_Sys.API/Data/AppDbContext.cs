using MA_SYS.Api.Models;
using Microsoft.EntityFrameworkCore;


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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);                                 

        }
    }
}