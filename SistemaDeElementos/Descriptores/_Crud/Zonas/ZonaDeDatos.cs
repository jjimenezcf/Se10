using System.Collections.Generic;
using Utilidades;
using Gestor.Errores;
using ModeloDeDto;
using UtilidadesParaIu;
using System.Reflection;
using GestorDeElementos.Extensores;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Negocio;
using System.Linq;
using ServicioDeDatos.Entorno;
using GestorDeElementos;

namespace MVCSistemaDeElementos.Descriptores
{

    public class ZonaDeDatos<TElemento> : ControlHtml where TElemento : ElementoDto
    {
        public DescriptorDeMantenimiento<TElemento> Mnt => (DescriptorDeMantenimiento<TElemento>)Padre;
        public dynamic Historial => Padre;

        public bool EsHistorial => typeof(TElemento) == typeof(HistorialDto);
        //{
        //    get
        //    {
        //        var tipoDescriptor = typeof(DescriptorDeHistorial<,>);
        //        var tipoPadre = Padre.GetType();
        //        var tipoGenerico = tipoDescriptor.MakeGenericType(typeof(TElemento), typeof(HistorialDto));
        //        bool esTipoCorrecto = tipoGenerico.IsAssignableFrom(tipoPadre);
        //        return esTipoCorrecto;
        //    }
        //}

        public Grid Grid { get; private set; }

        public List<ColumnaDelGrid> Columnas => Grid.columnas;

        private List<FilaDelGrid> Filas => Grid.filas;

        private int _cantidadPorLeer = 10;

        public int CantidadPorLeer { get { return _cantidadPorLeer == 0 ? 10 : _cantidadPorLeer; } set { _cantidadPorLeer = value; } }
        public int PosicionInicial { get; set; } = 0;

        public string IdHtmlModal { get; set; }

        public int TotalEnBd { get; set; }
        public ZonaDeDatos(ControlHtml padre)
        : base(
          padre: padre,
          id: $"{padre.Id}_Grid",
          etiqueta: null,
          propiedad: null,
          ayuda: null,
          posicion: null
        )
        {
            Tipo = enumTipoControl.ZonaDeDatos;
            Grid = new Grid(this);

            if (!EsHistorial && !Mnt.Crud.EsModal)
            {
                if (Mnt.Crud.Negocio != enumNegocio.No_Definido)
                {
                    var cantidadJobject = (JObject)Mnt.Crud.Negocio.LeerParametroDeUsuario<JObject>(Mnt.Crud.Contexto, enumParametrosDeUsuario.USU_Cantidad_A_Leer);
                    if (cantidadJobject.HasValues)
                        CantidadPorLeer = cantidadJobject[ltrParametrosDeUsuarios.cantidadPorLeer].ToObject<int>();
                }
                else
                {
                    var cantidadBd = Mnt.Crud.Contexto.SeleccionarPorAk<ParametroVistaPorUsuarioDtm>(new Dictionary<string, object> {
                                                { nameof(ParametroVistaPorUsuarioDtm.IdVista), Mnt.Crud.IdVista },
                                                { nameof(ParametroVistaPorUsuarioDtm.IdUsuario), Mnt.Crud.Contexto.DatosDeConexion.IdUsuario },
                                                { nameof(ParametroVistaPorUsuarioDtm.Nombre), enumParametrosDeUsuario.USU_Cantidad_A_Leer },
                                    }, errorSiNoHay: false);
                    if (cantidadBd is not null)
                    {
                        var cantidadJobject = JObject.Parse(cantidadBd.Valor.ToString());
                        CantidadPorLeer = cantidadJobject[ltrParametrosDeUsuarios.cantidadPorLeer].ToObject<int>();
                    }
                }
            }
        }

        public void AnadirFila(FilaDelGrid fila)
        {
            fila.Datos = this;
            fila.NumeroDeFila = Filas.Count;
            Filas.Add(fila);
        }

        internal void AnadirColumna(ColumnaDelGrid columnaDelGrid)
        {
            if (EsHistorial)
                Historial.Datos.Columnas.Add(columnaDelGrid);
            else
                Mnt.Datos.Columnas.Add(columnaDelGrid);
            columnaDelGrid.ZonaDeDatos = this;
        }

