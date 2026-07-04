import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { obtenerUsuario, cambiarPasswordUsuario } from '../api/auth'
import { useAuth } from '../context/AuthContext'

export default function CambiarPassword() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { user } = useAuth()

  const esAdmin = user?.rol === 'Administrador'
  const esUnoMismo = user?.id === id

  const [usuario, setUsuario] = useState(null)
  const [password, setPassword] = useState('')
  const [confirmacion, setConfirmacion] = useState('')
  const [loading, setLoading] = useState(true)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  useEffect(() => {
    // No cargamos el usuario si la operación no está permitida.
    if (!esAdmin || esUnoMismo) {
      setLoading(false)
      return
    }
    let cancelado = false
    setLoading(true)
    obtenerUsuario(id)
      .then((u) => !cancelado && setUsuario(u))
      .catch((e) =>
        !cancelado &&
        setError(e.response?.status === 404 ? 'Usuario no encontrado.' : (e.message ?? 'Error al cargar')),
      )
      .finally(() => !cancelado && setLoading(false))
    return () => {
      cancelado = true
    }
  }, [id, esAdmin, esUnoMismo])

  async function onSubmit(e) {
    e.preventDefault()
    setError('')

    if (password.length < 6) {
      setError('La contraseña debe tener al menos 6 caracteres.')
      return
    }
    if (password !== confirmacion) {
      setError('Las contraseñas no coinciden.')
      return
    }

    setSaving(true)
    try {
      await cambiarPasswordUsuario(id, password)
      navigate('/usuarios')
    } catch (e) {
      const detalle = e.response?.data?.detail ?? e.response?.data?.title ?? e.message
      setError(detalle ?? 'No se pudo cambiar la contraseña')
      setSaving(false)
    }
  }

  const inputClass =
    'mt-1 w-full rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500 dark:border-slate-600 dark:bg-slate-900 dark:text-slate-100'

  const volver = (
    <div>
      <Link to="/usuarios" className="text-sm font-medium text-indigo-600 hover:text-indigo-800 dark:text-indigo-400 dark:hover:text-indigo-300">
        ← Volver a usuarios
      </Link>
    </div>
  )

  // Reglas de acceso: solo un Administrador y nunca sobre uno mismo.
  if (!esAdmin) {
    return (
      <div className="space-y-6">
        {volver}
        <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">Cambiar contraseña</h1>
        <div className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-700 dark:border-amber-800 dark:bg-amber-900/20 dark:text-amber-300">
          Solo un Administrador puede cambiar la contraseña de un usuario.
        </div>
      </div>
    )
  }

  if (esUnoMismo) {
    return (
      <div className="space-y-6">
        {volver}
        <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">Cambiar contraseña</h1>
        <div className="rounded-lg border border-amber-200 bg-amber-50 px-4 py-3 text-sm text-amber-700 dark:border-amber-800 dark:bg-amber-900/20 dark:text-amber-300">
          No puedes cambiar tu propia contraseña por esta vía; solo la de otros usuarios.
        </div>
      </div>
    )
  }

  if (loading) return <p className="text-slate-400 dark:text-slate-500">Cargando…</p>

  return (
    <div className="space-y-6">
      {volver}

      <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">Cambiar contraseña</h1>
      {usuario && (
        <p className="text-sm text-slate-500 dark:text-slate-400">
          Usuario: <span className="font-medium text-slate-700 dark:text-slate-200">{usuario.username}</span>
        </p>
      )}

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/20 dark:text-red-300">
          {error}
        </div>
      )}

      <form onSubmit={onSubmit} className="max-w-lg space-y-4 rounded-xl border border-slate-200 bg-white p-5 shadow-sm dark:border-slate-700 dark:bg-slate-800">
        <div>
          <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">Nueva contraseña</label>
          <input
            value={password}
            onChange={(e) => setPassword(e.target.value)}
            placeholder="Nueva contraseña"
            type="password"
            required
            minLength={6}
            className={inputClass}
          />
        </div>

        <div>
          <label className="block text-sm font-medium text-slate-700 dark:text-slate-300">Confirmar contraseña</label>
          <input
            value={confirmacion}
            onChange={(e) => setConfirmacion(e.target.value)}
            placeholder="Repite la contraseña"
            type="password"
            required
            minLength={6}
            className={inputClass}
          />
        </div>

        <div className="flex items-center gap-3 pt-2">
          <button
            type="submit"
            disabled={saving}
            className="rounded-lg bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:opacity-50"
          >
            {saving ? 'Guardando…' : 'Cambiar contraseña'}
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
