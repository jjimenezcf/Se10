using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Expediente;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Gastos;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Utilidades;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Expediente
{
    public class GestorDeDatosJuridicos : GestorDeElementos<ContextoSe, DatosJuridicosDtm, DatosJuridicosDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrDatosJuridicos
        {
        }

        public class MapearDatosJuridicos : Profile
        {
            public MapearDatosJuridicos()
            {                
                CreateMap<DatosJuridicosDtm, DatosJuridicosDto>()
                .ForMember(dto => dto.Juzgado, dtm => dtm.MapFrom(dtm => dtm.Juzgado == null ? "" : dtm.Juzgado.Expresion))
                .ForMember(dto => dto.Abogado, dtm => dtm.MapFrom(dtm => dtm.Abogado == null ? "" : dtm.Abogado.Expresion))
                .ForMember(dto => dto.Procurador, dtm => dtm.MapFrom(dtm => dtm.Procurador == null ? "" : dtm.Procurador.Expresion))
                ;
                CreateMap<DatosJuridicosDto, DatosJuridicosDtm>()
                .ForMember(dtm => dtm.Procurador, dto => dto.Ignore())
                .ForMember(dtm => dtm.Abogado, dto => dto.Ignore())
                .ForMember(dtm => dtm.Juzgado, dto => dto.Ignore())
                ;
            }
        }

        public GestorDeDatosJuridicos(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeDatosJuridicos Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeDatosJuridicos(contexto, mapeador);
        }

        protected override IQueryable<DatosJuridicosDtm> AplicarJoins(IQueryable<DatosJuridicosDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta= base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Procurador);
            consulta = consulta.Include(x => x.Abogado);
            consulta = consulta.Include(x => x.Juzgado);
            return consulta;
        }

        protected override IQueryable<DatosJuridicosDtm> AplicarFiltros(IQueryable<DatosJuridicosDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return base.AplicarFiltros(consulta, filtros, parametros);
        }

        protected override void AntesDePersistir(DatosJuridicosDtm datos, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(datos, parametros);

            // 1. Validación de Obligatoriedad según Etapa 
            bool estaEnJuzgado = datos.AmpliacionDe<ExpedienteDtm>(Contexto).EstaEnLaEtapa(enumEtapasDeExpedientes.EXP_Etapa_En_Juzgado);

            if ((datos.NIG.IsNullOrEmpty() || datos.Procedimiento.IsNullOrEmpty()) && estaEnJuzgado)
            {
                GestorDeErrores.Emitir($"El {nameof(DatosJuridicosDtm.NIG)} y nº de {nameof(DatosJuridicosDtm.Procedimiento)} es obligatorio por estar el Auto presentado en el juzgado");
            }

            if (!ParametrosJuridicos.ExpresionRegularDeUnNIG.IsNullOrEmpty() && !datos.NIG.IsNullOrEmpty())
            {
                if (parametros.Insertando || datos.PropiedadCambiada<string>(nameof(DatosJuridicosDtm.NIG), parametros))
                {
                    if (!Regex.IsMatch(datos.NIG, ParametrosJuridicos.ExpresionRegularDeUnNIG))
                    {
                        GestorDeErrores.Emitir($"El formato del '{nameof(DatosJuridicosDtm.NIG)}'  no es válido, ha de cumplir con la expresión '{ParametrosJuridicos.ExpresionRegularDeUnNIG}'.");
                    }
                }
            }

            if (!ParametrosJuridicos.ExpresionRegularDeUnProcedimiento.IsNullOrEmpty() && !datos.Procedimiento.IsNullOrEmpty())
            {
                if (parametros.Insertando || datos.PropiedadCambiada<string>(nameof(DatosJuridicosDtm.Procedimiento), parametros))
                {
                    if (!Regex.IsMatch(datos.Procedimiento, ParametrosJuridicos.ExpresionRegularDeUnProcedimiento))
                    {
                        GestorDeErrores.Emitir($"El formato del '{nameof(DatosJuridicosDtm.Procedimiento)}' no es válido, ha de cumplir con la expresión '{ParametrosJuridicos.ExpresionRegularDeUnProcedimiento}'.");
                    }
                }
            }
        }

        protected override void AntesDeMapearElElemento(DatosJuridicosDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDeMapearElElemento(registro, parametros);
        }

        protected override void DespuesDeMapearElElemento(DatosJuridicosDtm registro, DatosJuridicosDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
        }

    }
}
