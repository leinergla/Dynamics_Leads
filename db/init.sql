-- =============================================================================
-- Dynamics Leads — script único de inicialización de la base de datos.
--
-- Contiene, en orden: creación de la BD, tablas, datos iniciales (roles y
-- permisos) y todas las rutinas (funciones de lectura + procedimientos de
-- escritura, todas en LANGUAGE plpgsql).
--
-- Idempotente: puede ejecutarse varias veces sin error
-- (CREATE ... IF NOT EXISTS / ON CONFLICT DO NOTHING / CREATE OR REPLACE).
--
-- Uso manual (conéctate a la BD 'postgres'; el script crea 'dynamics_leads'):
--     psql -h localhost -U postgres -d postgres -f db/init.sql
--
-- En docker-compose se monta en /docker-entrypoint-initdb.d y se ejecuta solo
-- en el primer arranque (con el volumen de datos vacío).
--
-- Nota: el usuario administrador NO se siembra aquí; lo crea la API al arrancar
-- (PBKDF2), configurable con Seed:AdminUsername / Seed:AdminPassword.
-- =============================================================================

-- ===================== 0) CREACIÓN DE LA BASE DE DATOS =======================
-- Se crea solo si no existe. En el init de Docker ya existe (POSTGRES_DB),
-- por lo que este paso se omite y \c simplemente reconecta a la misma BD.

SELECT 'CREATE DATABASE dynamics_leads'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'dynamics_leads')\gexec

\connect dynamics_leads

-- ===================== 1) TABLAS =============================================

-- Leads: cada fila es un envío de formulario con sus campos en un jsonb.
CREATE TABLE IF NOT EXISTS public.leads
(
    leadid         uuid NOT NULL DEFAULT gen_random_uuid(),
    formulario     character varying(255) NOT NULL,
    datos          jsonb NOT NULL,
    fecha_creacion timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_leads PRIMARY KEY (leadid)
);

CREATE INDEX IF NOT EXISTS ix_leads_formulario     ON public.leads (formulario);
CREATE INDEX IF NOT EXISTS ix_leads_fecha_creacion ON public.leads (fecha_creacion DESC);

-- Archivos de un lead: solo se guarda la referencia (storage_key), no el binario.
CREATE TABLE IF NOT EXISTS public.lead_archivos
(
    id             uuid NOT NULL DEFAULT gen_random_uuid(),
    leadid         uuid NOT NULL,
    nombre_archivo character varying(255) NOT NULL,
    storage_key    character varying(1024) NOT NULL,
    nombre_campo   character varying(255),
    content_type   character varying(255),
    tamano         bigint NOT NULL DEFAULT 0,
    fecha_creacion timestamp without time zone DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT pk_lead_archivos PRIMARY KEY (id),
    CONSTRAINT fk_lead_archivos_lead FOREIGN KEY (leadid)
        REFERENCES public.leads (leadid) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS ix_lead_archivos_leadid ON public.lead_archivos (leadid);

-- Roles, permisos y su relación N:N; usuarios con un rol.
CREATE TABLE IF NOT EXISTS public.roles
(
    id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    nombre         varchar(100) NOT NULL UNIQUE,
    fecha_creacion timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE IF NOT EXISTS public.permisos
(
    id          uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    codigo      varchar(100) NOT NULL UNIQUE,
    descripcion varchar(255)
);

CREATE TABLE IF NOT EXISTS public.rol_permisos
(
    rol_id     uuid NOT NULL REFERENCES public.roles(id)    ON DELETE CASCADE,
    permiso_id uuid NOT NULL REFERENCES public.permisos(id) ON DELETE CASCADE,
    PRIMARY KEY (rol_id, permiso_id)
);

CREATE TABLE IF NOT EXISTS public.usuarios
(
    id             uuid PRIMARY KEY DEFAULT gen_random_uuid(),
    username       varchar(100) NOT NULL UNIQUE,
    email          varchar(255),
    password_hash  text NOT NULL,
    rol_id         uuid NOT NULL REFERENCES public.roles(id),
    activo         boolean NOT NULL DEFAULT true,
    fecha_creacion timestamp without time zone DEFAULT CURRENT_TIMESTAMP
);

-- ===================== 2) DATOS INICIALES (roles y permisos) =================

INSERT INTO public.roles (nombre) VALUES ('Administrador'), ('Editor')
ON CONFLICT (nombre) DO NOTHING;

INSERT INTO public.permisos (codigo, descripcion) VALUES
    ('leads.read',      'Consultar leads'),
    ('leads.create',    'Crear leads'),
    ('leads.update',    'Actualizar leads'),
    ('leads.delete',    'Eliminar leads'),
    ('usuarios.manage', 'Gestionar usuarios y roles')
ON CONFLICT (codigo) DO NOTHING;

-- Administrador: todos los permisos.
INSERT INTO public.rol_permisos (rol_id, permiso_id)
SELECT r.id, p.id
FROM public.roles r CROSS JOIN public.permisos p
WHERE r.nombre = 'Administrador'
ON CONFLICT DO NOTHING;

-- Editor: lectura/creación/actualización de leads (sin borrar ni gestionar usuarios).
INSERT INTO public.rol_permisos (rol_id, permiso_id)
SELECT r.id, p.id
FROM public.roles r
JOIN public.permisos p ON p.codigo IN ('leads.read', 'leads.create', 'leads.update')
WHERE r.nombre = 'Editor'
ON CONFLICT DO NOTHING;

-- ===================== 3) RUTINAS: FUNCIONES (lectura) ======================
-- Las lecturas usan funciones (RETURNS TABLE / escalar) porque un PROCEDURE no
-- devuelve result sets de forma limpia a Dapper. 'datos' se devuelve como text
-- (datos::text) para mapearlo a string.

