using Iot2Project.Domain.Ports;

namespace Iot2Project.Application.Users.DeleteUser;

public sealed class DeleteUserService
{
    private readonly IUserRepository _repo;
    public DeleteUserService(IUserRepository repo) => _repo = repo;

    public Task<bool> ExecuteAsync(int id, CancellationToken ct = default)
        => _repo.DeleteAsync(id, ct);   // devolve true ou false
}
