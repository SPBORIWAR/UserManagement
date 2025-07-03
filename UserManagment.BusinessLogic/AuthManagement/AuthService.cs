using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using UserManagement.BusinessLogic.Dtos;
using UserManagement.BusinessLogic.SessionManagment;
using UserManagement.EntityFrameworkCore.Models;
using UserManagement.EntityFrameworkCore.Repository;

namespace UserManagement.BusinessLogic.AuthManagement
{
    public class AuthService : IAuthService
    {
        private readonly IRepository<User> _userRepo;
        private readonly IRepository<Role> _roleRepo;
        private readonly IRepository<Tenant> _tenantRepo;
        private readonly IRepository<UserRole> _userRoleRepo;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly IConfiguration _config;
        private readonly ISessionService _sessionService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        public AuthService(
            IRepository<User> userRepo,
            IRepository<Role> roleRepo,
            IRepository<Tenant> tenantRepo,
            IRepository<UserRole> userRoleRepo,
            IPasswordHasher<User> passwordHasher,
            IConfiguration config,
            ISessionService sessionService,
            IHttpContextAccessor httpContextAccessor)
        {
            _userRepo = userRepo;
            _roleRepo = roleRepo;
            _tenantRepo = tenantRepo;
            _userRoleRepo = userRoleRepo;
            _passwordHasher = passwordHasher;
            _config = config;
            _sessionService = sessionService;
            _httpContextAccessor = httpContextAccessor;
        }

        // ✅ User Registration with duplicate checks
        public async Task RegisterAsync(RegisterUserDto dto)
        {
            var tenant = await _tenantRepo.GetByIdAsync(dto.TenantId)
                         ?? throw new InvalidOperationException("Tenant not found");

            // Duplicate checks
            if (await _userRepo.AnyAsync(u => u.UserName == dto.UserName))
                throw new InvalidOperationException("Username already exists.");
            if (!string.IsNullOrEmpty(dto.EmailAddress) && await _userRepo.AnyAsync(u => u.EmailAddress == dto.EmailAddress))
                throw new InvalidOperationException("Email already exists.");
            if (!string.IsNullOrEmpty(dto.PhoneNo) && await _userRepo.AnyAsync(u => u.PhoneNo == dto.PhoneNo))
                throw new InvalidOperationException("Phone number already exists.");

            var role = await _roleRepo.GetAll()
                .FirstOrDefaultAsync(r => r.Id == dto.RoleId && r.TenantId == dto.TenantId)
                ?? throw new InvalidOperationException("Role not found for tenant");

            var user = new User
            {
                UserName = dto.UserName,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                EmailAddress = dto.EmailAddress,
                PhoneNo = dto.PhoneNo,
                TenantId = dto.TenantId
            };

            user.Password = _passwordHasher.HashPassword(user, dto.Password);

            await _userRepo.InsertAsync(user);
            await _userRepo.SaveChangesAsync();

            var userRole = new UserRole
            {
                UserId = user.Id,
                RoleId = dto.RoleId,
                TenantId = dto.TenantId
            };

            await _userRoleRepo.InsertAsync(userRole);
            await _userRoleRepo.SaveChangesAsync();
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepo.GetAll()
                .FirstOrDefaultAsync(u => u.UserName == dto.UserName && u.TenantId == dto.TenantId)
                ?? throw new InvalidOperationException("Invalid user or tenant");

            var result = _passwordHasher.VerifyHashedPassword(user, user.Password, dto.Password);
            if (result != PasswordVerificationResult.Success)
                throw new InvalidOperationException("Invalid password");

            var accessToken = await GenerateAccessToken(user);
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
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken)
                ?? throw new InvalidOperationException("Invalid refresh token");

            if (user.RefreshTokenExpiryTime < DateTime.UtcNow)
                throw new InvalidOperationException("Refresh token expired");

            var newAccessToken = await GenerateAccessToken(user);
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

        // ✅ Password Reset
        public async Task ResetPasswordAsync(long userId, string newPassword)
        {
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found");

            user.Password = _passwordHasher.HashPassword(user, newPassword);
            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();
        }