CREATE OR REPLACE FUNCTION public.fn_list_leads(
    p_formulario character varying,
    p_offset     integer,
    p_limit      integer
)
RETURNS TABLE(
    leadid         uuid,
    formulario     character varying,
    datos          text,
    fecha_creacion timestamp without time zone
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    RETURN QUERY
    SELECT l.leadid, l.formulario, l.datos::text, l.fecha_creacion
    FROM public.leads l
    WHERE (p_formulario IS NULL OR l.formulario = p_formulario)
    ORDER BY l.fecha_creacion DESC
    OFFSET p_offset LIMIT p_limit;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_count_leads(p_formulario character varying)
RETURNS bigint
LANGUAGE plpgsql STABLE
AS $$
DECLARE v_total bigint;
BEGIN
    SELECT COUNT(*) INTO v_total
    FROM public.leads l
    WHERE (p_formulario IS NULL OR l.formulario = p_formulario);
    RETURN v_total;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_get_lead(p_leadid uuid)
RETURNS TABLE(
    leadid         uuid,
    formulario     character varying,
    datos          text,
    fecha_creacion timestamp without time zone
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    RETURN QUERY
    SELECT l.leadid, l.formulario, l.datos::text, l.fecha_creacion
    FROM public.leads l
    WHERE l.leadid = p_leadid;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_get_archivos_by_leads(p_leadids uuid[])
RETURNS TABLE(
    id             uuid,
    leadid         uuid,
    nombre_archivo character varying,
    storage_key    character varying,
    nombre_campo   character varying,
    content_type   character varying,
    tamano         bigint,
    fecha_creacion timestamp without time zone
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    RETURN QUERY
    SELECT a.id, a.leadid, a.nombre_archivo, a.storage_key, a.nombre_campo,
           a.content_type, a.tamano, a.fecha_creacion
    FROM public.lead_archivos a
    WHERE a.leadid = ANY(p_leadids)
    ORDER BY a.fecha_creacion;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_get_archivo(p_archivoid uuid)
RETURNS TABLE(
    id             uuid,
    leadid         uuid,
    nombre_archivo character varying,
    storage_key    character varying,
    nombre_campo   character varying,
    content_type   character varying,
    tamano         bigint,
    fecha_creacion timestamp without time zone
)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    RETURN QUERY
    SELECT a.id, a.leadid, a.nombre_archivo, a.storage_key, a.nombre_campo,
           a.content_type, a.tamano, a.fecha_creacion
    FROM public.lead_archivos a
    WHERE a.id = p_archivoid;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_list_formularios()
RETURNS TABLE(formulario character varying)
LANGUAGE plpgsql STABLE
AS $$
BEGIN
    RETURN QUERY
    SELECT DISTINCT l.formulario
    FROM public.leads l
    ORDER BY l.formulario;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_get_usuario_by_username(p_username varchar)
RETURNS TABLE(id uuid, username varchar, email varchar, password_hash text, rol_id uuid, rol_nombre varchar, activo boolean)
LANGUAGE plpgsql STABLE AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.username, u.email, u.password_hash, u.rol_id, r.nombre, u.activo
    FROM public.usuarios u
    JOIN public.roles r ON r.id = u.rol_id
    WHERE u.username = p_username;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_get_usuario(p_id uuid)
RETURNS TABLE(id uuid, username varchar, email varchar, password_hash text, rol_id uuid, rol_nombre varchar, activo boolean, fecha_creacion timestamp without time zone)
LANGUAGE plpgsql STABLE AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.username, u.email, u.password_hash, u.rol_id, r.nombre, u.activo, u.fecha_creacion
    FROM public.usuarios u
    JOIN public.roles r ON r.id = u.rol_id
    WHERE u.id = p_id;
END;
$$;

-- Se elimina la versión anterior sin paginación (firma distinta = sobrecarga nueva).
DROP FUNCTION IF EXISTS public.fn_list_usuarios();
CREATE OR REPLACE FUNCTION public.fn_list_usuarios(p_offset integer, p_limit integer)
RETURNS TABLE(id uuid, username varchar, email varchar, password_hash text, rol_id uuid, rol_nombre varchar, activo boolean, fecha_creacion timestamp without time zone)
LANGUAGE plpgsql STABLE AS $$
BEGIN
    RETURN QUERY
    SELECT u.id, u.username, u.email, u.password_hash, u.rol_id, r.nombre, u.activo, u.fecha_creacion
    FROM public.usuarios u
    JOIN public.roles r ON r.id = u.rol_id
    ORDER BY u.username
    OFFSET p_offset LIMIT p_limit;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_get_permisos_by_rol(p_rol_id uuid)
RETURNS TABLE(codigo varchar)
LANGUAGE plpgsql STABLE AS $$
BEGIN
    RETURN QUERY
    SELECT p.codigo
    FROM public.permisos p
    JOIN public.rol_permisos rp ON rp.permiso_id = p.id
    WHERE rp.rol_id = p_rol_id
    ORDER BY p.codigo;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_list_roles()
RETURNS TABLE(id uuid, nombre varchar)
LANGUAGE plpgsql STABLE AS $$
BEGIN
    RETURN QUERY
    SELECT r.id, r.nombre FROM public.roles r ORDER BY r.nombre;
END;
$$;

CREATE OR REPLACE FUNCTION public.fn_count_usuarios()
RETURNS bigint
LANGUAGE plpgsql STABLE AS $$
DECLARE v_total bigint;
BEGIN
    SELECT COUNT(*) INTO v_total FROM public.usuarios;
    RETURN v_total;
END;
$$;

-- ===================== 4) RUTINAS: PROCEDIMIENTOS (escritura) ================
-- Las escrituras usan procedimientos (CALL); el resultado se devuelve por
-- parámetro INOUT.

CREATE OR REPLACE PROCEDURE public.sp_insert_lead(
    IN    p_formulario character varying,
    IN    p_datos      jsonb,
    INOUT p_leadid     uuid DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO public.leads (formulario, datos)
    VALUES (p_formulario, p_datos)
    RETURNING leadid INTO p_leadid;
END;
$$;

CREATE OR REPLACE PROCEDURE public.sp_insert_lead_archivo(
    IN    p_leadid         uuid,
    IN    p_nombre_archivo character varying,
    IN    p_storage_key    character varying,
    IN    p_nombre_campo   character varying,
    IN    p_content_type   character varying,
    IN    p_tamano         bigint,
    INOUT p_id             uuid DEFAULT NULL
)
LANGUAGE plpgsql
AS $$
BEGIN
    INSERT INTO public.lead_archivos
        (leadid, nombre_archivo, storage_key, nombre_campo, content_type, tamano)
    VALUES
        (p_leadid, p_nombre_archivo, p_storage_key, p_nombre_campo, p_content_type, p_tamano)
    RETURNING id INTO p_id;
END;
$$;

CREATE OR REPLACE PROCEDURE public.sp_update_lead(
    IN    p_leadid      uuid,
    IN    p_formulario  character varying,
    IN    p_datos       jsonb,
    INOUT p_actualizado boolean DEFAULT false
)
LANGUAGE plpgsql
AS $$
DECLARE v_count integer;
BEGIN
    UPDATE public.leads
    SET formulario = p_formulario, datos = p_datos
    WHERE leadid = p_leadid;

    GET DIAGNOSTICS v_count = ROW_COUNT;
    p_actualizado := v_count > 0;
END;
$$;

CREATE OR REPLACE PROCEDURE public.sp_delete_lead(
    IN    p_leadid    uuid,
    INOUT p_eliminado boolean DEFAULT false
)
LANGUAGE plpgsql
AS $$
DECLARE v_count integer;
BEGIN
    -- FK lead_archivos -> leads ON DELETE CASCADE: borra también las filas de archivos.
    DELETE FROM public.leads WHERE leadid = p_leadid;

    GET DIAGNOSTICS v_count = ROW_COUNT;
    p_eliminado := v_count > 0;
END;
$$;

CREATE OR REPLACE PROCEDURE public.sp_delete_lead_archivo(
    IN    p_archivoid uuid,
    INOUT p_eliminado boolean DEFAULT false
)
LANGUAGE plpgsql
AS $$
DECLARE v_count integer;
BEGIN
    DELETE FROM public.lead_archivos WHERE id = p_archivoid;

    GET DIAGNOSTICS v_count = ROW_COUNT;
    p_eliminado := v_count > 0;
END;
$$;

CREATE OR REPLACE PROCEDURE public.sp_insert_usuario(
    IN    p_username      varchar,
    IN    p_email         varchar,
    IN    p_password_hash text,
    IN    p_rol_id        uuid,
    IN    p_activo        boolean,
    INOUT p_id            uuid DEFAULT NULL
)
LANGUAGE plpgsql AS $$
BEGIN
    INSERT INTO public.usuarios (username, email, password_hash, rol_id, activo)
    VALUES (p_username, p_email, p_password_hash, p_rol_id, p_activo)
    RETURNING id INTO p_id;
END;
$$;

CREATE OR REPLACE PROCEDURE public.sp_update_usuario(
    IN    p_id          uuid,
    IN    p_email       varchar,
    IN    p_rol_id      uuid,
    IN    p_activo      boolean,
    INOUT p_actualizado boolean DEFAULT false
)
LANGUAGE plpgsql AS $$
DECLARE v_count integer;
BEGIN
    UPDATE public.usuarios
    SET email = p_email, rol_id = p_rol_id, activo = p_activo
    WHERE id = p_id;
    GET DIAGNOSTICS v_count = ROW_COUNT;
    p_actualizado := v_count > 0;
END;
$$;

CREATE OR REPLACE PROCEDURE public.sp_update_usuario_password(
    IN    p_id            uuid,
    IN    p_password_hash text,
    INOUT p_actualizado   boolean DEFAULT false
)
LANGUAGE plpgsql AS $$
DECLARE v_count integer;
BEGIN
    UPDATE public.usuarios SET password_hash = p_password_hash WHERE id = p_id;
    GET DIAGNOSTICS v_count = ROW_COUNT;
    p_actualizado := v_count > 0;
END;
$$;

CREATE OR REPLACE PROCEDURE public.sp_delete_usuario(
    IN    p_id        uuid,
    INOUT p_eliminado boolean DEFAULT false
)
LANGUAGE plpgsql AS $$
DECLARE v_count integer;
BEGIN
    DELETE FROM public.usuarios WHERE id = p_id;
    GET DIAGNOSTICS v_count = ROW_COUNT;
    p_eliminado := v_count > 0;
END;
$$;
