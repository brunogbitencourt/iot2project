using Iot2Project.Application.Users.GetAllUsers;
using Iot2Project.Domain.Ports;

public sealed class GetAllUsersService
{
    private readonly IUserRepository _repo;

    public GetAllUsersService(IUserRepository repo) => _repo = repo;

    public async Task<IEnumerable<UserDto>> ExecuteAsync(CancellationToken ct = default)
    {
        var users = await _repo.GetAllAsync(ct);

        return users.Select(u => new UserDto(
            u.UserId,
            u.FullName,
            u.Email,
            u.UserProfileId,
            u.CreatedAt,
            u.UpdatedAt, 
            u.IsDeleted));
    }
}
