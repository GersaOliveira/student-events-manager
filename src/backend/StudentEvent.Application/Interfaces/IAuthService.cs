using StudentEvent.Application.DTOs;
using System.Threading.Tasks;

namespace StudentEvent.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> ForgotPasswordAsync(string email);

        Task<bool> ResetPasswordAsync(ResetPasswordDto dto);

        Task<string> GenerateResetTokenAsync(string email);

        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    }
}