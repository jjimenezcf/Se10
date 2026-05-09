using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Gastos;
using ServicioDeDatos.Elemento;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{

    public class ColumnaAccion
    {
        public string accion { get; set; }
        public string titulo { get; set; }
        public int tamano { get; set; }
        public bool visible { get; set; }

    }

    public class GridDeRelacion : ControlHtml
    {

        private DescriptorDeColumnas _Columnas { get; }
        public List<ColumnaAccion> acciones = new List<ColumnaAccion>();
        public string IdHtmlContenedor => $"{IdHtml}-contenedor";

        public Type Controlador { get; }

        public Type TipoDto
        {
            get
            {
                Type tipoElemento = Controlador.BaseType.GetGenericArguments().LastOrDefault();
                
                if (tipoElemento is not null)
                    return tipoElemento;
                return null;
            }
        }

        public string AccionDeConsulta { get; set; }
        public bool ConCapa { get; set; } = true;
        public string PropiedadRestrictora { get; private set; }
        public string RestrictorFijo { get; private set; }
        

        public string CampoRestrictor { get; set; }

        public string OrdenarPor { get; private set; }
        public bool AplicarJoin { get; private set; }

        public int IdVinculado { get; private set; }

        public string PaginaDondeNavegarAlEditar { get; private set; }
        public string TituloDeAbrir { get; private set; }

        public bool CargarPorEvento { get; private set; }
        public string TrasCargarGrid { get; private set; }
        public string FiltrarPara { get; set; } = ltrFiltros.CargarGridDeRelacion;


        private bool _permiteEditar;
        private bool _permiteBorrar;

        public enumNameSpaceTs EspaciodeNombres { get; set; } = enumNameSpaceTs.Crud;
        public string AccionDeBorrado { get; set; }
        public string AccionDeLeerPorId { get; set; }

        public bool PermitirBorrar
        {
            get
            {
                return _permiteBorrar;
            }
            set
            {
                _permiteBorrar = value;
                foreach (var accion in acciones)
                {
                    if (accion.titulo == "Borrar")
                    {
                        //if (value && Controlador.GetMethods().Where(x => x.Name == ltrControladores.epCrearRelacion).FirstOrDefault() == null)
                        //    throw new Exception($"El controlado {Controlador.Name} debe de implemtar el método de {ltrControladores.epBorrarRelacionPorId}");

                        accion.visible = value;
                        return;
                    }
                }
                acciones.Add(new ColumnaAccion
                {
                    accion = Referencia.BorrarRelacion((DescriptorDeExpansor)Padre, EspaciodeNombres, AccionDeBorrado),
                    titulo = "Borrar",
                    tamano = 100,
                    visible = value
                });
            }
        }
        public bool PermitirEditar
        {
            get
            {
                return _permiteEditar;
            }
            set
            {
                _permiteEditar = value;
                foreach (var accion in acciones.Where(accion => accion.titulo == "Editar"))
                {
                    accion.visible = value;
                    return;
                }

                acciones.Add(new ColumnaAccion
                {
                    accion = Referencia.AbrirEditarDto((DescriptorDeExpansor)Padre, PropiedadRestrictora, EspaciodeNombres),
                    titulo = "Editar",
                    tamano = 100,
                    visible = value
                });
            }
        }

        public bool PermitirNavegar
        {
            get
            {
                return PaginaDondeNavegarAlEditar.IsNullOrEmpty();
            }
        }

        public string idModalParaEditar => _permiteEditar ? $"{Id}-{enumTipoDeModal.ModalDeEditarRelacion}" : "";

        public int Cantidad { get; private set; }
        public int IdNegocio { get; }

        public bool OcultarSiVacio { get; }

        public GridDeRelacion(DescriptorDeExpansor Expansor, DescriptorDeColumnas columnas, Dictionary<string, object> parametros)
        : base(Expansor, $"{enumTipoControl.GridDeDetalle.Render()}-{columnas.Id}", "", "", "", null)
        {
            _Columnas = columnas;
            Tipo = enumTipoControl.GridDeDetalle;

            if (!parametros.ContainsKey(nameof(Controlador)))
                GestorDeErrores.Emitir($"Para definir un GridDeDetalle necesita indicar el {nameof(Controlador)}");

            if (!parametros.ContainsKey(nameof(AccionDeConsulta)))
                GestorDeErrores.Emitir($"Para definir un GridDeDetalle necesita indicar la {nameof(AccionDeConsulta)}");
            if (!parametros.ContainsKey(nameof(PropiedadRestrictora)))
                GestorDeErrores.Emitir($"Para definir un GridDeDetalle necesita indicar la {nameof(PropiedadRestrictora)}");

            CargarPorEvento = parametros.LeerValor<bool>(nameof(CargarPorEvento), false);
            OcultarSiVacio = parametros.LeerValor<bool>(nameof(OcultarSiVacio), false);
            TrasCargarGrid = parametros.LeerValor<string>(nameof(TrasCargarGrid), null);

            Controlador = (Type)parametros[nameof(Controlador)];
            AccionDeConsulta = parametros[nameof(AccionDeConsulta)].ToString();
            PropiedadRestrictora = parametros[nameof(PropiedadRestrictora)].ToString();
            if (parametros.ContainsKey(nameof(RestrictorFijo))) RestrictorFijo = parametros[nameof(RestrictorFijo)].ToString();
            CampoRestrictor = parametros.ContainsKey(nameof(CampoRestrictor)) ? parametros[nameof(CampoRestrictor)].ToString() : nameof(ElementoDto.Id);
            OrdenarPor = parametros.ContainsKey(nameof(OrdenarPor)) ? parametros[nameof(OrdenarPor)].ToString() : "";

            AplicarJoin = !parametros.ContainsKey(nameof(AplicarJoin)) ? true : (bool)parametros[nameof(AplicarJoin)];
            Cantidad = !parametros.ContainsKey(nameof(Cantidad)) ? -1 : (int)parametros[nameof(Cantidad)];

            IdVinculado = (int)parametros.LeerValor(ltrParametrosEp.idVinculado, 0);
            
            var idnegocio = (int)parametros.LeerValor(nameof(IdNegocio), 0);
            IdNegocio = idnegocio == 0 ? (Padre as DescriptorDeExpansor).IdNegocio : idnegocio;

            //if (IdVinculado > 0 && NegociosDeSe.ToEnumerado(IdVinculado).TipoDtm().ImplementaUsaDescripcion())
            //{
            //    _Columnas.Add(titulo: nameof(IUsaDescripcion.Descripcion), mostrar: false);
            //}
            //if (IdVinculado == 0 && IdNegocio > 0 && NegociosDeSe.ToEnumerado(IdNegocio).TipoDtm().ImplementaUsaDescripcion())
            //{
            //    _Columnas.Add(titulo: nameof(IUsaDescripcion.Descripcion), mostrar: false);
            //}
            //_Columnas.Add(titulo: nameof(IUsaDescripcion.Descripcion), mostrar: false);
            TituloDeAbrir = (string)parametros.LeerValor(nameof(TituloDeAbrir), "Abrir");

            if (parametros.ContainsKey(nameof(EspaciodeNombres))) EspaciodeNombres = (enumNameSpaceTs)parametros[nameof(EspaciodeNombres)];
            if (parametros.ContainsKey(nameof(AccionDeBorrado))) AccionDeBorrado = (string)parametros[nameof(AccionDeBorrado)];
            if (parametros.ContainsKey(nameof(AccionDeLeerPorId))) AccionDeLeerPorId = (string)parametros[nameof(AccionDeLeerPorId)];

            Expansor.GidDeRelacion = this;
            PermitirBorrar = true;
            PermitirEditar = false;

            PaginaDondeNavegarAlEditar = ((string)parametros.LeerValor(nameof(PaginaDondeNavegarAlEditar), "")).Replace(ltrEndPoint.Controller, "");
            if (!PaginaDondeNavegarAlEditar.IsNullOrEmpty())
            {
                acciones.Add(new ColumnaAccion
                {
                    accion = Referencia.NavegarAEditar((DescriptorDeExpansor)Padre, PaginaDondeNavegarAlEditar, nameof(IRegistro.Id)),
                    titulo = TituloDeAbrir,
                    tamano = 100,
                    visible = true
                });
            }
        }

        public string RenderGridDeRelacion()
        {
            return RenderControl();
        }



        private string RenderColumnasDeAcciones(string columnaId)
        {
            var columnas = "";
            foreach (var accion in acciones)
            {
                var columna = RenderColumnaAccion(accion.titulo, accion.accion, accion.tamano, accion.visible);
                columnas = $"{columnas}{Environment.NewLine}{columna}";
            }
            return columnas;
        }

        private string RenderColumnaAccion(string titulo, string accion, int tamano, bool visible = true)
        {
            var clasesCss = $"{(visible ? enumCssGrid.ColumnaAccion.Render() : enumCssGrid.ColumnaOculta.Render())}";
            var columna = @$"
                <div scope='col' id='[{nameof(IdHtml)}]-{titulo.ToLower()}' class='{enumCssDiv.Th.Render()} {clasesCss}' propiedad='{titulo}' columna-accion=true tamano-fijo='{tamano}' accion = ¨{accion}¨>
                   
                </div>
            ";
            return columna;
        }

        private object renderColumna(Type tipoDto, PropiedadDto propiedadDto)
        {
            var p = propiedadDto.Propiedad.ToLower();

            var propiedadInfo = tipoDto.GetProperty(propiedadDto.Propiedad);
            var alineacion = propiedadInfo != null && propiedadInfo.PropertyType == typeof(bool) ? enumAliniacion.centrada.Render() :propiedadDto.Aliniacion.Render();
            var clasesCss = $"{(propiedadDto.Mostrar ? enumCssGrid.ColumnaCabecera.Render() : enumCssGrid.ColumnaOculta.Render())}";
            if (propiedadDto.Mostrar && propiedadDto.AutoAjustable) clasesCss = $"{clasesCss} auto-ajustable";
            var esFecha = propiedadDto.EsFecha ? "es-fecha = 'T'" : "";
            var formato = propiedadDto.Formato is null ? "" : $"formato = '{propiedadDto.Formato.Descripcion()}'";

            var paddingRigth = propiedadDto.Formato == enumFormato.Porcentaje || propiedadDto.Formato == enumFormato.Moneda || propiedadDto.Formato == enumFormato.Numero_2 || propiedadDto.Formato == enumFormato.Numero_6 ? "padding-right: 1em;" : "";
            var tamanoFijo = propiedadDto.Tamano == 0 ? "" : $"tamano-fijo='{propiedadDto.Tamano}'";
            return @$"
                <div scope='col' id='[{nameof(IdHtml)}]-{p}' class='{enumCssDiv.Th.Render()} {clasesCss}' propiedad='{p}' {tamanoFijo} {formato} {esFecha} style='text-align:{alineacion}; {paddingRigth}'> 
                   {propiedadDto.Titulo}
                </div>
            ";//
        }

        private string renderFilaCabecera()
        {
            var filaCabeceraHtml = $@"
             <div id='[{nameof(IdHtml)}]-tr' class='{enumCssDiv.Tr.Render()}'>
               ColumnasHtml
             </div> 
            "
            ;
            var renderColumnas = "";
            for (var i = 0; i < _Columnas.Cantidad; i++)
            {
                renderColumnas = $"{renderColumnas}{Environment.NewLine}{renderColumna(TipoDto, _Columnas[i])}";
            }

            renderColumnas = $"{renderColumnas}{Environment.NewLine}{RenderColumnasDeAcciones($"{IdHtml}-id")}";

            return filaCabeceraHtml.Replace("ColumnasHtml", renderColumnas);
        }


        public override string RenderControl()
        {
            var filaCabecera = renderFilaCabecera();
            var tablHtml = @$"
            <div id='[{nameof(IdHtmlContenedor)}]' 
                 tipo='{Tipo.Render()}' 
                 name='contenedor-[{nameof(IdHtml)}]' 
                 class='[CssContenedor]' 
                 controlador='[{nameof(Controlador)}]' 
                 accion-de-consulta='[{nameof(AccionDeConsulta)}]'
                 con-capa='[{nameof(ConCapa)}]'
                 accion-de-leer-por-id='[{nameof(AccionDeLeerPorId)}]'
                 restrictor='[{nameof(PropiedadRestrictora)}]'
                 campo-restrictor='[{nameof(CampoRestrictor)}]'
                 restrictor-fijo='[{nameof(RestrictorFijo)}]'
                 cargar-por-evento = '{CargarPorEvento}'
                 ordenar-por='[{nameof(OrdenarPor)}]'
                 filtrar-para='[{nameof(FiltrarPara)}]'
                 modal-para-editar-relacion='[{nameof(idModalParaEditar)}]'
                 aplicar-join='[{nameof(AplicarJoin)}]'
                 id-negocio-vinculado='{IdVinculado}'
                 cantidad-a-leer='[{nameof(Cantidad)}]'
                 id-negocio='[{nameof(IdNegocio)}]'
                 ocultar-si-vacio='{OcultarSiVacio}'
                 tras-cargar-grid='{TrasCargarGrid}'
               >
               <div id='[{nameof(IdHtml)}]-tabla' class='{enumCssDiv.Tabla.Render()} table table-striped' style='margin-bottom: 0px;'>
                  <div id='[{nameof(IdHtml)}]-cabecera' class='{enumCssDiv.Thead.Render()} cuerpo-datos-thead'> 
                     {filaCabecera}
                  </div>
                  <div id = '[{nameof(IdHtml)}]-tbody' class='{enumCssDiv.Tbody.Render()} cuerpo-datos-tbody' style='height: 200px;'>
                  </div>
               </div>
            </div> 
            ";

            var valores = new Dictionary<string, object>();

            valores["CssContenedor"] = Css.Render(enumCssExpansor.GridDeRelacion);
            valores[nameof(IdHtmlContenedor)] = IdHtmlContenedor;
            valores[nameof(IdHtml)] = IdHtml;
            valores[nameof(Controlador)] = Controlador.Name.Replace(ltrEndPoint.Controller, "");
            valores[nameof(AccionDeConsulta)] = AccionDeConsulta;
            valores[nameof(ConCapa)] = ConCapa.ToString();
            valores[nameof(AccionDeLeerPorId)] = AccionDeLeerPorId;
            valores[nameof(PropiedadRestrictora)] = PropiedadRestrictora;
            valores[nameof(CampoRestrictor)] = CampoRestrictor;
            valores[nameof(RestrictorFijo)] = RestrictorFijo;
            valores[nameof(OrdenarPor)] = OrdenarPor;
            valores[nameof(idModalParaEditar)] = idModalParaEditar.ToLower();
            valores[nameof(AplicarJoin)] = AplicarJoin;
            valores[nameof(Cantidad)] = Cantidad;
            valores[nameof(IdNegocio)] = IdNegocio;
            valores[nameof(FiltrarPara)] = FiltrarPara;

            return PlantillasHtml.Render(tablHtml, valores);

        }
    }
}
