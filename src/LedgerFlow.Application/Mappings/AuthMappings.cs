using LedgerFlow.Application.Dtos.Auth;
using LedgerFlow.Domain.Identity;

namespace LedgerFlow.Application.Mappings;

public static class AuthMappings
{
    public static UserResponseDto ToDto(this User user) =>
        new(user.Id, user.Name, user.Email, user.Role);
}
