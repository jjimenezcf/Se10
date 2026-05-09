using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using Gestor.Errores;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;
using GestorDeElementos.Extensores;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Terceros;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeIrpfsEmt : GestorDeElementos<ContextoSe, IrpfEmtDtm, IrpfEmtDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrIrpfsEmt
        {
        }

        public class MapearIrpfsEmt : Profile
        {
            public MapearIrpfsEmt()
            {
                CreateMap<IrpfEmtDtm, IrpfEmtDto>();
                CreateMap<IrpfEmtDto, IrpfEmtDtm>();
            }
        }

        public GestorDeIrpfsEmt(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeIrpfsEmt Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeIrpfsEmt(contexto, mapeador);
        }

        protected override IQueryable<IrpfEmtDtm> AplicarJoins(IQueryable<IrpfEmtDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.TipoIrpf);
            return consulta;
        }

        public override IrpfEmtDtm PersistirRegistro(IrpfEmtDtm irpfEmt, ParametrosDeNegocio parametros)
        {
            if (parametros.Insertando && (irpfEmt.BiSujeta is null || irpfEmt.BiSujeta == 0) && (irpfEmt.Irpf is null || irpfEmt.Irpf == 0))
                return irpfEmt;
            return base.PersistirRegistro(irpfEmt, parametros);
        }

        protected override void AntesDePersistir(IrpfEmtDtm irpfEmt, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(irpfEmt, parametros);

            var factura = irpfEmt.AmpliacionDe<FacturaEmtDtm>(Contexto);

            if (factura.EsRectificativa && factura.ClaseRectificativa == enumClaseDeRectificativa.OR && !parametros.Insertando)
            {
                GestorDeErrores.Emitir($"Una factura rectificativa '{enumClaseDeRectificativa.OR.Descripcion()}' no se le puede variar el Irpf");
            }
            
            if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura) )
                GestorDeErrores.Emitir($"La factura '{factura.Referencia}' es firme, no se puede variar el irpf");


            if (parametros.Modificando && (irpfEmt.BiSujeta is null || irpfEmt.BiSujeta == 0))
            {
                if (factura.EsRectificativa )
                    GestorDeErrores.Emitir($"El irpf de la factura rectificativa '{factura.Referencia}', no se puede eliminar");

                parametros.Eliminar();
                return;
            }

            var puedellevarIrpf = (ApiDeTerceros.TipoDeTerceroEsp(factura.Sociedad(Contexto).NIF) == enumTipoTercero.Autonomo &&
                ApiDeTerceros.TipoDeTerceroEsp(factura.Cliente(Contexto).NIF(Contexto)) == enumTipoTercero.Empresa) ||
                (ApiDeTerceros.TipoDeTerceroEsp(factura.Sociedad(Contexto).NIF) == enumTipoTercero.Autonomo &&
                ApiDeTerceros.TipoDeTerceroEsp(factura.Cliente(Contexto).NIF(Contexto)) == enumTipoTercero.Autonomo);


            if (!puedellevarIrpf)
                GestorDeErrores.Emitir($"La factura no puede llevar irpf. Lleva irpf si la sociedad facturadora es '{enumTipoTercero.Autonomo}' y la facturada de una '{enumTipoTercero.Empresa}' o '{enumTipoTercero.Autonomo}' ");

            if (factura.EsRectificativa && factura.ClaseRectificativa != enumClaseDeRectificativa.OR)
                GestorDeErrores.Emitir($"La factura '{factura.Referencia}' es rectificativa de clase '{factura.ClaseRectificativa.Descripcion()}', no puede llevar irpf");

            if (!factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura) && parametros.Parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDePersistencia, true))
                GestorDeErrores.Emitir($"La factura '{factura.Referencia}' es firme, no se puede variar el irpf");

            if (irpfEmt.BiSujeta is null || irpfEmt.BiSujeta == 0 || irpfEmt.Irpf is null || irpfEmt.Irpf == 0)
                GestorDeErrores.Emitir($"Para indicar el irpf de la factura '{factura.Referencia}' ha de indicar una BI sujeta y el porcentaje a aplicar");

            factura.ValidarIrpf(Contexto, (decimal) irpfEmt.BiSujeta);

        }

        protected override void DespuesDePersistir(IrpfEmtDtm irpfEmt, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(irpfEmt, parametros);
            var factura = irpfEmt.AmpliacionDe<FacturaEmtDtm>(Contexto);
            factura.VaciarCachesDeImportes();
        }

    }
}
