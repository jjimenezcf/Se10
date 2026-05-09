using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;

using Utilidades;

namespace GestoresDeNegocio.Negocio
{
    public class GestorDeAccionesDeTrn : GestorDeRelaciones<ContextoSe, AccionesDeTrnDtm, AccionesDeTrnDto>, IGestorGenerico
    {
        private string _tabla { get; set; }

        private enumNegocio _Negocio;

        public override enumNegocio Negocio => _Negocio;

        private IGestor gestor => NegociosDeSe.CrearGestor(Contexto, Negocio);
        
        public string Tabla => _tabla;



        public class ltrTransiciones
        {
        }

        public class MapearTransiciones : Profile
        {
            public MapearTransiciones()
            {
                CreateMap<AccionesDeTrnDtm, AccionesDeTrnDto>();
                CreateMap<AccionesDeTrnDto, AccionesDeTrnDtm>();
            }
        }

        public GestorDeAccionesDeTrn(ContextoSe contexto)
        : base(contexto)
        {
        }

        public GestorDeAccionesDeTrn(ContextoSe contexto, enumNegocio negocio)
        : base(contexto, contexto.Mapeador)
        {
            AsignarNegocio(negocio);
        }

        public void AsignarNegocio(enumNegocio negocio)
        {
            _tabla = ApiDeAccionDeTrn.TablaDeAcciones(negocio.TipoDtm());
            _Negocio = negocio;
            PermitirMasDeUnRegistroParaLosMismosIds = true;
        }

        public static GestorDeAccionesDeTrn Gestor(ContextoSe contexto, enumNegocio negocio) => new GestorDeAccionesDeTrn(contexto, negocio);

        public static AccionesDeTrnDtm CrearAccionTrn(ContextoSe contexto, enumNegocio negocio, TransicionDtm transicion, string nombreAccion, enumMomentoDeEjecucion momento, string parametro, int orden, string descripcion)
        {
            AccionDtm accion = GestorDeAcciones.LeerAccion(contexto, nombreAccion);
            var gestor = Gestor(contexto, negocio);


            var f1 = new ClausulaDeFiltrado(nameof(AccionesDeTrnDtm.IdTransicion), enumCriteriosDeFiltrado.igual, $"{transicion.Id}");
            var f2 = new ClausulaDeFiltrado(nameof(AccionesDeTrnDtm.Orden), enumCriteriosDeFiltrado.igual, $"{orden}");
            var f3 = new ClausulaDeFiltrado(nameof(AccionesDeTrnDtm.Momento), enumCriteriosDeFiltrado.igual, momento.ToString());
            var hayAccionTrn = gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { f1, f2, f3 });
            if (hayAccionTrn.Count == 0)
            {
                f1 = new ClausulaDeFiltrado(nameof(AccionesDeTrnDtm.IdTransicion), enumCriteriosDeFiltrado.igual, $"{transicion.Id}");
                f2 = new ClausulaDeFiltrado(nameof(AccionesDeTrnDtm.IdAccion), enumCriteriosDeFiltrado.igual, $"{accion.Id}");
                f3 = new ClausulaDeFiltrado(nameof(AccionesDeTrnDtm.Momento), enumCriteriosDeFiltrado.igual, momento.ToString());
                List<AccionesDeTrnDtm> leido = gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { f1, f2, f3 });
                if (leido.Count == 0)
                {
                    var accionTrn = new AccionesDeTrnDtm();
                    accionTrn.IdTransicion = transicion.Id;
                    accionTrn.IdAccion = accion.Id;
                    accionTrn.Momento = momento.ToString();
                    accionTrn.Activo = true;
                    accionTrn.Parametros = parametro;
                    accionTrn.Orden = orden;
                    accionTrn.Descripcion = descripcion;

                    accionTrn = gestor.PersistirRegistro(accionTrn, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                    return gestor.LeerRegistroPorId(accionTrn.Id);
                }
                if (leido[0].Momento != momento.ToString() || leido[0].Descripcion != descripcion || leido[0].Parametros != parametro)
                {
                    leido[0].Momento = momento.ToString();
                    leido[0].Descripcion = descripcion;
                    leido[0].Parametros = parametro;
                    gestor.PersistirRegistro(leido[0], new ParametrosDeNegocio(enumTipoOperacion.Modificar));
                }
                return leido[0];
            }

