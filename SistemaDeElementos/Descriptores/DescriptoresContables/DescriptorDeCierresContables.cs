using GestorDeElementos;
using ModeloDeDto.Contabilidad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCierresContables : DescriptorDeCrud<CierreContableDto>
    {
        public DescriptorDeCierresContables(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CierresContablesController)
               , nameof(CierresContablesController.CrudCierresContables)
               , modo
               , rutaBase: enumNameSpaceTs.Contabilidad)
        {
            Mnt.OrdenacionInicial = $"{nameof(CierreContableDto.CerradaEl)}:{nameof(CierreContableDto.CerradaEl)}:{enumModoOrdenacion.descendente.Render()}";

            DefinirFiltros(contexto);

            // Realizar cierre (menú general) pide sociedad, ejercicio y contabilidad y somete el trabajo de crear cierre contable
            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CrearCierreContableDto), eventosDeMf.Ccc_CrearCierre, ltrDeUnCierreContable.Menu_CrearCierre));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Ccc_CrearCierre}' accion-menu='{eventosDeMf.Ccc_CrearCierre}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>{ltrDeUnCierreContable.Menu_CrearCierre}</li>");

            // Eliminar cierre (menú individual) somete el trabajo de anular el lote contable del cierre seleccionado
            Mnt.OpcionesPorElemento.Add($"<li id='{menuIndividual}.{eventosDeMf.Ccc_EliminarCierre}' accion-menu='{eventosDeMf.Ccc_EliminarCierre}' {AtributosHtml.Mf(enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Gestor, false)}>{ltrDeUnCierreContable.Menu_EliminarCierre}</li>");
        }

        // Filtros por lote (nombre o referencia), por sociedad y por fecha de cierre
        private void DefinirFiltros(ContextoSe contexto)
        {
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos("Lote", ltrDeUnCierreContable.FiltroPorLote, "Buscar por nombre o referencia del lote contable");

            var sociedades = contexto.SeleccionarTodos<SociedadDtm>(
                filtros: new Dictionary<string, object> { { ltrDeSociedad.FiltroParaSociedadesACerrar, true } },
                parametros: new Dictionary<string, object> { { ltrParametrosNeg.AplicarElOrden, new List<ClausulaDeOrdenacion> { new ClausulaDeOrdenacion(nameof(SociedadDtm.NIF), ModoDeOrdenancion.ascendente) } } }
                );

            var opciones = new Dictionary<string, string>();
            foreach (SociedadDtm sociedad in sociedades)
                opciones.Add(sociedad.Id.ToString(), sociedad.Expresion);

            new ListaDeValores<CierreContableDto>(Mnt.BloqueGeneral,
                etiqueta: "Sociedad",
                ayuda: "Seleccione la sociedad",
                opciones: opciones,
                filtraPor: nameof(CierreContableDto.IdSociedad),
                posicion: new Posicion() { fila = 0, columna = 1 });

            new FiltroEntreFechas<CierreContableDto>(Mnt.BloqueGeneral,
                etiqueta: "Cerrada el",
                propiedad: nameof(CierreContableDto.CerradaEl),
                ayuda: "cierres realizados entre fechas",
                posicion: new Posicion() { fila = 1, columna = 0 });
        }

        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
                return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/CierresContables.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{
                           {RutaBase}.CrearCrudDeCierresContables('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}')
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
