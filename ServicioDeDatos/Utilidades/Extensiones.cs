using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using Utilidades;

namespace ServicioDeDatos.Utilidades
{
    public static class Parametros
    {

        public static string ParsearParametros(this DbParameterCollection dbParametros)
        {
            var parametros = "";
            if (dbParametros == null || dbParametros.Count == 0)
                return "";
            for (int i = 0; i<dbParametros.Count; i++)
            {
                if (i >= dbParametros.Count)
                    continue;
                var dbParametro = dbParametros[i];
                parametros = $"{parametros}declare {(!dbParametro.ParameterName.StartsWith("@") ? "@" : "")}{dbParametro.ParameterName} {dbParametro.ParsearValorParametro()};{Environment.NewLine}";
            }

            if (dbParametros.Count > 0)
                parametros = parametros.Substring(0, parametros.Length - Environment.NewLine.Length);
            return parametros;
        }

        private static string ParsearValorParametro(this DbParameter dbParametro)
        {
            //if (dbParametro.Value == DBNull.Value)
            //    return "is null";

            if (dbParametro.EsDelTipo(new List<DbType>() { DbType.String, DbType.Guid, DbType.AnsiString}))
                return $"varchar({dbParametro.Size}) = {(dbParametro.Value == DBNull.Value ? Simbolos.ValorNuloDeUnParametro : $"'{dbParametro.Value}'")}";

            if (dbParametro.EsDelTipo(new List<DbType>() { DbType.Decimal }))
                return $"decimal = {(dbParametro.Value == DBNull.Value ? Simbolos.ValorNuloDeUnParametro : dbParametro.Value)}";

            if (dbParametro.EsDelTipo(new List<DbType>() { DbType.DateTime2, DbType.DateTime }))
                return $"datetime2 = {(dbParametro.Value == DBNull.Value ? Simbolos.ValorNuloDeUnParametro : $"'{dbParametro.Value}'")}";

            return $"int = {(dbParametro.Value == DBNull.Value ? Simbolos.ValorNuloDeUnParametro : dbParametro.Value)}";

        }
               
        private static bool EsDelTipo(this DbParameter parametro, List<DbType> listaDeTipos)
        {
            if (parametro == null)
                return true;

            return listaDeTipos.Contains(parametro.DbType);
        }

    }


}
