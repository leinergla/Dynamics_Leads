import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  obtenerUsuario,
  crearUsuario,
  actualizarUsuario,
  listarRoles,
} from '../api/auth'

export default function UsuarioForm() {
  const { id } = useParams()
  const navigate = useNavigate()
  const esEdicion = Boolean(id)

  const [roles, setRoles] = useState([])
  const [form, setForm] = useState({
    username: '',
    email: '',
    password: '',
    rolId: '',
    activo: true,
  })
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  // Carga de roles y, en edición, del usuario.
  useEffect(() => {
    let cancelado = false
    setLoading(true)
    const peticiones = esEdicion
      ? Promise.all([listarRoles(), obtenerUsuario(id)])
      : Promise.all([listarRoles(), null])

    peticiones
      .then(([rs, usuario]) => {
        if (cancelado) return
        setRoles(rs)
        if (usuario) {
          setForm({
            username: usuario.username ?? '',
            email: usuario.email ?? '',
            password: '',
            rolId: usuario.rolId ?? rs[0]?.id ?? '',
            activo: usuario.activo ?? true,
          })
        } else {
          setForm((f) => ({ ...f, rolId: f.rolId || rs[0]?.id || '' }))
        }
      })
      .catch((e) =>
        !cancelado &&
        setError(
          e.response?.status === 404
            ? 'Usuario no encontrado.'
            : e.response?.status === 403
              ? 'No tienes permiso.'
              : (e.message ?? 'Error al cargar'),
        ),
      )
      .finally(() => !cancelado && setLoading(false))
    return () => {
      cancelado = true
    }
  }, [id, esEdicion])

  async function onSubmit(e) {
    e.preventDefault()
    setError('')

    if (!esEdicion && !form.username.trim()) {
      setError('El usuario es obligatorio.')
      return
    }
    if (!esEdicion && !form.password) {
      setError('La contraseña es obligatoria.')
      return
    }
    if (!form.rolId) {
      setError('El rol es obligatorio.')
      return
    }

    setSaving(true)
    try {
      if (esEdicion) {
        await actualizarUsuario(id, {
          email: form.email.trim() || null,
          rolId: form.rolId,
          activo: form.activo,
        })
      } else {
        await crearUsuario({
          username: form.username.trim(),
          email: form.email.trim() || null,
          password: form.password,
          rolId: form.rolId,
        })
      }
      navigate('/usuarios')
    } catch (e) {
      const detalle = e.response?.data?.detail ?? e.response?.data?.title ?? e.message
      setError(detalle ?? 'No se pudo guardar el usuario')
      setSaving(false)
    }
  }

  if (loading) return <p className="text-slate-400 dark:text-slate-500">Cargando…</p>

  const inputClass =
    'mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-900 dark:text-slate-100'

  return (
    <div className="space-y-6">
      <div>
        <Link
          to="/usuarios"
          className="text-sm font-medium text-indigo-600 hover:text-indigo-800 dark:text-indigo-400 dark:hover:text-indigo-300"
        >
          ← Volver a usuarios
        </Link>
      </div>

      <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">
        {esEdicion ? 'Editar usuario' : 'Nuevo usuario'}
      </h1>

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/20 dark:text-red-300">
          {error}
        </div>
      )}

      <form onSubmit={onSubmit} className="max-w-lg space-y-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm dark:border-slate-700 dark:bg-slate-800">
        <div>
          <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">Usuario</label>
          <input
            value={form.username}
            onChange={(e) => setForm({ ...form, username: e.target.value })}
            placeholder="Usuario"
            required={!esEdicion}
            disabled={esEdicion}
            maxLength={100}
            className={`${inputClass} disabled:cursor-not-allowed disabled:bg-slate-100 disabled:text-slate-500 dark:disabled:bg-slate-700 dark:disabled:text-slate-400`}
          />
          {esEdicion && (
            <p className="mt-1 text-xs text-slate-400 dark:text-slate-500">El nombre de usuario no se puede modificar.</p>
          )}
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">Email (opcional)</label>
          <input
            value={form.email}
            onChange={(e) => setForm({ ...form, email: e.target.value })}
            placeholder="correo@ejemplo.com"
            type="email"
            maxLength={255}
            className={inputClass}
          />
        </div>

        {!esEdicion && (
          <div>
            <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">Contraseña</label>
            <input
              value={form.password}
              onChange={(e) => setForm({ ...form, password: e.target.value })}
              placeholder="Contraseña"
              type="password"
              required
              minLength={6}
              className={inputClass}
            />
          </div>
        )}

        <div>
          <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">Rol</label>
          <select
            value={form.rolId}
            onChange={(e) => setForm({ ...form, rolId: e.target.value })}
            className={`${inputClass} bg-white dark:bg-slate-900`}
          >
            {roles.map((r) => (
              <option key={r.id} value={r.id}>
                {r.nombre}
              </option>
            ))}
          </select>
        </div>

        {esEdicion && (
          <label className="flex items-center gap-2 text-sm font-medium text-slate-700 dark:text-slate-300">
            <input
              type="checkbox"
              checked={form.activo}
              onChange={(e) => setForm({ ...form, activo: e.target.checked })}
              className="h-4 w-4 rounded border-slate-300 text-indigo-600 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-900"
            />
            Usuario activo
          </label>
        )}

        <div className="flex items-center gap-3 pt-2">
          <button
            type="submit"
            disabled={saving}
            className="rounded-lg bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:opacity-50"
          >
            {saving ? 'Guardando…' : esEdicion ? 'Guardar cambios' : 'Crear usuario'}
          </button>
          <Link
            to="/usuarios"
            className="rounded-lg border border-slate-300 px-5 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-slate-100 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700"
          >
            Cancelar
          </Link>
        </div>
      </form>
    </div>
  )
}
