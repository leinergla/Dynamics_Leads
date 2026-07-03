import { Link } from 'react-router-dom'

const features = [
  {
    titulo: 'Campos dinámicos',
    desc: 'Cada formulario define sus propios campos; la consulta los devuelve como objeto dinámico.',
  },
  {
    titulo: 'Archivos por lead',
    desc: 'Los adjuntos se consultan y descargan desde su propio endpoint, sin recargar la lista.',
  },
  {
    titulo: 'Filtro y paginación',
    desc: 'Busca leads por formulario y navega los resultados página a página.',
  },
]

export default function Inicio() {
  return (
    <div className="space-y-12">
      <section className="rounded-2xl bg-gradient-to-br from-indigo-600 to-violet-600 px-8 py-16 text-center text-white shadow-xl">
        <h1 className="text-4xl font-bold tracking-tight sm:text-5xl">
          Gestión de Leads
        </h1>
        <p className="mx-auto mt-4 max-w-2xl text-lg text-indigo-100">
          Consulta los leads capturados desde tus formularios, revisa sus campos y
          descarga los archivos asociados.
        </p>
        <div className="mt-8">
          <Link
            to="/leads"
            className="inline-block rounded-xl bg-white px-6 py-3 font-semibold text-indigo-700 shadow-md transition hover:bg-indigo-50"
          >
            Consultar Leads →
          </Link>
        </div>
      </section>

      <section className="grid gap-6 sm:grid-cols-3">
        {features.map((f) => (
          <div
            key={f.titulo}
            className="rounded-xl border border-slate-200 bg-white p-6 shadow-sm"
          >
            <h3 className="text-lg font-semibold text-slate-900">{f.titulo}</h3>
            <p className="mt-2 text-sm text-slate-500">{f.desc}</p>
          </div>
        ))}
      </section>
    </div>
  )
}
