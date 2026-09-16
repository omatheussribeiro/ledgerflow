using LedgerFlow.Application.Dtos.Auth;

namespace LedgerFlow.Application.Interfaces.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken);
    Task<AuthResponseDto> RefreshAsync(RefreshRequestDto request, CancellationToken cancellationToken);
    Task LogoutAsync(RefreshRequestDto request, CancellationToken cancellationToken);
}
