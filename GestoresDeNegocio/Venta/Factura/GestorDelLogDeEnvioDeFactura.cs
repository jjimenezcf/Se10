using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDelLogDeEnvioDeFactura : GestorDeElementos<ContextoSe, LogDeEnvioDeFacturaDtm, LogDeEnvioDeFacturaDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;


        public class MapearIrpfsEmt : Profile
        {
            public MapearIrpfsEmt()
            {
                CreateMap<LogDeEnvioDeFacturaDtm, LogDeEnvioDeFacturaDto>();
                CreateMap<LogDeEnvioDeFacturaDto, LogDeEnvioDeFacturaDtm>();
            }
        }

        public GestorDelLogDeEnvioDeFactura(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDelLogDeEnvioDeFactura Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDelLogDeEnvioDeFactura(contexto, mapeador);
        }

        protected override IQueryable<LogDeEnvioDeFacturaDtm> AplicarJoins(IQueryable<LogDeEnvioDeFacturaDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Factura);
            return consulta;
        }

        public override LogDeEnvioDeFacturaDtm PersistirRegistro(LogDeEnvioDeFacturaDtm log, ParametrosDeNegocio parametros)
        {
            if (parametros.EsUnaPeticion)
                return null;
            parametros.ValidarPermisosDePersistencia = false;
            return base.PersistirRegistro(log, parametros);
        }

        protected override void AntesDePersistir(LogDeEnvioDeFacturaDtm log, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(log, parametros);
            if (parametros.Insertando) log.GeneradaEl = DateTime.Now;
        }

        protected override void DespuesDePersistir(LogDeEnvioDeFacturaDtm log, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(log, parametros);
            if (!parametros.Insertando)
                return;

            var tu = TrabajosDeFacturasEmt.SometerEnvioDeFacturaAeat(Contexto, log.IdFactura);
            var nuevoContexto = ContextoSe.Crear(Contexto);
            Task.Run(() => tu.EjecutarTrabajo(nuevoContexto));
        }

        protected override void DespuesDeMapearElElemento(LogDeEnvioDeFacturaDtm log, LogDeEnvioDeFacturaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(log, elemento, parametros);
            elemento.Factura = log.Factura?.Expresion;
        }

    }
}
