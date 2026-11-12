using Gestor.Errores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Entorno
{
    public enum enumParametrosDeVistaMvc
    {
        [Description("Indica para un id de un tipo de un id de negocio, que id de vista a de usar")]
        Vista_Por_Tipo_De_Negocio,
        [Description("Indica si ha de bloqueear con capa alleer los vinculados")]
        Con_Capa_Los_Vinculados
    }

    public class VistaPorTipoDeNegocio
    {
        public int IdNegocio { get; set; }
        public int IdTipo { get; set; }
        public int IdVista { get; set; }
    }


    public static class VariablesDeVistasMvc
    {

        private static readonly string _jsonDeVista_Por_Tipo_De_Negocio = $"[{{\"IdNegocio\": 0,\"IdTipo\": 0,\"IdVista\": 0}}]";
        
        /// <summary>
        /// Se llama cuando hay más de una vista que usa el mismo dto del negocio. Orden de resolución:
        /// 1. terna (idNegocio, idTipo, idVista) del parámetro 'Vista_Por_Tipo_De_Negocio'
        /// 2. terna (idNegocio, 0, idVista): vista por defecto para cualquier tipo del negocio
        /// 3. por convención, la vista candidata llamada 'Crud' + plural del negocio (p.e. CrudExpedientes)
        /// 4. error
        /// </summary>
        public static VistaPorTipoDeNegocio VistasPorTipoDeNegocio(this NegocioDtm negocio, int idTipo, IEnumerable<VistaMvcDtm> vistasCandidatas = null)
        {
            var json = enumNegocio.VistaMvc.Parametro(enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio, crearParametro: true, valorPorDefecto: _jsonDeVista_Por_Tipo_De_Negocio).Valor;
            var parametroSinDefinir = json == _jsonDeVista_Por_Tipo_De_Negocio;

            var relaciones = parametroSinDefinir
                ? new List<VistaPorTipoDeNegocio>()
                : ParsearVistaPorTipoDeNegocio(json).Where(x => x.IdNegocio == negocio.Id).ToList();

            var porTipo = BuscarUnica(relaciones, negocio, idTipo);
            if (porTipo != null)
                return porTipo;

            var porDefecto = BuscarUnica(relaciones, negocio, 0);
            if (porDefecto != null)
                return porDefecto;

            var porConvencion = BuscarPorConvencion(negocio, vistasCandidatas);
            if (porConvencion != null)
                return new VistaPorTipoDeNegocio { IdNegocio = negocio.Id, IdTipo = idTipo, IdVista = porConvencion.Id };

            if (parametroSinDefinir)
                GestorDeErrores.Emitir($"Cuando un negocio se representa con varias vistas, ha de definir el parámetro '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio}' donde para el id del tipo de elemento del negocio '{negocio.Enumerado}' se define el id de la vista que se usa (o crear una vista llamada '{NombreDeVistaPorConvencion(negocio)}')");

            if (relaciones.Count == 0)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio}' para el id de negocio '{negocio.Id}' y id de tipo '{idTipo}', ya que el id de negocio no está referenciado");

            GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio}' para el id de negocio '{negocio.Id}' y id de tipo '{idTipo}' (o con id de tipo 0), ya que el tipo no está referenciado");
            return null;
        }

        private static VistaPorTipoDeNegocio BuscarUnica(List<VistaPorTipoDeNegocio> relaciones, NegocioDtm negocio, int idTipo)
        {
            var tipos = relaciones.Where(r => r.IdTipo == idTipo).ToList();
            if (tipos.Count > 1)
                GestorDeErrores.Emitir($"El parámetro '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio}' para el id de negocio '{negocio.Id}' y id de tipo '{idTipo}' contiene más de un registro");
            return tipos.FirstOrDefault();
        }

        private static string NombreDeVistaPorConvencion(NegocioDtm negocio)
        {
            return Enum.TryParse<enumNegocio>(negocio.Enumerado, out var enumerado)
                ? enumerado.Plural()
                : null;
        }

        private static VistaMvcDtm BuscarPorConvencion(NegocioDtm negocio, IEnumerable<VistaMvcDtm> vistasCandidatas)
        {
            var nombre = NombreDeVistaPorConvencion(negocio);
            if (vistasCandidatas == null || nombre.IsNullOrEmpty())
                return null;

            var encontradas = vistasCandidatas.Where(v => v.Nombre.Equals(nombre, StringComparison.OrdinalIgnoreCase)).ToList();
            return encontradas.Count == 1 ? encontradas[0] : null;
        }


        private static List<VistaPorTipoDeNegocio> ParsearVistaPorTipoDeNegocio(string json)
        {
            try
            {
                var jsonArray = JArray.Parse(json);
                return jsonArray.Select(item => new VistaPorTipoDeNegocio
                {
                    IdNegocio = item["IdNegocio"].Value<int>(),
                    IdTipo = item["IdTipo"].Value<int>(),
                    IdVista = item["IdVista"].Value<int>()
                }).ToList();
            }
            catch (Exception ex)
            {
                GestorDeErrores.Emitir($"Error al parsear el json: '{json}' al objeto del tipo '{_jsonDeVista_Por_Tipo_De_Negocio}', debe definirlo en el parámetro de negocio '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio.ToString()}'", ex);
            }
            return null;
        }

    }
}
