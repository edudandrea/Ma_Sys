using MA_Sys.API.Models;
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
        public DbSet<Pagamentos> Pagamentos { get; set; }
        public DbSet<Professor> Professores { get; set; }
        public DbSet<Academia> Academias { get; set; }
        public DbSet<Modalidade> Modalidades { get; internal set; }
        public DbSet<FormaPagamento> FormaPagamentos { get; set; }
        public DbSet<Matricula> Matriculas { get; set; }
        public DbSet<PagamentoAcademia> PagamentosAcademias { get; set; }
        public DbSet<Financeiro> Financeiros { get; set; }
        public DbSet<CategoriaTransacao> CategoriasTransacao { get; set; }
        public DbSet<MensalidadeSistema> MensalidadesSistema { get; set; }
        public DbSet<Turma> Turmas { get; set; }
        public DbSet<TurmaAluno> TurmasAlunos { get; set; }
        public DbSet<Exercicio> Exercicios { get; set; }
        public DbSet<Treino> Treinos { get; set; }
        public DbSet<TreinoExercicio> TreinosExercicios { get; set; }
        public DbSet<Filiados> Filiado { get; set; }
        public DbSet<PagamentoFiliado> PagamentosFiliados { get; set; }
        public DbSet<Federacao> Federacoes { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.CreatedByUser)
                .WithMany(u => u.CreatedUsers)
                .HasForeignKey(u => u.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Users>()
                .HasOne(u => u.Federacao)
                .WithMany()
                .HasForeignKey(u => u.FederacaoId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Academia>()
                .HasOne(a => a.OwnerUser)
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MensalidadeSistema>()
                .HasOne(m => m.OwnerUser)
                .WithMany()
                .HasForeignKey(m => m.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagamentoAcademia>()
                .HasOne(p => p.MensalidadeSistema)
                .WithMany()
                .HasForeignKey(p => p.MensalidadeSistemaId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TurmaAluno>()
                .HasOne(ta => ta.Turma)
                .WithMany(t => t.Alunos)
                .HasForeignKey(ta => ta.TurmaId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Turma>()
                .HasOne(t => t.Professor)
                .WithMany()
                .HasForeignKey(t => t.ProfessorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Treino>()
                .HasOne(t => t.Professor)
                .WithMany()
                .HasForeignKey(t => t.ProfessorId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<TurmaAluno>()
                .HasOne(ta => ta.Aluno)
                .WithMany()
                .HasForeignKey(ta => ta.AlunoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TreinoExercicio>()
                .HasOne(te => te.Treino)
                .WithMany(t => t.Exercicios)
                .HasForeignKey(te => te.TreinoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<TreinoExercicio>()
                .HasOne(te => te.Exercicio)
                .WithMany()
                .HasForeignKey(te => te.ExercicioId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Filiados>()
                .HasOne(a => a.OwnerUser)
                .WithMany()
                .HasForeignKey(a => a.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<PagamentoFiliado>()
                .HasOne(p => p.Filiado)
                .WithMany()
                .HasForeignKey(p => p.FiliadoId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Federacao>()
                .HasOne(f => f.OwnerUser)
                .WithMany()
                .HasForeignKey(f => f.OwnerUserId)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
