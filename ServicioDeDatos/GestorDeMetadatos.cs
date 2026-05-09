using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using Gestor.Errores;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using ServicioDeDatos.Elemento;
using Utilidades;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace ServicioDeDatos
{

    public class ReferenciaFk : IRegistro
    {
        public int Id { get; set; }
        public string Esquema { get; set; }
        public string Tabla { get; set; }
        public string Campo { get; set; }
    }

    public class GestorDeMetadatos
    {
        private string existeTablaSql = @"
                 SELECT count(*) as Id
                 FROM INFORMATION_SCHEMA.TABLES
                 WHERE TABLE_SCHEMA = '[Esquema]' 
                   AND TABLE_NAME = '[Tabla]'";

        private string existePaSql = @"
           SELECT cast(t2.Id as int) as Id 
                , t1.specific_schema as Esquema
                , t1.specific_name as Nombre 
           FROM INFORMATION_SCHEMA.ROUTINES t1
           inner join sysobjects t2 on t2.name = t1.specific_name
           where t1.ROUTINE_TYPE = 'PROCEDURE' 
             and t2.type = 'P'
             and t1.specific_schema like '[Esquema]'
             and t1.specific_name like '[Pa]'";

        private class ResultadoBoleano : IRegistro
        {
            public int Id { get; set; }
        }


        private ConsultaSql<ResultadoBoleano> Consulta { get; }

        private GestorDeMetadatos(string esquema, string objeto, bool esTabla)
        {
            var consultaParaEjecutar = esTabla ? existeTablaSql.Replace("[Esquema]", esquema).Replace("[Tabla]", objeto) : existePaSql.Replace("[Esquema]", esquema).Replace("[Pa]", objeto);
            Consulta = new ConsultaSql<ResultadoBoleano>(consultaParaEjecutar, false, fichero: $"Existe_{(esTabla ? "Tabla" : "Pa")}_{esquema}_{objeto}");
        }

        public static bool ExisteTabla(string esquemaTabla)
        {
            var partes = esquemaTabla.Split(".");
            if (partes.Length != 2)
                throw new Exception($"El esquema y tabla está mal definido {esquemaTabla}");
            return ExisteTabla(partes[0], partes[1]);
        }

        public static bool ExisteTabla(string esquema, string tabla)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Dtm_ExisteTabla);
            var i = $"{esquema}.{tabla}";
            if (!cache.ContainsKey(i))
            {
                var c = new GestorDeMetadatos(esquema, tabla, esTabla: true);
                var resultados = c.Consulta.LanzarConsulta();
                cache[i] = resultados[0].Id == 1;
            }
            return (bool)cache[i];
        }


        public static void ValidarExisteTabla(string esquemaTabla)
        {
            var partes = esquemaTabla.Split(".");
            if (partes.Length != 2)
                GestorDeErrores.Emitir($"El valor {esquemaTabla} no es válido, ha de indicar el esquema, un punto, y una tabla");

            ValidarExisteTabla(partes[0], partes[1]);
        }

        public static void ValidarExisteTabla(string esquema, string tabla)
        {
            if (!ExisteTabla(esquema, tabla))
                GestorDeErrores.Emitir($"Ha de definir la tabla {esquema}.{tabla}");
        }


        public static void ValidarSql(string sql)
        {
            var p = new TSql110Parser(true);
            IList<ParseError> errors;
            p.Parse(new StringReader(sql), out errors);
            if (errors.Count > 0)
            {
                var mensaje = string.Join(Environment.NewLine, errors.Select(x => x.Message));
                //var m2 = errors.Aggregate("", (a, e) => a + Environment.NewLine + e.Message);
                GestorDeErrores.Emitir($"Sql mal redactada: " + mensaje);
            }
        }

        public static List<ReferenciaFk> ReferenciasA(string esquema, string tabla)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_ReferenciasA);
            var i = $"{esquema}.{tabla}";
            if (!cache.ContainsKey(i))
            {
                string query = @$"
                SELECT 
                    ROW_NUMBER() OVER (ORDER BY s_ref.name, t.name, c.name) AS {nameof(ReferenciaFk.Id)},
                    s_ref.name AS {nameof(ReferenciaFk.Esquema)},
                    t.name AS {nameof(ReferenciaFk.Tabla)},
                    c.name AS {nameof(ReferenciaFk.Campo)}
                FROM 
                    sys.foreign_keys fk
                INNER JOIN 
                    sys.tables t ON fk.parent_object_id = t.object_id
                INNER JOIN 
                    sys.schemas s_ref ON t.schema_id = s_ref.schema_id
                INNER JOIN 
                    sys.tables rt ON fk.referenced_object_id = rt.object_id
                INNER JOIN 
                    sys.schemas s ON rt.schema_id = s.schema_id
                INNER JOIN
                    sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                INNER JOIN
                    sys.columns c ON fkc.parent_column_id = c.column_id AND fkc.parent_object_id = c.object_id
                WHERE 
                    rt.name = 'ARCHIVO' 
                    AND s.name = 'SISDOC'
                    AND fkc.referenced_column_id = (
                        SELECT column_id 
                        FROM sys.columns 
                        WHERE object_id = rt.object_id AND name = 'ID'
                    )
                ";
                var referencias = new ConsultaSql<ReferenciaFk>(query, false, fichero: $"ReferenciaA_{esquema}_{tabla}");
                cache[i] = referencias.LanzarConsulta();
            }
            return (List<ReferenciaFk>)cache[i];
        }

    }

}

