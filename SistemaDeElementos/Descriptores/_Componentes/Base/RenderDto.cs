using System;
using System.Collections.Generic;
using Utilidades;
using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto;
using ServicioDeDatos;
using System.Linq;
using AutoMapper.Internal;

namespace MVCSistemaDeElementos.Descriptores
{

    public static class RenderDto
    {
        public static string RenderFilaParaElDto(DescriptorDeTabla tabla, short i)
        {
            var fila = tabla.ObtenerFila(i);
            var htmlColumnas = "";
            var estilo = "grid-template-columns:";
            var htmlFila =
                    $@"<div id='{fila.IdHtml}' name='tr_lbl_propiedad' class='{enumCssDiv.Tr.Render()} {enumCssEdicion.Tr.Render()} tr-propiedad' style='[grid-template-columns];'>
                         htmlColumnas
                       </div>
                      ";

            double anchoColumna = tabla.NumeroDeColumnas == 0 ? 0 : (double)(100 / tabla.NumeroDeColumnas);
            var filaVisible = false;
            for (short j = 0; j < tabla.NumeroDeColumnas; j++)
            {
                var info = RenderColumnaParaElDto(tabla, i, j, anchoColumna);
                var htmlColumna = info.Item1;
                estilo = estilo + " " + (info.Item2 ? "1fr" : "0px");
                if (info.Item2) filaVisible = true;
                htmlColumnas = htmlColumnas + htmlColumna;
            }
            if (!filaVisible) 
                estilo = estilo + "; " + "grid-template-rows=0px";
            return htmlFila.Replace("htmlColumnas", $"{htmlColumnas}").Replace("[grid-template-columns]", estilo);
        }

        private static (string, bool) RenderColumnaParaElDto(DescriptorDeTabla tabla, short i, short j, double anchoColumna)
        {
            var visible = tabla.ObtenerFila(i).ObtenerColumna(j).NumeroControlesVisibles > 0;
            if (!visible && tabla.NumeroDeColumnas >= j + 1)
            {
                for (var t = j + 1; t < tabla.NumeroDeColumnas; t++)
                {
                    foreach (var c in tabla.ObtenerColumna(i, (short)(t)).ObtenerControles())
                    {
                        if (c.Atributos.MantenerHuecoDeLaIzquierda)
                        {
                            var ultimoColSpanIndicado = UltimoColSpanIndicado(tabla, i, j);
                            visible = ultimoColSpanIndicado.pos + ultimoColSpanIndicado.colspan <= j;
                            break;
                        }
                    }
                    if (visible) break;
                }
            }

            var modoDisplaDelTd = tabla.ObtenerFila(i).ObtenerColumna(j).DisplaCss;

            var cssDelDivDelaTd = tabla.ObtenerFila(i).ObtenerColumna(j).CssDelDivDeLaCelda;
            var colspan = tabla.ObtenerFila(i).ObtenerColumna(j).ColSpan;
            var estilo = visible ? $"" : "display:none";
            var td = $@"<div id='{tabla.IdHtml}_{i}_{j}' 
                            name='td-propiedad' 
                            class='{enumCssDiv.Td.Render()} {enumCssEdicion.Td.Render()} td-propiedad' >
                            [renderControles]
                      </div>
                     ";
            var celda = RenderControlesParaMapearLaCeldaDelTd(tabla, i, j);
            var rendercontroles = $@"
                         <div id='{tabla.IdHtml}_{i}_{j}_celda' name='div-propiedad' [estiloCelda] class='div-propiedad [otraClase]'>
                              {celda.html}
                         </div>";

            var columna = tabla.ObtenerFila(i).ObtenerColumna(j);

            if (HayMasDeUnControlVisibleEnLaColumna(tabla, columna))
            {
                rendercontroles = rendercontroles.Replace("[estiloCelda]", $"style=¨{celda.estilo}¨");
                rendercontroles = rendercontroles.Replace("[otraClase]", $" {enumCssDiv.DivConMasPropiedades.Render()} {(AlinearElContenidoALaDerecha(tabla, columna) ? enumCssDiv.DivConConteidoAlineadoALaDerecha.Render() : "")}");
            }
            else
            {
                rendercontroles = rendercontroles.Replace("[estiloCelda]", "");
                rendercontroles = rendercontroles.Replace("[otraClase]", $"{(cssDelDivDelaTd == enumCssDiv.Nulo ? "" : cssDelDivDelaTd.Render())}");
            }

            td = td.Replace("[renderControles]", rendercontroles);

            return (td, visible);
        }

        private static (int pos, int colspan) UltimoColSpanIndicado(DescriptorDeTabla tabla, short i, short j)
        {
            for (var h = j - 1; h >= 0; h--)
            {
                if (tabla.ObtenerFila(i).ObtenerColumna((short)h).ColSpan> 0)
                {
                    return (h,tabla.ObtenerFila(i).ObtenerColumna((short)h).ColSpan);
                }
            }
            return (0,0);
        }

        private static (string html, string estilo) RenderControlesParaMapearLaCeldaDelTd(DescriptorDeTabla tabla, short i, short j)
        {
            var porcentajeDeEtiqueta = (short)ApiDeAtributos.ValorDelAtributo(tabla.Tipo, nameof(IUDtoAttribute.AnchoEtiqueta));
            var pocentajeDeControl = 100 - porcentajeDeEtiqueta;
            var porcentajeDelSeparador = (short)ApiDeAtributos.ValorDelAtributo(tabla.Tipo, nameof(IUDtoAttribute.AnchoSeparador));
            var columna = tabla.ObtenerFila(i).ObtenerColumna(j);
            var htmlControles = "";
            var estilo = "grid-template-columns: ";
            double anchoEtiqueta = columna.NumeroDeEtiquetasVisibles == 0 ? 0 : porcentajeDeEtiqueta / columna.NumeroDeEtiquetasVisibles;
            double anchoControl = columna.NumeroControlesVisibles == 0 ? 0 : (pocentajeDeControl - (porcentajeDelSeparador * (columna.NumeroControlesVisibles - 1))) / columna.NumeroControlesVisibles;
            bool anadirSeparador = false;
            if (columna.ColSpan > 1)
            {
                var ajuste = anchoEtiqueta / columna.ColSpan;
                anchoEtiqueta = anchoEtiqueta - ajuste;
                anchoControl = anchoControl + ajuste;
            }

            double anchoTotal = 0;
            var controlesVisibles = ControlVisibleEnLaColumna(tabla, columna);
            var hayMasDeUnControl = controlesVisibles > 1;

            for (short z = 0; z <= columna.PosicionMaxima; z++)
            {
                var descriptorControl = columna.ObtenerControlEnLaPosicion(z);
                string htmlSeparador = "";
                string htmlEtiqueta = "";

                if (descriptorControl == null || !descriptorControl.Atributos.EsVisible(tabla.ModoDeTrabajo))
                    continue;

                if (tabla.NoRenderizar != null && tabla.NoRenderizar.Contains(descriptorControl.propiedad, StringComparer.InvariantCultureIgnoreCase))
                    continue;

                if (anadirSeparador && !descriptorControl.Atributos.Oculto)
                {
                    htmlSeparador = $"<div id=¨{tabla.IdHtml}-{i}-{j}-separador-{z}¨ class=¨div-separardor-propiedad¨ style=¨width:2%¨></div>";
                    anchoTotal += 2;
                }

                if (!descriptorControl.Atributos.Etiqueta.IsNullOrEmpty())
                {
                    htmlEtiqueta = RenderEtiquetaDelDto(tabla, descriptorControl, i, j, anchoEtiqueta);
                    anchoTotal = anchoTotal + anchoEtiqueta;
                }
                else
                    anchoControl = 100;

                if (z == columna.PosicionMaxima)
                    anchoControl = 100 - anchoTotal;

                string htmlControl = RenderDescriptorControlDto(tabla, descriptorControl, anchoControl);
                anchoTotal = anchoTotal + anchoControl;

                anadirSeparador = true;
                if (descriptorControl.Atributos.Oculto)
                    htmlControles = htmlControles + Environment.NewLine + htmlControl;
                else
                {
                    if (hayMasDeUnControl)
                    {
                        var ancho = "auto";
                        if (!descriptorControl.Atributos.AnchoMaximoContenedor.IsNullOrEmpty())
                            ancho = descriptorControl.Atributos.AnchoMaximoContenedor;
                        else if (!descriptorControl.Atributos.AnchoMaximo.IsNullOrEmpty())
                            ancho = descriptorControl.Atributos.AnchoMaximo;
                        estilo = estilo + $" {ancho}";
                        htmlControles = htmlControles + Environment.NewLine +
                        //style=¨width:{100/controlesVisibles}%¨
                        $@"<div> 
                            {htmlEtiqueta + Environment.NewLine + htmlControl + Environment.NewLine + htmlSeparador}
                           </div>";
                    }
                    else
                        htmlControles = htmlControles + Environment.NewLine + htmlEtiqueta + Environment.NewLine + htmlControl;
                }
            }

            if (!hayMasDeUnControl)
            {
                estilo = "";
            }
            return (htmlControles, estilo);
        }

