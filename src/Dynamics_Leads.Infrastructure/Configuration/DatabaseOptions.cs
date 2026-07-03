namespace Dynamics_Leads.Infrastructure.Configuration;

/// <summary>
/// Opciones de configuración para el acceso a la base de datos PostgreSQL.
/// Se enlaza desde la sección "Database" de appsettings.json.
/// </summary>
public sealed class DatabaseOptions
{
    public const string SectionName = "Database";

    /// <summary>Cadena de conexión a la base de datos dynamics_leads.</summary>
    public string ConnectionString { get; set; } = string.Empty;
}
