// Program.cs  –  Iot2Project.API
// ============================================================
using System.Data;
using Dapper;
using Iot2Project.Application.Users.CreateUser;   // serviços Application
using Iot2Project.Application.Users.DeleteUser;
using Iot2Project.Application.Users.GetAllUsers;
using Iot2Project.Application.Users.GetUserById;
using Iot2Project.Application.Users.UpdateUser;
using Iot2Project.Domain.Ports;                  // interfaces de repositório
using Iot2Project.Infrastructure.Persistence.Context;
using Iot2Project.Infrastructure.Persistence.Repositories;
using Microsoft.OpenApi.Models;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

//-------------------------------------------------------------
// 0)  Porta padrão 5000 (útil para Docker ou execução local)
//-------------------------------------------------------------
//builder.WebHost.UseUrls("http://0.0.0.0:5000");

//-------------------------------------------------------------
// 1)  MVC Controllers + JSON
//-------------------------------------------------------------
builder.Services.AddControllers()
                .AddJsonOptions(o =>
                    o.JsonSerializerOptions.PropertyNamingPolicy = null); // CamelCase off (opcional)

//-------------------------------------------------------------
// 2)  Swagger / OpenAPI
//-------------------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    // doc padrão
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Iot2Project API", Version = "v1" });

    // inclui comentários XML
    var xmlPath = Path.Combine(AppContext.BaseDirectory, "Iot2Project.API.xml");
    c.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
});



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy =>
        {
            policy
              .AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
        });
});

//-------------------------------------------------------------
// 3)  PostgreSQL + Dapper (Connection Factory)
//-------------------------------------------------------------
builder.Services.AddSingleton<IDbConnectionFactory, PgsqlConnectionFactory>();
builder.Services.AddScoped<IDbConnection>(sp =>
    sp.GetRequiredService<IDbConnectionFactory>().CreateConnection());

//-------------------------------------------------------------
// 4)  Repositórios + Serviços de Aplicação
//-------------------------------------------------------------
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<CreateUserService>();
builder.Services.AddScoped<GetAllUsersService>();
builder.Services.AddScoped<GetUserByIdService>();
builder.Services.AddScoped<UpdateUserService>();
builder.Services.AddScoped<DeleteUserService>();


var app = builder.Build();

//-------------------------------------------------------------
// 5)  Middleware Pipeline
//-------------------------------------------------------------
//if (app.Environment.IsDevelopment())
//;{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();              // → http://localhost:5000/swagger
//}

app.UseRouting();
app.UseAuthorization();
app.MapControllers();                // ← habilita UsersController
app.UseCors("AllowAll");
app.Run();

//-------------------------------------------------------------
// 6)  PgsqlConnectionFactory (pode ir em Infrastructure)
//-------------------------------------------------------------
internal sealed class PgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connStr;

    public PgsqlConnectionFactory(IConfiguration cfg) =>
        _connStr = cfg.GetConnectionString("Default") ??
                   throw new InvalidOperationException("ConnectionStrings:Default não encontrado.");

    public IDbConnection CreateConnection() => new NpgsqlConnection(_connStr);
}
