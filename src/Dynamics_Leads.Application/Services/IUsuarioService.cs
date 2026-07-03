using Dynamics_Leads.Application.DTOs;
using Dynamics_Leads.Application.DTOs.Auth;

namespace Dynamics_Leads.Application.Services;

public interface IUsuarioService
{
    Task<PagedResult<UsuarioResponse>> ListAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<UsuarioResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<UsuarioResponse> CreateAsync(CreateUsuarioRequest request, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Guid id, UpdateUsuarioRequest request, CancellationToken cancellationToken = default);
    Task<bool> ChangePasswordAsync(Guid id, ChangePasswordRequest request, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RolResponse>> ListRolesAsync(CancellationToken cancellationToken = default);
}
