using Gestor.Errores;
using ServicioDeDatos.Negocio;
using ServicioDeDatos;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeParmetrosDeNegocio
    {

        public static ParametroDeNegocioDtm ResetearParametro<T>(this enumNegocio negocio, ContextoSe contexto, T enumParametro, string valor)
        where T : struct, System.Enum
        {
            var parametro = negocio.Parametro(contexto, enumParametro, errorSiNoHay: false, errorSinValor: false, errorSinMasDeUno: true);
            
            if (parametro is null)
                return negocio.CrearParametro(contexto, enumParametro, valor);

            if (parametro.Valor == valor)
                return parametro;

            parametro.Valor = valor;
            return parametro.ModificarComoAdministrador(contexto, esUnaAccion:true);
        }

        public static ParametroDeNegocioDtm IncluirEnParametro<T>(this enumNegocio negocio, ContextoSe contexto, T parametro, string valor, string separador = Simbolos.Coma, bool resetearSiValorNulo = true)
        where T : struct, System.Enum
        {
            var parametroDtm = negocio.Parametro(contexto, parametro, errorSiNoHay: false, errorSinValor: false, errorSinMasDeUno: true);
            if (parametroDtm is null)
                return new ParametroDeNegocioDtm
                {
                    IdNegocio = negocio.IdNegocio(),
                    Nombre = parametro.ToString(),
                    Valor = valor,
                }.Insertar(contexto);

            var valoresEnBd = parametroDtm.Valor.ToLista<string>(separador).Distinct().ToList();
            var nuevos = valor.ToLista<string>(separador).Distinct().ToList();
            var elementosNuevos = nuevos.Except(valoresEnBd).ToList();

            bool hayCambios = false;
            if (elementosNuevos.Any())
            {
                valoresEnBd.AddRange(elementosNuevos); 
                hayCambios = true;
            }
            
            string valoresEnBdString = string.Join(", ", valoresEnBd.OrderBy(valor => valor));
            hayCambios = hayCambios || valoresEnBdString != parametroDtm.Valor;

            if (resetearSiValorNulo && hayCambios && valoresEnBd.Count() == 1 && valoresEnBd[0] == Simbolos.ValorNuloDeUnParametro)
                return ResetearParametro(negocio, contexto, parametro, valoresEnBdString);

            parametroDtm.Valor = valoresEnBdString;

            return !hayCambios ? parametroDtm : parametroDtm.Modificar(contexto);
        }

        public static ParametroDeNegocioDtm EliminarParametro<T>(this enumNegocio negocio, ContextoSe contexto, T parametro)
        where T : struct, System.Enum
        {
            var parametroDtm = negocio.Parametro(contexto, parametro, errorSiNoHay: false, errorSinValor: false, errorSinMasDeUno: true);
            if (parametroDtm is not null)
                parametroDtm.Eliminar(contexto);
            return parametroDtm;
        }

        public static ParametroDeNegocioDtm DefinirParametro<T>(this enumNegocio negocio, ContextoSe contexto, T parametro, string valor = "")
        where T : struct, System.Enum
        {
            var parametroLeido = negocio.Parametro(contexto, parametro, errorSiNoHay: false, errorSinValor: false, errorSinMasDeUno: true);
            if (parametroLeido is null)
                return new ParametroDeNegocioDtm
                {
                    IdNegocio = negocio.IdNegocio(),
                    Nombre = parametro.ToString(),
                    Valor = valor,
                }.Insertar(contexto);

            return parametroLeido;
        }

        public static ParametroDeNegocioDtm LeerCrearParametro<T>(this enumNegocio negocio, ContextoSe contexto, T parametro, string valor)
        where T : struct, System.Enum
        {
            var parametroLeido = negocio.Parametro(contexto, parametro, errorSiNoHay: false, errorSinValor: false, errorSinMasDeUno: true);
            return parametroLeido is null ? negocio.CrearParametro(contexto, parametro, valor) : parametroLeido;
        }

        public static ParametroDeNegocioDtm CrearParametro<T>(this enumNegocio negocio, ContextoSe contexto, T parametro, string valor)
        where T : struct, System.Enum
        {
            return new ParametroDeNegocioDtm
            {
                IdNegocio = negocio.IdNegocio(),
                Nombre = parametro.ToString(),
                Valor = valor,
            }.InsertarComoAdministrador(contexto, parametros: new Dictionary<string, object> {{ ltrParametrosNeg.ValidarPermisosDePersistencia, false }});
        }

        public static ParametroDeNegocioDtm LeerParametro<T>(this enumNegocio negocio, ContextoSe contexto, T parametro, bool errorSinValor = true, bool errorSiNoHay = true)
        where T : struct, System.Enum
        {
            var parametroLeido = negocio.Parametro(contexto, parametro, errorSiNoHay: errorSiNoHay, errorSinValor: errorSinValor, errorSinMasDeUno: true);
            return parametroLeido;
        }

        public static ParametroDeNegocioDtm Parametro<T>(this enumNegocio negocio, ContextoSe contexto, T parametro, bool errorSiNoHay, bool errorSinValor, bool errorSinMasDeUno = true)
        where T : struct, System.Enum
        {

            var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_ParametrosDeNegocio);
            var indice = $"{negocio}-{parametro}";

            if (!cache.ContainsKey(indice))
            {
                var filtros = new Dictionary<string, object>
                           {
                               {nameof(ParametroDeNegocioDtm.IdNegocio), negocio.IdNegocio() },
                               {nameof(ParametroDeNegocioDtm.Nombre), parametro }
                           };
                var registro = contexto.SeleccionarTodos<ParametroDeNegocioDtm>(filtros);

                if (errorSiNoHay && registro.Count() == 0)
                    GestorDeErrores.Emitir($"Debe definir para el negocio de '{negocio.Singular()}' el parámetro '{parametro}'");

                if (errorSinValor && registro.Count() == 1 && registro[0].Valor.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"Debe definir un valor para el parámetro '{parametro}' del negocio '{negocio.Singular()}'");

                if (errorSinMasDeUno && registro.Count() > 1)
                    GestorDeErrores.Emitir($"Hay más de un parámetro definido '{parametro}' para el negocio '{negocio.Singular()}'");

                if (registro.Count() == 0) return null;

                cache[indice] = registro[0];
            }

            return (ParametroDeNegocioDtm)cache[indice];
        }

    }
}
