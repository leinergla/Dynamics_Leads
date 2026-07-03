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

Entra en **Consultar Leads** (`/leads`). Para ver leads debes **seleccionar primero un formulario** en el desplegable de *Formulario*; hasta entonces la tabla aparece vacía con el mensaje «Selecciona un formulario…».

- **Seleccionar formulario** (obligatorio): elige un tipo de formulario para cargar sus leads, ordenados del más reciente al más antiguo.
- **Paginación**: navega entre páginas con los controles **← Anterior / Siguiente →** (10 por página).
- **Campos dinámicos**: cada formulario tiene sus propios campos, por lo que **las columnas pueden variar** según el tipo de lead. En la tabla se muestran los primeros campos de cada lead.
- **Ver detalle**: usa el enlace **Ver detalle →** de cada fila.

Desde esta pantalla puedes abrir el **detalle** de un lead, **crear** uno nuevo (**+ Nuevo lead**) y **exportar a Excel** (si tienes permiso).

---

## 5. Ver el detalle de un lead

Pulsa **Ver detalle →** en un lead para abrir su **detalle** (`/leads/:id`). Verás:

- Los **datos del formulario** (todos los campos con su valor).
- El **Lead ID**, la **fecha de creación** y el **formulario** al que pertenece.
- La lista de **archivos adjuntos**, con opción de **Descargar** cada uno.
- Botones de **Editar** y **Eliminar** (según tus permisos).

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
   - Puedes **agregar** más campos con **+ Agregar campo** o quitarlos con **✕**.
4. Opcionalmente, **adjunta archivos** con el selector de archivos (permite varios; ver [Archivos adjuntos](#9-archivos-adjuntos)).
5. Pulsa **Crear lead**.

Al guardar, el lead queda registrado y se te redirige a su **detalle**.

---

## 7. Editar un lead

Requiere el permiso **editar** (`leads.update`).

1. Desde la lista o el detalle, pulsa **Editar** (`/leads/:id/editar`).
2. El formulario se **precarga** con el nombre del formulario y todos los campos actuales (respetando su orden y alias).
3. Modifica el **formulario** y/o los **campos** que necesites.
4. En la sección **Archivos** puedes **eliminar** archivos existentes del lead (con confirmación). **No se pueden añadir** archivos nuevos al editar: para adjuntar más archivos hay que crear un lead desde cero.
5. Pulsa **Guardar cambios**.

> **Importante**: la edición reemplaza el **nombre del formulario y los campos**. Sobre los archivos, la edición **solo permite eliminarlos**, no añadir nuevos.

---

## 8. Eliminar un lead

Requiere el permiso **eliminar** (`leads.delete`, solo **Administrador** por defecto).

1. Desde la lista o el detalle, pulsa **Eliminar**.
2. Confirma la acción.

Al eliminar un lead se borran también **todos sus archivos adjuntos**. Esta acción **no se puede deshacer**.

---

## 9. Archivos adjuntos

- **Al crear** un lead puedes adjuntar uno o varios archivos (por ejemplo, un CV o un documento) con el selector de archivos.
- **En el detalle** del lead puedes **descargar** cada archivo con el botón **Descargar**.
- **Al editar** un lead puedes **eliminar** archivos existentes (sección Archivos de la pantalla de edición). Para **añadir** archivos nuevos a un lead ya creado no hay opción: se crea un lead nuevo.
- Puede haber **límites** de tamaño y de tipo/extensión de archivo, configurados por el administrador. Si un archivo supera el tamaño permitido o no tiene una extensión aceptada, la aplicación lo rechazará con un mensaje de error.

---

## 10. Exportar a Excel

Requiere el permiso **consultar** (`leads.read`).

1. En **Consultar Leads**, **selecciona un formulario** (el botón de exportar permanece deshabilitado hasta que haya un formulario seleccionado con leads).
2. Pulsa **↓ Exportar a Excel**.
3. El navegador descargará un archivo `.xlsx` con los leads de ese formulario.

El nombre del archivo sigue el patrón `leads_<formulario>_<fecha>.xlsx`.

---

## 11. Gestión de usuarios

Requiere el permiso **gestionar usuarios** (`usuarios.manage`, solo **Administrador** por defecto). Menú **Usuarios** (`/usuarios`).

La tabla muestra **Usuario, Email, Rol y Estado** (Activo/Inactivo), con acciones **Editar**, **Contraseña** y **Eliminar** por fila.

- **Listar usuarios**: tabla **paginada** (10 por página), ordenada por nombre de usuario.
- **Crear usuario** (**+ Nuevo usuario**, `/usuarios/nuevo`): define **usuario**, **email** (opcional), **contraseña** (mínimo 6 caracteres) y **rol**. (El estado *activo* solo se ajusta al editar.)
- **Editar usuario** (enlace **Editar**, `/usuarios/:id/editar`): cambia el **email/rol/estado**. El **nombre de usuario no se modifica**, y la contraseña **no** se cambia desde aquí.
- **Restablecer contraseña de otro usuario** (enlace **Contraseña**, `/usuarios/:id/password`): establece una nueva contraseña (mínimo 6 caracteres) para **otro** usuario. El enlace **solo aparece** para el rol Administrador y sobre usuarios distintos al propio.
  - **No puedes cambiar tu propia contraseña** por esta vía; la aplicación lo impedirá.
- **Eliminar usuario** (enlace **Eliminar**, con confirmación): quita el acceso de ese usuario.

---

## 12. Cerrar sesión

Pulsa **Salir** en la barra superior (junto a tu nombre de usuario). Se borrará tu sesión y volverás a la pantalla de inicio de sesión.

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
