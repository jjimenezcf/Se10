using ModeloDeDto;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using Utilidades;

namespace GestorDeElementos
{
    public enum enumTipoOperacion { Insertar, Modificar, LeerConBloqueo, LeerSinBloqueo, NoDefinida, Eliminar, Contar, Transitar, MapearElDtoAlDtm, MapearElDtmAlDto };

    public static class ltrParametrosNeg
    {
        public static readonly string NombreDelFicheroParaTrazarLaConsulta = nameof(NombreDelFicheroParaTrazarLaConsulta);
        public static readonly string AplicarJoin = nameof(AplicarJoin);
        public static readonly string AplicarElOrden = nameof(AplicarElOrden);
        public static readonly string IncluirBajas = nameof(IncluirBajas);
        public static readonly string FiltroPorId = nameof(FiltroPorId);
        public static readonly string HayFiltroPorTipo = nameof(HayFiltroPorTipo);
        public static readonly string FiltroPorNombreIgual = nameof(FiltroPorNombreIgual);
        public static readonly string IncluirCancelados = nameof(IncluirCancelados);
        public static readonly string FiltrarPorBaja = nameof(FiltrarPorBaja);
        public static readonly string FiltrarPorBloqueo = nameof(FiltrarPorBloqueo);
        public static readonly string SoloEnAlta = nameof(SoloEnAlta);
        public static readonly string CantidadLeida = nameof(CantidadLeida);
        public static readonly string CantidadPorLeer = nameof(CantidadPorLeer);
        public static readonly string PosicionInicial = nameof(PosicionInicial);
        public static readonly string JoinConUsuarioDtm = nameof(JoinConUsuarioDtm);
        public static readonly string QueMostrar = nameof(QueMostrar);
        public static readonly int MostrarBajas = 1;
        public static readonly int MostrarBloqueados = 1;
        public static readonly int Cancelados = 2;
        public static readonly int Terminados = 3;
        public static readonly int Iniciales = 4;
        public static readonly int ConRelacion = 5;
        public static readonly int SinRelacion = ltrFiltros.SinRelacion.Entero();
        public static readonly int TodosMenosCanceladas = 8;
        public static readonly int MostrarTodos = 9;
        public static readonly int ConValores = 9;
        public static readonly int SinValores = 9;
        public static readonly string ExcluirCancelados = nameof(ExcluirCancelados);
        public static readonly string ExcluirTerminados = nameof(ExcluirTerminados);
        public static readonly string IncluirDetalles = nameof(IncluirDetalles);
        public static readonly string Negocio = nameof(Negocio);
        public static readonly string IdNegocio = nameof(IdNegocio);
        public static readonly string IdElemento = nameof(IdElemento);
        public static readonly string Elemento = nameof(Elemento);
        public static readonly string Vinculado = nameof(Vinculado);
        public static readonly string IdVinculado = nameof(IdVinculado);
        public static readonly string InsertandoParaVincular = nameof(InsertandoParaVincular);
        public static readonly string Copiando = nameof(Copiando);
        public static readonly string ErrorSiNoLoHay = nameof(ErrorSiNoLoHay);
        public static readonly string ErrorSiEstaVinculado = nameof(ErrorSiEstaVinculado);
        public static readonly string ValidarTrazaDeBloqueo = nameof(ValidarTrazaDeBloqueo);
        public static readonly string ValidarPermisosDePersistencia = nameof(ValidarPermisosDePersistencia);
        public static readonly string ValidarPermisosDeConsulta = nameof(ValidarPermisosDeConsulta);
        public static readonly string ValidarEtapaDocumental = nameof(ValidarEtapaDocumental);
        public static readonly string EstaEjecutandoUnaAccion = nameof(EstaEjecutandoUnaAccion);
        public static readonly string IdSemaforo = nameof(IdSemaforo);
        public static readonly string ObtenerCertificado = nameof(ObtenerCertificado);
        public static readonly string solicitadoPorLaCola = nameof(solicitadoPorLaCola);
        public static readonly string Firmar = nameof(Firmar);
        public static readonly string IncluirOriginales = nameof(IncluirOriginales);
        public static readonly string CopiaSeguridad = nameof(CopiaSeguridad);
        public static readonly string UsarLaCache = nameof(UsarLaCache);
        public static readonly string EnConsulta = nameof(EnConsulta);
        public static readonly string EsUnaPeticion = nameof(EsUnaPeticion);
        public static readonly string RegistroAntesTransitar = nameof(RegistroAntesTransitar);
        public static readonly string Peticion = nameof(Peticion);
        public static readonly string TransionPendienteDeEjecucion = nameof(TransionPendienteDeEjecucion);
        public static readonly string ColumnasDelGrid = enumParametro.ColumnasDelGrid;
        public static readonly string AmpliacionesSolicitadas = nameof(AmpliacionesSolicitadas);
        public static readonly string AplicarFiltroPorEstado = nameof(AplicarFiltroPorEstado);
        public static readonly string EstadoOrigen = nameof(EstadoOrigen);
        public static readonly string EstadoDestino = nameof(EstadoDestino);
        public static readonly string EsUnaTransicion = nameof(EsUnaTransicion);
        public const string ModificoParaTransitar = nameof(ModificoParaTransitar);
        public static readonly string NoTransitarSiYaLoEsta = nameof(NoTransitarSiYaLoEsta);
        public static readonly string ObtenerDatosFiscales = nameof(ObtenerDatosFiscales);
        public static readonly string LeerDireccionDeContacto = nameof(LeerDireccionDeContacto);
        public static readonly string ModoAccesoLeido = nameof(ModoAccesoLeido);
        public static readonly string AccionQueSeEjecuta = nameof(AccionQueSeEjecuta);
        public static readonly string CrearPermisosDelElemento = nameof(CrearPermisosDelElemento);
        public static readonly string EstoyLeyendoParaAnalizarElModoDeAcceso = nameof(EstoyLeyendoParaAnalizarElModoDeAcceso);
        public static readonly string FechaDeCreacion = nameof(FechaDeCreacion);
        public static readonly string FechaDeTransicion = nameof(FechaDeTransicion);
    }

