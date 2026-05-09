using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Contabilidad;
using ServicioDeDatos.Contabilidad;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeCuentas : DescriptorDeCrud<CuentaDto>
    {
        public DescriptorDeCuentas(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(CuentasController)
               , nameof(CuentasController.CrudCuentas)
               , modo
               , rutaBase: enumNameSpaceTs.Contabilidad)
        {          
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos(ltrCuenta.Cuenta, nameof(CuentaDto.Expresion), "Buscar por código o descripción de cuenta");
            Mnt.OrdenacionInicial = $"{nameof(CuentaDto.Codigo)}:{nameof(CuentaDto.Codigo)}:{enumModoOrdenacion.ascendente.Render()}";
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Cuentas.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeCuentas('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el crud', error.message);
                         }}
                      </script>
                    ";
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
            ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice] = render.Render();
			return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
        }


    }
}
