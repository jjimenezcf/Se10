using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Negocio;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos.Elemento;
using System.Collections.Generic;
using Utilidades;


namespace MVCSistemaDeElementos.Descriptores
{
    public class EspansorDeObservaciones
    {
        public IControlConIdNegocioConExpansor Padre { get; }

        public bool EsParaConsulta => Padre is DescriptorDePaginaDeConsulta;

        public EspansorDeObservaciones(IControlConIdNegocioConExpansor padre)
        {
            Padre = padre;
        }    

        public void DefinirDescriptorDeObservacion()
        {
            var expansor = new DescriptorDeExpansor(Padre, $"{Padre.Id}-observaciones", "Observaciones", true, "Observaciones de usuario");
            Padre.Expanes.Add(expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("observaciones");
            columnas.Add(titulo: "Asunto", propiedad: nameof(ObservacionDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);            
            if (!EsParaConsulta)
            {
                columnas.Add(titulo: "Creada por", propiedad: nameof(ObservacionDto.Creador), alineacion: enumAliniacion.izquierda, tamano: 200, mostrar: true);
            }
            columnas.Add(titulo: "Creada el", propiedad: nameof(ObservacionDto.CreadaEl), tamano: 150, formato: enumFormato.FechaTiempo);
            columnas.Add(titulo: "Id", propiedad: nameof(ObservacionDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: nameof(ObservacionDto.Descripcion), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(ObservacionesController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(ObservacionesController.epLeerElementos)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ObservacionDtm.IdElemento) }
                 , { nameof(GridDeRelacion.OcultarSiVacio), true}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            if (Padre is DescriptorDePaginaDeConsulta)
            {
                gridDeRelacion.PermitirEditar = false; 
                gridDeRelacion.acciones.Add(new ColumnaAccion
                {
                    accion = Referencia.MostrarPropiedad(expansor, nameof(ObservacionDto.Descripcion), "Detalle"),
                    titulo = "Detalle",
                    tamano = 100,
                    visible = true
                });
            }
            else
            {
                expansor.DescriptorDeCrearRelaciones(Padre.Contexto, typeof(ObservacionDto), typeof(ObservacionesController), nameof(ObservacionDto.IdElemento), "Añadir observación");
                expansor.DescriptorDeEditarRelaciones(Padre.Contexto, typeof(ObservacionDto), typeof(ObservacionesController), "Editar observacion", soloConsulta: false);
            }
        }


    }
}
