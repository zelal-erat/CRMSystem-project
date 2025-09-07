using CRMSystem.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CRMSystem.Application.Common.Interfaces;
using CRMSystem.Infrastructure;

namespace CRMSystem.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _jwtTokenService = jwtTokenService;
    }

    // ✅ Kullanıcı girişi
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.Email);
        if (user == null)
            return BadRequest(new { Success = false, Message = "E-posta veya şifre hatalı" });

        var result = await _signInManager.CheckPasswordSignInAsync(user, dto.Password, false);
        if (!result.Succeeded)
            return BadRequest(new { Success = false, Message = "E-posta veya şifre hatalı" });

        var roles = await _userManager.GetRolesAsync(user);
        var tokens = _jwtTokenService.GenerateTokens(user, roles);

        var userDto = new UserDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Roles = roles.ToList()
        };

        var response = new AuthResponseDto
        {
            Token = tokens.Token,
            RefreshToken = tokens.RefreshToken,
            ExpiresAt = tokens.ExpiresAt,
            User = userDto
        };

        return Ok(new { Success = true, Data = response });
    }


    // ✅ Token yenileme
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
    {
        var result = await _jwtTokenService.RefreshTokenAsync(dto.Token, dto.RefreshToken);
        
        if (!result.IsSuccess)
            return BadRequest(new { Success = false, Message = result.Error });

        return Ok(new { Success = true, Data = result.Data });
    }


    // ✅ Çıkış yapma
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { Success = true, Message = "Başarıyla çıkış yapıldı" });
    }

}
