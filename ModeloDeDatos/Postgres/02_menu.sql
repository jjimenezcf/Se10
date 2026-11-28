-- Port a PostgreSQL de ServicioDeDatos/Sql/Entorno/ArbolDeMenu.sql
-- (ENTORNO.ARBOL_MENU función con CTE recursiva + vista ENTORNO.MENU_SE)

DROP VIEW IF EXISTS entorno.menu_se;
DROP FUNCTION IF EXISTS entorno.arbol_menu(int);

CREATE FUNCTION entorno.arbol_menu(p_id int)
RETURNS TABLE (id int, nombre varchar, descripcion varchar, icono varchar, activo boolean,
               idpadre int, idvista_mvc int, orden int)
LANGUAGE sql STABLE
AS $$
    WITH RECURSIVE menupadre AS (
        SELECT t1.id, t1.nombre, t1.descripcion, t1.icono, t1.activo, t1.idpadre, t1.idvista_mvc, t1.orden
        FROM entorno.menu t1
        UNION ALL
        SELECT s1.id, s1.nombre, s1.descripcion, s1.icono, s1.activo, s1.idpadre, s1.idvista_mvc, s1.orden
        FROM entorno.menu s1
        INNER JOIN menupadre t1 ON s1.idpadre = t1.id
    )
    SELECT DISTINCT p1.id, p1.nombre, p1.descripcion, p1.icono, p1.activo, p1.idpadre, p1.idvista_mvc, p1.orden
    FROM menupadre p1
$$;

CREATE VIEW entorno.menu_se AS
    SELECT t1.id,
           t1.nombre,
           t1.descripcion,
           t1.icono,
           t1.activo,
           t1.idpadre,
           t1.idvista_mvc,
           t1.orden,
           t2.nombre AS padre,
           t3.nombre AS vista,
           t3.controlador,
           t3.accion,
           t3.parametros
    FROM entorno.arbol_menu(0) AS t1
    LEFT JOIN entorno.menu t2 ON t2.id = t1.idpadre
    LEFT JOIN entorno.vista_mvc t3 ON t3.id = t1.idvista_mvc;
