namespace Callejero {

    export function CrearFormulario(idFormulario: string) {
        Formulario.formulario = new Callejero.ImportarCallejero(idFormulario);
        window.addEventListener("load", function () { Formulario.formulario.InicializarFormulario(); }, false);

        window.onbeforeunload = function () {
            Formulario.formulario.AntesDeSalir();
        }
    }

    class Archivo {
        parametro: string;
        valor: number;
    }

    export class ImportarCallejero extends Formulario.Base {

        constructor(idFormulario: string) {
            super(idFormulario);
        }

        protected AntesDeAceptar(): boolean {
            let sometido: boolean;

            function promesaNoResuelta(form: ImportarCallejero, motivo: string): boolean {
                MensajesSe.Error("PrometoSubirLosArchivos", motivo);
                return false;
            }

            if (super.AntesDeAceptar()) {
                ApiDeArchivos.SubirArchivos(this.CuerpoDelFormulario)
                    .then(resultados => sometido = this.ArchivosSubidos(resultados))
                    .catch(error => sometido = promesaNoResuelta(this, error))
                    .finally(() => {
                        QuitarCapa();
                    });
            }


            return sometido;
        }

        private ArchivosSubidos(resultados: string[]): boolean {
            MensajesSe.Info(`trabajo sometido con ${resultados.length.toString()} ficheros subidos`);
            let sometido: boolean;
            this.SometerTrabajo()
                .then(resultado => sometido = resultado)
                .catch(resultado => sometido = resultado);
            return sometido;
        }

        public SometerTrabajo(): Promise<boolean> {

            let someter: Promise<boolean> = new Promise((resolve, reject) => {
                let arrayDeArchivos: Archivo[] = [];

                let archivos: NodeListOf<HTMLInputElement> = this.CuerpoDelFormulario.querySelectorAll(`[${atControl.tipo}=${ltrTipoControl.Archivo}]`) as NodeListOf<HTMLInputElement>;
                for (let i = 0; i < archivos.length; i++) {
                    let idArchivo = Numero(archivos[i].getAttribute(atArchivo.idArchivo));
                    if (idArchivo > 0) {
                        let archivo: Archivo = new Archivo();
                        archivo.parametro = archivos[i].getAttribute(atControl.propiedad);
                        archivo.valor = idArchivo;
                        arrayDeArchivos.push(archivo);
                    }
                }
                var parametrosSometer = JSON.stringify(arrayDeArchivos);

                let url: string = `/${Ajax.Callejero.Importacion}/${Ajax.Callejero.accion.importar}?parametros=${parametrosSometer}`;

                let a = new ApiDeAjax.DescriptorAjax(this
                    , `${Ajax.Callejero.accion.importar}`
                    , arrayDeArchivos
                    , url
                    , ApiDeAjax.TipoPeticion.Asincrona
                    , ApiDeAjax.ModoPeticion.Get
                    , (peticion) => {
                        this.TrasSometer(peticion);
                        resolve(true);
                    }
                    , (peticion) => {
                        this.SiHayErrorAlSometer(peticion);
                        reject(false);
                    }
                );
                a.Ejecutar();
            });

            return someter;
        }


        private TrasSometer(peticion: ApiDeAjax.DescriptorAjax) {
            let datos: Archivo[] = peticion.DatosDeEntrada;
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.informativo, `Se ha sometido el trabajo de importación con ${datos.length} archivos`);
        }

        private SiHayErrorAlSometer(peticion: ApiDeAjax.DescriptorAjax) {
            let datos: Archivo[] = peticion.DatosDeEntrada;
            MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, `Error al someter el trabajo de importación con ${datos.length} archivos`);
        }
    }



}