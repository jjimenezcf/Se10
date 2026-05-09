using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using Utilidades;

namespace GestoresDeNegocio.Negocio
{
    public class GestorDeHitos : GestorDeElementos<ContextoSe, HitoDtm, HitoDto>, IGestorGenerico
    {
        private string _tabla { get; set; }

        private enumNegocio _Negocio;

        public override enumNegocio Negocio => _Negocio;

        private IGestor GestorDeElemento => NegociosDeSe.CrearGestor(Contexto, Negocio);


        public class ltrFlujos
        {
            public const string ultimoHito = nameof(ultimoHito);
        }

        public class MapearHitoDelFlujo : Profile
        {
            public MapearHitoDelFlujo()
            {
                CreateMap<HitoDtm, HitoDto>();
                CreateMap<HitoDto, HitoDtm>();
            }
        }

        public GestorDeHitos(ContextoSe contexto)
        : base(contexto)
        {
            Contexto = contexto;
        }

        public GestorDeHitos(ContextoSe contexto, enumNegocio negocio)
        : base(contexto, contexto.Mapeador)
        {
            AsignarNegocio(negocio);
        }

        public void AsignarNegocio(enumNegocio negocio)
        {
            _tabla = ApiDeHitos.TablaDeHistorias(negocio.TipoDtm());
            _Negocio = negocio;
        }

        public static GestorDeHitos Gestor(ContextoSe contexto, enumNegocio negocio) => new GestorDeHitos(contexto, negocio);

        public static void IniciarFlujo(ContextoSe contexto, enumNegocio negocio, int id, TipoConFlujoDtm tipo, DateTime fecha)
        {
            var flujo = new GestorDeHitos(contexto, negocio);
            var hito = new HitoDtm();
            hito.IdElemento = id;
            hito.IdEstado = tipo.IdEstado;
            var parametros = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
            parametros.Parametros[nameof(HitoDtm.Fecha)] = fecha;
            flujo.PersistirRegistro(hito, parametros);
        }

        public static IElementoDeProcesoDtm Transitar(ContextoSe contexto, enumNegocio negocio, IElementoDeProcesoDtm registro, TransicionDtm transicion, Dictionary<string, object> parametros)
        {
            GestorDeHitos flujo = new GestorDeHitos(contexto, negocio);
            HitoDtm ultimoHito = flujo.LeerUltimoHito(registro.Id);
            
            if (ultimoHito.IdEstado != registro.IdEstado)
                GestorDeErrores.Emitir($"El elemento '{registro.Expresion}' esta en el " +
                    $"estado '{registro.Estado(contexto).Nombre}', " +
                    $"no se le puede aplicar la transición '{transicion.Nombre}'");

            var asunto = transicion.Asunto is null ? parametros.LeerValor(ltrParametrosEp.asunto, "") : transicion.Asunto;
            if (!asunto.IsNullOrEmpty())
            {
                var detalle = parametros.LeerValor(ltrParametrosEp.detalleAsunto, "");
                if (asunto == transicion.Asunto && detalle.IsNullOrEmpty() && !contexto.Test)
                    GestorDeErrores.Emitir($"Para poder aplicar la transicion '{transicion.Nombre}' a '{registro.Referencia}' del negocio '{negocio.Singular(true)}' ha de indicar un detalle");

                ultimoHito.IdObservacion = registro.CrearObservacion(contexto, asunto, detalle, parametros: new Dictionary<string, object> {
                    { ltrParametrosNeg.ValidarPermisosDePersistencia, false } ,
                    { ltrParametrosNeg.EsUnaTransicion, true }}).Id;
            }

            var parametroDeHito = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
            ultimoHito.IdTransicion = transicion.Id;
            parametroDeHito.Parametros[nameof(HitoDtm.Fecha)] = parametros.LeerValor<DateTime>(ltrParametrosNeg.FechaDeTransicion, default);
            flujo.PersistirRegistro(ultimoHito, parametroDeHito);

            parametroDeHito = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
            parametroDeHito.Parametros[nameof(HitoDtm.Fecha)] = parametros.LeerValor<DateTime>(ltrParametrosNeg.FechaDeTransicion, default);
            var abrirHito = new HitoDtm();
            abrirHito.IdElemento = registro.Id;
            abrirHito.IdEstado = transicion.IdDestino;
            flujo.PersistirRegistro(abrirHito, parametroDeHito);

            var gestor = negocio.CrearGestor(contexto);
            registro.IdEstado = transicion.IdDestino;
            var parametroParaActualizar = new ParametrosDeNegocio(enumTipoOperacion.Modificar, parametros);
            parametroParaActualizar.ValidarPermisosDePersistencia = false;
            parametroParaActualizar.EsUnaTransicion = true;
            registro =  (IElementoDeProcesoDtm)gestor.PersistirRegistro(registro, parametroParaActualizar);
            return registro;
        }