        internal void InsertarColumna(ColumnaDelGrid columnaDelGrid, int posicion)
        {
            var columnas = EsHistorial ? Historial.Datos.Columnas.Count : Mnt.Datos.Columnas.Count;
            if (posicion >= columnas || posicion == -1)
            {
                AnadirColumna(columnaDelGrid);
                return;
            }

            if (EsHistorial)
                Historial.Datos.Columnas.Insert(columnaDelGrid);
            else
                Mnt.Datos.Columnas.Insert(posicion, columnaDelGrid);

            columnaDelGrid.ZonaDeDatos = this;
        }


        internal ColumnaDelGrid ObtenerColumna(string nombreColumna)
        {
            for (var i = 0; i < Columnas.Count; i++)
                if (Columnas[i].Propiedad == nombreColumna)
                    return Columnas[i];

            return null;
        }


        public void CalcularAnchosColumnas()
        {
            var totalPorcentaje = 0;
            var colDefinidas = 0;
            var colSinDefinir = 0;
            foreach (var col in Columnas)
            {
                if (!col.Visible) continue;
                if (col.Propiedad == ltrColumnasDelGrid.chksel)
                    continue;

                if (col.PorAnchoMnt == 0) colSinDefinir++;
                else
                {
                    totalPorcentaje += col.PorAnchoMnt;
                    colDefinidas++;
                }
            }

            if (totalPorcentaje > 100)
            {
                //GestorDeErrores.Emitir($"Las columnas definidas para el tipo {typeof(TElemento)} sobrepasan el 100%");
                colSinDefinir = 0;
                decimal reajuste = (10000 / totalPorcentaje);
                totalPorcentaje = 0;
                foreach (var col in Columnas)
                {
                    if (!col.Visible) continue;
                    if (col.Propiedad == ltrColumnasDelGrid.chksel)
                        continue;

                    if (col.PorAnchoMnt == 0) colSinDefinir++;
                    else
                    {
                        col.PorAnchoMnt = (int)(col.PorAnchoMnt * reajuste / 100);
                        totalPorcentaje += col.PorAnchoMnt;
                    }
                }
            }

            var porcDeReparto = 100 - totalPorcentaje;

            if (colSinDefinir == 0)
                return;

            var porcPorColNoDefinida = porcDeReparto / colSinDefinir;
            foreach (var col in Columnas)
            {
                if (col.PorAnchoMnt > 0 || !col.Visible) continue;

                col.PorAnchoMnt = porcPorColNoDefinida;
                porcDeReparto = porcDeReparto - porcPorColNoDefinida;

                if (porcPorColNoDefinida > porcDeReparto)
                    porcPorColNoDefinida = porcDeReparto;
            }
        }
        private string RenderZonaDeDatos()
        {
            var mostrarExpresion = $"{(string)ApiDeAtributos.ValorDelAtributo(typeof(TElemento), nameof(IUDtoAttribute.MostrarExpresion))}";

            var expresionElemento = typeof(TElemento).GetField("ExpresionElemento");
            if (expresionElemento != null)
                mostrarExpresion = $"[{expresionElemento.GetValue(typeof(TElemento))}]";
            else
            if (!ApiDeEnsamblados.HeredaDe(typeof(TElemento), typeof(ElementoDto)))
                GestorDeErrores.Emitir($"Debe definir los campos que componen la 'exprexión del elemento' para el objeto {typeof(TElemento).Name}");


            var idHtmlZonaFiltro = string.Empty;
            if (EsHistorial)
            {
                idHtmlZonaFiltro = !Historial.ContenidoEnEdicion ? Historial.Filtro.IdHtml : string.Empty;
            }
            else
            {
                idHtmlZonaFiltro = Mnt.Filtro.IdHtml;
            }

            var tablacograficos = !EsHistorial && Mnt.Crud.HayTablaConGraficos;

            var htmlDiv = @$" <!-- ********************  grid de datos ******************** -->
                              <div id = ¨{IdHtml}¨ 
                                  class=¨{Css.Render(enumCssCuerpo.CuerpoDatosGrid)}¨ 
                                  seleccionables = ¨-1¨ 
                                  zona-de-filtro = ¨{idHtmlZonaFiltro}¨ 
                                  expresion-elemento = ¨{mostrarExpresion.ToLower()}¨ 
                                  tabla-de-datos = ¨{Grid.IdHtmlTabla}¨ 
                                  zona-de-navegador = ¨{(EsHistorial ? Historial.IdHtmlZonaNavegador : Mnt.IdHtmlZonaNavegador)}¨
                                  cabecera-de-tabla = ¨{Grid.IdHtmlCabeceraDeTabla}¨
                                  {(IdHtmlModal.IsNullOrEmpty() ? "" : $"id-modal=¨{IdHtmlModal}¨")}>     
                                  {Grid.ToHtml(tablacograficos)}
                             </div>";
            return htmlDiv;
        }

