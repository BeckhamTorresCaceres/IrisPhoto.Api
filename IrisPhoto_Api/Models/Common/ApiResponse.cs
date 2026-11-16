using System.Text.Json.Serialization;

namespace IrisPhoto_Api.Models.Common
{
    public record ApiResponse<T>
    {
        public bool Success { get; init; }
        public string Message { get; init; } = string.Empty;

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public T? Data { get; init; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public IEnumerable<string>? Errors { get; init; }

        // Métodos de fábrica para crear respuestas rápido y limpio
        public static ApiResponse<T> Ok(T data, string message = "Operation successful.") =>
            new()
            {
                Success = true,
                Message = message,
                Data = data
            };

        public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
            new()
            {
                Success = false,
                Message = message,
                Errors = errors
            };
    }
}
