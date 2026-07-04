import { NavLink, Outlet, useNavigate } from 'react-router-dom'
import { useAuth } from '../context/AuthContext'
import ThemeToggle from './ThemeToggle'

const linkBase = 'px-4 py-2 rounded-lg text-sm font-medium transition-colors'
const linkClass = ({ isActive }) =>
  isActive
    ? `${linkBase} bg-white/20 text-white`
    : `${linkBase} text-indigo-100 hover:bg-white/10 hover:text-white`

export default function Layout() {
  const { user, logout, hasPermiso } = useAuth()
  const navigate = useNavigate()

  async function onLogout() {
    await logout()
    navigate('/login', { replace: true })
  }

  return (
    <div className="min-h-full bg-slate-50 text-slate-800 dark:bg-slate-900 dark:text-slate-200">
      <header className="bg-gradient-to-r from-indigo-600 to-violet-600 shadow-lg">
        <nav className="mx-auto flex max-w-6xl items-center justify-between px-6 py-4">
          <div className="flex items-center gap-2 text-white">
            <span className="grid h-9 w-9 place-items-center rounded-lg bg-white/20 text-lg font-bold">
              DL
            </span>
            <span className="text-lg font-semibold tracking-tight">Dynamics Leads</span>
          </div>

          <div className="flex items-center gap-1">
            <NavLink to="/" end className={linkClass}>
              Inicio
            </NavLink>
            {hasPermiso('leads.read') && (
              <NavLink to="/leads" className={linkClass}>
                Consultar Leads
              </NavLink>
            )}
            {hasPermiso('usuarios.manage') && (
              <NavLink to="/usuarios" className={linkClass}>
                Usuarios
              </NavLink>
            )}
          </div>

          <div className="flex items-center gap-3 text-white">
            <span className="hidden text-sm sm:inline">
              {user?.username}
              <span className="ml-1 text-indigo-200">({user?.rol})</span>
            </span>
            <ThemeToggle variant="header" />
            <button
              onClick={onLogout}
              className="rounded-lg bg-white/15 px-3 py-1.5 text-sm font-medium text-white transition hover:bg-white/25"
            >
              Salir
            </button>
          </div>
        </nav>
      </header>

      <main className="mx-auto max-w-6xl px-6 py-8">
        <Outlet />
      </main>

      <footer className="mx-auto max-w-6xl px-6 py-8 text-center text-sm text-slate-400 dark:text-slate-500">
        Dynamics Leads API · Frontend de demostración
      </footer>
    </div>
  )
}
