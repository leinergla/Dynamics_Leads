import { useEffect, useState } from 'react'
import { Link } from 'react-router-dom'
import {
  listarLeads,
  listarFormularios,
  camposDinamicos,
  exportarLeadsAExcel,
} from '../api/leads'
import { useAuth } from '../context/AuthContext'

const PAGE_SIZE = 10

function formatFecha(valor) {
  if (!valor) return '—'
  const d = new Date(valor)
  return Number.isNaN(d.getTime()) ? valor : d.toLocaleString()
}

export default function ConsultarLeads() {
  const { hasPermiso } = useAuth()
  const [formulario, setFormulario] = useState('')
  const [formularios, setFormularios] = useState([])
  const [page, setPage] = useState(1)
  const [data, setData] = useState(null)
  const [loading, setLoading] = useState(false)
  const [error, setError] = useState('')
  const [exportando, setExportando] = useState(false)

  // Carga los formularios disponibles para el dropdown.
  useEffect(() => {
    let cancelado = false
    listarFormularios()
      .then((res) => !cancelado && setFormularios(res))
      .catch(() => {})
    return () => {
      cancelado = true
    }
  }, [])

  useEffect(() => {
    // No se cargan leads hasta que se selecciona un formulario.
    if (!formulario) {
      setData(null)
      setError('')
      setLoading(false)
      return
    }
    let cancelado = false
    setLoading(true)
    setError('')
    listarLeads({ formulario, page, pageSize: PAGE_SIZE })
      .then((res) => !cancelado && setData(res))
      .catch((e) => !cancelado && setError(e.message ?? 'Error al cargar leads'))
      .finally(() => !cancelado && setLoading(false))
    return () => {
      cancelado = true
    }
  }, [formulario, page])

  function cambiarFormulario(e) {
    setPage(1)
    setFormulario(e.target.value)
  }

  async function exportarExcel() {
    if (!formulario) return
    setExportando(true)
    setError('')
    try {
      await exportarLeadsAExcel({ formulario })
    } catch (e) {
      setError(e.message ?? 'Error al exportar a Excel')
    } finally {
      setExportando(false)
    }
  }

  const items = data?.items ?? []
  const totalPages = data?.totalPages ?? 0

  return (
    <div className="space-y-6">
      <div className="flex flex-wrap items-end justify-between gap-4">
        <div>
          <div className="flex items-center gap-3">
            <h1 className="text-2xl font-bold text-slate-900">Consultar Leads</h1>
            {hasPermiso('leads.create') && (
              <Link
                to="/leads/nuevo"
                className="rounded-lg bg-indigo-600 px-3 py-1.5 text-sm font-medium text-white transition hover:bg-indigo-700"
              >
                + Nuevo lead
              </Link>
            )}
            <button
              type="button"
              onClick={exportarExcel}
              disabled={!formulario || exportando || (data?.total ?? 0) === 0}
              className="inline-flex items-center gap-1.5 rounded-lg border border-emerald-600 px-3 py-1.5 text-sm font-medium text-emerald-700 transition hover:bg-emerald-50 disabled:cursor-not-allowed disabled:border-slate-300 disabled:text-slate-400 disabled:hover:bg-transparent"
            >
              {exportando ? 'Exportando…' : '↓ Exportar a Excel'}
            </button>
          </div>
          <p className="mt-1 text-sm text-slate-500">
            {!formulario
              ? 'Selecciona un formulario para ver sus leads'
              : data
                ? `${data.total} lead(s) encontrados`
                : 'Cargando…'}
          </p>
        </div>

        <div>
          <label className="block text-xs font-medium text-slate-500">Formulario</label>
          <select
            value={formulario}
            onChange={cambiarFormulario}
            className="mt-1 w-64 rounded-lg border border-slate-300 bg-white px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
          >
            <option value="">Seleccione un formulario…</option>
            {formularios.map((f) => (
              <option key={f} value={f}>
                {f}
              </option>
            ))}
          </select>
        </div>
      </div>

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <div className="overflow-hidden rounded-xl border border-slate-200 bg-white shadow-sm">
        <table className="min-w-full divide-y divide-slate-200">
          <thead className="bg-slate-50">
            <tr>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                Formulario
              </th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                Campos
              </th>
              <th className="px-4 py-3 text-left text-xs font-semibold uppercase tracking-wide text-slate-500">
                Fecha
              </th>
              <th className="px-4 py-3 text-right text-xs font-semibold uppercase tracking-wide text-slate-500">
                Acciones
              </th>
            </tr>
          </thead>
          <tbody className="divide-y divide-slate-100">
            {loading && (
              <tr>
                <td colSpan={4} className="px-4 py-10 text-center text-slate-400">
                  Cargando…
                </td>
              </tr>
            )}

            {!loading && items.length === 0 && (
              <tr>
                <td colSpan={4} className="px-4 py-10 text-center text-slate-400">
                  {!formulario
                    ? 'Selecciona un formulario en el desplegable para ver sus leads.'
                    : 'No hay leads para este formulario.'}
                </td>
              </tr>
            )}

            {!loading &&
              items.map((lead) => {
                const campos = camposDinamicos(lead)
                return (
                  <tr key={lead.leadId} className="hover:bg-slate-50">
                    <td className="px-4 py-3">
                      <span className="inline-flex rounded-full bg-indigo-100 px-2.5 py-0.5 text-xs font-medium text-indigo-700">
                        {lead.formulario}
                      </span>
                    </td>
                    <td className="px-4 py-3">
                      <div className="flex flex-wrap gap-1.5">
                        {campos.slice(0, 4).map(([k, v]) => (
                          <span
                            key={k}
                            className="rounded-md bg-slate-100 px-2 py-0.5 text-xs text-slate-600"
                          >
                            <span className="font-medium text-slate-500">{k}:</span> {String(v)}
                          </span>
                        ))}
                        {campos.length > 4 && (
                          <span className="text-xs text-slate-400">
                            +{campos.length - 4} más
                          </span>
                        )}
                        {campos.length === 0 && (
                          <span className="text-xs text-slate-400">sin campos</span>
                        )}
                      </div>
                    </td>
                    <td className="px-4 py-3 text-sm text-slate-500">
                      {formatFecha(lead.fechaCreacion)}
                    </td>
                    <td className="px-4 py-3 text-right">
                      <Link
                        to={`/leads/${lead.leadId}`}
                        className="text-sm font-medium text-indigo-600 hover:text-indigo-800"
                      >
                        Ver detalle →
                      </Link>
                    </td>
                  </tr>
                )
              })}
          </tbody>
        </table>
      </div>

      {totalPages > 1 && (
        <div className="flex items-center justify-between">
          <button
            disabled={page <= 1}
            onClick={() => setPage((p) => p - 1)}
            className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
          >
            ← Anterior
          </button>
          <span className="text-sm text-slate-500">
            Página {page} de {totalPages}
          </span>
          <button
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
            className="rounded-lg border border-slate-300 px-4 py-2 text-sm font-medium text-slate-600 transition hover:bg-slate-100 disabled:cursor-not-allowed disabled:opacity-40"
          >
            Siguiente →
          </button>
        </div>
      )}
    </div>
  )
}
