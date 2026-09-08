using GestorDeElementos;
using GestoresDeNegocio.Terceros;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using ServicioDeReportes.Base;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.SistemaDocumental
{
    /// <summary>
    /// Describe cómo obtener un maestro concreto (Cliente, Proveedor, Solicitante...) a partir del elemento
    /// que se imprime. Es la lista a la que hay que añadir una entrada nueva cada vez que se necesite un
    /// maestro más (Iva, Irpf, Unitario, Juzgado...): el resto de DatosMaestros no cambia.
    /// </summary>
    public class DefinicionDeMaestro
    {
        /// <summary>Clave con la que se referencia en las etiquetas: {{{maestro.&lt;Clave&gt;.campo}}}.</summary>
        public string Clave { get; init; }

        /// <summary>
        /// Id del maestro para este elemento, o null si el elemento no lo tiene (interfaz no implementada,
        /// Id opcional sin informar, o -para maestros indirectos como MiSociedad- porque el maestro del que
        /// depende tampoco se pudo resolver). Recibe el contexto porque algún maestro (MiSociedad) necesita
        /// leer un registro intermedio (el Centro Gestor) para averiguar su Id.
        /// </summary>
        public Func<ContextoSe, ElementoDtm, int?> LeerId { get; init; }

        public Func<ContextoSe, int, Dictionary<string, object>> CargarDatosPrincipales { get; init; }

        /// <summary>Opcional: null si este maestro no tiene direcciones.</summary>
        public Func<ContextoSe, int, List<Dictionary<string, object>>> CargarDirecciones { get; init; }

        /// <summary>Opcional: null si este maestro no tiene cuentas bancarias.</summary>
        public Func<ContextoSe, int, List<Dictionary<string, object>>> CargarCuentasBancarias { get; init; }
    }

    /// <summary>
    /// Registro extensible de maestros disponibles para las plantillas. Cliente/Proveedor/Solicitante son
    /// los primeros; los maestros que no cuelgan de un tercero relacionado con el elemento (Iva, Irpf,
    /// Unitario, Juzgado...) se añadirán aquí más adelante con su propia forma de resolver DatosPrincipales
    /// (no necesariamente vía interfaz + Direcciones/CuentasBancarias).
    /// </summary>
    public static class DefinicionesDeMaestros
    {
        private static List<Dictionary<string, object>> CuentasBancariasDe<TDtm, TDto>(GestorDeElementos.GestorDeElementos<ContextoSe, TDtm, TDto> gestor, int idElemento)
            where TDtm : RegistroDtm, IDetalle
            where TDto : ElementoDto
        {
            return gestor.LeerElementos(0, -1,
                new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(
                    clausula: nameof(IDetalle.IdElemento),
                    criterio: enumCriteriosDeFiltrado.igual,
                    valor: idElemento) },
                orden: null,
                parametros: new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, true } })
                .Select(c => c.ToDictionary())
                .ToList();
        }

        /// <summary>Id de la Sociedad del Centro Gestor del elemento, o null si el elemento no tiene CG (no implementa IUsaCg) o el CG no tiene sociedad.</summary>
        private static int? LeerIdDeSociedadDelCg(ContextoSe contexto, ElementoDtm elemento)
        {
            if (elemento is not IUsaCg cg) return null;
            return new GestorDeCentrosGestores(contexto, contexto.Mapeador).LeerElementoPorId(cg.IdCg)?.IdSociedad;
        }

        public static readonly List<DefinicionDeMaestro> Registro = new()
        {
            new DefinicionDeMaestro
            {
                Clave = "cliente",
                LeerId = (contexto, elemento) => elemento switch
                {
                    IUsaCliente c => c.IdCliente,
                    IPuedeUsarCliente c => c.IdCliente,
                    _ => null
                },
                CargarDatosPrincipales = (contexto, id) => new GestorDeClientes(contexto, contexto.Mapeador).LeerElementoPorId(id)?.ToDictionary(),
                CargarDirecciones = (contexto, id) => GestorDeDirecciones.LeerDirecciones(contexto, enumNegocio.Cliente, id).Select(d => d.ToDictionary()).ToList(),
                CargarCuentasBancarias = (contexto, id) => CuentasBancariasDe(new GestorDeCuentasDeCliente(contexto, contexto.Mapeador), id)
            },
            new DefinicionDeMaestro
            {
                Clave = "proveedor",
                LeerId = (contexto, elemento) => elemento switch
                {
                    IUsaProveedor p => p.IdProveedor,
                    IPuedeUsarProveedor p => p.IdProveedor,
                    _ => null
                },
                CargarDatosPrincipales = (contexto, id) => new GestorDeProveedores(contexto, contexto.Mapeador).LeerElementoPorId(id)?.ToDictionary(),
                CargarDirecciones = (contexto, id) => GestorDeDirecciones.LeerDirecciones(contexto, enumNegocio.Proveedor, id).Select(d => d.ToDictionary()).ToList(),
                CargarCuentasBancarias = (contexto, id) => CuentasBancariasDe(new GestorDeCuentasDeProveedor(contexto, contexto.Mapeador), id)
            },
            new DefinicionDeMaestro
            {
                Clave = "solicitante",
                LeerId = (contexto, elemento) => elemento is IUsaSolicitante s ? s.IdSolicitante : (int?)null,
                CargarDatosPrincipales = (contexto, id) => new GestorDeInterlocutores(contexto, contexto.Mapeador).LeerElementoPorId(id)?.ToDictionary(),
                CargarDirecciones = (contexto, id) => GestorDeDirecciones.LeerDirecciones(contexto, enumNegocio.Interlocutor, id).Select(d => d.ToDictionary()).ToList(),
                CargarCuentasBancarias = (contexto, id) => CuentasBancariasDe(new GestorDeCuentasDeInterlocutor(contexto, contexto.Mapeador), id)
            },
            new DefinicionDeMaestro
            {
                // El Centro Gestor del elemento (VENTA.FACTURA_EMT.ID_CG, etc.) -- disponible en cualquier
                // elemento que herede de ElementoConCgDtm/ElementoDeProcesoDtm (implementan IUsaCg).
                // Por ahora solo DatosPrincipales: CentroGestorDtm no tiene direcciones ni cuentas propias
                // (las suyas son las de su Sociedad, ver el maestro "misociedad").
                Clave = "micg",
                LeerId = (contexto, elemento) => elemento is IUsaCg c ? c.IdCg : (int?)null,
                CargarDatosPrincipales = (contexto, id) => new GestorDeCentrosGestores(contexto, contexto.Mapeador).LeerElementoPorId(id)?.ToDictionary(),
            },
            new DefinicionDeMaestro
            {
                // La Sociedad titular del Centro Gestor del elemento -- lo que hasta ahora había que traer
                // a mano en el PA con un join FACTURA_EMT -> CENTRO_GESTOR -> SOCIEDAD (ver
                // Plt_De_Negocio_Factura_de_Word.sql). Con este maestro ya no hace falta.
                Clave = "misociedad",
                LeerId = LeerIdDeSociedadDelCg,
                CargarDatosPrincipales = (contexto, id) => new GestorDeSociedades(contexto, contexto.Mapeador).LeerElementoPorId(id)?.ToDictionary(),
                CargarDirecciones = (contexto, id) => GestorDeDirecciones.LeerDirecciones(contexto, enumNegocio.Sociedad, id).Select(d => d.ToDictionary()).ToList(),
                CargarCuentasBancarias = (contexto, id) => CuentasBancariasDe(new GestorDeCuentasDeMiSociedad(contexto, contexto.Mapeador), id)
            },
        };
    }

    /// <summary>
    /// Datos de los maestros relacionados con el elemento que se imprime (Cliente/Proveedor/Solicitante,
    /// y los que se vayan añadiendo a DefinicionesDeMaestros.Registro), listos para que
    /// ApiDePlantillas.ProcesarEtiquetasDeMaestros sustituya etiquetas {{{maestro.&lt;clave&gt;....}}}.
    /// Se calcula una sola vez por impresión, igual que DetallesDelObjeto.
    /// </summary>
    public class GestorDeMaestros
    {
        public Dictionary<string, DatosDeUnMaestro> Maestros { get; } = new(StringComparer.OrdinalIgnoreCase);

        public GestorDeMaestros(ContextoSe contexto, ElementoDtm elemento)
        {
            foreach (var definicion in DefinicionesDeMaestros.Registro)
            {
                var idMaestro = definicion.LeerId(contexto, elemento);
                if (idMaestro is null || idMaestro <= 0) continue;

                Maestros[definicion.Clave] = new DatosDeUnMaestro
                {
                    DatosPrincipales = definicion.CargarDatosPrincipales(contexto, idMaestro.Value) ?? new Dictionary<string, object>(),
                    Direcciones = definicion.CargarDirecciones?.Invoke(contexto, idMaestro.Value) ?? new List<Dictionary<string, object>>(),
                    CuentasBancarias = definicion.CargarCuentasBancarias?.Invoke(contexto, idMaestro.Value) ?? new List<Dictionary<string, object>>()
                };
            }
        }
    }
}
