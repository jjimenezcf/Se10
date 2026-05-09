using GestorDeElementos;
using ModeloDeDto.Contabilidad;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using Utilidades;
using UtilidadesParaIu;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDePreasientos : DescriptorDeCrud<PreasientoDto>
    {
        public DescriptorDePreasientos(ContextoSe contexto, string renderCache) : base(contexto, renderCache)
        {
        }

        public DescriptorDePreasientos(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(PreasientosController)
               , nameof(PreasientosController.CrudPreasientos)
               , modo
               , rutaBase: enumNameSpaceTs.Contabilidad)
        {
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos("Varios", nameof(ltrDeUnPreasiento.NombreCuentaApunte), "Buscar por preasiento (p:) o referencia (r:) o cuenta (c:) o apunte (a:)");

            Mnt.OrdenacionInicial = $"{nameof(PreasientoDto.Referencia)}:{nameof(PreasientoDto.Referencia)}:{enumModoOrdenacion.descendente.Render()}";

            modalesParaPedirDatos.Add(new ModalDeTotales(this, typeof(TotalesPorCuenta), eventosDeMf.Totalizador_Mostrar, $"{ltrTotalizador.Menu_MostrarTotales} de la selección"));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Totalizador_Mostrar}' accion-menu='{eventosDeMf.Totalizador_Mostrar}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Consultor, false)}>{ltrTotalizador.Menu_MostrarTotales}</li>");

            modalesParaPedirDatos.Add(new ModalParaPedirDatos(this, typeof(CrearLoteContableDto), eventosDeMf.Spr_CrearLote, ltrDeUnPreasiento.Menu_CrearLoteContable));
            Mnt.IncluirMfContextual($"<li id='{menuContextual}.{eventosDeMf.Spr_CrearLote}' accion-menu='{eventosDeMf.Spr_CrearLote}' {AtributosHtml.Mf(enumCssOpcionMenu.DeVista, enumModoDeAccesoDeDatos.Gestor, false)}>{ltrDeUnPreasiento.Menu_CrearLoteContable}</li>");

            var idsDeUsuarios = enumNegocio.Preasiento.Parametro(enumParametrosDePreasiento.SPR_Usuarios_Con_Permiso_Para_Generar, valorPorDefecto: Literal.Cero).Valor.ToLista<int>();
            if (idsDeUsuarios.Contains(contexto.DatosDeConexion.IdUsuario))
            {
                Editor.IncluirMfIndividual(ltrDeUnPreasiento.Menu_RegenerarPreasiento, eventosDeMf.Spr_RegenerarPreasiento, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
                Editor.IncluirMfIndividual(ltrDeUnPreasiento.Menu_CrearLoteContable, eventosDeMf.Spr_CrearLoteConUnPreasiento, enumCssOpcionMenu.DeElemento, enumModoDeAccesoDeDatos.Administrador);
            }
            var modal = new ModalDeFiltrado<PreasientoDto>(Mnt.Filtro, "filtrosDelSpr", "Datos de preasientos");
            Mnt.Filtro.Modales.Add(modal);
            modal.ControlesDeFiltrado.Add(new DescriptorFltDePreasientos(modal));
        }

        public override string RenderControl()
        {
            if (!_renderCache.IsNullOrEmpty())
                return _renderCache;

            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Preasientos.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDePreasientos('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
