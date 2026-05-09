using System;
using System.Collections.Generic;
using Utilidades;

namespace ModeloDeDto
{
    public class IndCrud
    {
        public const string SiempreEnConsulta = nameof(SiempreEnConsulta);
        public const string CapaConVinculados = nameof(CapaConVinculados);
        public const string MostrarVisorAlIniciar = nameof(MostrarVisorAlIniciar);
        public const string UsaTotalizador = nameof(UsaTotalizador);
    }

    public interface IUsaNombreDto : IElementoDto
    {
        public string Nombre { get; set; }
    }

    public interface IUsaBajaDto
    {
        public bool Baja { get; set; }
    }


    public interface IUsaBloqueoDto
    {
        public bool Bloqueado { get; set; }
        public string Bloqueador { get; set; }
    }

    public interface IAmpliacionDto
    {
        public int IdElemento { get; set; }
    }
    public interface IDetalleDto
    {
        public int IdElemento { get; set; }
        public string Titulo { get; set; }
    }

    public interface IAuditadoDto
    {
        [IUPropiedad(Visible = false)]
        public DateTime CreadoEl { get; set; }

        [IUPropiedad(Visible = false)]
        public DateTime? ModificadoEl { get; set; }

        [IUPropiedad(Visible = false)]
        public string Creador { get; set; }

        [IUPropiedad(Visible = false)]
        public int IdCreador { get; set; }

        [IUPropiedad(Visible = false)]
        public string Modificador { get; set; }

        [IUPropiedad(Visible = false)]
        public int? IdModificador { get; set; }

    }

    public interface IUsaArchivoDto
    {
        public int? IdArchivo { get; set; }
        public string Archivo { get; set; }
    }

    public interface IRelacionDto
    {

    }

    public interface IUsaNegocioDto
    {
        public int IdNegocio { get; set; }
    }

    public interface IElementoDto
    {
        int Id { get; set; }
    }

    public interface ITotalesDto
    {
        public int Procesados { get; set; }
    }

    public class TotalesDto: ITotalesDto
    {
        [IUPropiedad(Visible = false)]
        public int Procesados { get; set; }
    }

    public interface IPlantillaDeUsuarioDto
    {
        string Plantilla { get; set; }
    }

    public interface IUsaEstadoDto
    {
        public int IdEstado { get; set; }

        public string Estado { get; set; }  
    }

    public interface IUsaTipoDto
    {
        public int IdTipo { get; set; }

        public string Tipo { get; set; }
    }

    public interface IUsaSolicitanteDto
    {
        public int IdSolicitante { get; set; }

        public string Solicitante { get; set; }
    }

    public interface IPuedeUsarResponsableDto
    {
        public int? IdResponsable { get; set; }

        public string Responsable { get; set; }
    }
    

    public interface IUsaDescripcionDto
    {

        public string Descripcion { get; set; }
    }

    public interface IUsaDirecciones
    {
        public string Direcciones { get; set; }
    }
    public interface IUsaReferenciaDto
    {
        public string Referencia { get; set; }
    }
    public interface IElmentoAuditadoDto : IAuditadoDto
    {
        public bool HayClases { get; set; }
        public int IdNegocio { get; set; }
    }

    public interface IUsaClasePorTipoDto
    {        public bool UsaClasePorTipo { get; set; }
    }

    public interface IElementoConTipoDto : IElmentoAuditadoDto, IUsaTipoDto, IUsaDescripcionDto, IUsaReferenciaDto, IUsaNombreDto, IUsaClasePorTipoDto
    {
        public bool EsInterventor { get; set; }

        public bool EsGestor { get; set; }
    }


    public interface IElementoConTipoConCgDto : IElementoConTipoDto
    {
        public int? IdSociedadDelCg { get; set; }
        public int IdCg { get; set; }
        public string Cg { get; set; }
    }

    public interface IElementoDeUnProcesoDto : IElementoConTipoConCgDto, IUsaEstadoDto
    {
        public List<string> Etapas { get; set; }

        public DateTime? TransitadoEl { get; set; }
        public string TransitadoAl { get; set; }
        public int? IdEstadoAnterior { get; set; }
        public string EstadoAnterior { get; set; }

        public int? IdTransicionAplicable { get; set; }

        public string TransicionAplicable { get; set; }

        public int? IdEstadoDestino { get; set; }
        public int TransicionesDisponibles { get; set; }
    }

    public interface IUsaCuentaBancariaDto
    {
        public string Iban { get; set; }
        public string Entidad { get; set; }
        public string Oficina { get; set; }
        public string DcCcc { get; set; }
        public string Numero { get; set; }
    }

    public interface ISelectorDto
    {
        public int IdElemento { get; set; }
        public string Elemento { get; set; }
    }

    public interface IRenombrarDto
    {
        public int IdElemento { get; set; }
        public string Elemento { get; set; }
    }

    public class SelectorDto : ISelectorDto
    {
        public int IdElemento { get; set; }
        public string Elemento { get; set; }
    }

    internal interface ISelectorDeFechasDto
    {
        public DateTime FechaDesde { get; set; }
        public DateTime FechaHasta { get; set; }
    }

    public interface IInformacionRpt<T>
    {
        T Datos { get; set; }
    }


    public interface ITotales<T>
    {
        T Totales { get; set; }
    }


    public enum enumClaseHistorialRpt { Observacion, Traza, Hito }

    public class HistorialRpt
    {
        public DateTime CreadaEl { get; set; }
        public string Nombre { get; set; }
        public string Creador { get; set; }
        public string Descripcion { get; set; }
        public enumClaseHistorialRpt Clase { get; set; }
    }

    public static class ApiDeInterfaceDto
    {
        public static bool ImplementaUnElementoDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IElementoDto).FullName);
        public static bool ImplementaAuditoriaDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IAuditadoDto).FullName);
        public static bool ImplementaRelacionDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IRelacionDto).FullName);
        public static bool ImplementaTipoDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IUsaTipoDto).FullName);
        public static bool ImplementaUsaSolicitanteDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IUsaSolicitanteDto).FullName);
        public static bool ImplementaProcesoDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IElementoDeUnProcesoDto).FullName);
        public static bool ImplementaElementoConTipoDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IElementoConTipoDto).FullName);
        public static bool ImplementaDescripcionDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IUsaDescripcionDto).FullName);
        public static bool ImplementaReferenciaDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IUsaReferenciaDto).FullName);
        public static bool ImplementaBajaDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IUsaBajaDto).FullName);
        public static bool ImplementaAmpliacionDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IAmpliacionDto).FullName);
        public static bool ImplementaNombreDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IUsaNombreDto).FullName);
        public static bool ImplementaInformacionRpt(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IInformacionRpt<object>).FullName);
        public static bool ImplementaUsaBloqueoDto(this Type tipoElemento) => ApiDeEnsamblados.ImplementaInterface(tipoElemento, typeof(IUsaBloqueoDto).FullName);


    }
}
