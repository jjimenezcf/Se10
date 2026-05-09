using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Expediente;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.SistemaDocumental;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;
using static ServicioDeDatos.Literal;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeActividades : DescriptorDeCrud<ExpedienteDto>
    {

        public DescriptorDeActividades(ContextoSe contexto, string renderCache)
        : base(contexto, renderCache)
        {
        }

        public DescriptorDeActividades(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(ExpedientesController)
               , nameof(ExpedientesController.CrudActividades)
               , modo
               , rutaBase: enumNameSpaceTs.Administracion)
        {

            Mnt.Etiqueta = "Gestión de actividades";
            Editor.Etiqueta = "Actividad";
            Creador.Etiqueta = "Actividad";

            IncluirFiltrosDatosActividades();
            DescriptorDeSpanDeActividadesFormativas();
        }



        private void IncluirFiltrosDatosActividades()
        {

        }

        private void DescriptorDeSpanDeActividadesFormativas()
        {
            var expansor = new DescriptorDeExpansor(Editor, $"{Editor.Id}-actividadesformativas", "Actividades formativas", mostrarPlegado: true, "Actividades formativas del expedite");
            Editor.Expanes.Insert(0, expansor);
            var columnas = new DescriptorDeColumnas("actividad");
            columnas.Add(titulo: "Actividad", propiedad: nameof(CircuitoDocDto.Expresion), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(CircuitoDocDto.Estado), propiedad: nameof(CircuitoDocDto.Estado), alineacion: enumAliniacion.izquierda, mostrar: true);
            columnas.Add(titulo: nameof(CircuitoDocDto.Id), propiedad: nameof(CircuitoDocDto.Id), alineacion: enumAliniacion.derecha, mostrar: false);
            var parametros = new Dictionary<string, object> {
               { nameof(GridDeRelacion.Controlador), typeof(CircuitosDocController) },
               { nameof(GridDeRelacion.AccionDeConsulta), nameof(CircuitosDocController.epLeerElementos)},
               { nameof(GridDeRelacion.PropiedadRestrictora), nameof(ltrDeUnCircuito.IdExpedientePadre) },
               { nameof(GridDeRelacion.PaginaDondeNavegarAlEditar) , $"{nameof(CircuitosDocController)}/{nameof(CircuitosDocController.CrudActividadesFormativas)}?id={nameof(CircuitoDocDto.Id)}"},
               { nameof(GridDeRelacion.OcultarSiVacio), false}
            };

            var gridDeRelacion = new GridDeRelacion(expansor, columnas, parametros);
            gridDeRelacion.PermitirBorrar = false;
            gridDeRelacion.PermitirEditar = false;
            var idtipo = VariablesDeCircuitosDoc.IdDelTipoCircuitoDocParaActividadesFormativas();
            var nombreDelTipo = Contexto.SeleccionarPorId<TipoDeCircuitoDocDtm>(idtipo).Nombre;
            expansor.DescriptorDeEditarRelaciones(Editor.Crud.Contexto, typeof(CircuitoDocDto), typeof(CircuitosDocController), "Editar actividad", soloConsulta: false);
            var modal = expansor.DescriptorDeCrearVinculos(Editor.Crud.Contexto, typeof(CircuitoDocDto), nameof(CircuitosDocController), "Crear actividad");
            modal.AccionTrasAbrirModal = $"javascript: Crud.{enumGestorDeEventos.EventosDeExpansores}('{eventosDeExpansor.TrasAbrirModal}', '{modal.IdHtml};{ltrComandos.ProponerCg}|{ltrComandos.ProponerElTipo}={idtipo}={nombreDelTipo}')";
            modal.Vista = enumVistasSistemaDocumental.CrudActividadesFormativas;
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();
            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeAdministracion.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/Actividades.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeActividades('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
            return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
        }
    }
}