    public enum enumPeticion
    {
        epLeerAnexados, epLeerPorId, epPersistirAmpliacion, epLeerDatosParaElGrid, epLeerElementos, epLeerElemento, epCrearRelacion, epCrearRelaciones, epProcesarOpcionMf,
        epModificarRelacion, epBorrarRelacionPorId, epBorrarPorId, PersistirElemento, epCrearVinculo, epBorrarVinculo, epTransitar, epExportar, LeerEventos, epModificarPorId, epImprimir
        , epDarDeBaja, epDarDeAlta, epSinPeticion, epImputar, epCrearDetalle, epTotales, epQuitarAnexado, epAnularFirma
    }

    public static class TipoOperacion
    {
        public static enumTipoOperacion ToTipoOperacion(this object tipo)
        {
            switch (tipo.ToString())
            {
                case nameof(enumTipoOperacion.Insertar): return enumTipoOperacion.Insertar;
                case nameof(enumTipoOperacion.Modificar): return enumTipoOperacion.Modificar;
                case nameof(enumTipoOperacion.LeerConBloqueo): return enumTipoOperacion.LeerConBloqueo;
                case nameof(enumTipoOperacion.LeerSinBloqueo): return enumTipoOperacion.LeerSinBloqueo;
                case nameof(enumTipoOperacion.NoDefinida): return enumTipoOperacion.NoDefinida;
                case nameof(enumTipoOperacion.Eliminar): return enumTipoOperacion.Eliminar;
                case nameof(enumTipoOperacion.Contar): return enumTipoOperacion.Contar;
            }

            throw new Exception($"No se ha definido el tipo de operación {tipo}");
        }
        public static string ToBd(this enumTipoOperacion tipo)
        {
            switch (tipo)
            {
                case enumTipoOperacion.Insertar: return "I";
                case enumTipoOperacion.Modificar: return "M";
                case enumTipoOperacion.LeerConBloqueo: return "L";
                case enumTipoOperacion.LeerSinBloqueo: return "X";
                case enumTipoOperacion.NoDefinida: return "N";
                case enumTipoOperacion.Eliminar: return "E";
                case enumTipoOperacion.Contar: return "C";
                case enumTipoOperacion.Transitar: return "T";
            }

            throw new Exception($"No se ha definidocomo registrar en la BD la operación {tipo}");
        }
    }

