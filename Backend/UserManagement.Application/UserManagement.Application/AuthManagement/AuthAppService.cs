using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using UserManagement.BusinessLogic.AuthManagement;
using UserManagement.BusinessLogic.Dtos;

namespace UserManagement.Application.AuthManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthAppService : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthAppService(IAuthService authService)
        {
            _authService = authService;
        }

        /// <summary>
        /// Register new user.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDto dto)
        {
            await _authService.RegisterAsync(dto);
            return Ok("User registered successfully.");
        }

        /// <summary>
        /// Login and get access + refresh token.
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);
            return Ok(result);
        }

        /// <summary>
        /// Refresh access token using refresh token.
        /// </summary>
        [HttpPost("refresh-token")]
        public async Task<ActionResult<LoginResponseDto>> RefreshToken(string refreshToken)
        {
            var result = await _authService.RefreshTokenAsync(refreshToken);
            return Ok(result);
        }

        // POST api/auth/reset-password
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto dto)
        {
            await _authService.ResetPasswordAsync(dto.UserId, dto.NewPassword);
            return Ok("Password reset successfully.");
        }

        // POST api/auth/social-login
        [HttpPost("social-login")]
        public async Task<ActionResult<LoginResponseDto>> SocialLogin(SocialLoginDto dto)
        {
            var result = await _authService.SocialLoginAsync(dto);
            return Ok(result);
        }

        [Authorize]
        [HttpPost("update-profile")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            await _authService.UpdateProfileAsync(dto);
            return Ok(new { message = "Profile updated successfully." });
        }

        [Authorize]
        [HttpGet("profile")]
        public async Task<ActionResult<UserProfileDto>> GetProfile()
        {
            var profile = await _authService.GetProfileAsync();
            return Ok(profile);
        }



    }
}