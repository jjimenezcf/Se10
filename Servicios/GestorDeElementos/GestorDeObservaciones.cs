using AutoMapper;
using Gestor.Errores;
using GestorDeElementos.Extensores;
using Microsoft.Win32;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestorDeElementos
{
    public class GestorDeObservaciones : GestorDeElementos<ContextoSe, ObservacionDtm, ObservacionDto>
    {
        public string _Tabla { get; }

        private enumNegocio _Negocio;

        public override enumNegocio Negocio => _Negocio;

        private IGestor gestor => NegociosDeSe.CrearGestor(Contexto, Negocio);


        public class ltrObservaciones
        {
        }

        public class MapearObservaciones : Profile
        {
            public MapearObservaciones()
            {
                CreateMap<ObservacionDtm, ObservacionDto>();
                CreateMap<ObservacionDto, ObservacionDtm>();
            }
        }

        public GestorDeObservaciones(ContextoSe contexto, enumNegocio negocio)
        : base(contexto, contexto.Mapeador)
        {
            _Tabla = ApiDeElementoDtm.TablaDeObservacion(negocio.TipoDtm());
            _Negocio = negocio;
            Contexto = contexto;
        }

        public static GestorDeObservaciones Gestor(ContextoSe contexto, enumNegocio negocio) => new GestorDeObservaciones(contexto, negocio);

        public ObservacionDtm LeerRegistroPorId(int id, bool emitirError = true)
            => ObservacionSql.LeerPorId(Contexto, _Tabla, id, emitirError);

        public IEnumerable<ObservacionDtm> LeerRegistros(int idElemento, int posicion, int cantidad)
            => ObservacionSql.ObservacionesDeUnElemento(Contexto, _Tabla, idElemento, posicion, cantidad);

        public int ContarRegistros(int idElemento)
            => ObservacionSql.ContarRegistros(Contexto, _Tabla, idElemento);

        public override ObservacionDtm LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, bool errorSiNoHay, Dictionary<string, object> parametros = null)
        {
            return LeerRegistroPorId(id);
        }


        public new IEnumerable<ObservacionDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> parametros)
        {
            int idElemento = 0;
            foreach (var f in filtros.Where(f => f.Clausula == nameof(ObservacionDtm.IdElemento)))
            {
                idElemento = f.Valor.Entero();
            }

            var registros = LeerRegistros(idElemento, posicion, cantidad);
            return MapearElementos(registros.ToList());
        }

        protected override void DespuesDeMapearElRegistro(ObservacionDto elemento, ObservacionDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);
            if (elemento.IdArchivo.Entero() > 0)
            {
                opciones.Parametros[nameof(IUsaArchivo.IdArchivo)] = elemento.IdArchivo;
            }

        }

        protected override void AntesDePersistir(ObservacionDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (Negocio.UsaBaja())
            {
                var elemento = gestor.LeerRegistroPorId(registro.IdElemento, false);
                if (((IUsaBaja)elemento).Baja)
                    GestorDeErrores.Emitir("No se pueden añadir observaciones a un elemento de baja.");
            }

            if (registro.Descripcion.Length > IDominio.Longitud(IDominio.VARCHAR_2000))
                GestorDeErrores.Emitir($"La propiedad '{nameof(ObservacionDtm.Descripcion)}' tiene una longitud de '{registro.Descripcion.Length}', y ésta excede de '{IDominio.Longitud(IDominio.VARCHAR_2000)}', longitud permitida en la base de datos.");

            if (registro.Nombre.Length > IDominio.Longitud(IDominio.VARCHAR_250))
                GestorDeErrores.Emitir($"La propiedad '{nameof(ObservacionDtm.Nombre)}' tiene una longitud de '{registro.Nombre.Length}', y ésta excede de '{IDominio.Longitud(IDominio.VARCHAR_250)}', longitud permitida en la base de datos.");

        }
        protected override void Persistir(ObservacionDtm registro, ParametrosDeNegocio parametros)
        {
            var idUsuario = parametros.Parametros.LeerValor(ltrDeObservaciones.CreadaPorAdminSe, false) ? Contexto.Administrador().Id : Contexto.DatosDeConexion.IdUsuario;
            var elemento = (IElementoDtm)Negocio.LeerRegistro(Contexto, registro.IdElemento);
            var idTipo = Negocio.UsaTipo() ? ((IUsaTipo)elemento).IdTipo : 0;
            var permitirSiTerminado = parametros.Parametros.LeerValor(ltrDeObservaciones.PermitirSiTerminado, false) || Negocio.PermitirObservacionesSiTerminado(idTipo, Contexto);
            if (Negocio.UsaEstado() && !permitirSiTerminado)
            {
                var estado = ((IElementoDeProcesoDtm)elemento).Estado(Contexto);
                if (!parametros.EsUnaTransicion && (estado.Cancelado || estado.Terminado))
                {
                    GestorDeErrores.Emitir($"El elemento '{((IElementoDeProcesoDtm)elemento).Referencia}' está '{(estado.Terminado ? "Terminado" : "Cancelado")}' no se le pueden '{(parametros.Insertando ? "añadir" : "modificar")}' observaciones, para poder, parametrice en el negocio '{Negocio.Singular()}' el parametro '{nameof(enumParametroDeNegocio.NEG_ObservacionesSiTerminado)}'");
                }
            }

            if (Negocio.UsaBaja() && !permitirSiTerminado)
            {
                if (((IUsaBaja)elemento).Baja)
                {
                    GestorDeErrores.Emitir($"El elemento '{elemento.Referencia(Contexto)}' esta 'de baja' no se le pueden '{(parametros.Insertando ? "añadir" : "modificar")}' observaciones");
                }
            }

            if (Negocio.UsaBloqueos() && !permitirSiTerminado)
            {
                if (((IUsaBaja)elemento).Baja)
                {
                    GestorDeErrores.Emitir($"El elemento '{elemento.Referencia(Contexto)}' esta 'Bloqueado' no se le pueden '{(parametros.Insertando ? "añadir" : "modificar")}' observaciones");
                }
            }

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                var ultimaCreada = ObservacionSql.UltimaCreada(Contexto, _Tabla, registro.IdElemento);
                if (ultimaCreada != null && registro.Nombre == ultimaCreada.Nombre && ultimaCreada.Descripcion == registro.Descripcion)
                {
                    GestorDeErrores.Emitir($"Ya ha creado con fecha '{ultimaCreada.CreadaEl.ToString(extFechas.DiaHora)}' una observación para el mismo elemento con la misma información");
                }
                registro.Id = ObservacionSql.Insertar(Contexto, _Tabla, registro.IdElemento, registro.Nombre, registro.Descripcion, idUsuario).Id;
                return;
            }
            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                if (registro.IdCreador != Contexto.DatosDeConexion.IdUsuario)
                    GestorDeErrores.Emitir("No se pueden modificar las observaciones no añadidas por Ud.");

                if (Negocio.UsaTrazas()) parametros.registroEnBd = ObservacionSql.LeerPorId(Contexto, _Tabla, registro.Id);
                ObservacionSql.Modificar(Contexto, _Tabla, registro.Id, registro.Descripcion);

                if (Negocio.UsaTrazas()) GestorDeTrazas.ObservacionModificada(Contexto, Negocio, registro, (ObservacionDtm)parametros.registroEnBd);
                return;
            }
            GestorDeErrores.Emitir($"La operacion {parametros.Operacion} no está permitida para las observaciones del negocio {Negocio.ToNombre()}");
        }

        protected override void DespuesDePersistir(ObservacionDtm observacion, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(observacion, parametros);
            var elemento = (ElementoDtm)Negocio.LeerRegistro(Contexto, observacion.IdElemento);
            var responsable = elemento.Responsable(Contexto);
            var receptor = responsable != null && responsable.Id != Contexto.DatosDeConexion.IdUsuario ? responsable : null;

            if (receptor is null && elemento.Modificador(Contexto) is not null && elemento.Modificador(Contexto).Id != Contexto.DatosDeConexion.IdUsuario)
                receptor = elemento.Modificador(Contexto);

            if (receptor is null && elemento.Creador(Contexto).Id != Contexto.DatosDeConexion.IdUsuario)
                receptor = elemento.Creador(Contexto);

            if (receptor is not null)
            {
                string refHtml = elemento.CrearHref(Contexto);
                var asunto = parametros.Insertando
                ? $"Registro de observación en {Negocio.Singular()}. {observacion.Nombre}"
                : $"Modificación de observación en {Negocio.Singular()}. {observacion.Nombre}";
                Contexto.EnviarCorreoPorAdministrador(CacheDeVariable.Cfg_ServidorDeCorreo, new List<string> { receptor.eMail },
                    asunto,
                    $"{Negocio.Singular()}: {elemento.Expresion}{Simbolos.br}" +
                    observacion.Descripcion + Simbolos.br +
                    "Enlace: " + refHtml
                    );
            }

            var idArchivo = parametros.Parametros.LeerValor(nameof(IUsaArchivo.IdArchivo), 0);
            if (idArchivo > 0)
            {
                var archivo = Contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
                var ext = Path.GetExtension(archivo.Nombre);
                archivo.Nombre = NombreDeFicheroAsociado(observacion) + $"{ext}";
                archivo.ModificarComoAdministrador(Contexto, accionQueSeEjecuta: ltrDeUnArchivo.Accion_Permitir_Modificar_Nombre);
                GestorDeVinculos.Vincular(Contexto, Negocio, enumNegocio.Archivos, observacion.IdElemento, (int)idArchivo, new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
            }
        }

        private static string NombreDeFicheroAsociado(ObservacionDtm observacion)
        {
            return $"Obs.{observacion.Id}-{observacion.Nombre}".Left(240).NormalizarFichero();
        }

        protected override void DespuesDeMapearElElemento(ObservacionDtm observacion, ObservacionDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(observacion, elemento, parametros);
            elemento.IdNegocio = NegociosDeSe.IdNegocio(Negocio);
            elemento.Negocio = NegociosDeSe.ToNombre(Negocio);
            elemento.ModoDeAcceso = Contexto.DatosDeConexion.IdUsuario == elemento.IdCreador ?
                ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Gestor :
                ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;

            if (parametros.LeerPorIdParaEditar)
            {
                var padre = Negocio.LeerRegistro(Contexto, elemento.IdElemento);
                var anexados = padre.LeerAnexados(Contexto);
                var archivo = anexados.FirstOrDefault(a => a.Nombre.StartsWith(NombreDeFicheroAsociado(observacion)));
                if (archivo != null)
                {
                    elemento.GuidDeDescarga = archivo.RegistrarDescargaConGuid(Contexto, caducaEl: null, maximoDeDescargas: null, auditar: false);
                    elemento.NombreDeAccion = $"Descargar: {Contexto.SeleccionarPorId<ArchivoDtm>(archivo.Id).Nombre}";
                    elemento.IdArchivo = archivo.Id;
                }
                else
                {
                    elemento.NombreDeAccion = $"No hay archivo adjunto";
                }
            }

        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(ObservacionDtm registro, ObservacionDto elemento, ParametrosDeNegocio parametros)
        {
            if (parametros.EsUnaTransicion)
                elemento.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
            else
                elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, Negocio, ((IObservacion)registro).IdElemento);
        }

    }

}
