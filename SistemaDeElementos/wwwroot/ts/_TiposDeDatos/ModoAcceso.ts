namespace ModoAcceso {

    export enum enumModoDeAccesoDeDatos {
        Administrador,
        Interventor,
        Gestor,
        Consultor,
        SinPermiso,
        Creador,
        SerAdministrador
    }

    export const ModoDeAccesoDeDatos = {
        SerAdministrador: 'SerAdministrador',
        Administrador: "Administrador",
        Interventor: "Interventor",
        Gestor: "Gestor",
        Consultor: "Consultor",
        SinPermiso: "SinPermiso",
        Creador: "Creador"
    };


    function ParsearPermiso(permiso: string): enumModoDeAccesoDeDatos {
        switch (permiso.toLowerCase().trim()) {
            case 'seradministrador': return enumModoDeAccesoDeDatos.SerAdministrador;
            case 'administrador': return enumModoDeAccesoDeDatos.Administrador;
            case 'interventor': return enumModoDeAccesoDeDatos.Interventor;
            case 'creador': return enumModoDeAccesoDeDatos.Creador;
            case 'gestor': return enumModoDeAccesoDeDatos.Gestor;
            case 'consultor': return enumModoDeAccesoDeDatos.Consultor;
            case 'sinPermiso': return enumModoDeAccesoDeDatos.SinPermiso;
        }
        MensajesSe.EmitirExcepcion('ParsearPermiso', `No está definido el permiso: ${permiso}`);
    }

    export function HayPermisos(permisosNecesarios: enumModoDeAccesoDeDatos, permisosDelUsuario: enumModoDeAccesoDeDatos | string): boolean {


        if (typeof permisosDelUsuario === 'string')
            permisosDelUsuario = ParsearPermiso(permisosDelUsuario);

        if (permisosNecesarios === enumModoDeAccesoDeDatos.SerAdministrador)
            return Registro.EsAdministrador();

        if (permisosNecesarios === enumModoDeAccesoDeDatos.SinPermiso)
            return false;

        if (permisosNecesarios === enumModoDeAccesoDeDatos.Consultor && permisosDelUsuario !== enumModoDeAccesoDeDatos.SinPermiso)
            return true;

        if (permisosNecesarios === enumModoDeAccesoDeDatos.Gestor &&
            (permisosDelUsuario === enumModoDeAccesoDeDatos.Gestor || permisosDelUsuario === enumModoDeAccesoDeDatos.Interventor || permisosDelUsuario === enumModoDeAccesoDeDatos.Administrador)
        )
            return true;

        if (permisosNecesarios === enumModoDeAccesoDeDatos.Interventor &&
            (permisosDelUsuario === enumModoDeAccesoDeDatos.Interventor || permisosDelUsuario === enumModoDeAccesoDeDatos.Administrador)
        )
            return true;

        if (permisosNecesarios === enumModoDeAccesoDeDatos.Administrador && permisosDelUsuario === enumModoDeAccesoDeDatos.Administrador)
            return true;

        return false;
    }

    export function Parsear(modoDeAcceso: string): enumModoDeAccesoDeDatos {
        if (!HayAlgunPermisos(modoDeAcceso))
            return enumModoDeAccesoDeDatos.SinPermiso;
        if (SerAdministrador(modoDeAcceso))
            return enumModoDeAccesoDeDatos.SerAdministrador;
        if (EsAdministrador(modoDeAcceso))
            return enumModoDeAccesoDeDatos.Administrador;
        if (EsInterventor(modoDeAcceso))
            return enumModoDeAccesoDeDatos.Interventor;
        if (EsGestor(modoDeAcceso))
            return enumModoDeAccesoDeDatos.Gestor;
        if (EsConsultor(modoDeAcceso))
            return enumModoDeAccesoDeDatos.Consultor;
        return enumModoDeAccesoDeDatos.SinPermiso;
    }

    export function Descripcion(modoDeAcceso: enumModoDeAccesoDeDatos): string {
        if (EsAdministrador(modoDeAcceso))
            return ModoDeAccesoDeDatos.Administrador;
        if (EsInterventor(modoDeAcceso))
            return ModoDeAccesoDeDatos.Interventor;
        if (EsGestor(modoDeAcceso))
            return ModoDeAccesoDeDatos.Gestor;
        if (EsConsultor(modoDeAcceso))
            return ModoDeAccesoDeDatos.Consultor;
        return ModoDeAccesoDeDatos.SinPermiso;
    }

    export function HayAlgunPermisos(modoAcceso: string): boolean {
        if (IsNullOrEmpty(modoAcceso) || modoAcceso === ModoDeAccesoDeDatos.SinPermiso)
            return false;
        else
            return true;
    }

    export function EsCreador(objeto: any): boolean {
        let idCreador: number = ObtenerPropiedad(objeto, ltrPropiedades.Elemento.IdCreador, 0, false);
        if (idCreador === 0) return true;
        return Registro.UsuarioConectado().id === idCreador;
    }

    export function SerAdministrador(modoAcceso: string | enumModoDeAccesoDeDatos): boolean {
        if (typeof modoAcceso === 'string') {
            if (IsNullOrEmpty(modoAcceso))
                return false;

            if (ModoDeAccesoDeDatos.SerAdministrador === modoAcceso && Registro.EsAdministrador())
                return true;
            else
                return false;

        }
        switch (modoAcceso as enumModoDeAccesoDeDatos) {
            case enumModoDeAccesoDeDatos.SinPermiso: return false;
            case enumModoDeAccesoDeDatos.Consultor: return false;
            case enumModoDeAccesoDeDatos.Gestor: return false;
            case enumModoDeAccesoDeDatos.Interventor: return false;
            case enumModoDeAccesoDeDatos.Administrador: return false;
            case enumModoDeAccesoDeDatos.SerAdministrador: return Registro.EsAdministrador();
        }
    }
    export function EsAdministrador(modoAcceso: string | enumModoDeAccesoDeDatos): boolean {
        if (typeof modoAcceso === 'string') {
            if (IsNullOrEmpty(modoAcceso))
                return false;

            if (ModoDeAccesoDeDatos.Administrador === modoAcceso || (ModoDeAccesoDeDatos.SerAdministrador === modoAcceso && Registro.EsAdministrador()))
                return true;
            else
                return false;

        }
        switch (modoAcceso as enumModoDeAccesoDeDatos) {
            case enumModoDeAccesoDeDatos.SinPermiso: return false;
            case enumModoDeAccesoDeDatos.Consultor: return false;
            case enumModoDeAccesoDeDatos.Gestor: return false;
            case enumModoDeAccesoDeDatos.Interventor: return false;
            case enumModoDeAccesoDeDatos.Administrador: return true;
            case enumModoDeAccesoDeDatos.SerAdministrador: return Registro.EsAdministrador();
        }
    }

    export function EsInterventor(modoAcceso: string | enumModoDeAccesoDeDatos): boolean {
        if (typeof modoAcceso === 'string') {
            if (IsNullOrEmpty(modoAcceso))
                return false;

            if (EsAdministrador(modoAcceso) || ModoDeAccesoDeDatos.Interventor === modoAcceso)
                return true;
            else
                return false;

        }
        switch (modoAcceso as enumModoDeAccesoDeDatos) {
            case enumModoDeAccesoDeDatos.SinPermiso: return false;
            case enumModoDeAccesoDeDatos.Consultor: return false;
            case enumModoDeAccesoDeDatos.Gestor: return false;
            case enumModoDeAccesoDeDatos.Interventor: return true;
            case enumModoDeAccesoDeDatos.Administrador: return true;
            case enumModoDeAccesoDeDatos.SerAdministrador: return Registro.EsAdministrador();
        }
    }

    export function EsGestor(modoAcceso: string | enumModoDeAccesoDeDatos): boolean {
        if (typeof modoAcceso === 'string') {
            if (IsNullOrEmpty(modoAcceso))
                return false;

            if (EsAdministrador(modoAcceso) || EsInterventor(modoAcceso) || ModoDeAccesoDeDatos.Gestor === modoAcceso)
                return true;
            else
                return false;

        }
        switch (modoAcceso as enumModoDeAccesoDeDatos) {
            case enumModoDeAccesoDeDatos.SinPermiso: return false;
            case enumModoDeAccesoDeDatos.Consultor: return false;
            case enumModoDeAccesoDeDatos.Gestor: return true;
            case enumModoDeAccesoDeDatos.Interventor: return true;
            case enumModoDeAccesoDeDatos.Administrador: return true;
            case enumModoDeAccesoDeDatos.SerAdministrador: return Registro.EsAdministrador();
        }
    }

    export function EsConsultor(modoAcceso: string | enumModoDeAccesoDeDatos): boolean {
        if (typeof modoAcceso === 'string') {
            if (IsNullOrEmpty(modoAcceso))
                return false;

            if (EsGestor(modoAcceso) || ModoDeAccesoDeDatos.Consultor === modoAcceso)
                return true;
            else
                return false;
        }

        switch (modoAcceso as enumModoDeAccesoDeDatos) {
            case enumModoDeAccesoDeDatos.SinPermiso: return false;
            case enumModoDeAccesoDeDatos.Consultor: return true;
            case enumModoDeAccesoDeDatos.Gestor: return true;
            case enumModoDeAccesoDeDatos.Interventor: return true;
            case enumModoDeAccesoDeDatos.Administrador: return true;
            case enumModoDeAccesoDeDatos.SerAdministrador: return Registro.EsAdministrador();
        }
    }

    export function AplicarPermisosAReferenciasPost(panel: HTMLDivElement, objeto: any): void {
        let referencias: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.ReferenciaPost}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < referencias.length; i++) {
            if (referencias[i].readOnly)
                continue;
            let cumpleConAlgunoDeLosPermisos: string = referencias[i].getAttribute(atControl.PermisosNecesarios);
            if (IsNullOrEmpty(cumpleConAlgunoDeLosPermisos))
                continue;
            let cumple: boolean = TieneAlgunoDeLosPermisos(referencias[i].id, objeto, cumpleConAlgunoDeLosPermisos);
            if (!cumple)
                ApiControl.BloquearReferenciaPost(referencias[i]);
            else
                ApiControl.DesbloquearReferenciaPost(referencias[i]);
        }
    }

    export function AplicarPermisosAReferencias(panel: HTMLDivElement, objeto: any): void {
        let referencias: NodeListOf<HTMLAnchorElement> = panel.querySelectorAll(`a[${atControl.tipo}='${ltrTipoControl.Referencia}']`) as NodeListOf<HTMLAnchorElement>;
        for (var i = 0; i < referencias.length; i++) {
            let cumpleConAlgunoDeLosPermisos: string = referencias[i].getAttribute(atControl.PermisosNecesarios);
            if (IsNullOrEmpty(cumpleConAlgunoDeLosPermisos))
                continue;
            let cumple: boolean = TieneAlgunoDeLosPermisos(referencias[i].id, objeto, cumpleConAlgunoDeLosPermisos);
            ApiControl.OcultarHtmlAnchor(referencias[i], !cumple);
        }
    }
    export function HabilitarRefSi(panel: HTMLDivElement, id: string, condicion: boolean): void {
        if (condicion)
            HabilitarRef(panel, id);
        else
            DeshabilitarRef(panel, id);
    }

    export function HabilitarRef(panel: HTMLDivElement, id: string): void {
        let controles = panel.querySelectorAll(`a[${atControl.id}='${id}']`) as NodeListOf<HTMLElement>;

        if (controles.length !== 1)
            MensajesSe.Info(`No se ha localizado el control o hay más de uno, ${id}`);
        else {

            let tipo = controles[0].getAttribute(atControl.tipo);
            if (tipo === ltrTipoControl.Referencia)
                AjustarReferencia(controles[0] as HTMLAnchorElement, ModoAcceso.enumModoDeAccesoDeDatos.Gestor);
        }
    }

    export function DeshabilitarRef(panel: HTMLDivElement, id: string): void {
        let controles = panel.querySelectorAll(`a[${atControl.id}='${id}']`) as NodeListOf<HTMLElement>;

        if (controles.length !== 1)
            MensajesSe.Info(`No se ha localizado el control o hay más de uno, ${id}`);
        else {

            let tipo = controles[0].getAttribute(atControl.tipo);
            if (tipo === ltrTipoControl.Referencia)
                AjustarReferencia(controles[0] as HTMLAnchorElement, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
        }
    }

    export function AplicarPermisosAEditores(panel: HTMLDivElement, objeto: any): void {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.Editor}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < editores.length; i++) {
            if (editores[i].readOnly)
                continue;
            let cumpleConAlgunoDeLosPermisos: string = editores[i].getAttribute(atControl.PermisosNecesarios);
            if (IsNullOrEmpty(cumpleConAlgunoDeLosPermisos))
                continue;
            let cumple: boolean = TieneAlgunoDeLosPermisos(editores[i].id, objeto, cumpleConAlgunoDeLosPermisos);
            if (!cumple)
                ApiControl.BloquearInput(editores[i]);
            else
                ApiControl.DesbloquearEditor(editores[i]);
        }
    }

    function TieneAlgunoDeLosPermisos(idControl: string, objeto: any, permisosSolicitados): boolean {
        let permisos = permisosSolicitados.split('|');
        let cumple: boolean = false;
        for (let j = 0; j < permisos.length; j++) {
            let ma = ObtenerPropiedad(objeto, literal.ModoDeAcceso, ModoAcceso.enumModoDeAccesoDeDatos.Consultor);
            let permiso = permisos[j];
            cumple = TieneElPermiso(idControl, objeto, permiso, ma);
            if (cumple)
                break;
        }
        return cumple;
    }

    function TieneElPermiso(idControl: string, objeto: any, permiso: string, modoDeAcceso: any): boolean {
        let cumple = false;
        switch (ParsearPermiso(permiso)) {
            case enumModoDeAccesoDeDatos.Administrador:
                if (ModoAcceso.EsAdministrador(modoDeAcceso))
                    cumple = true;
                break;
            case enumModoDeAccesoDeDatos.Interventor:
                if (ModoAcceso.EsInterventor(modoDeAcceso))
                    cumple = true;
                break;
            case enumModoDeAccesoDeDatos.Gestor:
                if (ModoAcceso.EsGestor(modoDeAcceso) || ModoAcceso.EsCreador(objeto))
                    cumple = true;
                break;
            case enumModoDeAccesoDeDatos.Creador:
                if (ModoAcceso.EsCreador(objeto))
                    cumple = true;
                break;
            default: MensajesSe.Info(`No se ha contemplado el ${permiso} para aplicarlo al control ${idControl}`);
                break;
        }
        return cumple;
    }

    export function AplicarloAlPanel(panel: HTMLDivElement, modoDeAcceso: enumModoDeAccesoDeDatos, estaDeBaja: boolean): void {
        if (estaDeBaja) modoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
        if (EsTrue(panel.getAttribute(atModal.soloConsulta)))
            modoDeAcceso = enumModoDeAccesoDeDatos.Consultor;

        AplicarloALosEditores(panel, modoDeAcceso);
        AplicarloALosRestrictores(panel);
        AplicarloAlasAreasDeTexto(panel, modoDeAcceso);
        AplicarloALasFechas(panel, modoDeAcceso);
        AplicarloALosChecks(panel, modoDeAcceso);
        AplicarloALasListaDeElementos(panel, modoDeAcceso);
        AplicarloALasListaDeValores(panel, modoDeAcceso);
        AplicarModoDeAccesoALasListasDinamicas(panel, modoDeAcceso);
        AplicarModoAccesoAlSelectorDeArchivos(panel, modoDeAcceso);
        AjustarOpcionesDeMenu(panel, modoDeAcceso);
        AjustarReferencias(panel, modoDeAcceso);
        AjustarReferenciasPost(panel, modoDeAcceso);
        AjustarSelectorDeImagenes(panel, modoDeAcceso);
        AjustarSelectorDeUrlDeImagenes(panel, modoDeAcceso);
        AjustarSelectorDeUnArchivo(panel, modoDeAcceso);
    }

    export function AplicarAlContenedorDeArchivos(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let visible: boolean = permisosDeUsuario === enumModoDeAccesoDeDatos.Gestor || permisosDeUsuario === enumModoDeAccesoDeDatos.Administrador;
        let botones: NodeListOf<HTMLButtonElement> = panel.querySelectorAll(`Button[class='${ltrCss.borrarAnexado}']`) as NodeListOf<HTMLButtonElement>;
        for (let i: number = 0; i < botones.length; i++) {
            botones[i].style.display = !visible ? ltrStyle.display.none : ltrStyle.display.block;
        }
        botones = panel.querySelectorAll(`Button[class='${ltrCss.firmarArchivo}']`) as NodeListOf<HTMLButtonElement>;
        for (let i: number = 0; i < botones.length; i++) {
            botones[i].style.display = !visible ? ltrStyle.display.none : ltrStyle.display.block;
        }
    }

    function AjustarSelectorDeUrlDeImagenes(panel: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`[${atControl.tipo}=${ltrTipoControl.UrlDeArchivo}]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < archivos.length; i++) {
            AplicarAlSelectorDeArchivos(archivos[i], modoDeAcceso);
        }
    }

    function AjustarSelectorDeUnArchivo(panel: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`[${atControl.tipo}=${ltrTipoControl.SelectorDeUnArchivo}]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < archivos.length; i++) {
            AplicarAlSelectorDeUnArchivo(archivos[i], modoDeAcceso);
        }
    }

    function AjustarSelectorDeImagenes(panel: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let archivos: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`[${atControl.tipo}=${ltrTipoControl.UrlDeArchivo}]`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < archivos.length; i++) {
            AplicarAlSelectorDeArchivos(archivos[i], modoDeAcceso);
        }
    }

    export function AplicarAlSelectorDeUnArchivo(control: HTMLInputElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
        let ref: HTMLAnchorElement = document.getElementById(`${control.id}.ref`) as HTMLAnchorElement;
        let tr: HTMLElement = ref;
        if (Definido(tr)) {
            while (Definido(tr.parentElement)) {
                tr = tr.parentElement;
                if (tr.classList.contains('tr-propiedad')) break;
            }
            if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                ref.style.display = ltrStyle.display.none;
                tr.style.display = ltrStyle.display.none;
            }
            else {
                ref.style.display = '';
                tr.style.display = '';
            }
        }
    }


    export function AplicarAlSelectorDeArchivos(control: HTMLInputElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
        let ref: HTMLAnchorElement = document.getElementById(`${control.id}.ref`) as HTMLAnchorElement;
        if (Definido(ref)) {
            let tr: HTMLTableRowElement = ref.parentNode.parentNode as HTMLTableRowElement;
            if (!ModoAcceso.EsGestor(modoDeAcceso)) {
                ref.style.display = ltrStyle.display.none;
                tr.style.display = ltrStyle.display.none;
            }
            else {
                ref.style.display = ltrStyle.display.block;
                tr.style.display = ltrStyle.display.block;
            }
        }
    }

    function AjustarReferencias(panel: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let referencias: NodeListOf<HTMLAnchorElement> = panel.querySelectorAll(`a[${atControl.tipo}='${ltrTipoControl.Referencia}']`) as NodeListOf<HTMLAnchorElement>;
        for (var i = 0; i < referencias.length; i++) {
            AjustarReferencia(referencias[i], modoDeAcceso);
        }
    }

    function AjustarReferencia(ref: HTMLAnchorElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
        if (!ModoAcceso.EsGestor(modoDeAcceso))
            ApiControl.OcultarHtmlAnchor(ref, EsTrue(ref.getAttribute(atRef.enConsultaOcultar)));
        //ref.style.display =  ? ltrStyle.display.none : ltrStyle.display.block;
        else
            ApiControl.OcultarHtmlAnchor(ref, false);
        //ref.style.display = ltrStyle.display.block;
    }

    function AjustarReferenciasPost(panel: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let referencias: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`a[${atControl.tipo}='${ltrTipoControl.ReferenciaPost}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < referencias.length; i++) {
            AjustarReferenciaPost(referencias[i], modoDeAcceso);
        }
    }

    function AjustarReferenciaPost(ref: HTMLInputElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos) {
        if (!ModoAcceso.EsGestor(modoDeAcceso))
            ref.style.display = EsTrue(ref.getAttribute(atRef.enConsultaOcultar)) ? ltrStyle.display.none : ltrStyle.display.block;
        else
            ref.style.display = ltrStyle.display.block;
    }

    export function AjustarOpcionesDeMenu(panel: HTMLDivElement, modoDeAcceso: ModoAcceso.enumModoDeAccesoDeDatos): void {
        let opcionesDeElemento: NodeListOf<HTMLButtonElement> = panel.querySelectorAll(`input[${atOpcionDeMenu.clase}="${enumCssOpcionMenu.DeElemento}"]`) as NodeListOf<HTMLButtonElement>;
        for (var i = 0; i < opcionesDeElemento.length; i++) {
            let opcion: HTMLButtonElement = opcionesDeElemento[i];
            let permisosNecesarios: string = opcion.getAttribute(atOpcionDeMenu.permisosNecesarios);
            //if (permisosNecesarios === ModoAcceso.ModoDeAccesoDeDatos.SerAdministrador && !Registro.EsAdministrador())
            //    opcion.disabled = false;
            //else
            opcion.disabled = !ModoAcceso.HayPermisos(ModoAcceso.Parsear(permisosNecesarios), modoDeAcceso);
        }
    }

    export function AplicarModoDeAccesoALasOpcionesDelNegocio(opcionesGenerales: NodeListOf<HTMLButtonElement>, modoDeAccesoDelUsuario: enumModoDeAccesoDeDatos): void {
        for (var i = 0; i < opcionesGenerales.length; i++) {
            let opcion: HTMLButtonElement = opcionesGenerales[i];

            if (ApiControl.EstaOculta(opcion))
                continue;

            let permisosNecesarios: string = opcion.getAttribute(atOpcionDeMenu.permisosNecesarios);
            ApiControl.BloquearDesbloquearOpcionDeMenu(opcion, !ModoAcceso.HayPermisos(ModoAcceso.Parsear(permisosNecesarios), modoDeAccesoDelUsuario));
        }
    }

    export function AplicarModoAccesoAlElemento(opcion: HTMLButtonElement, seleccionados: number, permisos: ModoAcceso.enumModoDeAccesoDeDatos) {

        let hayQueHabilitar: boolean = ApiControl.EstaBloqueada(opcion)
            ? HayQueHabilitar(seleccionados, opcion, permisos)
            : !HayQueDesHabilitar(seleccionados, opcion, permisos);

        ApiControl.BloquearDesbloquearOpcionDeMenu(opcion, !hayQueHabilitar);
    }

    export function HayQueHabilitar(seleccionados: number, opcion: HTMLElement, permisos: ModoAcceso.enumModoDeAccesoDeDatos): boolean {
        let hayMasDeUnaSeleccionada: boolean = seleccionados > 1;

        let permisosNecesarios: string = opcion.getAttribute(atOpcionDeMenu.permisosNecesarios);
        let permiteMultiSeleccion: string = opcion.getAttribute(atOpcionDeMenu.permiteMultiSeleccion);

        if (!EsTrue(permiteMultiSeleccion) && hayMasDeUnaSeleccionada) {
            return false;
        }

        if (EsTrue(permiteMultiSeleccion)) {
            let numeroMaximo: number = Numero(opcion.getAttribute(atOpcionDeMenu.numeroMaximoSeleccionable));
            if (numeroMaximo !== -1 && numeroMaximo < seleccionados) {
                return false;
            }
        }

        return ModoAcceso.HayPermisos(ModoAcceso.Parsear(permisosNecesarios), permisos);
    }

    export function HayQueDesHabilitar(seleccionados: number, opcion: HTMLElement, permisos: ModoAcceso.enumModoDeAccesoDeDatos): boolean {
        let hayMasDeUnaSeleccionada: boolean = seleccionados > 1;

        let permisosNecesarios: string = opcion.getAttribute(atOpcionDeMenu.permisosNecesarios);
        let permiteMultiSeleccion: string = opcion.getAttribute(atOpcionDeMenu.permiteMultiSeleccion);

        if (!EsTrue(permiteMultiSeleccion) && hayMasDeUnaSeleccionada) {
            return true;
        }
        //Si permite multiselección pero sobrepasa el límite, y la hay __> bloqueo
        if (EsTrue(permiteMultiSeleccion)) {
            let numeroMaximo: number = Numero(opcion.getAttribute(atOpcionDeMenu.numeroMaximoSeleccionable));
            if (numeroMaximo !== -1 && numeroMaximo < seleccionados) {
                return true;
            }
        }

        return !ModoAcceso.HayPermisos(ModoAcceso.Parsear(permisosNecesarios), permisos);
    }

    export function AplicarloALosChecks(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let checkes: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.Check}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < checkes.length; i++) {
            var control = checkes[i] as HTMLInputElement;
            AplicarDisable(control, permisosDeUsuario);
        }
    }

    function AplicarloALosEditores(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let editores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.Editor}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < editores.length; i++) {
            var control = editores[i] as HTMLInputElement;
            AplicarReadOnly(control, permisosDeUsuario);
        }
    }

    function AplicarloALosRestrictores(panel: HTMLDivElement) {
        let restrictores: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.restrictorDeEdicion}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < restrictores.length; i++) {
            var control = restrictores[i] as HTMLInputElement;
            control.readOnly = true;
        }
    }

    function AplicarloALasListaDeElementos(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}='${ltrTipoControl.ListaDeElementos}']`) as NodeListOf<HTMLSelectElement>;
        for (var i = 0; i < listas.length; i++) {
            var control = listas[i] as HTMLSelectElement;
            AplicarReadOnly(control, permisosDeUsuario);
        }
    }

    function AplicarloALasListaDeValores(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let listas: NodeListOf<HTMLSelectElement> = panel.querySelectorAll(`select[${atControl.tipo}='${ltrTipoControl.ListaDeValores}']`) as NodeListOf<HTMLSelectElement>;
        for (var i = 0; i < listas.length; i++) {
            var control = listas[i] as HTMLSelectElement;
            AplicarReadOnly(control, permisosDeUsuario);
        }
    }

    function AplicarModoDeAccesoALasListasDinamicas(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let listas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.ListaDinamica}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < listas.length; i++) {
            var control = listas[i] as HTMLInputElement;
            AplicarReadOnly(control, permisosDeUsuario);
        }
    }

    function AplicarloAlasAreasDeTexto(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let areas: NodeListOf<HTMLTextAreaElement> = panel.querySelectorAll(`textarea[${atControl.tipo}='${ltrTipoControl.AreaDeTexto}']`) as NodeListOf<HTMLTextAreaElement>;
        for (var i = 0; i < areas.length; i++) {
            var control = areas[i] as HTMLTextAreaElement;
            AplicarReadOnly(control, permisosDeUsuario);
        }
    }

    //export function AplicarModoAccesoAlSelectorDeArchivos(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
    //    let divSelectorDeArchivos: HTMLDivElement = panel.querySelector(`.${ltrCss.Archivos.SelectorDeArchivos}`);  

    //    if (NoDefinido(divSelectorDeArchivos))
    //        return;

    //    let visible: boolean = permisosDeUsuario === enumModoDeAccesoDeDatos.Gestor || permisosDeUsuario === enumModoDeAccesoDeDatos.Administrador;
    //    divSelectorDeArchivos.style.display = !visible ? ltrStyle.display.none : "grid";
    //}

    export function AplicarModoAccesoAlSelectorDeArchivos(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let divSelectorDeArchivos: HTMLDivElement = panel.querySelector(`.${ltrCss.Archivos.SelectorDeArchivos}`);

        if (NoDefinido(divSelectorDeArchivos))
            return;

        let visible: boolean = permisosDeUsuario === enumModoDeAccesoDeDatos.Gestor || permisosDeUsuario === enumModoDeAccesoDeDatos.Interventor || permisosDeUsuario === enumModoDeAccesoDeDatos.Administrador;

        if (!visible) {
            ApiControl.IncluirCss(divSelectorDeArchivos, ltrCss.Archivos.SelectorDeArchivosEnConsulta);
            //if (!divSelectorDeArchivos.classList.contains(ltrCss.Archivos.SelectorDeArchivosEnConsulta))
            //    divSelectorDeArchivos.classList.add(ltrCss.Archivos.SelectorDeArchivosEnConsulta);
        } else {
            ApiControl.ExcluirCss(divSelectorDeArchivos, ltrCss.Archivos.SelectorDeArchivosEnConsulta);
            //divSelectorDeArchivos.classList.remove(ltrCss.Archivos.SelectorDeArchivosEnConsulta);
        }
    }

    function AplicarloALasFechas(panel: HTMLDivElement, permisosDeUsuario: enumModoDeAccesoDeDatos) {
        let fechas: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.SelectorDeFecha}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechas.length; i++) {
            var control = fechas[i] as HTMLInputElement;
            AplicarReadOnly(control, permisosDeUsuario);
        }

        let fechaHora: NodeListOf<HTMLInputElement> = panel.querySelectorAll(`input[${atControl.tipo}='${ltrTipoControl.SelectorDeFechaHora}']`) as NodeListOf<HTMLInputElement>;
        for (var i = 0; i < fechaHora.length; i++)
            AplicarloALaFecha(fechaHora[i] as HTMLInputElement, permisosDeUsuario);

    }

    export function AplicarloALaFecha(fecha: HTMLInputElement, permisosDeUsuario: enumModoDeAccesoDeDatos): void {
        AplicarReadOnly(fecha, permisosDeUsuario);
        let idHora: string = fecha.getAttribute(atSelectorDeFecha.hora);
        let controlHora: HTMLInputElement = document.getElementById(idHora) as HTMLInputElement;
        AplicarReadOnly(controlHora, permisosDeUsuario);
    }

    function AplicarReadOnly(control: HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement, permiso: enumModoDeAccesoDeDatos): void {
        var editable = control.getAttribute(atControl.editable);
        if (control instanceof HTMLSelectElement) {
            if ((control as HTMLSelectElement).disabled || EsTrue((control as HTMLSelectElement).getAttribute(atListas.ReadOnly)))
                return;
            if (EsTrue(editable)) {
                (control as HTMLSelectElement).disabled = !HayPermisos(enumModoDeAccesoDeDatos.Gestor, permiso);
                (control as HTMLSelectElement).setAttribute(atListas.ReadOnly, !HayPermisos(enumModoDeAccesoDeDatos.Gestor, permiso) ? "true" : "false");
            }
            else {
                (control as HTMLSelectElement).disabled = true;
                (control as HTMLSelectElement).setAttribute(atListas.ReadOnly, "true");
            }
        }
        else if (control instanceof HTMLInputElement) {
            {
                if ((control as HTMLInputElement).disabled || (control as HTMLInputElement).readOnly)
                    return;
                if (EsTrue(editable)) {
                    control.readOnly = !HayPermisos(enumModoDeAccesoDeDatos.Gestor, permiso);
                }
                else {
                    control.readOnly = true;
                }
            }
        }
        else {
            if (EsTrue(editable)) {
                control.readOnly = !HayPermisos(enumModoDeAccesoDeDatos.Gestor, permiso);
            }
            else {
                control.readOnly = true;
            }
        }
    }


    function AplicarDisable(control: HTMLInputElement, permiso: enumModoDeAccesoDeDatos): void {
        var editable = control.getAttribute(atControl.editable);
        if (EsTrue(editable))
            control.disabled = !HayPermisos(enumModoDeAccesoDeDatos.Gestor, permiso);
        else
            control.disabled = true;
    }

}