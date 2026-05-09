using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.SistemaDocumental;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using MVCSistemaDeElementos.Controllers;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace SistemaDeElementos.Controllers.Seguridad;

public class AccesoController : HomeController
{
    private readonly GestorDeUsuarios _gestordeUsuarios;


    public AccesoController(ContextoSe contexto,
        GestorDeUsuarios gestorDeUsuarios, GestorDeErrores gestorDeErrores)
        : base(contexto, gestorDeUsuarios.Mapeador, gestorDeErrores)
    {
        _gestordeUsuarios = gestorDeUsuarios;
    }

    public async Task<IActionResult> Logout()
    {
        await AnularLaCookie();
        ServicioDeCaches.EliminarCachesDeDescriptores();
        return LocalRedirect($"~/{enumControladoresSeguridad.Acceso}/{enumVistasSeguridad.Conectar}.html");
    }


    //END-POINT: Desde Conectar.ts
    public JsonResult EpReferenciarFoto(string restrictor)
    {
        var r = new Resultado();

        try
        {

            var filtros = JsonConvert.DeserializeObject<List<ClausulaDeFiltrado>>(restrictor);
            var opcionesDeMapeo = new Dictionary<string, object>
            {
                { ltrParametrosDto.DescargarGestionDocumental, true },
                { ltrParametrosNeg.ObtenerCertificado, false },
                { ltrParametrosNeg.AplicarJoin, true }
            };

            var elementos = _gestordeUsuarios.LeerElementos(0, -1, filtros, null, opcionesDeMapeo).ToList();

            if (elementos.Count == 0)
            {
                Emitir($"No se ha localizado el usuario: {filtros[0].Valor}");
            }

            if (elementos.Count > 1)
            {
                throw new Exception($"Hay más de un usuario identificado como: {filtros[0].Valor}");
            }

            r.Datos = elementos[0].Archivo == null ? "" : elementos[0].Archivo.Replace("Archivos", "Acceso");
            r.Estado = enumEstadoPeticion.Ok;
            //r.Mensaje = $"se han leido 1 {(1 > 1 ? "registros" : "registro")}";
            // r.consola = $"login: {restrictor},  idDeUsuario: {elementos[0].Id}, avatar: {elementos[0].Archivo}, idDeArchivo: {elementos[0].IdArchivo}";
        }
        catch (Exception e)
        {
            r.Estado = enumEstadoPeticion.Error;
            r.Consola = Detalle(e);

            if (e.Data.Contains(Datos.Mostrar) && (bool)e.Data[Datos.Mostrar]!)
            {
                r.Mensaje = e.Message;
            }
            else
            {
                r.Mensaje = "Error al leer";
            }
        }

        return new JsonResult(r);

    }

    public FileStreamResult EpDescargarThumsnail(string negocio, int idElemento, int idArchivo, int ancho, int alto)
    {
        //todo: validar acceso al negocio en consulta
        var ruta = ApiDeArchivos.FicheroNoEncontrado;
        var mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(ruta));
        var nombreFichero = ApiDeArchivos.NombreDelFicheroNoEncontrado;
        try
        {
            var archivo = GestorDeArchivos.LeerRegistroPorId(Contexto, idArchivo);
            mimeType = MimeTypeMap.GetMimeType(Path.GetExtension(archivo.Nombre));
            nombreFichero = archivo.Nombre;
            ruta = ApiDeArchivos.DescargarThumbnail(Contexto, idArchivo, archivo.Nombre, archivo.AlmacenadoEn, ancho, alto, mimeType);
        }
        catch (Exception e)
        {
            Contexto.Traza.AnotarExcepcion(e);
        }

