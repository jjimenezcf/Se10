using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Ventas;
using ServicioDeDatos.Ventas;
using Gestor.Errores;

namespace GestoresDeNegocio.Ventas
{
    public class GestorDeTiposDeFacturaEmt : GestorDeTiposDeElemento<ContextoSe, TipoDeFacturaEmtDtm, TipoDeFacturaEmtDto>
    {
        public class ltrDeUnTipoDeFacturaEmt
        {

        }

        public class MapearTipoDeFacturaEmt : MapearTipoDeElemento
        {
            public MapearTipoDeFacturaEmt()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeFacturaEmtDtm, TipoDeFacturaEmtDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.FacturaEmitida))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));

                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeFacturaEmtDto, TipoDeFacturaEmtDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeFacturaEmt(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.FacturaEmitida)
        {

        }

        public static GestorDeTiposDeFacturaEmt Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeFacturaEmt(contexto, mapeador);
        }

        public static TipoDeFacturaEmtDtm PersistirTipo(ContextoSe contexto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla, string serie, int vencimiento)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeFacturaEmtDtm();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.Serie = serie;
                tipo.Vencimiento = vencimiento;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla || leido.Serie != serie || leido.ClaseDeLibro != clsLibro || leido.Vencimiento != vencimiento)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro; leido.Serie = serie; leido.Vencimiento = vencimiento;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDeFacturaEmtDtm> AplicarJoins(IQueryable<TipoDeFacturaEmtDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                   .Include(x => x.Estado);
        }

        protected override int ValidarNoHayElementos(TipoDeFacturaEmtDtm registro, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(registro, parametros);
            if (cantidad > 0 && parametros.Operacion == enumTipoOperacion.Modificar)
            {
                if (((TipoDeFacturaEmtDtm)parametros.registroEnBd).Serie != registro.Serie
                    || ((TipoDeFacturaEmtDtm)parametros.registroEnBd).Serie != registro.Serie)
                    GestorDeErrores.Emitir($"No se puede modificar la serie del tipo '{registro.Nombre}' ya que tiene elementos asociados");
            }
            return cantidad;
        }

        protected override void AntesDePersistir(TipoDeFacturaEmtDtm tipoFactura, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoFactura, parametros);
            if (tipoFactura.Serie.IsNullOrEmpty() || tipoFactura.Serie.Trim().Length > 3 || tipoFactura.Serie.EsEntero())
                GestorDeErrores.Emitir($"La serie del tipo de factura '{tipoFactura.Nombre}' solo puede contener un caracter alfanumérico");

            tipoFactura.Serie = tipoFactura.Serie.ToUpper();

            if (tipoFactura.Serie == ltrDeFacturaRectificada.Serie) 
                GestorDeErrores.Emitir($"La serie '{ltrDeFacturaRectificada.Serie}' está reservada para las rectificativas");

            tipoFactura.ValidarSerie(Contexto);

            if (tipoFactura.Vencimiento < 0) 
                GestorDeErrores.Emitir($"El vencimiento debe ser mayor de cero");
        }

    }
}
