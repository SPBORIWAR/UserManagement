using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.EntityFrameworkCore.Models;
using UserManagement.EntityFrameworkCore.Repository;

namespace UserManagement.BusinessLogic.AuthManagement
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Tenant> _tenantRepo;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;

        public AuthService(
            IRepository<User> userRepo,
            IRepository<Role> roleRepo,
            IRepository<Tenant> tenantRepo,
            IPasswordHasher<User> passwordHasher,
            IConfiguration config)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _tenantRepo = tenantRepo;
            _passwordHasher = passwordHasher;
            _config = config;
        }

        public async Task RegisterAsync(RegisterUserDto dto)
        {
            var tenant = await _tenantRepo.GetByIdAsync(dto.TenantId)
                         ?? throw new InvalidOperationException("Tenant not found");

            var role = await _roleRepo.GetAll()
                .FirstOrDefaultAsync(r => r.Id == dto.RoleId && r.TenantId == dto.TenantId)
                ?? throw new InvalidOperationException("Role not found for tenant");

            var user = new User
            {
                UserName = dto.UserName,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                TenantId = dto.TenantId,
                RoleId = dto.RoleId
            };

            user.Password = _passwordHasher.HashPassword(user, dto.Password);

            await _userRepo.InsertAsync(user);
            await _userRepo.SaveChangesAsync();
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetAll()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.TenantId == dto.TenantId)
                ?? throw new InvalidOperationException("Invalid user or tenant");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, dto.Password);
            if (result != PasswordVerificationResult.Success)
                throw new InvalidOperationException("Invalid password");

            var accessToken = GenerateAccessToken(user);
            var refreshToken = GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpiryDays"));
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken)
        {
            var user = await _userRepo.GetAll()
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
                ?? throw new InvalidOperationException("Invalid refresh token");

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new InvalidOperationException("Refresh token expired");

            var newAccessToken = GenerateAccessToken(user);
            var newRefreshToken = GenerateRefreshToken();

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(_config.GetValue<int>("Jwt:RefreshTokenExpiryDays"));
            await _userRepo.SaveChangesAsync();

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken
            };
        }

        private string GenerateAccessToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),          // <- standard NameIdentifier
                new Claim(ClaimTypes.Name, user.UserName),                         // <- standard Name maps to unique_name
                new Claim("TenantId", user.TenantId.ToString()),                   // <- custom
                new Claim("UserId", user.Id.ToString()),                           // <- custom
                new Claim("RoleId", user.Role.Id.ToString()),                      // <- custom
                new Claim(ClaimTypes.Role, user.Role.Name)                         // <- standard Role
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(30),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }



        private string GenerateRefreshToken()
        {
            var randomBytes = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomBytes);
            return Convert.ToBase64String(randomBytes);
        }
    }


}
