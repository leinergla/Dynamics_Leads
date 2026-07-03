import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'

/**
 * Protege rutas: exige sesión y, opcionalmente, un permiso concreto.
 * - Sin sesión → redirige a /login (recordando el destino).
 * - Con sesión pero sin el permiso → muestra aviso de acceso denegado.
 */
export default function ProtectedRoute({ children, permiso }) {
  const { isAuth, loading, hasPermiso } = useAuth()
  const location = useLocation()

  if (loading) {
    return <div className="grid min-h-screen place-items-center text-slate-400">Cargando…</div>
  }

  if (!isAuth) {
    return <Navigate to="/login" replace state={{ from: location }} />
  }

  if (permiso && !hasPermiso(permiso)) {
    return (
      <div className="mx-auto max-w-md py-20 text-center">
        <h2 className="text-lg font-semibold text-slate-900">Acceso denegado</h2>
        <p className="mt-2 text-sm text-slate-500">
          No tienes permiso para ver esta sección.
        </p>
      </div>
    )
  }

  return children
}
