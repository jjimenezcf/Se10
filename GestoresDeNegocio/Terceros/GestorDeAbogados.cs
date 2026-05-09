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
    public class GestorDeAbogados : GestorDeElementos<ContextoSe, AbogadoDtm, AbogadoDto>
    {
        public override enumNegocio Negocio => enumNegocio.Abogado;

        public class MapearAbogados : Profile
        {
            public MapearAbogados()
            {
                CreateMap<AbogadoDtm, AbogadoDto>();
                CreateMap<AbogadoDto, AbogadoDtm>()
                .ForMember(dtm => dtm.Expresion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Nombre, dto => dto.MapFrom(x => x.Expresion));
            }
        }

        public GestorDeAbogados(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeAbogados Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAbogados(contexto, mapeador); ;
        }

        protected override IQueryable<AbogadoDtm> AplicarJoins(IQueryable<AbogadoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Interlocutor);
            return consulta;
        }

        protected override IQueryable<AbogadoDtm> AplicarFiltros(IQueryable<AbogadoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(AbogadoDto.IdSociedad), StringComparison.CurrentCultureIgnoreCase))
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdSociedad == filtro.Valor.Entero() && x.Interlocutor.IdContacto == null);

                if (filtro.Clausula.Equals(nameof(AbogadoDto.IdPersona), StringComparison.CurrentCultureIgnoreCase))
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdPersona == filtro.Valor.Entero());

                if (filtro.Clausula.Equals(nameof(AbogadoDto.Expresion), StringComparison.CurrentCultureIgnoreCase))
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
                    var vinculos = NegociosDeSe.ToEnumerado(filtro.IdNegocio).Abogados(Contexto).Where(x => x.idElemento1 == filtro.IdElemento);
                    consulta = consulta.ElementosNoVinculadosDeLaMismaSociedad(Contexto, filtro, vinculos);
                }
            }
            return consulta;
        }

        protected override void AntesDePersistir(AbogadoDtm abogado, ParametrosDeNegocio parametros)
        {
            if (!parametros.Insertando) abogado.Nombre = ((AbogadoDtm)parametros.registroEnBd).Nombre;
            base.AntesDePersistir(abogado, parametros);
        }

        protected override void AlDarDeAlta(AbogadoDtm abogado, ParametrosDeNegocio parametros)
        {
            base.AlDarDeAlta(abogado, parametros);

            if (abogado.Interlocutor(Contexto).Baja)
                GestorDeErrores.Emitir($"Debe dar de alta el interlocutor '{abogado.Interlocutor(Contexto).Expresion(Contexto)}' antes de dar de alta el abogado");
        }

        public static List<AbogadoDtm> CrearAbogados(ContextoSe contexto, List<int> idsDeInter)
        {
            var result = new List<AbogadoDtm>();
            foreach (var idInter in idsDeInter)
            {
                result.Add(CrearAbogado(contexto, idInter));
            }
            return result;
        }

        public static AbogadoDtm CrearAbogado(ContextoSe contexto, string nif)
        {
            var inter = ExtensorDeInterlocutores.CrearInterlocutor(contexto, nif);
            return CrearAbogado(contexto, inter);
        }

        public static AbogadoDtm CrearAbogado(ContextoSe contexto, int idInter) => CrearAbogado(contexto, contexto.SeleccionarPorId<InterlocutorDtm>(idInter));

        public static AbogadoDtm CrearAbogado(ContextoSe contexto, InterlocutorDtm inter)
        {

            var abogado = contexto.SeleccionarPorFk<AbogadoDtm>(nameof(AbogadoDtm.IdInterlocutor), inter.Id, errorSiNoHay: false);

            if (abogado != null)
                return abogado;

            if (inter.Baja)
                GestorDeErrores.Emitir($"No se puede dar de alta un abogado por estar de baja el interlocutor");

            if (abogado == null)
            {
                abogado = new AbogadoDtm();
                abogado.IdInterlocutor = inter.Id;
                abogado.Nombre = inter.Expresion;
                abogado.Telefono = inter.Telefono;
                abogado.eMail = inter.eMail;
                abogado = abogado.Insertar(contexto);
            }

            return abogado;
        }
    }

    public static class ExtensorDeAbogados
    {
        public static IQueryable<VinculoDtm> Abogados(this enumNegocio negocio, ContextoSe contexto)
        {
            throw new Exception($"Se debe indicar como obtener los abogados vinculados al negocio: {negocio}");
        }
    }
}
