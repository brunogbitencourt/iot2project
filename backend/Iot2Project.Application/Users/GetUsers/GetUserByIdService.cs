using Iot2Project.Domain.Ports;
using Iot2Project.Application.Users.GetAllUsers; // reaproveita UserDto

namespace Iot2Project.Application.Users.GetUserById;

public sealed class GetUserByIdService
{
    private readonly IUserRepository _repo;

    public GetUserByIdService(IUserRepository repo) => _repo = repo;

    public async Task<UserDto?> ExecuteAsync(int id, CancellationToken ct = default)
    {
        var u = await _repo.GetByIdAsync(id, ct);
        return u is null ? null
                         : new UserDto(u.UserId, u.FullName, u.Email,
                                       u.UserProfileId, u.CreatedAt, u.UpdatedAt, u.IsDeleted);
    }
}
