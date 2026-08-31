using GestorDeElementos.Extensores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.SistemaDocumental;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos
{
    public static class ExtensorDelPanelDelControl
    {

        public static List<EstadoDeDashBoardPorNegocio> DatosParaInicializarDashBoard(ContextoSe contexto)
        {
            var jDatos = (JObject)enumNegocio.Negocio.LeerParametroDeUsuario<JObject>(contexto, enumParametrosDeUsuario.USU_Disposicion_DashBoard);

            if (jDatos == null || !jDatos.HasValues)
                return new List<EstadoDeDashBoardPorNegocio>();

            return jDatos["estados"]?.ToObject<List<EstadoDeDashBoardPorNegocio>>() ?? new List<EstadoDeDashBoardPorNegocio>();
        }

        public static List<object> LeerInformacioParaDashBoard(ContextoSe contexto)
        {
            var resultado = new List<object>();

            foreach (var negocio in Enum.GetValues<enumNegocio>())
            {
                if (negocio == enumNegocio.No_Definido) continue;
                if (!negocio.UsaTipo() || !negocio.UsaEstado()) continue;

                var metadatos = negocio.ObtenerMetadatos(emitirError: false);
                if (metadatos?.TipoDtm == null || metadatos?.EstadoDtm == null) continue;

                if (negocio == enumNegocio.CircuitoDoc)
                {
                    LeerInformacionParaElDashBoardDeCircuitosDoc(contexto, resultado);
                }
                else if (negocio == enumNegocio.Expediente)
                {
                    LeerInformacionParaElDashBoardDeExpedientes(contexto, resultado);
                }
                else if (negocio == enumNegocio.Contrato)
                {
                    LeerInformacionParaElDashBoardDeContrato(contexto, resultado);
                }
                else
                {
                    var tipos = negocio.TiposConFlujo(contexto).Select(tipo => new { tipo.Id, tipo.Nombre, tipo.IdEstado }).ToList();

                    if (tipos.Count == 0)
                        continue;

                    var estados = negocio.Estados(contexto).Select(estado => new { estado.Id, estado.Nombre }).ToList();
                    var cantidades = ElementoDeProcesoSql.ObtenerCantidadDeElementosPorTipoYEstado(contexto, negocio.TipoDtm());
                    if (!cantidades.Any() || cantidades.Sum(c => c.Cantidad) == 0)
                        continue;

                    var url = $"{enumNameSpaceTs.EntornoSe}.{enumFunctionTs.AbrirVista}('{negocio.Controlador()}', '{negocio.VistaMvc(contexto).Accion}', null,event);";
                    resultado.Add(new
                    {
                        nombre = negocio.Singular(),
                        enumerado = negocio.ToString(),
                        numTipos = tipos.Count,
                        numEstados = estados.Count,
                        tipos,
                        estados,
                        cantidades,
                        url
                    });
                }

            }

            return resultado;

        }


        private static void LeerInformacionParaElDashBoardDeCircuitosDoc(ContextoSe contexto, List<object> resultado)
        {
            enumNegocio negocio = enumNegocio.CircuitoDoc;
            var tipos = negocio.TiposConFlujo(contexto).ToList();
            var estados = negocio.Estados(contexto).ToList();
            var cantidades = ElementoDeProcesoSql.ObtenerCantidadDeElementosPorTipoYEstado(contexto, negocio.TipoDtm());

            // IDs de los 4 tipos especiales (0 = no definido en este sistema)
            var idFichadas = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaFichada(errorSiNoEstaDefinido: false);
            var idLotes = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaLoteDePreasientos(errorSiNoEstaDefinido: false);
            var idEstimaciones = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaEstimacionDirecta(errorSiNoEstaDefinido: false);
            var idActFormativas = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaActividadesFormativas(errorSiNoEstaDefinido: false);
            var idsEspeciales = new HashSet<int>(new[] { idFichadas, idLotes, idEstimaciones, idActFormativas }.Where(id => id > 0));

            var grupo = AgregarGrupo(contexto, negocio, VariablesDeCircuitosDoc.Fichadas, $"{enumNegocio.CircuitoDoc}_Fichadas", tipos.FirstOrDefault(t => t.Id == idFichadas), cantidades, enumVistasSistemaDocumental.CrudFichadas);
            if (grupo != null) resultado.Add(grupo);

            grupo = AgregarGrupo(contexto, negocio, VariablesDeCircuitosDoc.LotesContables, $"{enumNegocio.CircuitoDoc}_LotesContables", tipos.FirstOrDefault(t => t.Id == idLotes), cantidades, enumVistasSistemaDocumental.CrudLotesContables);
            if (grupo != null) resultado.Add(grupo);

            grupo = AgregarGrupo(contexto, negocio, VariablesDeCircuitosDoc.EstimacionesDirectas, $"{enumNegocio.CircuitoDoc}_EstimacionesDirectas", tipos.FirstOrDefault(t => t.Id == idEstimaciones), cantidades, enumVistasSistemaDocumental.CrudEstimacionesDirectas);
            if (grupo != null) resultado.Add(grupo);

            grupo = AgregarGrupo(contexto, negocio, VariablesDeCircuitosDoc.ActividadesFormativas, $"{enumNegocio.CircuitoDoc}_ActividadesFormativas", tipos.FirstOrDefault(t => t.Id == idActFormativas), cantidades, enumVistasSistemaDocumental.CrudActividadesFormativas);
            if (grupo != null) resultado.Add(grupo);

            var tiposResto = tipos.Where(t => !idsEspeciales.Contains(t.Id)).ToList();
            LeerInformacionConjuntaDeNegocio(contexto, resultado, negocio, enumVistasSistemaDocumental.CrudCircuitosDoc, tiposResto, cantidades);
        }

        private static void LeerInformacionParaElDashBoardDeExpedientes(ContextoSe contexto, List<object> resultado)
        {
            enumNegocio negocio = enumNegocio.Expediente;
            var tipos = negocio.TiposConFlujo(contexto).ToList();
            var estados = negocio.Estados(contexto).ToList();
            var cantidades = ElementoDeProcesoSql.ObtenerCantidadDeElementosPorTipoYEstado(contexto, negocio.TipoDtm());

            var idActividades = VariablesDeExpedientes.IdDelTipoParaActividades(errorSiNoEstaDefinido: false);
            var idsProcedimientosJudiciales = VariablesDeExpedientes.IdDelTipoParaProcedimientosJudiciales(errorSiNoEstaDefinido: false);
            var idsEspeciales = new HashSet<int>(new[] { idActividades }.Concat(idsProcedimientosJudiciales).Where(id => id > 0));

            var grupo = AgregarGrupo(contexto, negocio, VariablesDeExpedientes.Actividades, $"{enumNegocio.Expediente}_Actividades", tipos.FirstOrDefault(t => t.Id == idActividades), cantidades, enumVistasAdministrativo.CrudActividades);
            if (grupo != null) resultado.Add(grupo);

            var tiposProcedimientosJudiciales = tipos.Where(t => idsProcedimientosJudiciales.Contains(t.Id)).ToList();
            LeerInformacionConjuntaDeNegocio(contexto, resultado, negocio, enumVistasAdministrativo.CrudExpedientes, tiposProcedimientosJudiciales, cantidades, VariablesDeExpedientes.ProcedimientosJudiciales, $"{enumNegocio.Expediente}_ProcedimientosJudiciales", $"clase={enumClaseDeExpediente.juridico}");

            var tiposResto = tipos.Where(t => !idsEspeciales.Contains(t.Id)).ToList();
            LeerInformacionConjuntaDeNegocio(contexto, resultado, negocio, enumVistasAdministrativo.CrudExpedientes, tiposResto, cantidades);
        }

        private static void LeerInformacionParaElDashBoardDeContrato(ContextoSe contexto, List<object> resultado)
        {
            enumNegocio negocio = enumNegocio.Contrato;
            var tipos = negocio.TiposConFlujo(contexto).ToList();
            var estados = negocio.Estados(contexto).ToList();
            var cantidades = ElementoDeProcesoSql.ObtenerCantidadDeElementosPorTipoYEstado(contexto, negocio.TipoDtm());

            var idCompras = tipos.Where(t => t is TipoDeContratoDtm && ((TipoDeContratoDtm)t).ClaseDeContrato == enumClaseDeContrato.Compra).Select(t => t.Id).FirstOrDefault();
            var idVentas = tipos.Where(t => t is TipoDeContratoDtm && ((TipoDeContratoDtm)t).ClaseDeContrato == enumClaseDeContrato.Venta).Select(t => t.Id).FirstOrDefault();
            var idMatriculas = tipos.Where(t => t is TipoDeContratoDtm && ((TipoDeContratoDtm)t).ClaseDeContrato == enumClaseDeContrato.MatriculaDeGuarderia).Select(t => t.Id).FirstOrDefault();

            string CrudDeMatriculas = $"{enumVistasJuridicos.CrudContratos}?clase={enumClaseDeContrato.MatriculaDeGuarderia}";
            string CrudDeCrudDeCompras = $"{enumVistasJuridicos.CrudContratos}?clase={enumClaseDeContrato.Compra}";
            string CrudDeContratosDeVentas = $"{enumVistasJuridicos.CrudContratos}?clase={enumClaseDeContrato.Venta}";

            var grupo = AgregarGrupo(contexto, negocio, VariablesDeContratos.ContratosDeCompra, $"{enumNegocio.Contrato}_Compras", tipos.FirstOrDefault(t => t.Id == idCompras), cantidades, CrudDeCrudDeCompras);
            if (grupo != null) resultado.Add(grupo);

            grupo = AgregarGrupo(contexto, negocio, VariablesDeContratos.ContratosDeVenta, $"{enumNegocio.Contrato}_Ventas", tipos.FirstOrDefault(t => t.Id == idVentas), cantidades, CrudDeContratosDeVentas);
            if (grupo != null) resultado.Add(grupo);

            grupo = AgregarGrupo(contexto, negocio, VariablesDeContratos.ContratosDeMatricula, $"{enumNegocio.Contrato}_Matriculas", tipos.FirstOrDefault(t => t.Id == idMatriculas), cantidades, CrudDeMatriculas);
            if (grupo != null) resultado.Add(grupo);
        }

        private static void LeerInformacionConjuntaDeNegocio(ContextoSe contexto, List<object> resultado, enumNegocio negocio, string vista, List<TipoConFlujoDtm> tiposResto, List<ElementoDeProcesoSql.CantidadPorTipoYEstado> cantidades, string nombre = null, string enumerado = null, string parametros = null)
        {
            if (tiposResto.Any())
            {
                var idsResto = tiposResto.Select(t => t.Id).ToHashSet();
                if (idsResto.Count > 0)
                {
                    var estadosResto = tiposResto
                        .SelectMany(t => negocio.ObtenerFlujo(contexto, t.Id))
                        .GroupBy(e => e.Id)
                        .Select(g => new { g.First().Id, g.First().Nombre })
                        .ToList<object>();

                    var idsEstadosResto = estadosResto
                        .Select(e => (int)((dynamic)e).Id)
                        .ToHashSet();
                    if (idsEstadosResto.Count > 0)
                    {
                        var cantidadesAgregadas = cantidades.Where(c => idsResto.Contains(c.IdTipo) && idsEstadosResto.Contains(c.IdEstado)).ToList();
                        if (cantidadesAgregadas.Sum(c => c.Cantidad) > 0)
                            resultado.Add(new
                            {
                                nombre = nombre ?? negocio.Singular(),
                                enumerado = enumerado ?? negocio.ToString(),
                                numTipos = tiposResto.Count,
                                numEstados = estadosResto.Count,
                                tipos = tiposResto,
                                estados = estadosResto,
                                cantidades = cantidadesAgregadas,
                                url = $"{enumNameSpaceTs.EntornoSe}.{enumFunctionTs.AbrirVista}('{negocio.Controlador()}', '{vista}', {(parametros == null ? "null" : $"'{parametros}'")},event);"
                            });
                    }
                }
            }
        }

        // Función local que construye y agrega una entrada al resultado
        private static object AgregarGrupo(ContextoSe contexto, enumNegocio negocio, string nombre, string enumerado, TipoConFlujoDtm? tipo, List<ElementoDeProcesoSql.CantidadPorTipoYEstado> cantidades, string vista)
        {
            if (tipo == null || tipo.Id <= 0)
            {
                return null;
            }

            var flujoDelTipo = negocio.ObtenerFlujo(contexto, tipo.Id);
            var idsEstados = flujoDelTipo.Select(e => e.Id).ToHashSet();

            if (!cantidades.Any(c => c.IdTipo == tipo.Id && idsEstados.Contains(c.IdEstado)))
                return null;

            var cantidadesAgregada = cantidades.Where(c => c.IdTipo == tipo.Id && idsEstados.Contains(c.IdEstado)).ToList();
            if (cantidadesAgregada.Sum(c => c.Cantidad) == 0)
                return null;

            var url = $"{enumNameSpaceTs.EntornoSe}.{enumFunctionTs.AbrirVista}('{negocio.Controlador()}', '{vista}', null,event);";
            return new
            {
                nombre = nombre,
                enumerado = enumerado,
                numTipos = 1,
                numEstados = idsEstados.Count,
                tipos = new[] { tipo }.ToList(),
                estados = flujoDelTipo.Select(e => new { e.Id, e.Nombre }).ToList<object>(),
                cantidades = cantidadesAgregada,
                url = url
            };
        }



    }
}
