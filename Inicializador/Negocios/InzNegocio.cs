using Utilidades;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Callejero;
using ModeloDeDto.Entorno;
using ModeloDeDto.Negocio;
using ModeloDeDto.Seguridad;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Terceros;
using ModeloDeDto.TrabajosSometidos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.TrabajosSometidos;
using ServicioDeDatos.RegistroEs;
using ModeloDeDto.RegistroEs;
using ServicioDeDatos.Tarea;
using ModeloDeDto.Tarea;
using ServicioDeDatos;
using GestoresDeNegocio.RegistroEs;
using ServicioDeDatos.Expediente;
using ModeloDeDto.Expediente;
using ServicioDeDatos.Juridico;
using ModeloDeDto.Juridico;
using ServicioDeDatos.MaestrosTecnico;
using ModeloDeDto.MaestrosTecnico;
using ServicioDeDatos.Presupuesto;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Gastos;
using ModeloDeDto.Gastos;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Guarderias;
using ModeloDeDto.Guarderias;
using ServicioDeDatos.Logistica;
using ModeloDeDto.Logistica;
using ServicioDeDatos.Contabilidad;
using ModeloDeDto.Contabilidad;

namespace SistemaDeElementos.Inicializador
{
    public static class InzNegocios
    {
        public static void DefinirNegocios(ContextoSe contexto)
        {
            using (var gestor = GestorDeNegocios.Gestor(contexto, contexto.Mapeador))
            {
                gestor.Contexto.IniciarTraza(nameof(DefinirNegocios),true);
                var tran = gestor.Contexto.IniciarTransaccion();
                try
                {
                    gestor.Contexto.DatosDeConexion.CreandoModelo = true;
                    //Entorno
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Usuario, "Usuarios", typeof(UsuarioDtm), typeof(UsuarioDto), "usuario.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.VistaMvc, "Vistas", typeof(VistaMvcDtm), typeof(VistaMvcDto), "vista.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Variable, "Variables", typeof(VariableDtm), typeof(VariableDto), "cog-solid.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Menu, "Menus", typeof(MenuDtm), typeof(MenuDto), "funcionalidad-3.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Accion, "Acciones", typeof(AccionDtm), typeof(AccionDto), "Acciones.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Agenda, "Agendas", typeof(AgendaDtm), typeof(AgendaDto), "Agenda.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Certificado, "Certificados", typeof(CertificadoDtm), typeof(CertificadoDto), "Certificado.svg", esDeParametrizacion: true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.EventoDeAgenda, "Eventos del calendario", typeof(EventoDeAgendaDtm), typeof(EventoDeAgendaDto), "Agenda_1.svg", true, usaCg: false);

                    //Seguridad
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Puesto, "Puestos", typeof(PuestoDtm), typeof(PuestoDto), "puestoDeTrabajo.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Permiso, "Permisos", typeof(PermisoDtm), typeof(PermisoDto), "acceso.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Negocio, "Negocios", typeof(NegocioDtm), typeof(NegocioDto), "red.svg", true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Rol, "Roles", typeof(RolDtm), typeof(RolDto), "roles.svg", true, usaCg: false);

                    //Callejero
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Pais, "Paises", typeof(PaisDtm), typeof(PaisDto), "paises_1.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Provincia, "Provincias", typeof(ProvinciaDtm), typeof(ProvinciaDto), "provincias_1.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Correo, "Correos", typeof(CorreoDtm), typeof(CorreoDto), "Correo_1.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Municipio, "Municipios", typeof(MunicipioDtm), typeof(MunicipioDto), "municipio2.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.TipoDeVia, "Tipos de vía", typeof(TipoDeViaDtm), typeof(TipoDeViaDto), "TipoDeVia.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Calle, "Calle", typeof(CalleDtm), typeof(CalleDto), "callejero.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Barrio, "Barrio", typeof(BarrioDtm), typeof(BarrioDto), "barrio.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Zona, "Zona", typeof(ZonaDtm), typeof(ZonaDto), "zona_1.svg", false, usaCg: false);

                    //Terceros
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Persona, "Personas", typeof(PersonaDtm), typeof(PersonaDto), "Persona.svg", esDeParametrizacion: false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Sociedad, "Sociedades", typeof(SociedadDtm), typeof(SociedadDto), "Sociedad.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Contacto, "Contacto", typeof(ContactoDtm), typeof(ContactoDto), "Sociedad.svg", false, usaCg: false, usaSeguridad: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.CentroGestor, "Centros gestores", typeof(CentroGestorDtm), typeof(CentroGestorDto), "CentroGestor.svg", esDeParametrizacion: true, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Interlocutor, "Interlocutor", typeof(InterlocutorDtm), typeof(InterlocutorDto), "Interlocutor.svg", esDeParametrizacion: false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Procurador, "Procurador", typeof(ProcuradorDtm), typeof(ProcuradorDto), "Procurador.svg", esDeParametrizacion: false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Abogado, "Abogado", typeof(AbogadoDtm), typeof(AbogadoDto), "Abogado.svg", esDeParametrizacion: false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Juzgado, "Juzgado", typeof(JuzgadoDtm), typeof(JuzgadoDto), "Juzgado.svg", esDeParametrizacion: false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Proveedor, "Proveedor", typeof(ProveedorDtm), typeof(ProveedorDto), "Proveedor.svg", esDeParametrizacion: false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Cliente, "Cliente", typeof(ClienteDtm), typeof(ClienteDto), "Cliente.svg", esDeParametrizacion: false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Trabajador, "Trabajador", typeof(TrabajadorDtm), typeof(TrabajadorDto), "Trabajador.svg", esDeParametrizacion: false, usaCg: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Banco, enumNegocio.Banco.Plural(), typeof(BancoDtm), typeof(BancoDto), enumNegocio.Banco.Icono(), esDeParametrizacion: false, usaCg: false);
                    
                    enumNegocio.Cliente.DefinirParametro(gestor.Contexto, enumParametrosDeCliente.CLI_TipoArchivador);
                    enumNegocio.Cliente.DefinirParametro(gestor.Contexto, enumParametrosDeCliente.CLI_PuestoDeTrabajo);
                    enumNegocio.Cliente.DefinirParametro(gestor.Contexto, enumParametrosDeCliente.CLI_CG_De_Cliente);

                    //Sistema docuemntal
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Archivador, enumNegocio.Archivador.Plural(), typeof(ArchivadorDtm), typeof(ArchivadorDto), "Archivadores.svg", false, usaCg: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Carpeta, "Carpetas", typeof(CarpetaDtm), typeof(CarpetaDto), "carpetas.svg", false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.CircuitoDoc, enumNegocio.CircuitoDoc.Plural(), typeof(CircuitoDocDtm), typeof(CircuitoDocDto), enumNegocio.CircuitoDoc.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);


