using CRMSystem.Application.Common.Interfaces;
using CRMSystem.Application.DTOs.Auth;
using CRMSystem.Application.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;

namespace CRMSystem.Infrastructure.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly IConfiguration _configuration;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenService(IConfiguration configuration, UserManager<ApplicationUser> userManager)
        {
            _configuration = configuration;
            _userManager = userManager;
        }

        public AuthResponseDto GenerateTokens(ApplicationUser user, IList<string> roles)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var issuer = jwtSection["Issuer"]!;
            var audience = jwtSection["Audience"]!;
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));
            var accessMinutes = int.Parse(jwtSection["AccessTokenMinutes"]!);
            var refreshDays = int.Parse(jwtSection["RefreshTokenDays"]!);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim("fullName", user.FullName)
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var accessToken = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(accessMinutes),
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);
            
            // Daha güvenli refresh token oluşturma
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray()) + 
                             Convert.ToBase64String(BitConverter.GetBytes(DateTime.UtcNow.Ticks));

            return new AuthResponseDto
            {
                Token = tokenString,
                RefreshToken = refreshToken,
                ExpiresAt = accessToken.ValidTo,
                User = new UserDto
                {
                    Id = user.Id,
                    Email = user.Email ?? string.Empty,
                    FullName = user.FullName,
                    Roles = roles.ToList()
                }
            };
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            var jwtSection = _configuration.GetSection("Jwt");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"]!));

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = false,
                ValidateAudience = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = key,
                ValidateLifetime = false
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out var securityToken);
                if (securityToken is not JwtSecurityToken jwtToken || jwtToken.Header.Alg != SecurityAlgorithms.HmacSha256)
                    return null;

                return principal;
            }
            catch
            {
                return null;
            }
        }

        public async Task<Result<AuthResponseDto>> RefreshTokenAsync(string accessToken, string refreshToken)
        {
            try
            {
                var principal = GetPrincipalFromExpiredToken(accessToken);
                if (principal == null)
                    return Result<AuthResponseDto>.Failure("Geçersiz token");

                var userId = principal.FindFirst("sub")?.Value;
                if (string.IsNullOrEmpty(userId))
                    return Result<AuthResponseDto>.Failure("Geçersiz token");

                var user = await _userManager.FindByIdAsync(userId);
                if (user == null)
                    return Result<AuthResponseDto>.Failure("Kullanıcı bulunamadı");

                // Refresh token doğrulaması (basit implementasyon)
                // Gerçek uygulamada refresh token'ı veritabanında saklamalısınız
                var roles = await _userManager.GetRolesAsync(user);
                var newTokens = GenerateTokens(user, roles);

                return Result<AuthResponseDto>.Success(newTokens);
            }
            catch (Exception ex)
            {
                return Result<AuthResponseDto>.Failure($"Token yenileme hatası: {ex.Message}");
            }
        }
    }
}


