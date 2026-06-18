using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Callejero;
using GestoresDeNegocio.Juridico;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Callejero;
using ModeloDeDto.Juridico;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Seguridad;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CallesController : EntidadController<ContextoSe, CalleDtm, CalleDto>
    {

        public CallesController(GestorDeCalles gestorDeCalles, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCalles,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudCalles()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeCalles).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Callejero}/{nameof(CrudCalles)}";
                    return base.View(destino, new DescriptorDeCalles(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<CalleDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeCalles(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }


        protected override IEnumerable<CalleDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var accion = opcionesDeMapeo.LeerValor<string>(ltrParametrosNeg.Peticion, null);
            if (accion is not null )
            {
                opcionesDeMapeo.Remove(ltrParametrosNeg.Peticion);
            }

            var calles = _GestorDeElementos.LeerElementos(posicion, cantidad, filtros, orden, opcionesDeMapeo).ToList();

            if (accion == "ValidarSiExisteCalle")
            {

                var nombreCalle = filtros.Where(x => x.Clausula.ToLower() == nameof(CalleDto.Nombre).ToLower()).First().Valor;
                var nombreTipovia = filtros.Where(x => x.Clausula.ToLower() == nameof(CalleDto.TipoDeVia).ToLower()).First().Valor;
                var nombreMunicipio = filtros.Where(x => x.Clausula.ToLower() == nameof(CalleDto.Municipio).ToLower()).First().Valor;
                var nombreCp = filtros.Where(x => x.Clausula.ToLower() == nameof(CalleDto.CodigoPostal).ToLower()).First().Valor;
                var busqueda = $"{nombreTipovia} {nombreCalle}, {nombreCp}-{nombreMunicipio}";
                if (calles.Count() > 1)
                {
                    GestorDeErrores.Emitir($"Existen más de una calle para los mismo valores buscados: '{busqueda}', no se puede crear");
                }
                if (calles.Count() == 1)
                {
                    GestorDeErrores.Emitir($"Ya existe una calle para los mismo valores buscados: '{busqueda}', no se puede crear");
                }

                var municipio = Contexto.SeleccionarPorNombre<MunicipioDtm>(nombreMunicipio, $"No se ha localizado el municipio indicado: '{nombreMunicipio}'", aplicarJoin: true);
                var tipovia = Contexto.SeleccionarPorNombre<TipoDeViaDtm>(nombreTipovia, errorSiNoHay: false);
                if (tipovia == null)
                {
                    tipovia = new TipoDeViaDtm {Nombre = nombreTipovia,  Sigla = nombreTipovia.Substring(1, 5).Trim() }.InsertarComoAdministrador(Contexto);
                }
                var cp = Contexto.SeleccionarPorPropiedad<CodigoPostalDtm>(nameof(CodigoPostalDtm.Codigo), nombreCp, errorSiNoHay: false);
                if (cp == null)
                {
                    cp = new CodigoPostalDtm { Codigo = nombreCp }.InsertarComoAdministrador(Contexto);
                    new CpsDeUnMunicipioDtm { IdMunicipio = municipio.Id, IdCp = cp.Id }.InsertarComoAdministrador(Contexto);
                }

                calles.Add(new CalleDto()
                {
                    Id = 0,
                    Nombre = nombreCalle,
                    IdPais = municipio.Provincia.IdPais,
                    Pais = municipio.Provincia.Pais.Nombre,
                    IdProvincia = municipio.Provincia.Id,
                    Provincia = municipio.Provincia.Nombre,
                    IdMunicipio = municipio.Id,
                    Municipio = nombreMunicipio,
                    TipoDeVia = tipovia.Nombre,
                    IdTipoDeVia = tipovia.Id,
                    IdCp = cp.Id,
                    CodigoPostal = nombreCp
                });

            }

            return calles;
        }
    }
}
