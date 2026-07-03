using Dynamics_Leads.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Dynamics_Leads.Application;

/// <summary>
/// Registro de los servicios de la capa de aplicación en el contenedor de DI.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<ILeadService, LeadService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUsuarioService, UsuarioService>();
        return services;
    }
}
