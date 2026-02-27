using ZuriFluxAPI.DTOs;

namespace ZuriFluxAPI.Services
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterDto dto);
        Task<AuthResponseDto> LoginAsync(LoginDto dto);
        Task<UserResponseDto> GetUserByIdAsync(int id);
    }
}
