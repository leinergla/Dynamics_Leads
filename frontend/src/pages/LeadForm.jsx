import { useEffect, useState } from 'react'
import { Link, useNavigate, useParams } from 'react-router-dom'
import {
  obtenerLead,
  obtenerArchivos,
  obtenerCampos,
  crearLead,
  actualizarLead,
  eliminarArchivo,
  archivoABase64,
} from '../api/leads'

const campoVacio = () => ({ nombre: '', valor: '', orden: 1, alias: '' })

export default function LeadForm() {
  const { id } = useParams()
  const navigate = useNavigate()
  const esEdicion = Boolean(id)

  const [formulario, setFormulario] = useState('')
  const [campos, setCampos] = useState([campoVacio()])
  const [files, setFiles] = useState([]) // File[] (solo creación)
  const [archivosExistentes, setArchivosExistentes] = useState([]) // edición
  const [loading, setLoading] = useState(esEdicion)
  const [saving, setSaving] = useState(false)
  const [error, setError] = useState('')

  // Carga inicial en modo edición.
  useEffect(() => {
    if (!esEdicion) return
    let cancelado = false
    setLoading(true)
    Promise.all([obtenerLead(id), obtenerCampos(id), obtenerArchivos(id)])
      .then(([lead, campos, archivos]) => {
        if (cancelado) return
        setFormulario(lead.formulario ?? '')
        // Campos crudos del backend: conservan nombre, valor, orden y alias originales.
        const filas = campos.map((c) => ({
          nombre: c.nombre ?? '',
          valor: c.valor == null ? '' : String(c.valor),
          orden: c.orden ?? 0,
          alias: c.alias ?? '',
        }))
        setCampos(filas.length ? filas : [campoVacio()])
        setArchivosExistentes(archivos)
      })
      .catch((e) =>
        !cancelado &&
        setError(e.response?.status === 404 ? 'Lead no encontrado.' : (e.message ?? 'Error al cargar')),
      )
      .finally(() => !cancelado && setLoading(false))
    return () => {
      cancelado = true
    }
  }, [id, esEdicion])

  function actualizarCampo(idx, prop, value) {
    setCampos((prev) => prev.map((c, i) => (i === idx ? { ...c, [prop]: value } : c)))
  }
  function agregarCampo() {
    setCampos((prev) => [...prev, { ...campoVacio(), orden: prev.length + 1 }])
  }
  function quitarCampo(idx) {
    setCampos((prev) => (prev.length === 1 ? prev : prev.filter((_, i) => i !== idx)))
  }

  async function borrarArchivoExistente(archivoId) {
    if (!window.confirm('¿Eliminar este archivo?')) return
    try {
      await eliminarArchivo(archivoId)
      setArchivosExistentes((prev) => prev.filter((a) => a.id !== archivoId))
    } catch (e) {
      setError(e.message ?? 'No se pudo eliminar el archivo')
    }
  }

  async function onSubmit(e) {
    e.preventDefault()
    setError('')

    const datos = campos
      .filter((c) => c.nombre.trim())
      .map((c) => ({
        nombre: c.nombre.trim(),
        valor: c.valor,
        orden: Number(c.orden) || 0,
        alias: c.alias?.trim() || c.nombre.trim(),
      }))

    if (!formulario.trim()) {
      setError("El campo 'formulario' es obligatorio.")
      return
    }
    if (datos.length === 0) {
      setError('Agrega al menos un campo con nombre.')
      return
    }

    setSaving(true)
    try {
      if (esEdicion) {
        await actualizarLead(id, { formulario: formulario.trim(), datos })
        navigate(`/leads/${id}`)
      } else {
        const archivos = await Promise.all(
          files.map(async (f) => ({
            nombreArchivo: f.name,
            nombreCampo: 'adjunto',
            contentType: f.type || 'application/octet-stream',
            contenidoBase64: await archivoABase64(f),
          })),
        )
        const creado = await crearLead({ formulario: formulario.trim(), datos, archivos })
        navigate(`/leads/${creado.leadId}`)
      }
    } catch (e) {
      const detalle = e.response?.data?.detail ?? e.response?.data?.title ?? e.message
      setError(detalle ?? 'No se pudo guardar el lead')
      setSaving(false)
    }
  }

  if (loading) return <p className="text-slate-400">Cargando…</p>

  return (
    <div className="space-y-6">
      <div>
        <Link
          to={esEdicion ? `/leads/${id}` : '/leads'}
          className="text-sm font-medium text-indigo-600 hover:text-indigo-800"
        >
          ← {esEdicion ? 'Volver al detalle' : 'Volver a la lista'}
        </Link>
      </div>

      <h1 className="text-2xl font-bold text-slate-900">
        {esEdicion ? 'Editar Lead' : 'Nuevo Lead'}
      </h1>

      {error && (
        <div className="rounded-lg border border-red-200 bg-red-50 px-4 py-3 text-sm text-red-700">
          {error}
        </div>
      )}

      <form onSubmit={onSubmit} className="space-y-6">
        {/* Formulario */}
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <label className="block text-sm font-medium text-slate-700">Formulario</label>
          <input
            value={formulario}
            onChange={(e) => setFormulario(e.target.value)}
            placeholder="p. ej. contacto"
            maxLength={255}
            className="mt-1 w-full max-w-md rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
          />
        </div>

        {/* Campos */}
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <div className="mb-3 flex items-center justify-between">
            <h2 className="text-sm font-semibold text-slate-700">Campos</h2>
            <button
              type="button"
              onClick={agregarCampo}
              className="rounded-lg bg-slate-100 px-3 py-1.5 text-sm font-medium text-slate-700 transition hover:bg-slate-200"
            >
              + Agregar campo
            </button>
          </div>

          <div className="space-y-2">
            <div className="hidden grid-cols-[1fr_1fr_5rem_1fr_2.5rem] gap-2 px-1 text-xs font-medium text-slate-400 sm:grid">
              <span>Nombre</span>
              <span>Valor</span>
              <span>Orden</span>
              <span>Alias</span>
              <span></span>
            </div>
            {campos.map((c, idx) => (
              <div
                key={idx}
                className="grid grid-cols-1 gap-2 sm:grid-cols-[1fr_1fr_5rem_1fr_2.5rem]"
              >
                <input
                  value={c.nombre}
                  onChange={(e) => actualizarCampo(idx, 'nombre', e.target.value)}
                  placeholder="nombre"
                  className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
                <input
                  value={c.valor}
                  onChange={(e) => actualizarCampo(idx, 'valor', e.target.value)}
                  placeholder="valor"
                  className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
                <input
                  type="number"
                  value={c.orden}
                  onChange={(e) => actualizarCampo(idx, 'orden', e.target.value)}
                  className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
                <input
                  value={c.alias}
                  onChange={(e) => actualizarCampo(idx, 'alias', e.target.value)}
                  placeholder="alias"
                  className="rounded-lg border border-slate-300 px-3 py-2 text-sm focus:border-indigo-500 focus:outline-none focus:ring-1 focus:ring-indigo-500"
                />
                <button
                  type="button"
                  onClick={() => quitarCampo(idx)}
                  disabled={campos.length === 1}
                  title="Quitar campo"
                  className="rounded-lg border border-slate-300 text-slate-500 transition hover:bg-red-50 hover:text-red-600 disabled:cursor-not-allowed disabled:opacity-40"
                >
                  ✕
                </button>
              </div>
            ))}
          </div>
        </div>

        {/* Archivos */}
        <div className="rounded-xl border border-slate-200 bg-white p-5 shadow-sm">
          <h2 className="mb-3 text-sm font-semibold text-slate-700">Archivos</h2>

          {esEdicion ? (
            <>
              {archivosExistentes.length === 0 ? (
                <p className="text-sm text-slate-400">Este lead no tiene archivos.</p>
              ) : (
                <ul className="divide-y divide-slate-100">
                  {archivosExistentes.map((a) => (
                    <li key={a.id} className="flex items-center justify-between gap-4 py-2">
                      <span className="truncate text-sm text-slate-700">{a.nombreArchivo}</span>
                      <button
                        type="button"
                        onClick={() => borrarArchivoExistente(a.id)}
                        className="shrink-0 rounded-lg border border-red-200 px-3 py-1 text-sm font-medium text-red-600 transition hover:bg-red-50"
                      >
                        Eliminar
                      </button>
                    </li>
                  ))}
                </ul>
              )}
              <p className="mt-3 text-xs text-slate-400">
                Para agregar archivos nuevos, crea el lead desde cero (la edición no añade archivos).
              </p>
            </>
          ) : (
            <>
              <input
                type="file"
                multiple
                onChange={(e) => setFiles(Array.from(e.target.files ?? []))}
                className="block w-full text-sm text-slate-600 file:mr-3 file:rounded-lg file:border-0 file:bg-indigo-50 file:px-4 file:py-2 file:text-sm file:font-medium file:text-indigo-700 hover:file:bg-indigo-100"
              />
              {files.length > 0 && (
                <ul className="mt-3 space-y-1 text-sm text-slate-600">
                  {files.map((f, i) => (
                    <li key={i}>• {f.name} <span className="text-slate-400">({f.type || 'desconocido'})</span></li>
                  ))}
                </ul>
              )}
            </>
          )}
        </div>

        <div className="flex items-center gap-3">
          <button
            type="submit"
            disabled={saving}
            className="rounded-lg bg-indigo-600 px-5 py-2.5 text-sm font-semibold text-white transition hover:bg-indigo-700 disabled:opacity-50"
          >
            {saving ? 'Guardando…' : esEdicion ? 'Guardar cambios' : 'Crear lead'}
          </button>
          <Link
            to={esEdicion ? `/leads/${id}` : '/leads'}
            className="rounded-lg border border-slate-300 px-5 py-2.5 text-sm font-medium text-slate-600 transition hover:bg-slate-100"
          >
            Cancelar
          </Link>
        </div>
      </form>
    </div>
  )
}
