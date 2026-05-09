using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using Gestor.Errores;
using ModeloDeDto.Presupuesto;
using ServicioDeDatos.Presupuesto;

namespace GestoresDeNegocio.Presupuesto
{
    public class GestorDeTiposDePresupuesto : GestorDeTiposDeElemento<ContextoSe, TipoDePresupuestoDtm, TipoDePresupuestoDto>
    {
        public class ltrDeUnTipoDePresupuesto
        {

        }

        public class MapearTipoDePresupuesto : MapearTipoDeElemento
        {
            public MapearTipoDePresupuesto()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDePresupuestoDtm, TipoDePresupuestoDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Presupuesto))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre))
               .ForMember(dto => dto.TipoFacturaEmt, dtm => dtm.MapFrom(x => x.TipoFacturaEmt.Nombre))
               .ForMember(dto => dto.TipoParteTr, dtm => dtm.MapFrom(x => x.TipoParteTr.Nombre));


                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDePresupuestoDto, TipoDePresupuestoDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoFacturaEmt, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoParteTr, dto => dto.Ignore()); 
            }
        }


        public GestorDeTiposDePresupuesto(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Presupuesto)
        {

        }

        public static GestorDeTiposDePresupuesto Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDePresupuesto(contexto, mapeador);
        }

        public static TipoDePresupuestoDtm PersistirTipo(ContextoSe contexto, enumClaseDePresupuesto clsPresupuesto, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDePresupuestoDtm();
                tipo.ClaseDePresupuesto = clsPresupuesto;
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo = gestor.PersistirRegistro(tipo, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(tipo.Id, false, false, false, true);
            }

            if (leido.Nombre != nombre || leido.Sigla != sigla || leido.ClaseDeLibro != clsLibro)
            {
                leido.Nombre = nombre; leido.Sigla = sigla; leido.ClaseDeLibro = clsLibro;
                gestor.PersistirRegistro(leido, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return leido;
        }

        protected override IQueryable<TipoDePresupuestoDtm> AplicarJoins(IQueryable<TipoDePresupuestoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                .Include(x => x.Estado)
                .Include(x => x.TipoFacturaEmt)
                .Include(x => x.TipoParteTr);
        }

        protected override int ValidarNoHayElementos(TipoDePresupuestoDtm tipoDePresupuesto, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(tipoDePresupuesto, parametros);
            if (cantidad > 0)
            {
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                {
                    if (((TipoDePresupuestoDtm)parametros.registroEnBd).ClaseDePresupuesto != tipoDePresupuesto.ClaseDePresupuesto)
                        GestorDeErrores.Emitir($"No se puede modificar el tipo '{tipoDePresupuesto.Nombre}' ya que tiene elementos asociados");
                }
            }
            return cantidad;
        }

        protected override void AntesDePersistir(TipoDePresupuestoDtm tipoPpt, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoPpt, parametros);
            if (tipoPpt.IdTipoFacturaEmt != null && tipoPpt.ClaseDePresupuesto != enumClaseDePresupuesto.venta)
                GestorDeErrores.Emitir($"La clase '{tipoPpt.ClaseDePresupuesto.Descripcion()}' no admite facturación");

            if (tipoPpt.IdTipoParteTr != null && tipoPpt.ClaseDePresupuesto != enumClaseDePresupuesto.venta)
                GestorDeErrores.Emitir($"La clase '{tipoPpt.ClaseDePresupuesto.Descripcion()}' no admite partes de trabajo");
        }


    }
}
