

    //EntornoSe.MapearUsuarioDeConexion(this.crudDeCreacion.PanelDeCrear)
    //    .then((resultado) => {
    //        let panel: HTMLDivElement = resultado['panel'];
    //        let usuarioConectado = resultado['usuario'];
    //        let idUsuario: number = usuarioConectado['id'] as number;
    //        let usuario: string = usuarioConectado['login'] as string;
    //        ApiControl.MapearPropiedadRestrictoraAlControl(panel, idsometedor, idUsuario, usuario);
    //    })
    //    .catch(() => usuarioNoLeido(this.crudDeCreacion));

    //export function MapearUsuarioDeConexion(panel: HTMLDivElement): Promise<any> {

    //    function RegistrarCookie(peticion: ApiDeAjax.DescriptorAjax) {
    //        let registro: any = peticion.resultado.datos;
    //        Cookies.Guardar(Cookies.Cooky.UsuarioConectado, registro);
    //    }

    //    let usuarioConectado = Registro.UsuarioConectado();

    //    return new Promise((resolve, reject) => {

    //        let url: string = `/${Ajax.Usuarios.ruta}/${Ajax.Usuarios.accion.LeerUsuarioDeConexion}`;

    //        let a = new ApiDeAjax.DescriptorAjax(panel
    //            , Ajax.Usuarios.accion.LeerUsuarioDeConexion
    //            , panel
    //            , url
    //            , ApiDeAjax.TipoPeticion.Asincrona
    //            , ApiDeAjax.ModoPeticion.Get
    //            , (peticion) => {
    //                RegistrarCookie(peticion);
    //                var resultado = { panel: panel, usuario: Cookies.LeerCookie(Cookies.Cooky.UsuarioConectado)}
    //                resolve(resultado);
    //            }
    //            , () => {
    //                reject();
    //            }
    //        );
    //        if (usuarioConectado !== null)
    //            resolve({panel: panel, usuario: usuarioConectado});
    //        else
    //          a.Ejecutar();
    //    });
    //}