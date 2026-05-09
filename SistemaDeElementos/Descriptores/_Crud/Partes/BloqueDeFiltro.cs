using System;
using System.Collections.Generic;
using Utilidades;
using ModeloDeDto;
using ServicioDeDatos;
using Microsoft.EntityFrameworkCore.Query.Internal;
using System.Linq;

namespace MVCSistemaDeElementos.Descriptores
{

    public class BloqueDeFitro<TElemento> : ControlFiltroHtml where TElemento : ElementoDto
    {
        public TablaFiltro Tabla { get; set; }

        public ICollection<ControlFiltroHtml> Controles => Tabla.Controles;

        public bool HayExpansor { get; private set; } = false;

        public bool Plegado { get; set; } = false;

        public bool ConSpan => ZonaDeFiltrado.Bloques.Count > 1;

        public ZonaDeFiltro<TElemento> ZonaDeFiltrado => (ZonaDeFiltro<TElemento>)Padre;

        public BloqueDeFitro(ZonaDeFiltro<TElemento> filtro, string titulo, Dimension dimension)
        : base(
          padre: filtro,
          id: $"{filtro.Id}_{filtro.Bloques.Count}_bloque",
          etiqueta: titulo,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.Bloque;
            Tabla = new TablaFiltro(this, dimension, new List<ControlFiltroHtml>());
            filtro.Bloques.Add(this);
            Plegado = false;
        }


        public void AnadirControl(ControlFiltroHtml c)
        {

            this.AnadirControlEn(c);
            //Controles.Add(c);
            // AjustarDimensionDeLaTabla();
        }

        private void AjustarDimensionDeLaTabla()
        {
            foreach (var control in Controles)
            {
                if (control.Tipo == enumTipoControl.GridModal)
                    continue;
                if (control.Posicion.fila >= Tabla.Dimension.Filas)
                    Tabla.Dimension.NumeroDeFilas(control.Posicion.fila + 1);
                if (control.Posicion.columna >= Tabla.Dimension.Columnas)
                    Tabla.Dimension.NumeroDeColumnas(control.Posicion.columna + 1);
            }

            var numeroColumnas = Tabla.Dimension.Columnas;
            var maximoColuma = 0;
            foreach (var control in Controles)
            {
                if (control.Tipo == enumTipoControl.GridModal)
                    continue;
                if (maximoColuma < control.Posicion.columna)
                    maximoColuma = control.Posicion.columna;
            }
            if (maximoColuma < numeroColumnas - 1)
                Tabla.Dimension.NumeroDeColumnas(maximoColuma + 1);
        }

        public void AnadirControlEn(ControlFiltroHtml nuevoControl)
        {
            if (nuevoControl.Posicion != null)
            {
                if (Tabla.HayControlEn(nuevoControl.Posicion) && !nuevoControl.Etiqueta.IsNullOrEmpty())
                    DesplazarControles(nuevoControl);
            }
            Controles.Add(nuevoControl);
        }

        public void QuitarControl(string propiedad)
        {
            var control = Tabla.Controles.FirstOrDefault(x => x.Propiedad.ToLower() == propiedad.ToLower());
            Controles.Remove(control);
        }

        private void DesplazarControles(ControlFiltroHtml control)
        {
            if (control.Posicion.columna < Tabla.Dimension.Columnas)
                EmpujarHaciaDerecha(Tabla.ControlEn(control.Posicion));
            else
                EmpujarHaciaAbajo(Tabla.ControlEn(control.Posicion));
        }

        private void EmpujarHaciaAbajo(ControlFiltroHtml controlAEmpujar)
        {
            var nuevaPosicion = new Posicion { fila = controlAEmpujar.Posicion.fila + 1, columna = 0 };
            if (Tabla.HayControlEn(nuevaPosicion))
                DesplazarControles(Tabla.ControlEn(nuevaPosicion));
            controlAEmpujar.Posicion.columna = 0;
            controlAEmpujar.Posicion.fila++;
        }

        private void EmpujarHaciaDerecha(ControlFiltroHtml controlAEmpujar)
        {
            if (controlAEmpujar.Posicion.columna + 1 == Tabla.Dimension.Columnas)
                EmpujarHaciaAbajo(controlAEmpujar);
            else for (int columna = controlAEmpujar.Posicion.columna + 1; columna < Tabla.Dimension.Columnas; columna++)
                {
                    var nuevaPosicion = new Posicion { fila = controlAEmpujar.Posicion.fila, columna = columna };
                    if (Tabla.HayControlEn(nuevaPosicion))
                        DesplazarControles(Tabla.ControlEn(nuevaPosicion));
                    controlAEmpujar.Posicion.columna = columna;
                }
        }

        public void AnadirSelectorElemento<t1>(ListaDeElemento<t1> s) where t1 : ElementoDto
        {
            AnadirControl(s);
        }
        public void AnadirSelectorElemento<t1>(ListasDinamicas<t1> s) where t1 : ElementoDto
        {
            AnadirControl(s);
        }

        public void AnadirSelector<t1, t2>(SelectorDeFiltro<t1, t2> s) where t1 : ElementoDto where t2 : ElementoDto
        {
            AnadirControl(s);
            AnadirControl(s.Modal);
        }

