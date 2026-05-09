using Utilidades;
using GestoresDeNegocio.Callejero;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using UtilidadesParaIu;
using ServicioDeDatos.Callejero;

namespace MVCSistemaDeElementos.Descriptores
{
    public class DescriptorImportarCallejero: DescriptorDeFormulario
    {
        public DescriptorImportarCallejero(ContextoSe contexto):
            base(contexto,
                id: "importar-callejero",
                titulo: "Importar Callejero",
                controlador: nameof(ImportarCallejeroController),
                ruta: enumNameSpaceTs.Callejero,
                vista: nameof(ImportarCallejeroController.ImportarCallejero))
        {

            //Añadimos los dos bloques que hay en el cuerpo
            Cuerpo.BloquesApilados.Add(new BloqueApilado(Cuerpo, "General", "Datos maestros"));
            Cuerpo.BloquesApilados.Add(new BloqueApilado(Cuerpo, "Otros", "Callejero"));

            var bloque = Cuerpo.BloquesApilados[0];
            //añadimos los controles del primer bloque del lado izquierdo
            bloque.Izquierdo.Add(new ControlDeArchivoEnFormulario(bloque, GestorDePaises.ltrDeUnPais.ParametroPais, "Fichero de paises", "Selecciona un fichero para importar los paises", "*.csv"));
            bloque.Izquierdo.Add(new ControlDeArchivoEnFormulario(bloque, ltrDeUnaProvincia.ParametroProvincia, "Fichero de provincias", "Selecciona un fichero para importar provincias", "*.csv"));
            bloque.Izquierdo.Add(new ControlDeArchivoEnFormulario(bloque, ltrDeUnMunicipio.csvMunicipio, "Fichero de municipio", "Selecciona un fichero para importar municipios", "*.csv"));
            bloque.Izquierdo.Add(new ControlDeArchivoEnFormulario(bloque, GestorDeTiposDeVia.ltrDeUnTipoDeVia.ParametroTipoDeVia, "Fichero de tipos de vía", "Selecciona un fichero para importar tipos de vías", "*.csv"));
            bloque.Izquierdo.Add(new ControlDeArchivoEnFormulario(bloque, GestorDeCodigosPostales.ltrDeUnCp.csvCp, "Fichero de CP", "Selecciona un fichero para importar los códigos postales", "*.csv"));
            
            //añadimos los controles del primer bloque del lado derecho
            bloque.Derecho.Add(new ControlDeArchivoEnFormulario(bloque, GestorDeZonas.ltrZonas.ParametroZona, "Fichero de pedanias", "Selecciona un fichero para importar los pedanias", "*.csv"));
            bloque.Derecho.Add(new ControlDeArchivoEnFormulario(bloque, GestorDeBarrios.ltrBarrios.ParametroBarrio, "Fichero de barrios", "Selecciona un fichero para importar los barrios", "*.csv"));


            bloque = Cuerpo.BloquesApilados[1];
            //añadimos los controles del segundo bloque del lado izquierdo
            bloque.Izquierdo.Add(new ControlDeArchivoEnFormulario(bloque, ltrCalles.csvCalle, "Fichero de calles", "Selecciona un fichero para importar un callejero", "*.csv"));
        }

        public string RenderImportarCallejero()
        {
            var render = RenderFormulario();

            render = render +
                   $@"<script src=¨../../js/{RutaVista}/ImportarCallejero.js?v={System.DateTime.Now.Ticks}¨></script>
                      <script>
                         try {{                           
                            {RutaVista}.CrearFormulario('{IdHtml}') 
                         }}
                         catch(error) {{                           
                            MensajesSe.Error('Creando el formulario', error);
                         }}
                      </script>
                    ";

            return render.Render();
        }

    }
}
