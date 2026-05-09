using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Runtime.Serialization;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ServicioDeDatos.Elemento
{
    public static class ltrPropiedades
    {
        public const string Orden = nameof(Orden);
    }

    public interface IRegistro
    {
        public int Id { get; set; }
    }
    public interface IRowVersion
    {
        public byte[] RowVersion { get; set; }
    }

    public interface INombre : IRegistro
    {
        public string Nombre { get; set; }
        public string Expresion { get; }
    }

    public interface IRegistroDeParametrizacion
    {

    }

    public interface IPlantillaDeUsuario : INombre, ITieneCampoNegocio
    {
        public string Valor { get; set; }
        public int IdUsuario { get; set; }
        public string Vista { get; set; }
    }

    public interface IConPermiso
    {
        public int IdPermiso { get; set; }
        public PermisoDtm Permiso { get; set; }
    }

    public interface IPlantillaConAccion : INombre, IRegistro, IConPermiso, IUsaArchivo
    {
        public int IdAccion { get; set; }
        public AccionDtm Accion { get; set; }
        public string fichero { get; }

        public string NombrePa { get; }
    }

    public interface IPlantillaDeNegocio : IPlantillaConAccion, ITieneCampoNegocio
    {

    }

    public interface IPlantillaPorTipo : IPlantillaConAccion
    {
        public enumNegocio Negocio { get; }
    }

    public class PlantillaDeUsuario : RegistroConNombreDtm, IPlantillaDeUsuario
    {
        public string Valor { get; set; }
        public int IdNegocio { get; set; }
        public int IdUsuario { get; set; }
        public string Vista { get; set; }
        public virtual NegocioDtm Negocio { get; set; }
        public virtual UsuarioDtm Usuario { get; set; }

    }

    public interface IAuditoria : IRowVersion
    {
        public int IdUsuaCrea { get; set; }
        public int? IdUsuaModi { get; set; }
        public DateTime FechaCreacion { get; set; }
        public UsuarioDtm UsuarioCreador { get; set; }
        public DateTime? FechaModificacion { get; set; }
        public UsuarioDtm UsuarioModificador { get; set; }

    }

    public interface IElementoDtm : IAuditoria, INombre
    {
    }

    public interface ILibroDeRegistro
    {
        public enumClaseDeLibro ClaseDeLibro { get; set; }
        public string Sigla { get; set; }
    }

    public interface ITipoDeElementoDtm : ITipoDtm, ILibroDeRegistro
    {
        public static enumNegocio Negocio { get; }
    }

    public interface IUsaClase
    {
        public int IdClase { get; set; }
        public ClaseDelNegocioDtm Clase { get; set; }
        public static enumNegocio Negocio { get; }
    }

    public interface IClaseDelTipoDtm : IUsaClase, IUsaTipo
    {
    }

    public interface IVinculoDtm : IRegistro
    {
        public int idElemento1 { get; }
        public int idElemento2 { get; }
    }

    public interface IRelacion : IRegistro
    {

        [IgnoreDataMember]
        [NotMapped]
        public string PropiedadDelIdElemento1 { get; set; }

        [IgnoreDataMember]
        [NotMapped]
        public string PropiedadDelIdElemento2 { get; set; }

        [IgnoreDataMember]
        [NotMapped]
        public string PropiedadDelElemento1 { get; }

        [IgnoreDataMember]
        [NotMapped]
        public string PropiedadDelElemento2 { get; }

        [IgnoreDataMember]
        [NotMapped]
        public int IdElemento1 { get; }
        [IgnoreDataMember]
        [NotMapped]
        public int IdElemento2 { get; }
    }

    public interface IPermisoOtorgado
    {
        public int IdPermiso { get; set; }
        public int IdUsuario { get; set; }

    }


    public interface ITipoDtm : IRegistro, INombre
    {
        public int? IdPadre { get; set; }

        public int IdPermisoDeGestor { get; set; }

        public int IdPermisoDeConsultor { get; set; }

        public int IdPermisoDeAdministrador { get; set; }

        public PermisoDtm PermisoDeGestor { get; set; }
        public PermisoDtm PermisoDeAdministrador { get; set; }
        public PermisoDtm PermisoDeConsultor { get; set; }
        public bool Activo { get; set; }

        public enumClaseDeLibro ClaseDeLibro { get; set; }
        public string Sigla { get; set; }

        public bool NombreModificable { get; set; }
        public bool PermiteCrear { get; set; }
    }

    public interface IElementoConTipo : IElementoDtm, IUsaTipo, IUsaReferencia, IAuditoria
    {

    }

    public interface IUsaTipo
    {
        public int IdTipo { get; set; }
        public abstract ITipoDtm Tipo { get; }
    }

    public interface IHitoDtm : IRegistro
    {
        public int IdElemento { get; set; }
        public DateTime Fecha { get; set; }
        public int IdEstado { get; set; }
        public int IdUsuario { get; set; }
        public long? Tiempo { get; set; }
        public int? IdTransicion { get; set; }
        public int? IdObservacion { get; set; }
    }
    public interface INecesitaSerParametrizador
    {

    }

    public interface IInstanciaEstado : IEstado
    {
        public static enumNegocio Negocio { get; }
    }

    public interface IEstado : IRegistro, INombre, INecesitaSerParametrizador
    {
        public int Orden { get; set; }
        public int IdPermiso { get; set; }
        public bool Inicial { get; set; }
        public bool Terminado { get; set; }
        public bool Cancelado { get; set; }
    }

    public interface IUsaEstado
    {
        public int IdEstado { get; set; }
        public abstract IEstado Estado { get; }
    }

    public interface IUsaObservacion
    {
    }


    public interface ITransicion : IRegistro, INombre, INecesitaSerParametrizador
    {
        public int IdOrigen { get; set; }
        public int IdDestino { get; set; }
        public bool DelSistema { get; set; }
        public bool ConObservacion { get; set; }
        public string Asunto { get; set; }
        public int IdPermiso { get; set; }
        public bool Activo { get; set; }
    }

    public interface IAccionDe : IRegistro, INecesitaSerParametrizador
    {
        public int IdAccion { get; set; }
        public string Parametros { get; set; }
        public string Descripcion { get; set; }
        public string Momento { get; set; }
        public int Orden { get; set; }
        public bool Activo { get; set; }
    }
    public interface IAccionDeTrn : IAccionDe
    {
        public int IdTransicion { get; set; }
        public string Transicion { get; }
        public string Accion { get; }
    }
    public interface IAccionDeRelacion : IAccionDe
    {
        public int IdNegocio { get; set; }
        public int IdVinculado { get; set; }
    }

    public interface IObservacion : IRegistro, INombre, IUsaDescripcion, IUsaElemento
    {
        public new int IdElemento { get; set; }
        public int IdCreador { get; set; }
        public DateTime CreadaEl { get; set; }
        public enumNegocio Negocio { get; }

    }
    public interface ITraza : IRegistro, INombre, IUsaDescripcion, IUsaElemento
    {
        public new int IdElemento { get; set; }
        public int IdCreador { get; set; }
        public DateTime CreadaEl { get; set; }

        public enumNegocio Negocio { get; }

    }

    public interface IDatosDeContacto
    {
        public string eMail { get; set; }
        public string Telefono { get; set; }
    }

    public interface IEsInterlocutor
    {
        public int IdInterlocutor { get; set; }
        public InterlocutorDtm Interlocutor { get; set; }
    }

    public interface ITerceroContable : IEsInterlocutor, INombre
    {
        int? CodigoContable { get; set; }
        public int IdCuenta { get; set; }
        public CuentaDtm Cuenta { get; set; }

    }
    public interface IUsaCalseDeElemento
    {
        public int? IdClaseDeElemento { get; set; }
        ClaseDelNegocioDtm ClaseDeElemento { get; set; }
    }

    public interface IUsaSolicitante : IDatosDeContacto
    {
        public string Contacto { get; set; }
        public int IdSolicitante { get; set; }
        public abstract InterlocutorDtm Solicitante { get; set; }
    }

    public interface IPuedeUsarCliente : IDatosDeContacto
    {
        public string Contacto { get; set; }
        public int? IdCliente { get; set; }
        public abstract ClienteDtm Cliente { get; }
    }

    public interface IUsaCliente : IDatosDeContacto
    {
        public string Contacto { get; set; }
        public int IdCliente { get; set; }
        public abstract ClienteDtm Cliente { get; }
    }

    public interface IPuedeUsarProveedor : IDatosDeContacto
    {
        public string Contacto { get; set; }
        public int? IdProveedor { get; set; }
        public abstract ProveedorDtm Proveedor { get; }
    }

    public interface IUsaProveedor : IDatosDeContacto
    {
        public string Contacto { get; set; }
        public int IdProveedor { get; set; }
        public abstract ProveedorDtm Proveedor { get; set; }
    }

    public interface IPuedeUsarTrabajador : IDatosDeContacto
    {
        public string Contacto { get; set; }
        public int? IdTrabajador { get; set; }
        public abstract TrabajadorDtm Trabajador { get; }
    }

    public interface IUsaTrabajador
    {
        public int IdTrabajador { get; set; }
        public abstract TrabajadorDtm Trabajador { get; }
    }

    public interface IPuedeUsarResponsable
    {
        public int? IdResponsable { get; set; }
        public UsuarioDtm Responsable { get; set; }
    }

    public interface IUsaDirecciones : IRegistro
    {

    }
    public interface IUsaExpediente
    {
        public int? IdExpediente { get; set; }
        public ExpedienteDtm Expediente { get; set; }
    }

    public interface IUsaPresupuesto
    {
        public int? IdPresupuesto { get; set; }
        public PresupuestoDtm Presupuesto { get; set; }
    }
    public interface IUsaPreasiento : IUsaReferencia
    {
        public int? IdPreasiento { get; set; }
        public PreasientoDtm Preasiento { get; set; }
    }
    public interface IEsUnTercero : IDatosDeContacto
    {
        public bool EsInterlocutor { get; set; }
    }

    public interface IUsaBaja
    {
        public bool Baja { get; set; }
    }

    public interface IUsaBloqueo
    {
        public bool Bloqueado { get; set; }
    }

    public interface IUsaActiva
    {
        public bool Activa { get; set; }
    }


    public interface IUsaTraza
    {
    }

    public interface IUsaCg
    {
        public int IdCg { get; set; }
        public abstract CentroGestorDtm Cg { get; set; }
        public static string Campo = ICampos.ID_CG;
    }
    public interface IPermisosPorCg
    {
    }

    public interface IPermisoDeInterventor
    {
        public int IdPermisoInterventor { get; set; }
        public PermisoDtm PermisoDeInterventor { get; set; }
    }

    public interface IDelSistema
    {
        public bool DelSistema { get; set; }
    }

    public interface INombreModificable
    {
        public bool NombreModificable { get; set; }
    }

    public interface IEtapas<Enum>
    {
        public abstract string Estados(Enum etapa);
    }

    public interface IUsaEtapas
    {
        public IEnumerable<Enum> Etapa { get; }
    }

    public interface IUsaArchivo
    {
        public int? IdArchivo { get; set; }
        public abstract ArchivoDtm Archivo { get; set; }
    }

    public interface IPuedeUsarAgenda
    {
        public int? IdAgenda { get; set; }
        public abstract AgendaDtm Agenda { get; set; }
    }

    public interface IUsaAgenda
    {
        public int IdAgenda { get; set; }
        public abstract AgendaDtm Agenda { get; set; }
    }

    public interface IUsaElemento
    {
        public int IdElemento { get; set; }
    }

    public interface IUsaDescripcion
    {
        public string Descripcion { get; set; }
    }
    public interface ITieneReferencia : IRegistro
    {
        public string Referencia { get; }
    }
    public interface IUsaReferencia : ITieneReferencia
    {
        public new string Referencia { get; set; }
    }

    public interface IPermisosDelElementoDtm
    {
        public int IdElemento { get; set; }
        public int IdGestor { get; set; }
        public int IdConsultor { get; set; }
        public int IdAdministrador { get; set; }

    }
    public interface IDireccionDtm
    {
        public int IdElemento { get; set; }
        public enumCalificadorDireccion Calificador { get; set; }
        public int IdPais { get; set; }
        public int IdProvincia { get; set; }
        public int IdMunicipio { get; set; }
        public int IdCalle { get; set; }
        public int? IdZona { get; set; }
        public int? IdBarrio { get; set; }
        public int? IdCp { get; set; }
        public int? Numero { get; set; }
        public string Escalera { get; set; }
        public string Piso { get; set; }
        public string Puerta { get; set; }
        public string Otros { get; set; }
        public string Url { get; set; }
        public bool Activo { get; set; }
        public int IdCreador { get; set; }
        public DateTime CreadaEl { get; set; }
        public enumNegocio Negocio { get; set; }

    }

    public class CrearDtm<T> where T : new()
    {
        private T t;

        public T Objeto => t;

        public CrearDtm()
        {
            t = new T();
        }
    }

    public static class ApiDeInterfaceDtm
    {
        public static IEnumerable<Type> EncontrarTiposConLaInterface(Type interfaceUsada)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Fija_InterfacesUsada);
            var indice = interfaceUsada.FullName;

            if (!cache.ContainsKey(indice))
            {
                // 1. Obtenemos solo el ensamblado donde está definida la interfaz o tus DTMs
                // Puedes usar Assembly.GetExecutingAssembly() si este código está en la misma DLL
                var ensambladoDtm = interfaceUsada.Assembly;

                cache[indice] = ensambladoDtm.GetTypes() // Solo busca en TU archivo .dll
                    .Where(p => interfaceUsada.IsAssignableFrom(p)
                                && p.IsClass
                                && !p.IsAbstract)
                    .ToList();
            }

            return (IEnumerable<Type>)cache[indice];
        }

        public static bool ImplementaRegistroDtm(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(IRegistro));
        public static bool ImplemtaUnaAmpliacion(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(IAmpliacion));
        public static bool ImplemtaUnDetalle(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(IDetalle));
        public static bool ImplementaNombre(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(INombre));
        public static bool ImplementaRegistroDeParametrizacion(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IRegistroDeParametrizacion).FullName);
        public static bool ImplementaNombre(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(INombre).FullName);
        public static bool ImplemtaUnIDetalle(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IDetalle).FullName);
        public static bool ImplementaUnaRelacion(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(IRelacion));
        public static bool ImplementaNecesitaSerParametrizador(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(INecesitaSerParametrizador));
        public static bool ImplementaNecesitaSerParametrizador(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(INecesitaSerParametrizador).FullName);
        public static bool ImplementaUnVinculo(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IVinculoDtm).FullName);
        public static bool ImplementaUnaRelacion(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IRelacion).FullName);
        public static bool ImplementaUnElemento(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(IElementoDtm));
        public static bool ImplementaUnElemento(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IElementoDtm).FullName);
        public static bool ImplementaUnTipoDeElemento(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(ITipoDtm).FullName);
        public static bool ImplementaUnEstado(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IEstado).FullName);
        public static bool ImplementaUnaTransicion(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(ITransicion).FullName);
        public static bool ImplementaUnaAccionDeTrn(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IAccionDeTrn).FullName);
        public static bool ImplementaUnaAccionDeRelacion(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IAccionDeRelacion).FullName);
        public static bool ImplementaUnaDireccion(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IDireccionDtm).FullName);
        public static bool ImplementaUnaTraza(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(ITraza).FullName);
        public static bool ImplementaUnHito(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IHitoDtm).FullName);

        public static bool ImplementaUnaObservacion(this Type tipoRegistro) => ApiDeEnsamblados.ImplementaInterface(tipoRegistro, typeof(IObservacion).FullName);

        public static bool ImplementaElementoConTipo(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaTipo).FullName)
            && !claseDtm.ImplementaClasesDelTipo()
            && !claseDtm.ImplementaPlantillasPorTipo();

        public static bool ImplementaUsaCg(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaCg).FullName);
        public static bool ImplementaPermisosPorCg(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IPermisosPorCg).FullName);
        public static bool ImplementaPermisosDeInterventor(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IPermisoDeInterventor).FullName);
        public static bool ImplementaGestionadoPorElSistemar(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IDelSistema).FullName);
        public static bool ImplementaNombreModificable(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(INombreModificable).FullName);
        public static bool ImplementaPermisosDelElemento(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IPermisosDelElementoDtm).FullName);
        public static bool ImplementaElementoTipado(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IElementoConTipo).FullName);
        public static bool ImplementaElementoDeUnProceso(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IElementoDeProcesoDtm).FullName);
        public static bool ImplemtaUnDetalle(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IDetalle).FullName);

        public static bool ImplementaUsaBloqueo(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaBloqueo).FullName);

        public static bool ImplementaUsaPreasiento(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaPreasiento).FullName);
        public static bool ImplementaUsaBaja(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaBaja).FullName);
        public static bool ImplementaUsaArchivo(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaArchivo).FullName);
        public static bool ImplementaUsaArchivo(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(IUsaArchivo));

        public static bool ImplementaUsaExpediente(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaExpediente).FullName);

        public static bool ImplementaUsaPresupuesto(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaPresupuesto).FullName);

        public static bool ImplementaUsaDescripcion(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaDescripcion).FullName);
        public static bool ImplementaUsaReferencia(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaReferencia).FullName);
        public static bool ImplementaUsaReferencia(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(IUsaReferencia));
        public static bool ImplementaUsaClaseDeElemento(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaCalseDeElemento).FullName);
        public static bool ImplementaTieneReferencia(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(ITieneReferencia).FullName);
        public static bool ImplementaAuditoria(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IAuditoria).FullName);
        public static bool ImplementaRowVersion(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IRowVersion).FullName);
        public static bool ImplementaTieneNegocio(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(ITieneCampoNegocio).FullName);
        public static bool ImplementaAmpliacion(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IAmpliacion).FullName);
        public static bool ImplementaDetalle(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IDetalle).FullName);
        public static bool ImplementaLibroDeRegistro(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(ILibroDeRegistro).FullName);
        public static bool ImplementaUsaEstado(this Type claseDtm)
        {
            var b = ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaEstado).FullName);
            if (b && !claseDtm.BaseType.Equals(typeof(TipoConFlujoDtm)) && !ImplementaElementoConTipo(claseDtm))
                throw new Exception($"La clase {claseDtm} está mal definida, no puede implementar {nameof(IUsaEstado)} y no implementar {nameof(IUsaTipo)} y {nameof(IPermisosPorCg)}");
            return b;
        }
        public static bool ImplementaUsaFlujo(this Type tipoRegistro) => !ApiDeEnsamblados.HeredaDe(tipoRegistro, typeof(TipoDeElementoDtm)) && ImplementaUsaEstado(tipoRegistro);
        
        //public static bool ImplementaUsaTipoConCg(this Type tipoRegistro) => ImplementaElementoConTipo(tipoRegistro) && ImplementaUsaCg(tipoRegistro);


        public static bool ImplementaUsaTipoConCg(this Type claseDtm) => ApiDeEnsamblados.ImplementaInterface(claseDtm, typeof(IUsaTipoConCG).FullName);

        public static bool ImplementaPlantillasPorTipo(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IPlantillaPorTipo));
        public static bool ImplementaClasesDelTipo(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IClaseDelTipoDtm));
        public static bool ImplementaPlantillasDeNegocio(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IPlantillaDeNegocio));
        public static bool ImplementaUsaSolicitante(this IRegistro registro) => registro.GetType().GetInterfaces().Contains(typeof(IUsaSolicitante));
        public static bool ImplementaDatosDeContacto(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IDatosDeContacto));
        public static bool ImplementaEsUnInterlocutor(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IEsInterlocutor));
        public static bool ImplementaPuedeUsarCliente(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IPuedeUsarCliente));
        public static bool ImplementaUsaCliente(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IUsaCliente));
        public static bool ImplementaPuedeUsarProveedor(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IPuedeUsarProveedor));
        public static bool ImplementaUsaProveedor(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IUsaProveedor));
        public static bool ImplementaPuedeUsarTrabajador(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IPuedeUsarTrabajador));
        public static bool ImplementaPuedeUsarResponsable(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IPuedeUsarResponsable));
        public static bool ImplementaUsaTrabajador(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IUsaTrabajador));
        public static bool ImplementaUsaSolicitante(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IUsaSolicitante));

        public static bool ImplementaUsaEtapas(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IUsaEtapas));

        public static bool ImplementaEsUnTerceroContable(this Type tipo) => tipo.GetInterfaces().Contains(typeof(ITerceroContable));

        public static bool ImplementaUsaAmpliaciones(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IUsaAmpliaciones));
        public static bool ImplementaUsaDetalles(this Type tipo) => tipo.GetInterfaces().Contains(typeof(IUsaDetalles));

        public static T QuitarResponsable<T>(this T registro)
        where T : IPuedeUsarResponsable
        {
            registro.Responsable = null;
            registro.IdResponsable = null;
            return registro;
        }

    }
}
