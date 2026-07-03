namespace Dynamics_Leads.Application.Auth;

/// <summary>Códigos de permiso del sistema (deben coincidir con la tabla public.permisos).</summary>
public static class Permisos
{
    public const string LeadsRead = "leads.read";
    public const string LeadsCreate = "leads.create";
    public const string LeadsUpdate = "leads.update";
    public const string LeadsDelete = "leads.delete";
    public const string UsuariosManage = "usuarios.manage";

    /// <summary>Tipo de claim usado para los permisos dentro del JWT.</summary>
    public const string ClaimType = "permiso";

    public static readonly string[] Todos =
        [LeadsRead, LeadsCreate, LeadsUpdate, LeadsDelete, UsuariosManage];
}
