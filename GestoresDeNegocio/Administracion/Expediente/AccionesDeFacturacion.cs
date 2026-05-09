using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Expediente
{
    public static class AccionesDeFacturacion
    {
        public const string N_CerrarArchivadorDeFacturasParaElMes = "Cerrar archivador de facturas del mes";
        public const string N_AbrirArchivadorDeFacturasParaElMes = "Abrir archivador para facturas del mes";
        public const string N_AbrirExpedienteDeFacturasParaElEjercicio = "Abrir expediente para las facturas del año";

        public enum enumParametros { IdTipoExpediente, IdTipoArchivador, Mes };

        public static readonly string PermitirAperturaSiempre = nameof(PermitirAperturaSiempre);
        public static string PrefijoArchivadorFCT(this ExpedienteDtm expediente) => $"Archivador del {expediente.Referencia} correspondiente al mes de";
        private static string PrefijoExpedienteFCT(this ExpedienteDtm expediente) => $"Control de facturas del año [anyo], relacionado con el {expediente.Referencia}";

        public static string NombreDeArchivadorFCT(this ExpedienteDtm expediente, int mes) => $"{expediente.PrefijoArchivadorFCT()} {extFechas.Mes(mes)}";
        public static string NombreDeExpedienteFCT(this ExpedienteDtm expediente, int anyo) => expediente.PrefijoExpedienteFCT().Replace("[anyo]", $"{anyo}");

        public static void AbrirArchivadorDeFacturasParaElMes(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            if (entorno.EsEventodeNegocio && entorno.Parametros.LeerValor(enumParametros.IdTipoExpediente, 0) != expediente.IdTipo)
                return;

            var parametros = ValidarParametrosDeFCT(entorno);
            var archivador = expediente.Vinculados<ArchivadorDtm>(entorno.Contexto, new System.Collections.Generic.Dictionary<string, object> 
            { 
                { ltrParametrosNeg.IncluirBajas, true} 
            })
            .FirstOrDefault(x => x.Nombre == expediente.NombreDeArchivadorFCT(parametros.mes));
            if (archivador == null) archivador = VincularArchivadorParaFacturas(entorno, expediente, parametros);

            if (archivador.Baja == true)
            {
                archivador.Baja = false;
                archivador.Modificar(entorno.Contexto, nameof(AbrirArchivadorDeFacturasParaElMes));
            }
        }

        public static void CerrarArchivadorDeFacturasParaElMes(EntornoDeUnaAccion entorno)
        {
            var expediente = (ExpedienteDtm)entorno.Registro;

            var parametros = ValidarParametrosDeFCT(entorno);
            var archivador = expediente.Vinculados<ArchivadorDtm>(entorno.Contexto, new System.Collections.Generic.Dictionary<string, object>
            {
                { ltrParametrosNeg.IncluirBajas, true}
            })
            .FirstOrDefault(x => x.Nombre == expediente.NombreDeArchivadorFCT(parametros.mes));
            if (archivador == null) archivador = VincularArchivadorParaFacturas(entorno, expediente, parametros);

            if (archivador.Baja == false)
            {
                archivador.Baja = true;
                archivador.Modificar(entorno.Contexto, nameof(CerrarArchivadorDeFacturasParaElMes));
            }
        }

        public static void AbrirExpedienteDeFacturasParaElEjercicio(EntornoDeUnaAccion entorno)
        {
            if (entorno.EsEventodeNegocio)
                GestorDeErrores.Emitir($"la accion '{entorno.Accion.Nombre}' sólo es de transición");

            var expediente = (ExpedienteDtm)entorno.Registro;
            var anyoDeApertura = DateTime.Now.Year;
            var mes = DateTime.Now.Month;
            if (mes >= 2) anyoDeApertura = DateTime.Now.Year + 1;
            if (!entorno.Entrada.LeerValor(PermitirAperturaSiempre,false) && (mes != 12 || mes != 1))
                GestorDeErrores.Emitir("Solo se puede abrir un expediente para el control de facturas en diciembre o en enero");


            var nuevoExpediente = expediente.Vinculados<ExpedienteDtm>(entorno.Contexto).FirstOrDefault(x => x.Nombre == expediente.NombreDeExpedienteFCT(anyoDeApertura));
            if (nuevoExpediente is not null) 
                return;

            nuevoExpediente = VincularExpedienteParaFacturas(entorno, expediente, anyoDeApertura);
            nuevoExpediente.CopiarDireccionesDel(entorno.Contexto, expediente, enumCalificadorDireccion.contacto);
        }

        private static ArchivadorDtm VincularArchivadorParaFacturas(EntornoDeUnaAccion entorno, ExpedienteDtm expediente, (int idTipoArchivador, int mes) parametros)
        {
            ArchivadorDtm archivador = new ArchivadorDtm
            {
                IdCg = expediente.IdCg,
                IdTipo = parametros.idTipoArchivador,
                Nombre = expediente.NombreDeArchivadorFCT(parametros.mes),
                Descripcion = $"Facturas del periodo {extFechas.Mes(parametros.mes)} del ejercicio {DateTime.Now.Year}"
            }.Insertar(entorno.Contexto, nameof(AbrirArchivadorDeFacturasParaElMes));
            expediente.Vincular(entorno.Contexto, archivador);
            return archivador;
        }

        private static ExpedienteDtm VincularExpedienteParaFacturas(EntornoDeUnaAccion entorno, ExpedienteDtm expediente, int anyo)
        {
            ExpedienteDtm nuevoExpediente = new ExpedienteDtm
            {
                IdCg = expediente.IdCg,
                IdTipo = expediente.IdTipo,
                Nombre = expediente.NombreDeExpedienteFCT(anyo),
                Descripcion = $"Expediente del control de facturas del año {anyo}",
                IdSolicitante = expediente.IdSolicitante,
                IdResponsable = expediente.IdResponsable,
                Contacto = expediente.Contacto,
                Telefono = expediente.Telefono,
                eMail = expediente.eMail
            }.Insertar(entorno.Contexto, nameof(AbrirExpedienteDeFacturasParaElEjercicio));
            expediente.Vincular(entorno.Contexto, nuevoExpediente);
            return nuevoExpediente;
        }

        private static (int idTipoArchivador, int mes) ValidarParametrosDeFCT(EntornoDeUnaAccion entorno)
        {
            var mes = entorno.Parametros.LeerValor(enumParametros.Mes, 0);
            if (mes == 0) GestorDeErrores.Emitir($"la accion '{entorno.Accion.Nombre}' está mal parametrizada, el valor de '{enumParametros.Mes}' ha de ser mayor de '0' y menor de '12'");

            var idTipoArchivador = entorno.Parametros.LeerValor(enumParametros.IdTipoArchivador, 0);
            if (idTipoArchivador == 0) GestorDeErrores.Emitir($"la accion '{entorno.Accion.Nombre}' está mal parametrizada, el valor de '{enumParametros.IdTipoArchivador}' ha de ser mayor de '0'");

            return (idTipoArchivador, mes);
        }
    }
}
