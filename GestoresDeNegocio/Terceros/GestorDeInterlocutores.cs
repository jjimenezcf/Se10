
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using static Utilidades.Ampliaciones;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeInterlocutores : GestorDeElementos<ContextoSe, InterlocutorDtm, InterlocutorDto>
    {
        public override enumNegocio Negocio => enumNegocio.Interlocutor;

        public class MapearInterlocutores : Profile
        {
            public MapearInterlocutores()
            {
                CreateMap<InterlocutorDtm, InterlocutorDto>();
                CreateMap<InterlocutorDto, InterlocutorDtm>()
                .ForMember(dtm => dtm.Expresion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Nombre, dto => dto.MapFrom(x => x.Expresion));
            }
        }

        public GestorDeInterlocutores(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeInterlocutores Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeInterlocutores(contexto, mapeador); ;
        }

        protected override IQueryable<InterlocutorDtm> AplicarJoins(IQueryable<InterlocutorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Persona);
            consulta = consulta.Include(e => e.Sociedad);
            return consulta;
        }

        protected override IQueryable<InterlocutorDtm> AplicarFiltros(IQueryable<InterlocutorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorInterlocutoresNoVinculados(Contexto, filtros);
            consulta = consulta.FiltrarParaExpedientes(Contexto, filtros);
            consulta = consulta.FiltrarParaInfantes(Contexto, filtros);
            consulta = consulta.FiltrarPorNIFDeSociedad(filtros);
            consulta = consulta.FiltrarPorPersona(filtros);
            consulta = consulta.FiltrarPorContactosDelCliente(Contexto, filtros);
            return consulta;
        }

        protected override IQueryable<InterlocutorDtm> AplicarSeguridad(IQueryable<InterlocutorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador) consulta = FiltrarPorSeguridad.DeNegocio(Contexto, Negocio, consulta);
            return consulta;
        }

        protected override void AntesDePersistir(InterlocutorDtm interlocutor, ParametrosDeNegocio parametros)
        {
            ActualizarNombre(interlocutor);
            base.AntesDePersistir(interlocutor, parametros);
            if (parametros.Insertando)
            {
                if (interlocutor.EsSociedad)
                {
                    var sociedad = interlocutor.Sociedad(Contexto);
                    if (interlocutor.Telefono is null && sociedad.Telefono is not null) interlocutor.Telefono = sociedad.Telefono;
                    if (interlocutor.eMail is null && sociedad.eMail is not null) interlocutor.eMail = sociedad.eMail;
                }
                if (interlocutor.EsPersona)
                {
                    var sociedad = interlocutor.Persona(Contexto);
                    if (interlocutor.Telefono is null && sociedad.Telefono is not null) interlocutor.Telefono = sociedad.Telefono;
                    if (interlocutor.eMail is null && sociedad.eMail is not null) interlocutor.eMail = sociedad.eMail;
                }
            }
        }

        protected override void DespuesDePersistir(InterlocutorDtm interlocutor, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(interlocutor, parametros);

            if (parametros.Modificando)
            {
                interlocutor.TrazarModificaciones(Contexto, (InterlocutorDtm)parametros.registroEnBd);
                interlocutor.SincronizarConTerceros(Contexto, (InterlocutorDtm)parametros.registroEnBd);
            }
        }

        protected override void EliminarCaches(InterlocutorDtm interlocutor, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(interlocutor, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
            ServicioDeCaches.EliminarElemento(CacheDe.Int_Proveedor, interlocutor.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Int_Trabajador, interlocutor.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Int_Cliente, interlocutor.Id.ToString());
        }

        protected override void AlDarDeAlta(InterlocutorDtm interlocutor, ParametrosDeNegocio parametros)
        {
            base.AlDarDeAlta(interlocutor, parametros);
            if (interlocutor.EsContacto && interlocutor.Contacto(Contexto).Baja)
                GestorDeErrores.Emitir($"Debe dar de alta el contacto '{interlocutor.Contacto(Contexto).Expresion(Contexto)}' antes de dar de alta el interlocutor");
            if (interlocutor.EsPersona && interlocutor.Persona(Contexto).Baja)
                GestorDeErrores.Emitir($"Debe dar de alta la persona '{interlocutor.Persona(Contexto).Expresion}' antes de dar de alta el interlocutor");
            if (interlocutor.EsSociedad && interlocutor.Sociedad(Contexto).Baja)
                GestorDeErrores.Emitir($"Debe dar de alta la '{interlocutor.Sociedad(Contexto).Expresion}' antes de dar de alta el interlocutor");
        }

        protected override void AlDarDeBaja(InterlocutorDtm interlocutor, ParametrosDeNegocio parametros)
        {
            base.AlDarDeBaja(interlocutor, parametros);
            var cliente = Contexto.SeleccionarPorFk<ClienteDtm>(nameof(ClienteDtm.IdInterlocutor), interlocutor.Id, errorSiNoHay: false);
            if (cliente != null && cliente.Baja == false)
            {
                cliente.Baja = true;
                cliente.Modificar(Contexto);
            }
            var proveedor = Contexto.SeleccionarPorFk<ProveedorDtm>(nameof(ProveedorDtm.IdInterlocutor), interlocutor.Id, errorSiNoHay: false);
            if (proveedor != null && proveedor.Baja == false)
            {
                proveedor.Baja = true;
                proveedor.Modificar(Contexto);
            }
            var trabajador = Contexto.SeleccionarPorFk<TrabajadorDtm>(nameof(TrabajadorDtm.IdInterlocutor), interlocutor.Id, errorSiNoHay: false);
            if (trabajador != null && trabajador.Baja == false)
            {
                trabajador.Baja = true;
                trabajador.Modificar(Contexto);
            }
            var abogado = Contexto.SeleccionarPorFk<AbogadoDtm>(nameof(AbogadoDtm.IdInterlocutor), interlocutor.Id, errorSiNoHay: false);
            if (abogado != null && abogado.Baja == false)
            {
                abogado.Baja = true;
                abogado.Modificar(Contexto);
            }
            var procurador = Contexto.SeleccionarPorFk<ProcuradorDtm>(nameof(ProcuradorDtm.IdInterlocutor), interlocutor.Id, errorSiNoHay: false);
            if (procurador != null && procurador.Baja == false)
            {
                procurador.Baja = true;
                procurador.Modificar(Contexto);
            }
        }

        protected override void DespuesDeMapearElElemento(InterlocutorDtm interlocutor, InterlocutorDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(interlocutor, elemento, parametros);
            elemento.NIF = interlocutor.NIF(Contexto);
            if (parametros.Parametros.LeerValor(ltrParametrosNeg.LeerDireccionDeContacto, false))
            {
                elemento.DireccionDeContacto = interlocutor.DireccionDto(Contexto, enumCalificadorDireccion.contacto, errorSiNoHay: false)?.Expresion ?? ltrDireccion.NoIndicada;
            }
        }

        private void ActualizarNombre(InterlocutorDtm interlocutor)
        {
            if (interlocutor.EsContacto)
                interlocutor.Nombre = Contexto.SeleccionarPorId<ContactoDtm>((int)interlocutor.IdContacto, aplicarJoin: true).Expresion;
            else if (interlocutor.EsSociedad)
                interlocutor.Nombre = Contexto.SeleccionarPorId<SociedadDtm>((int)interlocutor.IdSociedad).Expresion;
            else if (interlocutor.EsPersona)
                interlocutor.Nombre = Contexto.SeleccionarPorId<PersonaDtm>((int)interlocutor.IdPersona).Expresion;

        }

        public static CuentaDeAcreedorDto LeerCuentaDeIngreso(ContextoSe contexto, int idInterlocutor)
        {
            var inter = contexto.SeleccionarPorId<InterlocutorDtm>(idInterlocutor);
            var cuenta = new CuentaDeAcreedorDto();
            var proveedor = inter.Proveedor(contexto, crearProveedor: false);
            if (proveedor is not null)
            {
                var cuentaDeProveedor = proveedor.CuentaDeProveedor(contexto, enumClaseDeCuentaBancaria.Ingreso);
                if (cuentaDeProveedor is not null)
                {
                    cuenta.Alias = cuentaDeProveedor.Alias;
                    cuenta.EsDePoveedor = true;
                    cuenta.IdProveedor = proveedor.Id;
                    cuenta.Proveedor = proveedor.Expresion;
                    return cuenta.MapearCuentaDeTercero(contexto, cuentaDeProveedor);
                }
                else return null;
            }

            var trabajador = inter.Trabajador(contexto);
            if (trabajador is not null)
            {
                var cuentaDeTrabajador = trabajador.CuentaDeTrabajador(contexto, enumClaseDeCuentaBancaria.Ingreso);
                if (cuentaDeTrabajador is not null)
                {
                    cuenta.Alias = cuentaDeTrabajador.Alias;
                    cuenta.EsDeTrabajador = true;
                    cuenta.IdTrabajador = trabajador.Id;
                    cuenta.Trabajador = trabajador.Expresion;
                    return cuenta.MapearCuentaDeTercero(contexto, cuentaDeTrabajador);
                }
                else return null;
            }

            var cuentaDeInterlocutor = inter.CuentaDeInterlocutor(contexto, enumClaseDeCuentaBancaria.Ingreso, errorSiNoHay: false);
            if (cuentaDeInterlocutor is not null)
            {
                cuenta.Alias = cuentaDeInterlocutor.Alias;
                cuenta.EsDeAcreedor = true;
                return cuenta.MapearCuentaDeTercero(contexto, cuentaDeInterlocutor);
            }
            return null;
        }

    }
}
