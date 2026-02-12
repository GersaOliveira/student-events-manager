using Hangfire;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using StudentEvent.Application.Interfaces;
using StudentEvent.Application.Services;
using StudentEvent.Domain.Interfaces;
using StudentEvent.Infra.Context;
using StudentEvent.Infra.Repositories;
using System.Net;
using System.Text;
using StudentEvent.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// Adiciona suporte aos Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options => 
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);

// Registrar o Repositório de Usuário
builder.Services.AddScoped<IUserRepository, UserRepository>();

// Registrar o Serviço de Autenticação
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<ISyncService, SyncService>();
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

// Configura o Swagger -
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "StudentEvent API", Version = "v1" });

    // Configura o botão "Authorize"
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header usando o esquema Bearer. Exemplo: \"Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
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
            },
            new string[] {}
        }
    });
});
//configuração de DB
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Registra o HttpClient para o serviço de integração

builder.Services.AddHttpClient<IMicrosoftGraphService, MicrosoftGraphService>(client =>
{
    client.Timeout = TimeSpan.FromSeconds(120);
})
.ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
{
    //força o .NET a usar exatamente a mesma configuração que o Firefox/Chrome
    Proxy = WebRequest.GetSystemWebProxy(),
    UseProxy = true,
    DefaultProxyCredentials = CredentialCache.DefaultCredentials,
    // Mata a validação de SSL apenas para teste pois não estava conseguindo pegar os dados da Microsoft.
    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
});

var key = Encoding.ASCII.GetBytes(builder.Configuration["Jwt:Key"]!);

builder.Services.AddAuthentication(x => {
    x.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    x.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(x => {
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"]
    };
});

// Configurar o Hangfire com o SQL Server
builder.Services.AddHangfire(config => config
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adicionar o servidor do Hangfire como um serviço em segundo plano
builder.Services.AddHangfireServer();

builder.Services.AddCors(options => {
    options.AddPolicy("DesafioPolicy", policy => {
        policy.WithOrigins(builder.Configuration["FrontendUrl"]!) 
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});


var app = builder.Build();

//Cria o Banco de Dados e Aplica Migrations
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.Migrate(); // Cria o banco se não existir e aplica as tabelas

    if (!db.Users.Any(u => u.Email == "admin@teste.com"))
    {
        db.Users.Add(new User { Id = Guid.NewGuid(), Email = "admin@teste.com", PasswordHash = "123456" });
        db.SaveChanges();
    }
}

// Ativa o Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("DesafioPolicy");

app.UseAuthentication();
app.UseAuthorization();

//Ativa o Dashboard do Hangfire
app.UseHangfireDashboard();

app.MapControllers();

//Configura o Job Recorrente do Desafio
RecurringJob.AddOrUpdate<ISyncService>("sync-graph-data", service => service.SyncStudentsAndEventsAsync(), Cron.MinuteInterval(5));

app.Run();