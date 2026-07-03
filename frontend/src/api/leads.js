import axios from 'axios'

// En desarrollo, Vite hace proxy de /api -> http://localhost:5137 (ver vite.config.js).
// En producción, define VITE_API_URL con la base de la API.
const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? '',
  withCredentials: true, // envía la cookie httpOnly de autenticación
})

// Handler que se dispara cuando una petición devuelve 401 (sesión expirada/ausente).
// Lo registra AuthContext para limpiar la sesión y forzar la redirección a /login.
let onUnauthorized = null

/** Registra el handler global de 401. */
export function setUnauthorizedHandler(fn) {
  onUnauthorized = fn
}

// Interceptor: ante un 401, limpia la sesión (salvo en los propios endpoints de auth,
// donde el 401 es un flujo esperado: login con credenciales inválidas o `me()` sin sesión).
api.interceptors.response.use(
  (response) => response,
  (error) => {
    const status = error?.response?.status
    const url = error?.config?.url ?? ''
    const esEndpointAuth = url.includes('/api/auth/')
    if (status === 401 && !esEndpointAuth && onUnauthorized) {
      onUnauthorized()
    }
    return Promise.reject(error)
  },
)

/** Claves reservadas que NO son campos dinámicos del formulario. */
export const CLAVES_RESERVADAS = ['leadId', 'formulario', 'fechaCreacion']

/** Devuelve solo los campos dinámicos de un lead (pares [clave, valor]). */
export function camposDinamicos(lead) {
  return Object.entries(lead).filter(([k]) => !CLAVES_RESERVADAS.includes(k))
}

/** Lista paginada de leads. */
export async function listarLeads({ formulario = '', page = 1, pageSize = 20 } = {}) {
  const params = { page, pageSize }
  if (formulario) params.formulario = formulario
  const { data } = await api.get('/api/Leads', { params })
  return data
}

/**
 * Descarga desde la API el Excel (.xlsx) con todos los leads del formulario indicado
 * (sin filtro = todos) y dispara la descarga en el navegador. El archivo lo genera el
 * servidor (GET /api/Leads/export); aquí solo se guarda el blob.
 */
export async function exportarLeadsAExcel({ formulario = '' } = {}) {
  const params = {}
  if (formulario) params.formulario = formulario
  const { data, headers } = await api.get('/api/Leads/export', {
    params,
    responseType: 'blob',
  })

  const fecha = new Date().toISOString().slice(0, 10)
  const nombreArchivo =
    nombreDeContentDisposition(headers?.['content-disposition']) ??
    `leads_${formulario || 'todos'}_${fecha}.xlsx`

  const url = URL.createObjectURL(data)
  const a = document.createElement('a')
  a.href = url
  a.download = nombreArchivo
  document.body.appendChild(a)
  a.click()
  a.remove()
  URL.revokeObjectURL(url)
}

/** Extrae el filename de una cabecera Content-Disposition, o null si no viene. */
function nombreDeContentDisposition(disposition) {
  if (!disposition) return null
  const match = /filename\*?=(?:UTF-8'')?["']?([^"';]+)/i.exec(disposition)
  return match ? decodeURIComponent(match[1]) : null
}

/** Lista los nombres de formulario distintos existentes (para el dropdown del filtro). */
export async function listarFormularios() {
  const { data } = await api.get('/api/Leads/formularios')
  return data
}

/** Detalle de un lead (objeto dinámico). */
export async function obtenerLead(id) {
  const { data } = await api.get(`/api/Leads/${id}`)
  return data
}

/** Crea un lead. payload: { formulario, datos:[...], archivos:[...] }. */
export async function crearLead(payload) {
  const { data } = await api.post('/api/Leads', payload)
  return data
}

/** Actualiza formulario y campos de un lead. payload: { formulario, datos:[...] }. */
export async function actualizarLead(id, payload) {
  const { data } = await api.put(`/api/Leads/${id}`, payload)
  return data
}

/** Elimina un lead (y sus archivos). */
export async function eliminarLead(id) {
  await api.delete(`/api/Leads/${id}`)
}

/** Elimina un archivo concreto. */
export async function eliminarArchivo(archivoId) {
  await api.delete(`/api/Leads/archivos/${archivoId}`)
}

/** Lee un File del navegador y devuelve su contenido en Base64 (sin el prefijo data:). */
export function archivoABase64(file) {
  return new Promise((resolve, reject) => {
    const reader = new FileReader()
    reader.onload = () => resolve(String(reader.result).split(',')[1] ?? '')
    reader.onerror = reject
    reader.readAsDataURL(file)
  })
}

/** Archivos de un lead. */
export async function obtenerArchivos(id) {
  const { data } = await api.get(`/api/Leads/${id}/archivos`)
  return data
}

/** Campos crudos de un lead (nombre, valor, orden, alias) para edición fiel. */
export async function obtenerCampos(id) {
  const { data } = await api.get(`/api/Leads/${id}/campos`)
  return data
}

/** URL absoluta de descarga de un archivo (a partir de la url relativa que da la API). */
export function urlDescarga(urlRelativa) {
  const base = import.meta.env.VITE_API_URL ?? ''
  return `${base}${urlRelativa}`
}

export default api
