using Utilidades;
using ServicioDeDatos.Elemento;
using System.Reflection;
using System.Text;
using GestorDeElementos;
using ServicioDeDatos.SistemaDocumental;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;

namespace ServicioDeReportes.Base
{
    public class TablaPlantillaDto
    {
        public string Nombre { get; set; } = "";
        public List<string> Encolumnado { get; set; } = new List<string>();

        public TablaPlantillaDto(Type tipoDto, enumEncabezadosDeTablas? nombre = null)
        {
            Nombre = nombre is null ? tipoDto.Name.Replace("Dto", "") : ((enumEncabezadosDeTablas)nombre).ToString();
            foreach (var propiedad in tipoDto.GetProperties())
            {
                if (propiedad.PropertyType.HeredaDe(typeof(ElementoDto), incluirTipo: true))
                    continue;
                if (propiedad.Name.StartsWith(nameof(IRegistro.Id)))
                    continue;
                if (propiedad.Name.Equals(nameof(ElementoDto.EstaCancelada)) ||
                    propiedad.Name.Equals(nameof(ElementoDto.EstaTerminada)) ||
                    propiedad.Name.Equals(nameof(ElementoDto.NombreModificable)) ||
                    propiedad.Name.Equals(nameof(ElementoDto.informacion)) ||
                    propiedad.Name.Equals(nameof(ElementoDto.ModoDeAcceso))
                    )
                    continue;
                if (tipoDto.HeredaDe(typeof(EsUnDetalleDto), incluirTipo: false))
                {
                    if (propiedad.Name.Equals(nameof(EsUnDetalleDto.Elemento)))
                        continue;
                }

                if (tipoDto.ImplementaAuditoriaDto())
                {
                    if (propiedad.Name.Equals(nameof(IAuditadoDto.Creador)) ||
                        propiedad.Name.Equals(nameof(IAuditadoDto.Modificador)) ||
                        propiedad.Name.Equals(nameof(IAuditadoDto.CreadoEl)) ||
                        propiedad.Name.Equals(nameof(IAuditadoDto.ModificadoEl))
                       )
                       continue;
                }

                if (tipoDto == typeof(HitoDto))
                {
                    if (propiedad.Name.Equals(nameof(HitoDto.Elemento)))
                        continue;
                    if (propiedad.Name.Equals(nameof(HitoDto.Negocio)))
                        continue;
                }
                if (tipoDto == typeof(ObservacionDto))
                {
                    if (propiedad.Name != nameof(ObservacionDto.Nombre) && propiedad.Name != nameof(ObservacionDto.Descripcion))
                        continue;
                }
                if (tipoDto == typeof(DireccionDto))
                {
                    if (propiedad.Name != nameof(DireccionDto.NombreDireccion) && propiedad.Name != nameof(DireccionDto.Calificador))
                        continue;
                }
                Encolumnado.Add(propiedad.Name);
            }
        }
    }

    public static class ApiDeEtiquetas
    {
        private const string Separador = "----------------------------------------------------------------------";
        private const string SeparadorDeBloque = "========================================================================";

        public static string CrearFicheroDeEtiquetas(Type tipoDtm)
        {
            if (!Path.Exists(enumRutas.RutaDePlantillas))
                Directory.CreateDirectory(enumRutas.RutaDePlantillas);

            string fichero = Path.Combine(enumRutas.RutaDePlantillas, $"Etiquetas de {tipoDtm.Name}.{enumExtensiones.txt}".NormalizarFichero());

            var lineas = new List<string>();
            lineas.AddRange(Cabecera(tipoDtm));
            lineas.AddRange(SeccionDeEtiquetasSimples(tipoDtm));
            lineas.AddRange(SeccionDeEtiquetasDeTabla(tipoDtm));
            lineas.AddRange(SeccionDeEtiquetasDeMaestros());

            File.WriteAllLines(fichero, lineas, Encoding.UTF8);
            return fichero;
        }

