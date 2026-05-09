using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Seguridad;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Negocio
{
    public class GestorDeTransiciones : GestorDeElementos<ContextoSe, TransicionDtm, TransicionDto>, IGestorGenerico
    {
        private string _tabla { get; set; }

        private enumNegocio _Negocio;

        public override enumNegocio Negocio => _Negocio;

        private IGestor _gestor => NegociosDeSe.CrearGestor(Contexto, Negocio);


        public class MapearTransiciones : Profile
        {
            public MapearTransiciones()
            {
                CreateMap<TransicionDtm, TransicionDto>();
                CreateMap<TransicionDto, TransicionDtm>();
            }
        }

        public GestorDeTransiciones(ContextoSe contexto)
        : base(contexto)
        {
        }

        public GestorDeTransiciones(ContextoSe contexto, enumNegocio negocio)
        : base(contexto, contexto.Mapeador)
        {
            AsignarNegocio(negocio);
        }

        public void AsignarNegocio(enumNegocio negocio)
        {
            _tabla = ApiDeTransicion.TablaDeTransiciones(negocio.TipoDtm());
            _Negocio = negocio;
        }

        public static GestorDeTransiciones Gestor(ContextoSe contexto, enumNegocio negocio) => new GestorDeTransiciones(contexto, negocio);

        public static TransicionDtm LeerPorNombre(ContextoSe contexto, enumNegocio negocio, string nombre) =>
            Gestor(contexto, negocio).LeerRegistro(nameof(INombre.Nombre), nombre, true, true, false, false);

        public static void DefinirTransicion(ContextoSe contexto, enumNegocio negocio, string transicion, string origen, string destino, bool activo = true, bool conObservacion = false, bool delSistema = false, string asunto = null, bool porDefecto=false)
        {
            var estadoOrigen = negocio.Estado(contexto, origen);
            var estadoDestino = negocio.Estado(contexto, destino);
            PersistirTransicion(contexto, negocio, estadoOrigen, estadoDestino, transicion, activo, asunto.IsNullOrEmpty() ? conObservacion : true, delSistema, asunto, porDefecto);
        }

        private static TransicionDtm PersistirTransicion(ContextoSe contexto, enumNegocio negocio, EstadoDtm origen, EstadoDtm destino,
            string nombre, bool activo, bool conObservacion, bool delSistema, string asunto, bool porDefecto)
        {
            var gestor = Gestor(contexto, negocio);
            var f1 = new ClausulaDeFiltrado(nameof(ltrTransiciones.filtroPorEstados), enumCriteriosDeFiltrado.igual, $"{origen.Id};{destino.Id}");
            var f2 = new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.igual, nombre);
            var leido = gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { f1, f2 });

            if (leido.Count == 0)
            {
                TransicionDtm transicion = new TransicionDtm();
                transicion.IdOrigen = origen.Id;
                transicion.IdDestino = destino.Id;
                transicion.Nombre = nombre;
                transicion.Activo = activo;
                transicion.Asunto = asunto;
                transicion.ConObservacion = conObservacion;
                transicion.DelSistema = delSistema;
                transicion.PorDefecto = false;
                transicion = gestor.PersistirRegistro(transicion, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                return gestor.LeerRegistroPorId(transicion.Id);
            }

            if (leido[0].Activo != activo || leido[0].Asunto != asunto
                || leido[0].ConObservacion != conObservacion
                || leido[0].DelSistema != delSistema)
            {
                leido[0].Activo = activo;
                leido[0].ConObservacion = conObservacion;
                leido[0].DelSistema = delSistema;
                leido[0].Asunto = asunto;
                gestor.PersistirRegistro(leido[0], new ParametrosDeNegocio(enumTipoOperacion.Modificar));
            }

            return leido[0];
        }

        public static List<TransicionDtm> TransicionesDesde(ContextoSe contexto, enumNegocio negocio, int idEstado, bool activo = false, bool? delSistema = null)
        {
            var gestor = Gestor(contexto, negocio);
            var filtros = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(TransicionDtm.IdOrigen), enumCriteriosDeFiltrado.igual, idEstado),
                new ClausulaDeFiltrado(nameof(TransicionDtm.Activo), enumCriteriosDeFiltrado.igual, activo)
            };
            if (delSistema != null) filtros.Add(new ClausulaDeFiltrado(nameof(TransicionDtm.DelSistema), enumCriteriosDeFiltrado.igual, delSistema));
            return gestor.LeerRegistros(0, -1, filtros);
        }

        public static TransicionDtm TransicionHasta(ContextoSe contexto, enumNegocio negocio,int idEstadoOrigen, int idEstadoDestino, bool delSistema)
        {
            var transiciones = TransicionesHasta(contexto, negocio, idEstadoOrigen, idEstadoDestino, activo: true, delSistema);
            if (transiciones.Count == 0)
                GestorDeErrores.Emitir($"No hay transición desde '{negocio.Estado(contexto, idEstadoOrigen).Nombre}' a '{negocio.Estado(contexto, idEstadoDestino).Nombre}' en el negocio '{negocio.Singular()}'");

            if (transiciones.Count > 1)
                GestorDeErrores.Emitir($"Hay '{transiciones.Count}' transiciones desde '{negocio.Estado(contexto, idEstadoOrigen).Nombre}' a '{negocio.Estado(contexto, idEstadoDestino).Nombre}' en el negocio '{negocio.Singular()}'");

            return transiciones[0];
        }

        private static List<TransicionDtm> TransicionesHasta(ContextoSe contexto, enumNegocio negocio, int idEstadoOrigen, int idEstadoDestino, bool activo = false, bool? delSistema = null)
        {
            var gestor = Gestor(contexto, negocio);
            var filtros = new List<ClausulaDeFiltrado>
            {
                new ClausulaDeFiltrado(nameof(TransicionDtm.IdOrigen), enumCriteriosDeFiltrado.igual, idEstadoOrigen),
                new ClausulaDeFiltrado(nameof(TransicionDtm.IdDestino), enumCriteriosDeFiltrado.igual, idEstadoDestino),
                new ClausulaDeFiltrado(nameof(TransicionDtm.Activo), enumCriteriosDeFiltrado.igual, activo)
            };
            if (delSistema != null) filtros.Add(new ClausulaDeFiltrado(nameof(TransicionDtm.DelSistema), enumCriteriosDeFiltrado.igual, delSistema));
            return gestor.LeerRegistros(0, -1, filtros);
        }

        public TransicionDtm LeerRegistroPorId(int id)
            => TransicionSql.LeerTransicionPorId(Contexto, _tabla, id);

        public IEnumerable<TransicionDtm> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden)
            => TransicionSql.LeerTransiciones(Contexto, _tabla, posicion, cantidad, filtros, orden);

        public int ContarRegistros(List<ClausulaDeFiltrado> filtros)
            => TransicionSql.ContarRegistros(Contexto, _tabla, filtros);

        public override TransicionDtm LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, bool errorSiNoHay, Dictionary<string, object> parametros = null)
        {
            return LeerRegistroPorId(id);
        }

        public override List<TransicionDtm> LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros = null, List<ClausulaDeOrdenacion> orden = null, ParametrosDeNegocio parametros = null)
        {
            var seleccionadas = LeerRegistros(posicion, cantidad, filtros, orden);
            if (parametros.CargarListaDeElementos)
            {
                var conpermisos = new List<TransicionDtm>();
                foreach(var transicion in seleccionadas)
                {
                    if (transicion.Activo && ApiDePermisos.HayPermisosDeTransicion(Contexto,Negocio, transicion, excluirLasDelSistema:true))
                    {
                        conpermisos.Add(transicion);
                    }
                }
                seleccionadas = conpermisos;
            }
            return seleccionadas.ToList();
        }

        public override int Contar(List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros = null)
        {
            CacheDeRecuentos[typeof(TransicionSql).FullName] = false;
            return TransicionSql.ContarRegistros(Contexto, _tabla, filtros);
        }
        protected override void AntesDePersistir(TransicionDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);

            if (registro.ConObservacion && registro.Asunto.IsNullOrEmpty())
                GestorDeErrores.Emitir("Si indica que la transición lleva observación, ha de indicar el asunto");

            if (parametros.Operacion == enumTipoOperacion.Insertar || parametros.Operacion == enumTipoOperacion.Modificar)
            {
                registro.Destino = GestorDeEstados.LeerRegistroPorId(Contexto, registro.IdDestino, negocio: Negocio).Nombre;
                registro.Origen = GestorDeEstados.LeerRegistroPorId(Contexto, registro.IdOrigen, negocio: Negocio).Nombre;
                var nombre = $"{registro.Origen} -> {registro.Nombre} -> {registro.Destino}";
                if (parametros.Operacion == enumTipoOperacion.Insertar)
                {
                    registro.IdPermiso = GestorDePermisos.CrearObtener(Contexto, Negocio, nombre, enumClaseDePermiso.Transicion, enumModoDeAccesoDeDatos.Gestor).Id;
                }
                else
                {
                    //bool modificarNombrePermiso = ModificarNombrePermiso(nuevo: registro, anterior: (ITransicionDtm)parametros.registroEnBd);
                    //if (modificarNombrePermiso)
                    //{
                    registro.IdPermiso = ((TransicionDtm)parametros.registroEnBd).IdPermiso;
                    var permiso = GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso);
                    GestorDePermisos.ModificarPermisoDeDatos(Contexto, Negocio, permiso, nombre, enumClaseDePermiso.Transicion, enumModoDeAccesoDeDatos.Consultor);
                    //}
                }
            }


            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                if (GestorDeAccionesDeTrn.HayAcciones(Contexto, Negocio, registro.Id))
                    GestorDeErrores.Emitir("La transicción so se puede eliminar por tener acciones asocoiadas");
            }
        }

        private bool ModificarNombrePermiso(TransicionDtm nuevo, ITransicion anterior)
        {
            var modificarNombrePermiso = false;
            if (nuevo.IdDestino != anterior.IdDestino)
            {
                modificarNombrePermiso = true;
                nuevo.Destino = GestorDePermisos.LeerRegistroPorId(Contexto, nuevo.IdDestino).Nombre;
            }
            if (nuevo.IdOrigen != anterior.IdOrigen)
            {
                modificarNombrePermiso = true;
                nuevo.Origen = GestorDePermisos.LeerRegistroPorId(Contexto, nuevo.IdOrigen).Nombre;
            }
            if (nuevo.Nombre != anterior.Nombre)
            {
                modificarNombrePermiso = true;
            }
            return modificarNombrePermiso;
        }

        public override TransicionDtm MapearRegistro(TransicionDto elemento, ParametrosDeNegocio opciones)
        {
            if (Negocio == enumNegocio.No_Definido)
            {
                if (elemento.IdNegocio > 0) AsignarNegocio(NegociosDeSe.ToEnumerado(elemento.IdNegocio));
                else GestorDeErrores.Emitir($"No se ha indicado el negocio de la transición a persistir");
            }
            return base.MapearRegistro(elemento, opciones);
        }

        protected override void Persistir(TransicionDtm registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion == enumTipoOperacion.Insertar)
                registro.Id = TransicionSql.Insertar(Contexto, _tabla, registro).Id;
            else if (parametros.Operacion == enumTipoOperacion.Modificar)
                TransicionSql.Modificar(Contexto, _tabla, registro);
            else
                TransicionSql.Borrar(Contexto, _tabla, registro);
        }

        protected override void DespuesDePersistir(TransicionDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
                GestorDePermisos.Eliminar(Contexto, Mapeador, GestorDePermisos.LeerRegistroPorId(Contexto, registro.IdPermiso));

            if (registro.PorDefecto && ( parametros.Insertando || registro.SeHaModificadoElCampo<bool>(x => x.Name == nameof(TransicionDtm.PorDefecto), parametros)))
            {
                TransicionSql.FijarPorDefecto(Contexto, _tabla, registro);
            }
        }

        protected override void EliminarCaches(TransicionDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            var clave = $"{_tabla}.{registro.Id}";
            ServicioDeCaches.EliminarElemento(nameof(TransicionSql.LeerTransicionPorId), clave);
            ServicioDeCaches.EliminarElemento(CacheDe.Negocio_Transiciones, Negocio.ToString());
            ServicioDeCaches.EliminarElementos(CacheDe.Negocio_TransicionesDisponibles, Negocio.ToString());
            ServicioDeCaches.EliminarElementos(CacheDe.Negocio_TransicionesHasta, Negocio.ToString());
        }

        protected override void DespuesDeMapearElElemento(TransicionDtm registro, TransicionDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.IdNegocio = NegociosDeSe.IdNegocio(Negocio);
            elemento.Negocio = NegociosDeSe.ToNombre(Negocio);
        }

        public static void GenerarSeguridadParaElUsuario(ContextoSe contexto, int idUsuario, Action<string> logger = null)
        {
            PermisosPorTransicionSql.QuitarPermisos(contexto, idUsuario, calculado: true);
            var permisosPorTransicion = ApiDePermisos.PermisosCalculadosDe(contexto, idUsuario, enumClaseDePermiso.Transicion);
            foreach (var permiso in permisosPorTransicion)
            {
                var negocio = enumNegocio.Parse<enumNegocio>(permiso.Nombre.Split(".")[0], true);
                var transicion = negocio.ListaDeTransiciones(contexto).First(x => x.IdPermiso == permiso.IdPermiso);
                PermisosPorTransicionSql.Otorgar(contexto, negocio.IdNegocio(), transicion.Id, idUsuario, transicion.IdPermiso, calculado: true);
            }

            PermisosPorTransicionSql.QuitarPermisos(contexto, idUsuario, calculado: false);
            var permisosDirectos = ApiDePermisos.PermisosDirectosDe(contexto, idUsuario, enumClaseDePermiso.Transicion);
            foreach (var permiso in permisosDirectos)
            {
                var negocio = enumNegocio.Parse<enumNegocio>(permiso.Nombre.Split(".")[0], true);
                var transicion = negocio.ListaDeTransiciones(contexto).First(x => x.IdPermiso == permiso.IdPermiso);
                PermisosPorTransicionSql.Otorgar(contexto, negocio.IdNegocio(), transicion.Id, idUsuario, transicion.IdPermiso, calculado: false);
            }
        }
        public static void GenerarSeguridad(ContextoSe contexto, Action<string> logger = null)
        {
            PermisosPorTransicionSql.EliminarTodos(contexto);
            foreach (var negocio in Enum.GetValues(typeof(enumNegocio)))
            {
                if (!NegociosDeSe.UsaFlujo((enumNegocio)negocio))
                    continue;

                var negocioDtm = NegociosDeSe.LeerNegocioPorEnumerado((enumNegocio)negocio);
                var transiciones = (List<TransicionDtm>)contexto.CrearGestorDeTransiciones((enumNegocio)negocio).LeerRegistros(0, -1, new List<ClausulaDeFiltrado>(), false);

                logger?.Invoke($"Procesando las transiciones del negocio de {(enumNegocio)negocio}");
                OtorgarSeguridadPorTransicion(contexto, negocioDtm.Id, transiciones);
            }
        }

        private static void OtorgarSeguridadPorTransicion(ContextoSe contexto, int idNegocio, List<TransicionDtm> transiciones)
        {
            foreach (var transicion in transiciones)
            {
                var filtro = new ClausulaDeFiltrado(nameof(PermisosDeUnRolDtm.IdPermiso), enumCriteriosDeFiltrado.igual);
                filtro.Valor = transicion.IdPermiso.ToString();
                var usuariosConElPermiso = GestorDePermisosDeUnUsuario.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });
                foreach (var usuario in usuariosConElPermiso)
                    PermisosPorTransicionSql.Otorgar(contexto, idNegocio, transicion.Id, usuario.IdUsuario, transicion.IdPermiso, calculado: true);
            }
        }


    }

}
