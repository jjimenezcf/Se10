using Gestor.Errores;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data;

namespace ServicioDeDatos.Elemento
{
    public static class PaSql
    {

        public static void ValidarExistePa(this ContextoSe contexto, string esquema, string pa)
        {
            if (!ExistePa(contexto, esquema, pa))
                GestorDeErrores.Emitir($"No existe el PA '{pa}' en el esquema '{esquema}'");
        }

        public static bool ExistePa(this ContextoSe contexto, string esquema, string nombre)
        {
            using (var command = contexto.Database.GetDbConnection().CreateCommand())
            {
                if (command.Connection.State != ConnectionState.Open)
                {
                    command.Connection.Open();
                }

                var transaction = contexto.Database.CurrentTransaction;
                if (transaction != null)
                {
                    command.Transaction = transaction.GetDbTransaction();
                }
                command.CommandText = @$"SELECT COUNT(*)
                         FROM INFORMATION_SCHEMA.ROUTINES t1 with(nolock)
                         inner join sysobjects t2 with(nolock) on t2.name = t1.specific_name
                         where t1.ROUTINE_TYPE = 'PROCEDURE' 
                           and t2.type = 'P'
                           and t1.specific_schema like @esquema
                           and t1.specific_name like @pa";

                var parameter = command.CreateParameter();
                parameter.ParameterName = "@esquema";
                parameter.Value = $"{esquema}";
                command.Parameters.Add(parameter);

                parameter = command.CreateParameter();
                parameter.ParameterName = "@pa";
                parameter.Value = $"{nombre}";
                command.Parameters.Add(parameter);

                var resultado = command.ExecuteReader();
                var existe = resultado.Read() ? resultado.GetInt32(0) > 0: false;
                resultado.Close();
                return existe;
            }
        }

        public static void CrearPAParaDatosPlantilla(this ContextoSe contexto, string esquema, string nombre)
        {
            if (ExistePa(contexto, esquema, nombre))
                GestorDeErrores.Emitir($" El PA: '{esquema}.{nombre}' ya existe");

            string sentencia = $@"
CREATE PROCEDURE {esquema}.{nombre}
  @IdNegocio INT,
  @IdElemento INT
AS
BEGIN   
    -- Descriptor de información para una plantilla
    select tipo, dato
    from (
          select 0 as orden, 'pltFormulasDePlantilla' as tipo, RTRIM(-1) as dato
          union
          select 1 as orden, 'pltDatosDePlantilla' as tipo, '1:fuente_1, 2:fuente_2, 3:fuente_3, 4:documento' as dato
          union
          select 2 as orden, 'pltFilasDeTabla' as tipo, '5:tabla_1,6:Tabla_2' as dato
          union
          select 3 as orden, 'pltMapeosDeTabla' as tipo, 
              'tabla_1:col0=columna_1,col1=columna_2,col2=columna_2,col3=column_4' + '|' +
              'tabla_2:col0=columna_1,col1=columna_2,col2=columna_2,col3=column_4' 
       		as dato)
       as descriptor
    order by orden

    -- Consulta para obtener los datos de la fuente_1
    select 'campo1' as campo1,'campo2' as campo2
    
    -- Consulta para obtener los datos de la fuente_2
    select 'campo1' as campo1,'campo2' as campo2
    
    -- Consulta para obtener los datos de la fuente_3
    select 'campo1' as campo1,'campo2' as campo2
    
    -- Consulta para obtener los datos del documento
    select 'titulo del documento' as titulo, 'DateTime.Now' as impresaEl
    
    -- Consulta para obtener tablas: Tabla_1
    select 'columna 1' as columna_1, 'columna 2' as columna_2, 'columna 3' as columna_3, 'columna 4' as columna_4
    
    -- Consulta para obtener tablas: Tabla_2
    select 'columna 1' as columna_1, 'columna 2' as columna_2, 'columna 3' as columna_3, 'columna 4' as columna_4

END";
            contexto.Database.ExecuteSqlRaw(sentencia);
        }

        public static void RenombrarPa(this ContextoSe contexto, string esquema, string nombre, string anterior)
        {
            if (!ExistePa(contexto, esquema, anterior) && !ExistePa(contexto, esquema, nombre))
            {
                CrearPAParaDatosPlantilla(contexto,esquema, nombre);
                return;
            }               

            if (nombre.ToLower() == anterior.ToLower()) return;

            if (ExistePa(contexto, esquema, nombre))
                GestorDeErrores.Emitir($"No se puede renombrar el '{esquema}.{anterior}' con '{esquema}.{nombre}' ya que existe '{esquema}.{nombre}'");

            string sentencia = $@"EXEC sp_rename '{esquema}.{anterior}', '{nombre}'";
            contexto.Database.ExecuteSqlRaw(sentencia);
        }

        public static void EliminarPa(this ContextoSe contexto, string esquema, string nombre)
        {
            if (ExistePa(contexto, esquema, nombre))
            {
                string sentencia = $@"DROP PROCEDURE IF EXISTS {esquema}.{nombre}";
                contexto.Database.ExecuteSqlRaw(sentencia);
            }
        }

        public static DataSet EjecutarPa(this ContextoSe contexto, string esquema, string nombrePa, int idNegocio, int idFactura)
        {
            contexto.ValidarExistePa(esquema, nombrePa);

            using (var command = contexto.Database.GetDbConnection().CreateCommand())
            {
                var transaction = contexto.Database.CurrentTransaction.GetDbTransaction();
                command.Transaction = transaction;
                command.CommandText = $"{esquema}.{nombrePa} @p0, @p1";
                command.Parameters.Add(new SqlParameter("@p0", idNegocio));
                command.Parameters.Add(new SqlParameter("@p1", idFactura));

                var adapter = new SqlDataAdapter((SqlCommand)command);
                var dataSet = new DataSet();
                contexto.IniciarTraza($"EjecutarPa_{nombrePa}");
                try
                {
                    adapter.Fill(dataSet);
                    return dataSet;
                }
                catch (Exception ex)
                {
                    contexto.AnotarExcepcion(ex);
                    throw;
                }
                finally
                {
                    contexto.CerrarTraza();
                }
            }
        }
    }


}

