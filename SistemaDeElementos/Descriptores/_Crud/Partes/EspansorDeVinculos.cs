using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Descriptores
{
    public class EspansorDeVinculos
    {
        public IControlConIdNegocioConExpansor Padre { get; }
        public enumNegocio Vinculado { get; private set; }
        public string Titulo { get; private set; }

        public int Posicion { get; set; } = 0;
        public DescriptorDeColumnas Columnas { get; private set; }
        public Dictionary<string, object> Parametros { get; private set; }

        public bool EsParaConsulta => Padre is DescriptorDePaginaDeConsulta;
        
        public EspansorDeVinculos(IControlConIdNegocioConExpansor padre, enumNegocio vinculado, string titulo, DescriptorDeColumnas columnas, Dictionary<string, object> parametros)
        {
            Padre = padre;
            Vinculado = vinculado;
            Titulo = titulo;
            Columnas = columnas;
            Parametros = parametros;
        }


        public void DefinirDescriptorDeVinculos()
        {
            var expansor = new DescriptorDeExpansor(Padre, $"{Padre.Id}-{Vinculado.ToString().ToLower()}", Vinculado.ToString(), true, Titulo);
            Padre.Expanes.Insert(Posicion, expansor);
            var gridDeRelacion = new GridDeRelacion(expansor, Columnas, Parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
        }

    }
}
