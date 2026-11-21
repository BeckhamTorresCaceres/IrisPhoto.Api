using System.Security.Claims;

namespace IrisPhoto_Api.Models.Common
{
    public static class ClaimsPrincipalExtensions
    {
        public static int GetUserId(this ClaimsPrincipal user)
        {
            var value = user.FindFirstValue(ClaimTypes.NameIdentifier)
                        ?? user.FindFirstValue("sub");

            return int.TryParse(value, out var userId)
                ? userId
                : throw new UnauthorizedAccessException("No se pudo identificar al usuario en el token.");
        }

        public static string GetUserEmail(this ClaimsPrincipal user)
        {
            return user.FindFirstValue(ClaimTypes.Email)
                   ?? user.FindFirstValue("email")
                   ?? string.Empty;
        }
    }
}
