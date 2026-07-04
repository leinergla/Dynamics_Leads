import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import { obtenerLead, obtenerArchivos, eliminarLead, camposDinamicos, urlDescarga } from '../api/leads'
import { useAuth } from '../context/AuthContext'

function formatFecha(valor) {
  if (!valor) return '—'
  const d = new Date(valor)
  return Number.isNaN(d.getTime()) ? valor : d.toLocaleString()
}

function formatBytes(bytes) {
  if (bytes == null) return ''
  if (bytes < 1024) return `${bytes} B`
  if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`
  return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
}

export default function DetalleLead() {
  const { id } = useParams()
  const navigate = useNavigate()
  const { hasPermiso } = useAuth()
  const [lead, setLead] = useState(null)
  const [archivos, setArchivos] = useState([])
  const [loading, setLoading] = useState(true)
  const [error, setError] = useState('')
  const [borrando, setBorrando] = useState(false)

  async function borrarLead() {
    if (!window.confirm('¿Eliminar este lead y todos sus archivos? Esta acción no se puede deshacer.')) return
    setBorrando(true)
    try {
      await eliminarLead(id)
      navigate('/leads')
    } catch (e) {
      setError(e.message ?? 'No se pudo eliminar el lead')
      setBorrando(false)
    }
  }

  useEffect(() => {
    let cancelado = false
    setLoading(true)
    setError('')
    Promise.all([obtenerLead(id), obtenerArchivos(id)])
      .then(([l, a]) => {
        if (cancelado) return
        setLead(l)
        setArchivos(a)
      })
      .catch((e) => {
        if (cancelado) return
        setError(e.response?.status === 404 ? 'Lead no encontrado.' : (e.message ?? 'Error al cargar el lead'))
      })
      .finally(() => !cancelado && setLoading(false))
    return () => {
      cancelado = true
    }
  }, [id])

  return (
    <div className="space-y-6">
      <div>
        <Link to="/leads" className="text-sm font-medium text-indigo-600 hover:text-indigo-800 dark:text-indigo-400 dark:hover:text-indigo-300">
          ← Volver a la lista
        </Link>
      </div>

      {loading && <p className="text-slate-400 dark:text-slate-500">Cargando…</p>}

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700 dark:border-red-800 dark:bg-red-900/20 dark:text-red-300">
          {error}
        </div>
      )}

      {!loading && !error && lead && (
        <>
          <div className="flex flex-wrap items-center gap-3">
            <h1 className="text-2xl font-bold text-slate-900 dark:text-slate-100">Detalle del Lead</h1>
            <span className="inline-flex rounded-full bg-indigo-100 px-3 py-1 text-sm font-medium text-indigo-700 dark:bg-indigo-500/20 dark:text-indigo-300">
              {lead.formulario}
            </span>
            <div className="ml-auto flex items-center gap-2">
              {hasPermiso('leads.update') && (
                <Link
                  to={`/leads/${lead.leadId}/editar`}
                  className="rounded-lg bg-indigo-600 px-4 py-2 text-sm font-medium text-white transition hover:bg-indigo-700"
                >
                  Editar
                </Link>
              )}
              {hasPermiso('leads.delete') && (
                <button
                  onClick={borrarLead}
                  disabled={borrando}
                  className="rounded-lg border border-red-300 px-4 py-2 text-sm font-medium text-red-600 transition hover:bg-red-50 disabled:opacity-50 dark:border-red-800 dark:text-red-400 dark:hover:bg-red-900/20"
                >
                  {borrando ? 'Eliminando…' : 'Eliminar'}
                </button>
              )}
            </div>
          </div>

          {/* Metadatos */}
          <div className="grid gap-4 sm:grid-cols-2">
            <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-800">
              <p className="text-xs font-medium uppercase tracking-wide text-slate-400 dark:text-slate-500">Lead ID</p>
              <p className="mt-1 break-all font-mono text-sm text-slate-700 dark:text-slate-200">{lead.leadId}</p>
            </div>
            <div className="rounded-xl border border-slate-200 bg-white p-4 shadow-sm dark:border-slate-700 dark:bg-slate-800">
              <p className="text-xs font-medium uppercase tracking-wide text-slate-400 dark:text-slate-500">Fecha de creación</p>
              <p className="mt-1 text-sm text-slate-700 dark:text-slate-200">{formatFecha(lead.fechaCreacion)}</p>
            </div>
          </div>

          {/* Campos dinámicos */}
          <section className="rounded-xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800">
            <h2 className="border-b border-slate-100 px-5 py-3 text-sm font-semibold text-slate-700 dark:border-slate-700 dark:text-slate-200">
              Campos del formulario
            </h2>
            <dl className="divide-y divide-slate-100 dark:divide-slate-700/60">
              {camposDinamicos(lead).map(([k, v]) => (
                <div key={k} className="grid grid-cols-3 gap-4 px-5 py-3">
                  <dt className="text-sm font-medium text-slate-500 dark:text-slate-400">{k}</dt>
                  <dd className="col-span-2 text-sm text-slate-800 dark:text-slate-200">{String(v)}</dd>
                </div>
              ))}
              {camposDinamicos(lead).length === 0 && (
                <p className="px-5 py-4 text-sm text-slate-400 dark:text-slate-500">Este lead no tiene campos.</p>
              )}
            </dl>
          </section>

          {/* Archivos */}
          <section className="rounded-xl border border-slate-200 bg-white shadow-sm dark:border-slate-700 dark:bg-slate-800">
            <h2 className="border-b border-slate-100 px-5 py-3 text-sm font-semibold text-slate-700 dark:border-slate-700 dark:text-slate-200">
              Archivos ({archivos.length})
            </h2>
            {archivos.length === 0 ? (
              <p className="px-5 py-4 text-sm text-slate-400 dark:text-slate-500">Este lead no tiene archivos.</p>
            ) : (
              <ul className="divide-y divide-slate-100 dark:divide-slate-700/60">
                {archivos.map((a) => (
                  <li key={a.id} className="flex items-center justify-between gap-4 px-5 py-3">
                    <div className="min-w-0">
                      <p className="truncate text-sm font-medium text-slate-800 dark:text-slate-200">{a.nombreArchivo}</p>
                      <p className="text-xs text-slate-400 dark:text-slate-500">
                        {a.nombreCampo ? `${a.nombreCampo} · ` : ''}
                        {a.contentType} · {formatBytes(a.tamano)}
                      </p>
                    </div>
                    <a
                      href={urlDescarga(a.url)}
                      target="_blank"
                      rel="noreferrer"
                      className="shrink-0 rounded-lg bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white transition hover:bg-indigo-700"
                    >
                      Descargar
                    </a>
                  </li>
                ))}
              </ul>
            )}
          </section>
        </>
      )}
    </div>
  )
}
