namespace ServicioDeDatos
{

    //Antigua forma, antes de usar Dapper
    public static class GestorDeConsultas
    {
        //public static void Seleccionar(ContextoSe contexto, ConsultaSql consultaSql, params object[] parameters)
        //{
        //    var sentenciaSql = contexto.Database.GetDbConnection().CreateCommand();
        //    sentenciaSql.CommandText = consultaSql.Select;

        //    var registrosLeidos = 0;
        //    contexto.Database.OpenConnection();
        //    try
        //    {
        //        var resultadoSql = sentenciaSql.ExecuteReader();

        //        while (resultadoSql.HasRows)
        //        {
        //            for (int i = 0; i < resultadoSql.FieldCount; i++)
        //                consultaSql.Columnas.Add(resultadoSql.GetName(i));

        //            resultadoSql.Read();
        //            var registro = new List<object>();
        //            for (int i = 0; i < resultadoSql.FieldCount; i++)
        //            {
        //                registro.Add(resultadoSql.GetValue(i));
        //            }
        //            consultaSql.Registros.Add(registrosLeidos, registro);

        //            registrosLeidos++;
        //            resultadoSql.NextResult();
        //        }
        //    }
        //    finally
        //    {
        //        contexto.Database.CloseConnection();
        //        consultaSql.Leidos = registrosLeidos;
        //    }
        //}

        //public static async Task<RelationalDataReader> Seleccionar(ContextoDeElementos contexto, ConsultaSql consultaSql, params object[] parameters)
        //{
        //    var sentenciaSql = contexto.Database.GetDbConnection().CreateCommand();
        //    sentenciaSql.CommandText = consultaSql.Select;
        //    contexto.Database.OpenConnection();
        //    var resultadoSql = sentenciaSql.ExecuteReader();

        //    var registrosLeidos = 0;
        //    while (resultadoSql.HasRows)
        //    {
        //        for (int i = 0; i < resultadoSql.FieldCount; i++)
        //            consultaSql.Columnas.Add(resultadoSql.GetName(i));

        //        resultadoSql.Read();
        //        var registro = new List<object>();
        //        for (int i = 0; i < resultadoSql.FieldCount; i++)
        //        {
        //            registro.Add(resultadoSql.GetValue(i));
        //        }
        //        consultaSql.Registros.Add(registrosLeidos, registro);

        //        registrosLeidos++;
        //        resultadoSql.NextResult();
        //    }
        //    consultaSql.Leidos = registrosLeidos;
        //}
    }
}
