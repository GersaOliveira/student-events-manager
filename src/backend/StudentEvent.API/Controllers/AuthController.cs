using Microsoft.AspNetCore.Mvc;
using StudentEvent.Application.DTOs;
using StudentEvent.Application.Interfaces;
using System.Threading.Tasks;

namespace StudentEvent.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] string email)
        {
            await _authService.GenerateResetTokenAsync(email);
            return Ok();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            var result = await _authService.ResetPasswordAsync(dto);
            if (!result) return BadRequest(new { message = "Token inválido ou expirado." });

            return Ok(new { message = "Senha alterada com sucesso!" });
        }

        [HttpPost("login")]
public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
{
    var token = await _authService.LoginAsync(request);
    
    if (token == null) return Unauthorized("E-mail ou senha inválidos");

    return Ok(new { token });
}
    }
}