using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Entorno;
using ModeloDeDto.Entorno;
using GestorDeElementos.Extensores;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Negocio;

namespace GestorDeElementos
{
    public static class ApiDePermisos
    {
        public static bool HayPermisos(this PermisoDtm permiso, ContextoSe contexto) => TieneElPermiso(contexto, permiso.Id);

        public static bool TieneElPermiso(this ContextoSe contexto, int idPermiso) => TieneAlgunPermiso(contexto, new List<int> { idPermiso });

        public static bool TieneAlgunPermiso(this ContextoSe contexto, List<int> idsDePermiso)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_TieneAlgunPermiso);
            var indice = contexto.DatosDeConexion.IdUsuario.ToString();
            if (!cache.ContainsKey(indice))
            {
                cache[indice] = contexto.Set<UsuariosDeUnPermisoDtm>().Where(permisos => permisos.IdUsuario == contexto.DatosDeConexion.IdUsuario).Select(x => x.IdPermiso).ToList();
            }

            return ((List<int>)cache[indice]).Any(idsDePermiso.Contains);
        }

        public static bool HayPermisosDeRelacion(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, IRelacion registro, enumModoDeAccesoDeDatos permisosNecesarios)
        {
            if (NegociosDeSe.EsDeParametrizacion(negocio1) || NegociosDeSe.EsDeParametrizacion(negocio2))
            {
                if (!contexto.DatosDeConexion.EsAdministrador)
                    GestorDeErrores.Emitir("Se necesitan permisos de administración para modificar relaciones con negocios de parametrización");
                return true;
            }

            if (negocio1.Equals(enumNegocio.No_Definido) && negocio2.Equals(enumNegocio.No_Definido))
                return true;

            if (negocio1.Equals(enumNegocio.No_Definido))
            {
                var modoQueTengo = LeerModoDeAcceso(contexto, negocio2, registro.IdElemento2);
                return ModoDeAcceso.SoyGestor(modoQueTengo);
            }

            if (negocio2.Equals(enumNegocio.No_Definido))
            {
                var modoQueTengo = LeerModoDeAcceso(contexto, negocio1, registro.IdElemento1);
                return ModoDeAcceso.SoyGestor(modoQueTengo);
            }

            var modoAlElemento1 = LeerModoDeAcceso(contexto, negocio1, registro.IdElemento1);
            var modoAlElemento2 = LeerModoDeAcceso(contexto, negocio2, registro.IdElemento2);
            return ModoDeAcceso.PuedoRelacionar(permisosNecesarios, modoAlElemento1, modoAlElemento2);
        }

        public static void ValidarQueElPermisoNoEstaOtorgado(ContextoSe contexto, IPermisoOtorgado registro, PermisoDtm permiso)
        {
            var idPuesto = PuestoDondeEsta(contexto, registro);
            if (idPuesto > 0)
            {
                var puesto = (PuestoDtm)NegociosDeSe.CrearGestor(contexto, enumNegocio.Puesto).LeerRegistroPorId(idPuesto, true);
                GestorDeErrores.Emitir($"El permiso {permiso.Nombre} que intenta crear ya lo hereda el usuario del puesto {puesto.Expresion}");
            }
        }

        public static bool EsConsultorDeLaAgenda(ContextoSe contexto, AgendaDtm agenda)
        {
            var permisoDeUsuario = PermisosSobreLaAgenda(contexto, agenda);
            if (permisoDeUsuario.Count == 0)
                return false;
            return true;
        }

        public static bool EsGestorDeLaAgenda(ContextoSe contexto, AgendaDtm agenda)
        {
            var permisoDeUsuario = PermisosSobreLaAgenda(contexto, agenda);
            if (permisoDeUsuario.Count == 0)
                return false;
            foreach (var p in permisoDeUsuario)
            {
                if (p.IdPermiso == agenda.IdGestor || p.IdPermiso == agenda.IdInterventor)
                    return true;
            }

            return false;
        }

        public static int PuestoDondeEsta(ContextoSe contexto, IPermisoOtorgado registro)
        {
            var rolesDeUnPermiso = contexto.Set<PermisosDeUnRolDtm>().AsNoTracking().Where(x => x.IdPermiso.Equals(registro.IdPermiso)).Include(x => x.Rol).ThenInclude(x => x.Puestos);
            var puestosDeUnUsuario = contexto.Set<PuestosDeUnUsuarioDtm>().AsNoTracking().Where(x => x.IdUsuario.Equals(registro.IdUsuario));

            if (rolesDeUnPermiso.Count() > 0 && puestosDeUnUsuario.Count() > 0)
            {
                var puestosDondeEstaElPermiso = rolesDeUnPermiso.Select(x => x.Rol.Puestos.Select(x => x.IdPuesto));
                if (puestosDondeEstaElPermiso.Count() == 0)
                    return 0;

                var idPuestosDondeEstaElUsuario = puestosDeUnUsuario.Select(x => x.IdPuesto);
                foreach (var idPuestoDeUsuario in idPuestosDondeEstaElUsuario.Where(idPuestoDeUsuario => puestosDondeEstaElPermiso.Any(x => x.Contains(idPuestoDeUsuario))))
                {
                    return idPuestoDeUsuario;
                }
            }
            return 0;
        }

        public static void ValidarPermisosDePersistencia(ContextoSe contexto, enumNegocio negocio, ParametrosDeNegocio parametros, RegistroDtm registro)
        {
            if (!negocio.EsNegocioDeBD())
                throw new Exception($"El negocio {negocio} no puede usar el método {nameof(ValidarPermisosDePersistencia)} sin usar el parámetro del tipo de objeto Dtm");
            ValidarPermisosDePersistencia(contexto, negocio, ApiDeRegistroDtm.ObtenerTypoDtm(NegociosDeSe.LeerNegocioPorEnumerado(negocio).ElementoDtm), parametros, registro);
        }

        public static void ValidarPermisosDePersistencia(ContextoSe contexto, enumNegocio negocio, Type tipoDeNegocioDtm, ParametrosDeNegocio parametros, RegistroDtm registro)
        {
            if (!parametros.ValidarPermisosDePersistencia)
                return;

            if (parametros.EsUnaPeticion && registro.ImplementaNecesitaSerParametrizador() && !contexto.SePuedeParametrizar())
                GestorDeErrores.Emitir($"Los objetos de parametrización {registro.GetType().Name}{(registro.ImplementaNombre()?$": {((INombre)registro).Nombre}":"")} solo los puede modificar un administrador marcado como parametrizador");

            if (parametros.EstaEjecutandoUnaAccion || parametros.Transitando)
                return;

            if (contexto.DatosDeConexion.CreandoModelo)
                return;

            if (registro is EventoDeAgendaDtm)
            {

                if (!parametros.EsUnaPeticion && ((EventoDeAgendaDtm)registro).EsDelSistema)
                    return;

                if (contexto.TrabajoSometido)
                    return;

                if (((EventoDeAgendaDtm)registro).IdAgenda == 0)
                    GestorDeErrores.Emitir($"No ha indicado el la agenda donde almacenar el evento {((EventoDeAgendaDtm)registro).Nombre}");

                var filtros = new List<ClausulaDeFiltrado>();
                var agenda = contexto.SeleccionarPorId<AgendaDtm>(((EventoDeAgendaDtm)registro).IdAgenda, parametros: parametros.Parametros);
                filtros.Add(new ClausulaDeFiltrado(nameof(UsuariosDeUnPermisoDtm.IdUsuario), enumCriteriosDeFiltrado.igual, contexto.DatosDeConexion.IdUsuario));
                filtros.Add(new ClausulaDeFiltrado(nameof(UsuariosDeUnPermisoDtm.IdPermiso), enumCriteriosDeFiltrado.esAlgunoDe, $"{agenda.IdGestor},{agenda.IdInterventor}"));

                if (contexto.Contar<PermisosDeUnUsuarioDto, UsuariosDeUnPermisoDtm>(filtros) == 0)
                    GestorDeErrores.Emitir("No tiene permisos de gestión sobre la agenda seleccionada");
                return;
            }

            if (ApiDeInterfaceDtm.ImplementaUnaTraza(registro.GetType()))
                return;

            if (ApiDeInterfaceDtm.ImplementaUnaAccionDeRelacion(registro.GetType()))
                return;

            if (ApiDeInterfaceDtm.ImplementaPermisosDelElemento(registro.GetType()))
                return;

            if (ApiDeInterfaceDtm.ImplementaUnHito(registro.GetType()))
                return;

            if (ApiDeInterfaceDtm.ImplementaUnaDireccion(registro.GetType()))
            {
                var modo = LeerModoDeAcceso(contexto, negocio, ((IDireccionDtm)registro).IdElemento);
                if (modo == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir("No tiene permisos para añadir direcciones al elemento.");
                return;
            }

            if (ApiDeInterfaceDtm.ImplementaUnaObservacion(registro.GetType()))
            {
                var modo = LeerModoDeAcceso(contexto, negocio, ((IObservacion)registro).IdElemento);
                if (modo == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir("No tiene permisos para añadir observaciones al elemento.");
                return;
            }

            if (ApiDeInterfaceDtm.ImplementaUnaRelacion(registro))
            {
                var n1 = ApiDeRelaciones.ObtenerTipoDtm(tipoDeNegocioDtm, enumDtmsDeRelacion.Negocio1).NegocioDeUnDtm();
                var n2 = ApiDeRelaciones.ObtenerTipoDtm(tipoDeNegocioDtm, enumDtmsDeRelacion.Negocio2).NegocioDeUnDtm();
                ValidarPermisosDeRelacion(contexto, n1, n2, (IRelacion)registro);
                return;
            }

            if (ApiDeInterfaceDtm.ImplementaAmpliacion(registro.GetType()))
            {
                var modo = LeerModoDeAcceso(contexto, ((IAmpliacion)registro).Negocio, ((IAmpliacion)registro).IdElemento);

                var elemento = ((IAmpliacion)registro).AmpliacionDe(contexto);

                if (modo == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir($"No tiene permisos para añadir ampliaciones al elemento '{elemento.Referencia(contexto)}' del negocio de {((IAmpliacion)registro).Negocio.Singular(true)}.");

                if (modo == enumModoDeAccesoDeDatos.Consultor)
                    GestorDeErrores.Emitir($"Solo tiene permisos de consulta al elemento '{elemento.Referencia(contexto)}' del negocio de {((IAmpliacion)registro).Negocio.Singular(true)}.");

                return;
            }

            if (ApiDeInterfaceDtm.ImplementaDetalle(registro.GetType()))
            {
                var modo = LeerModoDeAcceso(contexto, ((IDetalle)registro).Negocio, ((IDetalle)registro).IdElemento);

                var elemento = ((IDetalle)registro).DetalleDe(contexto);
                var referencia = elemento.GetType().ImplementaElementoDeUnProceso() ? ((IElementoDeProcesoDtm)elemento).Referencia(contexto) : ((IElementoDtm)elemento).Nombre;

                if (modo == enumModoDeAccesoDeDatos.SinPermiso)
                    GestorDeErrores.Emitir($"No tiene permisos para añadir detalles al elemento '{referencia}' del negocio de {((IDetalle)registro).Negocio.Singular(true)}.");

                if (modo == enumModoDeAccesoDeDatos.Consultor)
                    GestorDeErrores.Emitir($"Solo tiene permisos de consulta al elemento '{referencia}' del negocio de {((IDetalle)registro).Negocio.Singular(true)}.");

                return;
            }

            if (NegociosDeSe.EsUnRegistro(negocio) || !NegociosDeSe.UsaSeguridad(negocio))
                return;

            if (negocio.UsaHitos() && parametros.Parametros.LeerValor(ltrParametrosNeg.ValidarEtapaDocumental, false))
            {
                if (negocio.TipoDtm().Terminados(contexto).Contains(((IUsaEstado)registro).IdEstado))
                {
                    var etapa = ParametroDeNegocioSql.Parametro(negocio, enumParametrosDeArchivadores.NEGOCIO_Etapa_Documental, emitirError: false, crearParametro: true, 0).Valor.ToLista<int>();
                    if (etapa.Contains(((IUsaEstado)registro).IdEstado) && ((IElementoDeProcesoDtm)registro).EsGestor(contexto))
                        return;
                    GestorDeErrores.Emitir($"Para poder anexar documentación al registro '{((IUsaReferencia)registro).Referencia}', que está '{((IElementoDeProcesoDtm)registro).Estado(contexto).Nombre}', ha de estar en la etapa documental y tener permisos de gestión");
                }
            }

            if (ApiDeInterfaceDtm.ImplementaUnTipoDeElemento(tipoDeNegocioDtm) ||
                ApiDeInterfaceDtm.ImplementaUnEstado(registro.GetType()) ||
                ApiDeInterfaceDtm.ImplementaUnaTransicion(registro.GetType()) ||
                NegociosDeSe.EsDeParametrizacion(negocio) ||
                registro.GetType().ImplementaPlantillasPorTipo() ||
                registro.GetType().ImplementaClasesDelTipo())
            {
                if (contexto.DatosDeConexion.EsAdministrador) return;
                GestorDeErrores.Emitir($"El usuario '{contexto.DatosDeConexion.Login}' no tiene permisos de parametrización sobre el negocio '{negocio.ToNombre()}'");
            }
            var esHistorico = (historico: false, cancelado: false, terminado: false);

            if (NegociosDeSe.UsaBaja(negocio) && registro is not null && ((IUsaBaja)registro).Baja)
            {
                if (!parametros.EstaDandoDeBaja((IUsaBaja)registro))
                    GestorDeErrores.Emitir($"el '{((IElementoDtm)registro).Referencia(contexto)}' del negocio '{negocio.Singular()}' está de baja, no es modificable");
            }


            if (NegociosDeSe.UsaEstado(negocio))
            {
                if (parametros.ProcesandoTransicion((IUsaEstado)registro))
                    return;
                esHistorico = EsHistorico(contexto, negocio, registro);
            }

            if (esHistorico.historico)
            {
                var inicio = negocio.EsUnNegocio() && registro.GetType().ImplementaUsaFlujo() ? $"{negocio.Singular()}:" : "El registro";
                GestorDeErrores.Emitir($"{inicio} '{((IUsaReferencia)registro).Referencia}' está {(esHistorico.cancelado ? "cancelado" : "terminado")}, no es modificable");
            }

            if (contexto.DatosDeConexion.EsAdministrador) return;

            var modoAcceso = LeerModoDeAccesoAlNegocio(contexto, negocio);
            var hayPermisos = ModoDeAcceso.SoyAdministrador(modoAcceso);
            if (!hayPermisos)
            {
                var permisoPorEstado = NegociosDeSe.UsaFlujo(negocio)
                    ? HayPermisosPorEstado(contexto, negocio, registro)
                    : true;
                if (!permisoPorEstado)
                {
                    var elemento = (IElementoDeProcesoDtm)registro;
                    var idEstado = elemento.IdEstado == 0 && elemento.IdTipo > 0 ? elemento.IdDelEstadoInicial(contexto) : elemento.IdEstado;
                    var estado = elemento.Estado == null
                    ? EstadoSql.LeerEstadoPorId(contexto, negocio.TablaDeEstados(), idEstado).Nombre
                    : ((IElementoDeProcesoDtm)registro).Estado.Nombre;
                    GestorDeErrores.Emitir($"El usuario '{contexto.DatosDeConexion.Login}' no tiene permisos sobre '{negocio.ToNombre()}: {((IElementoDeProcesoDtm)registro).Referencia}' en el estado '{estado}'");
                }

                var permisoPorElemento = false;
                if (!parametros.Insertando && NegociosDeSe.UsaPermisosPorElemento(tipoDeNegocioDtm))
                    permisoPorElemento = HayPermisosPorElemento(contexto, negocio, registro, modoSolicitado: enumModoDeAccesoDeDatos.Gestor);

                if (permisoPorElemento)
                    return;

                if (!NegociosDeSe.UsaTipo(negocio) && !NegociosDeSe.UsaPermisosPorCg(negocio) && !ModoDeAcceso.SoyGestor(modoAcceso))
                    GestorDeErrores.Emitir($"El usuario {contexto.DatosDeConexion.Login} no tiene permisos de gestor de '{negocio.ToNombre()}'");

                var permisoPorCg = !NegociosDeSe.UsaPermisosPorCg(negocio)
                    || (NegociosDeSe.UsaPermisosPorCg(negocio) &&
                        HayPermisosPorCg(contexto, negocio, ((IUsaCg)registro).IdCg, modoSolicitado: enumModoDeAccesoDeDatos.Gestor));

                if (!permisoPorCg)
                    GestorDeErrores.Emitir($"El usuario {contexto.DatosDeConexion.Login} no tiene permisos para " +
                        $"{parametros.Operacion} un objeto '{registro.GetType().Name}' " +
                        $"{(NegociosDeSe.UsaTipo(negocio) ? $"del tipo '{TipoDeElementoSql.LeerTipoPorId(contexto, negocio.ObtenerMetadatos().TipoDtm, ((IUsaTipo)registro).IdTipo).Expresion}'" : "")} en " +
                        $"el C.G. '{CentroGestorSql.LeerCgPorId(contexto, ((IUsaCg)registro).IdCg).Expresion}'");

                var permisoPorTipo = !NegociosDeSe.UsaTipo(negocio) || (NegociosDeSe.UsaTipo(negocio) && HayPermisosPorTipo(contexto, registro, negocio.ObtenerMetadatos().TipoDtm, enumModoDeAccesoDeDatos.Gestor));
                if (!permisoPorTipo)
                    GestorDeErrores.Emitir($"El usuario {contexto.DatosDeConexion.Login} no tiene permisos para al tipo '{TipoDeElementoSql.LeerTipoPorId(contexto, negocio.ObtenerMetadatos().TipoDtm, ((IUsaTipo)registro).IdTipo).Expresion}'");

                //TODO: Excepciones por la relación tipo y CG hay que mirar si para un tipo y CG existen excepciones, si existen entonces ver si se les ha quitado el permiso de persistencia
            }
        }

        public static bool HayPermisosDeTransicion(ContextoSe contexto, enumNegocio negocio, TransicionDtm transicion, bool excluirLasDelSistema = false)
        {
            if (contexto.DatosDeConexion.EsAdministrador || contexto.EsElContextoDeUnaAccion || contexto.EsElContextosDeUnEntorno)
                return true;

            if (!transicion.Activo)
                return false;

            if (transicion.DelSistema && excluirLasDelSistema)
                return false;

            if (transicion.DelSistema)
                return true;

            return PermisosPorTransicionSql.PermisosDeUsuario(contexto, negocio.IdNegocio(), transicion.Id).ToList().Count() > 0;
        }

        public static enumModoDeAccesoDeDatos LeerModoDeAccesoAlNegocio(ContextoSe contexto, enumNegocio negocio)
        {
            if (negocio == enumNegocio.Permiso)
                return enumModoDeAccesoDeDatos.Consultor;

            if (contexto.DatosDeConexion.EsAdministrador)
                return enumModoDeAccesoDeDatos.Administrador;

            if (NegociosDeSe.EsDeParametrizacion(negocio) && !contexto.DatosDeConexion.EsAdministrador)
                return enumModoDeAccesoDeDatos.Consultor;

            if (NegociosDeSe.EsUnRegistro(negocio) || !NegociosDeSe.UsaSeguridad(negocio))
                return enumModoDeAccesoDeDatos.Administrador;

            enumModoDeAccesoDeDatos modoDelUsuario = enumModoDeAccesoDeDatos.SinPermiso;

            var cache = ServicioDeCaches.Obtener(CacheDe.ModoAcceso_AlNegocio);
            var indice = $"Usuario:{contexto.DatosDeConexion.IdUsuario} Negocio:{negocio.ToNombre()}";

            if (!cache.ContainsKey(indice))
            {
                if (NegociosDeSe.UsaTipo(negocio))
                {
                    modoDelUsuario = EsAdministradorDelNegocio(contexto, negocio)
                    ? enumModoDeAccesoDeDatos.Administrador
                    : ModoDeAccesoAlTipo(contexto, negocio, null);
                }
                else
                {
                    modoDelUsuario = EsAdministradorDelNegocio(contexto, negocio)
                    ? enumModoDeAccesoDeDatos.Administrador
                    : EsGestorDelNegocio(contexto, negocio)
                    ? enumModoDeAccesoDeDatos.Gestor
                    : EsConsultorDelNegocio(contexto, negocio)
                    ? enumModoDeAccesoDeDatos.Consultor
                    : enumModoDeAccesoDeDatos.SinPermiso;
                }

                if (modoDelUsuario == enumModoDeAccesoDeDatos.SinPermiso && PermisosPorElementoSql.AlgunElementoConAcceso(contexto, negocio.IdNegocio()))
                    modoDelUsuario = enumModoDeAccesoDeDatos.Consultor;

                cache[indice] = modoDelUsuario;
            }

            if (ModoDeAcceso.SoyConsultor((enumModoDeAccesoDeDatos)cache[indice]) && !negocio.Activo())
                return enumModoDeAccesoDeDatos.Consultor;

            return (enumModoDeAccesoDeDatos)cache[indice];
        }

        public static bool HayPermisosDeEstado(ContextoSe contexto, enumNegocio negocio, EstadoDtm estado)
        =>
        estado.Cancelado || estado.Terminado ? false : PermisosPorEstadoSql.PermisosDeUsuario(contexto, negocio.IdNegocio(), estado.Id).ToList().Count() > 0;

        public static enumModoDeAccesoDeDatos LeerModoDeAccesoDeRelacion(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, IRelacion relacion)
        {
            var m1 = negocio1 == enumNegocio.No_Definido ? enumModoDeAccesoDeDatos.Administrador : LeerModoDeAcceso(contexto, negocio1, relacion.IdElemento1);
            var m2 = negocio2 == enumNegocio.No_Definido ? enumModoDeAccesoDeDatos.Administrador : LeerModoDeAcceso(contexto, negocio2, relacion.IdElemento2);

            return ModoDeAcceso.ModoAccesoDeRelacion(m1, m2);
        }

        public static enumModoDeAccesoDeDatos LeerModoDeAccesoAlVinculo(ContextoSe contexto, enumNegocio negocio, enumNegocio vinculado, int idElemento1, int idElemento2)
        {

            var m1 = LeerModoDeAcceso(contexto, negocio, idElemento1);
            var m2 = LeerModoDeAcceso(contexto, vinculado, idElemento2);

            return ModoDeAcceso.ModoAccesoDeRelacion(m1, m2);
        }

        public static enumModoDeAccesoDeDatos LeerModoDeAcceso(ContextoSe contexto, enumNegocio negocio, int idElemento, Dictionary<string, object> parametros = null)
        {

            var gestor = NegociosDeSe.CrearGestor(contexto, negocio);
            if (parametros == null) parametros = new Dictionary<string, object>();
            parametros[ltrParametrosNeg.EstoyLeyendoParaAnalizarElModoDeAcceso] = true;
            try
            {
                var registro = idElemento == 0
                    ? null
                    : (RegistroDtm)gestor.LeerRegistroPorId(idElemento, aplicarJoin: false, parametros: parametros);
                return LeerModoDeAcceso(contexto, negocio, registro);
            }
            finally
            {
                parametros[ltrParametrosNeg.EstoyLeyendoParaAnalizarElModoDeAcceso] = false;
            }
        }

        public static enumModoDeAccesoDeDatos LeerModoDeAcceso(ContextoSe contexto, enumNegocio negocio, RegistroDtm registro)
        {
            if (!NegociosDeSe.UsaSeguridad(negocio))
                return enumModoDeAccesoDeDatos.Administrador;

            var modoAccesoAlNegocio = LeerModoDeAccesoAlNegocio(contexto, negocio);
            var esHistorico = NegociosDeSe.UsaEstado(negocio)
                ? EsHistorico(contexto, negocio, registro).historico
                : false;

            esHistorico = esHistorico || (NegociosDeSe.UsaBaja(negocio) && registro != null ? ((IUsaBaja)registro).Baja : false);
            var modoDeAcceso = esHistorico ? enumModoDeAccesoDeDatos.Consultor : modoAccesoAlNegocio;

            if (registro is not null && registro.Id > 0 && negocio.UsaFlujo() && negocio.EstaEnEtapaDeConsulta(contexto, registro))
            {
                return enumModoDeAccesoDeDatos.Consultor;
            }

            if (modoDeAcceso.SoyAdministrador() || registro == null)
                return modoDeAcceso;

            var modoAccesoAlElemento = enumModoDeAccesoDeDatos.SinPermiso;
            if (registro.Id > 0 && NegociosDeSe.UsaPermisosPorElemento(NegociosDeSe.TipoDtm(negocio)))
            {
                modoAccesoAlElemento = LeerModoDeAccesoAlElemento(contexto, negocio, registro);
                modoAccesoAlElemento = esHistorico ? enumModoDeAccesoDeDatos.Consultor : modoAccesoAlElemento;

                if (modoAccesoAlElemento.SoyAdministrador())
                    return modoAccesoAlElemento;

                if (modoDeAcceso.SoyConsultor() && ModoDeAcceso.SoyGestor(modoAccesoAlElemento))
                    modoDeAcceso = enumModoDeAccesoDeDatos.Gestor;
            }

            if (!NegociosDeSe.UsaTipo(negocio) && !NegociosDeSe.UsaPermisosPorCg(negocio))
                return !modoDeAcceso.SoyConsultor() ? enumModoDeAccesoDeDatos.SinPermiso : modoDeAcceso;


            var modoDeAccesoAlCg = enumModoDeAccesoDeDatos.Gestor;
            if (NegociosDeSe.UsaPermisosPorCg(negocio))
            {
                modoDeAccesoAlCg = ModoDeAccesoAlCg(contexto, negocio, ((IUsaCg)registro).IdCg);

                var puedoConsultarElElemento = modoAccesoAlElemento.SoyConsultor();
                var puedoAccederAlCg = modoDeAccesoAlCg.SoyConsultor();

                if (!puedoAccederAlCg && !puedoConsultarElElemento)
                    return enumModoDeAccesoDeDatos.SinPermiso;

                if (modoDeAccesoAlCg == enumModoDeAccesoDeDatos.Consultor && modoAccesoAlElemento != enumModoDeAccesoDeDatos.SinPermiso)
                    return modoAccesoAlElemento;
            }

            var modoDeAccesoAlTipo = enumModoDeAccesoDeDatos.Gestor;
            if (NegociosDeSe.UsaTipo(negocio))
            {
                modoDeAccesoAlTipo = ModoDeAccesoAlTipo(contexto, negocio, registro);

                if (!modoAccesoAlElemento.SoyConsultor() && !modoDeAccesoAlTipo.SoyConsultor())
                    return enumModoDeAccesoDeDatos.SinPermiso;

                if (!modoAccesoAlElemento.SoyConsultor() && modoDeAccesoAlTipo == enumModoDeAccesoDeDatos.Consultor)
                    return enumModoDeAccesoDeDatos.Consultor;

                if (!modoDeAccesoAlTipo.SoyGestor() && !modoAccesoAlElemento.SoyGestor())
                    return enumModoDeAccesoDeDatos.Consultor;
            }

            if (!modoDeAccesoAlCg.SoyConsultor() || !modoDeAccesoAlTipo.SoyConsultor())
                return modoAccesoAlElemento != enumModoDeAccesoDeDatos.SinPermiso
                    ? modoAccesoAlElemento
                    : enumModoDeAccesoDeDatos.SinPermiso;

            if (modoDeAccesoAlCg.SoyGestor() && modoDeAccesoAlTipo.SoyAdministrador())
                return esHistorico ? enumModoDeAccesoDeDatos.Consultor : enumModoDeAccesoDeDatos.Administrador;

            if (modoAccesoAlElemento.SoyGestor())
                return enumModoDeAccesoDeDatos.Gestor;

            if (modoDeAccesoAlCg == enumModoDeAccesoDeDatos.Consultor) return enumModoDeAccesoDeDatos.Consultor;


            if (!modoDeAccesoAlTipo.SoyConsultor())
                return enumModoDeAccesoDeDatos.SinPermiso;

            return esHistorico ? enumModoDeAccesoDeDatos.Consultor : modoDeAccesoAlTipo;
        }

        private static List<UsuariosDeUnPermisoDtm> PermisosSobreLaAgenda(ContextoSe contexto, AgendaDtm agenda)
        {
            var filtros = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(UsuariosDeUnPermisoDtm.IdPermiso), enumCriteriosDeFiltrado.esAlgunoDe, $"{agenda.IdConsultor},{agenda.IdGestor},{agenda.IdInterventor}"),
                new ClausulaDeFiltrado(nameof(UsuariosDeUnPermisoDtm.IdUsuario), enumCriteriosDeFiltrado.igual, contexto.DatosDeConexion.IdUsuario)
            };
            return contexto.Registros<PermisosDeUnUsuarioDto, UsuariosDeUnPermisoDtm>(filtros);

        }

        private static bool HayPermisosPorTipo(ContextoSe contexto, RegistroDtm registro, Type tipo, enumModoDeAccesoDeDatos modoSolicitado)
        {
            var a = TipoDeElementoSql.LeerTipoPorId(contexto, tipo, ((IUsaTipo)registro).IdTipo);

            if (modoSolicitado == enumModoDeAccesoDeDatos.Consultor && PermisosPorTipoSql.UsuarioConAlgunPermiso(contexto, new List<int> { a.IdPermisoDeConsultor, a.IdPermisoDeGestor, a.IdPermisoDeAdministrador }))
                return true;

            if (modoSolicitado == enumModoDeAccesoDeDatos.Gestor && PermisosPorTipoSql.UsuarioConAlgunPermiso(contexto, new List<int> { a.IdPermisoDeGestor, a.IdPermisoDeAdministrador }))
                return true;

            return false;
        }

        public static bool HayPermisosPorCg(ContextoSe contexto, enumNegocio negocio, int idCg, enumModoDeAccesoDeDatos modoSolicitado)
        {
            var a = PermisosPorCg(contexto, negocio, idCg);

            if (a.Count() == 0)
                GestorDeErrores.Emitir($"No están definidos los permisos por CG para la entidad de negocio '{negocio}', defínalos.");

            if (contexto.DatosDeConexion.EsAdministrador) return true;

            if (modoSolicitado == enumModoDeAccesoDeDatos.Gestor && PermisosPorCgSql.UsuarioConAlgunPermiso(contexto, new List<int> { a.ToList()[0].IdGestor }))
                return true;

            if (modoSolicitado == enumModoDeAccesoDeDatos.Consultor && PermisosPorCgSql.UsuarioConAlgunPermiso(contexto, new List<int> { a.ToList()[0].IdConsultor, a.ToList()[0].IdGestor }))
                return true;

            return false;
        }

        private static List<NegociosDeUnCgDtm> PermisosPorCg(ContextoSe contexto, enumNegocio negocio, int idCg)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Permisos_CgPorNegocio);
            var i = $"{negocio}.{idCg}";
            if (!cache.ContainsKey(i))
            {
                var c = contexto.Set<NegociosDeUnCgDtm>().Where(x => x.IdCg.Equals(idCg) && x.IdNegocio.Equals(negocio.IdNegocio()));
                cache[i] = c.ToList();
            }
            return (List<NegociosDeUnCgDtm>)cache[i];
        }

        private static bool HayPermisosPorElemento(ContextoSe contexto, enumNegocio negocio, RegistroDtm registro, enumModoDeAccesoDeDatos modoSolicitado)
        {
            if (registro == null)
                return false;

            var modo = LeerModoDeAccesoAlElemento(contexto, negocio, registro);
            if (ModoDeAcceso.SoyAdministrador(modoSolicitado))
                return ModoDeAcceso.SoyAdministrador(modo);

            if (ModoDeAcceso.SoyGestor(modoSolicitado))
                return ModoDeAcceso.SoyGestor(modo);

            if (ModoDeAcceso.SoyConsultor(modoSolicitado))
                return ModoDeAcceso.SoyConsultor(modo);

            return false;
        }

        public static enumModoDeAccesoDeDatos ModoDeAccesoAlTipo(ContextoSe contexto, enumNegocio negocio, RegistroDtm registro)
        {
            var permisosDeUsuario = PermisosPorTipoSql.PermisosDeUsuario(contexto, NegociosDeSe.IdNegocio(negocio), registro == null ? 0 : ((IUsaTipo)registro).IdTipo);
            var modoAcceso = enumModoDeAccesoDeDatos.Administrador;
            var comoMuchoConsultor = NegociosDeSe.UsaFlujo(negocio) && registro != null && !HayPermisosPorEstado(contexto, negocio, registro);
            if (!EsAdministradorDeTipos(contexto, permisosDeUsuario))
            {
                modoAcceso = enumModoDeAccesoDeDatos.Gestor;
                if (!EsGestorDeTipos(contexto, permisosDeUsuario))
                {
                    return permisosDeUsuario.Count > 0 ? enumModoDeAccesoDeDatos.Consultor : enumModoDeAccesoDeDatos.SinPermiso;
                }
            }
            return comoMuchoConsultor ? enumModoDeAccesoDeDatos.Consultor : modoAcceso;
        }

        public static bool EsInterventorDelTipo(ContextoSe contexto, enumNegocio negocio, RegistroDtm registro)
        {
            if (contexto.DatosDeConexion.EsAdministrador) return true;
            var permisosDeUsuario = PermisosPorTipoSql.PermisosDeUsuario(contexto, NegociosDeSe.IdNegocio(negocio), registro == null ? 0 : ((IUsaTipo)registro).IdTipo);
            return EsInterventorDeTipos(contexto, permisosDeUsuario);
        }

        private static void ValidarPermisosDeRelacion(ContextoSe contexto, enumNegocio negocio1, enumNegocio negocio2, IRelacion registro)
        {
            var ma = LeerModoDeAccesoDeRelacion(contexto, negocio1, negocio2, registro);
            if (!ModoDeAcceso.SoyGestor(ma))
                GestorDeErrores.Emitir($"El usuario conectado no tiene permisos de relación, necesita al menos permiso de gestor sobre alguno de los negocios y consultor sobre el otro");
        }

        private static bool EsAdministradorDeTipos(ContextoSe contexto, List<PermisosPorTipoDtm> permisosDeUsuario)
        {
            foreach (var permisoDeUsuario in permisosDeUsuario)
            {
                var permiso = contexto.Set<PermisoDtm>().AsNoTracking().Include(x => x.Tipo).FirstOrDefault(x => x.Id.Equals(permisoDeUsuario.IdPermiso));
                var modo = ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(permiso.Tipo.Nombre);
                if (ModoDeAcceso.SoyAdministrador(modo))
                    return true;
            }
            return false;
        }

        public static bool EsInterventorDeTipos(ContextoSe contexto, List<PermisosPorTipoDtm> permisosDeUsuario)
        {
            foreach (var permisoDeUsuario in permisosDeUsuario)
            {
                var permiso = contexto.Set<PermisoDtm>().Include(x => x.Tipo).FirstOrDefault(x => x.Id.Equals(permisoDeUsuario.IdPermiso));
                var modo = ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(permiso.Tipo.Nombre);
                if (ModoDeAcceso.SoyInterventor(modo))
                    return true;
            }
            return false;
        }

        private static bool EsGestorDeTipos(ContextoSe contexto, List<PermisosPorTipoDtm> permisosDeUsuario)
        {
            foreach (var permisoDeUsuario in permisosDeUsuario)
            {
                var permiso = contexto.Set<PermisoDtm>().Include(x => x.Tipo).FirstOrDefault(x => x.Id.Equals(permisoDeUsuario.IdPermiso));
                var modo = ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(permiso.Tipo.Nombre);
                if (ModoDeAcceso.SoyGestor(modo))
                    return true;
                if (ModoDeAcceso.SoyAdministrador(modo))
                    return true;
            }
            return false;
        }

        private static bool EsAdministradorDelNegocio(ContextoSe contexto, enumNegocio negocio)
        {
            var permisosDeUsuario = PermisosPorNegocioSql.PermisosDeUsuario(contexto, NegociosDeSe.IdNegocio(negocio));
            foreach (var permisoDeUsuario in permisosDeUsuario)
            {
                var permiso = contexto.Set<PermisoDtm>().AsNoTracking().Include(x => x.Tipo).FirstOrDefault(x => x.Id.Equals(permisoDeUsuario.IdPermiso));
                var modo = ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(permiso.Tipo.Nombre);
                if (ModoDeAcceso.SoyAdministrador(modo))
                    return true;
            }
            return false;
        }

        private static bool EsGestorDelNegocio(ContextoSe contexto, enumNegocio negocio)
        {
            var permisosDeUsuario = PermisosPorNegocioSql.PermisosDeUsuario(contexto, NegociosDeSe.IdNegocio(negocio));
            foreach (var permisoDeUsuario in permisosDeUsuario)
            {
                var permiso = contexto.Set<PermisoDtm>().AsNoTracking().Include(x => x.Tipo).FirstOrDefault(x => x.Id.Equals(permisoDeUsuario.IdPermiso));
                var modo = ApiDeEnsamblados.ToEnumerado<enumModoDeAccesoDeDatos>(permiso.Tipo.Nombre);
                if (ModoDeAcceso.SoyGestor(modo))
                    return true;
            }
            return false;
        }

        private static bool EsConsultorDelNegocio(ContextoSe contexto, enumNegocio negocio)
        {
            var permisosDeUsuario = PermisosPorNegocioSql.PermisosDeUsuario(contexto, NegociosDeSe.IdNegocio(negocio));
            return permisosDeUsuario.Count > 0;
        }

        private static enumModoDeAccesoDeDatos ModoDeAccesoAlCg(ContextoSe contexto, enumNegocio negocio, int idCg)
        {
            if (!HayPermisosPorCg(contexto, negocio, idCg, enumModoDeAccesoDeDatos.Gestor))
            {
                if (!HayPermisosPorCg(contexto, negocio, idCg, enumModoDeAccesoDeDatos.Consultor))
                    return enumModoDeAccesoDeDatos.SinPermiso;
                else
                    return enumModoDeAccesoDeDatos.Consultor;
            }

            return enumModoDeAccesoDeDatos.Gestor;
        }

        public static enumModoDeAccesoDeDatos LeerModoDeAccesoAlElemento(ContextoSe contexto, enumNegocio negocio, RegistroDtm registro)
        {
            var modo = enumModoDeAccesoDeDatos.SinPermiso;

            var permisos = PermisosPorElementoSql.PermisosDeUsuario(contexto, NegociosDeSe.IdNegocio(negocio), registro.Id);
            var esGestor = false;
            foreach (var permiso in permisos)
            {
                var permisoDtm = contexto.SeleccionarPorId<PermisoDtm>(permiso.IdPermiso, aplicarJoin: true); // .Set<PermisoDtm>().Include(x => x.Tipo).FirstOrDefault(x => x.Id.Equals(permiso.IdPermiso));
                var modoLeido = permisoDtm.Tipo.Nombre.ToModoAcceso();
                if (modoLeido.SoyAdministrador())
                {
                    modo = enumModoDeAccesoDeDatos.Administrador;
                    break;
                }
                if (esGestor)
                    continue;

                if (modoLeido.SoyGestor())
                {
                    modo = enumModoDeAccesoDeDatos.Gestor;
                    esGestor = true;
                    continue;
                }

                if (modoLeido.SoyConsultor())
                    modo = enumModoDeAccesoDeDatos.Consultor;
            }
            return modo;
        }

        private static bool HayPermisosPorEstado(ContextoSe contexto, enumNegocio negocio, RegistroDtm registro)
        {
            if (negocio.TipoDtm().Cancelados(contexto).Contains(((IUsaEstado)registro).IdEstado) ||
                negocio.TipoDtm().Terminados(contexto).Contains(((IUsaEstado)registro).IdEstado))
                return false;
            //Si es interventor del registro y el registro no es cancelado ni terminado y tiene permisos de interventor, entonces return true
            var elemento = (IElementoDeProcesoDtm)registro;

            //Si estoy creando
            var idEstado = elemento.IdEstado == 0 && elemento.IdTipo > 0 ? elemento.IdDelEstadoInicial(contexto) : elemento.IdEstado;

            return PermisosPorEstadoSql.PermisosDeUsuario(contexto, negocio.IdNegocio(), idEstado).ToList().Count() > 0;
        }

        private static (bool historico, bool cancelado, bool terminado) EsHistorico(ContextoSe contexto, enumNegocio negocio, RegistroDtm registro)
        {
            if (registro == null) return (false, false, false);
            var terminado = negocio.TipoDtm().Terminados(contexto).Contains(((IUsaEstado)registro).IdEstado);
            var cancelado = negocio.TipoDtm().Cancelados(contexto).Contains(((IUsaEstado)registro).IdEstado);
            return (historico: terminado || cancelado, cancelado: cancelado, terminado: terminado);
        }

        internal static TransicionDtm ValidarTransicion<TContexto, TElementoDeProceso>(TContexto contexto, enumNegocio negocio, TElementoDeProceso registro, int idTransicion, Dictionary<string, object> parametros)
        where TContexto : ContextoSe
        where TElementoDeProceso : IElementoDeProcesoDtm
        {
            var transicion = TransicionSql.LeerTransicionPorId(contexto, ApiDeTransicion.TablaDeTransiciones(negocio.TipoDtm()), idTransicion);
            var diferenteEstadoOrigen = transicion.IdOrigen != ((IElementoDeProcesoDtm)registro).IdEstado;
            var estaEnElDestino = transicion.IdDestino == ((IElementoDeProcesoDtm)registro).IdEstado;

            if (diferenteEstadoOrigen && estaEnElDestino && parametros.LeerValor(ltrParametrosNeg.NoTransitarSiYaLoEsta, false))
                return null;

            if (diferenteEstadoOrigen)
            {
                var estado = registro.Estado(contexto);
                GestorDeErrores.Emitir($"No se puede aplicar la transición '{transicion.Nombre}' al {negocio.Singular(true)} '{((IUsaReferencia)registro).Referencia}' por estar en el " +
                    $"estado '{estado.Nombre}', la transición " +
                    $"está parametrizada para transitar desde el " +
                    $"estado '{transicion.Origen}' al " +
                    $"estado '{transicion.Destino}'");
            }

            if (transicion.DelSistema && !transicion.Activo)
                GestorDeErrores.Emitir($"La transición del sistema '{transicion.Nombre}' no se puede ejecutar por no estar activa");

            if (!transicion.Activo)
                GestorDeErrores.Emitir($"La transición '{transicion.Nombre}' no se puede ejecutar por no estar activa");

            if ((bool)parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDePersistencia, true) && !HayPermisosDeTransicion(contexto, negocio, transicion))
                GestorDeErrores.Emitir($"El usuario '{contexto.DatosDeConexion.Login}' no puede ejecutar la transición '{transicion.Nombre}'");

            return transicion;
        }

        public class PermisoAsignado
        {
            public int IdPermiso { get; set; }
            public string Nombre { get; set; }
            public string Tipo { get; set; }
            public string Clase { get; set; }
        }

        public static IEnumerable<PermisoAsignado> PermisosCalculadosDe(ContextoSe contexto, int idUsuario, enumClaseDePermiso clase)
        {
            return contexto.Set<UsuarioDtm>()
                       .Join(contexto.Set<PuestosDeUnUsuarioDtm>(), u => u.Id, up => up.IdUsuario, (u, up) => new { Usuario = u, Usu_Puesto = up })
                       .Join(contexto.Set<RolesDeUnPuestoDtm>(), up => up.Usu_Puesto.IdPuesto, rp => rp.IdPuesto, (up, rp) => new { up.Usuario, up.Usu_Puesto, Rol_Puesto = rp })
                       .Join(contexto.Set<PermisosDeUnRolDtm>(), rp => rp.Rol_Puesto.IdRol, rp2 => rp2.IdRol, (rp, rp2) => new { rp.Usuario, rp.Usu_Puesto, rp.Rol_Puesto, Rol_Permiso = rp2 })
                       .Join(contexto.Set<PermisoDtm>(), rp2 => rp2.Rol_Permiso.IdPermiso, p => p.Id, (rp2, p) => new { rp2.Usuario, rp2.Usu_Puesto, rp2.Rol_Puesto, rp2.Rol_Permiso, Permiso = p })
                       .Join(contexto.Set<TipoPermisoDtm>(), p => p.Permiso.IdTipo, tp => tp.Id, (p, tp) => new { p.Usuario, p.Usu_Puesto, p.Rol_Puesto, p.Rol_Permiso, p.Permiso, Tipo_Permiso = tp })
                       .Join(contexto.Set<ClasePermisoDtm>(), p => p.Permiso.IdClase, cp => cp.Id, (p, cp) => new { p.Usuario, p.Usu_Puesto, p.Rol_Puesto, p.Rol_Permiso, p.Permiso, p.Tipo_Permiso, Clase_Permiso = cp })
                       .Where(x => x.Clase_Permiso.Nombre.Equals(clase.ToString()) && x.Usuario.Id == idUsuario)
                       .OrderBy(x => x.Usuario.Id)
                       .ThenBy(x => x.Permiso.Nombre)
                       .Select(x => new PermisoAsignado
                       {
                           IdPermiso = x.Rol_Permiso.IdPermiso,
                           Nombre = x.Permiso.Nombre,
                           Tipo = x.Tipo_Permiso.Nombre,
                           Clase = x.Clase_Permiso.Nombre
                       });
        }

        public static IEnumerable<PermisoAsignado> PermisosDirectosDe(ContextoSe contexto, int idUsuario, enumClaseDePermiso clase)
        {
            return contexto.Set<PuestosDeUnUsuarioDtm>().Where(x => x.IdUsuario == idUsuario)
                .Join(contexto.Set<PermisosDirectosDtm>(),
                    t2 => t2.IdPuesto,
                    t1 => t1.IdPuesto,
                    (t2, t1) => new { IDUSUA = t2.IdUsuario, IDPERMISO = t1.IdPermiso })
                .Join(contexto.Set<PermisoDtm>(),
                    t1 => t1.IDPERMISO,
                    t3 => t3.Id,
                    (t1, t3) => new { permiso = t3.Nombre, IDPERMISO = t1.IDPERMISO, IDTIPO = t3.IdTipo, IDCLASE = t3.IdClase })
                .Join(contexto.Set<TipoPermisoDtm>(),
                    t3 => t3.IDTIPO,
                    t6 => t6.Id,
                    (t3, t6) => new { permiso = t3.permiso, tipopermiso = t6.Nombre, IDPERMISO = t3.IDPERMISO, IDCLASE = t3.IDCLASE })
                .Join(contexto.Set<ClasePermisoDtm>(),
                    t3 => t3.IDCLASE,
                    t7 => t7.Id,
                    (t3, t7) => new { permiso = t3.permiso, tipopermiso = t3.tipopermiso, clasepermiso = t7.Nombre, IDPERMISO = t3.IDPERMISO })
                .Where(t => t.clasepermiso == clase.ToString())
                .Select(t => new PermisoAsignado { Nombre = t.permiso, Tipo = t.tipopermiso, Clase = t.clasepermiso, IdPermiso = t.IDPERMISO });
        }
    }
}
