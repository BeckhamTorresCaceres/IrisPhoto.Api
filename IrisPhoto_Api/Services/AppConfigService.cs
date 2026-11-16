namespace IrisPhoto_Api.Services
{
    public interface IAppConfigService
    {
        /// <summary>
        /// Obtiene un valor de configuración respetando el entorno (Development vs Docker/Producción).
        /// </summary>
        /// <param name="appSettingsKey">Ruta en appsettings (ej: "JwtSettings:SecretKey")</param>
        /// <param name="envVarName">Nombre explícito de la variable en Docker (ej: "JWT_SECRET_KEY")</param>
        /// <param name="defaultValue">Valor por defecto opcional si no existe en ningún lado</param>
        string GetValue(string appSettingsKey, string? envVarName = null, string? defaultValue = null);

        /// <summary>
        /// Obtiene un valor obligatorio; lanza excepción al arrancar si falta.
        /// </summary>
        string GetRequiredValue(string appSettingsKey, string? envVarName = null);

        /// <summary>
        /// Obtiene un valor convertido a un tipo específico (int, bool, etc.).
        /// </summary>
        T GetValue<T>(string appSettingsKey, string? envVarName = null, T defaultValue = default!);
    }

    public class AppConfigService : IAppConfigService
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public AppConfigService(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public string GetValue(string appSettingsKey, string? envVarName = null, string? defaultValue = null)
        {
            // Si no se especifica un nombre personalizado de variable de entorno, 
            // convierte "JwtSettings:SecretKey" -> "JWTSETTINGS_SECRETKEY"
            var resolvedEnvName = envVarName ?? appSettingsKey.Replace(":", "_").ToUpperInvariant();

            string? value;

            if (_environment.IsDevelopment())
            {
                // En DEV: Prioridad 1 = appsettings.Development.json / appsettings.json | Prioridad 2 = Docker Env
                value = _configuration[appSettingsKey]
                        ?? Environment.GetEnvironmentVariable(resolvedEnvName);
            }
            else
            {
                // En PRODUCCIÓN / DOCKER: Prioridad 1 = Docker Env | Prioridad 2 = appsettings.json
                value = Environment.GetEnvironmentVariable(resolvedEnvName)
                        ?? _configuration[appSettingsKey];
            }

            return !string.IsNullOrWhiteSpace(value)
                ? value
                : defaultValue ?? string.Empty;
        }

        public string GetRequiredValue(string appSettingsKey, string? envVarName = null)
        {
            var value = GetValue(appSettingsKey, envVarName);

            if (string.IsNullOrWhiteSpace(value))
            {
                var envHint = envVarName ?? appSettingsKey.Replace(":", "_").ToUpperInvariant();
                throw new InvalidOperationException(
                    $"Falta la configuración requerida. Defina '{appSettingsKey}' en appsettings.json o la variable de entorno '{envHint}'.");
            }

            return value;
        }

        public T GetValue<T>(string appSettingsKey, string? envVarName = null, T defaultValue = default!)
        {
            var rawValue = GetValue(appSettingsKey, envVarName);

            if (string.IsNullOrWhiteSpace(rawValue))
                return defaultValue;

            return (T)Convert.ChangeType(rawValue, typeof(T));
        }
    }
}
