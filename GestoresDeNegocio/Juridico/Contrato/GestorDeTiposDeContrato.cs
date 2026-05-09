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
using ModeloDeDto.Juridico;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Ventas;

namespace GestoresDeNegocio.Juridico
{
    public class GestorDeTiposDeContrato : GestorDeTiposDeElemento<ContextoSe, TipoDeContratoDtm, TipoDeContratoDto>
    {
        public class ltrDeUnTipoDeContrato
        {

        }

        public class MapearTipoDeContrato : MapearTipoDeElemento
        {
            public MapearTipoDeContrato()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeContratoDtm, TipoDeContratoDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Contrato))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre == null ? null : x.Padre.Expresion))
               .ForMember(dto => dto.TipoArchivador, dtm => dtm.MapFrom(x => x.TipoArchivador == null ? null : x.TipoArchivador.Nombre))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado == null ? null : x.Estado.Nombre));


                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeContratoDto, TipoDeContratoDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDeContrato(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Contrato)
        {

        }

        public static GestorDeTiposDeContrato Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeContrato(contexto, mapeador);
        }

        public static TipoDeContratoDtm PersistirTipo(ContextoSe contexto, enumClaseDeContrato clsContrato, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeContratoDtm();
                tipo.ClaseDeContrato = clsContrato;
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

        protected override IQueryable<TipoDeContratoDtm> AplicarJoins(IQueryable<TipoDeContratoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                .Include(x => x.TipoArchivador)
                .Include(x => x.Estado);
        }

        protected override IQueryable<TipoDeContratoDtm> AplicarFiltros(IQueryable<TipoDeContratoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return base.AplicarFiltros(consulta, filtros, parametros);
        }

        protected override int ValidarNoHayElementos(TipoDeContratoDtm tipoDeContrato, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(tipoDeContrato, parametros);
            if (cantidad > 0)
            {
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                {
                    if (((TipoDeContratoDtm)parametros.registroEnBd).ClaseDeContrato != tipoDeContrato.ClaseDeContrato)
                        GestorDeErrores.Emitir($"No se puede modificar el tipo '{tipoDeContrato.Nombre}' ya que tiene elementos asociados");
                }
            }
            return cantidad;
        }

        protected override void AntesDePersistir(TipoDeContratoDtm tipoCtr, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoCtr, parametros);
            if (tipoCtr.IdTipoFacturaEmt != null && tipoCtr.ClaseDeContrato != enumClaseDeContrato.Venta && tipoCtr.ClaseDeContrato != enumClaseDeContrato.MatriculaDeGuarderia)
                GestorDeErrores.Emitir($"La clase '{tipoCtr.ClaseDeContrato.Descripcion()}' no admite facturación");
        }

        protected override void DespuesDeMapearElElemento(TipoDeContratoDtm tipoDtm, TipoDeContratoDto tipoDto, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(tipoDtm, tipoDto, parametros);
            if (parametros.Peticion == enumPeticion.epLeerPorId)
                tipoDto.TipoFacturaEmt = tipoDtm.IdTipoFacturaEmt == null ? null : Contexto.SeleccionarPorId<TipoDeFacturaEmtDtm>((int)tipoDtm.IdTipoFacturaEmt).Nombre;
        }

    }
}
