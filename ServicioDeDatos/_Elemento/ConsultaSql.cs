using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Utilidades;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public class RegistrosAfectados : RegistroDtm
    {
        public int cantidad { get; set; }
    }

    public class ClausulasDeConsultas
    {
        public const string ListaDeValores = nameof(ListaDeValores);
    }

    public class ConsultaSql<T> where T : IRegistro
    {
        private ContextoSe _contextoSe;

        private IDbConnection _conexion
        {
            get
            {
                if (_contextoSe != null)
                    return _contextoSe.Database.GetDbConnection();

                return new SqlConnection(CadenaDeConexion);
            }
        }

        private IDbTransaction _transaccion
        {
            get
            {
                if (_contextoSe != null)
                    return _contextoSe.Database.CurrentTransaction.GetDbTransaction();
                return null;
            }
        }

        private string _sentencia;

        public string Sentencia
        {
            get
            {
                while (_sentencia.IndexOf($"{Environment.NewLine}{Environment.NewLine}") > -1)
                    _sentencia = _sentencia.Replace($"{Environment.NewLine}{Environment.NewLine}", $"{Environment.NewLine}");

                return _sentencia;
            }
            private set
            {
                _sentencia = value;
            }
        }
        private bool _hayQueDebugar = false;

        public bool HayQueDebugar => _contextoSe == null ? _hayQueDebugar : _contextoSe.Debuggar;
        public string Fichero { get; set; }
        public TrazaSql Traza { get; set; }

        public static string CadenaDeConexion => ContextoSe.ObtenerDatosDeConexion().CadenaConexion;

        public ConsultaSql(string sentencia) :
        this(sentencia, false, "")
        {
        }

        public ConsultaSql(string sentencia, bool hayQueDebugar, string fichero)
        {
            Sentencia = sentencia;
            _hayQueDebugar = hayQueDebugar;
            Fichero = fichero.Replace(" ", "_");
        }



        public ConsultaSql(ContextoSe contexto, string sentencia)
        {
            _contextoSe = contexto;
            Sentencia = sentencia;
            Traza = contexto.Traza;
        }

        public List<T> LanzarConsulta(ContextoSe contetxto, DynamicParameters parametros = null)
        {
            _contextoSe = contetxto;
            return LanzarConsulta(parametros);
        }

        public List<T> LanzarConsulta(DynamicParameters parametros = null)
        {
            if (_contextoSe == null)
            {
                using (IDbConnection db = new SqlConnection(CadenaDeConexion))
                {
                    try
                    {
                        db.Open();
                    }
                    catch (Exception exc)
                    {
                        if (HayQueDebugar)
                        {
                            Traza = TrazaSql.CrearTraza(Fichero == null ? typeof(T).Name : Fichero);
                            Traza.AnotarMensaje("Error al abrir la BD", CadenaDeConexion);
                            Traza.AnotarExcepcion(exc);
                            Traza.Cerrar();
                        }
                        throw;
                    }
                    return LanzarConsulta(db, _transaccion, Fichero, parametros);
                }
            }

            if (_contextoSe.Traza != null && _contextoSe.Traza.Fichero != null) Fichero = _contextoSe.Traza.Fichero;


            return LanzarConsulta(_conexion, _transaccion, Fichero, parametros);
        }

        private List<T> LanzarConsulta(IDbConnection conexion, IDbTransaction transaccion, string fichero, DynamicParameters parametros = null)
        {
            List<T> resultado = null;
            if (HayQueDebugar) Traza = TrazaSql.CrearTraza(fichero.IsNullOrEmpty() ? typeof(T).Name : fichero);
            try
            {
                resultado = Lanzar(conexion, transaccion, parametros);
            }
            finally
            {
                if (HayQueDebugar) Traza.Cerrar();
            }
            return resultado;
        }

        private List<T> Lanzar(IDbConnection conexion, IDbTransaction transaccion, DynamicParameters parametros = null)
        {
            if (parametros is null)
                parametros = new DynamicParameters();
            SqlParameterCollection spc = new SqlCommand().Parameters;
            foreach (var nombre in parametros.ParameterNames)
            {
                var valor = parametros.Get<object>(nombre);
                spc.AddWithValue(nombre, valor);
            }

            var cronometro = new Stopwatch();

            List<T> resultado;
            try
            {
                if (Traza != null)
                    cronometro.Start();

                resultado = conexion.Query<T>(Sentencia, parametros, transaccion).ToList();

                if (Traza != null)
                {
                    cronometro.Stop();
                    Traza.AnotarTrazaSql(Sentencia, spc, cronometro.ElapsedMilliseconds);
                }
            }
            catch (Exception e)
            {
                if (Traza != null)
                {
                    cronometro.Stop();
                    Traza.AnotarExcepcion(e, Sentencia, spc);
                }
                throw;
            }

            return resultado;
        }

        public int EjecutarSentencia(DynamicParameters parametros = null)
        {
            if (_contextoSe != null)
                return EjecutarSentencia(_conexion, _transaccion, parametros);

            using (IDbConnection conexion = new SqlConnection(CadenaDeConexion))
            {
                conexion.Open();
                return EjecutarSentencia(conexion, _transaccion, parametros);
            }

        }

        public int EjecutarSentencia(IDbConnection conexion, IDbTransaction transaction, DynamicParameters parametros)
        {
            if (HayQueDebugar) Traza = TrazaSql.CrearTraza(Fichero == null ? typeof(T).Name : Fichero);
            try
            {
                return Ejecutar(conexion, transaction, parametros);
            }
            finally
            {
                if (HayQueDebugar) Traza.Cerrar();
            }
        }

        private int Ejecutar(IDbConnection conexion, IDbTransaction transaction, DynamicParameters parametros = null)
        {
            if (parametros is null)
                parametros = new DynamicParameters();

            SqlParameterCollection spc = new SqlCommand().Parameters;
            foreach (var nombre in parametros.ParameterNames)
            {
                var valor = parametros.Get<object>(nombre);
                spc.AddWithValue(nombre, valor);
            }

            var cronometro = new Stopwatch();
            int resultado;
            try
            {
                if (Traza != null)
                    cronometro.Start();

                resultado = conexion.Execute(Sentencia, parametros, transaction);

                if (Traza != null)
                {
                    cronometro.Stop();
                    Traza.AnotarTrazaSql(Sentencia, spc, cronometro.ElapsedMilliseconds);
                }
            }
            catch (Exception e)
            {
                if (Traza != null)
                {
                    cronometro.Stop();
                    Traza.AnotarExcepcion(e, Sentencia, spc);
                }
                else
                {
                    Traza = TrazaSql.CrearTraza("error.txt");
                    Traza.AnotarExcepcion(e, Sentencia, spc);
                }
                throw;
            }

            return resultado;
        }

        public static string DefinirClausulaOrden(List<ClausulaDeOrdenacion> orden)
        {
            string clausulaOrden = "";

            if (orden.Count == 0)
                return ApiDeInterfaceDtm.ImplementaNombre(typeof(T)) ? $"t1.{ICampos.NOMBRE} DESC" : $"t1.{ICampos.ID} DESC";

            if (orden.Count == 1 && orden[0].OrdenarPor.Equals(nameof(INombre.Nombre), StringComparison.CurrentCultureIgnoreCase))
            {
                clausulaOrden = $"t1.{ICampos.NOMBRE} {orden[0].ModoBd}";
            }

            return $"{clausulaOrden}";
        }

        public static string DefinirClausulaWhere(List<ClausulaDeFiltrado> filtros, Dictionary<string, object> parametrosSql)
        {
            string where = "1=1";

            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(IRegistro.Id), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorId(where, parametrosSql, filtro);
                }
                if (filtro.Clausula.Equals(nameof(INombre.Nombre), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorNombre(where, parametrosSql, filtro);
                }
                if (filtro.Clausula.Equals(nameof(EstadoDtm.Inicial), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorInicial(where, parametrosSql, filtro);
                }
                if (filtro.Clausula.Equals(nameof(EstadoDtm.Terminado), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorFinal(where, parametrosSql, filtro);
                }
                if (filtro.Clausula.Equals(nameof(EstadoDtm.Cancelado), StringComparison.CurrentCultureIgnoreCase))
                {
                    where = FiltroPorCancelado(where, parametrosSql, filtro);
                }
            }
            return $"Where {where}";
        }

        private static string FiltroPorId(string where, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            where = $"{where}{Environment.NewLine}and T1.{ICampos.ID} = @{ICampos.ID}";
            parametrosSql.Add($"@{ICampos.ID}", filtro.Valor);
            filtro.Aplicado = true;
            return where;
        }

        private static string FiltroPorNombre(string where, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            var porIni = filtro.Valor.StartsWith(Simbolos.Pipe) || filtro.Valor.StartsWith("=") ? "''" : "'%'";
            var porFin = filtro.Valor.StartsWith("=") ? "''" : "'%'";
            if (filtro.Criterio == enumCriteriosDeFiltrado.igual) porIni = porFin = "''";

            where = $"{where}{Environment.NewLine}and T1.{ICampos.NOMBRE} like {porIni} + @{ICampos.NOMBRE} + {porFin}";
            parametrosSql.Add($"@{ICampos.NOMBRE}", filtro.Valor);
            filtro.Aplicado = true;
            return where;
        }

        private static string FiltroPorInicial(string where, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            where = $"{where}{Environment.NewLine}and T1.{ICampos.INICIAL} {(filtro.Criterio == enumCriteriosDeFiltrado.igual ? "=" : "<>")} @{ICampos.INICIAL}";
            parametrosSql.Add($"@{ICampos.INICIAL}", filtro.Valor);
            filtro.Aplicado = true;
            return where;
        }

        private static string FiltroPorFinal(string where, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            where = $"{where}{Environment.NewLine}and T1.{ICampos.TERMINADO} {(filtro.Criterio == enumCriteriosDeFiltrado.igual ? "=" : "<>")} @{ICampos.TERMINADO}";
            parametrosSql.Add($"@{ICampos.TERMINADO}", filtro.Valor);
            filtro.Aplicado = true;
            return where;
        }
        private static string FiltroPorCancelado(string where, Dictionary<string, object> parametrosSql, ClausulaDeFiltrado filtro)
        {
            where = $"{where}{Environment.NewLine}and T1.{ICampos.CANCELADO} {(filtro.Criterio == enumCriteriosDeFiltrado.igual ? "=" : "<>")} @{ICampos.CANCELADO}";
            parametrosSql.Add($"@{ICampos.CANCELADO}", filtro.Valor);
            filtro.Aplicado = true;
            return where;
        }
        public void EliminarCriterio(string filtro)
        {
            Sentencia = Sentencia.Replace($"[{filtro}]", "");
        }

        public void AplicarFiltro(string filtro, string clausula)
        {
            Sentencia = Sentencia.Replace($"[{filtro}]", clausula);
        }

        public void AplicarClausulaIn(string filtro, string clausula, List<int> valores)
        {
            if (valores.Count > 0)
            {
                var lista = "";
                foreach (int valor in valores)
                    lista = lista + "," + valor.ToString();

                lista = lista.Substring(1);
                clausula = clausula.Replace($"[{ClausulasDeConsultas.ListaDeValores}]", lista);

                Sentencia = Sentencia.Replace($"[{filtro}]", clausula);
            }
            else
                EliminarCriterio(filtro);
        }

    }

}