        private HitoDtm LeerUltimoHito(int id)
        {
            var filtros = new List<ClausulaDeFiltrado>();
            filtros.Add(new ClausulaDeFiltrado(nameof(HitoDtm.IdElemento), enumCriteriosDeFiltrado.igual, id.ToString()));
            filtros.Add(new ClausulaDeFiltrado(ltrFlujos.ultimoHito, enumCriteriosDeFiltrado.esNulo));
            return LeerRegistros(0, 1, filtros)[0];
        }

        public HitoDtm LeerRegistroPorId(int id)
            => HitoSql.LeerHistoriaPorId(Contexto, _tabla, id);

        public IEnumerable<HitoDtm> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden)
        {
            int idElemento = BuscarIdElemento(filtros);
            if (filtros.Find(x => x.Clausula.Equals(ltrFlujos.ultimoHito)) == null)
                return HitoSql.LeerHistoriaDelElemento(Contexto, _tabla, idElemento);

            return HitoSql.LeerUltimoHito(Contexto, _tabla, idElemento);
        }
        public int ContarRegistros(List<ClausulaDeFiltrado> filtros)
            => HitoSql.ContarRegistros(Contexto, _tabla, BuscarIdElemento(filtros));

        private static int BuscarIdElemento(List<ClausulaDeFiltrado> filtros)
        {
            var idElemento = 0;
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == nameof(HitoDtm.IdElemento).ToLower());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                return filtro.Valor.Entero();
            }

            return idElemento;
        }

        public override HitoDtm LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, bool errorSiNoHay, Dictionary<string, object> parametros = null)
        {
            return LeerRegistroPorId(id);
        }

        public override List<HitoDtm> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros = null, List<ClausulaDeOrdenacion> orden = null, ParametrosDeNegocio parametros = null)
        {
            return LeerRegistros(posicion, cantidad, filtros, orden).ToList();
        }

        public override int Contar(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros = null)
        {
            CacheDeRecuentos[typeof(HitoSql).FullName] = false;
            return HitoSql.ContarRegistros(Contexto, _tabla, BuscarIdElemento(filtros));
        }

        protected override void AntesDePersistir(HitoDtm hito, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(hito, parametros);
            hito.IdUsuario = Contexto.DatosDeConexion.IdUsuario;
        }

        protected override void Persistir(HitoDtm hito, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                hito.Fecha = (DateTime)parametros.Parametros[nameof(HitoDtm.Fecha)];
                hito.Id = HitoSql.Insertar(Contexto, _tabla, hito).Id;
            }
            else if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                var fechafin = (DateTime)parametros.Parametros[nameof(HitoDtm.Fecha)];
                HitoSql.Modificar(Contexto, _tabla, hito, fechafin);
            }
        }

        protected override void EliminarCaches(HitoDtm hito, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(hito, parametros);
            var clave = $"{_tabla}.{hito.Id}";
            ServicioDeCaches.EliminarElemento(nameof(HitoSql.LeerHistoriaPorId), clave);
        }

        protected override void DespuesDeMapearElElemento(HitoDtm hito, HitoDto hitoDto, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(hito, hitoDto, parametros);
            hitoDto.IdNegocio = NegociosDeSe.IdNegocio(Negocio);
            hitoDto.Negocio = NegociosDeSe.ToNombre(Negocio);
            if (hito.IdTransicion.HasValue)
                hitoDto.Transicion = hitoDto.Transicion + " (" + hito.QuienLoHaTransitado(Contexto, Negocio).Login + ")";
        }

        protected override IQueryable<HitoDtm> AplicarJoins(IQueryable<HitoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            return base.AplicarJoins(consulta, filtros, parametros);
        }

    }
}
