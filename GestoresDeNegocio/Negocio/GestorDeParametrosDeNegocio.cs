using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Negocio;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Negocio;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Contabilidad;
using GestoresDeNegocio.Contabilidad;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDeParametrosDeNegocio : GestorDeElementos<ContextoSe, ParametroDeNegocioDtm, ParametroDeNegocioDto>
    {
        public class MapearParametrosDeNegocio : Profile
        {
            public MapearParametrosDeNegocio()
            {
                CreateMap<ParametroDeNegocioDtm, ParametroDeNegocioDto>()
                .ForMember(dto => dto.Negocio, dtm => dtm.MapFrom(dtm => dtm.Negocio.Nombre));

                CreateMap<ParametroDeNegocioDto, ParametroDeNegocioDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDeParametrosDeNegocio(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDeParametrosDeNegocio Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeParametrosDeNegocio(contexto, mapeador);
        }

        protected override void AntesDePersistir(ParametroDeNegocioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(parametro, parametros);

            if (parametros.EsUnaPeticion && (parametros.Insertando || parametros.Eliminando))
            {
                if (!Contexto.SePuedeParametrizar())
                    GestorDeErrores.Emitir($"No se puede crear y eliminar parámetros desde la interface");
            }

            if (parametros.EsUnaPeticion && parametros.Modificando && parametro.Nombre.ToLower() == enumParametrosDeFacturasEmt.FAE_SII_Activo.ToString().ToLower() && parametro.Valor.EsFalse())
                GestorDeErrores.Emitir($"No se puede modificar el parámetro '{enumParametrosDeFacturasEmt.FAE_SII_Activo}' desde la interface, use la opción de menú en el mantenimiento de sociedades");

            if (parametros.EsUnaPeticion && parametros.Modificando && parametro.Nombre.ToLower() == enumParametrosDeFacturasEmt.FAE_SII_En_Productivo.ToString().ToLower())
                GestorDeErrores.Emitir($"No se puede modificar el parámetro '{enumParametrosDeFacturasEmt.FAE_SII_En_Productivo}' desde la interface, use la opción de menú en el mantenimiento de sociedades");

            if (parametro.PropiedadCambiada<string>(nameof(ParametroDeNegocioDtm.Nombre), parametros))
                GestorDeErrores.Emitir($"El nombre del parámetro '{((ParametroDeNegocioDtm)parametros.registroEnBd).Nombre}' no se puede cambiar por el usuario");

            ValidarQueNoSeDesactivaLaGeneracionDePreasientos(parametro, parametros);
            ValidarLaActivacionDePreasientos(parametro, parametros);
        }

        // Sociedades que pasan a generar preasientos del negocio del parámetro: antes de grabar se valida que se puedan generar los de sus
        // documentos sin preasiento y se guardan para someter tras grabar el trabajo que los genera
        private const string SociedadesQueActivanPreasientos = nameof(SociedadesQueActivanPreasientos);

        private void ValidarLaActivacionDePreasientos(ParametroDeNegocioDtm parametro, ParametrosDeNegocio parametros)
        {
            if (parametros.Eliminando || parametro.IdNegocio != enumNegocio.Preasiento.IdNegocio())
                return;

            var negocio = VariablesDePreasiento.NegocioDelParametroDeGenerarPreasiento(parametro.Nombre);
            if (negocio is null)
                return;

            var anterior = parametros.Insertando ? null : ((ParametroDeNegocioDtm)parametros.registroEnBd)?.Valor;
            var activadas = VariablesDePreasiento.SociedadesQueEmpiezanAGenerarPreasientos(anterior, parametro.Valor);
            foreach (var idSociedad in activadas)
                TrabajosContables.ValidarActivarPreasientosDeLaSociedad(Contexto, idSociedad, (enumNegocio)negocio);

            parametros.Parametros[SociedadesQueActivanPreasientos] = activadas;
        }

        private void SometerLaActivacionDePreasientos(ParametroDeNegocioDtm parametro, ParametrosDeNegocio parametros)
        {
            var activadas = parametros.Parametros.LeerValor<List<int>>(SociedadesQueActivanPreasientos, null);
            if (activadas is null || activadas.Count == 0)
                return;

            var negocio = (enumNegocio)VariablesDePreasiento.NegocioDelParametroDeGenerarPreasiento(parametro.Nombre);
            foreach (var idSociedad in activadas)
                TrabajosContables.SometerActivarPreasientosDeLaSociedad(Contexto, idSociedad, negocio);
        }

        // Una vez que una sociedad genera preasientos de un negocio no se puede desactivar, ni desde la interface ni por proceso:
        // dejaría documentos sin contabilizar entre otros que sí lo están. Si hiciese falta, lo ha de hacer el administrador en la bd
        private void ValidarQueNoSeDesactivaLaGeneracionDePreasientos(ParametroDeNegocioDtm parametro, ParametrosDeNegocio parametros)
        {
            if (parametros.Insertando)
                return;

            var enBd = (ParametroDeNegocioDtm)parametros.registroEnBd;
            if (enBd is null || enBd.IdNegocio != enumNegocio.Preasiento.IdNegocio())
                return;

            var negocio = VariablesDePreasiento.NegocioDelParametroDeGenerarPreasiento(enBd.Nombre);
            if (negocio is null)
                return;

            var desactivadas = VariablesDePreasiento.SociedadesQueDejanDeGenerarPreasientos(enBd.Valor, parametros.Eliminando ? null : parametro.Valor);
            if (desactivadas.Count == 0)
                return;

            var sociedades = Contexto.Set<SociedadDtm>().Where(s => desactivadas.Contains(s.Id)).ToList()
                .Select(s => $"'{s.Expresion}'");
            GestorDeErrores.Emitir($"No se puede desactivar la generación de preasientos de {((enumNegocio)negocio).Plural(enMinusculas: true)} para {string.Join(", ", sociedades)}: una vez activada no se puede desactivar. Si fuese necesario, lo ha de hacer el administrador en la base de datos");
        }

        protected override void DespuesDePersistir(ParametroDeNegocioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(parametro, parametros);
            SometerLaActivacionDePreasientos(parametro, parametros);
        }

        protected override void EliminarCaches(ParametroDeNegocioDtm parametro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(parametro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Negocio_ParametrosDeNegocio, $"{NegociosDeSe.ToEnumerado(parametro.IdNegocio)}-{parametro.Nombre}");

            if (parametro.Nombre == enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil.ToString())
                ServicioDeCaches.EliminarCache(CacheDe.ListasDeNegocios);
            else if (parametro.Nombre == enumParametrosDeNegocio.IA_Prompt_Filtro.ToString())
                ServicioDeCaches.EliminarCache(CacheDe.Ia_Filtros);
        }

        protected override IQueryable<ParametroDeNegocioDtm> AplicarJoins(IQueryable<ParametroDeNegocioDtm> parametro, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            parametro = base.AplicarJoins(parametro, filtros, parametros);
            parametro = parametro.Include(p => p.Negocio);
            return parametro;
        }

        protected override void DespuesDeMapearElElemento(ParametroDeNegocioDtm parametro, ParametroDeNegocioDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(parametro, elemento, parametros);

            var negocio = NegociosDeSe.ToEnumerado(parametro.IdNegocio);
            elemento.Descripcion = negocio.ObtenerDescripcionDelParametro(parametro.Nombre);
            if (elemento.Descripcion == string.Empty)
                elemento.Descripcion = negocio.ObtenerDescripcionDeEtapa(parametro.Nombre);
        }
    }
}
