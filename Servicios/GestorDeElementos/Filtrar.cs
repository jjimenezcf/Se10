using Gestor.Errores;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Linq.Dynamic.Core.CustomTypeProviders;
using System.Linq.Expressions;
using System.Reflection;
using Utilidades;

namespace GestorDeElementos
{
    public class MyCustomTypeProvider : DefaultDynamicLinqCustomTypeProvider
    {
        public MyCustomTypeProvider(ParsingConfig config)
            : base(config, new List<Type>(), cacheCustomTypes: true)
        {
        }

        public override HashSet<Type> GetCustomTypes()
        {
            var types = base.GetCustomTypes();
            // Registramos EF para poder escribir "EF.Functions"
            types.Add(typeof(Microsoft.EntityFrameworkCore.EF));

            // Registramos esta clase: contiene el método de extensión .Like()
            types.Add(typeof(Microsoft.EntityFrameworkCore.DbFunctionsExtensions));

            return types;
        }
    }

    public static class Filtrar
    {

        public static bool OmitirFiltrosPorEstado(this List<ClausulaDeFiltrado> filtros, List<string> restrictores)
        {
            var filtroPorEstado = filtros.Where(x => x.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.CurrentCultureIgnoreCase) && x.Aplicado == false).FirstOrDefault();
            if (filtroPorEstado != null && filtroPorEstado.Criterio == enumCriteriosDeFiltrado.diferente && filtroPorEstado.Valor == $"{ltrParametrosNeg.Cancelados}{Simbolos.PuntoComa}{ltrParametrosNeg.Terminados}")
            {
                if (filtros.Any(f => f.Clausula.Equals(nameof(IRegistro.Id), StringComparison.InvariantCultureIgnoreCase)))
                {
                    filtroPorEstado.Aplicado = true;
                    return true;
                }
                foreach (var restrictor in restrictores)
                    if (filtros.Where(x => x.Clausula.Equals(restrictor, StringComparison.CurrentCultureIgnoreCase) && x.Criterio == enumCriteriosDeFiltrado.igual).Count() == 1)
                    {
                        filtroPorEstado.Aplicado = true;
                        return true;
                    }
            }
            return false;
        }