        public void AnadirLista<t1>(ListaDeValores<t1> s) where t1 : ElementoDto
        {
            AnadirControl(s);
        }

        public ControlHtml ObtenerControl(string id)
        {

            foreach (ControlHtml c in Controles)
            {
                if (c.Id == id)
                    return c;
            }

            throw new Exception($"El control {id} no está en la zona de filtrado");
        }

        public string RenderModalesBloque()
        {
            var htmlModalesEnBloque = "";
            foreach (ControlHtml c in Controles)
            {
                if (c.Tipo == enumTipoControl.GridModal)
                    htmlModalesEnBloque =
                        $@"{htmlModalesEnBloque}{(htmlModalesEnBloque.IsNullOrEmpty() ? "" : Environment.NewLine)}" +
                        $"{c.RenderControl()}";
                //ModalDeSeleccionDeFiltro.RenderModalDeSeleccionDeFiltro()
            }
            return htmlModalesEnBloque;
        }

        public override string RenderControl()
        {
            string bloqueHtml;
            if (((ZonaDeFiltro<TElemento>)Padre).EsHistorial)
            {
                bloqueHtml = $@"
                     <div id='mostrar.crud_historialdto_mantenimiento_filtro_0_bloque' class='{enumCssCuerpo.CuerpoDatosFiltroBloque.Render()}'>
                         <div id='filtro-bloque-historial' class='{enumCssMnt.MntTablaDeFiltro.Render()}'>
                            <div class='{enumCssFiltro.FilaFiltroSinSpan.Render()}' style='grid-template-columns: 1fr 1fr 1fr 1fr;'>
                                  {RenderControlInputDeFiltro(IdHtml, id: ltrSucesosFiltros.referencia, etiqueta: "Referencia", criterio: enumCriteriosDeFiltrado.igual, ayuda:"indique las referencias separadas por ;")}
                                  {RenderControlInputDeFiltro(IdHtml, id: ltrSucesosFiltros.suceso, etiqueta: "Suceso", criterio: enumCriteriosDeFiltrado.igual, ayuda: "indique el contenido separados por ;")}
                                  {RenderControlListaDeValoresFiltro(IdHtml, id: ltrSucesosFiltros.excluir, etiqueta: "Excluir", opciones: ZonaDeFiltrado.Historial.OpcionesDeFiltrado, criterio: enumCriteriosDeFiltrado.igual, ayuda: "seleccione para excluir", alSeleccionar: enumFunctionTs.Historial_ExcluirClaseDeObjeto)}
                                  {RenderControlListaDeValoresFiltro(IdHtml, id: ltrSucesosFiltros.excluidos, etiqueta: "Excluidos", opciones: new Dictionary<string, string> { { "-1", "incluir el ..." }, }, criterio: enumCriteriosDeFiltrado.igual, ayuda: "Seleccione para incluir",alSeleccionar: enumFunctionTs.Historial_IncluirClaseDeObjeto)}
                            </div>
                         </div>
                     </div>
                    ";
            }
            else
            {
                AjustarDimensionDeLaTabla();
                var espan = $@"
                        <a id=¨mostrar.{IdHtml}.ref¨ 
                           style=¨margin-left: 10px;¨
                           href=¨javascript:Crud.{enumGestorDeEventos.EventosDelMantenimiento}('{eventosDeMnt.OcultarMostrarBloque}', '{IdHtml}');¨>                           
                        bloque: {Etiqueta}
                        </a>
                        <input id=¨expandir.{IdHtml}.input¨ type=¨hidden¨ value={(Plegado ? Literal.Uno : Literal.Cero)}> 
                        ";

                var contenedor = 
                    $@"
                     <div id=¨mostrar.{IdHtml}¨ class=¨{Css.Render(enumCssCuerpo.CuerpoDatosFiltroBloque)}¨> 
                           {(!ConSpan ? "" : espan)}
                           [bloque]
                      </div>";

                bloqueHtml = ConSpan
                   ? @$"
                       <div id=¨{IdHtml}¨  class=¨{Css.Render(!Plegado ? enumCssDiv.DivVisible : enumCssDiv.DivOculto)}¨>
                         {Tabla.RenderTabla(enumCssFiltro.FilaFiltro)}
                       </div>
                   "
                   : $"{Tabla.RenderTabla(enumCssFiltro.FilaFiltroSinSpan)}";

                bloqueHtml = contenedor.Replace("[bloque]", bloqueHtml);
            }

            return bloqueHtml;
        }

        internal ControlFiltroHtml BuscarControlPorPropiedad(string propiedad)
        {
            foreach (ControlFiltroHtml c in Controles)
            {

                if (c.Id == $"{Id}_{c.Tipo.Render()}_{propiedad}")
                    return c;
            }
            return null;
        }
        internal ControlFiltroHtml BuscarControlPorEtiqueta(string etiqueta)
        {
            foreach (ControlFiltroHtml c in Controles)
            {

                if (c.Etiqueta == etiqueta)
                    return c;
            }
            return null;
        }
    }

}
