using Dapper;
using Dynamics_Leads.Application.Export;
using Dynamics_Leads.Application.Security;
using Dynamics_Leads.Application.Storage;
using Dynamics_Leads.Domain.Repositories;
using Dynamics_Leads.Infrastructure.Configuration;
using Dynamics_Leads.Infrastructure.Export;
using Dynamics_Leads.Infrastructure.Persistence;
using Dynamics_Leads.Infrastructure.Repositories;
using Dynamics_Leads.Infrastructure.Security;
using Dynamics_Leads.Infrastructure.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamics_Leads.Infrastructure;

/// <summary>
/// Registro de los servicios de la capa de infraestructura en el contenedor de DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Mapea columnas snake_case (fecha_creacion, nombre_archivo, ...) a propiedades PascalCase.
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.AddOptions<DatabaseOptions>()
            .Bind(configuration.GetSection(DatabaseOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.ConnectionString),
                "La cadena de conexión 'Database:ConnectionString' es obligatoria.")
            .ValidateOnStart();

        services.Configure<ArchivosOptions>(configuration.GetSection(ArchivosOptions.SectionName));

        services.AddOptions<JwtOptions>()
            .Bind(configuration.GetSection(JwtOptions.SectionName))
            .Validate(o => !string.IsNullOrWhiteSpace(o.Key) && o.Key.Length >= 32,
                "La clave 'Jwt:Key' es obligatoria y debe tener al menos 32 caracteres.")
            .ValidateOnStart();

        services.AddSingleton<IDbConnectionFactory, NpgsqlConnectionFactory>();
        services.AddScoped<ILeadRepository, LeadRepository>();
        services.AddSingleton<IArchivoStorage, FileSystemArchivoStorage>();
        services.AddSingleton<IExcelExporter, ClosedXmlExcelExporter>();

        // Auth
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IRolRepository, RolRepository>();
        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();

        return services;
    }
}
