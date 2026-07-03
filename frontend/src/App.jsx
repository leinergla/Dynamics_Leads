import { Routes, Route, Navigate } from 'react-router-dom'
import Layout from './components/Layout'
import ProtectedRoute from './components/ProtectedRoute'
import Login from './pages/Login'
import Inicio from './pages/Inicio'
import ConsultarLeads from './pages/ConsultarLeads'
import DetalleLead from './pages/DetalleLead'
import LeadForm from './pages/LeadForm'
import Usuarios from './pages/Usuarios'
import UsuarioForm from './pages/UsuarioForm'
import CambiarPassword from './pages/CambiarPassword'

export default function App() {
  return (
    <Routes>
      <Route path="/login" element={<Login />} />

      {/* Todo lo demás requiere sesión. */}
      <Route
        element={
          <ProtectedRoute>
            <Layout />
          </ProtectedRoute>
        }
      >
        <Route index element={<Inicio />} />
        <Route path="leads" element={<ProtectedRoute permiso="leads.read"><ConsultarLeads /></ProtectedRoute>} />
        <Route path="leads/nuevo" element={<ProtectedRoute permiso="leads.create"><LeadForm /></ProtectedRoute>} />
        <Route path="leads/:id" element={<ProtectedRoute permiso="leads.read"><DetalleLead /></ProtectedRoute>} />
        <Route path="leads/:id/editar" element={<ProtectedRoute permiso="leads.update"><LeadForm /></ProtectedRoute>} />
        <Route path="usuarios" element={<ProtectedRoute permiso="usuarios.manage"><Usuarios /></ProtectedRoute>} />
        <Route path="usuarios/nuevo" element={<ProtectedRoute permiso="usuarios.manage"><UsuarioForm /></ProtectedRoute>} />
        <Route path="usuarios/:id/editar" element={<ProtectedRoute permiso="usuarios.manage"><UsuarioForm /></ProtectedRoute>} />
        <Route path="usuarios/:id/password" element={<ProtectedRoute permiso="usuarios.manage"><CambiarPassword /></ProtectedRoute>} />
        <Route path="*" element={<Navigate to="/" replace />} />
      </Route>
    </Routes>
  )
}
