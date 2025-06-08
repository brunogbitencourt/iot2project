using Iot2Project.Domain.Ports;
using Iot2Project.Application.Users.GetAllUsers;   // reaproveita UserDto

namespace Iot2Project.Application.Users.UpdateUser;

public sealed class UpdateUserService
{
    private readonly IUserRepository _repo;
    public UpdateUserService(IUserRepository repo) => _repo = repo;

    public async Task<UserDto?> ExecuteAsync(
        int id,
        UpdateUserRequest patch,
        CancellationToken ct = default)
    {
        var user = await _repo.GetByIdAsync(id, ct);
        if (user is null) return null;                       // 404

        // aplica só os campos enviados
        if (!string.IsNullOrWhiteSpace(patch.FullName))
            user.FullName = patch.FullName.Trim();

        if (!string.IsNullOrWhiteSpace(patch.Email))
            user.Email = patch.Email.ToLowerInvariant();

        if (!string.IsNullOrWhiteSpace(patch.Password))
            user.PasswordHash = patch.Password;

        if (patch.UserProfileId is not null)
            user.UserProfileId = patch.UserProfileId.Value;


        user.UpdatedAt = DateTime.UtcNow;

        var saved = await _repo.UpdateAsync(id, user, ct);
        if (saved is null) return null;                      // não deveria ocorrer

        return new UserDto(
            saved.UserId, saved.FullName, saved.Email,
            saved.UserProfileId, saved.CreatedAt,
            saved.UpdatedAt, saved.IsDeleted);
    }

}
