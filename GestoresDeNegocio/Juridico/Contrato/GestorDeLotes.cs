using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Gestor.Errores;
using Utilidades;
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;
using System;
using GestorDeElementos.Extensores;
using ServicioDeDatos.Ventas;

namespace GestoresDeNegocio.Juridico
{

    public class GestorDeLotesDeUnContrato : GestorDeElementos<ContextoSe, LoteDeUnContratoDtm, LoteDeUnContratoDto>
    {
        public override enumNegocio Negocio => enumNegocio.LoteDeUnContrato;

        public class MapearLotes : Profile
        {
            public MapearLotes()
            {
                CreateMap<LoteDeUnContratoDtm, LoteDeUnContratoDto>()
                .ForMember(dto => dto.Contrato, dtm => dtm.MapFrom(dtm => dtm.Contrato.Nombre));

                CreateMap<LoteDeUnContratoDto, LoteDeUnContratoDtm>()
                .ForMember(dtm => dtm.Contrato, dto => dto.Ignore())
                .ForMember(dtm => dtm.Activo, dto => dto.Ignore());
            }

        }
        public GestorDeLotesDeUnContrato(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeLotesDeUnContrato Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeLotesDeUnContrato(contexto, mapeador);
        }

        protected override IQueryable<LoteDeUnContratoDtm> AplicarJoins(IQueryable<LoteDeUnContratoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);

            if ((parametros.Peticion == enumPeticion.epLeerDatosParaElGrid || parametros.Peticion == enumPeticion.epExportar) && !parametros.FiltroPorId && filtros.Where(x => x.Clausula.Contains(nameof(LoteDeUnContratoDtm.IdContrato))).Count() == 0)
                GestorDeErrores.Emitir($"Se han solicitado obtener los lotes de un contrato y no se indica el contrato");

            if (parametros.HacerJoinCon(ltrLotesDeUnContrato.JoinConContratos))
                consulta = consulta.Include(p => p.Contrato).ThenInclude(c => c.Estado);
            return consulta;
        }

        protected override void AntesDePersistir(LoteDeUnContratoDtm lote, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(lote, parametros);
            var contrato = lote.Contrato(Contexto);
            if (contrato.EstaEnAlgunaDeLasEtapa(new List<enumEtapasDeContratos> { enumEtapasDeContratos.CTR_Etapa_Finalizacion,
                enumEtapasDeContratos.CTR_Etapa_Derogado,
                enumEtapasDeContratos.CTR_Etapa_Cancelado}))
                GestorDeErrores.Emitir($"$No se puede '{parametros.Operacion}' el lote por no estar el contrato '{contrato.Referencia}' en una etapa modificable");

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                var filtro = new ClausulaDeFiltrado(nameof(UnitariosDeUnLoteDtm.IdLote), enumCriteriosDeFiltrado.igual, lote.Id);
                if (Contexto.Existen<UnitariosDeUnLoteDtm>(new List<ClausulaDeFiltrado> { { filtro } }))
                    GestorDeErrores.Emitir($"el lote '{lote.Nombre}' tiene unitarios, no se puede borrar");
            }
            lote.VigenteDesde = lote.VigenteDesde != default ? ((DateTime)lote.VigenteDesde).Date : lote.VigenteDesde;
            lote.VigenteHasta = lote.VigenteHasta != default ? ((DateTime)lote.VigenteHasta).Date : lote.VigenteHasta;

            var fechas = contrato.FechasDelContrato(Contexto);

            if (lote.VigenteDesde != null && lote.VigenteDesde < fechas.Inicio )
                GestorDeErrores.Emitir($"el lote '{lote.Nombre}' no puede iniciarse '{lote.VigenteDesde.Fecha().ToShortDateString()}' antes que el contrato '{fechas.Inicio.ToShortDateString()}'");

            if (fechas.Fin != null && lote.VigenteHasta != null && lote.VigenteHasta > fechas.Fin )
                GestorDeErrores.Emitir($"el lote '{lote.Nombre}' no puede finalizar '{lote.VigenteHasta.Fecha().ToShortDateString()}' después de la finalización del contrato '{fechas.Fin.Fecha().ToShortDateString()}'");

        }

        protected override void ValidarPermisosDePersistencia(LoteDeUnContratoDtm lote, ParametrosDeNegocio parametros)
        {
            var contrato = Contexto.SeleccionarPorId<ContratoDtm>(lote.IdContrato);
            var estado = contrato.Estado(Contexto);
            if (estado.Cancelado || estado.Terminado)
                GestorDeErrores.Emitir($"un contrato terminado o cancelado no acepta variaciones en sus lotes");

            if (parametros.Insertando)
                return;

            lote.ValidarQueElLoteEsModificable(Contexto);
        }

        protected override void DespuesDeMapearElElemento(LoteDeUnContratoDtm registro, LoteDeUnContratoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            var contrato = (ContratoDtm)NegociosDeSe.CrearGestor(Contexto, enumNegocio.Contrato).LeerRegistroPorId(registro.IdContrato, false);
            if (registro.Activo)
                elemento.Activo = contrato.EstaVigente();
            
            elemento.Expresion = $"({contrato.Referencia}) {registro.Nombre}";
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(LoteDeUnContratoDtm lote, LoteDeUnContratoDto elemento, ParametrosDeNegocio parametros) 
        =>
        elemento.ModoDeAcceso = lote.ModoDeAccesoAlLote(Contexto);


    }
}
