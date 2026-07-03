using System.Data;
using Dapper;
using Dynamics_Leads.Domain.Entities;
using Dynamics_Leads.Domain.Repositories;
using Dynamics_Leads.Infrastructure.Persistence;

namespace Dynamics_Leads.Infrastructure.Repositories;

/// <summary>Acceso a datos de usuarios con Dapper; solo rutinas de BD.</summary>
public sealed class UsuarioRepository : IUsuarioRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public UsuarioRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Usuario?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Usuario>(new CommandDefinition(
            "SELECT id, username, email, password_hash, rol_id, rol_nombre, activo FROM public.fn_get_usuario_by_username(@username)",
            new { username }, cancellationToken: cancellationToken));
    }

    public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleOrDefaultAsync<Usuario>(new CommandDefinition(
            "SELECT id, username, email, password_hash, rol_id, rol_nombre, activo, fecha_creacion FROM public.fn_get_usuario(@id)",
            new { id }, cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<Usuario>> ListAsync(int offset, int limit, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var usuarios = await connection.QueryAsync<Usuario>(new CommandDefinition(
            "SELECT id, username, email, password_hash, rol_id, rol_nombre, activo, fecha_creacion FROM public.fn_list_usuarios(@offset, @limit)",
            new { offset, limit }, cancellationToken: cancellationToken));
        return usuarios.ToList();
    }

    public async Task<long> CountAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteScalarAsync<long>(new CommandDefinition(
            "SELECT public.fn_count_usuarios()", cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<string>> GetPermisosByRolAsync(Guid rolId, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        var permisos = await connection.QueryAsync<string>(new CommandDefinition(
            "SELECT codigo FROM public.fn_get_permisos_by_rol(@rolId)",
            new { rolId }, cancellationToken: cancellationToken));
        return permisos.ToList();
    }

    public async Task<Guid> InsertAsync(Usuario usuario, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("username", usuario.Username, DbType.String);
        parameters.Add("email", usuario.Email, DbType.String);
        parameters.Add("password_hash", usuario.PasswordHash, DbType.String);
        parameters.Add("rol_id", usuario.RolId, DbType.Guid);
        parameters.Add("activo", usuario.Activo, DbType.Boolean);

        return await connection.QuerySingleAsync<Guid>(new CommandDefinition(
            "CALL public.sp_insert_usuario(@username, @email, @password_hash, @rol_id, @activo, NULL)",
            parameters, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdateAsync(Guid id, string? email, Guid rolId, bool activo, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("id", id, DbType.Guid);
        parameters.Add("email", email, DbType.String);
        parameters.Add("rol_id", rolId, DbType.Guid);
        parameters.Add("activo", activo, DbType.Boolean);

        return await connection.QuerySingleAsync<bool>(new CommandDefinition(
            "CALL public.sp_update_usuario(@id, @email, @rol_id, @activo, NULL)",
            parameters, cancellationToken: cancellationToken));
    }

    public async Task<bool> UpdatePasswordAsync(Guid id, string passwordHash, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);

        var parameters = new DynamicParameters();
        parameters.Add("id", id, DbType.Guid);
        parameters.Add("password_hash", passwordHash, DbType.String);

        return await connection.QuerySingleAsync<bool>(new CommandDefinition(
            "CALL public.sp_update_usuario_password(@id, @password_hash, NULL)",
            parameters, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
        return await connection.QuerySingleAsync<bool>(new CommandDefinition(
            "CALL public.sp_delete_usuario(@id, NULL)",
            new { id }, cancellationToken: cancellationToken));
    }
}
