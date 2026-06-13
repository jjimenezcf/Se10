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

    export function Agenda_EnviarEventoIcs(parametros: string): void {
        let partes = parametros.split(';');
        let idGrid: string = partes[0];
        let fila: number = Numero(partes[1]);
        let idEvento: number = Numero(ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila(idGrid, fila, literal.id));
        let idNegocio: number = Numero(ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila(idGrid, fila, ltrPropiedades.Negocio.idNegocio));
        let nombreEvento: string = ApiDeGrid.Expansor_ObtenerPropiedadDeLaFila(idGrid, fila, literal.nombre);

        // Crear modal dinámica
        const idModal = 'modal-enviar-ics';
        let modalExistente = document.getElementById(idModal);
        if (modalExistente) modalExistente.remove();

        let overlay = document.createElement('div');
        overlay.id = idModal;
        overlay.style.cssText = 'position:fixed;top:0;left:0;width:100%;height:100%;background:rgba(0,0,0,0.5);z-index:9999;display:flex;align-items:center;justify-content:center;';

        let dialogo = document.createElement('div');
        dialogo.style.cssText = 'background:#fff;border-radius:8px;padding:24px;min-width:380px;box-shadow:0 4px 20px rgba(0,0,0,0.3);';

        let titulo = document.createElement('h5');
        titulo.innerText = `Enviar evento: ${nombreEvento}`;
        titulo.style.marginBottom = '16px';

        let label = document.createElement('label');
        label.innerText = 'Correo electrónico:';
        label.style.display = 'block';
        label.style.marginBottom = '6px';

        let inputCorreo = document.createElement('input');
        inputCorreo.type = 'email';
        inputCorreo.placeholder = 'destino@ejemplo.com';
        inputCorreo.style.cssText = 'width:100%;padding:8px;border:1px solid #ccc;border-radius:4px;margin-bottom:16px;box-sizing:border-box;';

        let contenedorBotones = document.createElement('div');
        contenedorBotones.style.cssText = 'display:flex;justify-content:flex-end;gap:10px;';

        let btnCerrar = document.createElement('button');
        btnCerrar.innerText = 'Cerrar';
        btnCerrar.className = ltrCss.Modal.BotonSecundario; 
        btnCerrar.onclick = () => overlay.remove();

        let btnEnviar = document.createElement('button');
        btnEnviar.innerText = 'Enviar';
        btnEnviar.className = ltrCss.Modal.BotonSecundario; 
        btnEnviar.onclick = () => {
            let correos = inputCorreo.value.trim();
            if (IsNullOrEmpty(correos)) {
                MensajesSe.Error('Validación', 'Debe indicar un correo electrónico');
                return;
            }
            overlay.remove();
            let parametros: Array<Parametro> = new Array<Parametro>();
            parametros.push(new Parametro(literal.id, idEvento));
            parametros.push(new Parametro(ltrPropiedades.Negocio.idNegocio, idNegocio));
            parametros.push(new Parametro(ltrPropiedades.Entorno.EventoDeAgenda.Correos, correos));
            ApiDePeticiones.EjecutarPeticion(
                null,
                ltrControladores.Entorno.VisorDeAgenda,
                Ajax.EndPoint.Entorno.Agenda.EnviarEventoIcs,
                parametros,
                new Array<Parametro>()
            )
            .then((peticion) => {
                MensajesSe.Info(peticion.resultado.consola);
            })
            .catch((peticion) => ApiDePeticiones.EmitirError(peticion));
        };

        contenedorBotones.appendChild(btnCerrar);
        contenedorBotones.appendChild(btnEnviar);
        dialogo.appendChild(titulo);
        dialogo.appendChild(label);
        dialogo.appendChild(inputCorreo);
        dialogo.appendChild(contenedorBotones);
        overlay.appendChild(dialogo);
        document.body.appendChild(overlay);

        setTimeout(() => inputCorreo.focus(), 100);
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