    public class ClausulaDeJoin
    {
        public Type Dtm { get; set; }
    }


    public class enumParametro
    {
        public static string accion = nameof(accion);
        public static string filtro = nameof(filtro);
        public static string ColumnasDelGrid = "columnasopcionales";
    }


    public class ParametrosDeNegocio
    {
        public enumTipoOperacion Operacion { get; private set; }
        public bool Traquear { get; set; } = false;

        public bool AplicarJoin
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.AplicarJoin, false); }
            set { Parametros[ltrParametrosNeg.AplicarJoin] = value; }
        }
        public bool EstaEjecutandoUnaAccion
        {
            get { return !AccionQueSeEjecuta.IsNullOrEmpty() || (bool)Parametros.LeerValor(ltrParametrosNeg.EstaEjecutandoUnaAccion, false); }
            set { Parametros[ltrParametrosNeg.EstaEjecutandoUnaAccion] = value; }
        }

        public string AccionQueSeEjecuta
        {
            get { return (string)Parametros.LeerValor(ltrParametrosNeg.AccionQueSeEjecuta, ""); }
            set { Parametros[ltrParametrosNeg.AccionQueSeEjecuta] = value; }
        }

        public bool CargarLista
        {
            get { return CargarListaDinamica || CargarListaDeElementos; }
        }

        public bool CargarListaDinamica
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosEp.CargarListaDinamica, false); }
        }

        public bool LeerPorIdParaEditar
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosEp.LeerPorIdParaEditar, false);  }
        }

        public bool BorrarRelacion
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosEp.BorrarRelacion, false); }
        }

        public bool CreandoEnCrud
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosEp.CreandoEnCrud, false); }
        }

        public bool CargarListaDeElementos
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosEp.CargarListaDeElementos, false); }
        }
        public bool InsertandoParaVincular
        {
            get { return Parametros.LeerValor(ltrParametrosNeg.InsertandoParaVincular, false); }
            set { Parametros[ltrParametrosNeg.InsertandoParaVincular] = value; }
        }

        public bool? ExcluirCancelados
        {
            get { return Parametros.LeerValor(ltrParametrosNeg.ExcluirCancelados, (bool?)null); }
            set { Parametros[ltrParametrosNeg.ExcluirCancelados] = value; }
        }

        public bool? ExcluirTerminados
        {
            get { return Parametros.LeerValor(ltrParametrosNeg.ExcluirTerminados, (bool?)null); }
            set { Parametros[ltrParametrosNeg.ExcluirTerminados] = value; }
        }

        public bool ValidarPermisosDePersistencia
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDePersistencia, EstaEjecutandoUnaAccion ? false : true); }
            set { Parametros[ltrParametrosNeg.ValidarPermisosDePersistencia] = value; }
        }

        public bool ValidarTrazaDeBloqueo
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.ValidarTrazaDeBloqueo, true); }
            set { Parametros[ltrParametrosNeg.ValidarTrazaDeBloqueo] = value; }
        }

        public bool Firmar
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.Firmar, false); }
            set { Parametros[ltrParametrosNeg.Firmar] = value; }
        }

        public bool ValidarPermisosDeConsulta
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.ValidarPermisosDeConsulta, EstaEjecutandoUnaAccion ? false : true); }
            set { Parametros[ltrParametrosNeg.ValidarPermisosDeConsulta] = value; }
        }

        public bool LeerDatosParaElGridOParaExportar
        {
            get
            {
                var peticion = Parametros.LeerValor(ltrParametrosNeg.Peticion, enumPeticion.epSinPeticion);
                return peticion == enumPeticion.epLeerDatosParaElGrid || peticion == enumPeticion.epExportar;
            }
        }
        public bool LeerPorId => Parametros.LeerValor(ltrParametrosNeg.Peticion, enumPeticion.epSinPeticion) == enumPeticion.epLeerPorId;

        public bool IncluirBajas
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.IncluirBajas, false); }
            set { Parametros[ltrParametrosNeg.IncluirBajas] = value; }
        }

        public bool FiltroPorId
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.FiltroPorId, false); }
            set { Parametros[ltrParametrosNeg.FiltroPorId] = value; }
        }

        public bool HayFiltroPorTipo
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.HayFiltroPorTipo, false); }
            set { Parametros[ltrParametrosNeg.HayFiltroPorTipo] = value; }
        }

        public bool FiltroPorNombreIgual
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.FiltroPorNombreIgual, false); }
            set { Parametros[ltrParametrosNeg.FiltroPorNombreIgual] = value; }
        }

        public bool EnConsulta
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.EnConsulta, false); }
            set { Parametros[ltrParametrosNeg.EnConsulta] = value; }
        }

        public TipoDeElementoDtm TipoDeElemento
        {
            get { return Parametros.LeerValor(nameof(TipoDeElementoDtm), (TipoDeElementoDtm)null); }
            set { Parametros[nameof(TipoDeElementoDtm)] = value; }
        }

        public TipoConFlujoDtm TipoConFujo
        {
            get { return Parametros.LeerValor(nameof(TipoConFlujoDtm), (TipoConFlujoDtm)null); }
            set { Parametros[nameof(TipoConFlujoDtm)] = value; }
        }

        public bool EsUnaPeticion
        {
            get
            {
                if (EstaEjecutandoUnaAccion) return false;
                return Peticion != enumPeticion.epSinPeticion ? true : (bool)Parametros.LeerValor(ltrParametrosNeg.EsUnaPeticion, false);
            }
            set { Parametros[nameof(ltrParametrosNeg.EsUnaPeticion)] = value; }
        }

        public DateTime FechaDeCreacion
        {
            get => Parametros.LeerValor<DateTime>(ltrParametrosNeg.FechaDeCreacion, default);
            set { Parametros[nameof(ltrParametrosNeg.FechaDeCreacion)] = value; }
        }

        public bool EsUnaTransicion
        {
            get => (bool)Parametros.LeerValor(ltrParametrosNeg.EsUnaTransicion, false);
            set { Parametros[nameof(ltrParametrosNeg.EsUnaTransicion)] = value; }
        }

        public TransicionDtm TransicionQueSeEjecuta
        {
            get => Parametros.LeerValor<TransicionDtm>(nameof(TransicionDtm), null);
            set { Parametros[nameof(TransicionDtm)] = value; }
        }

        public bool AplicarFiltroQueMostrar
        {
            get { return (bool)Parametros.LeerValor(ltrParametrosNeg.AplicarFiltroPorEstado, true); }
            set { Parametros[nameof(ltrParametrosNeg.AplicarFiltroPorEstado)] = value; }
        }

        public enumPeticion Peticion
        {
            get
            {
                if (EstaEjecutandoUnaAccion)
                    return enumPeticion.epSinPeticion;
                return !Parametros.ContainsKey(ltrParametrosNeg.Peticion) ? enumPeticion.epSinPeticion : Parametros.LeerValor<enumPeticion>(ltrParametrosNeg.Peticion);
            }
            set
            {
                EsUnaPeticion = true;
                Parametros[nameof(ltrParametrosNeg.Peticion)] = value;
            }
        }

        public bool EstoyExportando
        {
            get
            {
                return Parametros.ContainsKey(ltrParametrosNeg.Peticion) && Parametros.LeerValor<enumPeticion>(ltrParametrosNeg.Peticion) == enumPeticion.epExportar;
            }
        }

        public bool EstoyTotalizando
        {
            get
            {
                return Parametros.ContainsKey(ltrParametrosNeg.Peticion) && Parametros.LeerValor<enumPeticion>(ltrParametrosNeg.Peticion) == enumPeticion.epTotales;
            }
        }

        public string FiltrarPara
        {
            get
            {
                return !Parametros.ContainsKey(ltrParametrosEp.filtrarPara) ? "" : Parametros.LeerValor<string>(ltrParametrosEp.filtrarPara);
            }
        }

        public bool CargarGridDeRelacion => FiltrarPara == ltrFiltros.CargarGridDeRelacion;


        public List<string> ColumnasDelGrid => Parametros.LeerValor(ltrParametrosNeg.ColumnasDelGrid, new List<string>());

        public List<string> AmpliacionesSolicitadas => Parametros.ContainsKey(ltrParametrosNeg.AmpliacionesSolicitadas)
            ? JsonConvert.DeserializeObject<List<string>>((string)Parametros[ltrParametrosNeg.AmpliacionesSolicitadas])
            : new List<string>();

        public List<ClausulaDeFiltrado> Filtros { get; set; }

        private Dictionary<string, object> _parametros = new Dictionary<string, object>();
        public Dictionary<string, object> Parametros
        {
            get { return _parametros; }
            set
            {
                if (value == null)
                    _parametros.Clear();
                else
                    foreach (var clave in value.Keys)
                        _parametros[clave] = value[clave];
            }
        }
        public IRegistro registroEnBd = null;

        public LibroDtm Libro = null;


        public bool EstaDandoDeBaja(IUsaBaja registro) => ((IUsaBaja)registroEnBd) is not null && Modificando && !((IUsaBaja)registroEnBd).Baja && registro.Baja;
        public bool EstaDandoDeAlta(IUsaBaja registro) => ((IUsaBaja)registroEnBd) is not null && Modificando && ((IUsaBaja)registroEnBd).Baja && !registro.Baja;


        public bool EstaActivando(IUsaActiva registro) => Modificando && !((IUsaActiva)registroEnBd).Activa && registro.Activa;
        public bool EstaDesactivando(IUsaActiva registro) => Modificando && ((IUsaActiva)registroEnBd).Activa && !registro.Activa;

        public bool ProcesandoTransicion(IUsaEstado registro) => Modificando &&
            (Transitando
            || Parametros.LeerValor(ltrParametrosNeg.ModificoParaTransitar, false)
            || (((IUsaEstado)registroEnBd) != null && ((IUsaEstado)registroEnBd).IdEstado != registro.IdEstado)
            );
        public bool Insertando => Operacion == enumTipoOperacion.Insertar;
        public bool Modificando => Operacion == enumTipoOperacion.Modificar;
        public bool Eliminando => Operacion == enumTipoOperacion.Eliminar;
        public void Eliminar()
        {
            Operacion = enumTipoOperacion.Eliminar;
        }
        public bool Transitando => EsUnaTransicion
            || Operacion == enumTipoOperacion.Transitar
            || Parametros.LeerValor(ltrParametrosNeg.Peticion, enumPeticion.epSinPeticion) == enumPeticion.epTransitar;

        public EstadoDtm EstadoDestino => EsUnaTransicion && Parametros.ContainsKey(nameof(TransicionDtm)) ? Parametros.LeerValor<EstadoDtm>(ltrParametrosNeg.EstadoDestino) : null;

        public bool OrdenarPorFechaDeCreacionDesc { get; set; } = false;

        public bool Copiando => Insertando && Parametros.LeerValor(ltrParametrosNeg.Copiando, false);

        public enumModoDeAccesoDeDatos PermisosNecesariosParaVincular { get; set; } = enumModoDeAccesoDeDatos.Gestor;

        public ParametrosDeNegocio(enumTipoOperacion tipo, bool aplicarJoin = false)
        {
            Operacion = tipo;

            if (tipo == enumTipoOperacion.LeerConBloqueo)
                Traquear = true;

            if (tipo == enumTipoOperacion.LeerSinBloqueo)
                Traquear = false;

            AplicarJoin = aplicarJoin;
        }

        public ParametrosDeNegocio(enumTipoOperacion tipo, Dictionary<string, object> parametros, bool aplicarJoin = false)
        : this(tipo, aplicarJoin)
        {
            if (parametros != null)
                Parametros = parametros;
        }

        public bool HacerJoinCon(string join)
        {
            if (!AplicarJoin)
                return false;

            if (!Parametros.ContainsKey(join))
                return true;
            return (bool)Parametros[join];
        }
    }

}
