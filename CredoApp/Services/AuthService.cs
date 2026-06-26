using CredoApp.DTOs;
using CredoApp.Enums;
using CredoApp.Interfaces;
using CredoApp.Models;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace CredoApp.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;

        public AuthService(IUserRepository userRepository, IConfiguration configuration, ILogger<AuthService> logger)
        {
            _userRepository = userRepository;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> RegisterAsync(RegisterDto dto)
        {
            var existingUser = await _userRepository.GetByUsernameAsync(dto.Username);
            if (existingUser != null)
            {
                throw new InvalidOperationException("Username already exists.");
            }

            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var user = new User
            {
                Username = dto.Username,
                PasswordHash = passwordHash,
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                PersonalNumber = dto.PersonalNumber,
                BirthDate = dto.BirthDate,
                Role = Enum.Parse<UserRole>(dto.Role, ignoreCase: true)
            };

            await _userRepository.AddAsync(user);
            await _userRepository.SaveChangesAsync();
            return true;
        }

        public async Task<AuthResultDto> LoginAsync(LoginDto dto)
        {
            var user = await _userRepository.GetByUsernameAsync(dto.Username);
            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            {
                throw new UnauthorizedAccessException("Invalid username or password.");
            }

            var token = GenerateJwtToken(user);

            return new AuthResultDto
            {
                Token = token,
                Role = user.Role.ToString()
            };
        }

        private string GenerateJwtToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(3),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
