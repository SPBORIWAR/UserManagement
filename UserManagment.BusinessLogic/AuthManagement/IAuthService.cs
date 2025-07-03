using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BusinessLogic.Dtos;

namespace UserManagement.BusinessLogic.AuthManagement
{
    public interface IAuthService
    {
        Task RegisterAsync(RegisterUserDto dto);
        Task<LoginResponseDto> LoginAsync(LoginDto dto);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken);
        Task ResetPasswordAsync(long userId, string newPassword);
        Task<LoginResponseDto> SocialLoginAsync(SocialLoginDto dto);
        Task UpdateProfileAsync(UpdateProfileDto dto);
        Task<UserProfileDto> GetProfileAsync();
    }

}