        return File(ApiController.DevolverFichero(Response, Contexto, ruta, mimeType, nombreFichero), mimeType);
    }


    //END-POINT: Desde Conectar.ts
    [HttpPost]
    public JsonResult EpValidarAcceso(string login, string password)
    {
        var r = new Resultado();

        try
        {
            Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
            password = HttpUtility.UrlDecode(password);
            _gestordeUsuarios.ValidarUsuario(login, password);
            r.Datos = null;
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "usuario validado";
        }
        catch (Exception e)
        {
            r.Estado = enumEstadoPeticion.Error;
            ApiController.PrepararError(e, r, "Error al validar usuario");
        }
        finally
        {
            Contexto.QuitarUsuario();
        }

        return new JsonResult(r);
    }


    //END-POINT: Desde Conectar.ts
    [HttpPost]
    public JsonResult epSolicitarNuevaContrasena(string login)
    {
        string referer = HttpContext.Request.Headers["Referer"].ToString();
        // Verificamos que el referer contenga la ruta esperada
        if (string.IsNullOrEmpty(referer) || !referer.Contains($"/{enumControladoresSeguridad.Acceso}/{enumVistasSeguridad.Conectar}", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonResult(new Resultado
            {
                Estado = enumEstadoPeticion.Error,
                Mensaje = "Acceso no autorizado: Origen no válido."
            });
        }

        var r = new Resultado();
        var tran = Contexto.IniciarTransaccion();
        Contexto.IniciarTraza(nameof(epSolicitarNuevaContrasena));
        try
        {
            Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
            var usuario = (UsuarioDtm)_gestordeUsuarios.LeerRegistro(nameof(UsuarioDtm.Login), login, errorSiNoHay: false);
            if (usuario == null || !usuario.Activo)
            {
                if (usuario!= null || usuario.IntentosFallidos > 3)
                    Emitir($"El usuario está bloqueado por tres intentos fallidos, espere al desbloqueo o contacte con el administrador");
                else
                    Emitir($"El usuario indicaddo no existe o no está activo");
            }

            usuario.SolicitarNuevaContrasena(Contexto);
            Contexto.Commit(tran);
            r.Datos = null;
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "Se ha enviado un correo con las instrucciones para cambiar la contraseña.";
        }
        catch (Exception e)
        {
            Contexto.Rollback(tran);
            ApiController.PrepararError(e, r, "Error al solicitar una nueva contraseña.");
        }
        finally
        {
            Contexto.CerrarTraza();
            Contexto.QuitarUsuario();
        }
        return new JsonResult(r);
    }

    [HttpPost]
    public JsonResult epActualizarContrasena(string guid, string password)
    {
        string referer = HttpContext.Request.Headers["Referer"].ToString();
        if (string.IsNullOrEmpty(referer) || !referer.Contains($"/{enumControladoresSeguridad.Acceso}/{enumVistasSeguridad.NuevaContrasena}", StringComparison.OrdinalIgnoreCase))
        {
            return new JsonResult(new Resultado
            {
                Estado = enumEstadoPeticion.Error,
                Mensaje = "Acceso no autorizado: Origen no válido."
            });
        }

        var r = new Resultado();
        password = HttpUtility.UrlDecode(password);
        var tran = Contexto.IniciarTransaccion();
        Contexto.IniciarTraza(nameof(epActualizarContrasena), debugar: true);
        try
        {
            Contexto.AsignarUsuario(ExtensorDeUsuarios.Administrador(Contexto));
            var usuario = (UsuarioDtm)_gestordeUsuarios.LeerRegistro(nameof(UsuarioDtm.Guid), guid, errorSiNoHay: false);
            if (usuario == null || !usuario.Activo)
            {
                if (usuario != null || usuario.IntentosFallidos > 3)
                    Emitir($"El usuario está bloqueado por tres intentos fallidos, espere al desbloqueo o contacte con el administrador");
                else
                    Emitir($"El guid indicado no existe o el usuario no está activo o la operación se anuló, solicite un nuevo cambio o contacte con el administrador.");
            }

            if (usuario.SolicitadaEl.Fecha().AddMinutes(15) < DateTime.Now)
                Emitir($"La operación de nueva contraseña caducó, vuelva a solicitarla");

            GestorDeUsuarios.NuevaContrasena(Contexto, usuario, password);
            Contexto.Commit(tran);
            r.Datos = null;
            r.Estado = enumEstadoPeticion.Ok;
            r.Mensaje = "Se ha cambiado la contraseña.";
        }
        catch (Exception e)
        {
            Contexto.Rollback(tran);
            ApiController.PrepararError(e, r, "Error al cambiar la contraseña.");
        }
        finally
        {
            Contexto.CerrarTraza();
            Contexto.QuitarUsuario();
        }
        return new JsonResult(r);
    }

    [HttpPost]
    public async Task<IActionResult> Conectar(string login, string password, string returnUrl)
    {
        await AnularLaCookie();

        try
        {
            password = HttpUtility.UrlDecode(password);
            var usuario = _gestordeUsuarios.ValidarUsuario(login, password);

            if (!GestorDePassword.CumpleCriteriosLaPassword(password) || password == VariableDeUsuario.PasswordPorDefecto())
            {
                string urlCambioPass = ExtensorDeUsuarios.UrlNuevaContrasena(
                                 _gestordeUsuarios.Contexto,
                                 Contexto.SeleccionarPorId<UsuarioDtm>(usuario.Id),
                                 motivo: ltrDeUnUsuario.Motivo_ReglasDeContraseña
                                 );

                return Redirect(urlCambioPass);
            }


            await RegistrarLaCookie(usuario);
        }
        catch
        {
            return await Logout();
        }

        _gestordeUsuarios.Contexto.AnotarTraza("Traza de conexión", $"Usario {login} validado" +
                                                                    $"{Environment.NewLine}Esquema:{HttpContext.Request.Scheme}" +
                                                                    $"{Environment.NewLine}Puerto:{HttpContext.Request.Host.Value}" +
                                                                    $"{Environment.NewLine}Base:{HttpContext.Request.PathBase}" +
                                                                    $"{Environment.NewLine}Ruta:{HttpContext.Request.Path}");

        if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return LocalRedirect(returnUrl);
        }
        else
        {
            return LocalRedirect("~/");
        }
    }


    private async Task RegistrarLaCookie(UsuarioDto usuario)
    {
        var caracteres = new List<Claim>
        {
            new(nameof(UsuarioDto.Id), usuario.Id.ToString()),
            new(nameof(UsuarioDto.Login), usuario.Login),
            new(ClaimTypes.Name, usuario.NombreCompleto)
        };

        var caracteresIdentitarios = new ClaimsIdentity(caracteres, CookieAuthenticationDefaults.AuthenticationScheme);
        var propiedadesDeAutentificacion = new AuthenticationProperties
        {
            IsPersistent = true,
            RedirectUri = Request.Host.Value,
            ExpiresUtc = DateTime.Now.AddMinutes(8 * 60)
        };

        HttpContext.User = new ClaimsPrincipal(caracteresIdentitarios);
        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, HttpContext.User, propiedadesDeAutentificacion);
    }

    private async Task AnularLaCookie()
    {
        try
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }
        catch
        {

        }
    }


    [HttpPost("movil/login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginInput input)
    {
        await AnularLaCookie();

        try
        {
            var usuario = _gestordeUsuarios.ValidarUsuario(input.UserName, input.Password);

            var claims = new List<Claim>
            {
                new(nameof(UsuarioDto.Id), usuario.Id.ToString()),
                new(nameof(UsuarioDto.Login), usuario.Login),
                new(ClaimTypes.Name, usuario.NombreCompleto)
            };

            var caracteresIdentitarios = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.User = new ClaimsPrincipal(caracteresIdentitarios);
            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, HttpContext.User);

            return Ok(new LoginResultOutput { UserName = usuario.Login, Name = usuario.Nombre, Surname = usuario.Apellido });
        }
        catch
        {
            return Unauthorized();
        }
    }

    [HttpGet("movil/logout")]
    public async Task LogoutMovil()
    {
        await AnularLaCookie();
    }

    [HttpGet("movil/sections")]
    [Authorize]
    public JsonResult EpNegociosParaAdjuntarDocumentacionMovil()
    {
        //enumNegocio.Archivador.PersistirParametro(_contexto, ExtensorDeParmetrosDeNegocio.enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil, "S");
        //enumNegocio.Expediente.PersistirParametro(_contexto, ExtensorDeParmetrosDeNegocio.enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil, "S");
        return EpNegociosParaAdjuntarDocumentacion(incluirSinPermisos: false);
    }

}