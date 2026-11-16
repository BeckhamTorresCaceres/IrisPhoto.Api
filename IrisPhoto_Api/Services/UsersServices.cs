using IrisPhoto_Api.Domain.Entities;
using IrisPhoto_Api.Infrastructure;
using IrisPhoto_Api.Models.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using static IrisPhoto_Api.Infrastructure.IBaseRepository;

namespace IrisPhoto_Api.Services
{
    public interface IUsersServices : IBaseRepository<User>
    {
        Task<User?> GetByEmailAsync(string email);
        bool ValidateCredentials(User user, string password);
        AuthResponse GenerateJwtToken(User user);
    }

    public class UsersServices : BaseRepository<User>, IUsersServices
    {
        private readonly IAppConfigService _configService;

        public UsersServices(AppDbContext context, IAppConfigService configService)
            : base(context)
        {
            _configService = configService;
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet.FirstOrDefaultAsync(u => u.Email == email);
        }

        // Hash válido generado una sola vez para consumir tiempo real de CPU cuando el usuario no existe
        private const string DummyPasswordHash = "AQAAAAIAAYagAAAAEL0I/1+H2k5V6x8Z9a0S1d2F3g4H5j6K7l8M9n0B1v2C3x4Z5a6S7d8F9g0H1j2K==";

        public bool ValidateCredentials(User? user, string password)
        {
            var passwordHasher = new PasswordHasher<User>();

            //passwordHasher.HashPassword(user, "Admin123*");


            if (user is null)
            {
                // Ejecuta la verificación falsa para igualar el costo de CPU y evitar timing attacks
                passwordHasher.VerifyHashedPassword(null!, DummyPasswordHash, password ?? string.Empty);
                return false;
            }

            if (!user.IsActive || string.IsNullOrWhiteSpace(password))
            {
                passwordHasher.VerifyHashedPassword(user, DummyPasswordHash, password ?? string.Empty);
                return false;
            }

            var result = passwordHasher.VerifyHashedPassword(user, user.PasswordHash, password);

            return result == PasswordVerificationResult.Success
                || result == PasswordVerificationResult.SuccessRehashNeeded;
        }

        public AuthResponse GenerateJwtToken(User user)
        {
            var secretKey = _configService.GetRequiredValue("JwtSettings:SecretKey", "JWT_SECRET_KEY");
            var issuer = _configService.GetValue("JwtSettings:Issuer", "JWT_ISSUER", "IrisPhotoApi");
            var audience = _configService.GetValue("JwtSettings:Audience", "JWT_AUDIENCE", "IrisPhotoMobile");
            var expirationHours = _configService.GetValue<int>("JwtSettings:ExpirationHours", "JWT_EXPIRATION_HOURS", 24);

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // 1. Claims: Información del usuario que viaja cifrada dentro del token
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
            };

            // 2. Tiempo de expiración dinámico (por defecto 24 horas para app móvil)
            var expiration = DateTime.UtcNow.AddHours(expirationHours);

            // 3. Construcción del token
            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: claims,
                expires: expiration,
                signingCredentials: creds
            );

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return new AuthResponse(
                Token: tokenString,
                Expiration: expiration,
                UserId: user.Id,
                Email: user.Email,
                IsActive: user.IsActive
            );
        }

    }
}