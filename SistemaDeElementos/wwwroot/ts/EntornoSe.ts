
namespace EntornoSe {

    export let Historial: HistorialSe.HistorialDeNavegacion = undefined;

    export function IniciarEntorno() {
        AjustarDivs();


        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.Consultor))
            return;

        if (!Registro.HayUsuarioDeConexion())
            Registro.RegistrarUsuarioDeConexion(this)
                .then((usuarioConectado: Registro.UsuarioDeConexion) => {
                    ArbolDeMenu.SolicitarArbolDeMenu('id-contenedor-menu');
                }
                )
                .catch(() => {
                    MensajesSe.Apilar(MensajesSe.enumTipoMensaje.error, "Error al leer el usuario de conexión");
                });
        else
            ArbolDeMenu.SolicitarArbolDeMenu('id-contenedor-menu');
    }

    export function AjustarDivs() {
        let { modalMenu, estadoMenu }: { modalMenu: HTMLDivElement; estadoMenu: HTMLElement; } = ArbolDeMenu.ObtenerDatosMenu();
        if (Definido(estadoMenu) && estadoMenu.getAttribute(atMenu.abierto) === literal.true)
            modalMenu.style.height = `${AlturaDelMenu().toString()}px`;

        if (Definido(Crud.crudMnt)) {
            if (Crud.crudMnt.EsCrud) {
                // Crud.crudMnt.PosicionarGrid();
            }
            if (Crud.crudMnt.ModoTrabajo === enumModoTrabajo.creando) {

            }
        }
    }

    export function AjustarModalesAbiertas(): void {

    }


    export function InicializarHistorial() {
        Historial = new HistorialSe.HistorialDeNavegacion();
    }

    export function PushRestrictores(paginaDestino: string, datos: Tipos.Restrictor[]): void {
        let estadoPaginaDestino: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(paginaDestino);
        estadoPaginaDestino.Agregar(ltrClaveDeEstado.restrictoresDeUnPost, datos);
        estadoPaginaDestino.Guardar();
    }

    export function PushPaginaOrigen(paginaDestino: string, paginaOrigen: string): void {
        let estadoPaginaDestino: HistorialSe.EstadoPagina = EntornoSe.Historial.ObtenerEstado(paginaDestino);
        estadoPaginaDestino.Agregar(ltrClaveDeEstado.paginaOrigen, paginaOrigen);
        estadoPaginaDestino.Guardar();
    }

    export function AbrirPestana(url: string, validar: boolean = true): Promise<void> {
        return new Promise<void>((resolve, reject) => {
            if (validar && !Registro.UrlValidada(url)) {
                validarUrl(url)
                    .then(() => {
                        window.open(url, '_blank');
                        resolve();
                    })
                    .catch((error) => {
                        alert(error.message);
                        reject(error);
                    });
            }
            else {
                window.open(url, '_blank', 'noopener,noreferrer');
                resolve();
            }
        });
    }

    export function DescargarArchivo(url) {
        fetch(url)
            .then(response => response.blob())
            .then(blob => {
                const a = document.createElement('a');
                const url = window.URL.createObjectURL(blob);
                a.href = url;
                a.download = 'archivo_descargado'; // Puedes ajustar el nombre del archivo si es necesario
                document.body.appendChild(a);
                a.click();
                window.URL.revokeObjectURL(url);
                document.body.removeChild(a);
            })
            .catch(error => console.error('Error al descargar el archivo:', error));
    }

    export async function NavegarAUrl(url) {
        if (!Registro.UrlValidada(url))
            try {
                await validarUrl(url);
            }
            catch (error) {
                alert(error.message);
                return;
            }
        window.location.href = url;
    }

    export function NuevaVentana(url: string, width: number, height: number) {
        window.open(url, '_blank', `width=${width},height=${height}`);
    }

    async function validarUrl(url: string) {
        PonerCapa();
        try {
            if (url.includes('/Crud')) {
                const response = await fetch(url, { method: 'HEAD' });
                if (!response.ok || response.status !== 200) {
                    let errorMessage = `Página '${url}' no disponible`;

                    if (response.status === 900) {
                        errorMessage = `Modulo no activo`;
                    } else if (response.status === 404) {
                        errorMessage = `Recurso no encontrado: '${url}'`;
                    } else if (response.status === 500) {
                        errorMessage = `Error interno del servidor al acceder a '${url}'`;
                    }

                    throw new Error(errorMessage);
                }
            }
            Registro.UrlValida(url);
        }
        finally {
            QuitarCapa();
        }
    }

    export function Sumit(form: HTMLFormElement) {
        form.submit();
    }

    export function Llamador(): string {
        var callerName;
        try { throw new Error(); }
        catch (e) {
            var re = /(\w+)@|at (\w+) \(/g, st = e.stack, m;
            re.exec(st), m = re.exec(st);
            callerName = m[1] || m[2];
        }
        return callerName;
    };

    function ContenedoresDeMenus() {
        let menuUltimosRegistros: HTMLDivElement = document.getElementById('contenedor-menu-ultimos-registros') as HTMLDivElement;
        let menuUltimosAccesos: HTMLDivElement = document.getElementById('contenedor-menu-ultimos-accesos') as HTMLDivElement;
        let menuFavoritos: HTMLDivElement = document.getElementById('contenedor-menu-favoritos') as HTMLDivElement;
        let menuAbout: HTMLDivElement = document.getElementById('contenedor-menu-about') as HTMLDivElement;
        return { menuUltimosAccesos, menuFavoritos, menuAbout, menuUltimosRegistros };
    }

    function cerrarMenusFlotantes() {

        let estaElModuloCargado = typeof Crud !== 'undefined';
        if (estaElModuloCargado && Definido(Crud.crudMnt)) {
            if (Crud.crudMnt.EstoyEditandoConsultando) {
                ApiDeMenuFlotante.CerrarMf(Crud.crudMnt.crudDeEdicion.PanelDeEditar);
            }
            else if (Crud.crudMnt.EstoyEnMantenimiento) {
                ApiDeMenuFlotante.CerrarMf(Crud.crudMnt.ContenedorDeLosMenusDelCrud);
            }
            else if (Crud.crudMnt.ModoTrabajo === enumModoTrabajo.historial) {
                ApiDeMenuFlotante.CerrarMf(Crud.crudMnt.crudHistorial.PanelDeHistorial);
            }
            else if (Crud.crudMnt.EstoyCreando) {
                ApiDeMenuFlotante.CerrarMf(Crud.crudMnt.crudDeCreacion.PanelDeCrear);
            }
        }
    }

    export function MostarUltimosRegistros(): void {
        let { menuUltimosAccesos, menuFavoritos, menuAbout, menuUltimosRegistros }:
            { menuUltimosAccesos: HTMLDivElement; menuFavoritos: HTMLDivElement; menuAbout: HTMLDivElement; menuUltimosRegistros: HTMLDivElement; }
            = ContenedoresDeMenus();
        ArbolDeMenu.CerrarMenu();
        cerrarMenusFlotantes();
        if (!menuUltimosRegistros.classList.contains(ltrCss.PanelDeControl.Menu.Oculto))
            ApiControl.IncluirCss(menuUltimosRegistros, ltrCss.PanelDeControl.Menu.Oculto);
        else {
            ApiControl.IncluirCss(menuUltimosAccesos, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.IncluirCss(menuFavoritos, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.IncluirCss(menuAbout, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.ExcluirCss(menuUltimosRegistros, ltrCss.PanelDeControl.Menu.Oculto);
        }
    }

    export function MostarUltimosAccesos(): void {
        let { menuUltimosAccesos, menuFavoritos, menuAbout, menuUltimosRegistros }:
            { menuUltimosAccesos: HTMLDivElement; menuFavoritos: HTMLDivElement; menuAbout: HTMLDivElement; menuUltimosRegistros: HTMLDivElement; }
            = ContenedoresDeMenus();
        ArbolDeMenu.CerrarMenu();
        cerrarMenusFlotantes();
        if (!menuUltimosAccesos.classList.contains(ltrCss.PanelDeControl.Menu.Oculto))
            ApiControl.IncluirCss(menuUltimosAccesos, ltrCss.PanelDeControl.Menu.Oculto);
        else {
            ApiControl.IncluirCss(menuFavoritos, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.IncluirCss(menuAbout, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.IncluirCss(menuUltimosRegistros, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.ExcluirCss(menuUltimosAccesos, ltrCss.PanelDeControl.Menu.Oculto);
        }
    }
    export function MostarOcultarFavoritos(): void {
        let { menuUltimosAccesos, menuFavoritos, menuAbout, menuUltimosRegistros }:
            { menuUltimosAccesos: HTMLDivElement; menuFavoritos: HTMLDivElement; menuAbout: HTMLDivElement; menuUltimosRegistros: HTMLDivElement; }
            = ContenedoresDeMenus();
        ArbolDeMenu.CerrarMenu();
        cerrarMenusFlotantes();
        if (!menuFavoritos.classList.contains(ltrCss.PanelDeControl.Menu.Oculto))
            ApiControl.IncluirCss(menuFavoritos, ltrCss.PanelDeControl.Menu.Oculto);
        else {
            ApiControl.IncluirCss(menuAbout, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.IncluirCss(menuUltimosRegistros, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.IncluirCss(menuUltimosAccesos, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.ExcluirCss(menuFavoritos, ltrCss.PanelDeControl.Menu.Oculto);
        }
    }

    export async function SeleccionarIa(opcion: HTMLAnchorElement) {
        const liElemento = opcion.parentElement as HTMLLIElement;

        if (!liElemento) {
            console.error('No se pudo encontrar el elemento <li> padre.');
            return;
        }
        const iaSeleccionada = liElemento.getAttribute('name');

        const url = `/${ltrControladores.Entorno.Menus}/${Ajax.EndPoint.Entorno.Menu.SeleccionarIa}`;
        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                },
                body: ParametroIa(iaSeleccionada), // Envía los datos como JSON
                keepalive: true
            });

            // La propiedad 'ok' es verdadera si el estado HTTP está entre 200-299.
            if (!response.ok) {
                MensajesSe.Error('SeleccionarIa', `Error HTTP: ${response.status} - ${response.statusText}`);
                return; // Detiene la ejecución si hay un error HTTP.
            }

            // Lee el cuerpo de la respuesta como JSON para verificar si el servidor
            // procesó la petición correctamente (true) o hubo un error (false).
            const exito = await response.json();

            if (exito['estado'] === 'Ok') {
                // Actualiza el DOM solo si el servidor confirma que todo fue bien.
                const opcionesSeleccionadas = document.querySelectorAll('.' + ltrCss.ia.Seleccionada) as NodeListOf<HTMLAnchorElement>;
                opcionesSeleccionadas.forEach(elemento => {
                    ApiControl.ExcluirCss(elemento, ltrCss.ia.Seleccionada);
                });

                ApiControl.IncluirCss(opcion, ltrCss.ia.Seleccionada);
                OcultarMenusRapidos();

                let estaElModuloCargado = typeof Crud !== 'undefined';
                if (estaElModuloCargado && Definido(Crud.crudMnt) && Crud.crudMnt.ModoTrabajo === enumModoTrabajo.creando) {
                    const aElemento = liElemento.querySelector('a');
                    let nombreIaTexto = '';
                    if (aElemento) {
                        nombreIaTexto = aElemento.textContent;
                        nombreIaTexto = aElemento.textContent.trim();
                    }
                    Crud.crudMnt.crudDeCreacion.ResetearIa(nombreIaTexto);
                };

            } else {
                // Manejar el caso donde el servidor devuelve 'false'.
                MensajesSe.Error('SeleccionarIa', exito['mensaje']);
            }
        } catch (error) {
            // Capturar errores de red o errores al procesar el JSON.
            console.error('Error en fetch:', error);
            MensajesSe.Error('SeleccionarIa', 'Ha ocurrido un error de conexión o de procesamiento de datos.');
        }
    }

    function ParametroIa(iaSeleccionada: string) {
        let parametros: Array<Parametro> = new Array<Parametro>();
        parametros.push(new Parametro(ltrPropiedades.Entorno.Menu.iaSeleccionada, iaSeleccionada));


        return JSON.stringify(parametros);
    }


    export function MostarOcultarAbout(): void {
        let { menuUltimosAccesos, menuFavoritos, menuAbout, menuUltimosRegistros }:
            { menuUltimosAccesos: HTMLDivElement; menuFavoritos: HTMLDivElement; menuAbout: HTMLDivElement; menuUltimosRegistros: HTMLDivElement; }
            = ContenedoresDeMenus();
        ArbolDeMenu.CerrarMenu();
        cerrarMenusFlotantes();
        if (!menuAbout.classList.contains(ltrCss.PanelDeControl.Menu.Oculto))
            ApiControl.IncluirCss(menuAbout, ltrCss.PanelDeControl.Menu.Oculto);
        else {
            ApiControl.IncluirCss(menuFavoritos, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.IncluirCss(menuUltimosRegistros, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.IncluirCss(menuUltimosAccesos, ltrCss.PanelDeControl.Menu.Oculto);
            ApiControl.ExcluirCss(menuAbout, ltrCss.PanelDeControl.Menu.Oculto);
        }
    }


    export function OcultarMenusRapidos(): void {
        let { menuUltimosAccesos, menuFavoritos, menuAbout, menuUltimosRegistros }:
            { menuUltimosAccesos: HTMLDivElement; menuFavoritos: HTMLDivElement; menuAbout: HTMLDivElement; menuUltimosRegistros: HTMLDivElement; }
            = ContenedoresDeMenus();
        ApiControl.IncluirCss(menuFavoritos, ltrCss.PanelDeControl.Menu.Oculto);
        ApiControl.IncluirCss(menuUltimosRegistros, ltrCss.PanelDeControl.Menu.Oculto);
        ApiControl.IncluirCss(menuUltimosAccesos, ltrCss.PanelDeControl.Menu.Oculto);
        ApiControl.IncluirCss(menuAbout, ltrCss.PanelDeControl.Menu.Oculto);
    }

    export function CambiarPassword(): void {
        Modales.SolicitarModal('contenedor-cambiar-password', 'cambiar-password');
        MostarOcultarFavoritos();
    }

    export function SubirCertificado(): void {
        Modales.SolicitarModal('contenedor-subir-certificado', 'subir-certificado');
        MostarOcultarFavoritos();
    }

    export function AbrirModalIa(): void {
        Modales.SolicitarModal('contenedor-modal-ia', 'modal-ia');
    }

    export function MiCalendario(): void {
        let urlBase: string = window.location.origin;
        let url: string = `${urlBase}/${Ajax.Entorno.Agenda.visor.controlador}/${Ajax.Entorno.Agenda.visor.crud}`;
        NavegarAUrl(url);
    }

    export function Logout(): void {
        let urlBase: string = window.location.origin;
        let url: string = `${urlBase}/${Ajax.Entorno.Acceso.controlador}/${Ajax.Entorno.Acceso.Logout}`;
        NavegarAUrl(url);
    }

    export function DescargarDeclaracionResponsable(): void {

        ApiDePeticiones.ValidarExisteDeclaracionResponsable(this)
            .then((peticion) => {
                let urlBase: string = window.location.origin;
                let url: string = `${urlBase}/${Ajax.Ventas.Facturas.controlador}/${Ajax.Ventas.Facturas.accion.DescargarDeclaracionResponsable}`;
                NavegarAUrl(url);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion))
            .finally(() => MostarOcultarAbout());
    }

    export function NavegarAtras(): void {
        if (document.referrer.includes(`/${ltrControladores.Entorno.Acceso}/${ltrVistas.Entorno.Conectar}`))
            location.href = document.referrer.replace(`/${ltrControladores.Entorno.Acceso}/${ltrVistas.Entorno.Conectar}`, '');
        else
            window.history.back();
    }

    export function MiCorreoApiKey(): void {
        let urlBase: string = window.location.origin;
        let url: string = `${urlBase}/${Ajax.Entorno.MiCorreo.controlador}/${Ajax.Entorno.MiCorreo.crudApiKey}`;
        NavegarAUrl(url);
    }

    export async function MiCorreo(): Promise<void> {
        ApiDePeticiones.PedirAccesoAlCorreo(this)
            .then((peticion) => EntornoSe.DespuesDePedirAccesoAlCorreo(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion))
            .finally(() => MostarOcultarFavoritos());

    }

    export function DespuesDePedirAccesoAlCorreo(peticion: ApiDeAjax.DescriptorAjax): any {
        var url = peticion.resultado.datos;
        EntornoSe.AbrirPestana(url);
    }


    export async function Fichar(): Promise<void> {
        const obtenerPosicion = (): Promise<GeolocationPosition | null> => {
            return new Promise((resolve) => {
                if (!navigator.geolocation) return resolve(null);
                navigator.geolocation.getCurrentPosition(
                    (pos) => resolve(pos),
                    () => resolve(null), 
                    { timeout: 10000 }
                );
            });
        };

        const posicion = await obtenerPosicion();
        const lat = posicion?.coords.latitude;
        const lon = posicion?.coords.longitude;

        ApiDePeticiones.Fichar(this, lat, lon)
            .then((peticion) => EntornoSe.DespuesDeFichar(peticion))
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion))
            .finally(() => MostarOcultarFavoritos());
    }



    export function DespuesDeFichar(peticion: ApiDeAjax.DescriptorAjax): any {
        //var url = peticion.resultado.datos;
        ApiLocalStorage.CambiarTextoFichada()
        //window.location.reload();
        //EntornoSe.AbrirPestana(url)
    }


}