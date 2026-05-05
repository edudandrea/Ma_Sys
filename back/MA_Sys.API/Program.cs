using MA_Sys.API.Data.Repository;
using MA_Sys.API.Data.Repository.interfaces;
using MA_Sys.API.Services;
using MA_SYS.Api.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using MA_Sys.API.Data.interfaces;

var builder = WebApplication.CreateBuilder(args);

var jwtKey = builder.Configuration["Jwt:Key"];
var jwtPlaceholder = string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Contains("__CONFIGURE_VIA_", StringComparison.Ordinal);

if (jwtPlaceholder && builder.Environment.IsDevelopment())
{
    jwtKey = "dev-only-jwt-key-marcia-prox-local-2026";
    Console.WriteLine("Jwt:Key ausente em Development. Usando chave temporaria apenas para ambiente local.");
}

if (string.IsNullOrWhiteSpace(jwtKey) || jwtKey.Contains("__CONFIGURE_VIA_", StringComparison.Ordinal))
{
    throw new InvalidOperationException("Jwt:Key nao configurada. Defina a chave real via variavel de ambiente Jwt__Key.");
}

var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];
var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? ["http://localhost:4200"];
var configuredDatabasePath = builder.Configuration["DATABASE_PATH"];
var defaultConnection = builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=martialartssys.db";
var connectionString = defaultConnection;

if (!string.IsNullOrWhiteSpace(configuredDatabasePath))
{
    var databaseDirectory = Path.GetDirectoryName(configuredDatabasePath);
    if (!string.IsNullOrWhiteSpace(databaseDirectory))
    {
        Directory.CreateDirectory(databaseDirectory);
    }

    connectionString = $"Data Source={configuredDatabasePath}";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(connectionString));

var key = Encoding.ASCII.GetBytes(jwtKey);
builder.Services.AddAuthentication(opt =>
    {
        opt.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        opt.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(opt =>
    {
        opt.RequireHttpsMetadata = false;
        opt.SaveToken = true;
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = !string.IsNullOrWhiteSpace(jwtIssuer),
            ValidIssuer = jwtIssuer,
            ValidateAudience = !string.IsNullOrWhiteSpace(jwtAudience),
            ValidAudience = jwtAudience,

            RoleClaimType = "role",
            NameClaimType = ClaimTypes.Name
        };
    }
);

builder.Services.AddAuthorization();

// BASE REPOSITORY
builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

//REPOSITORIES
builder.Services.AddScoped<IAlunoRepository, AlunoRepository>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IAcademiaRepository, AcademiaRepository>();
builder.Services.AddScoped<IModalidadeRepository, ModalidadeRepository>();
builder.Services.AddScoped<IProfessorRepository, ProfessorRepository>();
builder.Services.AddScoped<IPlanosRepository, PlanosRepository>();
builder.Services.AddScoped<IPagamentoRepository, PagamentosRepository>();
builder.Services.AddScoped<IPagamentoAcademiaRepository, PagamentoAcademiaRepository>();
builder.Services.AddScoped<IFinanceiroRepository, FinanceiroRepository>();
builder.Services.AddScoped<IFormaPagamentoRepository, FormaPagamentoRepository>();
builder.Services.AddScoped<IMatriculaRepository, MatriculaRepository>();
builder.Services.AddScoped<IMensalidadeSistemaRepository, MensalidadeSistemaRepository>();
builder.Services.AddScoped<IFiliadosRepository, FiliadosRepository>();

// SERVICES
builder.Services.AddScoped<AlunoService>();
builder.Services.AddScoped<TokenService>();
builder.Services.AddScoped<AcademiaService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<ModalidadeService>();
builder.Services.AddScoped<AdminService>();
builder.Services.AddScoped<ProfessorService>();
builder.Services.AddScoped<DashboardService>();
builder.Services.AddScoped<PlanosService>();
builder.Services.AddScoped<PagamentoService>();
builder.Services.AddScoped<PagamentoAcademiaService>();
builder.Services.AddScoped<FormaPagamentoService>();
builder.Services.AddScoped<MatriculaService>();
builder.Services.AddScoped<FluxoCaixaService>();
builder.Services.AddScoped<MensalidadeStatusService>();
builder.Services.AddScoped<MensalidadeSistemaService>();
builder.Services.AddScoped<TurmaService>();
builder.Services.AddScoped<ExercicioService>();
builder.Services.AddScoped<TreinoService>();
builder.Services.AddHttpClient<MercadoPagoGatewayService>();
builder.Services.AddScoped<FiliadosService>();
builder.Services.AddScoped<PagamentoFiliadoService>();
builder.Services.AddScoped<RelatorioService>();
builder.Services.AddScoped<FederacaoService>();


builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

//SWAGGER
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MA_Sys API",
        Version = "v1"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "Digite: Bearer {Token}",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            }, new string [] {}
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("frontend",
        policy =>
        {
            policy.WithOrigins(corsOrigins)
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.Migrate();
}

app.UseCors("frontend");
app.UseStaticFiles();

app.UseSwagger();

app.UseSwaggerUI();

app.UseAuthentication();

app.UseAuthorization();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();
