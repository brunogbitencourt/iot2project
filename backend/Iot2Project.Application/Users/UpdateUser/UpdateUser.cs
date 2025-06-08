using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Iot2Project.Application.Users.UpdateUser;

public record UpdateUserRequest(
    //[StringLength(120)]
    string? FullName,

    //[EmailAddress]
    string? Email,

    //[MinLength(8)]
    string? Password,

    //[Range(1, int.MaxValue)]
    int? UserProfileId);
