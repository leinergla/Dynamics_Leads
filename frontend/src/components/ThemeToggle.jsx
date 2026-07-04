import { useTheme } from '../context/ThemeContext'

/**
 * Botón para alternar entre tema claro y oscuro.
 * - `variant="header"`: sobre el gradiente del header (blanco translúcido).
 * - `variant="plain"`  (por defecto): bordeado y adaptado al tema (p. ej. Login).
 */
export default function ThemeToggle({ variant = 'plain' }) {
  const { isDark, toggleTheme } = useTheme()

  const base =
    'grid h-9 w-9 place-items-center rounded-lg text-lg transition-colors focus:outline-none focus:ring-2 focus:ring-indigo-400'
  const estilos =
    variant === 'header'
      ? 'bg-white/15 text-white hover:bg-white/25'
      : 'border border-slate-300 bg-white text-slate-600 hover:bg-slate-100 dark:border-slate-600 dark:bg-slate-800 dark:text-slate-300 dark:hover:bg-slate-700'

  return (
    <button
      type="button"
      onClick={toggleTheme}
      aria-label={isDark ? 'Cambiar a tema claro' : 'Cambiar a tema oscuro'}
      title={isDark ? 'Tema claro' : 'Tema oscuro'}
      className={`${base} ${estilos}`}
    >
      {isDark ? '☀️' : '🌙'}
    </button>
  )
}
