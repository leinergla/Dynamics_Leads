# Dynamics Leads — Manual de usuario

Dynamics Leads es una aplicación web para **registrar, consultar, editar y exportar leads** (formularios con campos dinámicos y archivos adjuntos), con **inicio de sesión** y **gestión de usuarios** por permisos.

Este documento es el **manual de usuario**. Para la descripción técnica (arquitectura, API, base de datos, despliegue), consulta [CLAUDE.md](./CLAUDE.md).

---

## Índice

1. [Acceso a la aplicación](#1-acceso-a-la-aplicación)
2. [Iniciar sesión](#2-iniciar-sesión)
3. [Roles y permisos](#3-roles-y-permisos)
4. [Consultar leads](#4-consultar-leads)
5. [Ver el detalle de un lead](#5-ver-el-detalle-de-un-lead)
6. [Crear un lead](#6-crear-un-lead)
7. [Editar un lead](#7-editar-un-lead)
8. [Eliminar un lead](#8-eliminar-un-lead)
9. [Archivos adjuntos](#9-archivos-adjuntos)
10. [Exportar a Excel](#10-exportar-a-excel)
11. [Gestión de usuarios](#11-gestión-de-usuarios)
12. [Cerrar sesión](#12-cerrar-sesión)
13. [Preguntas frecuentes](#13-preguntas-frecuentes)

---

## 1. Acceso a la aplicación

Abre la dirección de la aplicación en tu navegador. Según cómo esté desplegada:

- **Con Docker (recomendado)**: `http://localhost:3000`
- **En desarrollo (frontend)**: `http://localhost:5173`

Si no tienes usuario, pídeselo a un **Administrador** (ver [Gestión de usuarios](#11-gestión-de-usuarios)).

> ¿Quieres levantar la aplicación tú mismo? Los pasos de instalación y ejecución están en [CLAUDE.md](./CLAUDE.md).

---

## 2. Iniciar sesión

1. En la pantalla `/login`, escribe tu **usuario** y **contraseña**.
2. Pulsa **Iniciar sesión**.

La sesión se mantiene de forma segura mediante una cookie; no necesitas volver a introducir la contraseña mientras la sesión siga activa. Si tus credenciales son incorrectas, verás un mensaje de error.

> **Credenciales iniciales por defecto** (si es una instalación nueva): usuario `admin`, contraseña `Admin123*`. **Cámbialas cuanto antes** en un entorno real.

---

## 3. Roles y permisos

Cada usuario tiene **un rol**, y cada rol otorga una serie de **permisos**. La interfaz **oculta o muestra** botones y menús según lo que puedas hacer.

| Rol            | Puede hacer                                                        |
|----------------|-------------------------------------------------------------------|
| **Administrador** | Todo: consultar, crear, editar y eliminar leads, exportar y **gestionar usuarios**. |
| **Editor**     | Consultar, crear y editar leads (y exportar). **No** puede eliminar leads ni gestionar usuarios. |

Permisos individuales: `leads.read` (consultar), `leads.create` (crear), `leads.update` (editar), `leads.delete` (eliminar), `usuarios.manage` (gestionar usuarios).

Si intentas acceder a una función para la que no tienes permiso, la aplicación no te lo permitirá.

---

## 4. Consultar leads

Entra en **Consultar Leads** (`/leads`). Verás una **tabla paginada** con los leads, ordenados del más reciente al más antiguo.

- **Filtrar por formulario**: usa el desplegable de *formulario* para ver solo los leads de un tipo de formulario concreto.
- **Paginación**: navega entre páginas con los controles inferiores.
- **Campos dinámicos**: cada formulario tiene sus propios campos, por lo que **las columnas pueden variar** según el tipo de lead.

Desde esta pantalla puedes abrir el **detalle** de un lead, **crear** uno nuevo, **editar**, **eliminar** (si tienes permiso) y **exportar a Excel**.

---

## 5. Ver el detalle de un lead

Haz clic en un lead para abrir su **detalle** (`/leads/:id`). Verás:

- Los **datos del formulario** (todos los campos con su valor).
- La **fecha de creación** y el **formulario** al que pertenece.
- La lista de **archivos adjuntos**, con opción de **descargar** cada uno.

---

## 6. Crear un lead

Requiere el permiso **crear** (`leads.create`).

1. Pulsa **Nuevo lead** (`/leads/nuevo`).
2. Indica el **formulario** (nombre del tipo de formulario, obligatorio).
3. Añade los **campos** del formulario. Cada campo tiene:
   - **Nombre** (identificador del campo, p. ej. `email`).
   - **Valor** (el dato del lead).
   - **Alias** y **orden** (opcionales, para presentación).
   - Debe haber **al menos un campo**.
4. Opcionalmente, **adjunta archivos** (ver [Archivos adjuntos](#9-archivos-adjuntos)).
5. Pulsa **Guardar**.

Al guardar, el lead queda registrado y se te redirige a la lista o al detalle.

---

## 7. Editar un lead

Requiere el permiso **editar** (`leads.update`).

1. Desde la lista o el detalle, pulsa **Editar** (`/leads/:id/editar`).
2. El formulario se **precarga** con el nombre del formulario y todos los campos actuales (respetando su orden y alias).
3. Modifica el **formulario** y/o los **campos** que necesites.
4. Pulsa **Guardar**.

> **Importante**: la edición reemplaza el **nombre del formulario y los campos**. **No modifica los archivos adjuntos**; estos se gestionan por separado (añadir al crear, o eliminar desde el detalle).

---

## 8. Eliminar un lead

Requiere el permiso **eliminar** (`leads.delete`, solo **Administrador** por defecto).

1. Desde la lista o el detalle, pulsa **Eliminar**.
2. Confirma la acción.

Al eliminar un lead se borran también **todos sus archivos adjuntos**. Esta acción **no se puede deshacer**.

---

## 9. Archivos adjuntos

- **Al crear** un lead puedes adjuntar uno o varios archivos (por ejemplo, un CV o un documento).
- **En el detalle** del lead puedes **descargar** cada archivo y, si tienes permiso, **eliminar** un archivo individual.
- Puede haber **límites** de tamaño y de tipo/extensión de archivo, configurados por el administrador. Si un archivo supera el tamaño permitido o no tiene una extensión aceptada, la aplicación lo rechazará con un mensaje de error.

---

## 10. Exportar a Excel

Requiere el permiso **consultar** (`leads.read`).

1. En **Consultar Leads**, aplica opcionalmente el filtro por **formulario**.
2. Pulsa **Exportar a Excel**.
3. El navegador descargará un archivo `.xlsx` con los leads (todos, o solo los del formulario filtrado).

El nombre del archivo sigue el patrón `leads_<formulario|todos>_<fecha>.xlsx`.

---

## 11. Gestión de usuarios

Requiere el permiso **gestionar usuarios** (`usuarios.manage`, solo **Administrador** por defecto). Menú **Usuarios** (`/usuarios`).

- **Listar usuarios**: tabla **paginada** (10 por página), ordenada por nombre de usuario.
- **Crear usuario** (`/usuarios/nuevo`): define **usuario**, **contraseña** (mínimo 6 caracteres), **rol** y si está **activo**.
- **Editar usuario** (`/usuarios/:id/editar`): cambia el **email/rol/estado**. El **nombre de usuario no se modifica**, y la contraseña **no** se cambia desde aquí.
- **Restablecer contraseña de otro usuario** (`/usuarios/:id/password`): establece una nueva contraseña (mínimo 6 caracteres) para **otro** usuario.
  - **No puedes cambiar tu propia contraseña** por esta vía; la aplicación lo impedirá.
- **Eliminar usuario**: quita el acceso de ese usuario.

---

## 12. Cerrar sesión

Pulsa **Cerrar sesión** en el menú. Se borrará tu sesión y volverás a la pantalla de inicio de sesión.

---

## 13. Preguntas frecuentes

**No veo el botón de crear / editar / eliminar / usuarios.**
Tu rol no tiene ese permiso. Pide a un Administrador que revise tu rol (ver [Roles y permisos](#3-roles-y-permisos)).

**Me expulsó de la sesión / me pide iniciar sesión de nuevo.**
La sesión pudo caducar. Vuelve a iniciar sesión. Si persiste, contacta al administrador.

**Las columnas de la tabla cambian entre un lead y otro.**
Es normal: los campos son **dinámicos** y dependen del formulario de cada lead.

**No puedo subir un archivo.**
Puede superar el **tamaño máximo** permitido o tener una **extensión** no aceptada. Consulta con el administrador los límites configurados.

**Olvidé mi contraseña.**
Solo un **Administrador** puede restablecer la contraseña de otro usuario desde **Usuarios**.

---

> **Documentación técnica** (arquitectura Clean/Onion, API REST, endpoints, base de datos PostgreSQL, seguridad JWT, Docker y despliegue): consulta **[CLAUDE.md](./CLAUDE.md)**.
