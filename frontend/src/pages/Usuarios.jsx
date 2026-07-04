import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import { listarUsuarios, eliminarUsuario } from '../api/auth'
import { useAuth } from '../context/AuthContext'

const PAGE_SIZE = 10

export default function Usuarios() {
  const { user } = useAuth()
  const esAdmin = user?.rol === 'Administrador'
  const [page, setPage] = useState(1)
  const [data, setData] = useState(null)
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')

  useEffect(() => {
    let cancelado = false
    setLoading(true)
    setError('')
    listarUsuarios({ page, pageSize: PAGE_SIZE })
      .then((res) => !cancelado && setData(res))
      .catch((e) =>
        !cancelado &&
        setError(e.response?.status === 403 ? 'No tienes permiso.' : (e.message ?? 'Error')),
      )
      .finally(() => !cancelado && setLoading(false))
    return () => {
      cancelado = true
    }
  }, [page])

  async function onEliminar(id, username) {
    if (!window.confirm(`¿Eliminar el usuario "${username}"?`)) return
    try {
      await eliminarUsuario(id)
      // Si el borrado deja la página actual vacía, retrocede una; si no, recarga.
      if (items.length === 1 && page > 1) {
        setPage((p) => p - 1)
      } else {
        const res = await listarUsuarios({ page, pageSize: PAGE_SIZE })
        setData(res)
      }
    } catch (e) {
      setError(e.message ?? 'No se pudo eliminar')
    }
  }

  const items = data?.items ?? []
  const totalPages = data?.totalPages ?? 0

  return (
    <div className="space-y-6">
      <div className="flex items-center gap-3">
        <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">Usuarios</h1>
        <Link
          to="/usuarios/nuevo"
          className="rounded-lg bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white transition hover:bg-indigo-700"
        >
          + Nuevo usuario
        </Link>
      </div>

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/20 dark:text-red-300">
          {error}
        </div>
      )}

      {/* Lista */}
      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800">
        <table className="min-w-full divide-y divide-slate-200 dark:divide-slate-700">
          <thead className="bg-slate-50 dark:bg-slate-900/40">
            <tr>
              {['Usuario', 'Email', 'Rol', 'Estado', ''].map((h) => (
                <th key={h} className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500 dark:text-slate-400">
                  {h}
                </th>
              ))}
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100 dark:divide-slate-700/60">
            {loading && (
              <tr><td colSpan={5} className="px-4 py-10 text-center text-slate-400 dark:text-slate-500">Cargando…</td></tr>
            )}
            {!loading && items.length === 0 && (
              <tr><td colSpan={5} className="px-4 py-10 text-center text-slate-400 dark:text-slate-500">No hay usuarios.</td></tr>
            )}
            {!loading && items.map((u) => (
              <tr key={u.id} className="hover:bg-slate-50 dark:hover:bg-slate-700/40">
                <td className="px-4 py-3 text-sm font-medium text-slate-800 dark:text-slate-200">{u.username}</td>
                <td className="px-4 py-3 text-sm text-slate-500 dark:text-slate-400">{u.email ?? '—'}</td>
                <td className="px-4 py-3">
                  <span className="inline-flex rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-medium text-indigo-700 dark:bg-indigo-500/20 dark:text-indigo-300">
                    {u.rolNombre}
                  </span>
                </td>
                <td className="px-4 py-3 text-sm">
                  {u.activo
                    ? <span className="text-emerald-600 dark:text-emerald-400">Activo</span>
                    : <span className="text-slate-400 dark:text-slate-500">Inactivo</span>}
                </td>
                <td className="px-4 py-3 text-right">
                  <div className="flex items-center justify-end gap-4">
                    <Link
                      to={`/usuarios/${u.id}/editar`}
                      className="text-sm font-medium text-indigo-600 hover:text-indigo-800 dark:text-indigo-400 dark:hover:text-indigo-300"
                    >
                      Editar
                    </Link>
                    {esAdmin && u.id !== user?.id && (
                      <Link
                        to={`/usuarios/${u.id}/password`}
                        className="text-sm font-medium text-indigo-600 hover:text-indigo-800 dark:text-indigo-400 dark:hover:text-indigo-300"
                      >
                        Contraseña
                      </Link>
                    )}
                    <button
                      onClick={() => onEliminar(u.id, u.username)}
                      className="text-sm font-medium text-red-600 hover:text-red-800 dark:text-red-400 dark:hover:text-red-300"
                    >
                      Eliminar
                    </button>
                  </div>
                </td>
              </tr>
            ))}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <button
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
            className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700"
          >
            ← Anterior
          </button>
          <span className="text-sm text-slate-500 dark:text-slate-400">
            Página {page} de {totalPages}
          </span>
          <button
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
            className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40 dark:border-slate-600 dark:text-slate-300 dark:hover:bg-slate-700"
          >
            Siguiente →
          </button>
        </div>
      )}
    </div>
  )
}
