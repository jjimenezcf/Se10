namespace ApiDeAgenda {

    export function Agenda_AbrirAgenda(parametros: string) {
        let partes = parametros.split(';');
        let idGrid: string = partes[0];
        let fila: number = Numero(partes[1]);
        let valor: number = Numero(ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila(idGrid, fila, ltrParametrosUrl.idAgenda));
        let fecha: string = ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila(idGrid, fila, ltrPropiedades.Entorno.EventoDeAgenda.inicio).substring(0, 10);
        let url = `${window.location.origin}/${ltrUrls.Entorno.VisorDeAgenda}?${ltrParametrosUrl.guid}=${generarUUID()}&${ltrParametrosUrl.idAgenda}=${valor}&${ltrParametrosUrl.fecha}=${fecha}`;
        EntornoSe.AbrirPestana(url);
    }

    export function Agenda_EjecutarAccionAsociada(numeroDeFila: number) {
        let id = Numero(ApiDeGrid.ObtenerValorDeLaFilaParaLaPropiedad(Crud.crudMnt.Tabla, numeroDeFila, literal.id));
        let inicio = ObtenerPropiedad(Crud.crudMnt.DatosDelGrid.ObtenerPorId(id).Registro, ltrPropiedades.Entorno.EventoDeAgenda.inicio);
        let idAgenda = ObtenerPropiedad(Crud.crudMnt.DatosDelGrid.ObtenerPorId(id).Registro, ltrPropiedades.Entorno.EventoDeAgenda.IdAgenda);
        let url = `${window.location.origin}/${ltrUrls.Entorno.VisorDeAgenda}?${ltrParametrosUrl.guid}=${generarUUID()}&${ltrParametrosUrl.idAgenda}=${idAgenda}&${ltrParametrosUrl.fecha}=${inicio}`;
        EntornoSe.AbrirPestana(url);
    }

    export function InicializarModalDeCreacion(idPanel: string): void {
        let panel = document.getElementById(idPanel) as HTMLDivElement;
        let inicio = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.EventoDeAgenda.inicio, true) as HTMLInputElement;
        let fin = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.EventoDeAgenda.fin, true) as HTMLInputElement;

        ApiDeInicializacion.InicializarFecha(inicio);
        ApiDeInicializacion.InicializarFecha(fin);
        var ahora = new Date(Date.now());
        MapearAlControl.MapearSelectorDeFecha(inicio as HTMLInputElement, ahora.toString());

        var fechaPorDefecto = fin.getAttribute(atControl.valorPorDefecto);
        
        if (Definido(fechaPorDefecto)) {
            if (EsFechaValida(CrearFecha(fechaPorDefecto)))
                MapearAlControl.MapearSelectorDeFecha(fin as HTMLInputElement, fechaPorDefecto);
        }
        else {
            ahora = new Date(ahora.setHours(ahora.getHours() + 1))
            MapearAlControl.MapearSelectorDeFecha(fin as HTMLInputElement, ahora.toString());
        }
        let agenda = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.EventoDeAgenda.Agenda, true) as HTMLInputElement;

        var proponerAgendaDelObjeto = typeof Crud !== 'undefined' && (Crud.crudMnt.EnumeradoDeNegocio === enumNegocio.Infante ||
            Crud.crudMnt.EnumeradoDeNegocio === enumNegocio.CursoDeGuarderia)

        proponerAgendaDelObjeto = proponerAgendaDelObjeto &&
            ((Crud.crudMnt.EstoyEditando && Numero(ObtenerPropiedad(Crud.crudMnt.crudDeEdicion.Registro, ltrPropiedades.Entorno.EventoDeAgenda.IdAgenda)) > 0) ||
             (Crud.crudMnt.ModoTrabajo === enumModoTrabajo.mantenimiento &&
            Crud.crudMnt.InfoSelector.Cantidad === 1 &&
            Numero(ObtenerPropiedad(Crud.crudMnt.InfoSelector.LeerElemento(0).Registro, ltrPropiedades.Entorno.EventoDeAgenda.IdAgenda)) > 0
             )
            )

        if (proponerAgendaDelObjeto) {
            var registro = Crud.crudMnt.ModoTrabajo === enumModoTrabajo.mantenimiento ? Crud.crudMnt.InfoSelector.LeerElemento(0).Registro : Crud.crudMnt.crudDeEdicion.Registro;
            MapearAlControl.ListaDinamica(agenda,
                ObtenerPropiedad(registro, ltrPropiedades.Entorno.EventoDeAgenda.IdAgenda),
                ObtenerPropiedad(registro, ltrPropiedades.Entorno.EventoDeAgenda.Agenda), false);
        }
        else {
            MapearAlControl.ListaDinamica(agenda, Registro.UsuarioConectado().idAgenda, Registro.UsuarioConectado().NombreAgenda, false);
        }

        (ApiControl.BuscarControl(panel, literal.nombre, true) as HTMLInputElement).focus();
    }

    export function InicializarModalDeEdicion(idPanel: string): void {
        let panel = document.getElementById(idPanel) as HTMLDivElement;
        let eventoDeDia = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.EventoDeAgenda.eventoDeDia, true) as HTMLInputElement;
        let esDelSistema = ApiControl.BuscarControl(panel, ltrPropiedades.Entorno.EventoDeAgenda.EsDelSistema, true) as HTMLInputElement;
        if (esDelSistema.checked)
            return;
        Agenda_AlCambiar_EventoDeDia(eventoDeDia);        
    }

    export function Agenda_AlCambiar_EventoDeDia(check: HTMLInputElement) {
        let idTabla = check.id.replace(`-${check.getAttribute(atControl.propiedad)}`, '');
        let tabla = document.getElementById(idTabla);
        let horas: NodeListOf<HTMLInputElement> = tabla.querySelectorAll(`input[${atControl.class}='${ltrCss.controlesDto.hora}']`) as NodeListOf<HTMLInputElement>;
        for (let i = 0; i < horas.length; i++) {
            let editable = horas[i].getAttribute(atControl.editable);
            //si está bloqueado es por ser del sistema por tanto abandono
            if ((horas[i] as HTMLInputElement).readOnly && !EsTrue(editable))
                return;

            if (check.checked) {
                horas[i].value = '';
                ApiControl.BloquearInput(horas[i]);
            }
            else
                ApiControl.DesbloquearEditor(horas[i]);
        }
    }
}