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
        
        public static VistaPorTipoDeNegocio VistasPorTipoDeNegocio(this NegocioDtm negocio, int idTipo)
        {
            var json = enumNegocio.VistaMvc.Parametro(enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio, crearParametro: true, valorPorDefecto: _jsonDeVista_Por_Tipo_De_Negocio).Valor;
            if (json == _jsonDeVista_Por_Tipo_De_Negocio)
                GestorDeErrores.Emitir($"Cuando un negocio se representa con varias vistas, ha de definir el parámetro '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio}' donde para el id del tipo de elemento del negocio '{negocio.Enumerado}' se define el id de la vista que se usa");
            
            var relaciones = ParsearVistaPorTipoDeNegocio(json);
            if (relaciones.Count(x => x.IdNegocio == negocio.Id) == 0)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio}' para el id de negocio '{negocio.Id}' y id de tipo '{idTipo}', ya que el ide de negocio no está referenciado");

            var tipos = relaciones.Where(r => r.IdTipo == idTipo);
            if (tipos.Count() == 0)
                GestorDeErrores.Emitir($"Ha de configurar el parámetro '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio}' para el id de negocio '{negocio.Id}' y id de tipo '{idTipo}', ya que el tipo no está referenciado");

            if (tipos.Count() > 1)
                GestorDeErrores.Emitir($"El parámetro '{enumParametrosDeVistaMvc.Vista_Por_Tipo_De_Negocio}' para el id de negocio '{negocio.Id}' y id de tipo '{idTipo}' contiene más de un registro");

            return tipos.First();
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
