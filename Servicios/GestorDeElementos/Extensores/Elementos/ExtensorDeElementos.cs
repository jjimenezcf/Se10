using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using Microsoft.Win32;
using ModeloDeDto;
using ModeloDeDto.Callejero;
using ModeloDeDto.Negocio;
using ModeloDeDto.SistemaDocumental;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Guarderias;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Reflection;
using System.Text.RegularExpressions;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeElementos
    {

        public static bool SonIguales(this IElementoDtm origen, IElementoDtm detino, List<string> propiedadesNoComparables)
        {
            var propiedades = origen.PropiedadesDelObjeto();
            foreach (var propiedad in propiedades)
            {
                if (propiedadesNoComparables.Contains(propiedad.Name))
                    continue;

                if (propiedad.GetSetMethod() == null)
                    continue;

                if (Nullable.GetUnderlyingType(propiedad.PropertyType) == null && !ApiDeEnsamblados.ImplementaInterface(propiedad.PropertyType, typeof(IComparable).FullName))
                    continue;

                if (propiedad.GetValue(origen) == null && propiedad.GetValue(detino) == null)
                    continue;

                if (propiedad.GetValue(origen) == null) return false;
                if (propiedad.GetValue(detino) == null) return false;

                if (!propiedad.GetValue(origen).Equals(propiedad.GetValue(detino)))
                    return false;
            }
            return true;
        }

        public static TipoDeElementoDtm AntesDePersistirValidarTipo(this IElementoConTipo elemento, ContextoSe contexto, ParametrosDeNegocio parametros)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            var gestorDeTipos = negocio.CrearGestorDeTipo(contexto);
            var tipoDeElemento = TipoDeElementoSql.LeerTipoPorId(contexto, negocio.ObtenerMetadatos().TipoDtm, elemento.IdTipo);
            parametros.TipoDeElemento = tipoDeElemento;

            if (!tipoDeElemento.Activo)
                GestorDeErrores.Emitir($"El tipo del elemento '{tipoDeElemento.Nombre}' del negocio '{negocio}' no está activo, sólo permite consultas");

            if (parametros.Insertando && !tipoDeElemento.PermiteCrear)
                GestorDeErrores.Emitir($"No se puede crear registros del tipo '{tipoDeElemento.Nombre}' esta marcado que no permite creación");

            if (!tipoDeElemento.Mascara.IsNullOrEmpty() && !Regex.IsMatch(((INombre)elemento).Nombre, tipoDeElemento.Mascara))
                GestorDeErrores.Emitir($"El elemento '{((IElementoDtm)elemento).Referencia(contexto)}' no validad el patrón '{tipoDeElemento.Marcador}'");

            if (parametros.Modificando && !tipoDeElemento.NombreModificable)
            {
                var nuevoNombre = NormalizarCadena(((INombre)elemento).Nombre);
                var nombreenBd = NormalizarCadena(((INombre)parametros.registroEnBd).Nombre);
                if (nombreenBd != nuevoNombre && parametros.AccionQueSeEjecuta != ltrDeUnElemento.Accion_Renombrar)
                    GestorDeErrores.Emitir($"No se puede modificar el nombre {negocio.ConArticulo()} '{elemento.Referencia}' por  estar su tipo marcado como no modificable");
            }

            if (NegociosDeSe.UsaEstado(negocio))
            {
                var tipo = (TipoConFlujoDtm)gestorDeTipos.LeerRegistroPorId(elemento.IdTipo, true);
                parametros.TipoConFujo = tipo;
                if (parametros.Operacion == enumTipoOperacion.Insertar) ((IUsaEstado)elemento).IdEstado = tipo.Estado.Id;

                if (parametros.Modificando && elemento.IdTipo != ((IUsaTipo)parametros.registroEnBd).IdTipo)
                    GestorDeErrores.Emitir("No se puede cambiar el tipo una vez creado, use el método de reasignar tipo o cree un elemento nuevo");
            }
            return tipoDeElemento;
        }

        private static string NormalizarCadena(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;

            // Eliminar espacios múltiples y trim
            string resultado = Regex.Replace(input, @"\s+", " ").Trim();

            // Eliminar secuencias de escape literales (\n)
            resultado = resultado.Replace("\\n", " ")
                                   .Replace("%", "")
                                   .Replace(" ", "");


            return resultado;
        }

        public static void AntesDePersistirValidarCg(ContextoSe contexto, IUsaCg regConCg, ParametrosDeNegocio parametros)
        {
            var cg = contexto.SeleccionarPorId<CentroGestorDtm>(regConCg.IdCg);
            if (cg.Baja)
                GestorDeErrores.Emitir($"El C.G. {cg.Expresion} no está activo, sólo permite consultas");

            if (parametros.Modificando && regConCg.IdCg != ((IUsaCg)parametros.registroEnBd).IdCg)
                GestorDeErrores.Emitir("No se puede cambiar el centro gestor una vez creado, use el método de reasignar CG o cree un elemento nuevo");
        }

        internal static IQueryable<TRegistro> FiltrarPorBaja<TRegistro>(this IQueryable<TRegistro> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros) where TRegistro : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaBaja(typeof(TRegistro)))
                return consulta;

            if (parametros.IncluirBajas && filtros.Where(x => x.Clausula == ltrParametrosNeg.FiltrarPorBaja).Count() == 0)
                filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.FiltrarPorBaja, enumCriteriosDeFiltrado.igual, ltrParametrosNeg.MostrarTodos));

            var filtro = filtros.FirstOrDefault(x => (x.Clausula.ToLower() == ltrParametrosNeg.FiltrarPorBaja.ToLower() || x.Clausula.ToLower() == ltrParametrosNeg.IncluirBajas.ToLower()) && x.Valor == ltrParametrosNeg.MostrarTodos.ToString());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosNeg.SoloEnAlta.ToLower());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                parametros.Parametros[ltrParametrosNeg.IncluirBajas] = filtro.Valor == false.ToString();
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosNeg.FiltrarPorBaja.ToLower());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                if (filtro.Valor.Entero() == ltrParametrosNeg.MostrarBajas)
                {
                    consulta = consulta.Where(x => ((IUsaBaja)x).Baja);
                    return consulta;
                }
                parametros.IncluirBajas = filtro.Valor.Entero() == ltrParametrosNeg.MostrarTodos;
            }

            if (!parametros.IncluirBajas)
            {
                consulta = consulta.Where(x => !((IUsaBaja)x).Baja);
                return consulta;
            }

            filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrParametrosNeg.IncluirBajas.ToLower());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                consulta = filtro.Valor == false.ToString() ? consulta.Where(x => !((IUsaBaja)x).Baja) : consulta.Where(x => ((IUsaBaja)x).Baja);
                return consulta;
            }

            consulta = consulta.Where(x => !((IUsaBaja)x).Baja);

            return consulta;
        }

        internal static IQueryable<TRegistro> FiltrarPorCg<TRegistro>(this IQueryable<TRegistro> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        where TRegistro : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaCg(typeof(TRegistro)))
                return consulta;

            consulta = consulta.FiltrarPorSociedad(contexto, filtro);

            if (filtro.Clausula.Equals(nameof(IUsaCg.IdCg), StringComparison.InvariantCultureIgnoreCase))
            {
                var a = CentroGestorSql.CentrosGestoresDependientes(contexto, filtro.Valor.Entero());
                filtro.Criterio = enumCriteriosDeFiltrado.esAlgunoDe;
                foreach (var b in a) filtro.Valor = $"{filtro.Valor}, {b.Id}";
                consulta = consulta.AplicarFiltroPorEntero(filtro, nameof(PuestoDtm.IdCg));
                filtro.Aplicado = true;
            }
            if (filtro.Clausula.Equals(ltrFiltros.NombreCg, StringComparison.InvariantCultureIgnoreCase))
            {
                filtro.Clausula = "Cg.Nombre";
                consulta = consulta.AplicarFiltroDeCadena(filtro);
            }
            return consulta;
        }

        internal static IQueryable<TRegistro> FiltrarPorSociedad<TRegistro>(this IQueryable<TRegistro> consulta, ContextoSe contexto, ClausulaDeFiltrado filtro)
        where TRegistro : RegistroDtm
        {
            if (filtro.Clausula.Equals(ltrDeSociedad.FiltroPorIdSociedad, StringComparison.InvariantCultureIgnoreCase) ||
                filtro.Clausula.Equals(nameof(IElementoDeUnProcesoDto.IdSociedadDelCg), StringComparison.InvariantCultureIgnoreCase))
            {
                consulta = consulta.AplicarFiltroPorEntero(filtro, $"{nameof(IUsaCg.Cg)}.{nameof(CentroGestorDtm.Sociedad)}.{nameof(SociedadDtm.Id)}");
                filtro.Aplicado = true;
            }

            return consulta;
        }

        internal static IQueryable<TRegistro> FiltrarPorTipo<TRegistro>(this IQueryable<TRegistro> consulta, ContextoSe contexto, enumNegocio negocio, ClausulaDeFiltrado filtro, ParametrosDeNegocio parametros)
        where TRegistro : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaElementoConTipo(typeof(TRegistro)))
                return consulta;

            if (filtro.Clausula.Equals(ltrFiltros.NombreTipo, StringComparison.InvariantCultureIgnoreCase))
            {
                filtro.Clausula = "Tipo.Nombre";
                consulta = consulta.AplicarFiltroDeCadena(filtro);
            }
            else
            {
                if (!filtro.Clausula.Equals(nameof(IUsaTipo.IdTipo), StringComparison.InvariantCultureIgnoreCase))
                    return consulta;

                if (filtro.Criterio == enumCriteriosDeFiltrado.noEsNingunoDe)
                {
                    consulta = consulta.AplicarFiltroPorEntero(filtro);
                    parametros.HayFiltroPorTipo = true;
                    return consulta;
                }

                var tipos = filtro.Valor.Entero() > 0
                ? negocio.ObtenerIdsDelTipoConHijos(contexto, filtro.Valor.Entero())
                : filtro.Valor.ToLista<int>();

                filtro.Valor = tipos.ToString(Simbolos.Coma);
                if (tipos.Count > 1) filtro.Criterio = enumCriteriosDeFiltrado.esAlgunoDe;

                consulta = consulta.AplicarFiltroPorEntero(filtro);
            }

            parametros.HayFiltroPorTipo = true;
            return consulta;
        }

        internal static IQueryable<TRegistro> FiltrarPorEstado<TRegistro>(this IQueryable<TRegistro> consulta, ContextoSe contexto, enumNegocio negocio, ClausulaDeFiltrado filtro, bool aplicarFiltroQueMostrar)
        where TRegistro : RegistroDtm
        {
            if (filtro.Aplicado)
                return consulta;

            if (!ApiDeInterfaceDtm.ImplementaUsaFlujo(typeof(TRegistro)))
                return consulta;

            if (filtro.Clausula.Equals(ltrParametrosNeg.ExcluirCancelados, StringComparison.InvariantCultureIgnoreCase))
            {
                if (filtro.Valor == true.ToString())
                {
                    var listaDeCancelados = negocio.TipoDtm().Cancelados(contexto);
                    consulta = consulta.Where(x => !listaDeCancelados.Contains(((IUsaEstado)x).IdEstado));
                }
                filtro.Aplicado = true;
            }

            if (filtro.Clausula.Equals(ltrParametrosNeg.ExcluirTerminados, StringComparison.InvariantCultureIgnoreCase))
            {
                if (filtro.Valor == true.ToString())
                {
                    var listaDeTerminados = negocio.TipoDtm().Terminados(contexto);
                    consulta = consulta.Where(x => !listaDeTerminados.Contains(((IUsaEstado)x).IdEstado));
                }
                filtro.Aplicado = true;
            }

            if (filtro.Clausula.Equals(ltrFiltros.Estados, StringComparison.InvariantCultureIgnoreCase))
            {
                if (!filtro.Valor.IsNullOrEmpty())
                {
                    var filtrarPorDistinto = false;
                    if (filtro.Valor.StartsWith(Simbolos.filtroPorDistinto))
                    {
                        filtro.Valor = filtro.Valor.Substring(1);
                        filtrarPorDistinto = true;
                    }
                    var listaDeEstados = filtro.Valor.ToLista<string>(Simbolos.separadorDeCadenasDeFiltrado);
                    var estados = contexto.Estados(negocio).Where(e => listaDeEstados.Any(estado => e.Nombre.Contains(estado)));
                    consulta = filtrarPorDistinto
                        ? consulta.Where(x => !estados.Any(e => e.Id == ((IElementoDeProcesoDtm)x).IdEstado))
                        : consulta.Where(x => estados.Any(e => e.Id == ((IElementoDeProcesoDtm)x).IdEstado));
                }
                filtro.Aplicado = true;
            }

            if (filtro.Clausula.Equals(ltrFiltros.IdsDeEstado, StringComparison.InvariantCultureIgnoreCase))
            {
                var listaDeEstados = filtro.Valor.ToLista<int>(Simbolos.Coma);
                var estados = contexto.Estados(negocio).Where(estado => listaDeEstados.Any(idEstado => estado.Id == idEstado));
                consulta = consulta.Where(elemento => estados.Any(estado => estado.Id == ((IElementoDeProcesoDtm)elemento).IdEstado));
                filtro.Aplicado = true;
            }

            if (filtro.Clausula.Equals(ltrParametrosNeg.QueMostrar, StringComparison.InvariantCultureIgnoreCase) && aplicarFiltroQueMostrar)
            {
                if (negocio.ObtenerMetadatos().TipoEtapas is not null && Enum.IsDefined(negocio.ObtenerMetadatos().TipoEtapas, filtro.Valor))
                {
                    var listaDeEstados = (List<int>)negocio.ObtenerMetadatos().EstadosDeLaEtapa(ApiDeEnsamblados.ToEnumerado(negocio.ObtenerMetadatos().TipoEtapas, filtro.Valor));
                    consulta = consulta.AplicarFiltroPorListaDeEnteros(nameof(IUsaEstado.IdEstado), listaDeEstados);
                }
                else
                {
                    if (filtro.Valor.Contains(ltrParametrosNeg.TodosMenosCanceladas.ToString()))
                    {
                        var listaDeCancelados = negocio.TipoDtm().Cancelados(contexto);
                        consulta = consulta.Where(x => !listaDeCancelados.Contains(((IUsaEstado)x).IdEstado));
                    }
                    if (filtro.Valor.Contains(ltrParametrosNeg.Cancelados.ToString()))
                    {
                        var listaDeCancelados = negocio.TipoDtm().Cancelados(contexto);
                        if (filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                            consulta = consulta.Where(x => !listaDeCancelados.Contains(((IUsaEstado)x).IdEstado));
                        else
                            consulta = consulta.Where(x => listaDeCancelados.Contains(((IUsaEstado)x).IdEstado));
                    }
                    if (filtro.Valor.Contains(ltrParametrosNeg.Terminados.ToString()))
                    {
                        var listaDeTerminados = negocio.TipoDtm().Terminados(contexto);
                        if (filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                            consulta = consulta.Where(x => !listaDeTerminados.Contains(((IUsaEstado)x).IdEstado));
                        else
                            consulta = consulta.Where(x => listaDeTerminados.Contains(((IUsaEstado)x).IdEstado));
                    }
                    if (filtro.Valor.Contains(ltrParametrosNeg.Iniciales.ToString()))
                    {
                        var listaDeIniciales = negocio.TipoDtm().Iniciales(contexto);
                        if (filtro.Criterio == enumCriteriosDeFiltrado.diferente)
                            consulta = consulta.Where(x => !listaDeIniciales.Contains(((IUsaEstado)x).IdEstado));
                        else
                            consulta = consulta.Where(x => listaDeIniciales.Contains(((IUsaEstado)x).IdEstado));
                    }
                }
                filtro.Aplicado = true;
            }

            return consulta;
        }

        internal static (EstadoAuditado estado, TransicionAuditada transicion) PrepararFiltrosPorHitos<TRegistro>(ClausulaDeFiltrado filtro, (EstadoAuditado estado, TransicionAuditada transicion) hito)
        where TRegistro : RegistroDtm
        {
            if (!ApiDeInterfaceDtm.ImplementaUsaFlujo(typeof(TRegistro)))
                return hito;

            if (filtro.Clausula == ltrEstados.IdEstadoAuditado && filtro.Valor.Entero() > 0)
            {
                if (hito.estado == null) hito.estado = new EstadoAuditado();
                hito.estado.idEstado = filtro.Valor.Entero();
                filtro.Aplicado = true;
                return hito;
            }
            if (filtro.Clausula == ltrTransiciones.IdTransicionAuditada && filtro.Valor.Entero() > 0)
            {
                if (hito.transicion == null) hito.transicion = new TransicionAuditada();
                hito.transicion.idTransicion = filtro.Valor.Entero();
                filtro.Aplicado = true;
                return hito;
            }
            if (filtro.Clausula == ltrEstados.FechaEstadoAuditado && !filtro.Valor.IsNullOrEmpty())
            {
                if (hito.estado == null) hito.estado = new EstadoAuditado();
                hito.estado.fechas = filtro.Valor;
                filtro.Aplicado = true;
                return hito;
            }
            if (filtro.Clausula == ltrTransiciones.FechaTransicionAuditado && !filtro.Valor.IsNullOrEmpty())
            {
                if (hito.transicion == null) hito.transicion = new TransicionAuditada();
                hito.transicion.fechas = filtro.Valor;
                filtro.Aplicado = true;
                return hito;
            }

            return hito;

        }

        public static bool EstaEnEtapa(ContextoSe contexto, enumNegocio negocio, int idProceso, string etapa)
        {
            var proceso = (IElementoDeProcesoDtm)negocio.ElementoPorId(contexto, idProceso);
            //if (!etapa.Replace(",", "").EsNumero())
            //    etapa = CacheDeVariable.LeerValorDeVariable(etapa);
            var estados = etapa.ToLista<int>(Simbolos.Coma);
            return estados.Contains(proceso.IdEstado);
        }

        public static bool PropiedadCambiada<T>(this IRegistro nuevo, string propiedad, ParametrosDeNegocio parametros)
        {
            if (!parametros.Modificando) return false;
            return nuevo.PropiedadCambiada<T>(parametros.registroEnBd, propiedad);
        }

        public static bool PropiedadCambiada<T>(this IRegistro nuevo, IRegistro anterior, string propiedad)
        {
            T valorNuevo = (T)nuevo.LeerPropiedad(propiedad);
            T valorAnterior = (T)anterior.LeerPropiedad(propiedad);

            if (typeof(T).BaseType == typeof(Enum))
                return (valorAnterior == null && valorNuevo != null) ||
                       (valorAnterior != null && valorNuevo == null) ||
                       (valorAnterior != null && valorNuevo != null && !valorNuevo.Equals(valorAnterior));

            if (typeof(T) == typeof(int) || typeof(T) == typeof(decimal) || typeof(T) == typeof(bool) || typeof(T) == typeof(DateTime)) return !valorAnterior.Equals(valorNuevo);

            if (typeof(T) == typeof(int?) || typeof(T) == typeof(decimal?) || typeof(T) == typeof(DateTime?))
                return (valorAnterior == null && valorNuevo != null) ||
                       (valorAnterior != null && valorNuevo == null) ||
                       (valorAnterior != null && valorNuevo != null && !valorNuevo.Equals(valorAnterior));

            if (typeof(T) == typeof(string))
                return (valorAnterior == null && valorNuevo != null)
                    || (valorAnterior != null && valorNuevo == null)
                    || (valorAnterior != null && valorNuevo != null && !valorNuevo.Equals(valorAnterior));

            throw new Exception($"No está implementado en el método {nameof(PropiedadCambiada)} como tratar el tipo {typeof(T).Name}");
        }

        public static bool AsignarValor<T>(this ElementoDtm nuevo, ElementoDtm anterior, string propiedad)
        {
            if (typeof(T) == typeof(int?))
            {
                int? valorNuevo = (int?)nuevo.LeerPropiedad(propiedad);
                int? valorAnteriro = (int?)anterior.LeerPropiedad(propiedad);
                return valorAnteriro == null && valorNuevo != null;
            }

            throw new Exception($"No está implementado en el método {nameof(AsignarValor)} como tratar el tipo {typeof(T).Name}");
        }

        public static bool QuitarValor<T>(this ElementoDtm nuevo, ElementoDtm anterior, string propiedad)
        {
            if (typeof(T) == typeof(int?))
            {
                int? valorNuevo = (int?)nuevo.LeerPropiedad(propiedad);
                int? valorAnteriro = (int?)anterior.LeerPropiedad(propiedad);
                return valorAnteriro != null && valorNuevo == null;
            }

            throw new Exception($"No está implementado en el método {nameof(QuitarValor)} como tratar el tipo {typeof(T).Name}");
        }

        public static bool SeHaModificadoElCampo<T>(this RegistroDtm registro, Func<PropertyInfo, bool> predicado, ParametrosDeNegocio parametros)
        {
            if (parametros.Modificando)
            {
                var anterior = (RegistroDtm)parametros.registroEnBd;
                var valorAnterior = anterior.Valor<T>(predicado);
                var valorActual = registro.Valor<T>(predicado);
                return !valorActual.Equals(valorAnterior);
            }
            return false;
        }

        public static bool SoloSeHaModificadoElCampo(this IElementoDtm registro, ParametrosDeNegocio parametros, string propiedad)
        => parametros.Modificando
        ? SonIguales(registro, (IElementoDtm)parametros.registroEnBd, propiedadesNoComparables: new List<string> { propiedad })
        : false;

        public static IElementoDtm ElementoPorId(this enumNegocio negocio, ContextoSe contexto, int id, bool aplicarJoin = false, bool usarLaCache = true, bool errorSiNoHay = true)
        {
            var tipo = negocio.TipoDtm();

            if (tipo.ImplementaElementoDeUnProceso())
                return negocio.ElementoDeProcesoPorId(contexto, id, aplicarJoin, usarLaCache, errorSiNoHay);

            if (!tipo.ImplementaUnElemento())
                throw new Exception($"No se puede usar el método '{nameof(ElementoPorId)}' para el tipo '{tipo.Name}', ya que no implementa la interface '{nameof(IElementoDtm)}'");

            var parametros = new Dictionary<string, object>();
            parametros[ltrParametrosNeg.ErrorSiNoLoHay] = errorSiNoHay;

            return (IElementoDtm)negocio.CrearGestor(contexto, tipo).LeerRegistroPorId(id, aplicarJoin, usarLaCache, parametros: parametros);
        }

        public static IRegistro MapearDtm(this IElementoDto elemento, ContextoSe contexto)
        {
            var negocio = NegociosDeSe.NegocioDeUnDto(elemento.GetType());
            return negocio.MapearDtm(contexto, elemento);
        }

        public static T MapearDtm<T>(this IElementoDto elemento, ContextoSe contexto)
        =>
        (T)elemento.MapearDtm(contexto);

        public static ArchivoDtm AnexarArchivoConBloqueo(this IUsaReferencia elemento, ContextoSe contexto, string rutaConFichero, string motivoDeBloqueo)
        {
            var archivo = elemento.AnexarArchivo(contexto, rutaConFichero);
            new BloqueoDeUnArchivoDtm
            {
                IdArchivo = archivo.Id,
                Bloqueado = true
            }.Insertar(contexto, new Dictionary<string, object> { { nameof(AuditoriaDeUnArchivoDtm.Auditoria), motivoDeBloqueo } });
            return archivo;
        }

        public static ArchivoDtm AnexarArchivo(this IUsaReferencia elemento, ContextoSe contexto, string rutaConFichero, bool copiar = false)
        {
            var negocio = elemento.GetType().NegocioDeUnDtm();
            if (!negocio.UsaArchivos())
                GestorDeErrores.Emitir($"Ha solicitado anexar un archivo al elemento '{elemento.Referencia}' del negocio '{negocio.Singular(true)}', y éste no usa archivos");
            var sanitizar = true;
            return (ArchivoDtm)ApiDeEnsamblados.EjecutarMetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, ApiDeEnsamblados.ClaseDelServidorDocumental,
                                copiar ? ApiDeEnsamblados.MetodoDeCopiarArchivo : ApiDeEnsamblados.MetodoDeAnexarArchivo,
                                new object[] { contexto, negocio, ((IRegistro)elemento).Id, rutaConFichero, sanitizar });
        }

        public static ArchivoDtm QuitarArchivo(this IUsaReferencia elemento, ContextoSe contexto, string rutaConFichero)
        {
            var negocio = elemento.GetType().NegocioDeUnDtm();
            if (!negocio.UsaArchivos())
                GestorDeErrores.Emitir($"Ha solicitado anexar un archivo al elemento '{elemento.Referencia}' del negocio '{negocio.Singular(true)}', y éste no usa archivos");

            return (ArchivoDtm)ApiDeEnsamblados.EjecutarMetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio,
                ApiDeEnsamblados.ClaseDelServidorDocumental,
                ApiDeEnsamblados.MetodoDeAnexarArchivo,
                new object[] { contexto, negocio, ((IRegistro)elemento).Id, rutaConFichero });
        }

        public static void ValidarQueReferenciaA<T>(this IElementoDtm elemento, ContextoSe contexto, string propiedad, string nombreDelObjeto = null)
        where T : RegistroDtm
        {
            var negocioReferenciado = NegociosDeSe.NegocioDeUnDtm(typeof(T));
            var referenciado = negocioReferenciado.SeleccionarPorPropiedad<T>(contexto, propiedad, elemento.Id.ToString(), errorSiNoHay: false, errorSiMasDeuno: false);
            if (referenciado is null)
            {
                var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
                GestorDeErrores.Emitir($"El/La {negocio.Singular(true)} {(negocio.UsaReferencia() ? ((IUsaReferencia)elemento).Referencia : (elemento).Nombre)} no referencia un/a" +
                    $" {(nombreDelObjeto.IsNullOrEmpty() ? negocioReferenciado.Singular(true) : nombreDelObjeto)}");
            }
        }

        public static ElementoDtm Elemento(this IDetalle detalle, ContextoSe contexto)
        {
            return detalle.Elemento == null
                ? detalle.Negocio.SeleccionarPorId(contexto, detalle.IdElemento, aplicarJoin: true, usarLaCache: false)
                : detalle.Elemento;
        }

        public static string Referencia(this IDetalle detalle, ContextoSe contexto)
        {
            var elemento = detalle.Elemento(contexto);
            return Referencia(elemento, contexto);
        }

        public static string Referencia(this IElementoDtm elemento, ContextoSe contexto = null)
        {
            if (contexto != null && elemento.GetType().ImplementaEsUnInterlocutor())
            {
                if (((IEsInterlocutor)elemento).Interlocutor != null)
                    return ((IEsInterlocutor)elemento).Interlocutor.Referencia(contexto);

                return contexto.SeleccionarPorId<InterlocutorDtm>(((IEsInterlocutor)elemento).IdInterlocutor, aplicarJoin: true).Referencia(contexto);
            }

            return elemento.GetType().ImplementaUsaReferencia() ? ((IUsaReferencia)elemento).Referencia : elemento.Nombre;
        }

        public static void AsociarArchivo<T>(this T registro, ContextoSe contexto, int idArchivo, string accion)
        where T : ElementoDtm
        {
            registro = contexto.SeleccionarPorId<T>(registro.Id, aplicarJoin: true);
            ((IUsaArchivo)registro).IdArchivo = idArchivo;
            registro.Modificar(contexto, esUnaAccion: true, parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, accion } });
            GestorDeVinculos.Vincular(contexto, NegociosDeSe.ToEnumerado(NegociosDeSe.LeerNegocioPorDtm(typeof(T).FullName)), enumNegocio.Archivos, registro.Id, idArchivo);
        }

        public static List<RegistroDtm> ObtenerRegistrosAnexadosAlArchivo(ContextoSe contexto, enumNegocio negocio, int idArchivo, bool incluirBajas = false, bool incluirCancelados = false, bool incluirTerminados = true)
        {
            var vinculados = VinculoSql.LeerVinculosAl(contexto, negocio.TipoDtm(), enumNegocio.Archivos, typeof(ArchivoDtm), idArchivo, filtros: null);
            if (vinculados.Count == 0) return new List<RegistroDtm>();

            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado { Clausula = ltrFiltros.Id, Criterio = enumCriteriosDeFiltrado.esAlgunoDe, Valor = string.Join(",", vinculados.Select(o => o.idElemento1)) } };
            if (negocio.UsaBaja())
                filtros.Add(new ClausulaDeFiltrado { Clausula = ltrParametrosNeg.IncluirBajas, Criterio = enumCriteriosDeFiltrado.igual, Valor = incluirBajas.ToString() });
            else
                if (negocio.UsaFlujo())
                {
                    filtros.Add(new ClausulaDeFiltrado { Clausula = ltrParametrosNeg.ExcluirCancelados, Criterio = enumCriteriosDeFiltrado.igual, Valor = incluirCancelados.ToString() });
                    filtros.Add(new ClausulaDeFiltrado { Clausula = ltrParametrosNeg.ExcluirCancelados, Criterio = enumCriteriosDeFiltrado.igual, Valor = incluirTerminados.ToString() });
                }

            var elementos = negocio.SeleccionarRegistros(contexto, filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
            return elementos;
        }

        public static int ContarAnexadosAlArchivo(ContextoSe contexto, enumNegocio negocio, int idArchivo)
        {
            return VinculoSql.ContarVinculosAl(contexto, negocio.TipoDtm(), enumNegocio.Archivos, idElemento2: idArchivo);
        }

        public static bool EsAdministrador(this IElementoDtm registro, ContextoSe contexto)
        {
            if (contexto.DatosDeConexion.EsAdministrador) return true;

            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());

            if (negocio.UsaTipo())
            {
                var gestorDeTipo = negocio.CrearGestorDeTipo(contexto);
                var tipo = (ITipoDeElementoDtm)gestorDeTipo.LeerRegistroPorId(((IUsaTipo)registro).IdTipo, aplicarJoin: false);

                return registro.EsAdministrador(gestorDeTipo, tipo);
            }

            var modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlElemento(contexto, negocio, (RegistroDtm)registro);
            if (!ModoDeAcceso.SoyAdministrador(modoAlElemento))
            {
                modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlNegocio(contexto, negocio);
                return ModoDeAcceso.SoyAdministrador(modoAlElemento);
            }

            return true;
        }

        private static bool EsAdministrador(this IElementoDtm registro, IGestorDeTipos gestorDeTipo, ITipoDeElementoDtm tipo)
        {
            var modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlElemento(gestorDeTipo.Contexto, gestorDeTipo.Negocio, (RegistroDtm)registro);
            var soyAdminDelElemento = ModoDeAcceso.SoyAdministrador(modoAlElemento);

            if (gestorDeTipo.Negocio.UsaCg())
            {
                var hayPermisosPorCg = ApiDePermisos.HayPermisosPorCg(gestorDeTipo.Contexto, gestorDeTipo.Negocio, ((IUsaCg)registro).IdCg, enumModoDeAccesoDeDatos.Gestor);
                if (!hayPermisosPorCg && !soyAdminDelElemento)
                    return false;
            }

            var hayPermisosPorTipo = PermisosPorTipoSql.UsuarioConAlgunPermiso(gestorDeTipo.Contexto, new List<int> { tipo.IdPermisoDeAdministrador });
            if (!hayPermisosPorTipo && soyAdminDelElemento)
                hayPermisosPorTipo = true;

            return hayPermisosPorTipo;
        }

        public static bool EsInterventor<T>(this IElementoDtm registro, ContextoSe contexto)
        where T : TipoDeElementoDtm
        {
            if (contexto.DatosDeConexion.EsAdministrador) return true;

            var gestorDeTipo = (IGestorDeTipos)NegociosDeSe.CrearGestorDeTipoDtm(contexto, typeof(T));

            var tipo = ((IUsaTipo)registro).Tipo<T>(contexto);
            return registro.EsInterventor(gestorDeTipo, tipo);
        }

        public static bool EsInterventor(this IElementoDtm registro, ContextoSe contexto)
        {
            if (contexto.DatosDeConexion.EsAdministrador) return true;

            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());

            if (negocio.UsaTipo())
            {
                var gestorDeTipo = negocio.CrearGestorDeTipo(contexto);
                var tipo = (ITipoDeElementoDtm)gestorDeTipo.LeerRegistroPorId(((IUsaTipo)registro).IdTipo, aplicarJoin: false);

                return registro.EsInterventor(gestorDeTipo, tipo);
            }

            var modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlElemento(contexto, negocio, (RegistroDtm)registro);
            if (!ModoDeAcceso.SoyInterventor(modoAlElemento))
            {
                modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlNegocio(contexto, negocio);
                return ModoDeAcceso.SoyInterventor(modoAlElemento);
            }

            return true;
        }

        private static bool EsInterventor(this IElementoDtm registro, IGestorDeTipos gestorDeTipo, ITipoDeElementoDtm tipo)
        {
            var modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlElemento(gestorDeTipo.Contexto, gestorDeTipo.Negocio, (RegistroDtm)registro);
            var soyGestorDelElemento = ModoDeAcceso.SoyGestor(modoAlElemento);

            if (gestorDeTipo.Negocio.UsaCg())
            {
                var hayPermisosPorCg = ApiDePermisos.HayPermisosPorCg(gestorDeTipo.Contexto, gestorDeTipo.Negocio, ((IUsaCg)registro).IdCg, enumModoDeAccesoDeDatos.Gestor);
                if (!hayPermisosPorCg && !soyGestorDelElemento)
                    return false;
            }

            var idPermisoInterventor = gestorDeTipo.Negocio.ObtenerMetadatos().TipoDtm.TienenLaPropiedad(nameof(IPermisoDeInterventor.IdPermisoInterventor))
                ? ((IPermisoDeInterventor)tipo).IdPermisoInterventor
                : tipo.IdPermisoDeGestor;

            var hayPermisosPorTipo = PermisosPorTipoSql.UsuarioConAlgunPermiso(gestorDeTipo.Contexto, new List<int> { tipo.IdPermisoDeAdministrador, idPermisoInterventor });
            if (!hayPermisosPorTipo && idPermisoInterventor == tipo.IdPermisoDeGestor && soyGestorDelElemento)
                hayPermisosPorTipo = true;

            return hayPermisosPorTipo;
        }

        public static bool EsGestor<T>(this IElementoDtm registro, ContextoSe contexto)
        where T : TipoDeElementoDtm
        {
            if (contexto.DatosDeConexion.EsAdministrador) return true;
            var gestorDeTipo = (IGestorDeTipos)NegociosDeSe.CrearGestorDeTipoDtm(contexto, typeof(T));
            var tipo = ((IUsaTipo)registro).Tipo<T>(contexto);

            return registro.EsGestor(gestorDeTipo, tipo);
        }

        public static bool EsGestor(this IElementoDtm registro, ContextoSe contexto)
        {
            if (contexto.DatosDeConexion.EsAdministrador) return true;

            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());

            if (negocio.UsaTipo())
            {
                var gestorDeTipo = negocio.CrearGestorDeTipo(contexto);
                var tipo = (ITipoDeElementoDtm)gestorDeTipo.LeerRegistroPorId(((IUsaTipo)registro).IdTipo, aplicarJoin: false);
                return registro.EsGestor(gestorDeTipo, tipo);
            }

            var modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlElemento(contexto, negocio, (RegistroDtm)registro);
            if (!ModoDeAcceso.SoyGestor(modoAlElemento))
            {
                modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlNegocio(contexto, negocio);
                return ModoDeAcceso.SoyGestor(modoAlElemento);
            }

            return true;
        }

        private static bool EsGestor(this IElementoDtm registro, IGestorDeTipos gestorDeTipo, ITipoDeElementoDtm tipo)
        {

            var modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlElemento(gestorDeTipo.Contexto, gestorDeTipo.Negocio, (RegistroDtm)registro);
            var soyGestorDelElemento = ModoDeAcceso.SoyGestor(modoAlElemento);

            if (gestorDeTipo.Negocio.UsaCg())
            {
                var hayPermisosPorCg = ApiDePermisos.HayPermisosPorCg(gestorDeTipo.Contexto, gestorDeTipo.Negocio, ((IUsaCg)registro).IdCg, enumModoDeAccesoDeDatos.Gestor);
                if (!hayPermisosPorCg && !soyGestorDelElemento)
                    return false;
            }

            var idPermisoInterventor = gestorDeTipo.Negocio.ObtenerMetadatos().TipoDtm.TienenLaPropiedad(nameof(IPermisoDeInterventor.IdPermisoInterventor))
                ? ((IPermisoDeInterventor)tipo).IdPermisoInterventor
                : 0;

            var hayPermisosPorTipo = PermisosPorTipoSql.UsuarioConAlgunPermiso(gestorDeTipo.Contexto, new List<int> { tipo.IdPermisoDeAdministrador, idPermisoInterventor, tipo.IdPermisoDeGestor });
            if (!hayPermisosPorTipo && soyGestorDelElemento)
                hayPermisosPorTipo = true;

            return hayPermisosPorTipo;
        }

        public static bool EsConsultor<T>(this IElementoDtm registro, ContextoSe contexto)
        where T : TipoDeElementoDtm
        {
            if (contexto.DatosDeConexion.EsAdministrador) return true;
            var gestorDeTipo = (IGestorDeTipos)NegociosDeSe.CrearGestorDeTipoDtm(contexto, typeof(T));
            var tipo = ((IUsaTipo)registro).Tipo<T>(contexto);
            return registro.EsConsultor(gestorDeTipo, tipo);
        }

        public static bool EsConsultor(this IElementoDtm registro, ContextoSe contexto)
        {
            if (contexto.DatosDeConexion.EsAdministrador) return true;

            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());

            if (negocio.UsaTipo())
            {
                var gestorDeTipo = negocio.CrearGestorDeTipo(contexto);
                var tipo = (ITipoDeElementoDtm)gestorDeTipo.LeerRegistroPorId(((IUsaTipo)registro).IdTipo, aplicarJoin: false);
                return registro.EsConsultor(gestorDeTipo, tipo);
            }

            var modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlElemento(contexto, negocio, (RegistroDtm)registro);
            if (!ModoDeAcceso.SoyConsultor(modoAlElemento))
            {
                modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlNegocio(contexto, negocio);
                return ModoDeAcceso.SoyConsultor(modoAlElemento);
            }

            return true;
        }

        private static bool EsConsultor(this IElementoDtm registro, IGestorDeTipos gestorDeTipo, ITipoDeElementoDtm tipo)
        {
            var modoAlElemento = ApiDePermisos.LeerModoDeAccesoAlElemento(gestorDeTipo.Contexto, gestorDeTipo.Negocio, (RegistroDtm)registro);
            var soyConsultorDelElemento = ModoDeAcceso.SoyConsultor(modoAlElemento);

            if (gestorDeTipo.Negocio.UsaCg())
            {
                var hayPermisosPorCg = ApiDePermisos.HayPermisosPorCg(gestorDeTipo.Contexto, gestorDeTipo.Negocio, ((IUsaCg)registro).IdCg, enumModoDeAccesoDeDatos.Consultor);
                if (!hayPermisosPorCg && !soyConsultorDelElemento)
                    return false;
            }

            var idPermisoInterventor = gestorDeTipo.Negocio.ObtenerMetadatos().TipoDtm.TienenLaPropiedad(nameof(IPermisoDeInterventor.IdPermisoInterventor))
                ? ((IPermisoDeInterventor)tipo).IdPermisoInterventor
                : 0;

            var hayPermisosPorTipo = PermisosPorTipoSql.UsuarioConAlgunPermiso(gestorDeTipo.Contexto, new List<int> { tipo.IdPermisoDeAdministrador, idPermisoInterventor, tipo.IdPermisoDeGestor, tipo.IdPermisoDeConsultor });
            if (!hayPermisosPorTipo && soyConsultorDelElemento)
                hayPermisosPorTipo = true;

            return hayPermisosPorTipo;
        }


        public static enumModoDeAccesoDeDatos PermisosDe(this IElementoDtm registro, ContextoSe contexto)
        {
            if (contexto.DatosDeConexion.EsAdministrador || registro.EsAdministrador(contexto)) return enumModoDeAccesoDeDatos.Administrador;
            if (registro.EsInterventor(contexto)) return enumModoDeAccesoDeDatos.Interventor;
            if (registro.EsGestor(contexto)) return enumModoDeAccesoDeDatos.Gestor;
            if (registro.EsConsultor(contexto)) return enumModoDeAccesoDeDatos.Consultor;

            return enumModoDeAccesoDeDatos.SinPermiso;
        }

        public static PermisosDelElementoDtm CrearPermisosDelElemento(this IElementoDtm registro, ContextoSe contexto)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());

            return (PermisosDelElementoDtm)ApiDeEnsamblados.EjecutarMetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio,
                ApiDeEnsamblados.ClaseDelPemisosDelElemento,
                ApiDeEnsamblados.MetodoDeCrearPermisosDelElemento,
                new object[] { contexto, negocio, registro.Id });
        }

        public static IQueryable<ArchivadorDtm> Archivadores(this IElementoDtm elemento, ContextoSe contexto, bool excluirEnBaja = true)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!negocio.UsaArchivadores())
                GestorDeErrores.Emitir($"El {negocio.Singular(true)} no usa {enumNegocio.Archivador.Plural(true)} y ha solictado obtener los vinculadas al '{elemento.Referencia(contexto)}'");

            var archivadores = contexto.Set<ArchivadorDtm>().Join(negocio.Archivadores(contexto).Where(vin => vin.idElemento1 == elemento.Id),
                                                 arc => arc.Id,
                                                 arcVin => arcVin.idElemento2,
                                                 (archivador, archivadorVinculado) => archivador);
            if (excluirEnBaja)
                return archivadores.Where(x => !x.Baja);

            return archivadores;
        }

        public static List<ExtensionDeArchivo> ArchivosExt(this IElementoDtm elemento, ContextoSe contexto, bool recursivo, bool incluirOriginal = false)
        {
            if (!recursivo) return elemento.ArchivosExt(contexto, incluirOriginal);

            var negocio = elemento.GetType().NegocioDeUnDtm();
            if (negocio == enumNegocio.Archivador)
                return ((ArchivadorDtm)elemento).ArchivosExt(contexto, padre: null);

            List<ExtensionDeArchivo> archivosExt = new List<ExtensionDeArchivo>();
            var archivosExtDirectos = elemento.ArchivosExt(contexto, incluirOriginal);
            archivosExt.AddRange(archivosExtDirectos);

            if (negocio.UsaArchivadores())
            {
                var archivadores = elemento.Archivadores(contexto);
                foreach (var archivador in archivadores)
                {
                    var archivosExtDelArchivador = archivador.ArchivosExt(contexto, padre: elemento);
                    archivosExt.AddRange(archivosExtDelArchivador);
                }
            }
            return archivosExt;
        }

        private static List<ExtensionDeArchivo> ArchivosExt(this IElementoDtm elemento, ContextoSe contexto, bool incluirOriginal)
        {
            // Primero, obtenemos la lista de archivos usando el método existente
            var archivos = elemento.Archivos(contexto, incluirOriginal);
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            // Luego, proyectamos esta lista a ArchivoExt
            var archivosExt = archivos.Select(archivo => new ExtensionDeArchivo
            {
                IdArchivo = archivo.Id,
                Expresion = archivo.Nombre,
                Negocio = negocio,
                IdElemento = elemento.Id,
                IdArchivador = null,
                IdCarpeta = null,
                Archivo = archivo,
                Elemento = elemento,
            }).ToList();

            return archivosExt;
        }

        public static List<ArchivoDtm> Archivos(this IElementoDtm elemento, ContextoSe contexto, bool incluirOriginal = false)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!negocio.UsaArchivos())
                GestorDeErrores.Emitir($"El {negocio.Singular(true)} no usa {enumNegocio.Archivos.Plural(true)} y ha solictado obtener los vinculadas al '{elemento.Referencia(contexto)}'");
            return negocio.Archivos(contexto, elemento.Id, incluirOriginal);
        }

        public static List<ArchivoDtm> Archivos(this enumNegocio negocio, ContextoSe contexto, int idRegistro, bool incluirOriginal = false)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Arc_Anexados);
            var indice = $"{negocio.TipoDtm()}-{idRegistro}";
            if (!cache.ContainsKey(indice))
            {
                var archivos = contexto.Set<ArchivoDtm>().Join(negocio.Archivos(contexto).Where(vin => vin.idElemento1 == idRegistro),
                                                 arc => arc.Id,
                                                 arcVin => arcVin.idElemento2,
                                                 (archivo, archivoVinculado) => archivo);

                if (!incluirOriginal)
                    archivos = archivos.Where(a => !contexto.Set<FirmadoDtm>().Any(f => f.IdOriginal == a.Id));
                cache[indice] = archivos.ToList();
            }
            return (List<ArchivoDtm>)cache[indice];
        }

        public static void HistorialDeArchivadores(this IElementoDtm registro, ContextoSe contexto, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros, int nivel = 1)
        {
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrSucesosFiltros.excluir);
            var valores = filtro.Valor.Split(Simbolos.separadorDeValores);
            if (valores.Contains(ltrSucesosExcluir.archivadores) || (nivel > 1 && valores.Contains(ltrSucesosExcluir.nivel2archivadores)))
                return;
            var archivadores = registro.Archivadores(contexto, excluirEnBaja: false).ToList();
            for (var i = archivadores.Count() - 1; i >= 0; i--)
            {
                var archivador = archivadores[i];
                var suceso = new HistorialDto();
                suceso.Id = sucesos.Count + 1;
                suceso.IdRegistro = archivador.Id;
                suceso.Elemento = archivador.Referencia(contexto);
                suceso.Clase = $"{ltrSucesosCss.suceso}{Simbolos.separadorCss}{ltrSucesosCss.archivador}";
                suceso.Suceso = $"Archivador asociado: {archivador.Nombre}";
                suceso.EstaCancelada = archivador.Baja;
                suceso.EstaTerminada = false;
                suceso.Usuario = contexto.SeleccionarPorId<UsuarioDtm>(archivador.IdUsuaCrea).Login;
                suceso.OcurridoEl = archivador.FechaCreacion;
                suceso.Detalle = archivador.Descripcion;
                suceso.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                suceso.Accion = "Abrir";
                suceso.AccionJs = ltrAccionesDeSucesos.AbrirArchivador;

                suceso.Negocio = enumNegocio.Archivador.Singular();
                suceso.enumNegocio = enumNegocio.Archivador;
                suceso.IdNegocio = enumNegocio.Archivador.IdNegocio();
                suceso.Nivel = nivel;
                suceso.IdElemento = registro.Id;

                sucesos.Add(suceso);
                archivador.HistorialDeObservaciones(contexto, sucesos, filtros);
                archivador.HistorialDeArchivos(contexto, sucesos, filtros);
                archivador.HistorialDeTrazas(contexto, sucesos, filtros, enumTraza.envioDeCorreo);
            }
        }

        public static void HistorialDeTrazas(this IElementoDtm registro, ContextoSe contexto, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros, enumTraza traza, int nivel = 1)
        {
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrSucesosFiltros.excluir);
            var valores = filtro.Valor.Split(Simbolos.separadorDeValores);
            if (traza == enumTraza.envioDeCorreo && (valores.Contains(ltrSucesosExcluir.correos) || (nivel > 1 && valores.Contains(ltrSucesosExcluir.nivel2correos))))
                return;

            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
            if (!negocio.UsaTrazas())
                return;

            var gestor = GestorDeTrazas.Gestor(contexto, negocio);
            var trazas = gestor.LeerRegistros(registro.Id, 0, -1).Where(trz => trz.Nombre == enumTraza.envioDeCorreo.Descripcion()).OrderBy(trz => trz.CreadaEl).ToList();
            for (var i = trazas.Count() - 1; i >= 0; i--)
            {
                var correo = trazas[i];
                int indiceTransicion = correo.Descripcion.IndexOf(ltrDeTrazas.asuntoTransicion) > 0 ? correo.Descripcion.IndexOf(ltrDeTrazas.asuntoTransicion) + ltrDeTrazas.asuntoTransicion.Length : -1;
                int indiceAsunto = indiceTransicion > 1 ? indiceTransicion : correo.Descripcion.IndexOf(ltrDeTrazas.asuntoCorreo) > 1 ? correo.Descripcion.IndexOf(ltrDeTrazas.asuntoCorreo) + ltrDeTrazas.asuntoCorreo.Length : -1;
                int indiceCuerpo = correo.Descripcion.IndexOf(ltrDeTrazas.cuerpoCorreo);
                var asunto = indiceAsunto == -1 || indiceCuerpo == -1 || indiceAsunto >= indiceCuerpo
                ? correo.Nombre
                : correo.Descripcion.Substring(indiceAsunto, indiceCuerpo - indiceAsunto).Replace(Environment.NewLine, "").Trim();

                var suceso = new HistorialDto();
                suceso.Id = sucesos.Count + 1;
                suceso.IdRegistro = correo.Id;
                suceso.Elemento = registro.Referencia(contexto);
                suceso.Clase = $"{ltrSucesosCss.suceso}{Simbolos.separadorCss}{ltrSucesosCss.correo}";
                suceso.Suceso = asunto; // $"Correo: {asunto}";
                suceso.EstaCancelada = false;
                suceso.EstaTerminada = false;
                suceso.Usuario = contexto.SeleccionarPorId<UsuarioDtm>(correo.IdCreador).Login;
                suceso.OcurridoEl = correo.CreadaEl;
                suceso.Detalle = correo.Descripcion;
                suceso.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                suceso.Accion = "Mostrar";
                suceso.AccionJs = ltrAccionesDeSucesos.MostrarCorreo;

                suceso.Negocio = negocio.Singular();
                suceso.enumNegocio = negocio;
                suceso.IdNegocio = negocio.IdNegocio();
                suceso.Nivel = nivel;
                suceso.IdElemento = registro.Id;

                sucesos.Add(suceso);
            }
        }


        public static void HistorialDeObservaciones(this IElementoDtm registro, ContextoSe contexto, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros, int nivel = 1)
        {
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrSucesosFiltros.excluir);
            var valores = filtro.Valor.Split(Simbolos.separadorDeValores);
            if (valores.Contains(ltrSucesosExcluir.observaciones) || (nivel > 1 && valores.Contains(ltrSucesosExcluir.nivel2ob)))
                return;
            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
            var observaciones = negocio.Observaciones(contexto).Where(o => o.IdElemento == registro.Id).OrderBy(o => o.CreadaEl).ToList();
            for (var i = observaciones.Count() - 1; i >= 0; i--)
            {
                var observacion = observaciones[i];
                var suceso = new HistorialDto();
                suceso.Id = sucesos.Count + 1;
                suceso.IdRegistro = observacion.Id;
                suceso.Elemento = registro.Referencia(contexto);
                suceso.Clase = $"{ltrSucesosCss.suceso}{Simbolos.separadorCss}{ltrSucesosCss.observacion}";
                suceso.Suceso = observacion.Nombre; // $"Observacion añadidad: {observacion.Nombre}";
                suceso.EstaCancelada = false;
                suceso.EstaTerminada = false;
                suceso.Usuario = contexto.SeleccionarPorId<UsuarioDtm>(observacion.IdCreador).Login;
                suceso.OcurridoEl = observacion.CreadaEl;
                suceso.Detalle = observacion.Descripcion;
                suceso.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                suceso.Accion = "Mostrar";
                suceso.AccionJs = ltrAccionesDeSucesos.MostrarObservacion;

                suceso.Negocio = negocio.Singular();
                suceso.enumNegocio = negocio;
                suceso.IdNegocio = negocio.IdNegocio();
                suceso.Nivel = nivel;
                suceso.IdElemento = registro.Id;

                sucesos.Add(suceso);
            }
        }

        public static void HistorialDeArchivos(this IElementoDtm registro, ContextoSe contexto, List<HistorialDto> sucesos, List<ClausulaDeFiltrado> filtros, int nivel = 1)
        {
            ClausulaDeFiltrado filtro = filtros.First(f => f.Clausula == ltrSucesosFiltros.excluir);
            var valores = filtro.Valor.Split(Simbolos.separadorDeValores);
            if (valores.Contains(ltrSucesosExcluir.archivos) || (nivel > 1 && valores.Contains(ltrSucesosExcluir.nivel2archivos)))
                return;
            var negocio = NegociosDeSe.NegocioDeUnDtm(registro.GetType());
            var archivos = registro.Archivos(contexto, incluirOriginal: true);
            for (var i = archivos.Count() - 1; i >= 0; i--)
            {
                var archivo = archivos[i];
                var suceso = new HistorialDto();
                suceso.Id = sucesos.Count + 1;
                suceso.IdRegistro = archivo.Id;
                suceso.Elemento = registro.Referencia(contexto);
                suceso.Clase = $"{ltrSucesosCss.suceso}{Simbolos.separadorCss}{ltrSucesosCss.archivo}";
                suceso.Suceso = $"Archivo '{archivo.Nombre}' anexado";
                suceso.EstaCancelada = false;
                suceso.EstaTerminada = false;
                suceso.Usuario = contexto.SeleccionarPorId<UsuarioDtm>(archivo.IdUsuaCrea).Login;
                suceso.OcurridoEl = archivo.FechaCreacion;
                suceso.Detalle = archivo.Auditoria(contexto);
                suceso.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
                suceso.Accion = "Descargar";
                suceso.AccionJs = ltrAccionesDeSucesos.Descargar;

                suceso.Negocio = negocio.Singular();
                suceso.enumNegocio = negocio;
                suceso.IdNegocio = negocio.IdNegocio();
                suceso.Nivel = nivel;
                suceso.IdElemento = registro.Id;

                sucesos.Add(suceso);
            }
        }

        public static IElementoDtm NuevoDtm(Type T, string nombre, string descripcion, Dictionary<string, object> parametros)
        {
            var obj = (IElementoDtm)Activator.CreateInstance(T);
            obj.Nombre = nombre;
            if (T.ImplementaUsaDescripcion()) ((IUsaDescripcion)obj).Descripcion = descripcion;
            if (T.ImplementaUsaClaseDeElemento())
            {
                var idClaseDeElemento = (int?)parametros.LeerValor<long?>(nameof(IUsaCalseDeElemento.IdClaseDeElemento), null);
                ((IUsaCalseDeElemento)obj).IdClaseDeElemento = idClaseDeElemento == 0 || idClaseDeElemento == null ? null : idClaseDeElemento;
            }
            return obj;
        }

        public static UsuarioDtm Responsable(this IElementoDtm elemento, ContextoSe contexto)
        {
            if (elemento.GetType() == typeof(InfanteDtm))
            {
                return ((InfanteDtm)elemento).UsuarioDeSuProfesor(contexto);
            }

            if (!elemento.GetType().ImplementaPuedeUsarResponsable())
                return null;

            if (((IPuedeUsarResponsable)elemento).Responsable is not null)
                return ((IPuedeUsarResponsable)elemento).Responsable;

            if (((IPuedeUsarResponsable)elemento).IdResponsable is null)
                return null;

            ((IPuedeUsarResponsable)elemento).Responsable = contexto.SeleccionarPorId<UsuarioDtm>((int)((IPuedeUsarResponsable)elemento).IdResponsable);
            return ((IPuedeUsarResponsable)elemento).Responsable;
        }

        public static UsuarioDtm Creador(this IElementoDtm elemento, ContextoSe contexto)
        {
            if (elemento.UsuarioCreador is not null) return elemento.UsuarioCreador;
            elemento.UsuarioCreador = contexto.SeleccionarPorId<UsuarioDtm>(elemento.IdUsuaCrea);
            return elemento.UsuarioCreador;
        }
        public static UsuarioDtm Modificador(this IElementoDtm elemento, ContextoSe contexto)
        {
            if (elemento.IdUsuaModi is null) return null;
            if (elemento.UsuarioModificador is not null) return elemento.UsuarioModificador;
            elemento.UsuarioModificador = contexto.SeleccionarPorId<UsuarioDtm>((int)elemento.IdUsuaModi);
            return elemento.UsuarioModificador;
        }
        public static DireccionDtm CrearDireccion(this IElementoDtm elemento, ContextoSe Contexto, CrearDireccionDto crearDireccionDto)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());

            if (!negocio.UsaDirecciones())
                GestorDeErrores.Emitir($"No puede añadir una dirección a '{elemento.Referencia(Contexto)}', ya que el negocio '{negocio.Singular()}' no usa direcciones");

            var calle = Contexto.SeleccionarDto<CalleDto>(crearDireccionDto.IdCalle.Entero());

            if (crearDireccionDto.IdCp.Entero() == 0 && calle.IdCp.Entero() == 0)
                GestorDeErrores.Emitir("Debe indicar el código postal");

            if (crearDireccionDto.Calificador.IsNullOrEmpty())
                GestorDeErrores.Emitir("Debe indicar el calificador de la dirección");

            var idCp = crearDireccionDto.IdCp.Entero() > 0 ? crearDireccionDto.IdCp : calle.IdCp.Entero();
            return new DireccionDtm
            {
                Negocio = negocio,
                IdElemento = elemento.Id,
                IdPais = calle.IdPais,
                IdProvincia = calle.IdProvincia,
                IdMunicipio = calle.IdMunicipio,
                IdCalle = calle.Id,
                Calificador = ApiDeEnsamblados.ToEnumerado<enumCalificadorDireccion>(crearDireccionDto.Calificador),
                IdCp = idCp,
                Numero = crearDireccionDto.Numero,
                Escalera = crearDireccionDto.Escalera,
                Puerta = crearDireccionDto.Puerta,
                Piso = crearDireccionDto.Piso,
                Otros = crearDireccionDto.Otros
            }.Insertar(Contexto);
        }

        public static string CrearHref(this IElementoDtm elemento, ContextoSe contexto)
        {
            var refHtml = $@"<a href='{CrearLink(elemento, contexto)}' target='_blank' idelemento='{elemento.Id}'>{elemento.Referencia(contexto)}</a>";
            return refHtml;
        }

        public static string CrearLink(this IElementoDtm elemento, ContextoSe contexto)
        {
            var negocioDtm = NegociosDeSe.LeerNegocioPorDtm(elemento.GetType().FullName);
            var vistas = contexto.SeleccionarTodos<VistaMvcDtm>(nameof(VistaMvcDtm.ElementoDto), negocioDtm.ElementoDto);
            if (vistas.Count == 0)
                GestorDeErrores.Emitir($"No se ha definido la vista para mostrar un elemento del negocio '{negocioDtm.Enumerado}'");

            VistaMvcDtm vista = null;
            if (vistas.Count == 1)
            {
                vista = vistas[0];
            }
            else
            {
                if (!elemento.GetType().ImplementaElementoConTipo())
                {
                    GestorDeErrores.Emitir($"Hay más de una vista para mostrar un elemento del negocio '{negocioDtm.Enumerado}', defina una sóla");
                }
                var vistasPorTipo = negocioDtm.VistasPorTipoDeNegocio(((IElementoConTipo)elemento).IdTipo);
                vista = contexto.SeleccionarPorId<VistaMvcDtm>(vistasPorTipo.IdVista);
            }

            var uri = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
            {
                Path = $"/{vista.Controlador}/{vista.Accion}",
                Query = $"id={elemento.Id}"
            };

            return uri.Uri.ToString();
        }

        public static UsuarioDtm Bloqueador(this IUsaBloqueo elemento, ContextoSe contexto)
        {
            var negocio = elemento.ValidarUsaBloqueos();

            if (!elemento.Bloqueado)
                return null;

            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_Bloqueadores);
            var i = $"{negocio}-{((ElementoDtm)elemento).Id}";
            if (!cache.ContainsKey(i))
            {
                var trazas = negocio.Trazas(contexto).Where(x => x.IdElemento == ((ElementoDtm)elemento).Id && x.Nombre == ltrDeTrazas.Bloqueada).ToList();

                if (trazas.Count == 0)
                    return null;

                var traza = trazas.OrderByDescending(x => x.CreadaEl).First();
                cache[i] = contexto.SeleccionarPorId<UsuarioDtm>(traza.IdCreador);
            }
            return (UsuarioDtm)cache[i];
        }

        public static UsuarioDtm Desbloqueador(this IUsaBloqueo elemento, ContextoSe contexto)
        {
            var negocio = elemento.ValidarUsaBloqueos();

            if (elemento.Bloqueado)
                return null;

            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_Desbloqueadores);
            var i = $"{negocio}-{((ElementoDtm)elemento).Id}";
            if (!cache.ContainsKey(i))
            {
                var traza = negocio.Trazas(contexto).Where(x => x.IdElemento == ((ElementoDtm)elemento).Id && x.Nombre == ltrDeTrazas.Desbloqueada).OrderByDescending(x => x.CreadaEl).FirstOrDefault();
                cache[i] = traza is null ? null : contexto.SeleccionarPorId<UsuarioDtm>(traza.IdCreador);
            }
            return (UsuarioDtm)cache[i];
        }

        public static void Bloquear<T>(this IUsaBloqueo elemento, ContextoSe contexto)
        where T : RegistroDtm
        {
            var negocio = elemento.ValidarUsaBloqueos();

            if (!((ElementoDtm)elemento).EsGestor(contexto))
                GestorDeErrores.Emitir($"Necesita permisos de gestión para bloquear el elemento '{((IElementoDtm)elemento).Referencia(contexto)}'");

            if (elemento.Bloqueado)
                GestorDeErrores.Emitir(ltrDeTrazas.Error_De_Bloqueo.Replace("Referencia", ((IElementoDtm)elemento).Referencia(contexto)).Replace("Bloqueador", elemento.Bloqueador(contexto).Login));

            var tran = contexto.IniciarTransaccion();
            try
            {
                elemento.Bloqueado = true;
                ((T)elemento).Modificar(contexto, accionEjecutada: ltrDeUnElemento.Accion_Bloquear);
                ((IUsaTraza)elemento).CrearTraza(contexto, ltrDeTrazas.Bloqueada, $"El usuario '{contexto.DatosDeConexion.Login}' ha bloqueado el elemento '{((ElementoDtm)elemento).Referencia(contexto)}'");
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                elemento.Bloqueado = false;
                throw;
            }
            finally
            {
                var i = $"{negocio}-{((ElementoDtm)elemento).Id}";
                ServicioDeCaches.EliminarElemento(CacheDe.Ent_Bloqueadores, i);
            }
        }

        public static void Desbloquear<T>(this IUsaBloqueo elemento, ContextoSe contexto)
        where T : RegistroDtm
        {
            var negocio = elemento.ValidarUsaBloqueos();

            if (!((ElementoDtm)elemento).EsInterventor(contexto) && elemento.Bloqueador(contexto).Id != contexto.DatosDeConexion.IdUsuario)
                GestorDeErrores.Emitir($"Necesita permisos de intervención para desbloquear el elemento '{((IElementoDtm)elemento).Referencia(contexto)}'");

            if (!elemento.Bloqueado)
            {
                var desbloqueador = elemento.Desbloqueador(contexto);
                if (desbloqueador is null)
                    GestorDeErrores.Emitir(ltrDeTrazas.Error_De_NoHayBloqueo.Replace("Referencia", ((IElementoDtm)elemento).Referencia(contexto)));
                else
                    GestorDeErrores.Emitir(ltrDeTrazas.Error_De_Desbloqueo.Replace("Referencia", ((IElementoDtm)elemento).Referencia(contexto).Replace("desbloqueador", desbloqueador.Login)));
            }

            var tran = contexto.IniciarTransaccion();
            try
            {
                elemento.Bloqueado = false;
                ((T)elemento).Modificar(contexto, accionEjecutada: ltrDeUnElemento.Accion_Desbloquear);
                ((IUsaTraza)elemento).CrearTraza(contexto, ltrDeTrazas.Desbloqueada, $"El usuario '{contexto.DatosDeConexion.Login}' ha desbloqueado el elemento '{((IElementoDtm)elemento).Referencia(contexto)}'");
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                elemento.Bloqueado = true;
                throw;
            }
            finally
            {
                var i = $"{negocio}-{((ElementoDtm)elemento).Id}";
                ServicioDeCaches.EliminarElemento(CacheDe.Ent_Desbloqueadores, i);
            }
        }

        private static enumNegocio ValidarUsaBloqueos(this IUsaBloqueo elemento)
        {
            if (!elemento.GetType().ImplementaUsaBloqueo())
                GestorDeErrores.Emitir($"La clase '{elemento.GetType().Name}' no implementa bloqueos");
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!negocio.UsaTrazas())
                GestorDeErrores.Emitir($"El negocio '{negocio.Singular(true)}' no implementa traza de bloqueo");

            return negocio;
        }

        public static string DefinirLink(this IElementoDtm elementoDtm, ContextoSe contexto)
        {
            var negocio = elementoDtm.GetType().NegocioDeUnDtm();
            var elementoDto = elementoDtm.MapearDto(contexto, negocio);
            return ApiParaDtos.ComponerUrl(new TipoDtoElmento
            {
                TipoDto = negocio.TipoDto().FullName,
                IdElemento = elementoDtm.Id,
                Referencia = elementoDtm.ImplementaUsaReferencia() ? ((IUsaReferencia)elementoDtm).Referencia : "Abrir"
            });
        }

        public static string DefinirLink(this IElementoDtm elementoDtm, ContextoSe contexto, string nombre)
        {
            var negocio = elementoDtm.GetType().NegocioDeUnDtm();
            var elementoDto = elementoDtm.MapearDto(contexto, negocio);
            return ApiParaDtos.ComponerUrl(new TipoDtoElmento
            {
                TipoDto = negocio.TipoDto().FullName,
                IdElemento = elementoDtm.Id,
                Referencia = nombre
            });
        }

        public static string RegistrarConsultaConGuid(this IElementoDtm elementoDtm, ContextoSe contexto, DateTime? caducaEl, int? maximoDeDescargas, bool auditar = true)
        {
            var guid = Guid.NewGuid();
            var creadoEl = DateTime.Now;
            if (caducaEl.HasValue && (DateTime)caducaEl <= creadoEl)
            {
                GestorDeErrores.Emitir($"No puede registrar una consulta con fecha de caducida '{caducaEl.Fecha().ToString("yyy-MM-dd HH:mm")}', es anterior al momento actual");
            }

            if (caducaEl is null && maximoDeDescargas is null)
            {
                caducaEl = creadoEl.AddDays(1);
            }

            var negocio = NegociosDeSe.ToEnumerado(elementoDtm.GetType());

            if (!negocio.UsaTrazas())
                GestorDeErrores.Emitir("No se puede dar acceso de consulta a un elemento que no use trazas");


            var registrar = new ConsultaConGuidDtm
            {
                IdNegocio = negocio.IdNegocio(),
                IdElemento = elementoDtm.Id,
                Guid = guid,
                IdUsuario = contexto.Usuario.Id,
                CreadoEl = creadoEl,
                CaducaEl = caducaEl,
                MaximoDescargas = maximoDeDescargas
            }.InsertarComoAdministrador(contexto);

            if (auditar)
            {
                ((IUsaTraza)elementoDtm).CrearTraza(contexto, "Consulta con Guid", $"El usuario '{contexto.Usuario.Login}' a creado el guid '{guid.ToString()}' para la consulta del elemento");
            }

            return guid.ToString();
        }

        public static void CebarCacheDeIds<T>(ContextoSe contexto, List<int> listaDeIds)
        where T : ElementoDtm
        {
            var cacheRegistros = ServicioDeCaches.ObtenerCache(typeof(T).FullName, nameof(IRegistro.Id));
            var idsNuevos = listaDeIds
                            .Where(id => !cacheRegistros.ContainsKey(IndiceCacheDeRegistro(id)))
                            .Distinct()
                            .ToList();

            if (!idsNuevos.Any()) return;

            var registros = contexto.Set<T>()
                      .Where(p => idsNuevos.Contains(p.Id))
                      .ToList();

            foreach (var registro in registros)
            {
                cacheRegistros[IndiceCacheDeRegistro(registro.Id)] = registro;
            }
        }

        internal static string IndiceCacheDeRegistro(int id) => $"Id-{id}-{ServicioDeCaches.enumConJoin.no}";

    }
}
