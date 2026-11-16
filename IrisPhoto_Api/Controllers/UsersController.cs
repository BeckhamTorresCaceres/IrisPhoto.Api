using IrisPhoto_Api.Models.Common;
using IrisPhoto_Api.Models.Users;
using IrisPhoto_Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace IrisPhoto_Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly IUsersServices _usersService;

        public UsersController(IUsersServices usersService)
        {
            _usersService = usersService;
        }

        [HttpPost("LogIn")]
        public async Task<ActionResult<ApiResponse<AuthResponse>>> LoginAsync([FromBody] LoginRequest request)
        {
            const int minResponseTimeMs = 350;
            var stopwatch = Stopwatch.StartNew();

            var user = await _usersService.GetByEmailAsync(request.Email);
            var isValid = _usersService.ValidateCredentials(user, request.Password);

            // Esperar el tiempo restante si la operación terminó antes del mínimo establecido
            var elapsedMs = (int)stopwatch.ElapsedMilliseconds;
            if (elapsedMs < minResponseTimeMs)
            {
                await Task.Delay(minResponseTimeMs - elapsedMs);
            }

            if (!isValid || user is null)
            {
                return Unauthorized(ApiResponse<AuthResponse>.Fail("Incorrect username or password."));
            }

            var authResponse = _usersService.GenerateJwtToken(user);
            return Ok(ApiResponse<AuthResponse>.Ok(authResponse, "Login successful."));
        }

    }
}
