using ServicioDeDatos;
using MVCSistemaDeElementos.Controllers;
using UtilidadesParaIu;
using Utilidades;
using ModeloDeDto.Guarderias;
using ServicioDeDatos.Guarderias;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorDeAulasDeGuarderia : DescriptorDeCrud<AulaDeGuarderiaDto>
    {
        public DescriptorDeAulasDeGuarderia(ContextoSe contexto, ModoDescriptor modo)
        : base(contexto
               , nameof(AulasDeGuarderiaController)
               , nameof(AulasDeGuarderiaController.CrudAulasDeGuarderia)
               , modo
               , rutaBase: enumNameSpaceTs.Guarderias
              , tituloPlural: "Gestión de Aulas"
              , tituloSingular: "Aula")
        {
            Mnt.Filtro.FiltroDeNombre.CambiarAtributos(ltrDeAulasDeGuarderia.Aula, nameof(AulaDeGuarderiaDto.Expresion), "Buscar por nombre o cg");
        }


        public override string RenderControl()
        {
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{Modo}-{GetType().FullName}";
            if (ServicioDeCaches.Obtener(CacheDe.RenderCrud).ContainsKey(indice))
				 return (string)ServicioDeCaches.Obtener(CacheDe.RenderCrud)[indice];
            var render = base.RenderControl();

            render = render +
                   $@"<script src=¨../../js/{RutaBase}/AulasDeGuarderia.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{      
                           {RutaBase}.CrearCrudDeAulasDeGuarderia('{Mnt.IdHtml}', '{Creador.IdHtml}', '{Editor.IdHtml}', '{Borrado.IdHtml}') 
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