        private static bool HayMasDeUnControlVisibleEnLaColumna(DescriptorDeTabla tabla, DescriptorDeColumna columna)
        =>
        ControlVisibleEnLaColumna(tabla, columna) > 1;


        private static int ControlVisibleEnLaColumna(DescriptorDeTabla tabla, DescriptorDeColumna columna)
        {
            var controlesvisibles = 0;
            for (short z = 0; z <= columna.PosicionMaxima; z++)
            {
                var descriptorControl = columna.ObtenerControlEnLaPosicion(z);
                if (descriptorControl == null || !descriptorControl.Atributos.EsVisible(tabla.ModoDeTrabajo))
                    continue;

                if (tabla.NoRenderizar != null && tabla.NoRenderizar.Contains(descriptorControl.propiedad, StringComparer.InvariantCultureIgnoreCase))
                    continue;
                if (!descriptorControl.Atributos.Oculto)
                    controlesvisibles++;
            }

            return controlesvisibles;
        }

        private static bool AlinearElContenidoALaDerecha(DescriptorDeTabla tabla, DescriptorDeColumna columna)
        {
            for (short z = 0; z <= columna.PosicionMaxima; z++)
            {
                var descriptorControl = columna.ObtenerControlEnLaPosicion(z);
                if (descriptorControl == null || !descriptorControl.Atributos.EsVisible(tabla.ModoDeTrabajo))
                    continue;

                if (tabla.NoRenderizar != null && tabla.NoRenderizar.Contains(descriptorControl.propiedad, StringComparer.InvariantCultureIgnoreCase))
                    continue;
                if (descriptorControl.Atributos.Oculto)
                    continue;
                return descriptorControl.Atributos.AlinearContenido == enumAliniacion.derecha;
            }
            return false;
        }

        private static string RenderEtiquetaDelDto(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, short i, short j, double ancho)
        {
            if (descriptorControl.Atributos.TipoDeControl == enumTipoControl.Check)
                return "";
            if (descriptorControl.Atributos.TipoDeControl == enumTipoControl.Referencia)
                return "";
            if (descriptorControl.Atributos.TipoDeControl == enumTipoControl.SelectorDeUnArchivo)
            {
                var html = ControlHtml.RenderEtiquetaParaSeleccionarUnArchivo($"{descriptorControl.IdHtml}", descriptorControl.Atributos.Ayuda);
                html = DefinirFuncionPegarArchivo(html, tabla, descriptorControl);
                return html;
            }

            if (descriptorControl.Atributos.TipoDeControl == enumTipoControl.Archivo)
            {
                var html = ControlHtml.RenderEtiquetaParaSeleccionarUnaImagen($"{descriptorControl.IdHtml}", descriptorControl.Atributos.Etiqueta);
                html = DefinirFuncionPegarArchivo(html, tabla, descriptorControl);
                return html;
            }

            var otros = new Dictionary<string, string>();

            if (descriptorControl.Atributos.Alineada == enumAliniacion.derecha
                || descriptorControl.Atributos.TipoDeControl.Render() == "number"
                //|| descriptorControl.Atributos.TipoDeControl == enumTipoControl.SelectorDeFecha
                //|| descriptorControl.Atributos.TipoDeControl == enumTipoControl.SelectorDeFechaHora
                )
                otros.Add("clases", enumCssControles.EtiquetaDerecha.Render());
            otros.Add(nameof(IUPropiedadAttribute.Ayuda), descriptorControl.Atributos.Ayuda.Replace("[Negocio]", tabla.Negocio.ConArticulo()));

            return ControlHtml.RenderEtiqueta(descriptorControl.IdHtml, $"{tabla.IdHtml}_{i}_{j}_lbl_{descriptorControl.Atributos.Posicion}", descriptorControl.Atributos.Etiqueta, otros);
        }

        //

