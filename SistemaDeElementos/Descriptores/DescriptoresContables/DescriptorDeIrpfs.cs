using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ServicioDeDatos.Seguridad;
using ModeloDeDto.Contabilidad;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeIrpfs : DescriptorDeCrud<IrpfDto>
    {
        public DescriptorDeIrpfs(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(IrpfsController)
               , nameof(IrpfsController.CrudIrpfs)
               , modo
               , rutaBase: enumNameSpaceTs.Contabilidad)
        {
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos(ltrIrpf.Irpf, nameof(IrpfDto.Expresion), "Buscar por código o descripción de cuenta");
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/Irpfs.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeIrpfs('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
