using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Expediente
{
    public static class AccionesDeIrpf
    {
        public const string N_CerrarArchivadorDeIrpf = "Cerrar archivador de irpf";
        public const string N_AbrirArchivadorDeIrpf = "Abrir archivador para irpf";

        public enum enumParametros { IdTipoExpediente, IdTipoArchivador };

        public static string PrefijoArchivadorIrpf(this ExpedienteDtm expediente) => $"Archivador del expediente '{expediente.Referencia}' de IRPF correspondiente al año";

        private static string NombreDeArchivadorIrpf(this ExpedienteDtm expediente, int anyo) => $"{expediente.PrefijoArchivadorIrpf()} {anyo}";

        public static void AbrirArchivadorDeIrpf(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            if (entorno.EsEventodeNegocio && entorno.Parametros.LeerValor(enumParametros.IdTipoExpediente, 0) != expediente.IdTipo)
                return;

            var parametros = ValidarParametrosDeIrpf(entorno);
            var archivador = expediente.Vinculados<ArchivadorDtm>(entorno.Contexto, new System.Collections.Generic.Dictionary<string, object> 
            { 
                { ltrParametrosNeg.IncluirBajas, true} 
            })
            .FirstOrDefault(x => x.Nombre == expediente.NombreDeArchivadorIrpf(parametros.anyo));
            if (archivador == null) archivador = VincularArchivadorParaIrpf(entorno, expediente, parametros);

            if (archivador.Baja == true)
            {
                archivador.Baja = false;
                archivador.Modificar(entorno.Contexto, nameof(AbrirArchivadorDeIrpf));
            }
        }

        public static void CerrarArchivadorDeIrpf(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            var parametros = ValidarParametrosDeIrpf(entorno);
            var archivador = expediente.Vinculados<ArchivadorDtm>(entorno.Contexto, new System.Collections.Generic.Dictionary<string, object>
            {
                { ltrParametrosNeg.IncluirBajas, true}
            })
            .FirstOrDefault(x => x.Nombre == expediente.NombreDeArchivadorIrpf(parametros.anyo));
            if (archivador == null) archivador = VincularArchivadorParaIrpf(entorno, expediente, parametros);

            if (archivador.Baja == false)
            {
                archivador.Baja = true;
                archivador.Modificar(entorno.Contexto, nameof(CerrarArchivadorDeIrpf));
            }
        }

        private static ArchivadorDtm VincularArchivadorParaIrpf(EntornoDeUnaAccion entorno, ExpedienteDtm expediente, (int idTipoArchivador, int anyo) parametros)
        {
            ArchivadorDtm archivador = new ArchivadorDtm
            {
                IdCg = expediente.IdCg,
                IdTipo = parametros.idTipoArchivador,
                Nombre = expediente.NombreDeArchivadorIrpf(parametros.anyo),
                Descripcion = $"Documentación del ejercicio {DateTime.Now.Year} para la elaboración del irpf del expediente '{expediente.Referencia}'"
            }.Insertar(entorno.Contexto, nameof(AbrirArchivadorDeIrpf), parametros: new Dictionary<string, object> { { ltrParametrosNeg.CrearPermisosDelElemento, true } });
            expediente.Vincular(entorno.Contexto, archivador, parametros: new Dictionary<string, object> { { ltrCliente.OtorgarPermisos, true } });
            return archivador;
        }

        private static (int idTipoArchivador, int anyo) ValidarParametrosDeIrpf(EntornoDeUnaAccion entorno)
        {
            var idTipoArchivador = entorno.Parametros.LeerValor(enumParametros.IdTipoArchivador, 0);
            if (idTipoArchivador == 0) GestorDeErrores.Emitir($"la accion '{entorno.Accion.Nombre}' está mal parametrizada, el valor de '{enumParametros.IdTipoArchivador}' ha de ser mayor de '0'");

            return (idTipoArchivador, DateTime.Now.Year);
        }
    }
}
