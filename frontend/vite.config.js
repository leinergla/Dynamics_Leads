import { defineConfig } from 'vite'
import react from '@vitejs/plugin-react'
import tailwindcss from '@tailwindcss/vite'

// https://vite.dev/config/
export default defineConfig({
  plugins: [react(), tailwindcss()],
  server: {
    proxy: {
      // Redirige las llamadas a la API .NET (evita CORS en desarrollo).
      '/api': {
        target: 'http://localhost:5137',
        changeOrigin: true,
      },
    },
  },
})
