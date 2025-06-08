namespace Iot2Project.Application.Users.GetAllUsers;

/// <summary>
/// Representa um usuário retornado pela rota GET /users.
/// Não expõe informações sensíveis (hash da senha, flags internas etc.).
/// </summary>
public record UserDto(
    int UserId,
    string FullName,
    string Email,
    int UserProfileId,
    DateTime CreatedAt,
    DateTime UpdatedAt, 
    bool IsDeleted);

