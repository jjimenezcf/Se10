using GestorDeElementos;
using ModeloDeDto.Ventas;
using MVCSistemaDeElementos.Controllers;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorFacturasAeat : DescriptorDeCrud<FacturaAeatDto>
    {

        public DescriptorFacturasAeat(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }

        public DescriptorFacturasAeat(ContextoSe contexto, ModoDescriptor modo)
            : base(contexto,
                  nameof(FacturasEmtController),
                  nameof(FacturasEmtController.CrudFacturasAeat),
                  modo,
                  rutaBase: enumNameSpaceTs.Venta,
                  tituloPlural: ltrVistasTitulos.CrudDeFacturasAeat)
        {
            Mnt.BloqueGeneral.QuitarControl(nameof(INombre.Nombre));

            var sociedades = contexto.SeleccionarTodos<SociedadDtm>(
                filtros: new Dictionary<string, object> { {ltrDeSociedad.FiltroParaSociedadesGestionadas, true }}, 
                parametros: new Dictionary<string, object> { {ltrParametrosNeg.AplicarElOrden,new List<ClausulaDeOrdenacion> { new ClausulaDeOrdenacion(nameof(SociedadDtm.NIF), ModoDeOrdenancion.ascendente) }}}
                );

            var opciones = new Dictionary<string, string>();
            foreach (SociedadDtm sociedad in sociedades)
                opciones.Add(sociedad.Id.ToString(), sociedad.Expresion);

            var filtroPorSociedad = new ListaDeValores<FacturaAeatDto>(Mnt.BloqueGeneral,
                etiqueta: "Sociedad",
                ayuda: "Seleccione la sociedad",
                opciones: opciones,
                filtraPor: nameof(ltrDeSociedad.FiltroParaSociedadesGestionadas),
                posicion: new Posicion() { fila = 0, columna = 0 });

            var filtroPorAno  = new FiltroConNumero<FacturaAeatDto>(Mnt.BloqueGeneral,
                etiqueta: "Periodo facturación",
                ayuda: "Año de la factura",
                propiedad: ltrDeUnaFacturaEmt.AnoDeEmisison,
                posicion: new Posicion() { fila = 0, columna = 1 },
                valorPorDefecto: DateTime.Now.Year.ToString());

            opciones = new Dictionary<string, string>
            {
                { "1", "Enero" },
                { "2", "Febrero" },
                { "3", "Marzo" },
                { "4", "Abril" },
                { "5", "Mayo" },
                { "6", "Junio" },
                { "7", "Julio" },
                { "8", "Agosto" },
                { "9", "Septiembre" },
                { "10", "Octubre" },
                { "11", "Noviembre" },
                { "12", "Diciembre" }
            };

            var filtroPorMes = new ListaDeValores<FacturaAeatDto>(Mnt.BloqueGeneral,
                etiqueta: "",
                ayuda: "Seleccione el mes",
                opciones: opciones,
                filtraPor: ltrDeUnaFacturaEmt.MesDeEmision,
                posicion: new Posicion() { fila = 0, columna = 1 });

        }


        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/ApiDeFacturasEmt.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script src=¨../../js/{RutaBase}/FacturasAeat.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeFacturasAeat('{Mnt.IdHtml}') 
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
