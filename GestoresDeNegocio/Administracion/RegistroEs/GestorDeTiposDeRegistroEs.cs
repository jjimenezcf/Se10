using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using GestoresDeNegocio.Negocio;
using ServicioDeDatos.RegistroEs;
using ModeloDeDto.RegistroEs;
using Gestor.Errores;
using GestoresDeNegocio.SistemaDocumental;

namespace GestoresDeNegocio.RegistroEs
{
    public class GestorDeTiposDeRegistroEs : GestorDeTiposDeElemento<ContextoSe, TipoDeRegistroEsDtm, TipoDeRegistroEsDto>
    {
        public static class ltrDeUnTipoDeRegistroEs
        {
            public static string NombreArchivadorTipoPadreDelRegistroRe(string sigla) => $"{sigla}: Registro E/S";
            public static string NombreArchivadorTipoInterno(string sigla) => $"{sigla}: Interno";
            public static string NombreArchivadorTipoEntrada(string sigla) => $"{sigla}: Entrada";
            public static string NombreArchivadorTipoSalida(string sigla) => $"{sigla}: Salida";
        }

        public class MapearTipoDeRegistroEs : MapearTipoDeElemento
        {
            public MapearTipoDeRegistroEs()
            {
                 ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDeRegistroEsDtm, TipoDeRegistroEsDto>())
                .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Registro))
                .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
                .ForMember(dto => dto.TipoArchivadorDeEntrada, dtm => dtm.MapFrom(x => x.TipoArchivadorDeEntrada.Nombre))
                .ForMember(dto => dto.TipoArchivadorDeSalida, dtm => dtm.MapFrom(x => x.TipoArchivadorDeSalida.Nombre))
                .ForMember(dto => dto.TipoArchivadorInterno, dtm => dtm.MapFrom(x => x.TipoArchivadorInterno.Nombre))
                .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));


                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDeRegistroEsDto, TipoDeRegistroEsDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoArchivadorDeEntrada, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoArchivadorDeSalida, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoArchivadorInterno, dto => dto.Ignore());
            }

        }


        public GestorDeTiposDeRegistroEs(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Registro)
        {

        }

        public static GestorDeTiposDeRegistroEs Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDeRegistroEs(contexto, mapeador);
        }

        public static TipoDeRegistroEsDtm PersistirTipo(ContextoSe contexto, enumClaseDeRegistroEs clsReg, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var tipoRee = GestorDeTiposDeArchivadores.PersistirTipo(contexto, ltrDeUnTipoDeRegistroEs.NombreArchivadorTipoPadreDelRegistroRe(sigla), enumClaseDeLibro.POR_CG_TIPO, "RES", false);
            var tipoAre = GestorDeTiposDeArchivadores.PersistirTipo(contexto, ltrDeUnTipoDeRegistroEs.NombreArchivadorTipoEntrada(sigla), enumClaseDeLibro.POR_CG_TIPO, "ARE", true, delSistema: false, nombreModificable: true,  tipoRee.Id);
            var tipoArs = GestorDeTiposDeArchivadores.PersistirTipo(contexto, ltrDeUnTipoDeRegistroEs.NombreArchivadorTipoSalida(sigla), enumClaseDeLibro.POR_CG_TIPO, "ARS", true, delSistema: false, nombreModificable: true, tipoRee.Id);
            var tipoAri = GestorDeTiposDeArchivadores.PersistirTipo(contexto, ltrDeUnTipoDeRegistroEs.NombreArchivadorTipoInterno(sigla), enumClaseDeLibro.POR_CG_TIPO, "ARI", visible: false, delSistema: false, nombreModificable: true, tipoRee.Id);

            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDeRegistroEsDtm();
                tipo.ClaseDeRegistro = clsReg.ToString();
                tipo.IdEstado = idEstado;
                tipo.ClaseDeLibro = clsLibro;
                tipo.Nombre = nombre;
                tipo.Sigla = sigla;
                tipo.IdTipoArchivadorDeEntrada = tipoAre.Id;
                tipo.IdTipoArchivadorDeSalida = tipoArs.Id;
                tipo.IdTipoArchivadorInterno = tipoAri.Id;
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

        protected override IQueryable<TipoDeRegistroEsDtm> AplicarJoins(IQueryable<TipoDeRegistroEsDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta =  base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.TipoArchivadorDeEntrada)
                .Include(x => x.TipoArchivadorDeSalida)
                .Include(x => x.TipoArchivadorInterno)
                .Include(x => x.Estado);
        }

        protected override IQueryable<TipoDeRegistroEsDtm> AplicarFiltros(IQueryable<TipoDeRegistroEsDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarFiltros(registros, filtros, parametros);
            registros = registros.Include(x => x.Padre);
            return registros;
        }

        protected override int ValidarNoHayElementos(TipoDeRegistroEsDtm tipoRegistroEs, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(tipoRegistroEs, parametros);
            if (cantidad > 0)
            {
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                {
                    if (((TipoDeRegistroEsDtm)parametros.registroEnBd).IdTipoArchivadorDeEntrada != tipoRegistroEs.IdTipoArchivadorDeEntrada
                        || ((TipoDeRegistroEsDtm)parametros.registroEnBd).IdTipoArchivadorDeSalida != tipoRegistroEs.IdTipoArchivadorDeSalida
                        || ((TipoDeRegistroEsDtm)parametros.registroEnBd).IdTipoArchivadorInterno != tipoRegistroEs.IdTipoArchivadorInterno
                        || ((TipoDeRegistroEsDtm)parametros.registroEnBd).ClaseDeRegistro != tipoRegistroEs.ClaseDeRegistro)
                        GestorDeErrores.Emitir($"No se puede modificar el tipo '{tipoRegistroEs.Nombre}' ya que tiene elementos asociados");
                }
            }
            return cantidad;
        }


    }
}