        private static string RenderDescriptorControlDto(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, double ancho)
        {
            var atributos = descriptorControl.Atributos;
            var htmdDescriptorControl = "";
            try
            {
                switch (atributos.TipoDeControl)
                {
                    case enumTipoControl.Editor:
                        if (atributos.Oculto)
                            htmdDescriptorControl = RenderControlOculto(tabla, descriptorControl);
                        else
                            htmdDescriptorControl = RenderEditor(tabla, descriptorControl);
                        break;
                    case enumTipoControl.Password:
                        htmdDescriptorControl = RenderEditor(tabla, descriptorControl);
                        break;
                    case enumTipoControl.RestrictorDeEdicion:
                        htmdDescriptorControl = RenderRestrictor(tabla, descriptorControl, ancho);
                        break;
                    case enumTipoControl.ListaDeElemento:
                        htmdDescriptorControl = RenderListaDeElemento(tabla, descriptorControl, ancho);
                        break;
                    case enumTipoControl.Enumerado:
                        htmdDescriptorControl = RenderListaDeEnumerados(tabla, descriptorControl);
                        break;
                    case enumTipoControl.ListaDinamica:
                        htmdDescriptorControl = RenderListaDinamica(tabla, descriptorControl, ancho);
                        break;
                    case enumTipoControl.Archivo:
                        htmdDescriptorControl = RenderSelectorImagen(tabla, descriptorControl, ancho);
                        break;
                    case enumTipoControl.SelectorDeUnArchivo:
                        htmdDescriptorControl = RenderSelectorDeUnArcivo(tabla, descriptorControl, ancho);
                        break;
                    case enumTipoControl.UrlDeArchivo:
                        htmdDescriptorControl = RenderSelectorImagen(tabla, descriptorControl, ancho);
                        break;
                    case enumTipoControl.Check:
                        htmdDescriptorControl = RenderCheck(tabla, descriptorControl);
                        break;
                    case enumTipoControl.SelectorDeFecha:
                        htmdDescriptorControl = RenderSelectorDeFecha(tabla, descriptorControl);
                        break;
                    case enumTipoControl.SelectorDeFechaHora:
                        htmdDescriptorControl = RenderSelectorDeFechaHora(tabla, descriptorControl);
                        break;
                    case enumTipoControl.AreaDeTexto:
                        htmdDescriptorControl = RenderAreaDeTexto(tabla, descriptorControl);
                        break;
                    case enumTipoControl.Referencia:
                        htmdDescriptorControl = RenderReferencia(tabla, descriptorControl);
                        break;
                    default:
                        GestorDeErrores.Emitir($"No se ha implementado como renderizar una propiedad del tipo {atributos.TipoDeControl}");
                        break;
                }
                if (htmdDescriptorControl.Contains("ampliacion=¨N¨")) htmdDescriptorControl = htmdDescriptorControl.Replace("ampliacion=¨N¨", "");

                htmdDescriptorControl = htmdDescriptorControl.QuitarDobleIntro();
                return htmdDescriptorControl;
            }
            catch (Exception e)
            {
                throw new Exception($"Fallo al renderizar el control de '{descriptorControl.propiedad}' del tipo {atributos.TipoDeControl} con id {descriptorControl.IdHtml} ", e);
            }
        }

