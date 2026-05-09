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

namespace GestoresDeNegocio.Juridico
{
    public class GestorDeTiposDePleito : GestorDeTiposDeElemento<ContextoSe, TipoDePleitoDtm, TipoDePleitoDto>
    {
        public class ltrDeUnTipoDePleito
        {

        }

        public class MapearTipoDePleito : MapearTipoDeElemento
        {
            public MapearTipoDePleito()
            {
                ReglasDeMapeoDelDtmAlDto(CreateMap<TipoDePleitoDtm, TipoDePleitoDto>())
               .ForMember(dto => dto.Negocio, dtm => enumLiteralesDeNegocio.Plural(enumNegocio.Pleito))
               .ForMember(dto => dto.Padre, dtm => dtm.MapFrom(x => x.Padre.Expresion))
               .ForMember(dto => dto.Estado, dtm => dtm.MapFrom(x => x.Estado.Nombre));


                ReglasDeMapeoDelDtoAlDtm(CreateMap<TipoDePleitoDto, TipoDePleitoDtm>())
                .ForMember(dtm => dtm.Padre, dto => dto.Ignore());
            }
        }


        public GestorDeTiposDePleito(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador, enumNegocio.Pleito)
        {

        }

        public static GestorDeTiposDePleito Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTiposDePleito(contexto, mapeador);
        }

        public static TipoDePleitoDtm PersistirTipo(ContextoSe contexto, enumClaseDePleito clsPleito, string nombre, int idEstado, enumClaseDeLibro clsLibro, string sigla)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var leido = gestor.LeerRegistroCacheado(nameof(TipoDeElementoDtm.Nombre), nombre, false, false, false);
            if (leido == null)
            {
                var tipo = new TipoDePleitoDtm();
                tipo.ClaseDePleito = clsPleito;
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

        protected override IQueryable<TipoDePleitoDtm> AplicarJoins(IQueryable<TipoDePleitoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            return consulta.Include(x => x.Padre)
                .Include(x => x.Estado);
        }

        protected override int ValidarNoHayElementos(TipoDePleitoDtm tipoDePleito, ParametrosDeNegocio parametros)
        {
            var cantidad = base.ValidarNoHayElementos(tipoDePleito, parametros);
            if (cantidad > 0)
            {
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                {
                    if (((TipoDePleitoDtm)parametros.registroEnBd).ClaseDePleito != tipoDePleito.ClaseDePleito)
                        GestorDeErrores.Emitir($"No se puede modificar el tipo '{tipoDePleito.Nombre}' ya que tiene elementos asociados");
                }
            }
            return cantidad;
        }


    }
}
