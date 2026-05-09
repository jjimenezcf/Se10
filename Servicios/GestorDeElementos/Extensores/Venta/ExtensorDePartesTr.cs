using System;
using Utilidades;
using ServicioDeDatos.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using System.Collections.Generic;
using ServicioDeDatos.Terceros;
using System.Linq;
using static Gestor.Errores.GestorDeErrores;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using Gestor.Errores;
using ModeloDeDto.Negocio;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDePartesTr
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(LineaDeUnPtrDtm))
                return true;

            if (tipoDeDetalle == typeof(AsignacionDePtrDtm))
                return true;

            return false;
        }

        public static decimal Total(this ParteTrDtm elemento, ContextoSe contexto, bool conIva)
        {
            var cacheSinIva = ServicioDeCaches.Obtener(CacheDe.Ptr_TotalSinIva);
            var cacheConIva = ServicioDeCaches.Obtener(CacheDe.Ptr_TotalConIva);
            if (!cacheSinIva.ContainsKey(elemento.Id.ToString()))
            {
                decimal totalSinIva = 0;
                decimal totalConIva = 0;
                var lineas = elemento.Detalles<LineaDeUnPtrDtm>(contexto);
                foreach (var linea in lineas)
                {
                    if (linea.TipoDeLinea == Enumerados.enumTipoDeLinea.Comentario)
                        continue;
                    totalSinIva = totalSinIva + linea.ImporteConDto;
                    totalConIva = totalConIva + (decimal)linea.ImporteDeLinea;
                }
                cacheSinIva[$"{elemento.Id}"] = totalSinIva;
                cacheConIva[$"{elemento.Id}"] = totalConIva;
            }
            return conIva ? (decimal)cacheConIva[$"{elemento.Id}"] : (decimal)cacheSinIva[$"{elemento.Id}"];
        }

        public static TareaDtm CrearTarea(this ParteTrDtm parte, ContextoSe contexto, string tipo)
        {
            var tipoTarea = contexto.SeleccionarPorNombre<TipoDeTareaDtm>(tipo);
            var tarea = new TareaDtm
            {
                IdTipo = tipoTarea.Id,
                IdCg = parte.IdCg,
                Nombre = $"Tarea del parte {parte.Referencia}",
                Descripcion = $"{parte.Descripcion}",
                ClaseDeTarea = tipoTarea.ClaseDeTarea,
                IdSolicitante = parte.Cliente(contexto).IdInterlocutor
            }.Insertar(contexto);

            parte.Vincular(contexto, tarea);
            return tarea;
        }

        public static ParteTrDtm CrearParteTr(this PresupuestoDtm ppt, ContextoSe contexto)
        {
            var idTipoParteTr = ppt.Tipo<TipoDePresupuestoDtm>(contexto).IdTipoParteTr;
            if (idTipoParteTr == null)
                Emitir($"El tipo de {enumNegocio.Presupuesto.Singular(true)} {ppt.Tipo<TipoDePresupuestoDtm>(contexto).Nombre} no tiene indicado que tipo de {enumNegocio.ParteDeTrabajo.Singular(true)} a de generar");

            ParteTrDtm parte = new ParteTrDtm();
            parte.IdCg = ppt.IdCg;
            parte.IdTipo = (int)idTipoParteTr;
            parte.Nombre = $"Factura correspondiente al presupuesto {ppt.Referencia}";
            parte.Descripcion = ppt.Descripcion;
            parte.IdCliente = ppt.Cliente(contexto).Id;
            parte.Contacto = ppt.Contacto;
            parte.Telefono = ppt.Telefono;
            parte.eMail = ppt.eMail;
            parte.IdPresupuesto = ppt.Id;
            parte.IdFacturaEmt = null;
            parte.IdContrato = null;
            parte = parte.Insertar(contexto);

            parte.CopiarLineas(contexto, ppt);
            return parte;
        }

        public static void CopiarLineasDelPpt(this ParteTrDtm parte, ContextoSe contexto)
        =>
        parte.CopiarLineas(contexto, contexto.SeleccionarPorId<PresupuestoDtm>((int)parte.IdPresupuesto));

        public static ParteTrDtm CrearParteTr(this PlanificacionDeVentaDtm plv, ContextoSe contexto)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                var parteTr = new ParteTrDtm
                {
                    IdCg = plv.IdCg,
                    IdTipo = (int)plv.IdTipoDeParte,
                    IdCliente = plv.IdCliente,
                    IdContrato = plv.IdContrato,
                    Nombre = plv.Nombre,
                    Descripcion = plv.Descripcion
                }
                .Insertar(contexto);
                var lineas = plv.Detalles<LineaDeUnaPlfVentaDtm>(contexto);

                if (lineas.Count == 0)
                    Emitir($"La planificación {plv.Referencia} no tiene definido el detalle, no se puede genar partes de trabajo");

                foreach (var linea in lineas)
                {
                    new LineaDeUnPtrDtm
                    {
                        IdElemento = parteTr.Id,
                        Orden = linea.Orden,
                        TipoDeLinea = linea.TipoDeLinea,
                        IdUnitario = linea.IdUnitario,
                        Concepto = linea.Concepto,
                        Cantidad = linea.Cantidad,
                        Precio = linea.Venta,
                        Anotacion = linea.Anotacion,
                        Descuento = linea.Descuento,
                        IdIvaR = linea.IdIvaR,
                        Iva = linea.Iva,
                        Clase = linea.Clase,
                        IdUnidad = linea.IdUnidad,
                        IdNaturaleza = linea.IdNaturaleza
                    }
                    .Insertar(contexto);
                }
                contexto.Commit(tran);
                return parteTr;
            }
            catch (Exception exc)
            {
                contexto.Rollback(tran, exc);
                throw;
            }
        }

        public static void PersistirEvento(this AsignacionDePtrDtm asignacionPtr, ContextoSe contexto, ParametrosDeNegocio parametros)
        {
            if (parametros.Insertando)
            {
                asignacionPtr.CrearEvento(contexto);
                return;
            }

            var parte = asignacionPtr.DetalleDe<ParteTrDtm>(contexto);
            var nombreEvento = ltrDeUnParteTr.EventoDePlanificacion.Replace($"[{nameof(ParteTrDtm.Referencia)}]", parte.Referencia);
            var eventos = contexto.SeleccionarEventos(asignacionPtr.IdElemento, nombreEvento);

            if (parametros.Eliminando || asignacionPtr.PlfDeFin == default || asignacionPtr.PlfDeInicio == default)
            {
                parte.EliminarEventos(contexto, eventos);
                return;
            }

            if (eventos.Count > 1)
            {
                var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
                for (var i = eventos.Count() - 1; i > 0; i--)
                    parte.Desvincular(contexto, eventos[i], p);
            }

            if (eventos.Count > 0)
                asignacionPtr.ModificarEvento(eventos[0], contexto);
            else
                asignacionPtr.CrearEvento(contexto);
        }

        public static void AntesDeDarPorCancelada(this ParteTrDtm parte, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            //que no hay líneas a facturar que estén realizadas
            var asignaciones = parte.Detalles<AsignacionDePtrDtm>(contexto);
            foreach (var asignacion in asignaciones)
            {
                if (asignacion.Finalizada != null || asignacion.Duracion != null || asignacion.MedidoEn != null || asignacion.Iniciada != null)
                    Emitir($"No se puede cancelar el parte de trabajo {parte.Referencia} ya que tiene asignaciones iniciadas o terminadas");
            }

            //validar que las tareas asociadas están canceladas o terminadas
            var tareas = parte.Vinculados<TareaDtm>(contexto, parametros,
                new Dictionary<string, object> {
                    {
                        ltrDeVinculos.EstadosDelVinculadoDiferentes,
                        enumEtapasDeTareas.TAR_Etapa_Terminada.Estados() + "," +  enumEtapasDeTareas.TAR_Etapa_Cancelado.Estados()
                    }
                });

            foreach (var tarea in tareas)
                Emitir($"No se puede dar por realizado el parte de trabajo {parte.Referencia} ya que la tarea {tarea.Referencia} está sin terminar o no está cancelada");
        }

        public static ParteTrDtm TrasCancelar(this ParteTrDtm parte, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            //Eliminar asignaciones si la fecha de cancelación es anterior a la planificación
            var asignaciones = parte.Detalles<AsignacionDePtrDtm>(contexto);
            foreach (var asignacion in asignaciones.Where(asignacion => asignacion.PlfDeInicio.Fecha().Date > DateTime.Now.Date))
            {
                asignacion.Eliminar(contexto, esUnaAccion: true);
            }

            return parte.Recargar(contexto);
        }

        public static void AntesDeDarPorRealizado(this ParteTrDtm parte, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parte.HayDetallesDe<LineaDeUnPtrDtm>(contexto))
                Emitir($"No se puede dar por realizado el {enumNegocio.ParteDeTrabajo.Singular(true)} '{parte.Referencia}' por no tener indicado lo realizado");

            parte.ValidarTareasVinculadasEstanFinalizadas(contexto, parametros);
            parte.ValidarDatosDeEjecucion(contexto);
            parte.ValidarDatosDeDuracion(contexto);

        }

        public static void DespuesDeDarPorRealizado(this ParteTrDtm parte, ContextoSe contexto)
        {
            var plf = parte.Planificacion(contexto);
            if (plf != null && plf.IdContrato.Entero() > 0)
            {
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(plf.IdContrato.Entero());
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.RealizarPartePlanificado, parte.Total(contexto, conIva: false), plf.Total(contexto, conIva: false));
            }
            if (plf == null && parte.IdContrato.Entero() > 0)
            {
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(parte.IdContrato.Entero());
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.RealizarParteDeContrato, parte.Total(contexto, conIva: false), 0);
            }
        }

        public static void AntesDePrefacturar(this ParteTrDtm parte, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if (!parte.HayDetallesDe<LineaDeUnPtrDtm>(contexto))
                Emitir("No se ha indicado las líneas del parte de trabajo, no se puede dar como realizado");

            parte.ValidarTareasVinculadasEstanFinalizadas(contexto, parametros);
            parte.ValidarDatosDeEjecucion(contexto);
            parte.ValidarDatosDeDuracion(contexto);
            if (!(bool)parametros.LeerValor(ltrDeUnaFacturaEmt.FacturadoComoLinea, false))
                parametros[ltrDeUnaFacturaEmt.PrefacturaEmitida] = parte.CrearPrefactura(contexto);
        }

        public static ParteTrDtm AlCancelarPrefactura(this ParteTrDtm parte, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            //Blanquear la factura
            var factura = parte.IdFacturaEmt.Entero() > 0
            ? contexto.SeleccionarPorId<FacturaEmtDtm>((int)parte.IdFacturaEmt)
            : parametros.ContainsKey(ltrDeUnaFacturaEmt.FacturaDelParte)
            ? contexto.SeleccionarPorId<FacturaEmtDtm>(((FacturaEmtDtm)parametros[ltrDeUnaFacturaEmt.FacturaDelParte]).Id)
            : null;
            parte.IdFacturaEmt = null;
            parte.FacturaEmt = null;
            //parte = parte.Modificar(contexto, esUnaAccion: true);
            if (factura != null)
            {
                //Anotar traza de que se canceló la prefactura y su número
                new TrazasDeUnParteTrDtm
                {
                    IdElemento = parte.Id,
                    Negocio = enumNegocio.ParteDeTrabajo,
                    Nombre = ltrDeUnParteTr.TrazaDeCancelacionDePrefactura.Replace($"[{nameof(FacturaEmtDtm.Referencia)}]", factura.Referencia),
                    Descripcion = ltrDeUnParteTr.DescripcionDeTrazaDeCancelacionDePrefactura.
                                  Replace($"[{nameof(ContextoSe.DatosDeConexion.Login)}]", contexto.DatosDeConexion.Login).
                                  Replace($"[{nameof(FacturaEmtDtm.Expresion)}]", factura.Expresion)
                }.InsertarTraza(contexto);

            }

            return parte;
        }

        public static ParteTrDtm DespuesDePrefacturar(this ParteTrDtm parte, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            if ((bool)parametros.LeerValor(ltrDeUnaFacturaEmt.FacturadoComoLinea, false))
                parte.IdFacturaEmt = ((FacturaEmtDtm)parametros[ltrDeUnaFacturaEmt.FacturaDelParte]).Id;
            else
                parte.IdFacturaEmt = ((FacturaEmtDtm)parametros[ltrDeUnaFacturaEmt.PrefacturaEmitida]).Id;
            parte = parte.Modificar(contexto, esUnaAccion: true);
            return parte;
        }

        public static ParteTrDtm TrasDevolverAPendiente(this ParteTrDtm parte, ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var asignaciones = parte.Detalles<AsignacionDePtrDtm>(contexto);
            foreach (var asignacion in asignaciones)
            {
                asignacion.Finalizada = null;
                asignacion.Duracion = null;
                asignacion.MedidoEn = null;
                asignacion.Iniciada = null;
                asignacion.Modificar(contexto, esUnaAccion: true);
            }

            var plf = parte.Planificacion(contexto);
            if (plf != null && plf.IdContrato.Entero() > 0)
            {
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(plf.IdContrato.Entero());
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.AnularRealizacionDeParte, plf.Total(contexto, conIva: false), parte.Total(contexto, conIva: false));
            }

            if (plf == null && parte.IdContrato.Entero() > 0)
            {
                var contrato = contexto.SeleccionarPorId<ContratoDtm>(parte.IdContrato.Entero());
                contrato.RecalcularAvance(contexto, enumAvaceOperacion.AnularRealizacionDeContrato, 0, parte.Total(contexto, conIva: false));
            }
            return parte;
        }

        public static enumModoDeAccesoDeDatos ModoDeAccesoAlParteTr(this ParteTrDtm parte, ContextoSe contexto)
        {
            var modo = ApiDePermisos.LeerModoDeAcceso(contexto, enumNegocio.ParteDeTrabajo, parte.Id);
            if (ModoDeAcceso.SoyGestor(modo))
            {
                if (
                parte.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Pdt_Facturar) ||
                parte.EstaEnLaEtapa(enumEtapasDePartesTr.PTR_Etapa_Facturado))
                    return enumModoDeAccesoDeDatos.Consultor;
            }

            return modo;
        }

        public static PlanificacionDeVentaDtm Planificacion(this ParteTrDtm parte, ContextoSe contexto)
        =>
        Planificacion(contexto, parte.Id);

        public static PlanificacionDeVentaDtm Planificacion(ContextoSe contexto, int idParte)
        {
            var cachePtr = ServicioDeCaches.Obtener(CacheDe.PlvDeUnParteTr);
            if (!cachePtr.ContainsKey(idParte.ToString()))
            {
                cachePtr[idParte.ToString()] = contexto.SeleccionarPorFk<PlanificacionDeVentaDtm>(nameof(PlanificacionDeVentaDtm.IdParteTr), idParte, errorSiNoHay: false);
            }
            return (PlanificacionDeVentaDtm)cachePtr[idParte.ToString()];
        }

        public static void ValidarTieneFacturaEmt(ContextoSe contexto, List<int> idsDeFaes)
        {
            foreach (var id in idsDeFaes)
            {
                var fae = contexto.SeleccionarPorId<FacturaEmtDtm>(id);
                if (fae.IdParteTr == null)
                {
                    var lineas = fae.Detalles<LineaDeUnaFaeDtm>(contexto);
                    foreach (var linea in lineas) if (linea.IdParteTr != null) return;
                    Emitir($"La factura {fae.Referencia} no tiene partes de trabajo");
                }
            }
        }

        public static PresupuestoDtm Presupuesto(this ParteTrDtm parte, ContextoSe contexto, bool validarPermisos = false)
        {
            if (parte.IdPresupuesto is null)
                return null;
            if (parte.Presupuesto is not null)
                return parte.Presupuesto;
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, validarPermisos } };
            return contexto.SeleccionarPorId<PresupuestoDtm>((int)parte.IdPresupuesto, parametros: parametros);
        }

        public static FacturaEmtDtm FacturaEmt(this ParteTrDtm parte, ContextoSe contexto, bool validarPermisos = false)
        {
            if (parte.IdFacturaEmt is null)
                return null;
            if (parte.FacturaEmt is not null)
                return parte.FacturaEmt;
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, validarPermisos } };
            return contexto.SeleccionarPorId<FacturaEmtDtm>((int)parte.IdFacturaEmt, parametros: parametros);
        }

        public static AsignacionDePtrDtm Asignalo(this ParteTrDtm parte, ContextoSe contexto, TrabajadorDtm trabajador, DateTime inicio, DateTime fin)
        {
            return new AsignacionDePtrDtm
            {
                IdElemento = parte.Id,
                IdTrabajador = trabajador.Id,
                PlfDeInicio = inicio,
                PlfDeFin = fin
            }.Insertar(contexto);
        }

        public static AsignacionDePtrDtm DarPorRealizada(this AsignacionDePtrDtm asignacion, ContextoSe contexto, DateTime? inicio = null, DateTime? fin = null)
        {
            asignacion.Iniciada = inicio is null ? asignacion.PlfDeInicio : inicio;
            asignacion.Finalizada = fin is null ? asignacion.PlfDeFin : fin;
            asignacion.Duracion = (int)((DateTime)asignacion.Finalizada - (DateTime)asignacion.Iniciada).TotalDays;
            asignacion.MedidoEn = Enumerados.enumDurabilidad.Dias;
            return asignacion.Modificar(contexto);
        }

        public static DireccionDto DireccionDeEjecucion(this ParteTrDtm parte, ContextoSe contexto, bool errorSiNoHay = true)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.ParteDeTrabajo, parte.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.ejecucion && x.Activo);
            if (direccion == null)
            {
                direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.contacto && x.Activo);
                if (direccion == null)
                {
                    var cliente = parte.Cliente(contexto);
                    direccion = cliente.Direccion(contexto, enumCalificadorDireccion.contacto, false);
                    if (direccion is null && errorSiNoHay)
                        Emitir($"Debe definir una dirección de '{enumCalificadorDireccion.ejecucion.Descripcion().ToLower()}' para el {enumNegocio.ParteDeTrabajo.Singular(true)} '{parte.Referencia}' " +
                            $"o una de '{enumCalificadorDireccion.contacto.Descripcion().ToLower()}' para el {enumNegocio.Cliente.Singular(true)} '{parte.Cliente(contexto).NIF(contexto)}'");
                    if (direccion is null)
                        return null;
                }
            }
            return direccion.MapearDto(contexto, direccion.Negocio);
        }

        private static void EliminarEventos(this ParteTrDtm parte, ContextoSe contexto, List<EventoDeAgendaDtm> eventos)
        {
            if (eventos.Count > 0)
            {
                var p = new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } };
                foreach (var e in eventos.Where(e => e.EsDelSistema)) parte.Desvincular(contexto, e, p);
            }
            return;
        }

        private static void CrearEvento(this AsignacionDePtrDtm asignacionPtr, ContextoSe contexto)
        {
            if (asignacionPtr.PlfDeFin == default || asignacionPtr.PlfDeInicio == default)
                return;

            var trabajador = contexto.SeleccionarPorId<TrabajadorDtm>(asignacionPtr.IdTrabajador);
            if (trabajador.IdUsuario == null) return;
            var usuario = contexto.SeleccionarPorId<UsuarioDtm>((int)trabajador.IdUsuario);
            var parte = asignacionPtr.DetalleDe<ParteTrDtm>(contexto);

            var evento = new EventoDeAgendaDtm();
            evento.IdAgenda = usuario.IdAgenda;
            evento.IdElemento = asignacionPtr.IdElemento;
            evento.IdNegocio = enumNegocio.ParteDeTrabajo.IdNegocio();
            evento.Inicio = (DateTime)asignacionPtr.PlfDeInicio;
            evento.Fin = (DateTime)asignacionPtr.PlfDeFin;
            evento.Nombre = ltrDeUnParteTr.EventoDePlanificacion.Replace($"[{nameof(ParteTrDtm.Referencia)}]", parte.Referencia);
            evento.Descripcion = parte.DescripcioEvento();
            evento.EsDelSistema = true;
            GestorDeVinculos.Vincular(contexto, parte, evento, new Dictionary<string, object> { { ltrParametrosNeg.EstaEjecutandoUnaAccion, true } });
        }

        private static void ModificarEvento(this AsignacionDePtrDtm asignacionPtr, EventoDeAgendaDtm evento, ContextoSe contexto)
        {
            var parte = asignacionPtr.DetalleDe<ParteTrDtm>(contexto);
            if (evento.Inicio == asignacionPtr.PlfDeInicio && evento.Fin == asignacionPtr.PlfDeFin && evento.Descripcion == parte.DescripcioEvento())
                return;

            evento.Inicio = (DateTime)asignacionPtr.PlfDeInicio;
            evento.Fin = (DateTime)asignacionPtr.PlfDeFin;
            evento.Descripcion = parte.DescripcioEvento();
            evento.EsDelSistema = true;
            evento.Modificar(contexto, esUnaAccion: true);
        }

        private static string DescripcioEvento(this ParteTrDtm parte) => $"Se le ha asignado el parte de trabajo: {parte.Expresion}";

        private static void ValidarDatosDeEjecucion(this ParteTrDtm parte, ContextoSe contexto)
        {
            ClausulaDeFiltrado filtro;
            List<AsignacionDePtrDtm> asignaciones;
            filtro = new ClausulaDeFiltrado { Clausula = nameof(AsignacionDePtrDtm.Finalizada), Criterio = enumCriteriosDeFiltrado.esNulo };
            asignaciones = parte.Detalles<AsignacionDePtrDtm>(contexto, aplicarJoin: true, filtros: new List<ClausulaDeFiltrado> { filtro });
            if (asignaciones.Any())
                Emitir($"No se puede dar por realizado el parte {parte.Referencia} ya que hay asignaciones que no se han finalizado");
        }

        private static void ValidarDatosDeDuracion(this ParteTrDtm parte, ContextoSe contexto)
        {
            ClausulaDeFiltrado filtro;
            List<AsignacionDePtrDtm> asignaciones;
            filtro = new ClausulaDeFiltrado { Clausula = nameof(AsignacionDePtrDtm.Duracion), Criterio = enumCriteriosDeFiltrado.esNulo };
            asignaciones = parte.Detalles<AsignacionDePtrDtm>(contexto, aplicarJoin: true, filtros: new List<ClausulaDeFiltrado> { filtro });
            if (asignaciones.Any())
                Emitir($"No se puede dar por realizado {enumNegocio.ParteDeTrabajo.Singular(true)} '{parte.Referencia}' ya que hay asignaciones a las que no se ha indicado su duración");
        }

        private static void CopiarLineas(this ParteTrDtm parte, ContextoSe contexto, PresupuestoDtm ppt)
        {
            var lineas = ppt.Detalles<LineaDeUnPptDtm>(contexto);

            if (lineas.Count == 0)
                Emitir($"El {ppt.Referencia} no tiene definido el detalle con el que generar el parte de trabajo, defínalo previamente");

            foreach (var linea in lineas)
            {
                linea.ValidarIva(ppt.Referencia);

                new LineaDeUnPtrDtm
                {
                    IdElemento = parte.Id,
                    Orden = linea.Orden,
                    TipoDeLinea = linea.TipoDeLinea,
                    IdUnitario = linea.IdUnitario,
                    Concepto = linea.Concepto,
                    Cantidad = linea.Cantidad,
                    Precio = linea.Precio,
                    Anotacion = linea.Anotacion,
                    Descuento = linea.Descuento,
                    IdIvaR = linea.IdIvaR,
                    Iva = linea.Iva,
                    Clase = linea.Clase,
                    IdUnidad = linea.IdUnidad,
                    IdNaturaleza = linea.IdNaturaleza
                }
                .Insertar(contexto);
            }
        }



    }
}