        private static List<string> Cabecera(Type tipoDtm)
        {
            return new List<string>
            {
                $"ETIQUETAS DISPONIBLES PARA LA PLANTILLA DE: {tipoDtm.Name.Replace("Dtm", "")}",
                SeparadorDeBloque,
                "Cada etiqueta se sustituye por un dato real al imprimir. Cópiela tal cual se muestra a",
                "continuación, llaves {{{ }}} incluidas, en el lugar del documento Word donde quiera que",
                "aparezca ese dato.",
                ""
            };
        }

        private static List<string> SeccionDeEtiquetasSimples(Type tipoDtm)
        {
            var lineas = new List<string>
            {
                "1) ETIQUETAS DE UN SOLO DATO",
                SeparadorDeBloque,
                "Coloque cualquiera de estas etiquetas directamente en el texto del Word.",
                ""
            };

            IncluirEtiquetas(lineas, tipoDtm);

            var negocio = tipoDtm.NegocioDeUnDtm();
            var ampliaciones = negocio.TiposDeAmpliaciones();
            foreach (var ampliacion in ampliaciones)
            {
                var ampliacionDto = ampliacion.ToDto();
                lineas.Add("");
                IncluirEtiquetas(lineas, ampliacionDto, ampliacion.Name.Replace("Dtm", ""));
            }

            lineas.Add("");
            return lineas;
        }

        private static void IncluirEtiquetas(List<string> lineas, Type tipoDtm)
        {
            var clave = tipoDtm.Name.Replace("Dtm", "");
            var tipoDto = tipoDtm.ToDto();
            IncluirEtiquetas(lineas, tipoDto, clave);
        }

        private static void IncluirEtiquetas(List<string> lineas, Type tipoDto, string clave)
        {
            lineas.Add($"Etiquetas de: {clave}");
            lineas.Add(Separador);
            foreach (PropertyInfo propiedad in tipoDto.GetProperties())
                lineas.Add($"{Simbolos.PltInicio}{clave}.{propiedad.Name}{Simbolos.PltCierre}");
        }

        private static List<string> SeccionDeEtiquetasDeTabla(Type tipoDtm)
        {
            var tablas = DefinirTablasDto(tipoDtm);

            var lineas = new List<string>
            {
                "2) ETIQUETAS DE TABLA (detalles, líneas, hitos, observaciones, direcciones...)",
                SeparadorDeBloque,
                "Para cada una de estas tablas, incluya en el Word una tabla de 3 filas:",
                "  1. Fila marcadora: una celda con el texto exacto de la primera etiqueta indicada abajo",
                "     (p.ej. {{{NombreDeLaTabla}}}); dice de dónde se debe coger el detalle. El resto de",
                "     celdas de esa fila puede dejarse vacío.",
                "  2. Fila de cabecera (opcional, recomendada): los rótulos que verá el usuario final,",
                "     en texto normal, sin etiquetas.",
                "  3. Fila plantilla, que debe ser la ÚLTIMA fila de la tabla: una celda por columna,",
                "     cada una con ÚNICAMENTE una de las etiquetas de columna indicadas abajo.",
                ""
            };

            if (tablas.Count == 0)
            {
                lineas.Add("(Este negocio no tiene tablas de detalle)");
                lineas.Add("");
                return lineas;
            }

            foreach (var tabla in tablas)
            {
                lineas.Add($"Etiquetas de: {tabla.Nombre}");
                lineas.Add(Separador);
                lineas.Add($"Fila marcadora: {Simbolos.PltInicio}{tabla.Nombre}{Simbolos.PltCierre}");
                lineas.Add("Etiquetas de columna para la fila plantilla:");
                foreach (var columna in tabla.Encolumnado)
                    lineas.Add($"  {Simbolos.PltInicio}{columna}{Simbolos.PltCierre}");
                lineas.Add("");
            }

            return lineas;
        }

