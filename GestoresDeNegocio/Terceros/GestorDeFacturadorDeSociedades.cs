using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Terceros;
using Newtonsoft.Json.Linq;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using Utilidades;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeFacturadorDeSociedades : GestorDeElementos<ContextoSe, FacturadorDeSociedadDtm, FacturadorDeSociedadDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrFacturadorDeSociedad
        {
        }

        public class MapearFacturadorDeSociedad : Profile
        {
            public MapearFacturadorDeSociedad()
            {
                CreateMap<FacturadorDeSociedadDtm, FacturadorDeSociedadDto>()
                .ForMember(dto => dto.TipoDeFactura, x => x.MapFrom(dtm => dtm.TipoDeFactura.Nombre))
                .ForMember(dto => dto.Cg, x => x.MapFrom(dtm => dtm.Cg.Expresion));
                CreateMap<FacturadorDeSociedadDto, FacturadorDeSociedadDtm>()
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public GestorDeFacturadorDeSociedades(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeFacturadorDeSociedades Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeFacturadorDeSociedades(contexto, mapeador);
        }

        protected override IQueryable<FacturadorDeSociedadDtm> AplicarJoins(IQueryable<FacturadorDeSociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.TipoDeFactura);
            consulta = consulta.Include(x => x.Cg);
            return consulta;
        }

        protected override IQueryable<FacturadorDeSociedadDtm> AplicarFiltros(IQueryable<FacturadorDeSociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            var filtroPorCg = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(IUsaCg.Cg).ToLower(), System.StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorCg != null)
            {
                consulta = consulta.Where(x => x.IdElemento == Contexto.Set<CentroGestorDtm>().First(cg => cg.Id == filtroPorCg.Valor.Entero()).IdSociedad);
                filtroPorCg.Aplicado = true;
            }

            var filtroPorActiva = filtros.FirstOrDefault(x => x.Clausula.Equals(nameof(IUsaActiva.Activa).ToLower(), System.StringComparison.CurrentCultureIgnoreCase));
            if (filtroPorActiva != null)
            {
                consulta = consulta.Where(x => x.Activa == true &&
                    Contexto.Set<TipoDeFacturaEmtDtm>().Any(tipo => tipo.Activo == true && tipo.Id == x.IdTipoDeFactura) &&
                    Contexto.Set<CentroGestorDtm>().Any(cg => cg.Baja == false && cg.Id == x.IdCg));
                filtroPorActiva.Aplicado = true;
            }

            return consulta;
        }

        protected override void DefinirOrden(List<ClausulaDeOrdenacion> orden, ParametrosDeNegocio parametros)
        {
            base.DefinirOrden(orden, parametros);
            if (orden.Count() == 1 && orden.Any(o => o.OrdenarPor == nameof(RegistroDtm.Id) || o.OrdenarPor == nameof(IDetalle.IdElemento)))
            {
                orden.Clear();
                orden.Add(new ClausulaDeOrdenacion(nameof(FacturadorDeSociedadDtm.Activa), ModoDeOrdenancion.ascendente));
                orden.Add(new ClausulaDeOrdenacion($"{nameof(FacturadorDeSociedadDtm.Cg)}.{nameof(CentroGestorDtm.Codigo)}", ModoDeOrdenancion.ascendente));
                orden.Add(new ClausulaDeOrdenacion($"{nameof(FacturadorDeSociedadDtm.TipoDeFactura)}.{nameof(INombre.Nombre)}", ModoDeOrdenancion.ascendente));
            }
        }

        protected override void AntesDePersistir(FacturadorDeSociedadDtm facturador, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(facturador, parametros);

            if (!Contexto.SePuedeParametrizar())
                GestorDeErrores.Emitir($"El usuario conectado no tiene permisos ");

            if (parametros.Eliminando)
                GestorDeErrores.Emitir($"No puede eliminar el registro");

            ValidadorDeJson.ValidarPropiedadesJson(facturador.MapeosJson, Contexto, typeof(ValidadoresDeMapeosJsonDeFacturadorDeSociedades));

            if (parametros.Insertando)
            {
                var tipo = facturador.TipoDeFactura(Contexto);
                if (!tipo.Activo)
                    GestorDeErrores.Emitir($"El tipo '{tipo.Nombre}' de factura a emitir no está activo");

                var cg = facturador.Cg(Contexto);
                if (cg.Baja)
                    GestorDeErrores.Emitir($"El centro gestor '{cg.Codigo}' está de baja");

                if (cg.IdSociedad != facturador.IdElemento)
                    GestorDeErrores.Emitir($"El centro gestor '{cg.Codigo}' no pertenece a la sociedad '{Contexto.SeleccionarPorId<SociedadDtm>(facturador.IdElemento).NIF}'");
                facturador.Activa = true;
                facturador.ApiKey = GenerarApiKey(facturador.IdElemento, facturador.IdCg, facturador.IdTipoDeFactura);
            }
            else if (parametros.Modificando)
            {
                facturador.ValidarQueSonIguales((FacturadorDeSociedadDtm)parametros.registroEnBd,
                    excepto: new List<string> { nameof(IUsaActiva.Activa), nameof(FacturadorDeSociedadDtm.MapeosJson) });
                //if (parametros.EstaDesactivando(facturador) || parametros.EstaActivando(facturador))
                //else
                //    GestorDeErrores.Emitir($"No puede modificar el registro, desactívelo o actívelo");
            }

        }

        private static string GenerarApiKey(int IdSociedad, int idCg, int idTipoFactura)
        {
            string input = $"{IdSociedad}-{idCg}-{idTipoFactura}";
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));

                string base64Hash = Convert.ToBase64String(bytes)
                                           .Replace('+', '-')
                                           .Replace('/', '_')
                                           .TrimEnd('=');
                return base64Hash;
            }
        }

        public static void ValidarApiKey(int IdSociedad, int idCg, int idTipoFactura, string apiKey)
        {
            var apikeyGenerada = GestorDeFacturadorDeSociedades.GenerarApiKey(IdSociedad, idCg, idTipoFactura);
            if (apikeyGenerada != apiKey)
                GestorDeErrores.Emitir("El apikey indicado no es válido");
        }


        protected override void DespuesDePersistir(FacturadorDeSociedadDtm facturador, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(facturador, parametros);
            if (parametros.Insertando)
                ((SociedadDtm)facturador.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido un facturador", $"El facturador '{facturador.Nombre(Contexto)}' se  ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");
            else
            if (parametros.EstaActivando(facturador) || parametros.EstaDesactivando(facturador))
                ((SociedadDtm)facturador.DetalleDe(Contexto)).CrearTraza(Contexto, $"{(facturador.Activa ? "Activado " : "Desactivado")} el facturador", $"El facturador '{facturador.Nombre(Contexto)}' ha sido {(facturador.Activa ? "activado" : "desactivado")} por el usuario {Contexto.DatosDeConexion.Login}");
            else
            if (parametros.Eliminando)
                ((SociedadDtm)facturador.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado el facturador", $"El facturador '{facturador.Nombre(Contexto)}' ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");
        }



        protected override void DespuesDeMapearElElemento(FacturadorDeSociedadDtm facturador, FacturadorDeSociedadDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(facturador, elemento, parametros);

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Sociedad, facturador.DetalleDe(Contexto));
            elemento.Titulo = facturador.MapeosJson;            
        }

    }

}
