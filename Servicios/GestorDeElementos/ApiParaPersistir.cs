using GestorDeElementos.Extensores;
using Microsoft.IdentityModel.Tokens;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos
{
    public static class ApiParaPersistir
    {
        public static T Persistir<T>(this T registro, ContextoSe contexto, bool aplicarJoin = false)
        where T : RegistroConNombreDtm
        => registro.PersistirPorAk(contexto, ((IRegistro)registro).Id == 0 ? nameof(INombre.Nombre) : nameof(IRegistro.Id), aplicarJoin: aplicarJoin);
       
        public static T PersistirPorNombre<T>(this T registro, ContextoSe contexto, bool errorSiYaExiste = false, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroConNombreDtm
        => registro.PersistirPorAk<T>(contexto, nameof(INombre.Nombre), errorSiYaExiste, aplicarJoin, parametros: parametros);

        public static T PersistirPorAk<T>(this T registro, ContextoSe contexto, string propiedad, bool errorSiYaExiste = false, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            if (typeof(T).HeredaDe(typeof(EstadoDtm)))
                throw new Exception($"El método  {nameof(PersistirPorAk)}  no acepta un resgistro del tipo {typeof(T).Name} Use el método {nameof(ExtensorDeEstados.PersistirEstado)}");

            if (typeof(T).HeredaDe(typeof(TransicionDtm)))
                throw new Exception($"El método  {nameof(PersistirPorAk)}  no acepta un resgistro del tipo {typeof(T).Name} Use el método {nameof(ExtensorDeTransiciones.PersistirTransicion)}");

            if (registro.Id > 0)
                return registro.Modificar(contexto);

            var leido = contexto.SeleccionarPorPropiedad<T>(propiedad, registro.LeerPropiedad(propiedad), errorSiNoHay: errorSiYaExiste, parametros: parametros);
            if (leido == null)
                return registro.Insertar(contexto, parametros, aplicarJoin);

            registro.Id = leido.Id;
            return registro.Modificar(contexto, parametros, aplicarJoin);
        }

        public static T PersistirPorPropiedades<T>(this T registro, ContextoSe contexto, List<string> propiedades, bool errorSiYaExiste = false, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            if (typeof(T).HeredaDe(typeof(EstadoDtm)))
                throw new Exception($"El método {nameof(PersistirPorPropiedades)} no acepta un resgistro del tipo {typeof(T).Name} Use el método {nameof(ExtensorDeEstados.PersistirEstado)}");

            if (typeof(T).HeredaDe(typeof(TransicionDtm)))
                throw new Exception($"El método  {nameof(PersistirPorPropiedades)} no acepta un resgistro del tipo {typeof(T).Name} Use el método {nameof(ExtensorDeTransiciones.PersistirTransicion)}");

            if (registro.Id > 0)
                return registro.Modificar(contexto);

            var filtros = new Dictionary<string, object>();
            foreach (var propiedad in propiedades) filtros[propiedad] = registro.LeerPropiedad(propiedad);

            var leidos = contexto.SeleccionarTodos<T>(filtros);
            if (leidos.Count > 1)
                throw new Exception($"No se puede persistir el objeto de la clase {typeof(T).Name} ya que existen en la BD más de un registro que cumnplen con el criterio dado");

            if (leidos.Count == 0)
                return registro.Insertar(contexto, parametros, aplicarJoin);

            registro.Id = leidos[0].Id;
            return registro.Modificar(contexto, parametros, aplicarJoin);
        }

        public static T InsertarSiNoExiste<T>(this T registro, ContextoSe contexto, List<string> propiedades, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            if (typeof(T).HeredaDe(typeof(EstadoDtm)))
                throw new Exception($"El método {nameof(InsertarSiNoExiste)} no acepta un resgistro del tipo {typeof(T).Name} Use el método {nameof(ExtensorDeEstados.PersistirEstado)}");

            if (typeof(T).HeredaDe(typeof(TransicionDtm)))
                throw new Exception($"El método  {nameof(InsertarSiNoExiste)} no acepta un resgistro del tipo {typeof(T).Name} Use el método {nameof(ExtensorDeTransiciones.PersistirTransicion)}");

            if (registro.Id > 0) registro.Id = 0;

            var filtros = new Dictionary<string, object>();
            foreach (var propiedad in propiedades) filtros[propiedad] = registro.LeerPropiedad(propiedad);

            var leidos = contexto.SeleccionarTodos<T>(filtros);
            if (leidos.Count > 1)
                throw new Exception($"No se puede crear el objeto de la clase {typeof(T).Name} ya que existen en la BD más de un registro que cumnplen con el criterio dado");

            if (leidos.Count == 0)
                return registro.Insertar(contexto, parametros, aplicarJoin);

            return leidos[0];
        }

        public static T InsertarComoAdministradorSiNoExiste<T>(this T registro, ContextoSe contexto, List<string> propiedades, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            var usuarioDeConexion = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
            var otorgado = usuarioDeConexion.OtorgarAdministrador(contexto);
            try
            {
                return InsertarSiNoExiste(registro, contexto, propiedades, aplicarJoin, parametros);
            }
            finally
            {
                usuarioDeConexion.AnularAdministrador(contexto, otorgado);
            }
        }

        public static T InsertarComoAdministrador<T>(this T registro, ContextoSe contexto, Dictionary<string, object> parametros = null, bool aplicarJoin = false, string accionEjecutada = null)
        where T : RegistroDtm
        {
            var usuarioDeConexion = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
            var otorgado = usuarioDeConexion.OtorgarAdministrador(contexto);
            try
            {
                if (parametros == null) parametros = new Dictionary<string, object>();
                if (accionEjecutada != null) parametros[ltrParametrosNeg.AccionQueSeEjecuta] = accionEjecutada;

                return Insertar(registro, contexto, parametros, aplicarJoin);
            }
            finally
            {
                usuarioDeConexion.AnularAdministrador(contexto, otorgado);
            }
        }

        public static T Insertar<T>(this T registro, ContextoSe contexto, string accionEjecutada, Dictionary<string, object> parametros = null, bool aplicarJoin = false)
        where T : RegistroDtm
        {
            if (parametros == null) parametros = new Dictionary<string, object>();
            parametros[ltrParametrosNeg.AccionQueSeEjecuta] = accionEjecutada;

            return Insertar(registro, contexto, parametros, aplicarJoin);
        }

        public static IElementoDtm Insertar(this enumNegocio negocio,  ContextoSe contexto, IElementoDtm registro, Dictionary<string, object> parametros = null, bool aplicarJoin = false)
        {
            registro.Id = 0;
            return (IElementoDtm)negocio.CrearGestor(contexto).PersistirRegistro(registro, parametros == null
                   ? new ParametrosDeNegocio(enumTipoOperacion.Insertar, aplicarJoin)
                   : new ParametrosDeNegocio(enumTipoOperacion.Insertar, parametros, aplicarJoin));
        }


        public static T InsertarSinValidarPermisos<T>(this T registro, ContextoSe contexto, Dictionary<string, object> parametros = null, bool aplicarJoin = false)
        where T : RegistroDtm
        {
            if (parametros is null)
                parametros = new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } };
            else
                parametros[ltrParametrosNeg.ValidarPermisosDePersistencia] = false;

            return registro.Insertar(contexto, parametros, aplicarJoin);
        }


        public static T Insertar<T>(this T registro, ContextoSe contexto, Dictionary<string, object> parametros = null, bool aplicarJoin = false)
        where T : RegistroDtm
        {
            registro.Id = 0;

            if (typeof(T).HeredaDe(typeof(TrazaDtm)))
                return (T)ExtensorDeTrazasDtm.InsertarTraza((ITraza)registro, contexto, parametros);

            if (typeof(T).HeredaDe(typeof(ObservacionDtm)))
                return (T)ExtensorDeObservaciones.InsertarObservacion((IObservacion)registro, contexto, parametros);

            if (typeof(T).HeredaDe(typeof(DireccionDtm)))
                return (T)ExtensorDeDirecciones.InsertarDireccion((IDireccionDtm)registro, contexto, parametros);

            if (typeof(T).ImplementaUnVinculo())
            {
                var referenciados = registro.PropiedadesDelObjeto().Where(x => x.PropertyType.HeredaDe(typeof(RegistroDtm))).ToList();
                var negocio1 = NegociosDeSe.NegocioDeUnTipoDtm(referenciados[0].PropertyType);
                var negocio2 = NegociosDeSe.NegocioDeUnTipoDtm(referenciados[1].PropertyType);
                GestorDeVinculos.Vincular(contexto, negocio1, negocio2, ((IVinculoDtm)registro).idElemento1, ((IVinculoDtm)registro).idElemento2, parametros);
                return registro;
            }

            return (T)contexto.CrearGestorDeUnDtm<T>().PersistirRegistro(registro, parametros == null
                   ? new ParametrosDeNegocio(enumTipoOperacion.Insertar, aplicarJoin)
                   : new ParametrosDeNegocio(enumTipoOperacion.Insertar, parametros, aplicarJoin));
        }

        public static T Modificar<T>(this T registro, ContextoSe contexto, string accionEjecutada, Dictionary<string, object> parametros = null, bool aplicarJoin = false)
        where T : RegistroDtm
        {
            if (parametros == null) parametros = new Dictionary<string, object>();
            parametros[ltrParametrosNeg.AccionQueSeEjecuta] = accionEjecutada;

            return (T)contexto.CrearGestorDeUnDtm<T>().PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Modificar, parametros, aplicarJoin));
        }


        public static T ModificarComoAdministrador<T>(this T registro, ContextoSe contexto, Dictionary<string, object> parametros = null, bool aplicarJoin = false, bool esUnaAccion = false, string accionQueSeEjecuta = "")
        where T : RegistroDtm
        {
            var usuarioDeConexion = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
            var otorgado = usuarioDeConexion.OtorgarAdministrador(contexto);
            try
            {
                if (parametros == null) parametros = new Dictionary<string, object>();

                if (!accionQueSeEjecuta.IsNullOrEmpty())
                    parametros[ltrParametrosNeg.AccionQueSeEjecuta] = accionQueSeEjecuta;

                if (!parametros.LeerValor(ltrParametrosNeg.AccionQueSeEjecuta, "").IsNullOrEmpty())
                    esUnaAccion = true;

                return registro.Modificar(contexto, parametros, aplicarJoin, esUnaAccion);
            }
            finally
            {
                usuarioDeConexion.AnularAdministrador(contexto, otorgado);
            }
        }


        public static T Modificar<T>(this T registro, ContextoSe contexto, Dictionary<string, object> parametros = null, bool aplicarJoin = false, bool esUnaAccion = false)
        where T : RegistroDtm
        {
            if (parametros == null) parametros = new Dictionary<string, object>();

            if (esUnaAccion)
                parametros[ltrParametrosNeg.EstaEjecutandoUnaAccion] = esUnaAccion;

            return (T)contexto.CrearGestorDeUnDtm<T>().PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Modificar, parametros, aplicarJoin));
        }

        public static E ModificarDto<E, T>(this E elemento, ContextoSe contexto, Dictionary<string, object> parametros = null)
        where E : ElementoDto
        where T : RegistroDtm
        =>
        (E)NegociosDeSe.CrearGestor(contexto, typeof(T), typeof(E)).PersistirElementoDto(elemento, new ParametrosDeNegocio(enumTipoOperacion.Modificar, parametros));

        public static E ModificarDto<E>(this E elemento, ContextoSe contexto, Dictionary<string, object> parametros = null)
        where E : ElementoDto
        =>
        (E)NegociosDeSe.CrearGestor(contexto, ExtensionesDto.TipoDtm<E>(), typeof(E)).PersistirElementoDto(elemento, new ParametrosDeNegocio(enumTipoOperacion.Modificar, parametros));


        public static E InsertarDto<E, T>(this E elemento, ContextoSe contexto, Dictionary<string, object> parametros = null)
        where E : ElementoDto
        where T : RegistroDtm
        =>
        (E)NegociosDeSe.CrearGestor(contexto, typeof(T), typeof(E)).PersistirElementoDto(elemento, new ParametrosDeNegocio(enumTipoOperacion.Insertar, parametros));

        public static E InsertarDto<E>(this E elemento, ContextoSe contexto, Dictionary<string, object> parametros = null)
        where E : ElementoDto
        =>
        (E)NegociosDeSe.CrearGestor(contexto, ExtensionesDto.TipoDtm<E>(), typeof(E)).PersistirElementoDto(elemento, new ParametrosDeNegocio(enumTipoOperacion.Insertar, parametros));


        public static T EliminarComoAdministrador<T>(this T registro, ContextoSe contexto, Dictionary<string, object> parametros = null, string accionEjecutada = null)
        where T : RegistroDtm
        {
            var usuarioDeConexion = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
            var otorgado = usuarioDeConexion.OtorgarAdministrador(contexto);
            try
            {
                if (parametros == null) parametros = new Dictionary<string, object>();
                if (accionEjecutada !=null) parametros[ltrParametrosNeg.AccionQueSeEjecuta] = accionEjecutada;

                return registro.Eliminar<T>(contexto, parametros, esUnaAccion: true);
            }
            finally
            {
                usuarioDeConexion.AnularAdministrador(contexto, otorgado);
            }
        }

        public static T Eliminar<T>(this T registro, ContextoSe contexto, Dictionary<string, object> parametros = null, bool esUnaAccion = false)
        where T : RegistroDtm
        {
            if (parametros == null) parametros = new Dictionary<string, object>();

            if (esUnaAccion)
                parametros[ltrParametrosNeg.EstaEjecutandoUnaAccion] = esUnaAccion;

            if (typeof(T).ImplementaUnVinculo())
            {
                var referenciados = registro.PropiedadesDelObjeto().Where(x => x.PropertyType.HeredaDe(typeof(RegistroDtm))).ToList();
                var negocio1 = NegociosDeSe.NegocioDeUnTipoDtm(referenciados[0].PropertyType);
                var negocio2 = NegociosDeSe.NegocioDeUnTipoDtm(referenciados[1].PropertyType);
                GestorDeVinculos.BorrarVinculo(contexto, negocio1, negocio2, ((IVinculoDtm)registro).idElemento1, ((IVinculoDtm)registro).idElemento2, parametros);
                return registro;
            }

            return (T)contexto.CrearGestorDeUnDtm<T>().PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Eliminar, parametros));
        }



        public static T EliminarPorId<T>(this ContextoSe contexto, int id, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            var seleccionado = contexto.SeleccionarPorId<T>(id, errorSiNoHay: false);
            if (seleccionado != null)
            {
                seleccionado = seleccionado.Eliminar(contexto, parametros);
            }
            return seleccionado;
        }

        public static T EliminarPorAk<T>(this ContextoSe contexto, Dictionary<string, object> ak, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            var seleccionado = contexto.SeleccionarPorAk<T>(ak, errorSiNoHay: false, aplicarJoin: false);
            if (seleccionado != null)
            {
                seleccionado = seleccionado.Eliminar(contexto, parametros);
            }
            return seleccionado;
        }

        public static List<T> EliminarTodos<T>(this ContextoSe contexto, Dictionary<string, object> filtros, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            var seleccionados = contexto.SeleccionarTodos<T>(filtros, parametros: parametros);
            foreach (var seleccionado in seleccionados)
                seleccionado.Eliminar(contexto, parametros);
            return seleccionados;
        }

    }
}