            if (hayAccionTrn[0].IdAccion != accion.Id || hayAccionTrn[0].Descripcion != descripcion || hayAccionTrn[0].Parametros != parametro)
            {
                hayAccionTrn[0].IdAccion = accion.Id;
                hayAccionTrn[0].Descripcion = descripcion;
                hayAccionTrn[0].Parametros = parametro;
                gestor.PersistirRegistro(hayAccionTrn[0], new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }
            return hayAccionTrn[0];
        }

        public AccionesDeTrnDtm LeerRegistroPorId(int id)
            => AccionDeTrnSql.LeerAccionPorId(Contexto, _tabla, id);

        public IEnumerable<AccionesDeTrnDtm> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden)
            => AccionDeTrnSql.LeerAcciones(Contexto, _tabla, posicion, cantidad, filtros, orden);

        public int ContarRegistros(List<ClausulaDeFiltrado> filtros)
            => AccionDeTrnSql.ContarRegistros(Contexto, _tabla, filtros);


        public override AccionesDeTrnDtm LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, Dictionary<string, object> parametros = null)
        {
            return LeerRegistroPorId(id);
        }

        public override List<AccionesDeTrnDtm> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros = null, List<ClausulaDeOrdenacion> orden = null, ParametrosDeNegocio parametros = null)
        {
            return LeerRegistros(posicion, cantidad, filtros, orden).ToList();
        }

        public override int Contar(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros = null)
        {
            CacheDeRecuentos[typeof(AccionDeTrnSql).FullName] = false;
            return AccionDeTrnSql.ContarRegistros(Contexto, _tabla, filtros);
        }
        protected override void AntesDePersistir(AccionesDeTrnDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
        }


        protected override void Persistir(AccionesDeTrnDtm registro, ParametrosDeNegocio parametros)
        {
            if (!registro.Parametros.IsNullOrEmpty())
                extJson.ToDiccionarioDeParametros(registro.Parametros);

            if (parametros.Operacion == enumTipoOperacion.Insertar)
                registro.Id = AccionDeTrnSql.Insertar(Contexto, _tabla, registro).Id;
            else if (parametros.Operacion == enumTipoOperacion.Modificar)
                AccionDeTrnSql.Modificar(Contexto, _tabla, registro);
            else
                AccionDeTrnSql.Borrar(Contexto, _tabla, registro);
        }

        protected override void EliminarCaches(AccionesDeTrnDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            var clave = $"{_tabla}.{registro.Id}";
            ServicioDeCaches.EliminarElemento(nameof(AccionDeTrnSql.LeerAccionPorId), clave);
        }

        protected override void DespuesDeMapearElElemento(AccionesDeTrnDtm registro, AccionesDeTrnDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.IdNegocio = NegociosDeSe.IdNegocio(Negocio);
            elemento.Negocio = NegociosDeSe.ToNombre(Negocio);
        }

        internal static bool HayAcciones(ContextoSe contexto, enumNegocio negocio, int idTransicion)
        {
            var gestor = Gestor(contexto, negocio);
            var a = AccionDeTrnSql.ContarRegistros(contexto, gestor.Tabla, new List<ClausulaDeFiltrado> 
            {
                new ClausulaDeFiltrado 
                { Clausula = nameof(AccionesDeTrnDtm.Transicion)
                , Criterio = enumCriteriosDeFiltrado.igual
                , Valor = idTransicion.ToString() 
                } 
            });
            return a > 0;
        }
    }

}
