using System.Text;
using Dynamics_Leads.Api.Middleware;
using Dynamics_Leads.Application;
using Dynamics_Leads.Application.Auth;
using Dynamics_Leads.Application.Security;
using Dynamics_Leads.Domain.Entities;
using Dynamics_Leads.Domain.Repositories;
using Dynamics_Leads.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Controladores (API basada en controladores).
builder.Services.AddControllers();

// OpenAPI (documento generado en /openapi/v1.json).
builder.Services.AddOpenApi();

// CORS para el frontend de React.
const string FrontendCorsPolicy = "FrontendCors";
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicy, policy =>
    {
        if (allowedOrigins.Length > 0)
        {
            // Con orígenes explícitos se permiten credenciales (cookies) cross-origin.
            policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod().AllowCredentials();
        }
        else
        {
            // Sin orígenes configurados: permisivo (útil en desarrollo vía proxy de Vite).
            policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod();
        }
    });
});

// Autenticación JWT: el token viaja en una cookie httpOnly (no en el header).
var jwtSection = builder.Configuration.GetSection("Jwt");
var cookieName = jwtSection["CookieName"] ?? "access_token";
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata = false;
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidateAudience = true,
            ValidAudience = jwtSection["Audience"],
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSection["Key"] ?? string.Empty)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromSeconds(30),
        };
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue(cookieName, out var token))
                {
                    context.Token = token;
                }
                return Task.CompletedTask;
            },
        };
    });

// Una política por permiso: [Authorize(Policy = "leads.read")], etc.
builder.Services.AddAuthorization(options =>
{
    foreach (var permiso in Permisos.Todos)
    {
        options.AddPolicy(permiso, policy => policy.RequireClaim(Permisos.ClaimType, permiso));
    }
});

// Registro de capas mediante Inyección de Dependencias / Inversión de Control.
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

// Crea el usuario administrador inicial si no hay ningún usuario.
await SeedAdminAsync(app);

// Manejo centralizado de errores (ArgumentException -> 400, KeyNotFound/FileNotFound -> 404).
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configuración del pipeline HTTP.
if (app.Environment.IsDevelopment())
{
    // Documento OpenAPI + UI de Scalar (disponible en /scalar).
    app.MapOpenApi();
    app.MapScalarApiReference(options =>
    {
        options
            .WithTitle("Dynamics Leads API")
            .WithTheme(ScalarTheme.Purple);
    });
}

app.UseHttpsRedirection();

app.UseCors(FrontendCorsPolicy);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

static async Task SeedAdminAsync(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var sp = scope.ServiceProvider;
    var usuarios = sp.GetRequiredService<IUsuarioRepository>();

    if (await usuarios.CountAsync() > 0)
    {
        return;
    }

    var roles = sp.GetRequiredService<IRolRepository>();
    var hasher = sp.GetRequiredService<IPasswordHasher>();

    var admin = await roles.GetByNombreAsync("Administrador");
    if (admin is null)
    {
        app.Logger.LogWarning("No se encontró el rol 'Administrador'");
        return;
    }

    var seed = app.Configuration.GetSection("Seed");
    var username = seed["AdminUsername"] ?? "admin";
    var password = seed["AdminPassword"] ?? "Admin123*";

    await usuarios.InsertAsync(new Usuario
    {
        Username = username,
        Email = null,
        PasswordHash = hasher.Hash(password),
        RolId = admin.Id,
        Activo = true,
    });

    app.Logger.LogWarning("Usuario administrador inicial creado (usuario: {Username}). Cámbialo en producción.", username);
}
