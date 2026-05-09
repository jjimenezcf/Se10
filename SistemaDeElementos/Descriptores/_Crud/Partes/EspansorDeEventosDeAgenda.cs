using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using System.Collections.Generic;
using Utilidades;


namespace MVCSistemaDeElementos.Descriptores
{
    public class EspansorDeEventosDeAgenda
    {
        public IControlConIdNegocioConExpansor Padre { get; }

        public bool EsParaConsulta => Padre is DescriptorDePaginaDeConsulta;

        public EspansorDeEventosDeAgenda(IControlConIdNegocioConExpansor padre)
        {
            Padre = padre;
        }

        public void DefinirDescriptorDeEventos()
        {
            var expansor = new DescriptorDeExpansor(Padre, $"{Padre.Id}-eventos", "Eventos", true, "Eventos asociados");
            Padre.Expanes.Add(expansor);

            //Definimos el grid de detalles del cuerpo
            var columnas = new DescriptorDeColumnas("eventos");
            columnas.Add(titulo: "Agenda", propiedad: nameof(EventoDeAgendaDto.Agenda), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: "Evento", propiedad: nameof(EventoDeAgendaDto.Nombre), alineacion: enumAliniacion.izquierda, mostrar: true);
            //if (EsParaConsulta)
            //{
            //    columnas.Add(titulo: "Detalle", propiedad: nameof(EventoDeAgendaDto.Descripcion), alineacion: enumAliniacion.izquierda, mostrar: true);
            //}
            columnas.Add(titulo: "Desde", propiedad: nameof(EventoDeAgendaDto.Inicio), alineacion: enumAliniacion.centrada, mostrar: true, tamano: 150, formato: enumFormato.FechaHoraMinutos);
            columnas.Add(titulo: "Hasta", propiedad: nameof(EventoDeAgendaDto.Fin), alineacion: enumAliniacion.centrada, mostrar: true, tamano: 150, formato: enumFormato.FechaHoraMinutos);
            columnas.Add(titulo: "Id de agenda", propiedad: nameof(EventoDeAgendaDto.IdAgenda), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: "Id", propiedad: nameof(InterlocutorDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            columnas.Add(titulo: nameof(EventoDeAgendaDto.Descripcion), propiedad: nameof(EventoDeAgendaDto.Descripcion), alineacion: enumAliniacion.izquierda, mostrar: false);
            var parametros = new Dictionary<string, object> {
                   { nameof(GridDeRelacion.Controlador), typeof(VisorDeAgendaController) }
                 , { nameof(GridDeRelacion.AccionDeConsulta), nameof(VisorDeAgendaController.epLeerLosEventosDel)}
                 , { nameof(GridDeRelacion.PropiedadRestrictora), nameof(EventoDeAgendaDto.IdElemento) }
                 , { nameof(ltrParametrosEp.idVinculado) , NegociosDeSe.IdNegocio(enumNegocio.EventoDeAgenda) }
                 , { nameof(GridDeRelacion.OcultarSiVacio), false}
                };
            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            if (Padre is DescriptorDePaginaDeConsulta)
            {
                gridDeRelacion.PermitirEditar = false;
                gridDeRelacion.PermitirBorrar = false;
                gridDeRelacion.acciones.Add(new ColumnaAccion
                {
                    accion = Referencia.MostrarPropiedad(expansor, nameof(EventoDeAgendaDto.Descripcion), "Detalle del evento"),
                    titulo = "Detalle",
                    tamano = 100,
                    visible = true
                });
            }
            else
            {
                gridDeRelacion.PermitirBorrar = true;
                gridDeRelacion.acciones.Add(new ColumnaAccion
                {
                    accion = Referencia.Agenda_AbrirAgenda(expansor),
                    titulo = "Agenda",
                    tamano = 100,
                    visible = true
                });
                var modalCrearEvento = expansor.DescriptorDeCrearVinculos(Padre.Contexto, typeof(EventoDeAgendaDto), nameof(VisorDeAgendaController), "Crear evento");
                modalCrearEvento.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.ApiDeAgenda}.{enumFunctionTs.InicializarModalDeCreacion}('{modalCrearEvento.IdHtml}')";

                var modalEditarEvento = expansor.DescriptorDeEditarRelaciones(Padre.Contexto, typeof(EventoDeAgendaDto), typeof(VisorDeAgendaController), "Editar evento", soloConsulta: false);
                modalEditarEvento.AccionTrasAbrirModal = $"javascript: {enumNameSpaceTs.ApiDeAgenda}.{enumFunctionTs.InicializarModalDeEdicion}('{gridDeRelacion.idModalParaEditar.ToLower()}')";
            }

        }
    }
}
