using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Presupuesto;
using ModeloDeDto.Presupuesto;
using iText.IO.Util;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Ventas;

namespace GestoresDeNegocio.Presupuesto
{
    public class GestorDePptsDeVenta : GestorDeElementos<ContextoSe, PptDeVentaDtm, PptDeVentaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrPptsDeVenta
        {
        }

        public class MapearPptsDeVenta : Profile
        {
            public MapearPptsDeVenta()
            {
                CreateMap<PptDeVentaDtm, PptDeVentaDto>();
                CreateMap<PptDeVentaDto, PptDeVentaDtm>();
            }
        }

        public GestorDePptsDeVenta(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDePptsDeVenta Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePptsDeVenta(contexto, mapeador);
        }

        protected override void AntesDePersistir(PptDeVentaDtm propuestos, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(propuestos, parametros);

            if (propuestos.IdIvaR.Entero() == 0)
            {
                propuestos.Iva = null;
                propuestos.IdIvaR = null; 
                propuestos.Iva = null;
            }
            if (propuestos.IdIvaR.Entero() > 0)
            {
                var ivaR = Contexto.SeleccionarPorId<IvaRepercutidoDtm>(propuestos.IdIvaR.Entero());
                propuestos.Iva = ivaR.Porcentaje;
            }
        }

        protected override void DespuesDeMapearElElemento(PptDeVentaDtm registro, PptDeVentaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);

            var ppt = (PresupuestoDtm) registro.AmpliacionDe(Contexto);
            if (!ppt.Etapas().Contains(enumEtapasDePpts.PPT_Etapa_Elaboracion))
            {
                elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
            }
        }

    }
}
