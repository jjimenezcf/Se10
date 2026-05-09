
namespace ApiDePeticiones {

    export class DatosPeticionSubirArchivo {
        private _idArchivo: string;
        public NombreArchivo: string;

        public Archivo(): HTMLInputElement {
            return document.getElementById(this._idArchivo) as HTMLInputElement;
        }

        constructor(idArchivo: string, nombre: string) {
            this._idArchivo = idArchivo;
            this.NombreArchivo = nombre;
        }
    }


    export function SolicitarArbolDelMenu(llamador: any, idContenedorMenu: string, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Entorno.ArbolMenu.controlador}/${Ajax.Entorno.ArbolMenu.accion}?${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;
            let datosEntrada: any = { "idContenedorMenu": idContenedorMenu };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Entorno.ArbolMenu.accion
                , datosEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar();
        });
    }

    export function Fichar(llamador: any, latitud?: number, longitud?: number): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {
            // Añadimos los parámetros a la URL
            let url: string = `/${Ajax.SisDoc.CircuitosDoc.controlador}/${Ajax.SisDoc.CircuitosDoc.Fichar}?latitud=${latitud ?? ""}&longitud=${longitud ?? ""}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Entorno.MiCorreo.PeticionDeAcceso
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => resolve(peticion)
                , (peticion) => reject(peticion)
            );
            a.Ejecutar();
        });
    }


    export function ValidarExisteDeclaracionResponsable(llamador: any): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Ventas.Facturas.controlador}/${Ajax.Ventas.Facturas.accion.ValidarExisteDeclaracionResponsable}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Entorno.MiCorreo.PeticionDeAcceso
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar();
        });
    }
    export function PedirAccesoAlCorreo(llamador: any): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Entorno.MiCorreo.controlador}/${Ajax.Entorno.MiCorreo.PeticionDeAcceso}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Entorno.MiCorreo.PeticionDeAcceso
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar();
        });
    }
    export function SolicitarModal(llamador: any, idContenedor: string, modal: string, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Modal.controlador}/${Ajax.Modal.accion}?${Ajax.Modal.parametro}=${modal}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;
            let datosEntrada: any = { "idContenedor": idContenedor };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Entorno.ArbolMenu.accion
                , datosEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar();
        });
    }

    export function LeerDatosParaInicializarVista(llamador: any, controlador: string, negocio: string, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${Ajax.EndPoint.LeerDatosParaInicializarVista}`;
            url = url + `?${Ajax.Param.nombreDeNegocio}=${negocio}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;
            let datosEntrada: any = { "cotrolador": controlador, "negocio": negocio };
            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.LeerDatosParaInicializarVista
                , datosEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar();
        });
    }


    export function AnexarArchivo(llamador: any, negocio: string, idElemento, fichero: File, infoArchivo: HTMLInputElement, dondeMostrar: HTMLDivElement): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.AnexarArchivo}`;
            let datosEntrada: any = {
                "idElemento": idElemento
                , "negocio": negocio
                , "fichero": fichero.name
                , "infoArchivo": infoArchivo
                , "dondeMostrar": dondeMostrar
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.AnexarArchivo
                , datosEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            let datosPost = new FormData();
            datosPost.append(Ajax.Param.nombreDeNegocio, negocio);
            datosPost.append(Ajax.Param.idElemento, idElemento);
            datosPost.append(Ajax.Param.fichero, fichero);
            a.DatosPost = datosPost;
            if (infoArchivo) infoArchivo.setAttribute('estado', atArchivo.situacion.subiendo);
            a.Ejecutar();
        });
    }

    export function DescargarAdjuntoDeMail(llamador: any, idMail: string, idAdjunto: string, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Entorno.MiCorreo.controlador}/${Ajax.EndPoint.Entorno.MiCorreo.DescargarAdjunto}?${Ajax.Param.idMail}=${idMail}&${Ajax.Param.idAdjunto}=${idAdjunto}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.Entorno.MiCorreo.DescargarAdjunto
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }


    export function BorrarRelacionPorId(llamador: any, controlador: string, id: number, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>, accionDeBorrado: string = undefined): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            if (!Definido(accionDeBorrado)) accionDeBorrado = Ajax.EndPoint.BorrarRelacion;

            let url: string = `/${controlador}/${accionDeBorrado}?${Ajax.Param.id}=${id}&${Ajax.Param.parametros}=${JSON.stringify(parametros)}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accionDeBorrado
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function BorrarPorIds(llamador: any, controlador: string, ids: Array<number>, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${Ajax.EndPoint.BorrarPorId}`
                + `?${Ajax.Param.idsJson}=${JSON.stringify(ids)}`
                + `&${Ajax.Param.parametros}=${JSON.stringify(parametros)}`;

            let datosDeEntrada: Array<any> = [];
            datosDeEntrada.push(ids);
            datosDeEntrada.push(parametros);

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.BorrarPorId
                , JSON.stringify(datosDeEntrada)
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }


    export function LeerAnexados(contenedor: HTMLDivElement, negocio: string, idElemento: number, posicion: number, cantidad: number, guid: any): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            const accion = (typeof Crud !== 'undefined' && Crud.Consultor) ? Ajax.Archivos.accion.LeerAnexadosPorGuid : Ajax.Archivos.accion.LeerAnexados;

            let url: string = `/${Ajax.Archivos.controlador}/${accion}` + '?'
                + `negocio=${negocio}&`
                + `idElemento=${idElemento}&`
                + `posicion=${posicion}&`
                + `cantidad=${cantidad}&`
                + `guid=${guid}`;

            let datosDeEntrada: any = {
                "idElemento": idElemento
                , "negocio": negocio
            };

            let a = new ApiDeAjax.DescriptorAjax(contenedor
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
                , false
            );

            a.Ejecutar();
        });
    }


    export function QuitarAnexado(llamador: any, ctdArchivosAnexados: HTMLDivElement, idVisorDelArchivo: string, negocio: string, idElemento: number, idArchivo: number): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.QuitarAnexado}` + '?'
                + `negocio=${negocio}&`
                + `idElemento=${idElemento}&`
                + `idArchivo=${idArchivo}`;

            let datosDeEntrada: any = {
                "idElemento": idElemento
                , "negocio": negocio
                , "ctdArchivosAnexados": ctdArchivosAnexados
                , "idVisorDelArchivo": idVisorDelArchivo
                , "idArchivo": idArchivo
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.QuitarAnexado
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function CambiarPassword(llamador: any, actual: string, nueva: string, repetida: string): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let idUsuario: number = Registro.UsuarioConectado().id;

            if (idUsuario === 0) MensajesSe.EmitirExcepcion("CambiarPassword", "El usuario debe estar definido");

            let url: string = `/${Ajax.Usuarios.controlador}/${Ajax.Usuarios.accion.CambiarPassword}` + '?'
                + `idUsuario=${idUsuario}&`
                + `actual=${Encriptar(literal.ClaveDeEncriptacion, actual)}&`
                + `nueva=${Encriptar(literal.ClaveDeEncriptacion, nueva)}&`
                + `repetida=${Encriptar(literal.ClaveDeEncriptacion, repetida)}`;

            let datosDeEntrada: any = {
                "idElemento": idUsuario
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Usuarios.accion.CambiarPassword
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function SubirMiCertificado(llamador: any, idArchivo: number, password: string): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let idUsuario: number = Registro.UsuarioConectado().id;

            if (idUsuario === 0) MensajesSe.EmitirExcepcion("SubirMiCertificado", "El usuario debe estar definido");

            let url: string = `/${Ajax.Usuarios.controlador}/${Ajax.Usuarios.accion.SubirMiCertificado}` + '?'
                + `idUsuario=${idUsuario}&`
                + `idArchivo=${idArchivo}&`
                + `password=${password}`;

            let datosDeEntrada: any = {
                "idElemento": idUsuario
                , "idArchivo": idArchivo
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Usuarios.accion.SubirMiCertificado
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function RegistrarDescargaConGuid(llamador: any, idArchivo: number, caducaEl: Date): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {
            const caducaElFormateado = caducaEl && !isNaN(caducaEl.getTime())
                ? caducaEl.toISOString()
                : 'null';

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.RegistrarDescargaConGuid}?` +
                `${literal.id}=${Encriptar(literal.ClaveDeEncriptacion, idArchivo)}&` +
                `${ltrPropiedades.SisDoc.Archivo.CaducaEl}=${Encriptar(literal.ClaveDeEncriptacion, caducaElFormateado)}`;

            let datosDeEntrada: any = {
                "idArchivo": idArchivo,
                "caducaEl": caducaElFormateado !== 'null' ? caducaElFormateado : null
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.RegistrarDescargaConGuid
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }
    export function RegistrarConsultaConGuid(llamador: any, controlador: string, idElemento: number, caducaEl: Date): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {
            //const caducaElFormateado = caducaEl && !isNaN(caducaEl.getTime())
            //    ? caducaEl.toISOString()
            //    : 'null';

            const caducaElFormateado = caducaEl && !isNaN(caducaEl.getTime())
                ? `${caducaEl.getFullYear()}-${(caducaEl.getMonth() + 1).toString().padStart(2, '0')}-${caducaEl.getDate().toString().padStart(2, '0')}T${caducaEl.getHours().toString().padStart(2, '0')}:${caducaEl.getMinutes().toString().padStart(2, '0')}:${caducaEl.getSeconds().toString().padStart(2, '0')}`
                : 'null';

            let url: string = `/${controlador}/${Ajax.Entorno.Acceso.RegistrarConsultaConGuid}?` +
                `${literal.id}=${Encriptar(literal.ClaveDeEncriptacion, idElemento)}&` +
                `${ltrPropiedades.SisDoc.Archivo.CaducaEl}=${Encriptar(literal.ClaveDeEncriptacion, caducaElFormateado)}`;

            let datosDeEntrada: any = {
                "idElemento": idElemento,
                "caducaEl": caducaElFormateado !== 'null' ? caducaElFormateado : null
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Entorno.Acceso.RegistrarConsultaConGuid
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function FirmarArchivo(llamador: any, idNegocio: number, idElemento: number, idArchivo: number, idCertificado: number, modoDeAcceso: string, password: string): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.FirmarArchivo}` + '?'
                + `idNegocio=${idNegocio}&`
                + `idElemento=${idElemento}&`
                + `idArchivo=${idArchivo}&`
                + `idCertificado=${idCertificado}&`
                + `password=${password}`;

            let datosDeEntrada: any = {
                "idElemento": idElemento
                , "idNegocio": idNegocio
                , "idArchivo": idArchivo
                , "modoDeAcceso": modoDeAcceso
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.FirmarArchivo
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function AnularFirma(llamador: any, idNegocio: number, idElemento: number, idArchivo: number): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.AnularFirma}` + '?'
                + `idNegocio=${idNegocio}&`
                + `idElemento=${idElemento}&`
                + `idArchivo=${idArchivo}`;

            let datosDeEntrada: any = {
                "idNegocio": idNegocio,
                "idElemento": idElemento,
                "idArchivo": idArchivo
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.FirmarArchivo
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function BloquearArchivo(llamador: any, accion: string, idNegocio: number, idElemento: number, idArchivo: number, motivo: string): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${accion}` + '?'
                + `idNegocio=${idNegocio}&`
                + `idElemento=${idElemento}&`
                + `idArchivo=${idArchivo}&`
                + `motivo=${motivo}`;

            let datosDeEntrada: any = {
                "idNegocio": idNegocio,
                "idElemento": idElemento,
                "idArchivo": idArchivo,
                "motivo": motivo
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function ProcesarBloquearArchivos(llamador: any, idNegocio: number, idOrigen: number, motivo: string, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.BloquearArchivos}` + '?'
                + `idNegocio=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}&`
                + `idOrigen=${Encriptar(literal.ClaveDeEncriptacion, idOrigen)}&`
                + `motivo=${Encriptar(literal.ClaveDeEncriptacion, motivo)}`;

            let datosDeEntrada: any = {
                "motivo": motivo,
                "idNegocio": idNegocio,
                "idOrigen": idOrigen,
                "parametros": parametros
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.BloquearArchivos
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function ProcesarDesbloquearArchivos(llamador: any, idNegocio: number, idOrigen: number, motivo: string, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.DesbloquearArchivos}` + '?'
                + `idNegocio=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}&`
                + `idOrigen=${Encriptar(literal.ClaveDeEncriptacion, idOrigen)}&`
                + `motivo=${Encriptar(literal.ClaveDeEncriptacion, motivo)}`;

            let datosDeEntrada: any = {
                "motivo": motivo,
                "idNegocio": idNegocio,
                "idOrigen": idOrigen,
                "parametros": parametros
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.DesbloquearArchivos
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }
    export function ProcesarGenerarZip(llamador: any, idNegocio: number, idOrigen: number, nombreArchivador: string, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.GenerarZip}` + '?'
                + `idNegocio=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}&`
                + `idOrigen=${Encriptar(literal.ClaveDeEncriptacion, idOrigen)}&`
                + `nombre=${Encriptar(literal.ClaveDeEncriptacion, nombreArchivador)}`;

            let datosDeEntrada: any = {
                "nombre": nombreArchivador,
                "idNegocio": idNegocio,
                "idOrigen": idOrigen,
                "parametros": parametros
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.GenerarZip
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function ProcesarArchivos(llamador: any, operacion: string, idNegocio: number, idOrigen: number, idDestino: number, enumNegocioDestino: enumNegocio, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.ProcesarArchivos}` + '?'
                + `operacion=${Encriptar(literal.ClaveDeEncriptacion, operacion)}&`
                + `idNegocio=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}&`
                + `idOrigen=${Encriptar(literal.ClaveDeEncriptacion, idOrigen)}&`
                + `idDestino=${Encriptar(literal.ClaveDeEncriptacion, idDestino)}&`
                + `enumNegocioDestino=${Encriptar(literal.ClaveDeEncriptacion, enumNegocioDestino)}`;

            let datosDeEntrada: any = {
                "operacion": operacion,
                "idNegocio": idNegocio,
                "idOrigen": idOrigen,
                "idDestino": idDestino,
                "parametros": parametros
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.ProcesarArchivos
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }


    export function OperacionConArchivos(llamador: any, operacion: string, idNegocioOrigen: number, idElementoOrigen: number, idNegocioDestino: number, idElementoDestino: number, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.OperacionConArchivos}` + '?'
                + `operacion=${Encriptar(literal.ClaveDeEncriptacion, operacion)}&`
                + `idNegocioOrigen=${Encriptar(literal.ClaveDeEncriptacion, idNegocioOrigen)}&`
                + `idElementoOrigen=${Encriptar(literal.ClaveDeEncriptacion, idElementoOrigen)}&`
                + `idNegocioDestino=${Encriptar(literal.ClaveDeEncriptacion, idNegocioDestino)}&`
                + `idElementoDestino=${Encriptar(literal.ClaveDeEncriptacion, idElementoDestino)}`;

            let datosDeEntrada: any = {
                "operacion": operacion,
                "idNegocioOrigen": idNegocioOrigen,
                "idElementoOrigen": idElementoOrigen,
                "idNegocioDestino": idNegocioDestino,
                "idElementoDestino": idElementoDestino,
                "parametros": parametros
            };

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.OperacionConArchivos
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function Imputar(llamador: any, controlador: string, idNegocio: number, propiedadRestrictora: string, idRestrictor: number, ids: Array<number>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let parametros: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}` +
                `&${Ajax.Param.propiedadId}=${Encriptar(literal.ClaveDeEncriptacion, propiedadRestrictora)}` +
                `&${Ajax.Param.idDondeImputar}=${Encriptar(literal.ClaveDeEncriptacion, idRestrictor)}` +
                `&${Ajax.Param.idsJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(ids))}`;

            let url: string = `/${controlador}/${Ajax.EndPoint.Imputar}` + '?' + parametros;
            let datosDeEntrada: Array<any> = [];
            datosDeEntrada.push(propiedadRestrictora);
            datosDeEntrada.push(idRestrictor);
            datosDeEntrada.push(ids);

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.Imputar
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }


    export function CrearRelaciones(llamador: any, controlador: string, propiedadRestrictora: string, idRestrictor: number, ids: Array<number>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let parametros: string = `${Ajax.Param.propiedadId}=${propiedadRestrictora}` +
                `&${Ajax.Param.id}=${idRestrictor}` +
                `&${Ajax.Param.idsJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(ids))}`;

            let url: string = `/${controlador}/${Ajax.EndPoint.CrearRelaciones}` + '?' + parametros;
            let datosDeEntrada: Array<any> = [];
            datosDeEntrada.push(propiedadRestrictora);
            datosDeEntrada.push(idRestrictor);
            datosDeEntrada.push(ids);

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.CrearRelaciones
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function EjecutarAccion(llamador: any, controlador: string, accion: string, id: number, parametros: Array<Parametro>, datosDeEntrada: any): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {
            let url: string = `/${controlador}/${accion}?${Ajax.Param.id}=${id}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;
            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar();
        });
    }

    export function LeerElementoPorId(llamador: any, controlador: string, id: number, parametros: Array<Parametro>, datosDeEntrada: any, accion: string = undefined): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            const esConsultaConGuid: boolean = parametros.find(x => x.parametro === Ajax.Parametros.ConsultarConGuid && x.valor) != undefined;

            if (!Definido(accion)) accion = esConsultaConGuid ? Ajax.EndPoint.LeerPorIdPorGuid : Ajax.EndPoint.LeerPorId;

            let url: string = `/${controlador}/${accion}?${Ajax.Param.id}=${id}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }


    export function LeerElementoPorNombre(llamador: Crud.CrudMnt, controlador: string, nombre: string, errorSiNoHay: boolean = true, errorSiHayMasDeUno: boolean = true, soloActivos: boolean = true, datosDeCreacion = undefined): Promise<ApiDeAjax.DescriptorAjax> {
        return ApiDePeticiones.LeerElementoPorAk(llamador, controlador, ltrPropiedades.Elemento.Nombre, nombre, errorSiNoHay, errorSiHayMasDeUno, soloActivos, datosDeCreacion);
    }

    export function LeerElementoPorAk(llamador: Crud.CrudMnt, controlador: string, ak: string, valor: string, errorSiNoHay: boolean = true, errorSiHayMasDeUno: boolean = true, soloActivos: boolean = true, datosDeCreacion = undefined): Promise<ApiDeAjax.DescriptorAjax> {
        let filtros = new Array<ClausulaDeFiltrado>();
        filtros.push(new ClausulaDeFiltrado(ak, literal.filtro.criterio.igual, valor));
        let parametros = new Parametros();
        parametros.push(new Parametro(Ajax.Parametros.SoloActivos, soloActivos));
        parametros.push(new Parametro(Ajax.Parametros.ErrorSiNoHay, errorSiNoHay));
        parametros.push(new Parametro(Ajax.Parametros.ErrorSiHayMasDeUno, errorSiHayMasDeUno));

        if (datosDeCreacion) {
            parametros.Copiar(datosDeCreacion);
            parametros.add(Ajax.Parametros.ErrorSiNoHay, false);
        }
        return ApiDePeticiones.LeerElemento(llamador, controlador, filtros, parametros.Parametros);
    }

    export function LeerDatosDeFirma(llamador: any, negocio: string, idDelElemento: number, idArchivo: number, datosDeEntrada: any): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.LeerDatosDeFirma}`;
            url = `${url}?${Ajax.Param.nombreDeNegocio}=${negocio}`;
            url = `${url}&${Ajax.Param.idElemento}=${idDelElemento}`;
            url = `${url}&${Ajax.Archivos.parametro.idArchivo}=${idArchivo}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.LeerCertificados
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function LeerCertificadosParaFirmarElArchivo(llamador: any, negocio: string, idDelElemento: number, idArchivo: number, datosDeEntrada: any): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.LeerCertificados}`;
            url = `${url}?${Ajax.Param.nombreDeNegocio}=${negocio}`;
            url = `${url}&${Ajax.Param.idElemento}=${idDelElemento}`;
            url = `${url}&${Ajax.Archivos.parametro.idArchivo}=${idArchivo}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.Archivos.accion.LeerCertificados
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function LeerAmpliacion(llamador: any, controlador: string, negocio: number | string, idDelElemento: number, parametros: Array<Parametro>, datosDeEntrada: any): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = Numero(negocio) > 0
                ? `/${controlador}/${Ajax.EndPoint.LeerAmpliacionPorIdNegocio}?${Ajax.Param.idNegocio}=${negocio}&${Ajax.Param.idElemento}=${idDelElemento}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`
                : `/${controlador}/${Ajax.EndPoint.LeerAmpliacionPorNegocio}?${Ajax.Param.nombreDeNegocio}=${negocio}&${Ajax.Param.idElemento}=${idDelElemento}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Numero(negocio) > 0 ? Ajax.EndPoint.LeerAmpliacionPorIdNegocio : Ajax.EndPoint.LeerAmpliacionPorNegocio
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function LeerElemento(llamador: any, controlador: string, filtros: Array<ClausulaDeFiltrado>, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${Ajax.EndPoint.LeerElemento}?${Ajax.Param.filtros}=${JSON.stringify(filtros)}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.LeerElemento
                , null
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function LeerVinculos(llamador: any, controlador: string, idNegocio: number, idVinculado: number, idElemento1: number, parametros: Array<Parametro>, datosDeEntrada: any, conCapa: boolean = true)
        : Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let accion: string = Ajax.EndPoint.LeerVinculosCon;
            const estaElModuloCargado = typeof Crud !== 'undefined';
            if (estaElModuloCargado && Definido(Crud.Consultor)) {
                const yaExisteGuid = parametros.some(p => p.parametro === Ajax.Param.guid);
                if (!yaExisteGuid)
                    parametros.push(new Parametro(Ajax.Param.guid, Crud.Consultor.GuidDeConsulta));
                parametros.push(new Parametro(Ajax.Parametros.ConsultarConGuid, Crud.Consultor.PaginaDeConsultaConGuid));
                parametros.push(new Parametro(Ajax.Param.id, Crud.Consultor.RegistroId));
                accion = Ajax.EndPoint.LeerVinculosConPorGuid;
            }

            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.idVinculado}=${Encriptar(literal.ClaveDeEncriptacion, idVinculado)}`;
            param = `${param}&${Ajax.Param.idElemento1}=${Encriptar(literal.ClaveDeEncriptacion, idElemento1)}`;
            param = `${param}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;


            let url: string = `/${controlador}/${accion}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
                , conCapa
            );

            a.Ejecutar();
        });
    }

    export function LeerVinculosConEnum(llamador: any, controlador: string, idNegocio: number, enumerado: enumNegocio, idElemento: number, parametros: Array<Parametro>, datosDeEntrada: any)
        : Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.enumerado}=${Encriptar(literal.ClaveDeEncriptacion, enumerado)}`;
            param = `${param}&${Ajax.Param.idElemento}=${Encriptar(literal.ClaveDeEncriptacion, idElemento)}`;
            param = `${param}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let url: string = `/${controlador}/${Ajax.EndPoint.LeerVinculosConElNegocio}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.LeerVinculosConElNegocio
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function LeerElementos(llamador: any, controlador: string, accion: string, filtros: Array<ClausulaDeFiltrado>, parametros: Array<Parametro>, datosDeEntrada: any, conCapa: boolean = true)
        : Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            const estaElModuloCargado = typeof Crud !== 'undefined';
            if (estaElModuloCargado && Definido(Crud.Consultor)) {
                const yaExisteGuid = parametros.some(p => p.parametro === Ajax.Param.guid);
                if (!yaExisteGuid)
                    parametros.push(new Parametro(Ajax.Param.guid, Crud.Consultor.GuidDeConsulta));
                parametros.push(new Parametro(Ajax.Parametros.ConsultarConGuid, Crud.Consultor.PaginaDeConsultaConGuid));
                parametros.push(new Parametro(Ajax.Param.id, Crud.Consultor.RegistroId));
                accion =  accion === Ajax.EndPoint.LeerLosEventosDel ? Ajax.EndPoint.LeerLosEventosPorGuid : Ajax.EndPoint.LeerElementosPorGuid
            }

            let url: string = `/${controlador}/${accion}?${Ajax.Param.filtros}=${JSON.stringify(filtros)}&${Ajax.Param.parametros}=${JSON.stringify(parametros)}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
                , conCapa
            );

            a.Ejecutar();
        });
    }

    export function ProcesarOpcionMf(llamador: any, controlador: string, idNegocio: number, opcion: string, esContextual: boolean, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>)
        : Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let paramMf: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            paramMf = `${paramMf}&${Ajax.Param.opcionMf}=${Encriptar(literal.ClaveDeEncriptacion, opcion)}`;
            paramMf = `${paramMf}&${Ajax.Param.esContextual}=${Encriptar(literal.ClaveDeEncriptacion, esContextual)}`;
            paramMf = `${paramMf}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let url: string = `/${controlador}/${Ajax.EndPoint.ProcesarOpcionMf}?${paramMf}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.ProcesarOpcionMf
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }


    export function ProcesarPeticion(llamador: any, controlador: string, idNegocio: number, peticion: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>)
        : Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let paramUrl: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            paramUrl = `${paramUrl}&${Ajax.Param.idVista}=${Encriptar(literal.ClaveDeEncriptacion, 0)}`;
            paramUrl = `${paramUrl}&${Ajax.Param.peticion}=${Encriptar(literal.ClaveDeEncriptacion, peticion)}`;


            let url: string = `/${controlador}/${Ajax.EndPoint.ProcesarPeticion}?${paramUrl}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.ProcesarPeticion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function CargaDinamica(llamador: any, input: HTMLInputElement, filtros: Array<ClausulaDeFiltrado>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let clase: string = input.getAttribute(atListasDinamicas.claseElemento);
            let idInput: string = input.getAttribute('id');
            let cantidad: string = input.getAttribute(atListasDinamicas.cantidad);
            let url: string = DefinirPeticionDeCargarDinamica(llamador.Controlador, clase, Numero(cantidad), filtros);
            let datosDeEntrada = `{"ClaseDeElemento":"${clase}", "IdInput":"${idInput}", "buscada":"${input.value}"}`;
            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.CargaDinamica
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            input.setAttribute(atListasDinamicas.cargando, 'S');
            a.Ejecutar();
        });
    }

    function DefinirPeticionDeCargarDinamica(controlador: string, claseElemento: string, cantidad: number, filtros: Array<ClausulaDeFiltrado>): string {
        let url: string = `/${controlador}/${Ajax.EndPoint.CargaDinamica}?${Ajax.Param.claseElemento}=${claseElemento}&posicion=0&cantidad=${cantidad}&filtrosJson=${JSON.stringify(filtros)}`;
        return url;
    }

    export function SometerTrabajo(llamador: any, trabajo: string, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let paramTrabajo: string = `${Ajax.TrabajosSometidos.parametro.trabajo}=${Encriptar(literal.ClaveDeEncriptacion, trabajo)}`;
            let paramParametros: string = `${Ajax.TrabajosSometidos.parametro.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;


            let url: string = `/${Ajax.TrabajosSometidos.controlador}/${Ajax.EndPoint.SometerTrabajo}?${paramTrabajo}&${paramParametros}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.SometerTrabajo
                , parametros
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function BorrarVinculo(llamador: any, controlador: string, idNegocio: number, idVinculado: number, idElemento1: number, idElemento2: number, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.idVinculado}=${Encriptar(literal.ClaveDeEncriptacion, idVinculado)}`;
            param = `${param}&${Ajax.Param.idElemento1}=${Encriptar(literal.ClaveDeEncriptacion, idElemento1)}`;
            param = `${param}&${Ajax.Param.idElemento2}=${Encriptar(literal.ClaveDeEncriptacion, idElemento2)}`;
            param = `${param}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let url: string = `/${controlador}/${Ajax.EndPoint.BorrarVinculo}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.BorrarVinculo
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }
    export function CrearVinculo(llamador: any, controlador: string, idNegocio: number, idVinculado: number, idElemento: number, json: JSON, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.idVinculado}=${Encriptar(literal.ClaveDeEncriptacion, idVinculado)}`;
            param = `${param}&${Ajax.Param.idElemento1}=${Encriptar(literal.ClaveDeEncriptacion, idElemento)}`;
            param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;
            param = `${param}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let url: string = `/${controlador}/${Ajax.EndPoint.CrearVinculo}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.CrearVinculo
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function CrearDependiente(llamador: any, controlador: string, accion: string, idNegocio: number, idElemento: number, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.idElemento}=${Encriptar(literal.ClaveDeEncriptacion, idElemento)}`;

            let url: string = `/${controlador}/${accion}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , idElemento
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function CrearDetalle(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;
            param = `${param}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let url: string = `/${controlador}/${accion}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }
    export function Vincular(llamador: any, controlador: string, idNegocio: number, idVinculado: number, idElemento: number, json: JSON, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.idVinculado}=${Encriptar(literal.ClaveDeEncriptacion, idVinculado)}`;
            param = `${param}&${Ajax.Param.idElemento1}=${Encriptar(literal.ClaveDeEncriptacion, idElemento)}`;
            param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;
            param = `${param}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let url: string = `/${controlador}/${Ajax.EndPoint.Vincular}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.Vincular
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }


    export function CrearElementoPorPost(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${accion}?${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            //a.Ejecutar();
            var parametrosPost = new Array<Parametro>();
            parametrosPost.push(new Parametro(Ajax.Param.elementoJson, json));
            parametrosPost.push(new Parametro(Ajax.Param.parametrosDeCreacion, JSON.stringify(parametros)));
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametrosPost)));
        });
    }

    export function CrearElemento(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;
            param = `${param}&${Ajax.Param.parametrosDeCreacion}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let url: string = `/${controlador}/${accion}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function CreaRelacion(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON): Promise<ApiDeAjax.DescriptorAjax> {

        if (accion === Ajax.EndPoint.CrearRelacion)
            return CreaRelacionPost(llamador, controlador, Ajax.EndPoint.CrearRelacionPost, idNegocio, json);

        return new Promise((resolve, reject) => {

            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;

            let url: string = `/${controlador}/${accion}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    const _creandoRelacionesConPost: Map<string, boolean> = new Map<string, boolean>();
    export function CreaRelacionPost(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON): Promise<ApiDeAjax.DescriptorAjax> {

        if (_creandoRelacionesConPost.has(controlador)) {
            const msg = `El controlador '${controlador}' ya se está usando, espere a que se libere.`;

            // Devolvemos un objeto que "parezca" un DescriptorAjax para tu función EmitirError
            return Promise.reject({
                nombre: 'Bloqueo de seguridad (Anti-Double Click)',
                message: msg, // Para que entre por el primer if de EmitirError
                resultado: { mensaje: msg, consola: 'Petición cancelada localmente' }
            });
        }

        return new Promise((resolve, reject) => {
            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            //param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;

            let url: string = `/${controlador}/${accion}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    console.log(`El controlador '${controlador}' se a liberado.`)
                    _creandoRelacionesConPost.delete(controlador);
                    resolve(peticion);
                }
                , (peticion) => {
                    console.log(`El controlador '${controlador}' se a liberado.`)
                    _creandoRelacionesConPost.delete(controlador);
                    reject(peticion);
                }
            );

            //a.Ejecutar();
            var parametrosPost = new Array<Parametro>();
            parametrosPost.push(new Parametro(Ajax.Param.elementoJson, json));
            _creandoRelacionesConPost.set(controlador, true);
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametrosPost)));
        });
    }

    export function PersistirNodo(llamador: any, controlador: string, accion: string, negocio: string, json: JSON, datosDeEntrada: Diccionario<any>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let paramNegocio: string = `${Ajax.Param.nombreDeNegocio}=${Encriptar(literal.ClaveDeEncriptacion, negocio)}`;
            let paramTipo: string = `${Ajax.Param.json}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;
            let paramOperacion: string = `${Ajax.Param.operacion}=${Encriptar(literal.ClaveDeEncriptacion, datosDeEntrada.Obtener(Ajax.Param.operacion))}`;

            let url: string = `/${controlador}/${accion}?${paramNegocio}&${paramTipo}&${paramOperacion}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function CrearNodo(llamador: any, controlador: string, accion: string, negocio: string, json: JSON, datosDeEntrada: Diccionario<any>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let paramNegocio: string = `${Ajax.Param.nombreDeNegocio}=${Encriptar(literal.ClaveDeEncriptacion, negocio)}`;
            let paramTipo: string = `${Ajax.Param.json}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;

            let url: string = `/${controlador}/${accion}?${paramNegocio}&${paramTipo}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function LeerJerarquia(llamador: any, controlador: string, accion: string, negocio: string, idPadre: number, filtros: Diccionario<any>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let paramNegocio: string = `${Ajax.Param.nombreDeNegocio}=${Encriptar(literal.ClaveDeEncriptacion, negocio)}`;
            let paramPadre: string = `${Ajax.Param.nodoPadre}=${Encriptar(literal.ClaveDeEncriptacion, idPadre)}`;
            let paramFiltros: string = `${Ajax.Param.filtros}=${Encriptar(literal.ClaveDeEncriptacion, DiccionarioToJson(filtros))}`;

            let url: string = `/${controlador}/${accion}?${paramNegocio}&${paramPadre}&${paramFiltros}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , filtros
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function LeerNodoSeleccionado(llamador: any, controlador: string, accion: string, negocio: string, id: number, filtros: Diccionario<any>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let paramNegocio: string = `${Ajax.Param.nombreDeNegocio}=${Encriptar(literal.ClaveDeEncriptacion, negocio)}`;
            let paramId: string = `${Ajax.Param.id}=${Encriptar(literal.ClaveDeEncriptacion, id)}`;
            let paramFiltros: string = `${Ajax.Param.filtros}=${Encriptar(literal.ClaveDeEncriptacion, DiccionarioToJson(filtros))}`;

            let url: string = `/${controlador}/${accion}?${paramNegocio}&${paramId}&${paramFiltros}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function ModificarElemento(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        if (datosDeEntrada.find((parametro) => parametro.parametro === ltrOperacion.HayCambios && parametro.valor === true)) {
            parametros.push(new Parametro(ltrOperacion.HayCambios, true));
        }
        return new Promise((resolve, reject) => {
            let url: string = `/${controlador}/${accion}?${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            //a.Ejecutar();
            parametros.push(new Parametro(Ajax.Param.elementoJson, json));
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function ModificarRelacion(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON): Promise<ApiDeAjax.DescriptorAjax> {

        if (accion === Ajax.EndPoint.ModificarRelacion)
            return ModificarRelacionPorPost(llamador, controlador, accion, idNegocio, json);

        return new Promise((resolve, reject) => {

            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;

            let url: string = `/${controlador}/${accion}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function ModificarRelacionPorPost(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            //let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            //param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;

            //let url: string = `/${controlador}/${accion}?${param}`;
            let url: string = `/${controlador}/${accion}?${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            //a.Ejecutar();
            var parametros = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.elementoJson, json));
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function PersistirAmpliacion(llamador: any, controlador: string, accion: string, idNegocio: number, json: JSON): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let param: string = `${Ajax.Param.idNegocio}=${Encriptar(literal.ClaveDeEncriptacion, idNegocio)}`;
            //param = `${param}&${Ajax.Param.elementoJson}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(json))}`;

            let url: string = `/${controlador}/${accion}?${param}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , json
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            //a.Ejecutar();

            var parametros = new Array<Parametro>();
            parametros.push(new Parametro(Ajax.Param.elementoJson, json));
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }


    export function EnviarCorreo(llamador: any, controlador: string, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${Ajax.EndPoint.EnviarCorreo}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.EnviarCorreo
                , parametros
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function Transitar(llamador: any, controlador: string, accion: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${Ajax.EndPoint.Transitar}?${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.Transitar
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });

    }
    export function Imprimir(llamador: any, controlador: string, parametros: Array<Parametro>, datosDeEntrada: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${Ajax.EndPoint.Imprimir}?${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.Imprimir
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function CargarListaDeElementos(llamador: any, controlador: string, claseDeElementoDto: string, idLista: string): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let url: string = DefinirPeticionDeCargarElementos(controlador, claseDeElementoDto);
            let datosDeEntrada = `{"ClaseDeElemento":"${claseDeElementoDto}", "IdLista":"${idLista}"}`;
            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.CargarLista
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Get
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    function DefinirPeticionDeCargarElementos(controlador: string, claseElemento: string): string {
        let url: string = `/${controlador}/${Ajax.EndPoint.CargarLista}?${Ajax.Param.claseElemento}=${claseElemento}`;
        return url;
    }

    export function CargarGrid(llamador: any, controlador: string, accion: string, posicion: number, cantidad: number, datosDeEntrada: Crud.DatosPeticionNavegarGrid, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {
            let url: string = DefinirPeticionDeBusqueda(Ajax.EndPoint.LeerDatosParaElGrid, controlador, accion, posicion, cantidad);
            var datosDePeticion = datosDeEntrada;
            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.LeerDatosParaElGrid
                , datosDePeticion
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            llamador.Grid.setAttribute(atGrid.cargando, 'S');
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function Totales(llamador: any, controlador: string, posicion: number, cantidad: number, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${Ajax.EndPoint.Totales}?${Ajax.Param.posicion}=${posicion}&${Ajax.Param.cantidad}=${cantidad}`;
            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.Totales
                , undefined
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );
            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }

    export function Exportar(llamador: any, controlador: string, idNegocio: number, idPlantilla: number, parametros: Array<Parametro>): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${Ajax.EndPoint.Exportar}?${Ajax.Param.idNegocio}=${idNegocio}&${Ajax.Param.idPlantilla}=${idPlantilla}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , Ajax.EndPoint.Exportar
                , parametros
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }



    function DefinirPeticionDeBusqueda(endPoint: string, controlador: string, accion: string, posicion: number, cantidad: number): string {
        var posicion = posicion;

        let url: string = `/${controlador}/${endPoint}`;
        let parametrosPeticion: string = `${Ajax.Param.modo}=${enumModoTrabajo.mantenimiento}` +
            `&${Ajax.Param.accion}=${accion}` +
            `&${Ajax.Param.posicion}=${posicion}` +
            `&${Ajax.Param.cantidad}=${cantidad}`;

        let peticion: string = url + '?' + parametrosPeticion;
        return peticion;
    }

    export function EmitirError(peticion: ApiDeAjax.DescriptorAjax): void {

        if (Definido(peticion['message'])) {
            MensajesSe.Error("EmitirError", peticion['message']);
            return;
        }

        if (EsError(peticion))
            MensajesSe.EmitirExcepcion(`Error al ejecutar la petición ${peticion.nombre}`, ((peticion as unknown) as Error).message, ((peticion as unknown) as Error).stack);
        else
            MensajesSe.EmitirExcepcion(`Error al ejecutar la petición ${peticion.nombre}`, peticion.resultado.mensaje, `${peticion.resultado.consola}\n${peticion.Request.responseText}`);
    }

    //export function SubirPorTrozos(controlador: string, archivos: FileList, idArchivo: string) {
    //    const chunkSize = 1024 * 1024; // 1MB por chunk
    //    const file = archivos[0];
    //    let start = 0;
    //    const totalChunks = Math.ceil(file.size / chunkSize);
    //    let currentChunk = 1;

    //    // Obtener elementos de la barra de progreso
    //    let archivo: HTMLInputElement = document.getElementById(idArchivo) as HTMLInputElement;
    //    const infoArchivo = document.getElementById(archivo.getAttribute(atArchivo.infoArchivo)) as HTMLInputElement;
    //    const barraHtml = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
    //    const span = barraHtml.children[0] as HTMLElement;

    //    // Configurar la barra de progreso
    //    infoArchivo.style.display = ltrStyle.display.none;
    //    barraHtml.classList.remove(ltrCss.barraVerde, ltrCss.barraRoja);
    //    barraHtml.classList.add(ltrCss.barraAzul);
    //    ApiControl.HacerVisibleLaBarra(barraHtml, true);

    //    function ActualizarBarraDeProgreso(progress: number) {
    //        const porcentaje = Math.round(progress * 100);
    //        barraHtml.style.width = porcentaje + '%';
    //        span.innerHTML = porcentaje + '%';
    //    }

    //    function SubirProximoTrozo() {
    //        const end = Math.min(start + chunkSize, file.size);
    //        const chunk = file.slice(start, end);
    //        const formData = new FormData();
    //        formData.append('chunk', chunk);
    //        formData.append('fileName', file.name);
    //        formData.append('chunkNumber', currentChunk.toString());
    //        formData.append('totalChunks', totalChunks.toString());
    //        PonerCapa();
    //        fetch(`/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.SubirTrozo}`, {
    //            method: 'POST',
    //            body: formData
    //        })
    //            .then(response => {
    //                if (!response.ok) {
    //                    throw new Error('Error en la subida del trozo');
    //                }
    //                return response.json();
    //            })
    //            .then(data => {
    //                console.log(`Chunk ${currentChunk}/${totalChunks} subido`);
    //                start += chunkSize;
    //                currentChunk++;

    //                // Actualizar la barra de progreso
    //                var porcentaje = start / file.size
    //                ActualizarBarraDeProgreso(porcentaje > 1 ? 1 : porcentaje);

    //                if (start < file.size) {
    //                    SubirProximoTrozo();
    //                } else {
    //                    MensajesSe.Info(`Subida completada: ${file.name}`);
    //                    TrasSubirUnArchivoAlSelectorDeFichero(archivo, barraHtml, Numero(data.idArchivo));
    //                }
    //            })
    //            .catch(error => {
    //                MensajesSe.Error('SubirPorTrozos', `Error en la subida: ${error.message}`);
    //                // Cambiar el color de la barra a rojo en caso de error
    //                barraHtml.classList.remove(ltrCss.barraAzul);
    //                barraHtml.classList.add(ltrCss.barraRoja);
    //            })
    //            .finally(() => QuitarCapa());
    //    }

    //    SubirProximoTrozo();
    //}

    export async function SubirPorTrozos(controlador: string, archivos: FileList, idArchivo: string) {
        const chunkSize = 1024 * 1024; // 1MB por chunk
        const file = archivos[0];
        const totalChunks = Math.ceil(file.size / chunkSize);
        let idArchivoFinal: number | null = null;

        // Obtener elementos de la barra de progreso
        const archivo = document.getElementById(idArchivo) as HTMLInputElement;
        const infoArchivo = document.getElementById(archivo.getAttribute(atArchivo.infoArchivo)) as HTMLInputElement;
        const barraHtml = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
        const span = barraHtml.children[0] as HTMLElement;

        // Configurar la barra de progreso
        ConfigurarBarraProgreso(infoArchivo, barraHtml);

        PonerCapa();
        try {
            for (let currentChunk = 1; currentChunk <= totalChunks; currentChunk++) {
                const start = (currentChunk - 1) * chunkSize;
                const end = Math.min(start + chunkSize, file.size);
                const chunk = file.slice(start, end);

                const formData = new FormData();
                formData.append('chunk', chunk);
                formData.append('fileName', file.name);
                formData.append('chunkNumber', currentChunk.toString());
                formData.append('totalChunks', totalChunks.toString());
                const response = await fetch(`/${Ajax.Archivos.controlador}/${Ajax.Archivos.accion.SubirTrozo}`, {
                    method: 'POST',
                    body: formData
                });

                if (!response.ok) {
                    throw new Error('Error en la subida del trozo');
                }

                const data = await response.json();
                console.log(`Chunk ${currentChunk}/${totalChunks} subido`);

                ActualizarBarraDeProgreso(barraHtml, span, start / file.size);

                if (currentChunk === totalChunks) {
                    idArchivoFinal = Numero(data.idArchivo);
                }
            }

            MensajesSe.Info(`Subida completada: ${file.name}`);
            if (idArchivoFinal !== null) {
                TrasSubirUnArchivoAlSelectorDeFichero(archivo, barraHtml, idArchivoFinal);
            } else {
                throw new Error('No se pudo obtener el ID del archivo');
            }
        } catch (error) {
            ManejarError(barraHtml, error);
        } finally {
            ActualizarBarraDeProgreso(barraHtml, span, 1);
            QuitarCapa();
        }
    }


    function ConfigurarBarraProgreso(infoArchivo: HTMLInputElement, barraHtml: HTMLDivElement) {
        infoArchivo.style.display = ltrStyle.display.none;
        barraHtml.classList.remove(ltrCss.barraVerde, ltrCss.barraRoja);
        barraHtml.classList.add(ltrCss.barraAzul);
        ApiControl.HacerVisibleLaBarra(barraHtml, true);
    }

    function ActualizarBarraDeProgreso(barraHtml: HTMLDivElement, span: HTMLElement, progress: number) {
        const porcentaje = Math.round(progress * 100);
        barraHtml.style.width = `${porcentaje}%`;
        span.innerHTML = `${porcentaje}%`;
    }

    function ManejarError(barraHtml: HTMLDivElement, error: Error) {
        MensajesSe.Error('SubirPorTrozos', `Error en la subida: ${error.message}`);
        barraHtml.classList.remove(ltrCss.barraAzul);
        barraHtml.classList.add(ltrCss.barraRoja);
    }

    export function TrasSubirUnArchivoAlSelectorDeFichero(archivo: HTMLInputElement, barraHtml: HTMLDivElement, idArchivo: number) {
        barraHtml.classList.remove(ltrCss.barraAzul);
        barraHtml.classList.add(ltrCss.barraVerde);
        archivo.setAttribute(atArchivo.idArchivo, idArchivo.toString())
        let accion = archivo.getAttribute(atArchivo.trasSeleccionar);
        if (Definido(accion))
            Evaluar('ApiDePeticiones.TrasSubirUnArchivoAlSelectorDeFichero', accion, accion.includes('this') ? archivo : undefined);
        else {
            let estaElModuloCargado = typeof Crud !== 'undefined';
            if (estaElModuloCargado && Definido(Crud.CrudCreacion) && Crud.crudMnt.EstoyCreando) {
                Crud.crudMnt.crudDeCreacion.MostrarArchivo(archivo, idArchivo);
            }
        }
    }




    export function SubirArchivoSeleccionado(controlador: string, idArchivo: string): Promise<string> {

        return new Promise((resolve, reject) => {

            let archivo: HTMLInputElement = document.getElementById(idArchivo) as HTMLInputElement;
            let rutaDestino: string = archivo.getAttribute(atArchivo.rutaDestino);
            let extensionesValidas: string = archivo.getAttribute(atArchivo.extensionesValidas);

            let url: string = `/${controlador}/${Ajax.Archivos.accion.SubirArchivo}`;

            let a = new ApiDeAjax.DescriptorAjax(this
                , Ajax.Archivos.accion.SubirArchivo
                , new DatosPeticionSubirArchivo(idArchivo, archivo.files[0].name)
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            let datosPost = new FormData();
            datosPost.append(Ajax.Param.fichero, archivo.files[0]);
            datosPost.append(Ajax.Param.rutaDestino, IsNullOrEmpty(rutaDestino) ? '' : rutaDestino);
            datosPost.append(Ajax.Param.extensiones, extensionesValidas);


            a.DatosPost = datosPost;
            DefinirBarraDeProceso(a, archivo);
            a.Ejecutar();
        });
    }


    export function DefinirBarraDeProceso(descriptor: ApiDeAjax.DescriptorAjax, archivo: HTMLInputElement) {
        let infoArchivo: HTMLInputElement = document.getElementById(archivo.getAttribute(atArchivo.infoArchivo)) as HTMLInputElement;
        infoArchivo.style.display = ltrStyle.display.none;
        let barraHtml: HTMLDivElement = document.getElementById(archivo.getAttribute(atArchivo.barra)) as HTMLDivElement;
        let span: Element = barraHtml.children[0];
        barraHtml.classList.remove(ltrCss.barraVerde, ltrCss.barraRoja);
        barraHtml.classList.add(ltrCss.barraAzul);
        descriptor.Request.upload.addEventListener("progress", (event) => {
            let porcentaje = Math.round((event.loaded / event.total) * 100);
            barraHtml.style.width = porcentaje + '%';
            span.innerHTML = porcentaje + '%';
        });
        ApiControl.HacerVisibleLaBarra(barraHtml, true);
    }


    export function EjecutarPeticionPost(llamador: any, controlador: string, accion: string, parametros: Parametro[], datosDeEntrada: Parametro[]): Promise<ApiDeAjax.DescriptorAjax> {

        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${accion}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador
                , accion
                , datosDeEntrada
                , url
                , ApiDeAjax.TipoPeticion.Asincrona
                , ApiDeAjax.ModoPeticion.Post
                , (peticion) => {
                    resolve(peticion);
                }
                , (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar(Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros)));
        });
    }


    export function EjecutarPeticion(llamador: any, controlador: string, accion: string, parametros: Parametro[], datosDeEntrada: Parametro[], conCapa: boolean = true): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${accion}?${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador,
                accion,
                datosDeEntrada,
                url,
                ApiDeAjax.TipoPeticion.Asincrona,
                ApiDeAjax.ModoPeticion.Get,
                (peticion) => {
                    resolve(peticion);
                },
                (peticion) => {
                    reject(peticion);
                },
                conCapa
            );

            a.Ejecutar();
        });
    }

    export function LeerTipos(controlador: string, accion: string, parametros: Parametro[], llamador: any, datosDeEntrada: Parametro[]): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            let url: string = `/${controlador}/${accion}?${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador,
                accion,
                datosDeEntrada,
                url,
                ApiDeAjax.TipoPeticion.Asincrona,
                ApiDeAjax.ModoPeticion.Get,
                (peticion) => {
                    resolve(peticion);
                },
                (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }

    export function LeerTipo(llamador: any, negocio: string, idTipo: number, parametros: Array<Parametro> = null, datosDeEntrada: Array<Parametro> = null): Promise<ApiDeAjax.DescriptorAjax> {
        return new Promise((resolve, reject) => {

            const esConsultaConGuid: boolean = Definido(parametros) && parametros.find(x => x.parametro === Ajax.Parametros.ConsultarConGuid && x.valor) != undefined;
            let accion: string = esConsultaConGuid ? Ajax.EndPoint.Genericas.LeerTipoPorGuid : Ajax.EndPoint.Genericas.LeerTipo;

            let url: string = `/${ltrControladores.Negocio.TiposDeElemento}/${accion}?${Ajax.Param.enumNegocio}=${negocio}&${Ajax.Param.idTipo}=${idTipo}&${Ajax.Param.parametros}=${Encriptar(literal.ClaveDeEncriptacion, JSON.stringify(parametros))}`;

            let a = new ApiDeAjax.DescriptorAjax(llamador,
                accion,
                datosDeEntrada,
                url,
                ApiDeAjax.TipoPeticion.Asincrona,
                ApiDeAjax.ModoPeticion.Get,
                (peticion) => {
                    resolve(peticion);
                },
                (peticion) => {
                    reject(peticion);
                }
            );

            a.Ejecutar();
        });
    }




}