                    //Módulo de gestión administrativa
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Registro, enumNegocio.Registro.Plural(), typeof(RegistroEsDtm), typeof(RegistroEsDto), "RegistrosEs.svg", false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Tarea, enumNegocio.Tarea.Plural(), typeof(TareaDtm), typeof(TareaDto), "Tareas.svg", false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Expediente, enumNegocio.Expediente.Plural(), typeof(ExpedienteDtm), typeof(ExpedienteDto), "Expedientes.svg", false, usaCg: true, usaSeguridad: true);

                    //Módulo de gestión Jurídico
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Pleito, enumNegocio.Pleito.Plural(), typeof(PleitoDtm), typeof(PleitoDto), "Pleito.svg", esDeParametrizacion: false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Contrato, enumNegocio.Contrato.Plural(), typeof(ContratoDtm), typeof(ContratoDto), enumNegocio.Contacto.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.LoteDeUnContrato, enumNegocio.LoteDeUnContrato.Plural(), typeof(LoteDeUnContratoDtm), typeof(LoteDeUnContratoDto), enumNegocio.LoteDeUnContrato.Icono() , esDeParametrizacion: false, usaCg: false);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.PlanificadorDeVenta, enumNegocio.PlanificadorDeVenta.Plural(), typeof(PlanificadorDeVentaDtm), typeof(PlanificadorDeVentaDto), enumNegocio.PlanificadorDeVenta.Icono(), esDeParametrizacion: false, usaCg: false);

                    //Módulo de MT
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Unitario, enumNegocio.Unitario.Plural(), typeof(UnitarioDtm), typeof(UnitarioDto), "Unitario.svg", esDeParametrizacion: false, usaCg: false, usaSeguridad: true);

                    //Módulo de gestión de ventas
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Presupuesto, enumNegocio.Presupuesto.Plural(), typeof(PresupuestoDtm), typeof(PresupuestoDto), enumNegocio.Presupuesto.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.ParteDeTrabajo, enumNegocio.ParteDeTrabajo.Plural(), typeof(ParteTrDtm), typeof(ParteTrDto), enumNegocio.ParteDeTrabajo.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.FacturaEmitida, enumNegocio.FacturaEmitida.Plural(), typeof(FacturaEmtDtm), typeof(FacturaEmtDto), enumNegocio.FacturaEmitida.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.PlanificacionDeVenta, enumNegocio.PlanificacionDeVenta.Plural(), typeof(PlanificacionDeVentaDtm), typeof(PlanificacionDeVentaDto), enumNegocio.PlanificacionDeVenta.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.RemesaFae, enumNegocio.RemesaFae.Plural(), typeof(RemesaFaeDtm), typeof(RemesaFaeDto), enumNegocio.RemesaFae.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);

                    //Módulo de gestión de gastos
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Pago, enumNegocio.Pago.Plural(), typeof(PagoDtm), typeof(PagoDto), enumNegocio.Pago.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.RemesaPag, enumNegocio.RemesaPag.Plural(), typeof(RemesaPagDtm), typeof(RemesaPagDto), enumNegocio.RemesaPag.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.FacturaRecibida, enumNegocio.FacturaRecibida.Plural(), typeof(FacturaRecDtm), typeof(FacturaRecDto), enumNegocio.FacturaRecibida.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);

                    //Módulo de gestión de logística
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Pedido, enumNegocio.Pedido.Plural(), typeof(PedidoDtm), typeof(PedidoDto), enumNegocio.Pedido.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);

                    //Módulo de gestión contable
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Preasiento, enumNegocio.Preasiento.Plural(), typeof(PreasientoDtm), typeof(PreasientoDto), enumNegocio.Preasiento.Icono(), esDeParametrizacion: false, usaCg: true, usaSeguridad: true);

                    //Módulo de guarderias
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.Infante, enumNegocio.Infante.Plural(), typeof(InfanteDtm), typeof(InfanteDto), enumNegocio.Infante.Icono(), esDeParametrizacion: false, usaCg: false, usaSeguridad: true);
                    GestorDeNegocios.CrearNegocioSiNoExiste(gestor, enumNegocio.CursoDeGuarderia, enumNegocio.CursoDeGuarderia.Plural(), typeof(CursoDeGuarderiaDtm), typeof(CursoDeGuarderiaDto), enumNegocio.CursoDeGuarderia.Icono(), esDeParametrizacion: false, usaCg: false, usaSeguridad: true);

                    gestor.Contexto.Commit(tran);
                }
                catch
                {
                    gestor.Contexto.Rollback(tran);
                    throw;
                }
                finally
                {
                    gestor.Contexto.DatosDeConexion.CreandoModelo = false; 
                    gestor.Contexto.CerrarTraza();
                }
            }

        }

        public static void DefinirAccionesDeRelacion(ContextoSe contexto)
        {
            contexto.DatosDeConexion.CreandoModelo = true;
            contexto.IniciarTraza(nameof(DefinirAccionesDeRelacion));
            var tran = contexto.IniciarTransaccion();
            try
            {
                GestorDeAccionesDeRelacion.CrearAccionDeRelacion(contexto
                , enumNegocio.Registro
                , enumNegocio.Tarea
                , AccionesDelRegistro.N_VincularArchivadores
                , enumMomentoDeRelacion.DR
                , null
                , 10
                , "Al vincular un elemento con un regitro se le vinculan los archivadores");

            GestorDeAccionesDeRelacion.CrearAccionDeRelacion(contexto
                , enumNegocio.Registro
                , enumNegocio.Tarea
                , AccionesDelRegistro.N_DesvincularArchivadores
                , enumMomentoDeRelacion.DB
                , null
                , 10
                , "Al desvincular un elemento con un regitro se le desvinculan los archivadores del registro");

            GestorDeAccionesDeRelacion.CrearAccionDeRelacion(contexto
                , enumNegocio.Registro
                , enumNegocio.Tarea
                , AccionesDelRegistro.N_ValidarDesvincularUnaTareaDeUnRegistro
                , enumMomentoDeRelacion.AB
                , null
                , 10
                , AccionesDelRegistro.D_ValidarDesvincularUnaTareaDeUnRegistro);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
            finally
            {
                contexto.CerrarTraza();
                contexto.DatosDeConexion.CreandoModelo = false;
            }

        }

    }
}