        // ✅ Social login configuration (stub for external login integration)
        public async Task<LoginResponseDto> SocialLoginAsync(SocialLoginDto dto)
        {
            var user = await _userRepo.GetAll()
                .FirstOrDefaultAsync(u => u.EmailAddress == dto.EmailAddress && u.TenantId == dto.TenantId);

            if (user == null)
            {
                // Create new user for social login
                user = new User
                {
                    UserName = dto.EmailAddress.Split('@')[0],
                    EmailAddress = dto.EmailAddress,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    TenantId = dto.TenantId
                };
                await _userRepo.InsertAsync(user);
                await _userRepo.SaveChangesAsync();
            }

            var accessToken = await GenerateAccessToken(user);
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

        public async Task UpdateProfileAsync(UpdateProfileDto dto)
        {
            var userId = _sessionService.UserId;
            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found");

            // Email duplication check
            if (!string.IsNullOrWhiteSpace(dto.EmailAddress))
            {
                if (await _userRepo.AnyAsync(u => u.EmailAddress == dto.EmailAddress && u.Id != userId && u.TenantId == user.TenantId))
                    throw new InvalidOperationException("Email address already in use.");
            }

            // Phone number duplication check
            if (!string.IsNullOrWhiteSpace(dto.PhoneNo))
            {
                if (await _userRepo.AnyAsync(u => u.PhoneNo == dto.PhoneNo && u.Id != userId && u.TenantId == user.TenantId))
                    throw new InvalidOperationException("Phone number already in use.");
            }

            // Update only provided fields
            user.FirstName = dto.FirstName ?? user.FirstName;
            user.LastName = dto.LastName ?? user.LastName;
            user.EmailAddress = dto.EmailAddress ?? user.EmailAddress;
            user.PhoneNo = dto.PhoneNo ?? user.PhoneNo;
            user.Gender = dto.Gender ?? user.Gender;
            user.DateOfBirth = dto.DateOfBirth ?? user.DateOfBirth;
            user.Address = dto.Address ?? user.Address;
            user.City = dto.City ?? user.City;
            user.Country = dto.Country ?? user.Country;

            // Save profile picture if uploaded
            if (dto.ProfilePicture != null && dto.ProfilePicture.Length > 0)
            {
                var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "profile-pictures");
                if (!Directory.Exists(uploadsFolder))
                    Directory.CreateDirectory(uploadsFolder);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(dto.ProfilePicture.FileName)}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.ProfilePicture.CopyToAsync(stream);
                }

                user.ProfilePicturePath = $"/profile-pictures/{fileName}";
            }

            await _userRepo.UpdateAsync(user);
            await _userRepo.SaveChangesAsync();
        }

        public async Task<UserProfileDto> GetProfileAsync()
        {
            var userId = _sessionService.UserId;

            var user = await _userRepo.GetByIdAsync(userId)
                       ?? throw new InvalidOperationException("User not found");

            var baseUrl = $"{_httpContextAccessor.HttpContext?.Request.Scheme}://{_httpContextAccessor.HttpContext?.Request.Host}";

            return new UserProfileDto
            {
                UserName = user.UserName,
                FirstName = user.FirstName,
                LastName = user.LastName,
                EmailAddress = user.EmailAddress,
                PhoneNo = user.PhoneNo,
                Gender = user.Gender,
                DateOfBirth = user.DateOfBirth,
                Address = user.Address,
                City = user.City,
                Country = user.Country,
                ProfilePictureUrl = string.IsNullOrEmpty(user.ProfilePicturePath)
                    ? null
                    : $"{baseUrl}{user.ProfilePicturePath}"
            };
        }


        private async Task<string> GenerateAccessToken(User user)
        {
            var key = Encoding.ASCII.GetBytes(_config["Jwt:Key"]);
            var tokenHandler = new JwtSecurityTokenHandler();

            // Fetch user's roles via UserRole mapping
            var roleMappings = await _userRoleRepo.GetAllIncluding(ur => ur.Role)
                .Where(ur => ur.UserId == user.Id && ur.TenantId == user.TenantId)
                .ToListAsync();

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim("TenantId", user.TenantId.ToString()),
                new Claim("UserId", user.Id.ToString())
            };

            // Add all roles to claims
            foreach (var ur in roleMappings)
            {
                claims.Add(new Claim(ClaimTypes.Role, ur.Role.Name));
            }

            if (roleMappings.Any())
            {
                claims.Add(new Claim("PrimaryRole", roleMappings.First().Role.Name));
            }

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