using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto;
using ServicioDeDatos;
using System;
using System.Collections.Generic;
using System.Net.Mime;
using Utilidades;
using static SistemaDeElementos.Inicializador.enumVistas;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePaginaDeConsulta : ControlHtml, IControlConIdNegocioConExpansor
    {

        public ContextoSe Contexto { get; }

        protected string _renderCache { get; set; }

        public IControlConIdNegocio Pagina { get; set; }

        public enumNegocio Negocio { get; private set; }
        private string _controlador = null;
        public string Controlador { get { return _controlador.Replace(ltrEndPoint.Controller, ""); } private set { _controlador = value; } }
        public string Vista { get; private set; }

        public enumNameSpaceTs RutaBase { get; set; }

        public int IdNegocio => Negocio.IdNegocio();
        public List<DescriptorDeExpansor> Expanes { get; set; } = new List<DescriptorDeExpansor>();


        public DescriptorDePaginaDeConsulta(ContextoSe contexto, string controlador, string vista, enumNameSpaceTs rutaBase, Type tipoDeElemento, string tituloSingular)
        {
            Contexto = contexto;
            Vista = vista;
            Controlador = controlador.Replace(ltrEndPoint.Controller, "");
            RutaBase = rutaBase;

            Negocio = NegociosDeSe.NegocioDeUnDto(tipoDeElemento);

            // Validación preventiva: ¿tipoDeElemento hereda de ElementoDto?
            if (!typeof(ElementoDto).IsAssignableFrom(tipoDeElemento))
            {
                GestorDeErrores.Emitir($"El tipo {tipoDeElemento.Name} debe heredar de '{nameof(ElementoDto)}'");
            }
            Id = vista;
            Type tipoGenericoBase = typeof(DescriptorDeEdicion<>);
            Type tipoConfigurado = tipoGenericoBase.MakeGenericType(tipoDeElemento);

            // Activator buscará el constructor (DescriptorDeConsulta, string)
            Pagina = (IControlConIdNegocio)Activator.CreateInstance(tipoConfigurado, this, Negocio, tituloSingular);

            new EspansorDeObservaciones(this).DefinirDescriptorDeObservacion();

            if (Negocio == enumNegocio.Infante)
            {
                new EspansorDeEventosDeAgenda(this).DefinirDescriptorDeEventos();
            }

            var expanDeAnexados = new DescriptorDeExpansor(Pagina, $"{Pagina.Id}-archivos", "Archivos", true, "Archivos anexados");
            Expanes.Add(expanDeAnexados);
            expanDeAnexados.CuerpoDelExpansor = new ContenedorDeArchivos(expanDeAnexados, Negocio);

        }


        public override string RenderControl()
        {
            try
            {
                var render = Pagina.RenderControl();

                render = $"{render}{Environment.NewLine}{Expanes.Render(ampliaciones: null, ampliacionDetrasDe: null)}";

                //foreach (var expanes in Expanes)
                //{
                //    foreach (ControlHtml control in expanes.ControlesDelPie)
                //    {
                //        if (control is ModalDeEditarRelacion)
                //            render = $"{render}{Environment.NewLine}{((ModalDeEditarRelacion)control).RendelModalDeEditarRelacion()}";
                //    }
                //}

                return PanelDeControl.RenderPagina(Contexto, render, claseAdicional: enumCssCuerpo.CuerpoSoloConsulta.Render());
            }
            finally
            {
                BlanquearListaDeIds();
            }
        }


        public static object CrearDescriptorDeConsulta(ContextoSe contexto, enumNegocio negocio, int id)
        {
            if (negocio == enumNegocio.No_Definido)
            {
                GestorDeErrores.Emitir("No hay descriptor para un negocio, no definido.");
            }

            if (negocio == enumNegocio.CircuitoDoc)
            {
              var circuito =  negocio.LeerRegistro(contexto, id);
            }

            var descriptor = Activator.CreateInstance(negocio.ObtenerMetadatos().DescriptoDeConsultas, contexto);
            return descriptor;
        }
    }
}
