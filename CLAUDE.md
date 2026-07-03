# Dynamics Leads

Solución para registrar y consultar leads (con campos y archivos). Consta de:

- **API REST** en **.NET 10**, **Clean Architecture (Onion)** por capas con **inversión de dependencias**, **DI / IoC**, **Dapper** sobre **PostgreSQL**, documentación **OpenAPI + Scalar**.
- **Frontend** en **React (Vite) + Tailwind CSS** (carpeta `frontend/`), ver [Frontend](#frontend).

## Stack (API)

| Componente        | Versión / Paquete                                      |
|-------------------|--------------------------------------------------------|
| Framework         | .NET 10 (`net10.0`), ASP.NET Core Web API (controllers)|
| Base de datos     | PostgreSQL (BD `dynamics_leads`)                       |
| Acceso a datos    | Dapper 2.1.x + Npgsql 10.x                             |
| Persistencia      | Rutinas de BD: procedimientos (escritura) + funciones (lectura) |
| Exportación Excel | ClosedXML 0.105.x (`.xlsx`)                            |
| Seguridad         | JWT (`System.IdentityModel.Tokens.Jwt`) + PBKDF2 (`Microsoft.AspNetCore.Cryptography.KeyDerivation`) |
| Documentación API | `Microsoft.AspNetCore.OpenApi` + `Scalar.AspNetCore`   |
| Solución          | Formato `.slnx`                                        |

## Estructura

```
dynamics-leads/
├── dynamics_leads.slnx
├── CLAUDE.md
├── Dockerfile                            # Imagen del API (build multi-etapa)
├── docker-compose.yml                    # API + PostgreSQL (init de BD incluido)
├── .dockerignore
├── .env.example                          # Variables para docker-compose
├── postman/                              # Colección de Postman importable
├── db/
│   └── init.sql                          # Script único: BD + tablas + datos iniciales + rutinas (plpgsql)
├── frontend/                             # Frontend React (Vite) + Tailwind — ver sección Frontend
│   ├── Dockerfile                        # Build estático + Nginx (sirve SPA y proxya /api)
│   └── nginx.conf                        # Config de Nginx (fallback SPA + proxy a api:8080)
└── src/
    ├── Dynamics_Leads.Domain/            # Núcleo: entidades y contratos (sin dependencias)
    │   ├── Entities/Lead.cs
    │   ├── Entities/Archivo.cs
    │   ├── Entities/Usuario.cs           # Auth: usuario
    │   ├── Entities/Rol.cs               # Auth: rol
    │   ├── Repositories/ILeadRepository.cs
    │   ├── Repositories/IUsuarioRepository.cs
    │   └── Repositories/IRolRepository.cs
    ├── Dynamics_Leads.Application/       # Lógica de negocio
    │   ├── DTOs/CreateLeadRequest.cs
    │   ├── DTOs/UpdateLeadRequest.cs     # Entrada de actualización (formulario + campos)
    │   ├── DTOs/CampoDTO.cs
    │   ├── DTOs/ArchivoDTO.cs            # Entrada de archivo (Base64)
    │   ├── DTOs/ArchivoResponse.cs       # Salida de archivo (con URL)
    │   ├── DTOs/ArchivoContenido.cs      # Stream para descarga
    │   ├── DTOs/LeadResponse.cs          # Respuesta de creación
    │   ├── DTOs/PagedResult.cs           # Resultado paginado genérico
    │   ├── DTOs/Auth/                    # Login, usuarios, roles (request/response)
    │   ├── Auth/Permisos.cs              # Constantes de permisos + claim type
    │   ├── Storage/IArchivoStorage.cs    # Abstracción de almacenamiento
    │   ├── Export/IExcelExporter.cs      # Abstracción de exportación a Excel
    │   ├── Security/IPasswordHasher.cs   # Abstracción de hashing de contraseñas
    │   ├── Security/IJwtTokenGenerator.cs# Abstracción de generación de JWT
    │   ├── Services/ILeadService.cs
    │   ├── Services/LeadService.cs
    │   ├── Services/IAuthService.cs
    │   ├── Services/AuthService.cs
    │   ├── Services/IUsuarioService.cs
    │   ├── Services/UsuarioService.cs
    │   └── DependencyInjection.cs        # AddApplication()
    ├── Dynamics_Leads.Infrastructure/    # Acceso a datos (Dapper), almacenamiento, export y seguridad
    │   ├── Configuration/DatabaseOptions.cs
    │   ├── Configuration/ArchivosOptions.cs
    │   ├── Configuration/JwtOptions.cs
    │   ├── Persistence/IDbConnectionFactory.cs
    │   ├── Persistence/NpgsqlConnectionFactory.cs
    │   ├── Repositories/LeadRepository.cs        # Solo rutinas de BD (CALL / fn_...)
    │   ├── Repositories/UsuarioRepository.cs     # Auth: rutinas de BD
    │   ├── Repositories/RolRepository.cs         # Auth: rutinas de BD
    │   ├── Storage/FileSystemArchivoStorage.cs   # Guarda/abre archivos en disco
    │   ├── Export/ClosedXmlExcelExporter.cs      # Genera el .xlsx con ClosedXML
    │   ├── Security/Pbkdf2PasswordHasher.cs      # Hash PBKDF2-HMACSHA256
    │   ├── Security/JwtTokenGenerator.cs         # Firma el JWT (HS256)
    │   └── DependencyInjection.cs        # AddInfrastructure(configuration)
    └── Dynamics_Leads.Api/               # Capa de presentación (controllers)
        ├── Controllers/LeadsController.cs
        ├── Controllers/AuthController.cs         # login / logout / me
        ├── Controllers/UsuariosController.cs     # CRUD de usuarios
        ├── Controllers/RolesController.cs        # Lista de roles
        ├── Middleware/ExceptionHandlingMiddleware.cs
        ├── Program.cs
        └── appsettings.json
```

## Arquitectura Clean (Onion) — flujo de dependencias

Arquitectura en capas con **inversión de dependencias**: la **regla de dependencia apunta hacia el núcleo** (`Domain`). No es una N-capas (N-tier) clásica —en la que cada capa dependería de la de abajo hasta el acceso a datos—: aquí `Infrastructure` (acceso a datos) **depende hacia adentro** e *implementa* las abstracciones definidas en el núcleo (p. ej. `ILeadRepository` se define en `Domain` y se implementa en `Infrastructure`).

```
Api ──► Application ──► Domain  (núcleo, sin dependencias)
 │                        ▲
 └──► Infrastructure ─────┘     (implementa las abstracciones del núcleo)
```

- **Domain**: entidades (`Lead`, `Archivo`, `Usuario`, `Rol`) y abstracciones (`ILeadRepository`, `IUsuarioRepository`, `IRolRepository`). No depende de nada.
- **Application**: DTOs, abstracciones (`IArchivoStorage`, `IExcelExporter`, `IPasswordHasher`, `IJwtTokenGenerator`) y lógica de negocio (`LeadService`, `AuthService`, `UsuarioService`). Depende solo de Domain.
- **Infrastructure**: implementación con Dapper (`LeadRepository`, `NpgsqlConnectionFactory`), almacenamiento de archivos (`FileSystemArchivoStorage`), exportación a Excel (`ClosedXmlExcelExporter`) y seguridad (`Pbkdf2PasswordHasher`, `JwtTokenGenerator`). Depende de Application/Domain.
- **Api**: controladores, middleware y composición. Referencia Application + Infrastructure.

### Inversión de Control / DI

El registro se hace mediante métodos de extensión, invocados en `Program.cs`:

```csharp
builder.Services.AddApplication();                        // ILeadService/IAuthService/IUsuarioService -> *Service (Scoped)
builder.Services.AddInfrastructure(builder.Configuration);// ILeadRepository -> LeadRepository (Scoped)
                                                          // IUsuarioRepository/IRolRepository -> *Repository (Scoped)
                                                          // IDbConnectionFactory -> NpgsqlConnectionFactory (Singleton)
                                                          // IArchivoStorage  -> FileSystemArchivoStorage (Singleton)
                                                          // IExcelExporter   -> ClosedXmlExcelExporter (Singleton)
                                                          // IPasswordHasher  -> Pbkdf2PasswordHasher (Singleton)
                                                          // IJwtTokenGenerator -> JwtTokenGenerator (Singleton)
```

Cadena de resolución: `LeadsController → ILeadService → { ILeadRepository → IDbConnectionFactory, IArchivoStorage, IExcelExporter }`.

> Dapper usa `DefaultTypeMap.MatchNamesWithUnderscores = true` (activado en `AddInfrastructure`) para mapear columnas snake_case (`fecha_creacion`, `nombre_archivo`…) a propiedades PascalCase.

## Base de datos

### Tabla `public.leads`

```sql
leadid          uuid PRIMARY KEY DEFAULT gen_random_uuid()
formulario      varchar(255) NOT NULL          -- índice ix_leads_formulario
datos           jsonb NOT NULL                 -- array de campos del formulario
fecha_creacion  timestamp DEFAULT CURRENT_TIMESTAMP  -- índice ix_leads_fecha_creacion DESC
```

### Tabla `public.lead_archivos`

Cada archivo es una fila relacionada con el lead. **Solo se guarda la referencia (`storage_key`), nunca el binario.**

```sql
id             uuid PRIMARY KEY DEFAULT gen_random_uuid()
leadid         uuid NOT NULL  -- FK a leads(leadid) ON DELETE CASCADE
nombre_archivo varchar(255) NOT NULL
storage_key    varchar(1024) NOT NULL  -- ruta relativa, p. ej. 2026/06/«guid»_cv.txt
nombre_campo   varchar(255)
content_type   varchar(255)
tamano         bigint NOT NULL DEFAULT 0
fecha_creacion timestamp DEFAULT CURRENT_TIMESTAMP
```

Al borrar un lead se eliminan en cascada sus archivos en BD (el binario en disco no se borra automáticamente — ver *Pendientes*).

### Rutinas de base de datos

**`LeadRepository` no contiene SQL DML/DQL en línea**: todo el acceso a datos pasa por rutinas de BD. Las **escrituras** usan procedimientos (`CALL`, resultado vía parámetro `INOUT`); las **lecturas** usan funciones `RETURNS TABLE`/escalares (un `PROCEDURE` no devuelve result sets de forma limpia a Dapper), invocadas con `SELECT ... FROM fn_...`. Todas las rutinas están en **`LANGUAGE plpgsql`** y definidas en `db/init.sql`.

Procedimientos (escritura):

- `sp_insert_lead(p_formulario, p_datos jsonb, INOUT p_leadid uuid)`
- `sp_insert_lead_archivo(p_leadid, p_nombre_archivo, p_storage_key, p_nombre_campo, p_content_type, p_tamano, INOUT p_id uuid)`
- `sp_update_lead(p_leadid, p_formulario, p_datos jsonb, INOUT p_actualizado boolean)`
- `sp_delete_lead(p_leadid, INOUT p_eliminado boolean)`
- `sp_delete_lead_archivo(p_archivoid, INOUT p_eliminado boolean)`

Funciones (lectura):

- `fn_list_leads(p_formulario, p_offset, p_limit)` → `TABLE`
- `fn_count_leads(p_formulario)` → `bigint`
- `fn_get_lead(p_leadid)` → `TABLE`
- `fn_get_archivos_by_leads(p_leadids uuid[])` → `TABLE`
- `fn_get_archivo(p_archivoid)` → `TABLE`
- `fn_list_formularios()` → `TABLE` (`SELECT DISTINCT formulario ... ORDER BY formulario`)

Los procedimientos devuelven su resultado por `INOUT`, leído con `QuerySingleAsync<Guid>`/`<bool>` sobre `CALL ...`. La inserción del lead y sus archivos ocurre dentro de **una transacción** en `LeadRepository`. Las funciones de lectura devuelven `datos` como `text` (`datos::text`) para mapearlo a `string` con Dapper.

## Endpoints

| Método | Ruta                              | Descripción                                   | Respuestas              |
|--------|-----------------------------------|-----------------------------------------------|-------------------------|
| POST   | `/api/Leads`                      | Crea un lead (campos + archivos)              | `201`, `400`, `500`     |
| GET    | `/api/Leads?formulario=&page=&pageSize=` | Lista paginada (orden por fecha desc)  | `200`                   |
| GET    | `/api/Leads/formularios`          | Lista los formularios distintos (para dropdowns/filtros) | `200`  |
| GET    | `/api/Leads/export?formulario=`   | Exporta a Excel (`.xlsx`) los leads, opcionalmente filtrados por formulario | `200` |
| GET    | `/api/Leads/{id}`                 | Detalle de un lead                            | `200`, `404`            |
| GET    | `/api/Leads/{id}/archivos`        | Lista los archivos de un lead                 | `200`                   |
| GET    | `/api/Leads/{id}/campos`          | Campos crudos del lead (nombre, valor, orden, alias) — para edición fiel | `200`, `404` |
| PUT    | `/api/Leads/{id}`                 | Actualiza formulario y campos (devuelve el lead actualizado) | `200`, `400`, `404` |
| DELETE | `/api/Leads/{id}`                 | Elimina el lead, sus archivos (cascada) y los binarios en disco | `204`, `404` |
| GET    | `/api/Leads/archivos/{archivoId}` | Descarga el binario de un archivo             | `200`, `404`            |
| DELETE | `/api/Leads/archivos/{archivoId}` | Elimina un archivo individual (fila + binario)| `204`, `404`            |

> Todos los endpoints de leads requieren **autenticación** y el **permiso** correspondiente (ver [Seguridad](#seguridad-autenticación-y-autorización)): `GET → leads.read`, `POST → leads.create`, `PUT → leads.update`, `DELETE → leads.delete`. Sin sesión devuelven `401`; con sesión pero sin permiso, `403`.

### POST — crear lead

```json
{
  "formulario": "contacto",
  "datos": [
    { "nombre": "email", "valor": "leiner@demo.com", "orden": 1, "alias": "correo" }
  ],
  "archivos": [
    { "nombreArchivo": "cv.txt", "nombreCampo": "adjunto_cv", "contentType": "text/plain", "contenidoBase64": "SG9sYS4uLg==" }
  ]
}
```

- **`formulario`** (`string`, obligatorio, máx. 255).
- **`datos`** (`List<CampoDTO>`, obligatorio, mín. 1): `nombre`, `valor`, `orden`, `alias`. Se guardan en el jsonb `datos`.
- **`archivos`** (`List<ArchivoDTO>`, opcional): `nombreArchivo`, `nombreCampo`, `contentType` (opcional), `contenidoBase64`. Se guardan en disco + tabla `lead_archivos`.

Respuesta `201`: `{ "leadId": "f47ac10b-..." }` (con header `Location` al detalle).

### GET — listar / detalle (objeto dinámico)

Cada lead se devuelve como un **objeto dinámico**: los campos del formulario se "aplanan" al nivel raíz (clave = `nombre` del campo, valor = `valor`), junto a los metadatos. Como cada formulario tiene campos distintos, **las claves varían por lead**. **Los archivos NO se incluyen aquí** — se consultan en `GET /api/Leads/{id}/archivos`.

```json
[
  {
    "leadId": "4a45843d-...",
    "formulario": "contacto",
    "fechaCreacion": "2026-06-13T11:02:05.29",
    "email": "a@b.com",
    "ciudad": "Bogota"
  },
  {
    "leadId": "4d4bbaec-...",
    "formulario": "soporte",
    "fechaCreacion": "2026-06-13T11:02:05.37",
    "asunto": "falla login",
    "prioridad": "alta"
  }
]
```

- Claves **reservadas**: `leadId`, `formulario`, `fechaCreacion`. Prevalecen sobre cualquier campo del formulario que se llame igual (ese campo se omite del aplanado para no romper los metadatos).
- Los campos se ordenan por su `orden`. Solo se incluye `nombre → valor` (el `alias`/`orden` no aparecen en la salida plana).
- Implementado como `Dictionary<string, object?>` por lead. La lista va envuelta en `PagedResult<T>`: `{ items, page, pageSize, total, totalPages }`. El detalle (`GET /{id}`) devuelve un único objeto dinámico.

> Consumo en React: cada item es un objeto plano; usa `Object.entries(item)` (excluyendo las claves reservadas) para renderizar dinámicamente las columnas/campos de cada formulario. Para los archivos de un lead, llama aparte a `GET /api/Leads/{id}/archivos`.

Como la salida plana **pierde `orden`/`alias`**, para **editar fielmente** un lead existe `GET /api/Leads/{id}/campos`, que devuelve los campos crudos (`nombre`, `valor`, `orden`, `alias`) ordenados por `orden`. El formulario de edición del frontend usa este endpoint para precargar los campos sin perder información.

### GET — archivos de un lead

`GET /api/Leads/{id}/archivos` devuelve la lista de archivos del lead (vacía si no tiene). Cada elemento es un `ArchivoResponse`:

```json
[
  { "id": "72ab8e04-...", "nombreArchivo": "cv.txt", "nombreCampo": "cv",
    "contentType": "text/plain", "tamano": 2, "url": "/api/leads/archivos/72ab8e04-..." }
]
```

### GET — exportar a Excel

`GET /api/Leads/export?formulario=` genera y descarga un archivo `.xlsx` (content-type `application/vnd.openxmlformats-officedocument.spreadsheetml.sheet`) con los leads, opcionalmente filtrados por `formulario` (sin filtro, exporta todos). Requiere el permiso `leads.read`. El nombre del archivo tiene el patrón `leads_{formulario|todos}_{yyyy-MM-dd}.xlsx`. La generación del libro se hace en `LeadService.ExportarLeadsAsync` mediante la abstracción `IExcelExporter` (implementada con **ClosedXML** en `ClosedXmlExcelExporter`).

### PUT — actualizar lead

Body `UpdateLeadRequest` (igual que el POST pero sin archivos):

```json
{
  "formulario": "form_actualizado",
  "datos": [
    { "nombre": "email", "valor": "nuevo@y.com", "orden": 1, "alias": "correo" }
  ]
}
```

Reemplaza por completo `formulario` y `datos` (campos). **No modifica los archivos**, que se gestionan con los endpoints `POST` (al crear) y `DELETE /api/Leads/archivos/{id}`. Responde `200` con el objeto dinámico del lead actualizado, o `404` si el lead no existe.

### Consumo desde React

- Filtrar por formulario: `GET /api/Leads?formulario=contacto&page=1&pageSize=20`.
- Para cada archivo, usar `archivo.url` (relativa) anteponiendo la base de la API: `https://host{archivo.url}`. La API **nunca** expone rutas de disco.

## Seguridad (autenticación y autorización)

Autenticación por **JWT transportado en una cookie httpOnly** (no accesible desde JS, mitiga XSS). Autorización **por permisos**, agrupados en **roles** (un rol por usuario).

### Modelo

- Tablas: `roles`, `permisos`, `rol_permisos` (N:N), `usuarios` (con `rol_id`). Ver `db/init.sql`.
- **Roles sembrados**: `Administrador` (todos los permisos) y `Editor` (`leads.read`, `leads.create`, `leads.update`).
- **Permisos**: `leads.read`, `leads.create`, `leads.update`, `leads.delete`, `usuarios.manage`.
- Contraseñas hasheadas con **PBKDF2-HMACSHA256** (`Pbkdf2PasswordHasher`), formato `iteraciones.salt.hash`.
- El **JWT** incluye `sub` (id), `name`, `role` y un claim `permiso` por cada permiso. Se firma HS256 con `Jwt:Key`.
- Al arrancar, si no hay usuarios, se crea un **admin inicial** (config `Seed:AdminUsername`/`AdminPassword`, por defecto `admin` / `Admin123*`). Cambiar en producción.

### Endpoints de auth/usuarios

| Método | Ruta                  | Descripción                                  | Acceso             |
|--------|-----------------------|----------------------------------------------|--------------------|
| POST   | `/api/auth/login`     | Valida credenciales y setea la cookie httpOnly | anónimo          |
| POST   | `/api/auth/logout`    | Borra la cookie                              | anónimo            |
| GET    | `/api/auth/me`        | Usuario actual + rol + permisos              | autenticado        |
| GET    | `/api/usuarios?page=&pageSize=` | Lista **paginada** de usuarios (orden por `username`) | `usuarios.manage`  |
| GET    | `/api/usuarios/{id}`  | Detalle de un usuario (`200`/`404`)          | `usuarios.manage`  |
| POST   | `/api/usuarios`       | Crea usuario                                 | `usuarios.manage`  |
| PUT    | `/api/usuarios/{id}`  | Actualiza email/rol/activo                   | `usuarios.manage`  |
| PUT    | `/api/usuarios/{id}/password` | Restablece la contraseña de **otro** usuario (no la propia) | `usuarios.manage`  |
| DELETE | `/api/usuarios/{id}`  | Elimina usuario                              | `usuarios.manage`  |
| GET    | `/api/roles`          | Lista roles                                  | `usuarios.manage`  |

- **`GET /api/usuarios`**: paginado del lado del servidor con `PagedResult<UsuarioResponse>` (`{ items, page, pageSize, total, totalPages }`), igual que el listado de leads. Defaults `page=1`, `pageSize=10` (el servidor aplica `page < 1 → 1` y `Math.Clamp(pageSize, 1, 100)`). El orden es por `username`.
- **`PUT /api/usuarios/{id}` ya no cambia la contraseña**: el cambio de contraseña se hace por el endpoint dedicado `PUT /api/usuarios/{id}/password` (body `{ password }`, mín. 6). Regla de negocio: **no puedes cambiar tu propia contraseña por esta vía** — si el `id` coincide con el `sub` del token, responde `400`; si el usuario no existe, `404`; si ok, `204`. Como el permiso `usuarios.manage` solo lo tiene el rol `Administrador` (por seed), en la práctica solo un administrador puede restablecer la contraseña de otro usuario.

### Cómo funciona

- `AddJwtBearer` lee el token desde la cookie (`OnMessageReceived` → `Request.Cookies[Jwt:CookieName]`), no del header `Authorization`.
- Se registra **una política por permiso**; los endpoints usan `[Authorize(Policy = Permisos.LeadsRead)]`, etc.
- Sin sesión → `401`; con sesión pero sin permiso → `403`.
- Acceso a datos (usuarios/roles) por **rutinas de BD** (`fn_*`, `sp_*` en `db/init.sql`), como el resto del proyecto.

### Configuración (`appsettings.json`)

```json
"Jwt": {
  "Key": "...mínimo 32 caracteres...",
  "Issuer": "dynamics_leads",
  "Audience": "dynamics_leads",
  "ExpiryMinutes": 120,
  "CookieName": "access_token",
  "CookieSecure": false
},
"Seed": { "AdminUsername": "admin", "AdminPassword": "Admin123*" }
```

> En producción: usar una `Jwt:Key` fuerte (secreto), `CookieSecure=true` (HTTPS), y cambiar las credenciales del admin. Para frontend cross-origin con cookies, configurar `Cors:AllowedOrigins` (no usar `AllowAnyOrigin` con credenciales).

## Manejo de archivos

- El contenido llega en Base64; el servidor lo decodifica y lo guarda en disco bajo `Archivos/yyyy/MM/«guid»_«nombre»` (nombre saneado para evitar *path traversal*).
- Validaciones en el almacenamiento: tamaño máximo (`TamanoMaximoBytes`) y extensiones (`ExtensionesPermitidas`, vacío = todas). Si fallan, lanzan `ArgumentException` → HTTP 400.
- En BD se guarda solo `storage_key` (ruta relativa). La descarga (`GET /api/Leads/archivos/{id}`) abre el stream y devuelve el archivo con su `content_type` y nombre original.
- Al eliminar un lead (`DELETE /api/Leads/{id}`), el `ON DELETE CASCADE` borra las filas de `lead_archivos` y el servicio elimina además los **binarios en disco** (best-effort: si falla el borrado de un binario se registra en log pero no revierte el borrado en BD).

## Manejo de errores

`ExceptionHandlingMiddleware` traduce excepciones a `ProblemDetails`:

- `ArgumentException` → **400**
- `KeyNotFoundException` / `FileNotFoundException` → **404**
- Resto → **500** (se registra en log; no expone detalles).

## CORS

Política `FrontendCors` aplicada globalmente. Si `Cors:AllowedOrigins` está vacío, es permisiva (cualquier origen) — útil en desarrollo. En producción, listar los orígenes del frontend React.

## Documentación de la API (solo en Development)

- OpenAPI JSON: `GET /openapi/v1.json`
- UI Scalar: `GET /scalar/v1`

## Configuración (`appsettings.json`)

```json
"Database": {
  "ConnectionString": "Host=localhost;Port=5432;Database=dynamics_leads;Username=postgres;Password=postgres"
},
"Archivos": {
  "BasePath": "Archivos",
  "TamanoMaximoBytes": 10485760,
  "ExtensionesPermitidas": []
},
"Cors": {
  "AllowedOrigins": []
}
```

- **`Database:ConnectionString`**: se valida al arrancar (`ValidateOnStart`). En producción, usar *user-secrets* o variables de entorno.
- **`Archivos:BasePath`**: carpeta base (relativa al directorio de trabajo, o absoluta). Se crea automáticamente.
- **`Archivos:TamanoMaximoBytes`**: tamaño máximo por archivo (por defecto 10 MB).
- **`Archivos:ExtensionesPermitidas`**: lista blanca de extensiones (con punto, p. ej. `".pdf"`). Vacío = todas.
- **`Cors:AllowedOrigins`**: orígenes permitidos para el frontend.

## Comandos

```bash
# Compilar
dotnet build dynamics_leads.slnx

# Ejecutar la API
dotnet run --project src/Dynamics_Leads.Api/Dynamics_Leads.Api.csproj

# Inicializar la BD (crea 'dynamics_leads' + tablas + datos iniciales + rutinas)
# Conéctate a la BD 'postgres'; el script crea 'dynamics_leads' si no existe.
# Es idempotente: puede reejecutarse sin error.
psql -h localhost -U postgres -d postgres -f db/init.sql
```

## Docker (API)

`Dockerfile` en la raíz: build multi-etapa (SDK 10.0 → publish Release → imagen `aspnet:10.0`). La app corre como usuario no root y escucha en el **puerto 8080** del contenedor.

En **Producción** (`ASPNETCORE_ENVIRONMENT=Production`, por defecto en la imagen), `appsettings.Production.json` deja vacíos los secretos para forzar que vengan de **variables de entorno** (ASP.NET Core mapea `:` a `__`):

| Config                       | Variable de entorno            |
|------------------------------|--------------------------------|
| `Database:ConnectionString`  | `Database__ConnectionString`   |
| `Jwt:Key`                    | `Jwt__Key`                     |

Ambos se validan al arrancar (`ValidateOnStart`): si faltan, la app no inicia. En Producción, `Jwt:CookieSecure=true` (requiere HTTPS, normalmente terminado en un reverse proxy) y los archivos se guardan en `/app/Archivos` (volumen).

```bash
# Construir
docker build -t dynamics-leads-api:latest .

# Ejecutar (host.docker.internal apunta a la BD del host; ajusta según tu despliegue)
docker run -d --name dynamics-leads-api -p 8080:8080 \
  -e Database__ConnectionString="Host=host.docker.internal;Port=5432;Database=dynamics_leads;Username=postgres;Password=***" \
  -e Jwt__Key="una_clave_secreta_de_al_menos_32_caracteres" \
  -v dynamics_leads_archivos:/app/Archivos \
  dynamics-leads-api:latest
```

Otras variables opcionales: `Seed__AdminUsername`, `Seed__AdminPassword`, `Cors__AllowedOrigins__0` (orígenes del frontend), `Archivos__BasePath`.

### docker-compose (web + API + PostgreSQL)

`docker-compose.yml` levanta tres servicios ya cableados:

| Servicio | Imagen | Puerto host | Descripción |
|----------|--------|-------------|-------------|
| `web`    | Nginx + SPA (`frontend/Dockerfile`) | **3000** → 80 | Sirve el frontend y hace **proxy de `/api` → `api:8080`** |
| `api`    | `Dockerfile` (.NET 10) | **8080** | API REST |
| `db`     | `postgres:16` | **5433** → 5432 | PostgreSQL (init de BD incluido) |

La BD ejecuta `db/init.sql` mediante `/docker-entrypoint-initdb.d` (solo en el primer arranque, con el volumen vacío). Configurable con un archivo `.env` (ver `.env.example`): `POSTGRES_PASSWORD`, `JWT_KEY`, `JWT_COOKIE_SECURE`, `ADMIN_USERNAME`, `ADMIN_PASSWORD`.

```bash
cp .env.example .env      # ajusta los valores
docker compose up -d --build
# Frontend: http://localhost:3000  (todo el tráfico pasa por aquí; Nginx proxya /api)
# API directa (opcional): http://localhost:8080  ·  BD: localhost:5433
docker compose logs -f web api
docker compose down       # añade -v para borrar también los volúmenes (datos + archivos)
```

- **Mismo origen**: el navegador habla solo con `web` (puerto 3000); Nginx reenvía `/api` al contenedor `api`. Así la **cookie httpOnly es first-party** y no hace falta CORS.
- El API se conecta a la BD por la red interna (`Host=db;Port=5432`); el host mapea la BD a **5433** para no chocar con un Postgres local en 5432.
- En local (HTTP) `JWT_COOKIE_SECURE=false`; detrás de HTTPS, ponlo en `true`.
- Los binarios subidos persisten en el volumen `archivos`; los datos de la BD en `pgdata`.
- El frontend de producción llama a `/api` por ruta relativa (sin `VITE_API_URL`), por lo que el proxy de Nginx lo resuelve.

## Frontend

Aplicación **React (Vite) + Tailwind CSS** en `frontend/`, con login, gestión de sesión y pantallas protegidas. Ver `frontend/README.md` para el detalle completo.

- **Stack**: React 19, Vite, React Router, Tailwind CSS v4 (`@tailwindcss/vite`), axios.
- **Autenticación**: `AuthProvider` (contexto) recupera la sesión con `GET /api/auth/me`; `axios` usa `withCredentials` para enviar la cookie httpOnly. `ProtectedRoute` exige sesión y, opcionalmente, un permiso. Los botones/menús se ocultan según `hasPermiso(...)`.
- **Pantallas / rutas**:
  - `/login` — Inicio de sesión.
  - `/` — Inicio.
  - `/leads` — Consultar Leads (requiere `leads.read`; filtro por `formulario`, paginación, exportación a Excel).
  - `/leads/nuevo`, `/leads/:id/editar` — Crear/editar (requieren `leads.create` / `leads.update`).
  - `/leads/:id` — Detalle Lead (`leads.read`; campos + archivos con descarga).
  - `/usuarios` — Gestión de usuarios: lista **paginada** (10 por página, servidor) (requiere `usuarios.manage`).
  - `/usuarios/nuevo`, `/usuarios/:id/editar` — Crear/editar usuario en pantalla propia (`usuarios.manage`). Al editar, el `username` no se modifica y la contraseña **no** se toca aquí.
  - `/usuarios/:id/password` — Cambiar contraseña de otro usuario en pantalla propia (`usuarios.manage`). El frontend oculta el acceso salvo para rol `Administrador` y solo sobre usuarios distintos al propio; el backend rechaza el cambio sobre uno mismo.
- **Conexión con la API**: en desarrollo, Vite hace *proxy* de `/api` → `http://localhost:5137` (sin CORS); ver `frontend/vite.config.js`. En producción se usa la variable `VITE_API_URL` como base.
- **Campos dinámicos**: cada lead es un objeto plano; `camposDinamicos()` (en `src/api/leads.js`) filtra las claves reservadas y renderiza el resto genéricamente. Los archivos se consultan aparte vía `GET /api/Leads/{id}/archivos`.
- **Exportar a Excel**: `exportarLeadsAExcel()` (en `src/api/leads.js`) llama a `GET /api/Leads/export` (respuesta `blob`) y dispara la descarga del `.xlsx` en el navegador.

```bash
cd frontend
npm install
npm run dev      # http://localhost:5173 (requiere la API corriendo)
npm run build    # build de producción en dist/
```

## Postman

Colección importable en `postman/Dynamics_Leads.postman_collection.json` con todos los endpoints, variables (`baseUrl`, `leadId`, `archivoId`, `formulario`) y scripts que capturan automáticamente el `leadId`/`archivoId`.

## Pasos pendientes / mejoras

1. Inicializar la BD ejecutando `db/init.sql` (crea la BD, tablas, datos iniciales y rutinas). En docker-compose se aplica solo en el primer arranque.
2. Ajustar `Database:ConnectionString` (contraseña real).
3. Configurar `Archivos:BasePath` y permisos de escritura; `Cors:AllowedOrigins` para React.
4. **Almacenamiento en disco local** no se comparte entre instancias; para producción evaluar almacenamiento de objetos (S3 / Azure Blob / MinIO) detrás de `IArchivoStorage`.
5. **Base64 en JSON** carga el archivo completo en memoria (+33% de tamaño); para archivos grandes evaluar subida `multipart/form-data` con streaming.
```