        /// <summary>
        /// Etiquetas {{{maestro.<clave>....}}} (ver ApiDePlantillas.ProcesarEtiquetasDeMaestros y
        /// GestoresDeNegocio.SistemaDocumental.DefinicionesDeMaestros.Registro): datos de Cliente, Proveedor,
        /// Solicitante, Centro Gestor y Sociedad emisora, disponibles en CUALQUIER negocio sin que el
        /// procedimiento almacenado tenga que traerlos -- solo si el elemento implementa la interfaz de la
        /// que sale cada uno (IUsaCliente/IPuedeUsarCliente, IUsaProveedor/IPuedeUsarProveedor,
        /// IUsaSolicitante, IUsaCg). Si mañana se añade un maestro nuevo al registro (Iva, Irpf, Unitario,
        /// Juzgado...), añadir aquí su bloque correspondiente.
        /// </summary>
        private static List<string> SeccionDeEtiquetasDeMaestros()
        {
            var lineas = new List<string>
            {
                "3) ETIQUETAS DE DATOS MAESTROS",
                SeparadorDeBloque,
                "Disponibles en cualquier negocio, sin tocar el procedimiento almacenado (solo si el",
                "elemento tiene ese dato: cliente, proveedor, solicitante, centro gestor o sociedad).",
                "Coloque cualquiera de estas etiquetas directamente en el texto del Word.",
                ""
            };

            void AgregarMaestro(string clave, Type tipoDatosPrincipales, Type tipoCuentaBancaria)
            {
                var sinValor = PropiedadesSinValorDesdeElMaestro(clave);
                lineas.Add($"Etiquetas de: maestro.{clave}");
                lineas.Add(Separador);
                foreach (var propiedad in tipoDatosPrincipales.GetProperties())
                {
                    if (propiedad.PropertyType.HeredaDe(typeof(ElementoDto), incluirTipo: true)) continue;
                    if (sinValor.Contains(propiedad.Name)) continue;
                    lineas.Add($"{Simbolos.PltInicio}maestro.{clave}.{propiedad.Name}{Simbolos.PltCierre}");
                }

                lineas.Add($"{Simbolos.PltInicio}maestro.{clave}.direccion.<Campo>{Simbolos.PltCierre}  (la primera dirección de la lista)");
                lineas.Add($"{Simbolos.PltInicio}maestro.{clave}.direccion.[fiscal].Expresion{Simbolos.PltCierre}  (la primera con Calificador = 'fiscal')");
                lineas.Add("  <Campo> de una dirección puede ser cualquiera de estos:");
                foreach (var propiedad in typeof(DireccionDto).GetProperties())
                {
                    if (propiedad.PropertyType.HeredaDe(typeof(ElementoDto), incluirTipo: true)) continue;
                    lineas.Add("  " + propiedad.Name);
                }

                if (tipoCuentaBancaria is null)
                {
                    lineas.Add("");
                    return;
                }

                lineas.Add($"{Simbolos.PltInicio}maestro.{clave}.cuentabancaria.<Campo>{Simbolos.PltCierre}  (la primera cuenta de la lista)");
                lineas.Add($"{Simbolos.PltInicio}maestro.{clave}.cuentabancaria.[Ingreso].Cuenta{Simbolos.PltCierre}  (la primera con Clase = 'Ingreso')");
                lineas.Add("  <Campo> de una cuenta bancaria puede ser cualquiera de estos:");
                foreach (var propiedad in tipoCuentaBancaria.GetProperties())
                {
                    if (propiedad.PropertyType.HeredaDe(typeof(ElementoDto), incluirTipo: true)) continue;
                    lineas.Add("  " + propiedad.Name);
                }
                lineas.Add("");
            }

            AgregarMaestro("cliente", typeof(ClienteDto), typeof(CuentaDeClienteDto));
            AgregarMaestro("proveedor", typeof(ProveedorDto), typeof(CuentaDeProveedorDto));
            AgregarMaestro("solicitante", typeof(InterlocutorDto), typeof(CuentaDeInterlocutorDto));

            // MiCg no tiene direcciones ni cuentas propias -- las suyas son las de MiSociedad. Se trata aparte
            // porque no sigue el patrón DatosPrincipales+direccion+cuentabancaria de los otros cuatro.
            // (A diferencia de los otros maestros, CentroGestorDto se mapea entero sin condiciones en
            // DespuesDeMapearElElemento -- de hecho no lo sobrescribe -- así que no hay nada que excluir aquí.)
            lineas.Add("Etiquetas de: maestro.MiCg");
            lineas.Add(Separador);
            foreach (var propiedad in typeof(CentroGestorDto).GetProperties())
            {
                if (propiedad.PropertyType.HeredaDe(typeof(ElementoDto), incluirTipo: true)) continue;
                lineas.Add($"{Simbolos.PltInicio}maestro.MiCg.{propiedad.Name}{Simbolos.PltCierre}");
            }
            lineas.Add("");

            AgregarMaestro("MiSociedad", typeof(SociedadDto), typeof(CuentaDeMiSociedadDto));

            return lineas;
        }

