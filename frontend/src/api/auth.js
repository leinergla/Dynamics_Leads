import api from './leads'

/** Inicia sesión. Devuelve { usuario, permisos }. La cookie httpOnly la setea el servidor. */
export async function login(username, password) {
  const { data } = await api.post('/api/auth/login', { username, password })
  return data
}

/** Cierra sesión (borra la cookie). */
export async function logout() {
  await api.post('/api/auth/logout')
}

/** Usuario autenticado actual (o lanza 401 si no hay sesión). */
export async function me() {
  const { data } = await api.get('/api/auth/me')
  return data
}

// ---- Gestión de usuarios y roles (requiere permiso usuarios.manage) ----

export async function listarUsuarios({ page = 1, pageSize = 10 } = {}) {
  const { data } = await api.get('/api/usuarios', { params: { page, pageSize } })
  return data // PagedResult: { items, page, pageSize, total, totalPages }
}

export async function obtenerUsuario(id) {
  const { data } = await api.get(`/api/usuarios/${id}`)
  return data
}

export async function crearUsuario(payload) {
  const { data } = await api.post('/api/usuarios', payload)
  return data
}

export async function actualizarUsuario(id, payload) {
  await api.put(`/api/usuarios/${id}`, payload)
}

/** Restablece la contraseña de otro usuario (no la propia). Solo Administrador. */
export async function cambiarPasswordUsuario(id, password) {
  await api.put(`/api/usuarios/${id}/password`, { password })
}

export async function eliminarUsuario(id) {
  await api.delete(`/api/usuarios/${id}`)
}

export async function listarRoles() {
  const { data } = await api.get('/api/roles')
  return data
}
