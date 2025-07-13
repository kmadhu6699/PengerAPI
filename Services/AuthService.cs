using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using PengerAPI.DTOs;
using PengerAPI.Models;

namespace PengerAPI.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;

        public AuthService(UserManager<ApplicationUser> userManager, IConfiguration configuration)
        {
            _userManager = userManager;
            _configuration = configuration;
        }

        public async Task<ApiResponse<AuthResponseDto>> RegisterAsync(RegisterDto registerDto)
        {
            // Check if user already exists
            var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);
            if (existingUser != null)
                return ApiResponse<AuthResponseDto>.ErrorResult("User with this email already exists.");

            // Create new user
            var user = new ApplicationUser
            {
                UserName = registerDto.Username,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                EmailConfirmed = true // Auto-confirm for now
            };

            var result = await _userManager.CreateAsync(user, registerDto.Password);
            if (!result.Succeeded)
                return ApiResponse<AuthResponseDto>.ErrorResult("User creation failed.");

            // Assign default role (optional)
            await _userManager.AddToRoleAsync(user, "User");

            // Generate JWT token
            var token = await GenerateJwtToken(user);

            return ApiResponse<AuthResponseDto>.SuccessResult(
                new AuthResponseDto
                {
                    Token = token,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = "User",
                    ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:ExpiresInHours"]))
                },
                "User registered successfully"
            );
        }

        public async Task<ApiResponse<AuthResponseDto>> LoginAsync(LoginDto loginDto)
        {
            // Find user by email or username
            var user = await _userManager.FindByEmailAsync(loginDto.UsernameOrEmail) ??
                       await _userManager.FindByNameAsync(loginDto.UsernameOrEmail);

            if (user == null || !await _userManager.CheckPasswordAsync(user, loginDto.Password))
                return ApiResponse<AuthResponseDto>.ErrorResult("Invalid credentials.");

            // Generate JWT token
            var token = await GenerateJwtToken(user);
            var roles = await _userManager.GetRolesAsync(user);

            return ApiResponse<AuthResponseDto>.SuccessResult(
                new AuthResponseDto
                {
                    Token = token,
                    Username = user.UserName,
                    Email = user.Email,
                    Role = roles.FirstOrDefault() ?? "User",
                    ExpiresAt = DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:ExpiresInHours"]))
                },
                "Login successful"
            );
        }

        private async Task<string> GenerateJwtToken(ApplicationUser user)
        {
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JwtSettings:SecretKey"]));
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            // Add user roles to claims
            var roles = await _userManager.GetRolesAsync(user);
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var token = new JwtSecurityToken(
                issuer: _configuration["JwtSettings:Issuer"],
                audience: _configuration["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(Convert.ToDouble(_configuration["JwtSettings:ExpiresInHours"])),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
