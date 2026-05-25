-- ==========================================================
-- Inicialización Base de Datos Servicios (PostgreSQL)
-- ==========================================================

-- Tabla 1: categorias_servicio
CREATE TABLE IF NOT EXISTS categorias_servicio (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(100) NOT NULL UNIQUE,
    descripcion VARCHAR(255),
    estado BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TIMESTAMP
);

-- Tabla 2: servicios
CREATE TABLE IF NOT EXISTS servicios (
    id SERIAL PRIMARY KEY,
    nombre VARCHAR(120) NOT NULL,
    descripcion VARCHAR(255),
    precio_base NUMERIC(10, 2) NOT NULL,
    duracion_estimada_minutos INT NOT NULL,
    categoria_servicio_id INT NOT NULL,
    estado BOOLEAN NOT NULL DEFAULT TRUE,
    fecha_creacion TIMESTAMP NOT NULL DEFAULT CURRENT_TIMESTAMP,
    fecha_modificacion TIMESTAMP,

    CONSTRAINT fk_servicio_categoria
        FOREIGN KEY (categoria_servicio_id)
        REFERENCES categorias_servicio(id)
);

-- Índice único para evitar servicios activos con el mismo nombre en la misma categoría
CREATE UNIQUE INDEX IF NOT EXISTS ux_servicio_nombre_categoria_activo
ON servicios (LOWER(nombre), categoria_servicio_id)
WHERE estado = TRUE;

-- ==========================================================
-- Semilla de Datos Iniciales (Seed Data)
-- ==========================================================

-- Insertar categorías por defecto si no existen
INSERT INTO categorias_servicio (nombre, descripcion)
VALUES 
    ('Mantenimiento', 'Servicios preventivos y de rutina'),
    ('Frenos', 'Servicios de sistema de frenado'),
    ('Suspensión y Dirección', 'Alineación, balanceo y componentes de suspensión'),
    ('Diagnóstico', 'Escaneo computarizado y localización de fallas')
ON CONFLICT (nombre) DO NOTHING;

-- Obtener IDs e insertar servicios por defecto si la tabla está vacía
DO $$
DECLARE
    cat_mantenimiento_id INT;
    cat_frenos_id INT;
    cat_suspension_id INT;
    cat_diagnostico_id INT;
BEGIN
    SELECT id INTO cat_mantenimiento_id FROM categorias_servicio WHERE nombre = 'Mantenimiento';
    SELECT id INTO cat_frenos_id FROM categorias_servicio WHERE nombre = 'Frenos';
    SELECT id INTO cat_suspension_id FROM categorias_servicio WHERE nombre = 'Suspensión y Dirección';
    SELECT id INTO cat_diagnostico_id FROM categorias_servicio WHERE nombre = 'Diagnóstico';

    IF NOT EXISTS (SELECT 1 FROM servicios LIMIT 1) THEN
        INSERT INTO servicios (nombre, descripcion, precio_base, duracion_estimada_minutos, categoria_servicio_id)
        VALUES
            ('Cambio de Aceite y Filtro', 'Reemplazo de aceite de motor y filtro de aceite', 45.00, 30, cat_mantenimiento_id),
            ('Afinamiento Mayor', 'Limpieza de inyectores, cambio de bujías y filtros', 120.00, 120, cat_mantenimiento_id),
            ('Cambio de Pastillas de Freno', 'Reemplazo de pastillas de freno delanteras o traseras', 65.00, 60, cat_frenos_id),
            ('Rectificación de Discos', 'Rectificación de discos de freno', 40.00, 90, cat_frenos_id),
            ('Alineación y Balanceo', 'Alineación de dirección 3D y balanceo de 4 ruedas', 35.00, 45, cat_suspension_id),
            ('Diagnóstico de Motor Computarizado', 'Escaneo de códigos de error y diagnóstico de sensores', 30.00, 30, cat_diagnostico_id);
    END IF;
END $$;