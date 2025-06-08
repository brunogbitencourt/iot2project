// Iot2Project.Application/Users/CreateUser/CreateUserService.cs
using Iot2Project.Application.Users.GetAllUsers;
using Iot2Project.Domain.Entities;
using Iot2Project.Domain.Ports;
using System.Security.Cryptography;
using System.Text;
using System.Threading;

namespace Iot2Project.Application.Users.CreateUser;

public sealed class CreateUserService
{
    private readonly IUserRepository _repo;

    public CreateUserService(IUserRepository repo) => _repo = repo;

    public async Task<UserDto> ExecuteAsync(
        CreateUserRequest cmd,
        CancellationToken ct = default)
    {
        // (1) Regras simples de domínio / preparo
        var now = DateTime.UtcNow;

        var user = new User
        {
            FullName      = cmd.FullName.Trim(),
            Email         = cmd.Email.ToLowerInvariant(),
            PasswordHash  = HashPassword(cmd.Password),
            UserProfileId = cmd.UserProfileId,
            IsDeleted     = false,
            CreatedAt     = now,
            UpdatedAt     = now
        };

        // (2) Persistência
        var userPersistido = await _repo.CreateAsync(user, ct);

        // CreateUserService – depois de gravar no repositório
        //var persisted = await _repo.CreateAsync(user, ct);

        var dto = new UserDto(
            userPersistido.UserId,
            userPersistido.FullName,
            userPersistido.Email,
            userPersistido.UserProfileId,
            userPersistido.CreatedAt,
            userPersistido.UpdatedAt,
            userPersistido.IsDeleted);

        return dto;
    }

    // Helper bem simples (usar BCrypt ou ASP.NET Identity em produção)
    private static string HashPassword(string plain)
    {
        using var sha = SHA256.Create();
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(plain));
        return Convert.ToHexString(bytes);
    }
}
