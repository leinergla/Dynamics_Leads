using System.Data;
using Dapper;
using Dynamics_Leads.Domain.Entities;
using Dynamics_Leads.Domain.Repositories;
using Dynamics_Leads.Infrastructure.Persistence;

namespace Dynamics_Leads.Infrastructure.Repositories;

/// <summary>
/// Implementación de <see cref="ILeadRepository"/> con Dapper sobre PostgreSQL.
/// Todo el acceso a datos se realiza mediante rutinas de base de datos:
/// procedimientos (CALL) para las escrituras y funciones (SELECT ... FROM fn_...) para las lecturas.
/// No contiene SQL DML/DQL en línea contra las tablas.
/// </summary>
public sealed class LeadRepository : ILeadRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LeadRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Guid> InsertAsync(Lead lead, IReadOnlyList<Archivo> archivos, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        using var transaction = connection.BeginTransaction();

        var leadParams = new DynamicParameters();
        leadParams.Add("formulario", lead.Formulario, DbType.String);
        leadParams.Add("datos", lead.Datos, DbType.String);

        var leadId = await connection.QuerySingleAsync<Guid>(new CommandDefinition(
            "CALL public.sp_insert_lead(@formulario, @datos::jsonb, NULL)",
            leadParams, transaction, cancellationToken: cancellationToken));

        foreach (var archivo in archivos)
        {
            var archivoParams = new DynamicParameters();
            archivoParams.Add("leadid", leadId, DbType.Guid);
            archivoParams.Add("nombre_archivo", archivo.NombreArchivo, DbType.String);
            archivoParams.Add("storage_key", archivo.StorageKey, DbType.String);
            archivoParams.Add("nombre_campo", archivo.NombreCampo, DbType.String);
            archivoParams.Add("content_type", archivo.ContentType, DbType.String);
            archivoParams.Add("tamano", archivo.Tamano, DbType.Int64);

            await connection.ExecuteAsync(new CommandDefinition(
                "CALL public.sp_insert_lead_archivo(@leadid, @nombre_archivo, @storage_key, @nombre_campo, @content_type, @tamano, NULL)",
                archivoParams, transaction, cancellationToken: cancellationToken));
        }

        transaction.Commit();
        return leadId;
    }

    public async Task<IReadOnlyList<Lead>> ListAsync(string? formulario, int offset, int limit, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var leads = await connection.QueryAsync<Lead>(new CommandDefinition(
            "SELECT leadid, formulario, datos, fecha_creacion FROM public.fn_list_leads(@formulario, @offset, @limit)",
            new { formulario, offset, limit }, cancellationToken: cancellationToken));

        return leads.ToList();
    }

    public async Task<long> CountAsync(string? formulario, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT public.fn_count_leads(@formulario)",
            new { formulario }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<string>> ListFormulariosAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var formularios = await connection.QueryAsync<string>(new CommandDefinition(
            "SELECT formulario FROM public.fn_list_formularios()",
            cancellationToken: cancellationToken));

        return formularios.ToList();
    }

    public async Task<Lead?> GetByIdAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Lead>(new CommandDefinition(
            "SELECT leadid, formulario, datos, fecha_creacion FROM public.fn_get_lead(@leadId)",
            new { leadId }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<Archivo>> GetArchivosByLeadIdsAsync(IReadOnlyList<Guid> leadIds, CancellationToken cancellationToken = default)
    {
        if (leadIds.Count == 0)
        {
            return [];
        }

        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var archivos = await connection.QueryAsync<Archivo>(new CommandDefinition(
            "SELECT id, leadid, nombre_archivo, storage_key, nombre_campo, content_type, tamano, fecha_creacion FROM public.fn_get_archivos_by_leads(@ids)",
            new { ids = leadIds.ToArray() }, cancellationToken: cancellationToken));

        return archivos.ToList();
    }

    public async Task<Archivo?> GetArchivoByIdAsync(Guid archivoId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleOrDefaultAsync<Archivo>(new CommandDefinition(
            "SELECT id, leadid, nombre_archivo, storage_key, nombre_campo, content_type, tamano, fecha_creacion FROM public.fn_get_archivo(@archivoId)",
            new { archivoId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Lead lead, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("leadid", lead.LeadId, DbType.Guid);
        parameters.Add("formulario", lead.Formulario, DbType.String);
        parameters.Add("datos", lead.Datos, DbType.String);

        return await connection.QuerySingleAsync<bool>(new CommandDefinition(
            "CALL public.sp_update_lead(@leadid, @formulario, @datos::jsonb, NULL)",
            parameters, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteAsync(Guid leadId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleAsync<bool>(new CommandDefinition(
            "CALL public.sp_delete_lead(@leadId, NULL)",
            new { leadId }, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteArchivoAsync(Guid archivoId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        return await connection.QuerySingleAsync<bool>(new CommandDefinition(
            "CALL public.sp_delete_lead_archivo(@archivoId, NULL)",
            new { archivoId }, cancellationToken: cancellationToken));
    }
}