        public static IQueryable<TRegistro> AplicarFiltroPorPropiedad<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, Type tipo, string filtrarPor)
        {

            if (filtrarPor.ToLower().Equals(nameof(RegistroDtm.Id).ToLower()) && filtro.Criterio != enumCriteriosDeFiltrado.igual)
            {
                consulta = consulta.AplicarFiltroPorIdentificador(filtro, filtrarPor);
                return consulta;
            }

            if (filtrarPor.ToLower().Equals(nameof(ElementoDtm.Expresion).ToLower())
                && ApiDeInterfaceDtm.ImplementaUsaReferencia(typeof(TRegistro))
                && filtro.Criterio == enumCriteriosDeFiltrado.contiene)
            {
                filtro.Criterio = enumCriteriosDeFiltrado.porReferencia;
                consulta = consulta.AplicarFiltroDeCadena(filtro, filtrarPor);
                return consulta;
            }

            if (tipo == typeof(Guid) || tipo == typeof(Guid?))
            {
                Guid guidParsed = Guid.Parse(filtro.Valor);
                var expresion = $"x => x.Guid.Equals(@0)";
                var ConfiguracionDelParseo = new ParsingConfig();
                ConfiguracionDelParseo.UseParameterizedNamesInDynamicQuery = true;
                var where = DynamicExpressionParser.ParseLambda<TRegistro, bool>(ConfiguracionDelParseo, false, expresion, guidParsed);
                filtro.Aplicado = true;
                return consulta.Where(where);
            }

            if (filtro.Criterio == enumCriteriosDeFiltrado.porDiferentesPropiedades)
            {
                if (tipo != typeof(int) && tipo == typeof(string))
                    GestorDeErrores.Emitir($"No se ha implementado como filtrar por '{filtro.Criterio}' cuando el tipo es '{tipo.Name}' ");

                var propiedades = filtro.Clausula.Split(Simbolos.Or);
                var expresion = $"x => ";
                var or = "";
                foreach (var propiedad in propiedades)
                {
                    if (filtro.Valor.IsNullOrEmpty())
                    {
                        expresion = $"{expresion}{or}x.{propiedad} == null";
                    }
                    else
                    {
                        expresion = tipo == typeof(int) || tipo == typeof(int?)
                        ? $"{expresion}{or}x.{propiedad}.Equals({filtro.Valor})"
                        : $"{expresion}{or}x.{propiedad}.Equals(\"{filtro.Valor}\")";
                    }
                    or = " || ";
                }
                filtro.Aplicado = true;

                return consulta.AplicarFiltroPorExpresion(expresion);
            }

            if (tipo == typeof(string) && tipo.BaseType != typeof(Enum))
            {
                consulta = consulta.AplicarFiltroDeCadena(filtro, filtrarPor);
                return consulta;
            }

            if (tipo.BaseType == typeof(Enum))
            {
                consulta = consulta.AplicarFiltroDeEnumerado(tipo, filtro, filtrarPor);
                return consulta;
            }

            if ((tipo == typeof(int) || tipo == typeof(int?)) && filtrarPor.ToLower().StartsWith("id") && filtrarPor.Length > 2)
            {
                consulta = consulta.AplicarFiltroPorIdentificador(filtro, filtrarPor);
                return consulta;
            }

            if ((tipo == typeof(int) || tipo == typeof(int?)) && !filtrarPor.ToLower().StartsWith("id"))
            {
                consulta = consulta.AplicarFiltroPorEntero(filtro, filtrarPor);
                return consulta;
            }

            if ((tipo == typeof(decimal) || tipo == typeof(decimal?)))
            {
                consulta = consulta.AplicarFiltroPorDecimal(filtro, filtrarPor);
                return consulta;
            }

            if (tipo == typeof(bool))
            {
                consulta = consulta.AplicarFiltroPorBooleano(filtro, filtrarPor);
                return consulta;
            }

            if (tipo == typeof(DateTime) || tipo == typeof(DateTime?))
            {
                consulta = consulta.AplicarFiltroPorFecha(filtro, filtrarPor);
                return consulta;
            }

            return consulta;
        }

        public static IQueryable<TRegistro> AplicarFiltroPorExpresion<TRegistro>(this IQueryable<TRegistro> consulta, string expresion)
        {
            return consulta.Where(expresion);
        }

        public static IQueryable<TRegistro> AplicarFiltroConEntero<TRegistro>(this IQueryable<TRegistro> consulta, string expresion, ClausulaDeFiltrado filtro)
        {
            var ConfiguracionDelParseo = new ParsingConfig();
            ConfiguracionDelParseo.UseParameterizedNamesInDynamicQuery = true;
            var where = DynamicExpressionParser.ParseLambda<TRegistro, bool>(ConfiguracionDelParseo, false, expresion, filtro.Valor.Entero());
            filtro.Aplicado = true;
            return consulta.Where(where);
        }

        public static IQueryable<TRegistro> FiltroAplicado<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro)
        {
            filtro.Aplicado = true;
            return consulta;
        }

        //public static IQueryable<TRegistro> AplicarPredicado<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, Expression<Func<TRegistro, bool>> predicate)
        //{
        //    return filtro.Aplicado ? consulta : consulta.Where(predicate).FiltroAplicado(filtro);
        //}

        public static IQueryable<TRegistro> AplicarPredicado<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, Expression<Func<TRegistro, bool>> predicate)
        {
            if (filtro.Aplicado) return consulta;

            // Detectamos si el usuario está usando sintaxis especial
            bool tieneSimbolos = filtro.Valor.Contains(Simbolos.separadorDeCadenasDeFiltrado) ||
                                 filtro.Valor.Contains(Simbolos.Porcentaje) ||
                                 filtro.Valor.Contains(Simbolos.Pipe);

            if (tieneSimbolos)
            {
                // 1. Intentamos extraer la ruta de la propiedad (ej: "Interlocutor.Persona.Email")
                // Necesitamos un pequeño helper para obtener el string del predicado
                string rutaPropiedad = ObtenerRutaDePropiedad(predicate);

                if (!string.IsNullOrEmpty(rutaPropiedad))
                {
                    string expresionDinamica = ComponerFiltro(rutaPropiedad, filtro.Valor, porIgual: false);

                    // Primero creamos un config temporal para el provider
                    var configTemp = new ParsingConfig();
                    var provider = new MyCustomTypeProvider(configTemp);

                    // Luego creamos el config definitivo con el provider
                    var config = new ParsingConfig { CustomTypeProvider = provider };
                    var lambdaDinamica = DynamicExpressionParser.ParseLambda<TRegistro, bool>(config, false, expresionDinamica);

                    return consulta.Where(lambdaDinamica).FiltroAplicado(filtro);
                }
            }

            // Si no hay símbolos o no pudimos extraer la propiedad, ejecutamos el código original
            return consulta.Where(predicate).FiltroAplicado(filtro);
        }
        private static string ObtenerRutaDePropiedad(Expression expression)
        {
            if (expression == null) return "";

            // 1. Si es una Lambda, vamos al cuerpo
            if (expression is LambdaExpression lambda)
                return ObtenerRutaDePropiedad(lambda.Body);

            // 2. SI ES UNA EXPRESIÓN BINARIA (EL "||" QUE HAS VISTO EN DEBUG)
            if (expression is BinaryExpression binary)
            {
                var izquierda = ObtenerRutaDePropiedad(binary.Left);
                var derecha = ObtenerRutaDePropiedad(binary.Right);

                if (string.IsNullOrEmpty(izquierda)) return derecha;
                if (string.IsNullOrEmpty(derecha)) return izquierda;

                // Unimos ambas rutas usando el separador de propiedades que use tu ComponerFiltro
                return $"{izquierda}{Simbolos.separadorDePropiedades}{derecha}";
            }

            // 3. Si es una llamada a método (.Contains, .Equals)
            if (expression is MethodCallExpression call)
            {
                if (call.Object != null) return ObtenerRutaDePropiedad(call.Object);
                return ObtenerRutaDePropiedad(call.Arguments[0]);
            }

            // 4. Acceso a propiedad (x.Interlocutor.Persona.Apellidos)
            if (expression is MemberExpression member)
            {
                // Si el padre es el parámetro "x", devolvemos el nombre de la propiedad
                if (member.Expression is ParameterExpression)
                    return member.Member.Name;

                // Si el padre es otro acceso (anidado), recursión
                string rutaPadre = ObtenerRutaDePropiedad(member.Expression);
                return string.IsNullOrEmpty(rutaPadre) ? member.Member.Name : $"{rutaPadre}.{member.Member.Name}";
            }

            return "";
        }

        public static IQueryable<TRegistro> AplicarFiltroPorEntero<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad = null)
        {
            string expresion;
            if (!(filtro.Criterio == enumCriteriosDeFiltrado.esNulo ||
                filtro.Criterio == enumCriteriosDeFiltrado.noEsNulo ||
                filtro.Criterio == enumCriteriosDeFiltrado.esAlgunoDe ||
                filtro.Criterio == enumCriteriosDeFiltrado.noEsNingunoDe) && !filtro.Valor.EsEntero())
                GestorDeErrores.Emitir($"Se ha solicitado filtrar por un número, y el valor pasado no lo es. Filtro: {filtro.Clausula}, Criterio {filtro.Criterio} Valor: '{filtro.Valor}' Dtm: {typeof(TRegistro).Name}. ");

            var valorEntero = filtro.Valor.Entero();
            if (propiedad == null) propiedad = filtro.Clausula;

            filtro.Aplicado = true;
            switch (filtro.Criterio)
            {
                case enumCriteriosDeFiltrado.igual:
                    expresion = $"x => x.{propiedad} == {valorEntero}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.mayor:
                    expresion = $"x => x.{propiedad} > {valorEntero}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.menor:
                    expresion = $"x => x.{propiedad} < {valorEntero}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.mayorIgual:
                    expresion = $"x => x.{propiedad} >= {valorEntero}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.menorIgual:
                    expresion = $"x => x.{propiedad} <= {valorEntero}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.esNulo:
                    expresion = $"x => x.{propiedad} == null";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.noEsNulo:
                    expresion = $"x => x.{propiedad} != null";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.diferente:
                    expresion = $"x => x.{propiedad} <> {valorEntero}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.esAlgunoDe:
                    return consulta = consulta.AplicarFiltroPorListaDeEnteros(propiedad, filtro.Valor.Split(Simbolos.separadorDeEnteros).Select(s => s.Entero()).ToList());
                case enumCriteriosDeFiltrado.noEsNingunoDe:
                    return consulta = consulta.AplicarFiltroPorNoContenerListaDeEnteros(propiedad, filtro.Valor.Split(Simbolos.separadorDeEnteros).Select(s => s.Entero()).ToList());
            }

            throw new Exception($"El filtro {filtro.Clausula} para la entidad {consulta.GetType()} por el criterio {filtro.Criterio} no está definido");
        }

        public static IQueryable<TRegistro> AplicarFiltroPorListaDeEnteros<TRegistro>(this IQueryable<TRegistro> consulta, string propiedad, List<int> listaDeEnteros)
        =>
        consulta.Where($"@0.Contains({propiedad})", listaDeEnteros == null || listaDeEnteros.Count() == 0 ? new List<int> { 0 }.ToArray() : listaDeEnteros.ToArray());

        public static IQueryable<TRegistro> AplicarFiltroPorNoContenerListaDeEnteros<TRegistro>(this IQueryable<TRegistro> consulta, string propiedad, List<int> listaDeEnteros)
        =>
        consulta.Where($"!@0.Contains({propiedad})", listaDeEnteros == null || listaDeEnteros.Count() == 0 ? new List<int> { 0 }.ToArray() : listaDeEnteros.ToArray());

        public static IQueryable<TRegistro> AplicarFiltroPorDecimal<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            string expresion;
            if (!(filtro.Criterio == enumCriteriosDeFiltrado.esNulo || filtro.Criterio == enumCriteriosDeFiltrado.noEsNulo) && !filtro.Valor.EsNumero())
                GestorDeErrores.Emitir($"Se ha solicitado filtrar por un número, y el valor pasado no lo es. Filtro: {filtro.Clausula}, Criterio {filtro.Criterio} Valor: '{filtro.Valor}' Dtm: {typeof(TRegistro).Name}. ");

            var valorDecimal = filtro.Valor.Decimal();

            filtro.Aplicado = true;
            switch (filtro.Criterio)
            {
                case enumCriteriosDeFiltrado.igual:
                    expresion = $"x => x.{propiedad} == {valorDecimal}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.mayor:
                    expresion = $"x => x.{propiedad} > {valorDecimal}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.menor:
                    expresion = $"x => x.{propiedad} < {valorDecimal}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.mayorIgual:
                    expresion = $"x => x.{propiedad} >= {valorDecimal}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.menorIgual:
                    expresion = $"x => x.{propiedad} <= {valorDecimal}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.esNulo:
                    expresion = $"x => x.{propiedad} == null";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.noEsNulo:
                    expresion = $"x => x.{propiedad} != null";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.diferente:
                    expresion = $"x => x.{propiedad} <> {valorDecimal}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
            }

            filtro.Aplicado = false;
            throw new Exception($"El filtro {filtro.Clausula} para la entidad {consulta.GetType()} por el criterio {filtro.Criterio} no está definido");
        }
        public static IQueryable<TRegistro> AplicarFiltroPorIdentificador<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            var valorEntero = filtro.Valor.Entero();

            if (valorEntero == 0 && !(filtro.Criterio == enumCriteriosDeFiltrado.esNulo || filtro.Criterio == enumCriteriosDeFiltrado.noEsNulo || filtro.Criterio == enumCriteriosDeFiltrado.esAlgunoDe))
                GestorDeErrores.Emitir($"Se ha solicitado filtrar por '{filtro.Clausula}', con el criterio '{filtro.Criterio}' y el valor proporcionado es '{filtro.Valor}', y eso no se puede hacer sobre la clase '{typeof(TRegistro).Name}'. ");

            return consulta.AplicarFiltroPorEntero(filtro, propiedad);
        }

        public static IQueryable<TRegistro> AplicarFiltroPorFecha<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            filtro.Aplicado = true;
            switch (filtro.Criterio)
            {
                case enumCriteriosDeFiltrado.entreFechas: return AplicarFiltroEntreFechas(consulta, filtro, propiedad);
                case enumCriteriosDeFiltrado.mayorIgual: return AplicarFiltroMayorIgual(consulta, filtro, propiedad);
                case enumCriteriosDeFiltrado.menorIgual: return AplicarFiltroMenorIgual(consulta, filtro, propiedad);
                case enumCriteriosDeFiltrado.menor: return AplicarFiltroMenor(consulta, filtro, propiedad);
                case enumCriteriosDeFiltrado.mayor: return AplicarFiltroMayor(consulta, filtro, propiedad);
                case enumCriteriosDeFiltrado.igual: return AplicarFiltroPorFechaIgual(consulta, filtro, propiedad);
                case enumCriteriosDeFiltrado.esNulo: return AplicarFiltroPorFechaNula(consulta, propiedad);
                case enumCriteriosDeFiltrado.noEsNulo: return AplicarFiltroPorFechaNoNula(consulta, propiedad);
            }

            filtro.Aplicado = false;
            throw new Exception($"El filtro {filtro.Clausula} para la entidad {consulta.GetType()} por el criterio {filtro.Criterio} no está definido");
        }

        public static IQueryable<TRegistro> ElementosNoVinculadosDeLaMismaSociedad<TRegistro>(this IQueryable<TRegistro> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro, IQueryable<VinculoDtm> vinculos)
        {
            consulta = consulta.Where(x => !vinculos.Any(a => a.idElemento2 == ((IRegistro)x).Id));
            var vinculado = NegociosDeSe.ToEnumerado(filtro.IdNegocio);
            var negocio = NegociosDeSe.ToEnumerado(NegociosDeSe.LeerNegocioPorDtm(typeof(TRegistro).FullName));
            if (negocio.UsaCg() && vinculado.UsaCg())
            {
                var elemento = (ElementoConCgDtm)NegociosDeSe.CrearGestor(contexto, vinculado).LeerRegistroPorId(filtro.IdElemento, true);
                consulta = consulta.Where(x => ((IUsaCg)x).Cg.IdSociedad == elemento.Cg.IdSociedad);
            }
            filtro.Aplicado = true;
            return consulta;
        }

        public static IQueryable<TRegistro> AplicarFiltroPorFechaNula<TRegistro>(IQueryable<TRegistro> consulta, string propiedad)
        {
            var expresionFecha = $"x.{propiedad} == null || x.{propiedad} = DateTime({"0001"},{"01"},{"01"},{"00"},{"00"},{"00"})";
            return consulta.AplicarFiltroPorExpresion($"x => {expresionFecha}");
        }

        public static IQueryable<TRegistro> AplicarFiltroPorFechaNoNula<TRegistro>(IQueryable<TRegistro> consulta, string propiedad)
        {
            var expresionFecha = $"x.{propiedad} != null && x.{propiedad} != DateTime({"0001"},{"01"},{"01"},{"00"},{"00"},{"00"})";
            return consulta.AplicarFiltroPorExpresion($"x => {expresionFecha}");
        }

        public static IQueryable<TRegistro> AplicarFiltroMayorIgual<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            var fecha = filtro.Valor.Fecha();
            if (fecha == null)
                return consulta;

            var expresionFechaDesde = $"x.{propiedad} >= DateTime({((DateTime)fecha).Year},{((DateTime)fecha).Month},{((DateTime)fecha).Day},{((DateTime)fecha).Hour},{((DateTime)fecha).Minute},{((DateTime)fecha).Second})";

            string expresion = $"x => {expresionFechaDesde}";
            return consulta.AplicarFiltroPorExpresion(expresion);
        }
        public static IQueryable<TRegistro> AplicarFiltroMenorIgual<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            var fecha = filtro.Valor.Fecha();
            if (fecha == null)
                return consulta;

            var expresionFechaDesde = $"x.{propiedad} <= DateTime({((DateTime)fecha).Year},{((DateTime)fecha).Month},{((DateTime)fecha).Day},{((DateTime)fecha).Hour},{((DateTime)fecha).Minute},{((DateTime)fecha).Second})";

            string expresion = $"x => {expresionFechaDesde}";
            return consulta.AplicarFiltroPorExpresion(expresion);
        }

        public static IQueryable<TRegistro> AplicarFiltroMenor<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            var fecha = filtro.Valor.Fecha();
            if (fecha == null)
                return consulta;

            var expresionFechaDesde = $"x.{propiedad} < DateTime({((DateTime)fecha).Year},{((DateTime)fecha).Month},{((DateTime)fecha).Day},{((DateTime)fecha).Hour},{((DateTime)fecha).Minute},{((DateTime)fecha).Second})";

            string expresion = $"x => {expresionFechaDesde}";
            return consulta.AplicarFiltroPorExpresion(expresion);
        }

        public static IQueryable<TRegistro> AplicarFiltroMayor<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            var fecha = filtro.Valor.Fecha();
            if (fecha == null)
                return consulta;

            var expresionFechaDesde = $"x.{propiedad} > DateTime({((DateTime)fecha).Year},{((DateTime)fecha).Month},{((DateTime)fecha).Day},{((DateTime)fecha).Hour},{((DateTime)fecha).Minute},{((DateTime)fecha).Second})";

            string expresion = $"x => {expresionFechaDesde}";
            return consulta.AplicarFiltroPorExpresion(expresion);
        }


        public static IQueryable<TRegistro> AplicarFiltroEntreNumeros<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad, bool usarAbs)
        {
            var rango = filtro.ParsearRango();
            //{ (usarAbs ? $" || x.{propiedad} <= {-1 * rango.desde})" : ")")}
            //{(usarAbs ? $" || x.{propiedad} >= {-1 * rango.hasta})" : ")")}
            var expresionImporteDesde = rango.desde != default ? $"x.{propiedad} >= {rango.desde}" : "";
            var expresionImporteHasta = rango.hasta != default ? $"x.{propiedad} < {rango.hasta}" : "";
            string expresion = $"x => {expresionImporteDesde} {(rango.desde != default && rango.hasta != default ? "&&" : "")} {expresionImporteHasta}";

            if (usarAbs)
            {
                var expresionImporteDesdeAbs = rango.desde != default ? $"x.{propiedad} <= {-1 * rango.desde}" : "";
                var expresionImporteHastaAbs = rango.hasta != default ? $"x.{propiedad} > {-1 * rango.hasta}" : "";
                expresion = $"x => ({expresionImporteDesde} {(rango.desde != default && rango.hasta != default ? " && " : "")} {expresionImporteHasta}) || ({expresionImporteDesdeAbs} {(rango.desde != default && rango.hasta != default ? " && " : "")} {expresionImporteHastaAbs})";
            }

            filtro.Aplicado = true;
            expresion = expresion.Replace(Simbolos.SeparadorDecimal, CacheDeVariable.CFG_SeparadorDecimal_En_BD);
            return consulta.AplicarFiltroPorExpresion(expresion);
        }

        //public static IQueryable<TRegistro> AplicarFiltroEntreFechas<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        //{
        //    var fecha = filtro.ParsearFechas();
        //    if (fecha.hasta is not null && fecha.desde.Fecha() > fecha.hasta.Fecha())
        //        GestorDeErrores.Emitir($"La fecha hasta en el filtro '{filtro.Clausula}' es menor que la fecha desde");

        //    var expresionFechaDesde = fecha.desde != null ? $"x.{propiedad} >= DateTime({((DateTime)fecha.desde).Year},{((DateTime)fecha.desde).Month},{((DateTime)fecha.desde).Day},{((DateTime)fecha.desde).Hour},{((DateTime)fecha.desde).Minute},{((DateTime)fecha.desde).Second})" : "";
        //    var expresionFechaHasta = fecha.hasta != null ? $"x.{propiedad} <= DateTime({((DateTime)fecha.hasta).Year},{((DateTime)fecha.hasta).Month},{((DateTime)fecha.hasta).Day},{((DateTime)fecha.hasta).Hour},{((DateTime)fecha.hasta).Minute},{((DateTime)fecha.hasta).Second})" : "";
        //    string expresion = $"x => {expresionFechaDesde} {(fecha.desde != null && fecha.hasta != null ? "&&" : "")} {expresionFechaHasta}";
        //    filtro.Aplicado = true;
        //    return consulta.AplicarFiltroPorExpresion(expresion);
        //}

        public static IQueryable<TRegistro> AplicarFiltroEntreFechas<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            if (string.IsNullOrEmpty(filtro.Valor)) return consulta;

            // 1. Rompemos por el separador de cadenas (;) para soportar los "OR"
            var gruposDeFechas = filtro.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado, StringSplitOptions.RemoveEmptyEntries);
            List<string> partesExpresion = new List<string>();

            foreach (var grupo in gruposDeFechas)
            {
                // Parseamos cada rango individualmente
                var (desde, hasta) = ClausulaDeFiltrado.ParsearFechas(grupo);

                if (hasta is not null && desde.Fecha() > hasta.Fecha())
                    GestorDeErrores.Emitir($"En el rango '{grupo}', la fecha hasta es menor que la fecha desde");

                List<string> condicionesRango = new List<string>();

                if (desde != null)
                {
                    var d = (DateTime)desde;
                    condicionesRango.Add($"x.{propiedad} >= DateTime({d.Year}, {d.Month}, {d.Day}, {d.Hour}, {d.Minute}, {d.Second})");
                }

                if (hasta != null)
                {
                    var h = (DateTime)hasta;
                    condicionesRango.Add($"x.{propiedad} <= DateTime({h.Year}, {h.Month}, {h.Day}, {h.Hour}, {h.Minute}, {h.Second})");
                }

                if (condicionesRango.Any())
                {
                    // Unimos con AND las fechas del mismo rango: (desde <= x <= hasta)
                    partesExpresion.Add($"({string.Join(" && ", condicionesRango)})");
                }
            }

            if (!partesExpresion.Any()) return consulta;

            // 2. Unimos todos los rangos con OR
            // Resultado: (Rango1) || (Rango2) || (Rango3)
            string expresionFinal = $"x => {string.Join(" || ", partesExpresion)}";

            filtro.Aplicado = true;

            // Usamos el sistema de parseo dinámico que ya configuramos antes
            var configTemp = new ParsingConfig();
            var provider = new MyCustomTypeProvider(configTemp);
            var config = new ParsingConfig { CustomTypeProvider = provider };
            var lambda = DynamicExpressionParser.ParseLambda<TRegistro, bool>(config, false, expresionFinal);

            return consulta.Where(lambda);
        }

        public static IQueryable<TRegistro> AplicarFiltroPorEstarEnElPeriodo<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedades)
        {
            var fecha = filtro.ParsearFechas();
            if (fecha.hasta is not null && fecha.desde.Fecha() > fecha.hasta.Fecha())
                GestorDeErrores.Emitir($"La fecha hasta en el filtro '{filtro.Clausula}' es menor que la fecha desde");

            var propiedadPeriodo = propiedades.Split(Simbolos.separadorDePeriodos);
            if (propiedadPeriodo.Length != 2)
                GestorDeErrores.Emitir($"El filtro '{filtro.Clausula}' está mal definido, ya que solo se ha indicado una propiedad '{propiedades}'");

            var expresionFechaDesde = fecha.desde != null ? $"x.{propiedadPeriodo[0]} <= DateTime({((DateTime)fecha.desde).Year},{((DateTime)fecha.desde).Month},{((DateTime)fecha.desde).Day},0,0,0,0)" : "";
            var expresionFechaHasta = fecha.hasta != null ? $"x.{propiedadPeriodo[1]} > DateTime({((DateTime)fecha.hasta).Year},{((DateTime)fecha.hasta).Month},{((DateTime)fecha.hasta).Day},0,0,0,0)" : "";
            string expresion = $"x => {expresionFechaDesde} {(fecha.desde != null && fecha.hasta != null ? "&&" : "")} {expresionFechaHasta}";
            filtro.Aplicado = true;
            return consulta.AplicarFiltroPorExpresion(expresion);
        }

        public static IQueryable<TRegistro> AplicarFiltroPorNoEstarEnElPeriodo<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedades)
        {
            var fecha = filtro.ParsearFechas();
            if (fecha.hasta is not null && fecha.desde.Fecha() > fecha.hasta.Fecha())
                GestorDeErrores.Emitir($"La fecha hasta en el filtro '{filtro.Clausula}' es menor que la fecha desde");

            if (fecha.hasta != null) fecha.hasta = fecha.hasta.Fecha().AddDays(1);

            var propiedadPeriodo = propiedades.Split(Simbolos.separadorDePeriodos);
            if (propiedadPeriodo.Length != 2)
                GestorDeErrores.Emitir($"El filtro '{filtro.Clausula}' está mal definido, ya que solo se ha indicado una propiedad '{propiedades}'");

            var expresionFechaDesde = fecha.desde != null ? $"x.{propiedadPeriodo[0]} > DateTime({((DateTime)fecha.desde).Year},{((DateTime)fecha.desde).Month},0,0,0,0)" : "";
            var expresionFechaHasta = fecha.hasta != null ? $"x.{propiedadPeriodo[1]} < DateTime({((DateTime)fecha.hasta).Year},{((DateTime)fecha.hasta).Month},{((DateTime)fecha.hasta).Day},0,0,0,0)" : "";
            string expresion = $"x => {expresionFechaDesde} {(fecha.desde != null && fecha.hasta != null ? "||" : "")} {expresionFechaHasta}";
            filtro.Aplicado = true;
            return consulta.AplicarFiltroPorExpresion(expresion);
        }
        public static IQueryable<TRegistro> AplicarFiltroPorFechaIgual<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            if (filtro.Valor.IsNullOrEmpty())
                return AplicarFiltroPorFechaNula(consulta, propiedad);

            var fecha = filtro.Valor.Fecha();
            var expresionFecha = $"x.{propiedad} = DateTime({((DateTime)fecha).Year},{((DateTime)fecha).Month},{((DateTime)fecha).Day},{((DateTime)fecha).Hour},{((DateTime)fecha).Minute},{((DateTime)fecha).Second})";

            return consulta.AplicarFiltroPorExpresion($"x => {expresionFecha}");
        }

        public static IQueryable<TRegistro> AplicarFiltroPorBooleano<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string propiedad)
        {
            string expresion;
            filtro.Aplicado = true;

            if (filtro.Valor == null)
                return consulta;

            var valorBooleano = bool.Parse(filtro.Valor);
            switch (filtro.Criterio)
            {
                case enumCriteriosDeFiltrado.igual:
                    expresion = $"x => x.{propiedad} == {valorBooleano}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
                case enumCriteriosDeFiltrado.diferente:
                    expresion = $"x => x.{propiedad} != {valorBooleano}";
                    return consulta.AplicarFiltroPorExpresion(expresion);
            }

            throw new Exception($"El filtro {filtro.Clausula} para la entidad {consulta.GetType()} por el criterio {filtro.Criterio} no está definido");
        }


        public static IQueryable<TRegistro> AplicarFiltroDeCadena<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro)
        =>
        AplicarFiltroDeCadena(consulta, filtro, filtro.Clausula);

        public static IQueryable<TRegistro> AplicarFiltroDeCadena<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, string filtrarPor = "")
        {
            filtro.Aplicado = true;
            if (filtro.Valor.IsNullOrEmpty() && !(filtro.Criterio == enumCriteriosDeFiltrado.esNulo || filtro.Criterio == enumCriteriosDeFiltrado.noEsNulo))
                return consulta;

            if (filtro.Clausula.Equals(filtrarPor, StringComparison.CurrentCultureIgnoreCase))
            {
                filtro.ModificarCriterioDeFiltrado();
                string expresion = "";
                switch (filtro.Criterio)
                {
                    case enumCriteriosDeFiltrado.igual:
                        expresion = ComponerFiltro(filtrarPor, filtro.Valor, porIgual: true);
                        break;
                    case enumCriteriosDeFiltrado.contiene:
                        expresion = ComponerFiltro(filtrarPor, filtro.Valor, porIgual: false);
                        break;
                    case enumCriteriosDeFiltrado.noContiene:
                        expresion = $"x => !x.{filtrarPor}.Contains(@0)";
                        break;
                    case enumCriteriosDeFiltrado.diferente:
                        expresion = $"x => !x.{filtrarPor}.Equals(@0)";
                        break;
                    case enumCriteriosDeFiltrado.comienza:
                        expresion = $"x => x.{filtrarPor}.StartsWith(@0)";
                        break;
                    case enumCriteriosDeFiltrado.termina:
                        expresion = $"x => x.{filtrarPor}.EndsWith(@0)";
                        break;
                    case enumCriteriosDeFiltrado.esNulo:
                        expresion = $"x => string.IsNullOrWhiteSpace(x.{filtrarPor})";
                        break;
                    case enumCriteriosDeFiltrado.noEsNulo:
                        expresion = $"x => !string.IsNullOrWhiteSpace(x.{filtrarPor})";
                        break;
                    case enumCriteriosDeFiltrado.porReferencia:
                        if (filtro.Valor.IndexOf(Simbolos.separadorDeCadenasDeFiltrado) > 0)
                        {
                            foreach (var item in filtro.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado))
                            {
                                if (item.Length > 0)
                                {
                                    string expresionItem = "";
                                    if (filtro.Criterio == enumCriteriosDeFiltrado.igual)
                                    {
                                        expresionItem = $"{(expresion.IsNullOrEmpty() ? "x => " : "")} x.{nameof(IUsaReferencia.Referencia)}.Equals(\"{item}\")";
                                    }
                                    else
                                    {
                                        expresionItem = $"{(expresion.IsNullOrEmpty() ? "x => " : "")} x.{nameof(INombre.Nombre)}.Contains(\"{item}\") || x.{nameof(IUsaReferencia.Referencia)}.Contains(\"{item}\")";
                                    }
                                    expresion = $"{(expresion.IsNullOrEmpty() ? "" : $"{expresion} || ")} {expresionItem}";
                                }
                            }
                        }
                        else expresion = $"x => x.{nameof(INombre.Nombre)}.Contains(@0) || x.{nameof(IUsaReferencia.Referencia)}.Contains(@0)";
                        break;
                    case enumCriteriosDeFiltrado.porMismaReferencia:
                        foreach (var item in filtro.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado))
                        {
                            if (item.Length > 0)
                            {
                                string expresionItem = "";
                                expresionItem = $"{(expresion.IsNullOrEmpty() ? "x => " : "")} x.{nameof(IUsaReferencia.Referencia)}.Equals(\"{item}\")";
                                expresion = $"{(expresion.IsNullOrEmpty() ? "" : $"{expresion} || ")} {expresionItem}";
                            }
                        }
                        break;
                    case enumCriteriosDeFiltrado.esAlgunoDe:
                        foreach (var item in filtro.Valor.Split(Simbolos.separadorDeCadenasDeFiltrado))
                        {
                            string expresionItem = "";
                            expresionItem = $"{(expresion.IsNullOrEmpty() ? "x => " : "")} x.{filtro.Clausula}.Contains(\"{item}\")";
                            expresion = $"{(expresion.IsNullOrEmpty() ? "" : $"{expresion} || ")} {expresionItem}";
                        }
                        break;
                    default:
                        GestorDeErrores.Emitir($"No se ha implementado la expresión de filtrado para el criterio {filtro.Criterio}");
                        break;
                }

                var configTemp = new ParsingConfig();
                var provider = new MyCustomTypeProvider(configTemp);
                var ConfiguracionDelParseo = new ParsingConfig
                {
                    UseParameterizedNamesInDynamicQuery = true,
                    CustomTypeProvider = provider
                };

                var where = DynamicExpressionParser.ParseLambda<TRegistro, bool>(
                    ConfiguracionDelParseo,
                    false,
                    expresion,
                    filtro.Valor);

                return consulta.Where(where);
            }

            return consulta;
        }

        private static string ComponerFiltro(string filtrarPor, string valor, bool porIgual)
        {
            var filtro = "";
            foreach (var propiedad in filtrarPor.Split(Simbolos.separadorDePropiedades))
            {
                foreach (var item in valor.Split(Simbolos.separadorDeCadenasDeFiltrado))
                {
                    if (item.Length > 0)
                        filtro = FiltrarPorUnaDeLasCadena(propiedad, filtro, item, porIgual);
                }
            }
            return filtro;
        }

        private static string FiltrarPorUnaDeLasCadena(string propiedad, string filtro, string item, bool porIgual)
        {
            var porComienzo = false;
            var porFin = false;

            if (item.StartsWith(Simbolos.Igual))
            {
                item = item.Substring(1);
                porIgual = true;
            }
            else if (item.StartsWith(Simbolos.Pipe))
            {
                item = item.Substring(1);
                porComienzo = true;
            }
            else if (item.EndsWith(Simbolos.Pipe))
            {
                item = item.Substring(0, item.Length - 1);
                porFin = true;
            }

            var prefijo = filtro.IsNullOrEmpty() ? "x => " : "";
            string expresionItem;

            // Si contiene % construimos el patrón LIKE respetando además porComienzo/porFin
            if (!porIgual && item.Contains(Simbolos.Porcentaje))
            {
                var partes = item.Trim().Split(Simbolos.Porcentaje)
                                 .Select(p => p.Trim())
                                 .Where(p => p.Length > 0);

                // porComienzo → no añadimos % al inicio; porFin → no añadimos % al final
                var patron = (porComienzo ? "" : "%")
                           + string.Join("%", partes)
                           + (porFin ? "" : "%");

                expresionItem = $"{prefijo} EF.Functions.Like(x.{propiedad}, \"{patron}\")";
            }
            else
            {
                expresionItem = porIgual
                    ? $"{prefijo} x.{propiedad}.Equals(\"{item.Trim()}\")"
                    : porComienzo
                    ? $"{prefijo} x.{propiedad}.StartsWith(\"{item.Trim()}\")"
                    : porFin
                    ? $"{prefijo} x.{propiedad}.EndsWith(\"{item.Trim()}\")"
                    : $"{prefijo} x.{propiedad}.Contains(\"{item.Trim()}\")";
            }

            filtro = filtro.IsNullOrEmpty()
                ? expresionItem
                : filtro + "||" + expresionItem;

            return filtro;
        }

        public static IQueryable<TRegistro> AplicarFiltroDeEnumerado<TRegistro>(this IQueryable<TRegistro> consulta, Type tipo, ClausulaDeFiltrado filtro, string filtrarPor = "")
        {
            filtro.Aplicado = true;
            if (filtro.Valor.IsNullOrEmpty() && !(filtro.Criterio == enumCriteriosDeFiltrado.esNulo || filtro.Criterio == enumCriteriosDeFiltrado.noEsNulo))
                return consulta;

            if (filtro.Clausula.Equals(filtrarPor, StringComparison.CurrentCultureIgnoreCase) && filtro.Criterio == enumCriteriosDeFiltrado.igual)
            {
                return consulta.Where($"{filtro.Clausula} == @0", (Enum)Enum.Parse(tipo, filtro.Valor, ignoreCase: true));
            }

            if (filtro.Clausula.Equals(filtrarPor, StringComparison.CurrentCultureIgnoreCase) && filtro.Criterio == enumCriteriosDeFiltrado.diferente)
            {
                return consulta.Where($"{filtro.Clausula} != @0", (Enum)Enum.Parse(tipo, filtro.Valor, ignoreCase: true));
            }
            return consulta;
        }

        public static IQueryable<TRegistro> AplicarFiltroPorPropiedades<TRegistro>(this IQueryable<TRegistro> consulta, List<ClausulaDeFiltrado> filtros) where TRegistro : IRegistro
        {
            var propiedades = typeof(TRegistro).GetProperties();

            foreach (var filtro in filtros)
            {
                if (filtro.Aplicado)
                    continue;

                var filtrarPor = "";

                var hayQueFiltrar = filtro.Criterio != enumCriteriosDeFiltrado.porDiferentesPropiedades
                    ? AnalizarClausula(propiedades, filtro.Clausula, filtrarPor)
                    : AnalizarClausulaPorDiferentesPropiedades(propiedades, filtro.Clausula);

                if (!hayQueFiltrar.filtrar)
                    continue;
                filtro.Aplicado = true;
                consulta = consulta.AplicarFiltroPorPropiedad(filtro, hayQueFiltrar.porTipo, hayQueFiltrar.filtrarPor);
            }
            return consulta;
        }

        private static (bool filtrar, string filtrarPor, Type porTipo) AnalizarClausulaPorDiferentesPropiedades(PropertyInfo[] propiedades, string clausula)
        {
            // Dividir la cadena de cláusula por el separador '|'
            var partes = clausula.Split(Simbolos.Or);

            // Obtener la primera parte de la cadena
            var primeraPropiedad = partes[0];

            // Buscar la propiedad en la lista de propiedades
            var propiedad = Array.Find(propiedades, p => p.Name.Equals(primeraPropiedad, StringComparison.CurrentCultureIgnoreCase));

            // Si la propiedad existe, devolver la terna con los valores adecuados
            if (propiedad != null)
            {
                return (true, clausula, propiedad.PropertyType);
            }

            // Si no se encuentra la propiedad, devolver la terna con filtrar como false
            return (false, null, null);
        }

        private static (bool filtrar, string filtrarPor, Type porTipo) AnalizarClausula(PropertyInfo[] propiedades, string clausula, string filtrarPor)
        {
            var partesDeLaClausula = clausula.Split('.');

            var filtrar = ContieneClausulaComoPropiedad(propiedades, partesDeLaClausula[0]);
            if (partesDeLaClausula.Length == 1)
                return filtrar.Contenida ? (true, $"{(filtrarPor.IsNullOrEmpty() ? "" : $"{filtrarPor}.")}{partesDeLaClausula[0]}", filtrar.Tipo) : (false, "", null);

            //si la parte de la claúsula no existe como propiedad, no sigo, retorno false
            if (!filtrar.Contenida)
                return (filtrar: false, filtrarPor: "", null);

            //si la cláusula existe: Preparo el filtroPor y llamo recursivamente para que analice las otras dos partes.
            filtrarPor = $"{(filtrarPor.IsNullOrEmpty() ? "" : $"{filtrarPor}.")}{partesDeLaClausula[0]}";
            clausula = PendienteDeAnalizar(partesDeLaClausula);

            var resultado = AnalizarClausula(filtrar.Tipo.GetProperties(), clausula, filtrarPor);
            return resultado;
        }

        private static string PendienteDeAnalizar(string[] partesDeLaClausula)
        {
            var nuevaClausula = "";
            for (int i = 1; i < partesDeLaClausula.Length; i++)
            {
                nuevaClausula = nuevaClausula + partesDeLaClausula[i];
                if (i < partesDeLaClausula.Length - 1)
                    nuevaClausula = nuevaClausula + '.';
            }
            return nuevaClausula;
        }

        private static (bool Contenida, Type Tipo) ContieneClausulaComoPropiedad(PropertyInfo[] propiedades, string clausula)
        {
            foreach (PropertyInfo p in propiedades)
                if (p.Name.Equals(clausula, StringComparison.CurrentCultureIgnoreCase))
                    return (true, p.PropertyType);

            return (false, null);
        }

        private static ClausulaDeFiltrado ModificarCriterioDeFiltrado(this ClausulaDeFiltrado filtro)
        {
            if (filtro.Valor.IndexOf(Simbolos.separadorDeCadenasDeFiltrado) > 0)
                return filtro;

            //if (filtro.Valor.StartsWith(Simbolos.Pipe) && filtro.Valor.Length > 1)
            //{
            //    filtro.Valor = filtro.Valor.Substring(1);
            //    filtro.Criterio = enumCriteriosDeFiltrado.comienza;
            //}

            if (filtro.Valor.StartsWith(Simbolos.Igual) && filtro.Valor.Length > 1)
            {
                filtro.Valor = filtro.Valor.Substring(1);
                if (filtro.Clausula.ToLower() == nameof(IUsaReferencia.Referencia).ToLower())
                {
                    filtro.Criterio = enumCriteriosDeFiltrado.porMismaReferencia;
                }
                else
                {
                    filtro.Criterio = filtro.Criterio == enumCriteriosDeFiltrado.porReferencia ? enumCriteriosDeFiltrado.porMismaReferencia : enumCriteriosDeFiltrado.igual;
                }
            }

            //if (filtro.Valor.EndsWith(Simbolos.Pipe) && filtro.Valor.Length > 1)
            //{
            //    filtro.Valor = filtro.Valor.Substring(0, filtro.Valor.Length - 1);
            //    filtro.Criterio = enumCriteriosDeFiltrado.termina;
            //}

            return filtro;
        }

        public static List<ClausulaDeFiltrado> ToFiltros(this Dictionary<string, object> filtros)
        {
            List<ClausulaDeFiltrado> clausulas = new List<ClausulaDeFiltrado>();

            foreach (var filtroPor in filtros)
            {
                if (Enum.IsDefined(typeof(enumCriteriosDeFiltrado), $"{filtroPor.Value}"))
                {
                    var criterio = ApiDeEnsamblados.ToEnumerado<enumCriteriosDeFiltrado>($"{filtroPor.Value}");
                    clausulas.Add(new ClausulaDeFiltrado(filtroPor.Key, criterio, ""));
                }
                else
                    clausulas.Add(new ClausulaDeFiltrado(filtroPor.Key, enumCriteriosDeFiltrado.igual, $"{filtroPor.Value}"));
            }
            return clausulas;
        }

        public static string FusionarFiltros(List<ClausulaDeFiltrado> filtrosDePantalla, List<ClausulaDeFiltrado> filtrosDeIa)
        {
            // 1. Aseguramos que las listas no sean nulas para evitar excepciones
            var lista1 = filtrosDePantalla ?? new List<ClausulaDeFiltrado>();
            var lista2 = filtrosDeIa ?? new List<ClausulaDeFiltrado>();

            // 2. Unimos ambas listas
            var listaUnificada = lista1.Concat(lista2);

            // 3. Filtramos duplicados basándonos en Clausula, Criterio y Valor (en minúsculas)
            var filtrosUnicos = listaUnificada
                .GroupBy(f => new
                {
                    Clausula = f.Clausula?.ToLower().Trim(),
                    Criterio = f.Criterio, // Al ser un enum no hace falta ToLower
                    Valor = f.Valor?.ToLower().Trim()
                })
                .Select(g => g.First()) // Nos quedamos con el primero de cada grupo
                .ToList();

            // 4. Serializamos de nuevo a string JSON
            return JsonConvert.SerializeObject(filtrosUnicos);
        }

    }

    public static class FiltrosSobreHitos
    {
        internal static IQueryable<T> FiltrosPorHitos<T>(this IQueryable<T> consulta, ContextoSe contexto, enumNegocio negocio, (EstadoAuditado estado, TransicionAuditada transicion) hito, ParametrosDeNegocio parametros)
        where T : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaEstado(typeof(T)))
                return consulta;

            if (hito.estado == null && hito.transicion == null)
                return consulta;

            (DateTime? desde, DateTime? hasta) fechas;
            if (hito.estado != null)
            {
                //if (hito.estado.idEstado > 0 && hito.estado.fechas.IsNullOrEmpty() || hito.estado.idEstado == 0 && !hito.estado.fechas.IsNullOrEmpty())
                //    GestorDeErrores.Emitir("Para filtrar por un estado auditado ha de indicar el estado y el periodo");

                fechas = ClausulaDeFiltrado.ParsearFechas(hito.estado.fechas, errorSiNulo: false);
                consulta = consulta.Where(r => negocio.Hitos(contexto).Any(x => x.IdElemento == r.Id
                                           && x.IdEstado == hito.estado.idEstado
                                           && x.Fecha >= (fechas.desde == null ? new DateTime(0) : fechas.desde)
                                           && x.Fecha < (fechas.hasta == null ? DateTime.Now : fechas.hasta)
                ));
                parametros.AplicarFiltroQueMostrar = false;
            }

            if (hito.transicion == null) return consulta;

            //if (hito.transicion.idTransicion > 0 && hito.transicion.fechas.IsNullOrEmpty() || hito.transicion.idTransicion == 0 && !hito.transicion.fechas.IsNullOrEmpty())
            //    GestorDeErrores.Emitir("Para filtrar por una transición auditada ha de indicar la transición y el periodo");

            fechas = ClausulaDeFiltrado.ParsearFechas(hito.transicion.fechas, errorSiNulo: false);

            consulta = consulta.Where(r => negocio.Hitos(contexto).Any(historia =>
                                       historia.IdElemento == r.Id
                                       && (fechas.desde == null || historia.Fecha >= fechas.desde)
                                       && (fechas.hasta == null || historia.Fecha < fechas.hasta)
                                       && historia.Id > negocio.Hitos(contexto)
                                           .Where(th =>
                                               th.IdElemento == historia.IdElemento
                                               && th.IdTransicion != null
                                               && th.IdTransicion == hito.transicion.idTransicion
                                           )
                                           .Select(th => th.Id)
                                           .First()
                                       )
                                   );


            parametros.AplicarFiltroQueMostrar = false;
            return consulta;
        }


        internal static IQueryable<T> FiltrosPorHitos<T>(this IQueryable<T> consulta, ContextoSe contexto, enumNegocio negocio, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        where T : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaEstado(typeof(T)))
                return consulta;

            var idsDeEstado = filtros.FirstOrDefault(f => f.Aplicado == false && f.Clausula.ToLower() == ltrFiltros.IdsDeEstado && (f.Criterio == enumCriteriosDeFiltrado.esAlgunoDe || f.Criterio == enumCriteriosDeFiltrado.igual));

            if (idsDeEstado is not null)
            {
                var fechasDeEstados = filtros.FirstOrDefault(f => f.Aplicado == false && f.Clausula.ToLower() == ltrFiltros.FechasDeEstado && f.Criterio == enumCriteriosDeFiltrado.entreFechas);
                var listaDeEstados = idsDeEstado.Valor.ToLista<int>(separador: ",");
                var fechas = fechasDeEstados is not null ? ClausulaDeFiltrado.ParsearFechas(fechasDeEstados.Valor, errorSiNulo: false) : (null, null);
                consulta = consulta.Where(r => negocio.Hitos(contexto).Any(hito => hito.IdElemento == r.Id
                                           && listaDeEstados.Contains(hito.IdEstado)
                                           && hito.Fecha >= (fechas.desde == null ? new DateTime(0) : fechas.desde)
                                           && hito.Fecha < (fechas.hasta == null ? DateTime.Now : fechas.hasta)));
                idsDeEstado.Aplicado = true;
                if (fechasDeEstados != null) fechasDeEstados.Aplicado = true;
                parametros.AplicarFiltroQueMostrar = false;
            }

            var idsDeTransiciones = filtros.FirstOrDefault(f => f.Aplicado == false && f.Clausula.ToLower() == ltrFiltros.IdsDeTransicion && (f.Criterio == enumCriteriosDeFiltrado.esAlgunoDe || f.Criterio == enumCriteriosDeFiltrado.igual));

            if (idsDeTransiciones is not null)
            {
                var fechasDeTransiciones = filtros.FirstOrDefault(f => f.Aplicado == false && f.Clausula.ToLower() == ltrFiltros.FechasDeTransiciones && f.Criterio == enumCriteriosDeFiltrado.entreFechas);
                var listaDeTransiciones = idsDeTransiciones.Valor.ToLista<int>(separador: ",");
                var fechas = fechasDeTransiciones is not null ? ClausulaDeFiltrado.ParsearFechas(fechasDeTransiciones.Valor, errorSiNulo: false) : (null, null);
                consulta = consulta.Where(r => negocio.Hitos(contexto).Any(hito => hito.IdElemento == r.Id
                                           && hito.IdTransicion != null
                                           && listaDeTransiciones.Contains((int)hito.IdTransicion)
                                           && hito.Fecha >= (fechas.desde == null ? new DateTime(0) : fechas.desde)
                                           && hito.Fecha < (fechas.hasta == null ? DateTime.Now : fechas.hasta)));
                idsDeTransiciones.Aplicado = true;
                if (fechasDeTransiciones != null) fechasDeTransiciones.Aplicado = true;
                parametros.AplicarFiltroQueMostrar = false;
            }
            return consulta;
        }

        public static IQueryable<T> FiltrosPorEtapas<T>(this IQueryable<T> consulta, enumNegocio enumNegocio, List<ClausulaDeFiltrado> filtros)
        where T : RegistroDtm
        {
            //if (enumNegocio == enumNegocio.No_Definido)
            //    return consulta;

            var filtrosEtapa = filtros.Where(x => x.Clausula.ToLower() == ltrFiltros.FiltroPorEtapa.ToLower()  && !x.Aplicado).ToList();
            if (filtrosEtapa.Count == 0)
                return consulta;

            var metadatos = enumNegocio.ObtenerMetadatos(emitirError: false);
            foreach (var filtro in filtrosEtapa)
            {
                consulta = consulta.FiltroInternoPorEtapas(metadatos, filtro);
            }
            var filtroPorEstado = filtros.FirstOrDefault(x => x.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.CurrentCultureIgnoreCase) && x.Valor.Entero() > 0 && x.Aplicado == false);
            if (filtroPorEstado != null) filtroPorEstado.Aplicado = true;
            return consulta;
        }

        public static IQueryable<T> FiltrosPorEtapa<T>(this IQueryable<T> consulta, ClausulaDeFiltrado filtro)
        where T : RegistroDtm
        {
            var negocio = NegociosDeSe.ToEnumerado(typeof(T));
            var metadatos = negocio.ObtenerMetadatos();
            return consulta.FiltroInternoPorEtapas(metadatos, filtro);
        }

        private static IQueryable<T> FiltroInternoPorEtapas<T>(this IQueryable<T> consulta, Metadatos metadatos, ClausulaDeFiltrado filtro)
        where T : RegistroDtm
        {
            Type tipoDeEtapas = metadatos.TipoEtapas;

            if (tipoDeEtapas == null)
                GestorDeErrores.Emitir($"El negocio '{metadatos.TipoDto.NegocioDeUnDtm()}' no tiene definido, en los metadatos, un enumerado de etapas, por lo que no se pueden aplicar filtros por etapa");

            var listaEstados = new List<string>();


            // 1. Desglosar las etapas (pueden venir varias separadas por '|')
            string[] etapas;
            if (filtro.Valor.Contains(Simbolos.separadorDeEtapas))
                etapas = filtro.Valor.Split(Simbolos.separadorDeEtapas);
            else
                etapas = new string[] { filtro.Valor };

            // 2. Obtener los IDs de estado para cada etapa
            foreach (var etapaStr in etapas)
            {
                var etapaEnum = ApiDeEnsamblados.ToEnumerado(tipoDeEtapas, etapaStr);

                if (metadatos.EstadosDeLaEtapa == null)
                    GestorDeErrores.Emitir($"El negocio '{metadatos.TipoDto.NegocioDeUnDtm()}' no tiene definido, en los metadatos, no tiene definida la función para obtener los estados de una etapa");

                var estadosDeEtapa = (List<int>)metadatos.EstadosDeLaEtapa(ApiDeEnsamblados.ToEnumerado(tipoDeEtapas, etapaStr));

                if (estadosDeEtapa != null && estadosDeEtapa.Count > 0)
                {
                    listaEstados.AddRange(estadosDeEtapa.Select(id => id.ToString()));
                }
            }

            // 3. Limpiar duplicados de IDs
            var estadosLimpios = string.Join(Simbolos.separadorDeEnteros.ToString(), listaEstados.Distinct());

            if (!string.IsNullOrEmpty(estadosLimpios))
            {
                // Determinar si es una inclusión (esAlgunoDe) o exclusión (noEsNingunoDe)
                // Si el criterio de la IA es 'igual' -> usamos esAlgunoDe
                // Si el criterio de la IA es 'diferente' -> usamos noEsNingunoDe
                var filtroPorEstados = new ClausulaDeFiltrado
                (
                    clausula: nameof(IUsaEstado.IdEstado),
                    criterio: filtro.Criterio == enumCriteriosDeFiltrado.igual || filtro.Criterio == enumCriteriosDeFiltrado.esAlgunoDe || filtro.Criterio == enumCriteriosDeFiltrado.contiene
                              ? enumCriteriosDeFiltrado.esAlgunoDe
                              : enumCriteriosDeFiltrado.noEsNingunoDe,
                    valor: estadosLimpios
                );

                consulta = consulta.AplicarFiltroPorEntero(filtroPorEstados);
            }

            // Marcamos este filtro como procesado para que no se repita
            filtro.Aplicado = true;

            return consulta;
        }

    }

    public static class SeleccionarColumnas
    {

        public static IQueryable<TRegistro> DefinirColumnas<TRegistro>(this IQueryable<TRegistro> consulta, ClausulaDeFiltrado filtro, Type tipo, string filtrarPor)
        {



            return consulta;
        }
    }

    public static class FiltrarPorSeguridad
    {

        public static IQueryable<R> DeTipo<R, T, P>(ContextoSe contexto, enumNegocio negocio, IQueryable<R> consulta)
        where R : IElementoConTipo
        where T : TipoDeElementoDtm
        where P : PermisosDelElementoDtm
        {
            var idNegocio = NegociosDeSe.LeerNegocioPorEnumerado(negocio).Id;

            //Si usa: tipo, cg y elemento
            consulta = consulta.Where(x => contexto.Set<PermisosPorTipoDtm>().Any(y => y.IdTipo.Equals(x.IdTipo)
                                            && y.IdNegocio.Equals(idNegocio)
                                            && y.IdUsuario.Equals(contexto.DatosDeConexion.IdUsuario)
                                            && contexto.Set<T>().Any(t => t.IdPermisoDeConsultor.Equals(y.IdPermiso)
                                                                                         && t.Id.Equals(x.IdTipo))
                                            ||
                                            contexto.Set<PermisosPorElementoDtm>().Any(y => !NegociosDeSe.UsaPermisosPorElemento(negocio) ? false : y.IdElemento.Equals(x.Id)
                                            && y.IdNegocio.Equals(idNegocio)
                                            && y.IdUsuario.Equals(contexto.DatosDeConexion.IdUsuario)
                                            && contexto.Set<P>().Any(t => (t.IdConsultor.Equals(y.IdPermiso) || t.IdGestor.Equals(y.IdPermiso) || t.IdAdministrador.Equals(y.IdPermiso))
                                                                           && t.IdElemento.Equals(x.Id))
                                           )
                                      )
                               );
            //Si usa: tipo, no CG y elemento

            //Si usa: No tipo, CG y elemento
            return consulta;
        }

        public static IQueryable<R> DeCg<R, P>(ContextoSe contexto, enumNegocio negocio, IQueryable<R> consulta)
        where R : RegistroDtm
        where P : PermisosDelElementoDtm
        {
            var idNegocio = NegociosDeSe.LeerNegocioPorEnumerado(negocio).Id;
            consulta = consulta.Where(x => contexto.Set<PermisosPorCgDtm>().Any(y => y.IdCg.Equals(((IUsaCg)x).IdCg)
                                     && y.IdNegocio.Equals(idNegocio)
                                     && y.IdUsuario.Equals(contexto.DatosDeConexion.IdUsuario)
                                     && contexto.Set<NegociosDeUnCgDtm>().Any(t => t.IdConsultor.Equals(y.IdPermiso)
                                                                                && t.IdCg.Equals(((IUsaCg)x).IdCg)
                                                                                && t.IdNegocio.Equals(idNegocio)))
                                           ||
                                           contexto.Set<PermisosPorElementoDtm>().Any(y => !NegociosDeSe.UsaPermisosPorElemento(negocio) ? false : y.IdElemento.Equals(x.Id)
                                           && y.IdNegocio.Equals(idNegocio)
                                           && y.IdUsuario.Equals(contexto.DatosDeConexion.IdUsuario)
                                           && contexto.Set<P>().Any(t => (t.IdConsultor.Equals(y.IdPermiso) || t.IdGestor.Equals(y.IdPermiso) || t.IdAdministrador.Equals(y.IdPermiso))
                                                                          && t.IdElemento.Equals(x.Id))
                                          )
                                      );

            return consulta;
        }


        public static IQueryable<R> DeNegocio<R, P>(ContextoSe contexto, enumNegocio negocio, IQueryable<R> consulta)
        where R : RegistroDtm
        where P : PermisosDelElementoDtm
        {
            var idNegocio = NegociosDeSe.LeerNegocioPorEnumerado(negocio).Id;

            var permisos = contexto.Set<PermisosPorNegocioDtm>().Where(x => x.IdNegocio == idNegocio && x.IdUsuario == contexto.DatosDeConexion.IdUsuario).Select(x => x.IdPermiso);

            consulta = consulta.Where(x => contexto.Set<NegocioDtm>().Any(y => (y.Id.Equals(idNegocio) && (permisos.Contains(y.IdConsultor) ||
                                                                                                           permisos.Contains(y.IdGestor) ||
                                                                                                           permisos.Contains(y.IdAdministrador)))
                                          )
                                          ||
                                          contexto.Set<P>().Where(e => e.IdElemento == x.Id).Any(p => contexto.Set<UsuariosDeUnPermisoDtm>().Any(up => up.IdUsuario == contexto.DatosDeConexion.IdUsuario
                                           && (up.IdPermiso == p.IdConsultor || up.IdPermiso == p.IdGestor || up.IdPermiso == p.IdAdministrador)))
                                          );

            return consulta;
        }


        public static IQueryable<R> DeNegocio<R>(ContextoSe contexto, enumNegocio negocio, IQueryable<R> consulta)
        where R : RegistroDtm
        {
            var idNegocio = NegociosDeSe.LeerNegocioPorEnumerado(negocio).Id;

            var permisos = contexto.Set<PermisosPorNegocioDtm>().Where(x => x.IdNegocio == idNegocio && x.IdUsuario == contexto.DatosDeConexion.IdUsuario).Select(x => x.IdPermiso);

            consulta = consulta.Where(x => contexto.Set<NegocioDtm>().Any(y => y.Id.Equals(idNegocio) && (permisos.Contains(y.IdConsultor) ||
                                                                                                           permisos.Contains(y.IdGestor) ||
                                                                                                           permisos.Contains(y.IdAdministrador))
                                                                         )
            );

            return consulta;
        }

    }

    public static class FiltrarPorDependencias
    {
        public static IQueryable<T> FiltroSiHayDependenciaDe<T>(this IQueryable<T> consulta, string filtrarPor, ClausulaDeFiltrado filtro, string propiedad)
        where T : ElementoConCgDtm
        {
            if (filtro.Clausula.Equals(filtrarPor, StringComparison.CurrentCultureIgnoreCase))
            {
                var sinRelacion = $"x => x.{propiedad} == null";
                var conRelacion = $"x => x.{propiedad} != null";

                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.AplicarFiltroPorExpresion(conRelacion);
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.AplicarFiltroPorExpresion(sinRelacion);
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<T> FiltroSiHayDependenciaDe<T>(this IQueryable<T> consulta, ClausulaDeFiltrado filtro, string propiedad)
        where T : ElementoConCgDtm
        {
            var sinRelacion = $"x => x.{propiedad} == null";
            var conRelacion = $"x => x.{propiedad} != null";

            if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                consulta = consulta.AplicarFiltroPorExpresion(conRelacion);
            if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                consulta = consulta.AplicarFiltroPorExpresion(sinRelacion);
            filtro.Aplicado = true;
            return consulta;
        }

        public static IQueryable<T> FiltroSiHayDependenciaDe<T>(this IQueryable<T> consulta, List<ClausulaDeFiltrado> filtros, string filtrarPor, string filtroDeAsociacion, ParametrosDeNegocio parametros, bool aplicarFiltroDeEstado)
        where T : ElementoConCgDtm
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == filtrarPor.ToLower() && !x.Aplicado);
            if (filtro != null && filtro.Criterio == enumCriteriosDeFiltrado.igual)
            {
                var filtroPorIdReferenciado = $"x => x.{filtrarPor} == {filtro.Valor.Entero()}";
                consulta = consulta.AplicarFiltroPorExpresion(filtroPorIdReferenciado);
                if (!aplicarFiltroDeEstado) parametros.AplicarFiltroQueMostrar = filtros.OmitirFiltrosPorEstado(new List<string> { filtrarPor });
                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == filtroDeAsociacion.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                consulta = consulta.FiltroSiHayDependenciaDe(filtro, filtrarPor);

                //var sinRelacion = $"x => x.{filtrarPor} == null";
                //var conRelacion = $"x => x.{filtrarPor} != null";

                //if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                //    consulta = consulta.AplicarFiltroPorExpresion(conRelacion);
                //if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                //    consulta = consulta.AplicarFiltroPorExpresion(sinRelacion);
                //filtro.Aplicado = true;
            }
            return consulta;
        }
        public static IQueryable<T> FiltroSiElDetalleDependeDe<T>(this IQueryable<T> consulta, ClausulaDeFiltrado filtro, string propiedad)
        where T : IDetalle
        {
            var sinRelacion = $"x => x.Elemento.{propiedad} == null";
            var conRelacion = $"x => x.Elemento.{propiedad} != null";

            if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                consulta = consulta.AplicarFiltroPorExpresion(conRelacion);
            if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                consulta = consulta.AplicarFiltroPorExpresion(sinRelacion);
            filtro.Aplicado = true;
            return consulta;
        }

        public static IQueryable<T> FiltroSiHayHijosDe<T, R>(this IQueryable<T> consulta, ContextoSe contexto, string filtrarPor, ClausulaDeFiltrado filtro, string propiedad)
        where T : ElementoConCgDtm
        where R : ElementoConCgDtm
        {
            if (filtro.Clausula.Equals(filtrarPor, StringComparison.CurrentCultureIgnoreCase))
            {
                var expresion = $"y => y.{propiedad} == x.Id";

                if (filtro.Valor.Entero() == ltrParametrosNeg.ConRelacion)
                    consulta = consulta.Where(x => contexto.Set<R>().Any(expresion));
                if (filtro.Valor.Entero() == ltrParametrosNeg.SinRelacion)
                    consulta = consulta.Where(x => !contexto.Set<R>().Any(expresion));
                filtro.Aplicado = true;
            }

            return consulta;
        }

        public static IQueryable<E> FiltroDeElementosConVinculosCon<E, V>(this IQueryable<E> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, string filtrarPor, string filtroDeAsociacion)
        where E : ElementoDtm
        where V : VinculoDtm
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == filtrarPor.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                //selecciona las tareas vinculadas CON un expediente
                //selecciona las tareas vinculadas CON un ppt
                //selecciona los circuitos cad vinculados CON una actividad
                consulta = consulta.Where(elemento => contexto.Set<V>().Any(vinculo => vinculo.idElemento2 == elemento.Id && vinculo.idElemento1 == filtro.Valor.Entero()));

                var filtroQueMostrar = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosNeg.QueMostrar.ToLower() && x.Criterio == enumCriteriosDeFiltrado.diferente && !x.Aplicado);
                if (filtroQueMostrar != null)
                    filtroQueMostrar.Aplicado = true;

                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == filtroDeAsociacion.ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.ToLower() == ltrParametrosNeg.ConRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(elemento => contexto.Set<V>().Any(vinculo => vinculo.idElemento2 == elemento.Id));
                }
                if (filtro.Valor.ToLower() == ltrParametrosNeg.SinRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(elemento => !contexto.Set<V>().Any(vinculo => vinculo.idElemento2 == elemento.Id));
                }
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static IQueryable<E> FiltroDeElementosConVinculosA<E, V>(this IQueryable<E> consulta, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, string filtrarPor, string filtroDeAsociacion)
        where E : ElementoDtm
        where V : VinculoDtm
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == filtrarPor.ToLower() && !x.Aplicado);
            if (filtro != null)
            {
                //selecciona las facturas relacionadas A un circuito cad
                //selecciona los expedientes relacionados A una tarea
                //selecciona los ppts relacionados A una tarea
                consulta = consulta.Where(elemento => contexto.Set<V>().Any(vinculo => vinculo.idElemento1 == elemento.Id && vinculo.idElemento2 == filtro.Valor.Entero()));

                filtros.MostrarTodos();

                //var filtroQueMostrar = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosNeg.QueMostrar.ToLower() && x.Criterio == enumCriteriosDeFiltrado.diferente && !x.Aplicado);
                //if (filtroQueMostrar != null)
                //    filtroQueMostrar.Aplicado = true;

                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == filtroDeAsociacion.ToLower());
            if (filtro != null)
            {
                if (filtro.Valor.ToLower() == ltrParametrosNeg.ConRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(elemento => contexto.Set<V>().Any(vinculo => vinculo.idElemento1 == elemento.Id));
                }
                if (filtro.Valor.ToLower() == ltrParametrosNeg.SinRelacion.ToString().ToLower())
                {
                    consulta = consulta.Where(elemento => !contexto.Set<V>().Any(vinculo => vinculo.idElemento1 == elemento.Id));
                }
                filtro.Aplicado = true;
            }
            return consulta;
        }

        public static void MostrarTodos(this List<ClausulaDeFiltrado> filtros)
        {
            var filtroQueMostrar = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosNeg.QueMostrar.ToLower() && x.Criterio == enumCriteriosDeFiltrado.diferente && !x.Aplicado);
            if (filtroQueMostrar != null)
                filtroQueMostrar.Aplicado = true;
        }

    }

    public static class FiltrosPorDireccion
    {
        public static IQueryable<T> FiltroPorDireccion<T>(IQueryable<T> consulta, ContextoSe contexto, enumNegocio negocio, List<ClausulaDeFiltrado> filtros)
        where T : RegistroDtm
        {
            var fltCalle = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDireccion.FiltroPorCalle.ToLower());
            if (fltCalle != null)
            {
                var direcciones = negocio.Direcciones(contexto).Where(x => contexto.Set<CalleDtm>().Any(c => c.Id == x.IdCalle && c.Nombre.IndexOf(fltCalle.Valor) > -1));
                consulta = consulta.Where(x => direcciones.Any(y => y.IdElemento == x.Id));
                fltCalle.Aplicado = true;
            }
            var fltMunicipio = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDireccion.FiltroPorBarrio.ToLower());
            if (fltMunicipio != null)
            {
                var direcciones = negocio.Direcciones(contexto).Where(x => contexto.Set<BarrioDtm>().Any(b => b.Id == x.IdBarrio && b.Nombre.IndexOf(fltMunicipio.Valor) > -1));
                consulta = consulta.Where(x => direcciones.Any(y => y.IdElemento == x.Id));
                fltMunicipio.Aplicado = true;
            }
            var fltCp = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDireccion.FiltroPorZona.ToLower());
            if (fltCp != null)
            {
                var direcciones = negocio.Direcciones(contexto).Where(x => contexto.Set<ZonaDtm>().Any(z => z.Id == x.IdZona && z.Nombre.IndexOf(fltCp.Valor) > -1));
                consulta = consulta.Where(x => direcciones.Any(y => y.IdElemento == x.Id));
                fltCp.Aplicado = true;
            }
            var fltZona = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDireccion.FiltroPorMunicipio.ToLower());
            if (fltZona != null)
            {
                var direcciones = negocio.Direcciones(contexto).Where(x => contexto.Set<MunicipioDtm>().Any(m => m.Id == x.IdMunicipio && m.Nombre.IndexOf(fltZona.Valor) > -1));
                consulta = consulta.Where(x => direcciones.Any(y => y.IdElemento == x.Id));
                fltZona.Aplicado = true;
            }
            var fltBarrio = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrDireccion.FiltroPorCp.ToLower());
            if (fltBarrio != null)
            {
                var direcciones = negocio.Direcciones(contexto).Where(x => contexto.Set<CodigoPostalDtm>().Any(cp => cp.Id == x.IdCp && cp.Codigo == fltBarrio.Valor));
                consulta = consulta.Where(x => direcciones.Any(y => y.IdElemento == x.Id));
                fltBarrio.Aplicado = true;
            }
            return consulta;
        }

    }

    public static class FiltrosPorPropiedadesComunes
    {
        public static IQueryable<T> FiltrarPorExpresion<T>(this IQueryable<T> consulta, List<ClausulaDeFiltrado> filtros)
        where T : RegistroDtm
        {
            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == nameof(ltrFiltros.Expresion).ToLower());
            if (filtro != null)
            {
                filtro.Clausula = nameof(ltrFiltros.Nombre);
            }
            return consulta;
        }
    }

}
