using Dynamics_Leads.Application.DTOs;
using Dynamics_Leads.Application.DTOs.Auth;
using Dynamics_Leads.Application.Security;
using Dynamics_Leads.Domain.Entities;
using Dynamics_Leads.Domain.Repositories;

namespace Dynamics_Leads.Application.Services;

public sealed class UsuarioService : IUsuarioService
{
    private readonly IUsuarioRepository _usuarios;
    private readonly IRolRepository _roles;
    private readonly IPasswordHasher _hasher;

    public UsuarioService(IUsuarioRepository usuarios, IRolRepository roles, IPasswordHasher hasher)
    {
        _usuarios = usuarios;
        _roles = roles;
        _hasher = hasher;
    }

    public async Task<PagedResult<UsuarioResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        page = page < 1 ? 1 : page;
        pageSize = Math.Clamp(pageSize, 1, 100);

        var total = await _usuarios.CountAsync(cancellationToken);
        var usuarios = await _usuarios.ListAsync((page - 1) * pageSize, pageSize, cancellationToken);

        return new PagedResult<UsuarioResponse>
        {
            Items = usuarios.Select(AuthService.MapUsuario).ToList(),
            Page = page,
            PageSize = pageSize,
            Total = total,
        };
    }

    public async Task<UsuarioResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var usuario = await _usuarios.GetByIdAsync(id, cancellationToken);
        return usuario is null ? null : AuthService.MapUsuario(usuario);
    }

    public async Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var username = request.Username.Trim();
        if (await _usuarios.GetByUsernameAsync(username, cancellationToken) is not null)
        {
            throw new ArgumentException($"Ya existe un usuario con el nombre '{username}'.");
        }

        var usuario = new Usuario
        {
            Username = username,
            Email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim(),
            PasswordHash = _hasher.Hash(request.Password),
            RolId = request.RolId,
            Activo = true,
        };

        usuario.Id = await _usuarios.InsertAsync(usuario, cancellationToken);

        // Relee para devolver el rol resuelto y la fecha.
        var creado = await _usuarios.GetByIdAsync(usuario.Id, cancellationToken);
        return AuthService.MapUsuario(creado!);
    }

    public async Task<bool> UpdateAsync(Guid id, UpdateUsuarioRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = string.IsNullOrWhiteSpace(request.Email) ? null : request.Email.Trim();
        var actualizado = await _usuarios.UpdateAsync(id, email, request.RolId, request.Activo, cancellationToken);
        if (!actualizado)
        {
            return false;
        }

        if (!string.IsNullOrWhiteSpace(request.Password))
        {
            await _usuarios.UpdatePasswordAsync(id, _hasher.Hash(request.Password), cancellationToken);
        }

        return true;
    }

    public async Task<bool> ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return await _usuarios.UpdatePasswordAsync(id, _hasher.Hash(request.Password), cancellationToken);
    }

    public Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        => _usuarios.DeleteAsync(id, cancellationToken);

    public async Task<IReadOnlyList<RolResponse>> ListRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await _roles.ListAsync(cancellationToken);
        return roles.Select(r => new RolResponse { Id = r.Id, Nombre = r.Nombre }).ToList();
    }
}
