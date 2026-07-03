# Dynamics Leads — Frontend

Frontend en **React (Vite)** para la API de Dynamics Leads. Cubre **login**, gestión de sesión, y las
pantallas de **Inicio**, **Leads** (consultar, crear, editar, detalle, exportar a Excel) y **Usuarios**
(gestión, alta/edición y cambio de contraseña), con estilos en **Tailwind CSS**.

## Stack

- [React 19](https://react.dev/) + [Vite](https://vite.dev/)
- [React Router](https://reactrouter.com/) — enrutado de las pantallas (públicas y protegidas)
- [Tailwind CSS v4](https://tailwindcss.com/) — estilos (plugin `@tailwindcss/vite`)
- [axios](https://axios-http.com/) — llamadas HTTP a la API

## Requisitos

- Node.js 20+ (probado con Node 22)
- La **API** de Dynamics Leads corriendo (por defecto en `http://localhost:5137`)

## Puesta en marcha

```bash
# 1) Instalar dependencias
cd frontend
npm install

# 2) Levantar la API (.NET) en otra terminal, desde la raíz del repo
dotnet run --project src/Dynamics_Leads.Api/Dynamics_Leads.Api.csproj

# 3) Arrancar el frontend en modo desarrollo
npm run dev      # abre http://localhost:5173
```

## Scripts

| Comando           | Descripción                                            |
|-------------------|--------------------------------------------------------|
| `npm run dev`     | Servidor de desarrollo con HMR (http://localhost:5173) |
| `npm run build`   | Build de producción en `dist/`                         |
| `npm run preview` | Sirve localmente el build de producción                |
| `npm run lint`    | Ejecuta ESLint                                          |

## Docker

El frontend se despliega como build estático servido por **Nginx** (`Dockerfile` + `nginx.conf`). Nginx
sirve la SPA (con *fallback* a `index.html` para React Router) y hace **proxy de `/api` al contenedor
del API** (`api:8080`), de modo que todo queda en el **mismo origen** y la cookie httpOnly funciona sin CORS.

Forma parte del `docker-compose.yml` de la raíz (servicio `web`):

```bash
# desde la raíz del repo
docker compose up -d --build
# Frontend en http://localhost:3000  (login: admin / Admin123*)
```

Build/ejecución solo del frontend (sin compose):

```bash
docker build -t dynamics-leads-web ./frontend
docker run -p 3000:80 dynamics-leads-web   # requiere un contenedor 'api' alcanzable en la red
```

> En producción, el build usa rutas `/api` **relativas** (sin `VITE_API_URL`); el proxy de Nginx las
> reenvía al API. Si despliegas el frontend por separado, ajusta el `proxy_pass` de `nginx.conf` o
> define `VITE_API_URL` al compilar.

## Conexión con la API

En **desarrollo**, Vite hace *proxy* de las rutas `/api` hacia la API .NET, evitando problemas de
CORS. La configuración está en `vite.config.js`:

```js
server: {
  proxy: { '/api': { target: 'http://localhost:5137', changeOrigin: true } }
}
```

Si tu API corre en otro puerto, cambia el `target`.

Las variables de entorno se declaran en un `.env` (hay una plantilla `.env.example`):

```bash
cp .env.example .env
```

La única variable es `VITE_API_URL`, la base de la API que el cliente HTTP (`src/api/leads.js`) usa
como `baseURL`; si está vacía, usa ruta relativa (`/api`).

- **Desarrollo** y **docker-compose**: déjala **vacía**. En dev Vite proxya `/api` y en compose lo
  hace Nginx; así la cookie httpOnly es first-party (sin CORS). No pongas una URL absoluta en `.env`,
  porque se carga también en dev y rompería el proxy.
- **Frontend desplegado por separado**: define la base del API, p. ej. `VITE_API_URL=https://mi-api.ejemplo.com`.

> Solo las variables con prefijo `VITE_` se exponen al navegador. `.env` no se versiona (solo `.env.example`).

## Pantallas

| Ruta                | Pantalla         | Acceso             | Descripción                                                |
|---------------------|------------------|--------------------|------------------------------------------------------------|
| `/login`                | Login              | público            | Inicio de sesión (cookie httpOnly).                                  |
| `/`                     | Inicio             | autenticado        | Bienvenida.                                                          |
| `/leads`                | Consultar Leads    | `leads.read`       | Filtro por `formulario`, campos dinámicos, paginación, exportar a Excel. |
| `/leads/nuevo`          | Crear Lead         | `leads.create`     | Alta de lead con campos y archivos.                                  |
| `/leads/:id`            | Detalle Lead       | `leads.read`       | Campos + archivos (con descarga).                                   |
| `/leads/:id/editar`     | Editar Lead        | `leads.update`     | Edición fiel de formulario y campos.                               |
| `/usuarios`             | Usuarios           | `usuarios.manage`  | Lista **paginada** (10/página, servidor) con alta/baja y rol.       |
| `/usuarios/nuevo`       | Crear usuario      | `usuarios.manage`  | Alta de usuario (username + rol + contraseña).                      |
| `/usuarios/:id/editar`  | Editar usuario     | `usuarios.manage`  | Edita email/rol/activo (username y contraseña no se tocan aquí).    |
| `/usuarios/:id/password`| Cambiar contraseña | `usuarios.manage`  | Restablece la contraseña de **otro** usuario (no la propia).        |

### Autenticación

La sesión usa una **cookie httpOnly** emitida por la API (`POST /api/auth/login`). `axios` está configurado
con `withCredentials: true` para enviarla. El `AuthProvider` (`src/context/AuthContext.jsx`) recupera la sesión
con `GET /api/auth/me`, y `ProtectedRoute` exige sesión (y opcionalmente un permiso) por ruta. Los botones y
enlaces se ocultan según `hasPermiso(...)`. Usuario admin inicial por defecto: **admin / Admin123\*** (cambiar).

> Cada lead es un **objeto dinámico**: los campos del formulario varían según el `formulario`, por lo
> que se renderizan genéricamente (ver `camposDinamicos()` en `src/api/leads.js`). Los archivos se
> consultan en un endpoint aparte (`GET /api/Leads/{id}/archivos`).
>
> La pantalla de **crear/editar** está en `src/pages/LeadForm.jsx` (rutas `/leads/nuevo` y
> `/leads/:id/editar`). En edición, los campos se precargan desde `GET /api/Leads/{id}/campos`
> (campos crudos con `orden`/`alias`), por lo que la edición es fiel. La subida de archivos solo
> está disponible al crear; en edición se pueden eliminar los existentes.
>
> **Exportar a Excel**: desde `/leads`, `exportarLeadsAExcel()` (en `src/api/leads.js`) llama a
> `GET /api/Leads/export` (respuesta `blob`) y dispara la descarga del `.xlsx`, respetando el filtro
> por `formulario` activo.
>
> **Usuarios**: la lista (`src/pages/Usuarios.jsx`) es **paginada del lado del servidor** (10 por
> página). El alta/edición está en `src/pages/UsuarioForm.jsx` (en edición, el `username` no se
> modifica y la contraseña no se toca) y el cambio de contraseña de otro usuario en
> `src/pages/CambiarPassword.jsx`. El frontend oculta el acceso al cambio de contraseña salvo para el
> rol `Administrador` y solo sobre usuarios distintos al propio; el backend rechaza cambiar la propia.

## Estructura

```
frontend/
├── vite.config.js              # Plugin de Tailwind + proxy /api (desarrollo)
├── Dockerfile                  # Build estático + Nginx (producción)
├── nginx.conf                  # Fallback SPA + proxy /api -> api:8080
├── index.html
└── src/
    ├── main.jsx                # Punto de entrada + BrowserRouter + AuthProvider
    ├── App.jsx                 # Rutas (públicas y protegidas)
    ├── index.css               # @import "tailwindcss"
    ├── api/
    │   ├── leads.js            # Cliente axios (withCredentials) + utilidades de leads
    │   └── auth.js             # Login/logout/me + usuarios/roles
    ├── context/
    │   └── AuthContext.jsx     # Sesión, permisos, login/logout
    ├── components/
    │   ├── Layout.jsx          # Navegación + usuario + logout (gateado por permiso)
    │   └── ProtectedRoute.jsx  # Exige sesión y, opcionalmente, un permiso
    └── pages/
        ├── Login.jsx
        ├── Inicio.jsx
        ├── ConsultarLeads.jsx  # Lista + filtro + paginación + exportar a Excel
        ├── DetalleLead.jsx
        ├── LeadForm.jsx        # Crear / editar lead
        ├── Usuarios.jsx        # Gestión de usuarios (lista paginada)
        ├── UsuarioForm.jsx     # Crear / editar usuario
        └── CambiarPassword.jsx # Cambiar contraseña de otro usuario
```
