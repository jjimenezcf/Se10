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
    public static class AccionesDePIM
    {
        public const string N_AbrirArchivadoresDePIM = "Abrir archivadores para PIM";

        public enum enumParametros { IdTipoExpediente, IdTipoArchivador };

        public static string PrefijoArchivadorPIM(this ExpedienteDtm expediente) => $"Impuestos del exp: '{expediente.Referencia}' correspondiente al ";

        private static string NombreDeArchivadorPIM(this ExpedienteDtm expediente, int anyo) => $"{expediente.PrefijoArchivadorPIM()} {anyo}";

        public static void AbrirArchivadoresDePIM(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            if (entorno.EsEventodeNegocio && entorno.Parametros.LeerValor(enumParametros.IdTipoExpediente, 0) != expediente.IdTipo)
                return;

            var parametros = ValidarParametrosDePIM(entorno);
            CrearArchivadorDe(entorno, expediente, parametros);
            parametros.anyo = parametros.anyo + 1;
            CrearArchivadorDe(entorno, expediente, parametros);
        }

        private static void CrearArchivadorDe(EntornoDeUnaAccion entorno, ExpedienteDtm expediente, (int idTipoArchivador, int anyo) parametros)
        {
            var archivador = expediente.Vinculados<ArchivadorDtm>(entorno.Contexto, new Dictionary<string, object>
            {
                { ltrParametrosNeg.IncluirBajas, true}
            })
            .FirstOrDefault(x => x.Nombre == expediente.NombreDeArchivadorPIM(parametros.anyo));
            if (archivador == null) archivador = VincularArchivadorParaPIM(entorno, expediente, parametros);

            if (archivador.Baja == true)
            {
                archivador.Baja = false;
                archivador.Modificar(entorno.Contexto, nameof(AbrirArchivadoresDePIM));
            }
        }

        private static ArchivadorDtm VincularArchivadorParaPIM(EntornoDeUnaAccion entorno, ExpedienteDtm expediente, (int idTipoArchivador, int anyo) parametros)
        {
            ArchivadorDtm archivador = new ArchivadorDtm
            {
                IdCg = expediente.IdCg,
                IdTipo = parametros.idTipoArchivador,
                Nombre = expediente.NombreDeArchivadorPIM(parametros.anyo),
                Descripcion = $"Impuestos del ejercicio {parametros.anyo} del expediente '{expediente.Referencia}'"
            }.Insertar(entorno.Contexto, nameof(AbrirArchivadoresDePIM), parametros: new Dictionary<string, object> { { ltrParametrosNeg.CrearPermisosDelElemento, true} });
            expediente.Vincular(entorno.Contexto, archivador, parametros: new Dictionary<string, object> { { ltrCliente.OtorgarPermisos, true } });
            return archivador;
        }

        private static (int idTipoArchivador, int anyo) ValidarParametrosDePIM(EntornoDeUnaAccion entorno)
        {
            var idTipoArchivador = entorno.Parametros.LeerValor(enumParametros.IdTipoArchivador, 0);
            if (idTipoArchivador == 0) GestorDeErrores.Emitir($"la accion '{entorno.Accion.Nombre}' está mal parametrizada, el valor de '{enumParametros.IdTipoArchivador}' ha de ser mayor de '0'");

            return (idTipoArchivador, DateTime.Now.Year -1);
        }
    }
}
