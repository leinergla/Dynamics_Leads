import { createContext, useContext, useEffect, useState, useCallback } from 'react'
import * as authApi from '../api/auth'
import { setUnauthorizedHandler } from '../api/leads'

const AuthContext = createContext(null)

export function AuthProvider({ children }) {
  const [user, setUser] = useState(null) // { id, username, email, rol, permisos }
  const [loading, setLoading] = useState(true)

  // Ante un 401 en cualquier petición (token expirado), limpia la sesión:
  // al quedar `user` en null, ProtectedRoute redirige automáticamente a /login.
  useEffect(() => {
    setUnauthorizedHandler(() => setUser(null))
    return () => setUnauthorizedHandler(null)
  }, [])

  // Al cargar, intenta recuperar la sesión actual (cookie httpOnly).
  useEffect(() => {
    let cancelado = false
    authApi
      .me()
      .then((u) => !cancelado && setUser(u))
      .catch(() => !cancelado && setUser(null))
      .finally(() => !cancelado && setLoading(false))
    return () => {
      cancelado = true
    }
  }, [])

  const login = useCallback(async (username, password) => {
    await authApi.login(username, password)
    const u = await authApi.me()
    setUser(u)
    return u
  }, [])

  const logout = useCallback(async () => {
    try {
      await authApi.logout()
    } finally {
      setUser(null)
    }
  }, [])

  const hasPermiso = useCallback(
    (codigo) => Boolean(user?.permisos?.includes(codigo)),
    [user],
  )

  const value = { user, loading, login, logout, hasPermiso, isAuth: Boolean(user) }
  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>
}

// eslint-disable-next-line react-refresh/only-export-components
export function useAuth() {
  const ctx = useContext(AuthContext)
  if (!ctx) throw new Error('useAuth debe usarse dentro de <AuthProvider>')
  return ctx
}
