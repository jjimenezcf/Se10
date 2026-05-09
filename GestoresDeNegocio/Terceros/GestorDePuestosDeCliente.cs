using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDePuestosDeCliente : GestorDeElementos<ContextoSe, PuestoDeClienteDtm, PuestoDeClienteDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrPuestosDeCliente
        {
        }

        public class MapearPuestosDeCliente : Profile
        {
            public MapearPuestosDeCliente()
            {
                CreateMap<PuestoDeClienteDtm, PuestoDeClienteDto>()
                .ForMember(dto => dto.Puesto, x => x.MapFrom(dtm => dtm.Puesto.Expresion))
                .ForMember(dto => dto.Sociedad, x => x.Ignore());
                CreateMap<PuestoDeClienteDto, PuestoDeClienteDtm>()
                .ForMember(dtm => dtm.Puesto, x => x.Ignore());
            }
        }

        public GestorDePuestosDeCliente(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<PuestoDeClienteDtm> AplicarJoins(IQueryable<PuestoDeClienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Puesto);
            return consulta;
        }

        public static GestorDePuestosDeCliente Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePuestosDeCliente(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(PuestoDeClienteDto elemento, PuestoDeClienteDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);
            if (opciones.Operacion == enumTipoOperacion.Insertar)
            {
                var cgCliente = ExtensionCentrosGestores.Cfg_CG_De_ClienteWeb(Contexto, elemento.IdSociedad); 
                var cliente = Contexto.SeleccionarPorId<ClienteDtm>(elemento.IdElemento);
                var puesto = cgCliente.PuestoDeTrabajo(Contexto, $"PT para el cliente: {cliente.Referencia(Contexto)} ({elemento.Puesto})", $"Puesto de trabajo para los usuarios del cliente web {cliente.Referencia(Contexto)}", errorSiExiste: true);
                registro.IdPuesto = puesto.Id;
            }
        }

        protected override void AntesDePersistir(PuestoDeClienteDtm puestoDeCliente, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(puestoDeCliente, parametros);

            if (!Contexto.DatosDeConexion.EsAdministrador) GestorDeErrores.Emitir($"´Para realizar gestión de puestos de trabajo de un cliente necesita ser administrador");
        }

        protected override void DespuesDePersistir(PuestoDeClienteDtm puestoDeCliente, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(puestoDeCliente, parametros);

            if (parametros.Insertando)
                ((ClienteDtm)puestoDeCliente.DetalleDe(Contexto))
                    .CrearTraza(Contexto, "Se ha añadido un puesto de trabajo para el cliente", $"El Puesto '{puestoDeCliente.Puesto(Contexto).Expresion}' ha sido añadido por el usuario '{Contexto.DatosDeConexion.Login}'");

            if (parametros.Eliminando)
            {
                var Puesto = ((PuestoDeClienteDtm)(parametros.registroEnBd)).Puesto(Contexto);

                ((ClienteDtm)puestoDeCliente.DetalleDe(Contexto)).
                    CrearTraza(Contexto, "Se ha eliminado un puesto de trabajo al cliente", $"El Puesto '{Puesto.Expresion}' ha sido eliminado por el usuario '{Contexto.DatosDeConexion.Login}'");

                Contexto.EliminarTodos<PuestosDeUnUsuarioDtm>(new Dictionary<string, object> { { nameof(PuestosDeUnUsuarioDtm.IdPuesto), Puesto.Id } });
                Contexto.EliminarTodos<RolesDeUnPuestoDtm>(new Dictionary<string, object> { { nameof(RolesDeUnPuestoDtm.IdPuesto), Puesto.Id } });
                Contexto.EliminarTodos<PermisosDirectosDtm>(new Dictionary<string, object> { { nameof(PermisosDirectosDtm.IdPuesto), Puesto.Id } });
                Contexto.EliminarPorId<PuestoDtm>(Puesto.Id);
            }

            ServicioDeCaches.EliminarElemento(CacheDe.Ter_PuestoDeCliente, puestoDeCliente.IdPuesto.ToString());
        }
        protected override void DespuesDeMapearElElemento(PuestoDeClienteDtm registro, PuestoDeClienteDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            var puesto = Contexto.SeleccionarPorId<PuestoDtm>(registro.IdPuesto);
            var cg = Contexto.SeleccionarPorId<CentroGestorDtm>(puesto.IdCg, aplicarJoin: true);
            elemento.Sociedad = cg.Sociedad.Expresion;
        }
    }
}
