using IrisPhoto_Api.Infrastructure;
using IrisPhoto_Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
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
builder.Services.AddSingleton<IAppConfigService, AppConfigService>();

builder.Services.AddScoped<IUsersServices, UsersServices>();
builder.Services.AddScoped<IBoxesServices, BoxesServices>();


// 1. Configuración de Controladores y JSON (CamelCase por defecto para JS/React Native)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        // Opcional: Asegura que las propiedades serializadas usen camelCase (ej: "idProducto")
        // options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

// 2. Configuración de OpenAPI / Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        var scheme = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Pega aquí tu token JWT (sin escribir la palabra Bearer)."
        };

        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();
        document.Components.SecuritySchemes["Bearer"] = scheme;

        document.Security ??= [];
        document.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", document)] = []
        });

        return Task.CompletedTask;
    });
});

var configService = new AppConfigService(builder.Configuration, builder.Environment);

// 3. Configuración de Autenticación JWT (Reemplaza con tus claves reales desde appsettings.json)
var secretKey = configService.GetRequiredValue("JwtSettings:SecretKey", "JWT_SECRET_KEY");
var issuer = configService.GetValue("JwtSettings:Issuer", "JWT_ISSUER", "IrisPhotoApi");
var audience = configService.GetValue("JwtSettings:Audience", "JWT_AUDIENCE", "IrisPhotoMobile");

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
        ValidIssuer = issuer,
        ValidAudience = audience,
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
        await context.HttpContext.Response.WriteAsync("Too many requests. Please try again later.");
    };
});

var app = builder.Build();

// 5. Configuración del Pipeline de Middleware (El orden es estricto)

// Manejo global de excepciones para entornos que no son de desarrollo (devuelve ProblemDetails RFC 7807)
var enableSwagger = configService.GetValue<bool>("AppSettings:EnableSwagger", "ENABLE_SWAGGER", app.Environment.IsDevelopment());
if (enableSwagger)
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "IrisPhoto API v1");
        options.RoutePrefix = string.Empty;
    });
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
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