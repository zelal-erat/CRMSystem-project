using CRMSystem.Application.DTOs.Auth;
using CRMSystem.Infrastructure;
using System.Security.Claims;

namespace CRMSystem.Application.Common.Interfaces
{
    public interface IJwtTokenService
    {
        AuthResponseDto GenerateTokens(ApplicationUser user, IList<string> roles);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task<Result<AuthResponseDto>> RefreshTokenAsync(string accessToken, string refreshToken);
    }
}


