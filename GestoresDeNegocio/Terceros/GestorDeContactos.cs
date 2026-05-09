using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Elemento;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeContactos : GestorDeElementos<ContextoSe, ContactoDtm, ContactoDto>
    {
        public override enumNegocio Negocio => enumNegocio.Contacto;

        public class MapearContactos : Profile
        {
            public MapearContactos()
            {
                CreateMap<ContactoDtm, ContactoDto>()
                .ForMember(dto => dto.Sociedad, dtm => dtm.MapFrom(dtm => dtm.Sociedad != null ? dtm.Sociedad.Expresion : ""))
                .ForMember(dto => dto.IdElemento, dtm => dtm.MapFrom(dtm => dtm.IdSociedad));

                CreateMap<ContactoDto, ContactoDtm>()
                .ForMember(dtm => dtm.Expresion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Sociedad, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdSociedad, dto => dto.MapFrom(x => x.IdElemento));
            }
        }

        public GestorDeContactos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeContactos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeContactos(contexto, mapeador);
        }

        protected override IQueryable<ContactoDtm> AplicarJoins(IQueryable<ContactoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Sociedad);
            return consulta;
        }


        protected override void AntesDeMapearElRegistroParaInsertar(ContactoDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);
            if (!opciones.Parametros.ContainsKey(nameof(ContactoDto.CrearInterlocutor)))
                opciones.Parametros.Add(nameof(ContactoDto.CrearInterlocutor), elemento.CrearInterlocutor);
        }

        protected override void AntesDeMapearElRegistroParaModificar(ContactoDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaModificar(elemento, opciones);
            if (!opciones.Parametros.ContainsKey(nameof(ContactoDto.CrearInterlocutor)))
                opciones.Parametros.Add(nameof(ContactoDto.CrearInterlocutor), elemento.CrearInterlocutor);
        }

        protected override void AntesDePersistir(ContactoDtm contacto, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(contacto, parametros);

        }

        protected override void DespuesDePersistir(ContactoDtm contacto, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(contacto, parametros);

            if (parametros.Parametros.LeerValor(nameof(ContactoDto.CrearInterlocutor), false))
            {
                contacto.CrearInterlocutor(Contexto);
                return;
            }

            if (parametros.Modificando)
            {
                if (parametros.EstaDandoDeAlta(contacto))
                    return;
                contacto.SincronizarConInterlocutor(Contexto, (ContactoDtm)parametros.registroEnBd);
                var interlocutor = enumNegocio.Interlocutor.SeleccionarReferenciados<InterlocutorDtm>(Contexto, nameof(InterlocutorDtm.IdContacto), contacto.Id);

                if (interlocutor.Count == 0 && contacto.EsInterlocutor)
                    contacto.CrearInterlocutor(Contexto);
                else if (interlocutor.Count == 1 && !contacto.EsInterlocutor)
                    contacto.DarDeBajaElInterlocutor<ContactoDtm>(Contexto);
            }

        }

        protected override void DespuesDeMapearElElemento(ContactoDtm contacto, ContactoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(contacto, elemento, parametros);
            elemento.Sociedad = contacto.Sociedad(Contexto).Expresion;
            elemento.Expresion = contacto.Expresion(Contexto);

            var a = enumNegocio.Interlocutor.SeleccionarReferenciados<InterlocutorDtm>(Contexto, nameof(InterlocutorDtm.IdContacto), contacto.Id);
            elemento.IdInterlocutor = a.Count > 0 ? a[0].Id : null;
        }

        protected override void AlDarDeBaja(ContactoDtm contacto, ParametrosDeNegocio parametros)
        {
            base.AlDarDeBaja(contacto, parametros);
            if (contacto.EsInterlocutor) contacto.DarDeBajaElInterlocutor<ContactoDtm>(Contexto);
        }

        protected override IQueryable<ContactoDtm> AplicarFiltros(IQueryable<ContactoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(ContactoDto.IdElemento), System.StringComparison.CurrentCultureIgnoreCase))
                {
                    parametros.IncluirBajas = true;
                    filtro.Clausula = nameof(ContactoDtm.IdSociedad);
                }
            }
            return base.AplicarFiltros(consulta, filtros, parametros);
        }

        protected override void ValidarPermisosDePersistencia(ContactoDtm registro, ParametrosDeNegocio parametros)
        {
            var sociedad = GestorDeSociedades.LeerRegistroPorId(Contexto, registro.IdSociedad);
            ApiDePermisos.ValidarPermisosDePersistencia(Contexto, enumNegocio.Sociedad, typeof(RegistroDtm), parametros, sociedad);
        }

    }
}