        public override string RenderControl()
        {
            return RenderZonaDeDatos();
        }

        internal static bool AnadirPropiedades(ZonaDeDatos<TElemento> zonaDeDatos, PropertyInfo[] propiedades, Dictionary<int, ColumnaDelGrid> columnasPendientes, bool hayPosicionCero, List<PropiedaJson> atributosJson)
        {
            JObject columnasJobject = null;
            JObject disposicionJobject = null;
            JObject tamanosJobject = null;
            List<VisibilidadDeColumna> visibilidadDeColumnas = new List<VisibilidadDeColumna> { };
            List<DisposicionDeColumna> disposicionDeColumnas = new List<DisposicionDeColumna> { };
            List<TamanoDeColumna> tamanosDeColumnas = new List<TamanoDeColumna> { };

            if (!zonaDeDatos.EsHistorial)
            {
                if (zonaDeDatos.Mnt.Crud.Negocio != enumNegocio.No_Definido)
                {
                    columnasJobject = (JObject)zonaDeDatos.Mnt.Crud.Negocio.LeerParametroDeUsuario<JObject>(zonaDeDatos.Mnt.Crud.Contexto, enumParametrosDeUsuario.USU_Colunas_Del_Grid);
                    disposicionJobject = (JObject)zonaDeDatos.Mnt.Crud.Negocio.LeerParametroDeUsuario<JObject>(zonaDeDatos.Mnt.Crud.Contexto, enumParametrosDeUsuario.USU_Disposicion_Del_Encolumnado);
                    tamanosJobject = (JObject)zonaDeDatos.Mnt.Crud.Negocio.LeerParametroDeUsuario<JObject>(zonaDeDatos.Mnt.Crud.Contexto, enumParametrosDeUsuario.USU_Tamano_Del_Encolumnado);
                    if (columnasJobject.HasValues)
                        visibilidadDeColumnas = columnasJobject[ltrParametrosDeUsuarios.columnasJson].ToObject<List<VisibilidadDeColumna>>();
                    if (disposicionJobject.HasValues)
                        disposicionDeColumnas = disposicionJobject[ltrParametrosDeUsuarios.encolumnado].ToObject<List<DisposicionDeColumna>>();
                    if (tamanosJobject.HasValues)
                        tamanosDeColumnas = tamanosJobject[ltrParametrosDeUsuarios.tamanos].ToObject<List<TamanoDeColumna>>();
                }
                else
                {
                    var tamanosBd = zonaDeDatos.Mnt.Crud.Contexto.SeleccionarPorAk<ParametroVistaPorUsuarioDtm>(new Dictionary<string, object> {
                                                { nameof(ParametroVistaPorUsuarioDtm.IdVista), zonaDeDatos.Mnt.Crud.IdVista },
                                                { nameof(ParametroVistaPorUsuarioDtm.IdUsuario), zonaDeDatos.Mnt.Crud.Contexto.DatosDeConexion.IdUsuario },
                                                { nameof(ParametroVistaPorUsuarioDtm.Nombre), enumParametrosDeUsuario.USU_Tamano_Del_Encolumnado },
                                    }, errorSiNoHay: false);
                    if (tamanosBd is not null)
                    {
                        tamanosJobject = JObject.Parse(tamanosBd.Valor.ToString());
                        tamanosDeColumnas = tamanosJobject[ltrParametrosDeUsuarios.tamanos].ToObject<List<TamanoDeColumna>>();
                    }
                }
            }

            foreach (var propiedad in propiedades)
            {
                var columna = new ColumnaDelGrid { Propiedad = propiedad.Name, Tipo = propiedad.PropertyType };
                IUPropiedadAttribute atributos = ApiDeAtributos.ObtenerAtributos(propiedad, atributosJson);
                columna.Titulo = ApiDeAtributos.Titulo(propiedad, atributos);
                columna.Ayuda = atributos.Ayuda;
                columna.Visible = atributos.EsVisible(enumModoDeTrabajo.Mantenimiento) && !atributos.Oculto;
                columna.ConOrdenacion = !atributos.OrdenarGridPor.IsNullOrEmpty() ? true : atributos.Ordenar;
                columna.OrdenarPor = atributos.OrdenarGridPor;
                columna.Alineada = atributos.Alineada == enumAliniacion.no_definida ? columna.Tipo.Alineada() : atributos.Alineada;
                columna.CssDeLaColumna = atributos.CssDeLaColumna;
                var tamanoGuardado = tamanosDeColumnas.FirstOrDefault(t => t.Id.ToLower() == columna.Propiedad.ToLower());
                if (tamanoGuardado is not null)
                {
                    columna.TamanoFijo = tamanoGuardado.Tamano;
                    columna.PorAnchoMnt = 0;
                    columna.PorAnchoSel = 0;
                }
                else
                {
                    columna.TamanoFijo = atributos.TamanoFijo;
                    columna.PorAnchoMnt = atributos.PorAnchoMnt;
                    columna.PorAnchoSel = atributos.PorAnchoSel == 0 ? atributos.PorAnchoMnt : atributos.PorAnchoSel;
                }
                columna.Formato = atributos.Formato;
                columna.EsFecha = atributos.EsFecha;
                columna.TipoDeControl = atributos.TipoDeControlEnGrid.Render();

                if (columna.Tipo == typeof(string) && atributos.TipoDeControlEnGrid == enumTipoControl.Referencia && atributos.AccionRef.IsNullOrEmpty() && atributos.VisibleEnGrid)
                {
                    GestorDeErrores.Emitir($"Debe definir la acción para la columna '{columna.Propiedad}' del grid");
                }

                columna.EsAccion = !atributos.AccionRef.IsNullOrEmpty();
                columna.Accion = atributos.AccionRef;
                columna.PosicionEnElGrid = atributos.PosicionEnGrid;
                var disposicion = disposicionDeColumnas.FirstOrDefault(x => x.Propiedad.ToLower() == columna.Propiedad.ToLower());
                if (disposicion != null) atributos.PosicionEnGrid = disposicion.Posicion;
                if (atributos.PosicionEnGrid == -1)
                    zonaDeDatos.InsertarColumna(columna, atributos.PosicionEnGrid);
                else
                {
                    if (atributos.PosicionEnGrid == 0) hayPosicionCero = true;
                    while (columnasPendientes.ContainsKey(atributos.PosicionEnGrid))
                        atributos.PosicionEnGrid++;
                    columnasPendientes.Add(atributos.PosicionEnGrid, columna);
                }

                if (visibilidadDeColumnas.Count > 0)
                {
                    var visibilidadGuardada = visibilidadDeColumnas.FirstOrDefault(x => x.Propiedad.ToLower() == columna.Propiedad.ToLower());
                    if (visibilidadGuardada is not null)
                    {
                        columna.Visible = columna.Titulo.IsNullOrEmpty() ? false : visibilidadGuardada.Visible;
                        if (columna.PorAnchoMnt == 0 && columna.TamanoFijo.IsNullOrEmpty())
                            columna.TamanoFijo = "10em";
                        if (columna.CssDeLaColumna == enumCssGrid.ColumnaOculta)
                            columna.CssDeLaColumna = enumCssGrid.Nulo;
                    }
                }
            }

            return hayPosicionCero;
        }

        internal static ColumnaDelGrid ColumnaDeControlDeSeleccion()
        =>
        new ColumnaDelGrid { Propiedad = ltrColumnasDelGrid.chksel, Titulo = " ", TamanoFijo = "24px", Tipo = typeof(bool), TipoDeControl = enumTipoControl.Check.Render() };
    }

}
