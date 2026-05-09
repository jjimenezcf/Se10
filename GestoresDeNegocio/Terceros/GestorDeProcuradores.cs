using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using System;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using ServicioDeDatos.Elemento;
using ModeloDeDto;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeProcuradores : GestorDeElementos<ContextoSe, ProcuradorDtm, ProcuradorDto>
    {
        public override enumNegocio Negocio => enumNegocio.Procurador;

        public class MapearProcuradores : Profile
        {
            public MapearProcuradores()
            {
                CreateMap<ProcuradorDtm, ProcuradorDto>();
                CreateMap<ProcuradorDto, ProcuradorDtm>()
                .ForMember(dtm => dtm.Expresion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Nombre, dto => dto.MapFrom(x => x.Expresion));
            }
        }

        public GestorDeProcuradores(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeProcuradores Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeProcuradores(contexto, mapeador); ;
        }

        protected override IQueryable<ProcuradorDtm> AplicarJoins(IQueryable<ProcuradorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Interlocutor);
            return consulta;
        }

        protected override IQueryable<ProcuradorDtm> AplicarFiltros(IQueryable<ProcuradorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(ProcuradorDto.IdSociedad), StringComparison.CurrentCultureIgnoreCase))
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdSociedad == filtro.Valor.Entero() && x.Interlocutor.IdContacto == null);

                if (filtro.Clausula.Equals(nameof(ProcuradorDto.IdPersona), StringComparison.CurrentCultureIgnoreCase))
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdPersona == filtro.Valor.Entero());

                if (filtro.Clausula.Equals(nameof(ProcuradorDto.Expresion), StringComparison.CurrentCultureIgnoreCase))
                {
                    if (ApiDeTerceros.ValidarNif(filtro.Valor).IsNullOrEmpty())
                        consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.NIF.ToLower() == filtro.Valor);
                    else
                    if (ApiDeTerceros.CifValido(filtro.Valor))
                        consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Sociedad.NIF.ToLower() == filtro.Valor || x.Interlocutor.Contacto.Sociedad.NIF.ToLower() == filtro.Valor);
                    else if (filtro.Valor.EsEntero())
                        consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.eMail.Contains(filtro.Valor)
                                                                  || x.Interlocutor.Sociedad.eMail.Contains(filtro.Valor)
                                                                  || x.Interlocutor.Contacto.eMail.Contains(filtro.Valor)
                                                                  || x.Interlocutor.Persona.Telefono.Contains(filtro.Valor)
                                                                  || x.Interlocutor.Sociedad.Telefono.Contains(filtro.Valor)
                                                                  || x.Interlocutor.Contacto.Telefono.Contains(filtro.Valor));
                    else if (filtro.Valor.Contains("@"))
                        consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.eMail.Contains(filtro.Valor)
                                                                  || x.Interlocutor.Sociedad.eMail.Contains(filtro.Valor)
                                                                  || x.Interlocutor.Contacto.eMail.Contains(filtro.Valor));
                    else
                        consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.Apellidos.Contains(filtro.Valor)
                                                                       || x.Interlocutor.Sociedad.Nombre.Contains(filtro.Valor)
                                                                       || x.Interlocutor.Contacto.Nombre.Contains(filtro.Valor));
                }
                if (filtro.Clausula.Equals(ltrFiltros.VincularCon, StringComparison.CurrentCultureIgnoreCase))
                {
                    var vinculos = NegociosDeSe.ToEnumerado(filtro.IdNegocio).Procuradores(Contexto).Where(x => x.idElemento1 == filtro.IdElemento);
                    consulta = consulta.ElementosNoVinculadosDeLaMismaSociedad(Contexto, filtro, vinculos);
                }
            }
            return consulta;
        }

        protected override void AntesDePersistir(ProcuradorDtm procurador, ParametrosDeNegocio parametros)
        {
            if (!parametros.Insertando) procurador.Nombre = ((ProcuradorDtm)parametros.registroEnBd).Nombre;
            base.AntesDePersistir(procurador, parametros);
            if (parametros.Modificando)
            {
                var inter = Contexto.SeleccionarPorId<InterlocutorDtm>(procurador.IdInterlocutor);
                procurador.Nombre = inter.Expresion;
            }
        }

        protected override void AlDarDeAlta(ProcuradorDtm procurador, ParametrosDeNegocio parametros)
        {
            base.AlDarDeAlta(procurador, parametros);

            if (procurador.Interlocutor(Contexto).Baja)
                GestorDeErrores.Emitir($"Debe dar de alta el interlocutor '{procurador.Interlocutor(Contexto).Expresion(Contexto)}' antes de dar de alta el procurador");
        }

        public static List<ProcuradorDtm> CrearProcuradores(ContextoSe contexto, List<int> idsDeInter)
        {
            var result = new List<ProcuradorDtm>();
            foreach (var idInter in idsDeInter)
            {
                result.Add(CrearProcurador(contexto, idInter));
            }
            return result;
        }

        public static ProcuradorDtm CrearProcurador(ContextoSe contexto, string nif)
        {
            var inter = ExtensorDeInterlocutores.CrearInterlocutor(contexto, nif);
            return CrearProcurador(contexto, inter);
        }

        public static ProcuradorDtm CrearProcurador(ContextoSe contexto, int idInter) => CrearProcurador(contexto, contexto.SeleccionarPorId<InterlocutorDtm>(idInter));

        public static ProcuradorDtm CrearProcurador(ContextoSe contexto, InterlocutorDtm inter)
        {
            var procurador = contexto.SeleccionarPorFk<ProcuradorDtm>(nameof(ProcuradorDtm.IdInterlocutor), inter.Id, errorSiNoHay: false);

            if (procurador != null)
                return procurador;

            if (inter.Baja)
                GestorDeErrores.Emitir($"No se puede dar de alta un procurador por estar de baja el interlocutor");

            if (procurador == null)
            {
                procurador = new ProcuradorDtm();
                procurador.IdInterlocutor = inter.Id;
                procurador.Nombre = inter.Expresion;
                procurador.Telefono = inter.Telefono;
                procurador.eMail = inter.eMail;
                procurador = procurador.Insertar(contexto);
            }

            return procurador;
        }
    }

    public static class ExtensorDeProcuradores
    {
        public static IQueryable<VinculoDtm> Procuradores(this enumNegocio negocio, ContextoSe contexto)
        {
            throw new Exception($"Se debe indicar como obtener los procuradores vinculados al negocio: {negocio}");
        }
    }
}
