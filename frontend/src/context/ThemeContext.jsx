import { createContext, useContext, useEffect, useState, useCallback } from 'react'

const ThemeContext = createContext(null)
const STORAGE_KEY = 'theme'

// Tema inicial: lo guardado en localStorage o, si no hay, la preferencia del SO.
function temaInicial() {
  if (typeof window === 'undefined') return 'light'
  const guardado = localStorage.getItem(STORAGE_KEY)
  if (guardado === 'light' || guardado === 'dark') return guardado
  return window.matchMedia?.('(prefers-color-scheme: dark)').matches ? 'dark' : 'light'
}

export function ThemeProvider({ children }) {
  const [theme, setTheme] = useState(temaInicial)

  // Refleja el tema en <html> (clase `dark`) y lo persiste.
  useEffect(() => {
    const root = document.documentElement
    root.classList.toggle('dark', theme === 'dark')
    localStorage.setItem(STORAGE_KEY, theme)
  }, [theme])

  const toggleTheme = useCallback(
    () => setTheme((t) => (t === 'dark' ? 'light' : 'dark')),
    [],
  )

  const value = { theme, toggleTheme, isDark: theme === 'dark' }
  return <ThemeContext.Provider value={value}>{children}</ThemeContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useTheme() {
  const ctx = useContext(ThemeContext)
  if (!ctx) throw new Error('useTheme debe usarse dentro de <ThemeProvider>')
  return ctx
}
