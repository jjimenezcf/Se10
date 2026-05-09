using DocumentFormat.OpenXml.Drawing.Diagrams;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Expediente
{
    public static class AccionesDeCGI
    {
        public const string N_CerrarArchivadoresDeCGI = "Cerrar archivadores de CGI";
        public const string N_AbrirArchivadoresDeCGI = "Abrir archivadores para CGI";

        public enum enumClaseDeCGI
        {
            [Description("Gastos")]
            Gatos,
            [Description("Ingresos")]
            Ingresos,
            [Description("Contabilizados")]
            Contabilizados,
            [Description("Documentación 1º Trimestre")]
            PrimerTrimestre,
            [Description("Documentación 2º Trimestre")]
            SegundoTrimestre,
            [Description("Documentación 3º Trimestre")]
            TercerTrimestre,
            [Description("Documentación 4º Trimestre")]
            CuartoTrimestre
        }

        public enum enumParametros { IdTipoExpediente, IdTipoArchivador };

        public static string PrefijoArchivadorCGI(this ExpedienteDtm expediente, enumClaseDeCGI claseDeCGI)
        =>
        $"{claseDeCGI.Descripcion()} del exp: '{expediente.Referencia}' de CGI correspondiente al ";

        private static string NombreDeArchivadorCGI(this ExpedienteDtm expediente, enumClaseDeCGI claseDeCGI, int anyo)
        =>
        $"{expediente.PrefijoArchivadorCGI(claseDeCGI)} {anyo}";

        public static void AbrirArchivadoresDeCGI(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            if (entorno.EsEventodeNegocio && entorno.Parametros.LeerValor(enumParametros.IdTipoExpediente, 0) != expediente.IdTipo)
                return;

            var parametros = ValidarParametrosDeCGI(entorno);
            parametros.anyo = expediente.FechaCreacion.Year;

            //CrearArchivadorDe(entorno, expediente, enumClaseDeCGI.Gatos, parametros);
            //CrearArchivadorDe(entorno, expediente, enumClaseDeCGI.Ingresos, parametros);
            //CrearArchivadorDe(entorno, expediente, enumClaseDeCGI.Contabilizados, parametros);

            CrearArchivadorDe(entorno, expediente, enumClaseDeCGI.PrimerTrimestre, parametros);
            CrearArchivadorDe(entorno, expediente, enumClaseDeCGI.SegundoTrimestre, parametros);
            CrearArchivadorDe(entorno, expediente, enumClaseDeCGI.TercerTrimestre, parametros);
            CrearArchivadorDe(entorno, expediente, enumClaseDeCGI.CuartoTrimestre, parametros);
        }

        private static void CrearArchivadorDe(EntornoDeUnaAccion entorno, ExpedienteDtm expediente, enumClaseDeCGI clase, (int idTipoArchivador, int anyo) parametros)
        {
            var archivador = expediente.Vinculados<ArchivadorDtm>(entorno.Contexto, new Dictionary<string, object>
            {
                { ltrParametrosNeg.IncluirBajas, true}
            })
            .FirstOrDefault(x => x.Nombre == expediente.NombreDeArchivadorCGI(clase, parametros.anyo));
            if (archivador == null )
            {
                if (entorno.EsEventodeNegocio)
                {
                    archivador = VincularArchivadorParaCGI(entorno, expediente, clase, parametros);
                    if (clase == enumClaseDeCGI.PrimerTrimestre) archivador.CrearCarpetasMensuales(entorno.Contexto, new List<string> { $"00-Facturas del {parametros.anyo-1}", "01-Enero", "02-Febrero", "03-Marzo" });
                    if (clase == enumClaseDeCGI.SegundoTrimestre) archivador.CrearCarpetasMensuales(entorno.Contexto, new List<string> { "04-Abril", "05-Mayo", "06-Junio" });
                    if (clase == enumClaseDeCGI.TercerTrimestre) archivador.CrearCarpetasMensuales(entorno.Contexto, new List<string> { "07-Julio", "08-Agosto", "09-Septiembre" });
                    if (clase == enumClaseDeCGI.CuartoTrimestre) archivador.CrearCarpetasMensuales(entorno.Contexto, new List<string> { "10-Octubre", "11-Noviembre", "12-Diciembre" });
                }
            }
            else if (archivador.Baja == true)
            {
                archivador.Baja = false;
                archivador.Modificar(entorno.Contexto, accionEjecutada: nameof(AbrirArchivadoresDeCGI));
            }
        }

        private static void CrearCarpetasMensuales(this ArchivadorDtm archivador, ContextoSe contexto, List<string> carpetas)
        {
            foreach (var carpeta in carpetas)
            {
                new CarpetaDtm
                {
                    Nombre = carpeta,
                    IdArchivador = archivador.Id
                }.Insertar(contexto, accionEjecutada: nameof(CrearCarpetasMensuales));
            }
        }

        public static void CerrarArchivadoresDeCGI(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;
            var transicion = entorno.Entrada.LeerValor<TransicionDtm>(nameof(TransicionDtm));

            var parametros = ValidarParametrosDeCGI(entorno);
            parametros.anyo = expediente.FechaCreacion.Year;
            CerrarArchivadorDe(entorno, expediente, enumClaseDeCGI.Gatos, parametros, transicion.EsCancelado);
            CerrarArchivadorDe(entorno, expediente, enumClaseDeCGI.Ingresos, parametros, transicion.EsCancelado);
            CerrarArchivadorDe(entorno, expediente, enumClaseDeCGI.Contabilizados, parametros, transicion.EsCancelado);

            CerrarArchivadorDe(entorno, expediente, enumClaseDeCGI.PrimerTrimestre, parametros, transicion.EsCancelado);
            CerrarArchivadorDe(entorno, expediente, enumClaseDeCGI.SegundoTrimestre, parametros, transicion.EsCancelado);
            CerrarArchivadorDe(entorno, expediente, enumClaseDeCGI.TercerTrimestre, parametros, transicion.EsCancelado);
            CerrarArchivadorDe(entorno, expediente, enumClaseDeCGI.CuartoTrimestre, parametros, transicion.EsCancelado);
        }

        private static void CerrarArchivadorDe(EntornoDeUnaAccion entorno, ExpedienteDtm expediente, enumClaseDeCGI claseDeCGI, (int idTipoArchivador, int anyo) parametros, bool cancelando)
        {
            var archivador = expediente.Vinculados<ArchivadorDtm>(entorno.Contexto, new Dictionary<string, object>
            {
                { ltrParametrosNeg.IncluirBajas, true}
            })
            .FirstOrDefault(x => x.Nombre == expediente.NombreDeArchivadorCGI(claseDeCGI, parametros.anyo));
            if (archivador != null && (archivador.Bloqueado == false || archivador.Baja == false))
            {
                if (cancelando)
                {
                    if (archivador.Baja)
                        return;
                    archivador.Baja = true;
                }
                else
                {
                    if (archivador.Bloqueado)
                        return;
                    archivador.Bloqueado = true;
                }
                archivador.Modificar(entorno.Contexto, nameof(ltrDeUnElemento.Accion_Bloquear));
            }
        }

        private static ArchivadorDtm VincularArchivadorParaCGI(EntornoDeUnaAccion entorno, ExpedienteDtm expediente, enumClaseDeCGI claseDeCGI, (int idTipoArchivador, int anyo) parametros)
        {
            ArchivadorDtm archivador = new ArchivadorDtm
            {
                IdCg = expediente.IdCg,
                IdTipo = parametros.idTipoArchivador,
                Nombre = expediente.NombreDeArchivadorCGI(claseDeCGI, parametros.anyo),
                Descripcion = $"{claseDeCGI.Descripcion()} a contabilizar del ejercicio {parametros.anyo} del expediente '{expediente.Referencia}'",
                Baja = false,
                Bloqueado = false,
            }.Insertar(entorno.Contexto, accionEjecutada: nameof(AbrirArchivadoresDeCGI), parametros: new Dictionary<string, object> { { ltrParametrosNeg.CrearPermisosDelElemento, true } });
            expediente.Vincular(entorno.Contexto, archivador, parametros: new Dictionary<string, object> { { ltrCliente.OtorgarPermisos, true } });
            return archivador;
        }

        private static (int idTipoArchivador, int anyo) ValidarParametrosDeCGI(EntornoDeUnaAccion entorno)
        {
            var idTipoArchivador = entorno.Parametros.LeerValor(enumParametros.IdTipoArchivador, 0);
            if (idTipoArchivador == 0)
                GestorDeErrores.Emitir($"la accion '{entorno.Accion.Nombre}' está mal parametrizada, el valor de '{enumParametros.IdTipoArchivador}' ha de ser mayor de '0'");

            return (idTipoArchivador, 0);
        }
    }
}
