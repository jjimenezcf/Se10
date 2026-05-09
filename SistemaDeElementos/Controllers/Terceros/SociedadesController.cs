using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Contabilidad;
using GestoresDeNegocio.Terceros;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class SociedadesController : EntidadController<ContextoSe, SociedadDtm, SociedadDto>
    {
        public SociedadesController(GestorDeSociedades gestorDeSociedades, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeSociedades,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudSociedades()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeSociedades).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Terceros}/{nameof(CrudSociedades)}";
                    return base.View(destino, new DescriptorDeSociedades(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<SociedadDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeSociedades(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);
            indicadores.Add(IndInterlocutor.TercerosJudiciales, ExtensorDePleitos.ModuloActivo(contexto) || ExtensorDeExpedientes.HayTiposJuridicos(Contexto));
            return indicadores;
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            List<int> ids;
            switch (opcion)
            {
                case eventosDeMf.Interlocutores:
                    return negocio.CrearInterlocutores(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
                case eventosDeMf.CentrosGestores:
                    return null;
                case eventosDeMf.AsociarCertificado:
                    return null;
                case eventosDeMf.Soc_CuentasBancarias:
                    return null;
                case eventosDeMf.Soc_TarjetasBancarias:
                    return null;
                case eventosDeMf.Soc_facturador:
                    return null;
                case eventosDeMf.Spr_LoteDeTerceros:
                    ids = (List<int>)parametros[ltrParametrosEp.ids];
                    if (ids.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de una sociedad");
                    TrabajosContables.SometerCrearLoteDeTerceros(Contexto, ids[0]);
                    return null;
                case eventosDeMf.Soc_ActivarVerifactu:
                    ids = (List<int>)parametros[ltrParametrosEp.ids];
                    if (ids.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de una sociedad");
                    var mensaje = ((GestorDeSociedades)_GestorDeElementos).ActivarVerifactu(Contexto, ids[0]);
                    return new Resultado { Mensaje = mensaje};
                case eventosDeMf.Soc_RecomponerBlockChain:
                    ids = (List<int>)parametros[ltrParametrosEp.ids];
                    if (ids.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de una sociedad");
                    ((GestorDeSociedades)_GestorDeElementos).RecomponerBlockChain(Contexto, ids[0]);
                    return new Resultado { Mensaje = "Sistema de ficheros de BlockChain recompuesto" };
                case eventosDeMf.Soc_CatalogosJudiciales:
                    ids = (List<int>)parametros[ltrParametrosEp.ids];
                    if (ids.Count != 1) GestorDeErrores.Emitir("Solo ha de indicar el id de una sociedad");
                    ((GestorDeSociedades)_GestorDeElementos).CatalogosJudiciales(Contexto, ids[0]);
                    return new Resultado { Mensaje = "Trabajo de actualización de catálogos judiciales sometido" };
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

        public JsonResult epAsociarCertificado(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            Contexto.IniciarTraza(nameof(epAsociarCertificado));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var idCertificado = (int)parametros.LeerValor<long>(nameof(CertificadoDeUnaSociedadDto.IdCertificado));
                var idSociedad = (int)parametros.LeerValor<long>(nameof(CertificadoDeUnaSociedadDto.IdSociedad));
                ApiDeCertificados.ValidarPassword(Contexto, idCertificado, parametros.LeerValor<string>(nameof(CertificadoDeUnaSociedadDto.PassworDelCertificado)));
                GestorDeSociedades.AsociarCertificado(Contexto, idSociedad: idSociedad, idCertificado: idCertificado);
                r.Mensaje = $"Certificado asociado correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al asociar el certificado.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        public JsonResult epLeerDatosDeSociedad(string parametrosJson)
        {
            var r = new Resultado();
            Dictionary<string, object> parametros = parametrosJson.ToDiccionarioDeParametros();
            Contexto.IniciarTraza(nameof(epLeerDatosDeSociedad));
            try
            {
                ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Mapeador, HttpContext);
                var idSociedad = (int)parametros.LeerValor<long>(nameof(ltrParametrosEp.id));
                r.Datos = Contexto.SeleccionarDto<SociedadDto, SociedadDtm>(idSociedad, parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, aplicarJoin: true));
                r.Consola = $"Datos de sociedad leidos correctamente";
                r.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor.Render();
                r.Estado = enumEstadoPeticion.Ok;
            }
            catch (Exception e)
            {
                ApiController.PrepararError(e, r, "Error al leer los datos de sociedad.");
            }
            finally
            {
                Contexto.CerrarTraza();
            }
            return new JsonResult(r);
        }

        //

    }
}
