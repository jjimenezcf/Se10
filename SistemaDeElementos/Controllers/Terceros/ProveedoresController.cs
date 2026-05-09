using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using GestoresDeNegocio.Terceros;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using Utilidades;
using System;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Contabilidad;
using GestorDeElementos.Extensores;
using System.Linq;

namespace MVCSistemaDeElementos.Controllers
{
    public class ProveedoresController : EntidadController<ContextoSe, ProveedorDtm, ProveedorDto>
    {
        public ProveedoresController(GestorDeProveedores gestorDeProveedor, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeProveedor,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudProveedores()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeProveedores).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Terceros}/{nameof(CrudProveedores)}";
                    return base.View(destino, new DescriptorDeProveedores(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<ProveedorDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeProveedores(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        public IActionResult CrudTerceros()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            if (!HttpContext.Request.Query.ContainsKey(ltrParametrosEp.id))
                return RenderMensaje("No se ha indicado el Proveedor del que se quiere obtener su detalle");
            try
            {
                var id = HttpContext.Request.Query[ltrParametrosEp.id].ToString().Entero();
                var Proveedor = GestorDeProveedores.LeerRegistroPorId(Contexto, id, aplicarJoin: true);
                var url = @$"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

                if (Proveedor.Interlocutor.IdPersona != null)
                    url = @$"{url}/Personas/CrudPersonas?Id={Proveedor.Interlocutor.IdPersona}";
                if (Proveedor.Interlocutor.IdSociedad != null)
                    url = @$"{url}/Sociedades/CrudSociedades?Id={Proveedor.Interlocutor.IdSociedad}";

                return Redirect(url);
            }
            catch (Exception e)
            {
                var m = e.Data.Contains(GestorDeErrores.Datos.Mostrar) && (bool)e.Data[GestorDeErrores.Datos.Mostrar] == true
                    ? e.Message
                    : $"Error al acceder a los datos del tercero";
                return RenderMensaje(m);
            }
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarRelacion(ProveedorDto elemento)
        {
            var parametrosDeNegocio = base.AntesDeEjecutar_ModificarRelacion(elemento);
            var Proveedor = GestorDeProveedores.Gestor(Contexto, Contexto.Mapeador).LeerElementoPorId(elemento.Id);
            Proveedor.Telefono = elemento.Telefono;
            Proveedor.eMail = elemento.eMail;
            Proveedor.CopiarEn(elemento);
            return parametrosDeNegocio;
        }

        protected override bool HayQueCrearSiNoSeEncuentra(Dictionary<string, object> parametros)
        {
            var hayQueCrear = parametros.LeerValor(nameof(SociedadDto.CrearProveedor), false);

            return hayQueCrear;
        }

        protected override IEnumerable<ProveedorDto> CrearElementoSiNoSeEncuentra(IEnumerable<ProveedorDto> elementosLeidos, Dictionary<string, object> parametros)
        {
            int? idProveedor = null;
            if (elementosLeidos.Count() == 0)
            {
                var sociedad = new SociedadDtm
                {
                    Nombre = parametros.LeerValor<string>(nameof(SociedadDtm.Nombre)),
                    RazonSocial = parametros.LeerValor<string>(nameof(SociedadDtm.Nombre)),
                    NIF = parametros.LeerValor<string>(nameof(SociedadDtm.NIF)),
                    eMail = parametros.LeerValor(nameof(SociedadDtm.eMail), "@"),
                    Telefono = parametros.LeerValor(nameof(SociedadDtm.Telefono), "-")
                };
                idProveedor = ExtensorDeProveedores.CrearProveedor(Contexto, sociedad).Id;
            }
            else idProveedor = elementosLeidos.ToList()[0].Id;

            var numeroIban = parametros.LeerValor<string>(nameof(CuentaBancariaDtm.NumeroIban), null)?.Replace(" ", "").Replace(".", "").Replace("-", "") ?? null;
            if (!numeroIban.IsNullOrEmpty() && numeroIban.Length == 24)
            {
                ExtensorDeProveedores.CrearCuentaBancaria(Contexto, (int)idProveedor, numeroIban, "Cta de pago de facturas");
            }

            //var pais = parametros.LeerValor(nameof(ProvinciaDtm.Pais), "");
            //var provincia = parametros.LeerValor(nameof(MunicipioDtm.Provincia), "");
            //var municipio = parametros.LeerValor(nameof(CalleDtm.Municipio), "");
            //var calle = parametros.LeerValor(nameof(CallesDeUnaZonaDto.Calle), "");
            //var cp = parametros.LeerValor(nameof(CalleDto.CodigoPostal), "");
            //var tipoDeVia = parametros.LeerValor(nameof(CalleDto.TipoDeVia), "");
            //var np = parametros.LeerValor(nameof(ltrDireccion.NumeroPolicia), "");
            //var rd = parametros.LeerValor(nameof(ltrDireccion.RestoDireccion), "");
            //var direccion = ExtensorDeDirecciones.CrearDireccion(pais, provincia, municipio, tipoDeVia, calle, cp, np, rd);
            //if (direccion is not null)
            //{
            //    var proveedordTM = Contexto.SeleccionarPorId<ProveedorDtm>((int)idProveedor);
            //    proveedordTM.AsociarDireccion(Contexto, direccion);
            //}
            List<ClausulaDeFiltrado> filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(nameof(ProveedorDtm.Id), enumCriteriosDeFiltrado.igual, (int)idProveedor) };
            var orden = new List<ClausulaDeOrdenacion>();
            return _GestorDeElementos.LeerElementos(0, 2, filtros, orden, parametros);
        }

    }
}
