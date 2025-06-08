using System.ComponentModel.DataAnnotations;

namespace Iot2Project.Application.Users.CreateUser;

public record CreateUserRequest(
    [Required, StringLength(120)]
    string FullName,

    [Required, EmailAddress]
    string Email,

    [Required, MinLength(8)]
    string Password,

    [Range(1, int.MaxValue)]
    int UserProfileId);