        /// <summary>
        /// Propiedades del Dto de un maestro que NUNCA llegan con valor por esta vía, porque
        /// <c>GestorDeMaestros</c> llama a <c>LeerElementoPorId(id)</c> sin parámetros adicionales
        /// (ni <c>Peticion = epLeerPorId</c>, ni <c>ObtenerDatosFiscales</c>, ni ningún otro parámetro
        /// puntual) y el correspondiente <c>DespuesDeMapearElElemento</c> solo rellena estas propiedades
        /// cuando esos parámetros están presentes -- verificado en GestorDeClientes, GestorDeProveedores,
        /// GestorDeInterlocutores y GestorDeSociedades. Ofrecerlas como etiqueta induciría a pensar que
        /// van a imprimir un dato que en realidad siempre sale vacío.
        /// </summary>
        private static HashSet<string> PropiedadesSinValorDesdeElMaestro(string clave) => clave switch
        {
            "cliente" => new HashSet<string> { nameof(ClienteDto.NIF), nameof(ClienteDto.RazonSocial), nameof(ClienteDto.TipoDeTercero), nameof(ClienteDto.DireccionFiscal), nameof(ClienteDto.EsIntraComunitario), nameof(ClienteDto.EsExtraComunitario) },
            "proveedor" => new HashSet<string> { nameof(ProveedorDto.DireccionFiscal), nameof(ProveedorDto.RazonSocial), nameof(ProveedorDto.TipoFarPropuesto), nameof(ProveedorDto.CgPropuesto), nameof(ProveedorDto.DomiciliadaEn), nameof(ProveedorDto.Tarjeta) },
            "solicitante" => new HashSet<string> { nameof(InterlocutorDto.DireccionDeContacto) },
            "MiSociedad" => new HashSet<string> {
                nameof(SociedadDto.DireccionFiscal),
                nameof(SociedadDto.IdInterlocutor), nameof(SociedadDto.Interlocutor),
                nameof(SociedadDto.IdCliente), nameof(SociedadDto.Cliente),
                nameof(SociedadDto.IdProveedor), nameof(SociedadDto.Proveedor),
                nameof(SociedadDto.IdAbogado), nameof(SociedadDto.IdProcurador),
                nameof(SociedadDto.Cgs), nameof(SociedadDto.Agenda)
            },
            _ => new HashSet<string>()
        };

        private static List<TablaPlantillaDto> DefinirTablasDto(Type tipoDtm)
        {
            var tablas = new List<TablaPlantillaDto>();
            var negocio = tipoDtm.NegocioDeUnDtm();
            var detalles = negocio.TiposDeDetalles();
            foreach (var detalle in detalles) tablas.Add(new TablaPlantillaDto(detalle.ToDto()));
            if (negocio.UsaFlujo()) tablas.Add(new TablaPlantillaDto(typeof(HitoDto), enumEncabezadosDeTablas.Hitos));
            if (negocio.UsaObservaciones()) tablas.Add(new TablaPlantillaDto(typeof(ObservacionDto), enumEncabezadosDeTablas.Observaciones));
            if (negocio.UsaDirecciones()) tablas.Add(new TablaPlantillaDto(typeof(DireccionDto), enumEncabezadosDeTablas.Direcciones));
            return tablas;
        }

    }
}