        private static string RenderReferencia(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {
            var referencia = new Referencia(tabla,
                descriptorControl.IdHtml,
                descriptorControl.propiedad,
                descriptorControl.Atributos.Etiqueta,
                descriptorControl.Atributos.AccionRef,
                descriptorControl.Atributos.Ayuda.Replace("[Negocio]", tabla.Negocio.ConArticulo()),
                descriptorControl.Atributos.EnConsultaOcultar);
            return referencia.RenderReferencia(new List<enumCssControles> { descriptorControl.Atributos.css });
        }

        private static string RenderCheck(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {
            var atributos = descriptorControl.Atributos;
            if (atributos.ValorPorDefecto == default)
                atributos.ValorPorDefecto = false;
            Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            valores["Checked"] = atributos.ValorPorDefecto == default ? false.ToString().ToLower() : atributos.ValorPorDefecto.ToString().ToLower();
            valores["Etiqueta"] = atributos.Etiqueta;
            valores["Css"] = atributos.css.Render();
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto.ToString().ToLower();
            valores[nameof(IUPropiedadAttribute.Ayuda)] = atributos.Ayuda.ToString().ToLower();
            valores[nameof(IUPropiedadAttribute.CssDelContenedor)] = atributos.CssDelContenedor != enumCssControles.Nulo
                ? atributos.CssDelContenedor.Render()
                : enumCssControles.ContenedorCheck.Render();

            var htmlCheck = PlantillasHtml.Render(PlantillasHtml.checkDto, valores);

            htmlCheck = htmlCheck.Replace($"[{nameof(IUPropiedadAttribute.OnChange)}]", !atributos.OnChange.IsNullOrEmpty() ? $"onChange='{atributos.OnChange}'" : "");
            if (tabla.ModoDeTrabajo == enumModoDeTrabajo.Nuevo || tabla.ModoDeTrabajo == enumModoDeTrabajo.NuevaRelacion)
            {
                htmlCheck = htmlCheck.Replace($"[Checkeado]", (bool)atributos.ValorPorDefecto ? "Checked" : "");
                htmlCheck = htmlCheck.Replace($"[{nameof(IUPropiedadAttribute.EsAlmacenable)}]", atributos.EsAlmacenable ? $"es-almacenable='true'" : "");
            }
            else
            {
                htmlCheck = htmlCheck.Replace($"[{nameof(IUPropiedadAttribute.EsAlmacenable)}]", "");
                htmlCheck = htmlCheck.Replace($"[Checkeado]", "");
            }


            return htmlCheck;
        }

        private static string RenderListaDeEnumerados(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {
            var atributos = descriptorControl.Atributos;

            Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorListaDeElementos);
            valores["Css"] = Css.Render(enumCssControles.ListaDeElementos);
            valores["GuardarEn"] = descriptorControl.propiedad;

            var htmlSelect = PlantillasHtml.Render(PlantillasHtml.listaDeValoresDto, valores);

            Dictionary<string, string> opciones = atributos.Tipo.ToDiccionario();

            if (!atributos.OnReset.IsNullOrEmpty() && atributos.Obligatorio)
                GestorDeErrores.Emitir($"No se puede parametrizar el evento {nameof(atributos.OnReset)} e indicar que la lista de enumerados es Obligatoria");


                var opcionesHtml = !atributos.Obligatorio ? $"<option value='-1'>{atributos.Ayuda}</option>" : "";
            foreach (var clave in opciones.Keys)
                opcionesHtml = $"{opcionesHtml}{Environment.NewLine}<option value='{clave}' {(atributos.ValorPorDefecto != null && clave == atributos.ValorPorDefecto.ToString() ? "selected": "")}>{opciones[clave]}</option>";

            if (tabla.ModoDeTrabajo == enumModoDeTrabajo.Nuevo || tabla.ModoDeTrabajo == enumModoDeTrabajo.NuevaRelacion)
                htmlSelect = htmlSelect.Replace($"[deshabilitada]",
                    !atributos.EditableAlCrear ? $"editable='false' disabled readonly" : "editable='true'");
            else
            if (tabla.ModoDeTrabajo == enumModoDeTrabajo.Edicion)
                htmlSelect = htmlSelect.Replace($"[deshabilitada]",
                    !atributos.EditableAlEditar ? $"editable='false' disabled readonly" : "editable='true'");
            else
            if (tabla.ModoDeTrabajo == enumModoDeTrabajo.Jerarquia)
                htmlSelect = htmlSelect.Replace($"[deshabilitada]",
                    !atributos.EditableAlCrear ? $"editable='false' disabled readonly" : "editable='true'");

            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.OnBlur)}]", !atributos.OnBlur.IsNullOrEmpty() ? $"onBlur='{atributos.OnBlur}'" : "");
            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.OnChange)}]", !atributos.OnChange.IsNullOrEmpty() ? $"onChange='{atributos.OnChange}'" : "");
            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.OnReset)}]", !atributos.OnReset.IsNullOrEmpty() ? $"onReset='{atributos.OnReset}'" : "");
            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.EsAlmacenable)}]", atributos.EsAlmacenable ? $"es-almacenable='true'" : "");
            htmlSelect = htmlSelect.Replace($"[deshabilitada]", "");
            htmlSelect = htmlSelect.Replace("[opcionesDeLaLista]", opcionesHtml);
            return htmlSelect;
        }

        private static string RenderListaDinamica(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, double ancho)
        {
            var atributos = descriptorControl.Atributos;
            var valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            var propiedad = ApiDeEnsamblados.PropiedadesDelTipo(tabla.Tipo).Where(x => x.Name == atributos.GuardarEn).First();
            if (Nullable.GetUnderlyingType(propiedad.PropertyType) != null)
                valores["Obligatorio"] = "N";


            //valores["EstiloDelContenedor"] = atributos.ModoDisplayDelContenedor.IsNullOrEmpty() ? "" : $"display: {atributos.ModoDisplayDelContenedor};";

            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorListaDinamica);
            valores["Css"] = Css.Render(enumCssControles.ListaDinamica);
            if (atributos.SeleccionarDe == null)
            {
                if (atributos.Negocio != enumNegocio.No_Definido)
                    atributos.SeleccionarDe = ExtensionesDto.ObtenerTypoDto(NegociosDeSe.LeerNegocioPorEnumerado(atributos.Negocio).ElementoDto);
                else
                    GestorDeErrores.Emitir($"Falta indicar de donde se seleccionará los elementos para la lista dinámica '{atributos.Ayuda}', indíquelo en las propiedades del Dto o en el descriptor Json");
            }

            valores["ClaseElemento"] = atributos.SeleccionarDe.Name;
            valores["MostrarExpresion"] = atributos.MostrarExpresion.IsNullOrEmpty() ?
                ApiDeAtributos.ValorDelAtributo(atributos.SeleccionarDe, nameof(IUDtoAttribute.MostrarExpresion)) :
                atributos.MostrarExpresion;
            valores["BuscarPor"] = atributos.BuscarPor;
            valores["Longitud"] = atributos.LongitudMinimaParaBuscar;
            valores["AplicarJoin"] = atributos.AplicarJoin;
            valores["Cantidad"] = 10;
            valores["CriterioDeFiltro"] = atributos.CriterioDeBusqueda;

            if (!atributos.OnBlur.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Se ha definido para la lista {descriptorControl.propiedad} en el dto {tabla.Tipo.Name} el evento {nameof(atributos.OnBlur)}, se define el de {nameof(atributos.trasSeleccionar)}");

            var rutaTs = tabla.Padre is DescriptorDeFormulario ? enumNameSpaceTs.Formulario : enumNameSpaceTs.Crud;
            valores["OnInput"] = $"{rutaTs}.{enumGestorDeEventos.EventosDeListaDinamica}('{eventosDeListaDinamica.cargar}','{descriptorControl.IdHtml}')";
            valores["OnFocus"] = $"{rutaTs}.{enumGestorDeEventos.EventosDeListaDinamica}('{eventosDeListaDinamica.obtenerFoco}','{descriptorControl.IdHtml}')";
            valores["OnChange"] = $"{rutaTs}.{enumGestorDeEventos.EventosDeListaDinamica}('{eventosDeListaDinamica.perderFoco}','{descriptorControl.IdHtml}')";

            valores["Placeholder"] = atributos.AyudaDeCriteriosDeBusqueda.IsNullOrEmpty()
                ? $"Seleccionar ({atributos.CriterioDeBusqueda}) ..."
                : atributos.AyudaDeCriteriosDeBusqueda;
            valores["GuardarEn"] = atributos.GuardarEn;
            valores["RestringidoPor"] = atributos.RestringidoPorControl.ToLower();
            valores["PropiedadRestrictora"] = atributos.PropiedadRestrictora.ToLower();
            valores["ContenidoEn"] = tabla.IdHtmlContenedor;
            valores["Controlador"] = atributos.Controlador;
            valores["BlanquearControlesDependientes"] = atributos.AlSeleccionarBlanquearControl.ToLower();
            valores["BlanquearAlSalir"] = atributos.BlanquearAlSalir ? "S" : "N";
            valores["LongitudMaxima"] = atributos.LongitudMaxima > 0 ?
                    $"{Environment.NewLine}maxlength=¨{atributos.LongitudMaxima}¨"
                    : "";
            valores["ConNavegador"] = !atributos.VistaDondeNavegar.IsNullOrEmpty() ? "S" : "N";
            valores["Navegador"] = DefinirNavegador(descriptorControl.IdHtml, atributos.Controlador, atributos.VistaDondeNavegar, atributos.Negocio, atributos.OnClick);
            valores[nameof(ltrParametrosEp.negocio)] = atributos.Negocio.ToNombre();

            valores["ordenarPor"] = atributos.OrdenarListaDinamicaPor;
            valores[nameof(IUPropiedadAttribute.RestrictorFijo)] = atributos.RestrictorFijo;
            valores[nameof(IUPropiedadAttribute.SoloEnAlta)] = atributos.SoloEnAlta;

            valores[nameof(IUPropiedadAttribute.ParametrosParaNavegar)] = atributos.ParametrosParaNavegar;
            var a = PlantillasHtml.Render(PlantillasHtml.listaDinamicaDto, valores);

            a = a.Replace($"negocio='[{enumNegocio.No_Definido}]'", "")
                 .Replace($"parametros-para-navegar='[{nameof(IUPropiedadAttribute.ParametrosParaNavegar)}]'", "");


            a = a.Replace("ordenar-por=¨[ordenarPor]¨", "");
            a = a.Replace("style =¨[Estilos]¨", "");
            a = a.Replace($"[{nameof(IUPropiedadAttribute.EsAlmacenable)}]", atributos.EsAlmacenable ? $"es-almacenable='true'" : "");
            a = a.Replace($"[{nameof(IUPropiedadAttribute.trasSeleccionar)}]", !atributos.trasSeleccionar.IsNullOrEmpty() ? $"tras-seleccionar=¨{atributos.trasSeleccionar}¨" : "");
            a = a.Replace($"[{nameof(IUPropiedadAttribute.trasBlanquear)}]", !atributos.trasBlanquear.IsNullOrEmpty() ? $"tras-blanquear=¨{atributos.trasBlanquear}¨" : "");
            a = a.Replace($"[{nameof(IUPropiedadAttribute.antesDeBuscar)}]", !atributos.antesDeBuscar.IsNullOrEmpty() ? $"antes-de-buscar=¨{atributos.antesDeBuscar}¨" : "");
            a = a.Replace($"[{nameof(enumParamTs.idLista)}]", $"'{valores["IdHtml"]}'");
            return a;
        }


        private static string RenderRestrictor(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, double ancho)
        {
            var atributos = descriptorControl.Atributos;
            if (atributos.MostrarExpresion.IsNullOrEmpty())
                throw new Exception($"no se ha definido el atributo {nameof(atributos.MostrarExpresion)}  para el restrictor de la propiedad {descriptorControl.propiedad}");
            //if (atributos.PropiedadRestrictora.IsNullOrEmpty() && !descriptorControl.propiedad.StartsWith("id") && descriptorControl.Descriptor.PropertyType == typeof(string))
            //    throw new Exception($"no se ha definido el atributo {nameof(atributos.PropiedadRestrictora)}  para el restrictor de la propiedad {descriptorControl.propiedad}");

            Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorEditor);
            valores["Css"] = Css.Render(enumCssControles.EditorRestrictor);
            valores["MostrarPropiedad"] = atributos.MostrarExpresion.ToLower();
            valores[$"{nameof(IUPropiedadAttribute.PropiedadRestrictora)}"] = atributos.PropiedadRestrictora.ToLower();
            valores["Placeholder"] = atributos.Ayuda.Replace("[Negocio]", tabla.Negocio.ConArticulo());
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto;
            valores["Readonly"] = "editable=¨N¨ readonly";

            valores["ConNavegador"] = !atributos.VistaDondeNavegar.IsNullOrEmpty() ? "S" : "N";
            valores["Navegador"] = DefinirNavegador(descriptorControl.IdHtml, atributos.Controlador, atributos.VistaDondeNavegar,
                                                    atributos.Negocio == enumNegocio.No_Definido ? tabla.Negocio : atributos.Negocio,
                                                    atributos.OnClick);
            valores[nameof(ltrParametrosEp.negocio)] = atributos.Negocio == enumNegocio.No_Definido ? tabla.Negocio.ToNombre() : atributos.Negocio.ToNombre();


            if (tabla.ModoDeTrabajo == enumModoDeTrabajo.NuevaRelacion)
                valores["CriterioDeFiltrado"] = atributos.CriterioDeBusqueda == enumCriteriosDeFiltrado.contiene
                    ? enumCriteriosDeFiltrado.igual
                    : atributos.CriterioDeBusqueda;
            else
                valores["CriterioDeFiltrado"] = enumCriteriosDeFiltrado.igual;

            if (atributos.Alineada == enumAliniacion.derecha || atributos.TipoDeControl.Render() == "number")
                valores["Estilos"] = "text-align: right;";


            valores[nameof(IUPropiedadAttribute.ParametrosParaNavegar)] = atributos.ParametrosParaNavegar;

            var htmlEditor = PlantillasHtml.Render(PlantillasHtml.restrictorDto, valores);
            htmlEditor = htmlEditor.Replace($"negocio='[{enumNegocio.No_Definido}]'", "")
                .Replace($"parametros-para-navegar='[{nameof(IUPropiedadAttribute.ParametrosParaNavegar)}]'", "");

            return htmlEditor;
        }

        public static string DefinirNavegador(string idHtml, string controlador, string vistaDondeNavegar, enumNegocio negocio, string onClick)
        {
            onClick = onClick.IsNullOrEmpty() ? "" : $"al-pulsar = '{onClick}'";
            return !vistaDondeNavegar.IsNullOrEmpty() ?
               $@"<a id =¨{idHtml}-navegador¨ 
                      tipo =¨{enumTipoControl.Referencia}¨
                      class=¨boton-de-navegacion¨ 
                      {(onClick.IsNullOrEmpty() ? $"target=¨_blank¨" : "")}
                      controlador='{controlador.Replace(ltrEndPoint.Controller, "")}'
                      vista='{vistaDondeNavegar}'
                      negocio = '{negocio.ToNombre()}'
                      {onClick}
                      href=¨{(!onClick.IsNullOrEmpty() ? "#" : "")}¨/>></a>" : "";
        }

        internal static string DefinirBotonNavegador(string idHtml, bool urlExterna)
        {
            return $@"<a id =¨{idHtml}-navegador¨ 
                      tipo=¨{enumTipoControl.Navegador.Render()}¨
                      url-externa='{urlExterna}'
                      class=¨boton-de-navegacion¨ 
                      target=¨_blank¨ 
                      href=¨¨/>></a>";
        }


        private static string RenderControlOculto(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {
            var atributos = descriptorControl.Atributos;
            Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            valores[nameof(IUPropiedadAttribute.SoloParaTs)] = atributos.SoloParaTs;
            valores["Css"] = $"{enumCssControles.Editor.Render()} {enumCssControles.Oculto.Render()}";
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto;
            valores["LongitudMaxima"] = atributos.LongitudMaxima > 0 ?
                    $"{Environment.NewLine}maxlength=¨{atributos.LongitudMaxima}¨"
                    : "";
            valores["type"] = "text";
            if (descriptorControl.Atributos.Tipo == typeof(decimal) || descriptorControl.Atributos.Tipo == typeof(int))
            {
                valores["type"] = "number";
            }

            var htmlEditor = PlantillasHtml.Render(PlantillasHtml.controlOculto, valores);
            htmlEditor = htmlEditor.Replace("[onBlur]", !atributos.OnBlur.IsNullOrEmpty() ? $"onBlur='{atributos.OnBlur}'" : "");
            htmlEditor = htmlEditor.Replace($"[{nameof(IUPropiedadAttribute.OnChange)}]", !atributos.OnChange.IsNullOrEmpty() ? $"onChange='{atributos.OnChange}'" : "");
            htmlEditor = htmlEditor.Replace($"valor-de-defecto=¨¨", "");
            htmlEditor = htmlEditor.Replace($"[{nameof(IUPropiedadAttribute.Formato)}]", atributos.Formato != enumFormato.Sin_Formato ? $"formato='{atributos.Formato.Descripcion()}'" : "");

            return htmlEditor;
        }

        private static string RenderEditor(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {
            var atributos = descriptorControl.Atributos;
            Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);

            valores["CssContenedor"] = enumCssControles.ContenedorEditor.Render();
            if (atributos.CssDelContenedor != enumCssControles.Nulo)
                valores["CssContenedor"] = $"{valores["CssContenedor"]} {atributos.CssDelContenedor.Render()}";

            valores["Css"] = $"{enumCssControles.Editor.Render()}";
            valores["Placeholder"] = atributos.Ayuda.Replace("[Negocio]", tabla.Negocio.ConArticulo());
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto;
            valores["LongitudMaxima"] = atributos.LongitudMaxima > 0 ?
                    $"{Environment.NewLine}maxlength=¨{atributos.LongitudMaxima}¨"
                    : "";
            valores["type"] = "text";
            if (descriptorControl.Atributos.Tipo == typeof(decimal) || descriptorControl.Atributos.Tipo == typeof(int))
            {
                valores["type"] = descriptorControl.Atributos.Formato.Descripcion() == enumFormato.Moneda.Descripcion() ||
                                  descriptorControl.Atributos.Formato.Descripcion() == enumFormato.Porcentaje.Descripcion() ||
                                  descriptorControl.Atributos.Formato.Descripcion() == enumFormato.Numero_2.Descripcion() ||
                                  descriptorControl.Atributos.Formato.Descripcion() == enumFormato.Numero_6.Descripcion()
                ? "string"
                : "number";
            }

            if (descriptorControl.Atributos.TipoDeControl == enumTipoControl.Password)
            {
                valores["type"] = "password";
            }

            valores[nameof(IUPropiedadAttribute.PermisosNecesarios)] = atributos.PermisosNecesarios;
            if (atributos.ConLink) valores["Navegador"] = DefinirBotonNavegador(descriptorControl.IdHtml, true);


            if (atributos.Alineada == enumAliniacion.derecha || valores["type"].ToString() == "number")
                valores["Estilos"] = "text-align: right;";

            var htmlEditor = PlantillasHtml.Render(PlantillasHtml.editorDto, valores);
            htmlEditor = htmlEditor.Replace($"[{nameof(IUPropiedadAttribute.EsAlmacenable)}]", atributos.EsAlmacenable ? $"es-almacenable='true'" : "");
            htmlEditor = htmlEditor.Replace("[onKeyPress]", !atributos.OnKeyPress.IsNullOrEmpty() ? $"onKeyPress='{atributos.OnKeyPress}'" : "");
            htmlEditor = htmlEditor.Replace("[onBlur]", !atributos.OnBlur.IsNullOrEmpty() ? $"onBlur='{atributos.OnBlur}'" : "");
            htmlEditor = htmlEditor.Replace("[onfocus]", !atributos.OnFocus.IsNullOrEmpty() ? $"onfocus='{atributos.OnFocus}'" : "");
            htmlEditor = htmlEditor.Replace($"[{nameof(IUPropiedadAttribute.PermisosNecesarios)}]", "");
            htmlEditor = htmlEditor.Replace("[Navegador]", "");
            htmlEditor = htmlEditor.Replace($"[{nameof(IUPropiedadAttribute.OnPaste)}]", !atributos.OnPaste.IsNullOrEmpty() ? $"onPaste='{atributos.OnPaste}'" : "");
            htmlEditor = htmlEditor.Replace($"[{nameof(IUPropiedadAttribute.OnChange)}]", !atributos.OnChange.IsNullOrEmpty() ? $"onChange='{atributos.OnChange}'" : "");
            htmlEditor = htmlEditor.Replace($"[{nameof(IUPropiedadAttribute.Formato)}]", atributos.Formato != enumFormato.Sin_Formato ? $"formato='{atributos.Formato.Descripcion()}'" : "");
            htmlEditor = htmlEditor.Replace($"valor-de-defecto=¨¨", "");
            //htmlEditor = htmlEditor.Replace($"title='[Ayuda]'", $"title='{Ayuda}'");

            return htmlEditor;
        }

        private static string RenderListaDeElemento(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, double ancho)
        {
            var atributos = descriptorControl.Atributos;
            if (atributos.Controlador.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Debe definir el controlador para la clase de elemento {atributos.SeleccionarDe.Name}");

            Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorListaDeElementos);
            valores["Css"] = Css.Render(enumCssControles.ListaDeElementos);
            valores["ClaseElemento"] = atributos.SeleccionarDe.Name;
            valores["MostrarExpresion"] = atributos.MostrarExpresion.IsNullOrEmpty() ?
                ApiDeAtributos.ValorDelAtributo(atributos.SeleccionarDe, nameof(IUDtoAttribute.MostrarExpresion)) :
                atributos.MostrarExpresion;
            valores["GuardarEn"] = atributos.GuardarEn;
            valores["Controlador"] = atributos.Controlador;
            valores[nameof(IUPropiedadAttribute.RestrictorFijo)] = atributos.RestrictorFijo;
            valores[nameof(IUPropiedadAttribute.RestringidoPorControl)] = atributos.RestringidoPorControl.ToLower();
            valores[nameof(IUPropiedadAttribute.PropiedadRestrictora)] = atributos.PropiedadRestrictora.ToLower();
            valores[nameof(IUPropiedadAttribute.CargarBajoDemanda)] = atributos.CargarBajoDemanda;

            var htmlSelect = PlantillasHtml.Render(PlantillasHtml.listaDeElementosDto, valores)
                .Replace("style='[Estilos]'", "")
                .Replace($"restrictor-fijo='[{nameof(IUPropiedadAttribute.RestrictorFijo)}]'", "")
                .Replace($"restringido-por='[{nameof(IUPropiedadAttribute.RestringidoPorControl)}]'", "");

            if (tabla.ModoDeTrabajo == enumModoDeTrabajo.Nuevo || tabla.ModoDeTrabajo == enumModoDeTrabajo.NuevaRelacion)
                htmlSelect = htmlSelect.Replace($"[deshabilitada]",
                    !atributos.EditableAlCrear ? $"editable='false' disabled readonly" : "editable='true'");
            else
            if (tabla.ModoDeTrabajo == enumModoDeTrabajo.Edicion)
                htmlSelect = htmlSelect.Replace($"[deshabilitada]",
                    !atributos.EditableAlEditar ? $"editable='false' disabled readonly" : "editable='true'");
            else
            if (tabla.ModoDeTrabajo == enumModoDeTrabajo.Jerarquia)
                htmlSelect = htmlSelect.Replace($"[deshabilitada]",
                    !atributos.EditableAlCrear ? $"editable='false' disabled readonly" : "editable='true'");

            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.EsAlmacenable)}]", atributos.EsAlmacenable ? $"es-almacenable='true'" : "");
            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.OnBlur)}]", !atributos.OnBlur.IsNullOrEmpty() ? $"onBlur='{atributos.OnBlur}'" : "");
            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.OnChange)}]", !atributos.OnChange.IsNullOrEmpty() ? $"onChange='{atributos.OnChange}'" : "");
            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.TrasCargar)}]", !atributos.TrasCargar.IsNullOrEmpty() ? $"tras-cargar='{atributos.TrasCargar}'" : "");
            htmlSelect = htmlSelect.Replace($"[{nameof(IUPropiedadAttribute.AutoPosicionamiento)}]", atributos.AutoPosicionamiento ? $"auto-posicionamiento='true'" : "");

            htmlSelect = htmlSelect.Replace("([" + nameof(enumParamTs.idLista) + "])", $"(¨{descriptorControl.IdHtml}¨)");

            return htmlSelect;

        }

        private static string RenderSelectorDeFecha(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {
            var atributos = descriptorControl.Atributos;
            Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            valores["CssContenedor"] = Css.Render(enumCssControles.ContenedorFecha);
            if (descriptorControl.Atributos.Alineada == enumAliniacion.derecha)
                valores["CssContenedor"] = valores["CssContenedor"] + " " + enumCssControles.ContenedorFechaDerecha.Render();
            valores["Css"] = Css.Render(enumCssControles.SelectorDeFecha);
            valores["Placeholder"] = atributos.Ayuda;
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto;
            valores["Estilos"] = atributos.AnchoMaximo.IsNullOrEmpty() ? "cursor:pointer; width:100%;" : $"cursor:pointer; max-width: {atributos.AnchoMaximo};";

            if (!atributos.SelectorHasta.IsNullOrEmpty() && !atributos.OnBlur.IsNullOrEmpty())
                GestorDeErrores.Emitir($"El selector de fecha '{descriptorControl.propiedad}' o define un evento '{atributos.SelectorHasta}' o '{atributos.OnBlur}', no ambos");

            if (!atributos.SelectorHasta.IsNullOrEmpty())
            {
                if (atributos.SelectorHasta.Split(":").Length != 2)
                    GestorDeErrores.Emitir($"El selector de fecha {descriptorControl.propiedad} está mal definido, el atributo {nameof(atributos.SelectorHasta)} debe ser control:x");
                var fechaHasta = atributos.SelectorHasta.Split(":")[0];
                var incEnDias = atributos.SelectorHasta.Split(":")[1];

                valores[ValoresPorDefecto.ActualizarFechaHasta] = "javascript:" + nameof(enumNameSpaceTs.MapearAlControl) + "." + nameof(enumFunctionTs.ProponerFechaEn) +
                                                  "(this,'" + fechaHasta + "', " + incEnDias + ")";
            }

            if (!atributos.OnBlur.IsNullOrEmpty())
            {
                valores[ValoresPorDefecto.ActualizarFechaHasta] = atributos.OnBlur;
            }

            var htmSelectorDeFecha = PlantillasHtml.Render(PlantillasHtml.selectorDeFechaDto, valores);

            htmSelectorDeFecha = htmSelectorDeFecha.Replace($"{nameof(IUPropiedadAttribute.OnBlur)}=¨[{ValoresPorDefecto.ActualizarFechaHasta}]¨", "");

            return htmSelectorDeFecha;
        }

        private static string RenderSelectorDeFechaHora(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {

            var atributos = descriptorControl.Atributos;

            var a = new AtributosHtml(
                idHtmlContenedor: descriptorControl.IdHtmlContenedor,
                idHtml: descriptorControl.IdHtml,
                propiedad: descriptorControl.propiedad,
                tipoDeControl: atributos.TipoDeControl,
                visible: atributos.EsVisible(tabla.ModoDeTrabajo) && atributos.Obligatorio,
                editable: atributos.EsEditable(tabla.ModoDeTrabajo),
                obligatorio: atributos.Oculto || descriptorControl.Descriptor.PropertyType.IsNullableType() ? false : atributos.Obligatorio,
        ayuda: atributos.Ayuda,
                valorPorDefecto: atributos.ValorPorDefecto);

            a.AnchoMaximo = atributos.AnchoMaximo;
            a.NumeroDeFilas = atributos.NumeroDeFilas;
            a.SelectorHasta = atributos.SelectorHasta;

            a.OnClick = atributos.OnClick;
            a.CssBotonAccion = atributos.CssBotonAccion;

            return EditorDeFecha.RenderSelectorDeFechaHora(a);

            //Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            //valores["CssContenedor"] = Css.Render(enumCssControlesDto.ContenedorFechaHora);
            //valores["Css"] = Css.Render(enumCssControlesDto.SelectorDeFecha);
            //valores["CssHora"] = Css.Render(enumCssControlesDto.SelectorDeHora);
            //valores["Placeholder"] = atributos.Ayuda;
            //valores["ValorPorDefecto"] = atributos.ValorPorDefecto;

            //var htmSelectorDeFechaHora = PlantillasHtml.Render(PlantillasHtml.selectorDeFechaHoraDto, valores);

            //return htmSelectorDeFechaHora;
        }


        private static string RenderAreaDeTexto(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {
            var atributos = descriptorControl.Atributos;
            Dictionary<string, object> valores = ValoresDeAtributosComunes(tabla, descriptorControl, atributos);
            if (atributos.CssDelContenedor != enumCssControles.Nulo) 
            valores["CssContenedor"] = atributos.CssDelContenedor == enumCssControles.Nulo 
                    ? enumCssControles.ContenedorAreaDeTexto.Render()
                    : enumCssControles.ContenedorAreaDeTexto.Render() + " " + atributos.CssDelContenedor.Render();
            valores["Css"] = enumCssControles.AreaDeTexto.Render();
            if (atributos.CssDelArea != enumCssControles.Nulo)
                valores["Css"] = valores["Css"] + " " + Css.Render(enumCssControles.MonoSpaceText);
            valores["Placeholder"] = atributos.Ayuda.Replace("[Negocio]", tabla.Negocio.ConArticulo());
            valores["ValorPorDefecto"] = atributos.ValorPorDefecto?.ToString().Replace("\"", "&quot;").Replace(Environment.NewLine, "&#10;"); ;

            string alto = $"calc({(double)(1.5 * atributos.NumeroDeFilas)}em + .75rem + 2px);".Replace(",", ".");
            valores["Estilos"] = $"{valores["Estilos"]}{$" height: {alto}"}";

            var htmlArea = PlantillasHtml.Render(PlantillasHtml.AreaDeTextoDto, valores);
           if (!atributos.OnClick.IsNullOrEmpty())
            {
                htmlArea = htmlArea + $@"<div class=¨contenedor-etiqueta-ref-test-area-dto¨>
                                         <a href=¨#¨ id=¨{descriptorControl.IdHtml}-etiqueta-ref¨ class=¨etiqueta-ref-test-area-dto¨ onclick=¨{atributos.OnClick}¨>{atributos.EtiquetaRef}</a>
                                         </div>";
            }

            return htmlArea;
        }

        private static string RenderSelectorDeUnArcivo(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, double ancho)
        {
            var atributos = descriptorControl.Atributos;
            var htmlContenedor = RenderContenedorDto(descriptorControl, ancho, Css.Render(enumCssControles.ContenedorArchivo), onPaste: false);
            var idHtmlContenedorBarra = $"{descriptorControl.IdHtml}.contenedor.barra";
            var idHtmlBarra = $"{descriptorControl.IdHtml}.barra";
            var idHtmlInfoArchivo = $"nombre-{descriptorControl.IdHtml}";

            var htmlArchivo = $@"<form method=¨post¨ action=¨SubirArchivo¨ enctype=¨multipart/form-data¨>
                                   <input  {ControlHtml.RenderAtributos(descriptorControl.propiedad, descriptorControl.IdHtml, enumTipoControl.SelectorDeUnArchivo, enumCssControlesFormulario.Archivo.Render(), atributos.Ayuda)}
                                       type=¨{enumInputType.file.Render()}¨ 
                                       name=¨fichero¨  
                                       style=¨display: none;¨
                                       accept=¨{atributos.ExtensionesValidas}¨
                                       controlador=¨{tabla.Controlador}¨
                                       info-archivo=¨{idHtmlInfoArchivo}¨
                                       limite-en-byte = {atributos.LimiteEnByte}
                                       barra-vinculada = ¨{idHtmlBarra}¨ 
                                       {(tabla.ModoDeTrabajo == enumModoDeTrabajo.Nuevo ? $"visible-en-visor-al-crear = {atributos.VisibleEnVisorAlCrear}": "")}
                                       onChange=¨ApiDeArchivos.SubirArchivoSeleccionado('{tabla.IdHtmlContenedor}','{descriptorControl.IdHtml}','{idHtmlInfoArchivo}')¨
                                       [{nameof(IUPropiedadAttribute.trasSeleccionar)}]/>
                                   <input {ControlHtml.RenderAtributos(propiedad: "", idHtmlInfoArchivo, enumTipoControl.Editor, enumCssControlesFormulario.InfoArchivo.Render(), ayuda: "", $"type = ¨{enumInputType.text.Render()}¨")} readonly>
                                   </input>
                                   <div id = ¨{idHtmlContenedorBarra}¨ class=¨{Css.Render(enumCssControlesFormulario.InfoArchivo)}¨ style=¨display: none;¨>
                                      <div id = ¨{idHtmlBarra}¨ class=¨{Css.Render(enumCssControles.BarraAzulArchivo)}¨ contenedor-barra = ¨{idHtmlContenedorBarra}¨ style=¨display: none;¨>
                                         <span></span>
                                      </div>
                                   </div>
                                 </form>
                                ";

            htmlArchivo = htmlArchivo.Replace($"[{nameof(IUPropiedadAttribute.trasSeleccionar)}]", !atributos.trasSeleccionar.IsNullOrEmpty()
                ? $"tras-seleccionar='{atributos.trasSeleccionar}'".Replace("[idContenedor]", tabla.IdHtmlContenedor)
                : "");



            return htmlContenedor.Replace("controlParaRenderizar", htmlArchivo);
        }

        private static string RenderSelectorImagen(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, double ancho)
        {
            var atributos = descriptorControl.Atributos;
            var htmlContenedor = RenderContenedorDto(descriptorControl, ancho, Css.Render(enumCssControles.ContenedorArchivo), onPaste: true);

            var idHtmlBarra = $"barra-{descriptorControl.IdHtml}";
            var idHtmlImg = $"img-{descriptorControl.IdHtml}";
            var idHtmlCanva = $"canvas-{descriptorControl.IdHtml}";
            var idHtmlInfoArchivo = $"nombre-{descriptorControl.IdHtml}";

            htmlContenedor = DefinirFuncionPegarArchivo(htmlContenedor,tabla,descriptorControl);

            var htmlArchivo = @$"
            <form class=¨{Css.Render(enumCssControles.FormDeArchivo)}¨ method=¨post¨ enctype=¨multipart/form-data¨>
              <div class=¨{enumCssDiv.Tabla.Render()} {Css.Render(enumCssControles.TablaDeArchivo)}¨>
                 <div class=¨{enumCssDiv.Tr.Render()} {Css.Render(enumCssControles.FilaDeArchivo)}¨>
                   <div class=¨{enumCssDiv.Td.Render()} {Css.Render(enumCssControles.ColumnaDeArchivo)}¨>   
                      <input  {RenderAtributosComunes(tabla, descriptorControl, Css.Render(enumCssControles.SelectorDeImagen))}
                              type=¨file¨ 
                              name=¨fichero¨  
                              style=¨display: none;¨
                              accept=¨{atributos.ExtensionesValidas}¨
                              ruta-destino=¨{atributos.RutaDestino}¨
                              canvas-vinculado = ¨{idHtmlCanva}¨  
                              imagen-vinculada = ¨{idHtmlImg}¨   
                              barra-vinculada = ¨{idHtmlBarra}¨  
                              info-archivo=¨{idHtmlInfoArchivo}¨
                              placeholder =¨{atributos.Ayuda}¨
                              onChange=¨ApiDeArchivos.MostrarCanvas('{tabla.Controlador}','{descriptorControl.IdHtml}','{idHtmlCanva}')¨ />
                  </div>
                   <div class=¨{enumCssDiv.Td.Render()} {Css.Render(enumCssControles.ColumnaDeArchivo)}¨>

                   </div>
                 </div>
                 <div class=¨{enumCssDiv.Tr.Render()} {Css.Render(enumCssControles.FilaDeArchivo)}¨>
                   <div class=¨{enumCssDiv.Td.Render()} {Css.Render(enumCssControles.ColumnaDeArchivo)}¨>
                      <canvas id=¨{idHtmlCanva}¨ style='border: ridge;'></canvas>
                   </div>
                   <div class=¨{enumCssDiv.Td.Render()} {Css.Render(enumCssControles.ColumnaDeArchivo)}¨>
                      <div id = ¨{idHtmlBarra}¨ class=¨{Css.Render(enumCssControles.BarraAzulArchivo)}¨>
                          <span></span>
                      </div>
                       <div style=¨display: none;¨>
                           <img id=¨{idHtmlImg}¨
                                    tipo=¨{enumTipoControl.VisorDeArchivo.Render()}¨  
                                    propiedad=¨{(atributos.TipoDeControl == enumTipoControl.UrlDeArchivo ? descriptorControl.propiedad : atributos.UrlDelArchivo.ToLower())}¨ 
                                    src=¨¨>
                           <input id=¨{idHtmlInfoArchivo}¨> </input>
                       </div>
                   </div>
                 </div>
              </div>

             </form>
            ";
            return htmlContenedor.Replace("controlParaRenderizar", htmlArchivo);
        }

        private static string DefinirFuncionPegarArchivo(string html, DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl)
        {
            return ControlHtml.DefinirFuncionPegarArchivo(html, tabla.Controlador, descriptorControl.IdHtml);

            //var idHtmlImg = $"img-{descriptorControl.IdHtml}";
            //var idHtmlCanva = $"canvas-{descriptorControl.IdHtml}";
            //var idHtmlInfoArchivo = $"nombre-{descriptorControl.IdHtml}";

            //html = html.Replace("controlador", tabla.Controlador).
            //    Replace("idCanvas", idHtmlCanva).
            //    Replace("idImagen", idHtmlImg).
            //    Replace("idInputNombreArchivo", idHtmlInfoArchivo).
            //    Replace("idInputIdArchivo", descriptorControl.IdHtml);

            //return html;
        }

        private static string RenderContenedorDto(DescriptorDeControlDeLaTabla descriptorControl, double ancho, string cssClaseContenedor, bool onPaste)
        {
            //17.01.2021 --> Al usar css no me hace flata, y mostrar en bloques style=¨width: {ancho}%
            return $@"<div id='{descriptorControl.IdHtmlContenedor}' name='contenedor-control' class='{cssClaseContenedor}' {(onPaste ? "OnPaste=¨ApiDeArchivos.PegarPortaPapeles(event, 'controlador', 'idCanvas', 'idImagen', 'idInputNombreArchivo', 'idInputIdArchivo')¨ title=¨CTRL+V para pegar el portapapeles¨" : "")}>
                        controlParaRenderizar
                      </div>";
        }

        private static string RenderAtributosComunes(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, string claseCss)
        {
            var atributos = descriptorControl.Atributos;
            var atributosHtml = $@"id=¨{descriptorControl.IdHtml}¨ 
                                   propiedad=¨{descriptorControl.propiedad}¨ 
                                   class=¨{claseCss}¨ 
                                   tipo=¨{atributos.TipoDeControl.Render()}¨ 
                                   obligatorio=¨{(atributos.EsVisible(tabla.ModoDeTrabajo) && !atributos.Oculto && atributos.Obligatorio ? "S" : "N")}¨ 
                                   {(!atributos.EsEditable(tabla.ModoDeTrabajo) ? "readonly" : "")} ";
            return atributosHtml;
        }

        private static Dictionary<string, object> ValoresDeAtributosComunes(DescriptorDeTabla tabla, DescriptorDeControlDeLaTabla descriptorControl, IUPropiedadAttribute atributos)
        {
            var atributosHtml = AtributosHtml.AtributosComunes(descriptorControl.IdHtmlContenedor
                , descriptorControl.IdHtml
                , descriptorControl.propiedad
                , atributos.TipoDeControl
                , atributos.Ayuda.Replace("[Negocio]", tabla.Negocio.ConArticulo()));

            atributosHtml.Obligatorio = atributos.Oculto || descriptorControl.Descriptor.PropertyType.IsNullableType() ? false : atributos.Obligatorio;
            atributosHtml.Editable = atributos.EsEditable(tabla.ModoDeTrabajo);
            atributosHtml.AnchoMaximo = atributos.AnchoMaximo;
            atributosHtml.Ampliacion = tabla.Tipo.ImplementaAmpliacionDto() ? tabla.Tipo.Name : "N";
            atributosHtml.PanelPadre = tabla.IdHtmlContenedor;
            return atributosHtml.MapearComunes();
        }

    }
}
