using CRMSystem.Application.DTOs.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CRMSystem.Application.Common.Interfaces;
using CRMSystem.Infrastructure; 





namespace CRMSystem.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;

    public UsersController(UserManager<ApplicationUser> userManager, IJwtTokenService jwtTokenService)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
    }

    [HttpPost("add-staff")]
    public async Task<IActionResult> AddStaff([FromBody] RegisterDto dto)
    {
        if (dto.Password != dto.ConfirmPassword)
            return BadRequest(new { Success = false, Message = "Şifreler eşleşmiyor" });

        // E-posta kontrolü
        var existingUser = await _userManager.FindByEmailAsync(dto.Email);
        if (existingUser != null)
            return BadRequest(new { Success = false, Message = "Bu e-posta adresi zaten kullanılıyor" });

        var user = new ApplicationUser
        {
            UserName = dto.Email,
            Email = dto.Email,
            FullName = dto.FullName
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
            return BadRequest(new { Success = false, Message = string.Join("; ", result.Errors.Select(e => e.Description)) });

        // Personel rolü ata
        await _userManager.AddToRoleAsync(user, "Staff");

        var roles = await _userManager.GetRolesAsync(user);
        var tokens = _jwtTokenService.GenerateTokens(user, roles);
        return Ok(new { Success = true, Data = tokens });
    }

    [HttpGet("staff")]
    public async Task<IActionResult> GetStaffUsers()
    {
        var staffUsers = await _userManager.GetUsersInRoleAsync("Staff");
      var staffList = staffUsers.Select(u => new
{
    Id = u.Id,
    Email = u.Email,
    FullName = u.FullName
    // CreatedAt alanı kaldırıldı
});


        return Ok(new { Success = true, Data = staffList });
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteUser(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user == null)
            return NotFound(new { Success = false, Message = "Kullanıcı bulunamadı" });

        // Admin kendini silemesin
        if (await _userManager.IsInRoleAsync(user, "Admin"))
            return BadRequest(new { Success = false, Message = "Admin kullanıcısı silinemez" });

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
            return BadRequest(new { Success = false, Message = string.Join("; ", result.Errors.Select(e => e.Description)) });

        return Ok(new { Success = true, Message = "Kullanıcı başarıyla silindi" });
    }
}
