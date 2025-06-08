using Iot2Project.Domain.Entities;

public interface IUserRepository
{
    Task<User> CreateAsync(User user, CancellationToken ct = default);
    Task<IEnumerable<User>> GetAllAsync(CancellationToken ct = default);

    Task<User?> GetByIdAsync(int id, CancellationToken ct = default);

    Task<User?> UpdateAsync(int id, User user, CancellationToken ct = default);

    Task<bool> DeleteAsync(int id, CancellationToken ct = default);

    // … outros métodos
}
