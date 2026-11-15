using IrisPhoto_Api.Infrastructure;
using IrisPhoto_Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.Threading.RateLimiting;
using static IrisPhoto_Api.Infrastructure.IBaseRepository;

var builder = WebApplication.CreateBuilder(args);

// 1. Intentar obtener la cadena de conexión del appsettings.json o de una variable de entorno personalizada
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? Environment.GetEnvironmentVariable("DATABASE_URL");

// 2. Registrar el DbContext con esa cadena
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));
builder.Services.AddScoped<IUsersServices, UsersServices>();

// 1. Configuración de Controladores y JSON (CamelCase por defecto para JS/React Native)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Opcional: Asegura que las propiedades serializadas usen camelCase (ej: "idProducto")
        // options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 2. Configuración de OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();

// 3. Configuración de Autenticación JWT (Reemplaza con tus claves reales desde appsettings.json)
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"] ?? "SUPER_SECRET_KEY_CAMBIAR_EN_PRODUCCION_DE_AL MENOS_32_BYTES";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"] ?? "MiApiNet",
        ValidAudience = jwtSettings["Audience"] ?? "MiAppMobile",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

// 4. Configuración de Rate Limiting (Protección para la API móvil)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30, // Máximo 30 peticiones
                Window = TimeSpan.FromSeconds(10), // En una ventana de 10 segundos
                QueueLimit = 2
            }));

    options.OnRejected = async (context, cancellationToken) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        await context.HttpContext.Response.WriteAsync("Demasiadas peticiones. Por favor, inténtalo más tarde.");
    };
});

var app = builder.Build();

// 5. Configuración del Pipeline de Middleware (El orden es estricto)

// Manejo global de excepciones para entornos que no son de desarrollo (devuelve ProblemDetails RFC 7807)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "Mi API v1");
        options.RoutePrefix = string.Empty; // Opcional: hace que Swagger abra directamente en la raíz (http://localhost:xxxx/)
    });
}
else
{
    app.UseExceptionHandler("/error"); // Crea un endpoint o controlador para manejar errores generales de producción
    app.UseHsts();
}

app.UseHttpsRedirection();

// ¡Importante! Authentication siempre debe ir antes de Authorization
app.UseAuthentication();
app.UseAuthorization();

// Activar Rate Limiting en el pipeline
app.UseRateLimiter();

app.MapControllers();

app.Run();