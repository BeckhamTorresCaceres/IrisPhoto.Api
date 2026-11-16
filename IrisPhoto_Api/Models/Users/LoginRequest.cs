using System.ComponentModel.DataAnnotations;

namespace IrisPhoto_Api.Models.Users
{
    public record LoginRequest
    {
        [Required(ErrorMessage = "El correo electrónico es obligatorio.")]
        [EmailAddress(ErrorMessage = "El formato del correo electrónico no es válido.")]
        [MaxLength(255, ErrorMessage = "El correo no puede superar los 255 caracteres.")]
        public string Email { get; init; } = string.Empty;

        [Required(ErrorMessage = "La contraseña es obligatoria.")]
        public string Password { get; init; } = string.Empty;
    }

    public record AuthResponse(
        string Token,
        DateTime Expiration,
        int UserId,
        string Email,
        bool IsActive
    );
}
