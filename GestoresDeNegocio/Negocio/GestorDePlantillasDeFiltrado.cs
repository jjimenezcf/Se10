using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDePlantillasDeFiltrado : GestorDeElementos<ContextoSe, PlantillaDeFiltradoDtm, PlantillaDeFiltradoDto>
    {
        public class MapearVariables : Profile
        {
            public MapearVariables()
            {
                CreateMap<PlantillaDeFiltradoDtm, PlantillaDeFiltradoDto>()
                .ForMember(dto => dto.Plantilla, dtm => dtm.MapFrom(dtm => dtm.Nombre));

                CreateMap<PlantillaDeFiltradoDto, PlantillaDeFiltradoDtm>()
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDePlantillasDeFiltrado(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDePlantillasDeFiltrado Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlantillasDeFiltrado(contexto, mapeador);
        }

        protected override void AntesDePersistir(PlantillaDeFiltradoDtm parametro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(parametro, parametros);
        }

        protected override void DespuesDePersistir(PlantillaDeFiltradoDtm parametro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(parametro, parametros);
        }

        protected override void EliminarCaches(PlantillaDeFiltradoDtm registro, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(registro, parametros);
            ServicioDeCaches.EliminarCache(CacheDe.RenderCrud);
        }

        protected override IQueryable<PlantillaDeFiltradoDtm> AplicarJoins(IQueryable<PlantillaDeFiltradoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(p => p.Negocio);
            return consulta;
        }

        protected override void DespuesDeMapearElElemento(PlantillaDeFiltradoDtm parametro, PlantillaDeFiltradoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(parametro, elemento, parametros);
        }

        public static PlantillaDeFiltradoDto GuardarPlantillasDeFiltrado(ContextoSe contexto, enumNegocio negocio, string controlador, string accion, Dictionary<string, object> parametros)
        {
            var plantilla = parametros.LeerValor(nameof(PlantillaDeFiltradoDto.Plantilla), "").Trim();
            if (plantilla.IsNullOrEmpty()) GestorDeErrores.Emitir("Ha de indicar el nombre de la plantilla a crear");
            var titulo = parametros.LeerValor(nameof(PlantillaDeFiltradoDtm.Vista), "");
            if (plantilla.IsNullOrEmpty()) GestorDeErrores.Emitir("Ha de indicar el nombre de la vista");

            var vistaDtm = contexto.Set<VistaMvcDtm>().Where(v => v.Controlador == controlador && v.Accion == accion);
            if (!vistaDtm.Any())
            {
                GestorDeErrores.Emitir($"No se puede guarada el filtro indicado ya que la vista '{controlador}/{accion}' no existe");
            }
            if (vistaDtm.Count() > 1)
            {
                GestorDeErrores.Emitir($"No se puede guarada el filtro indicado ya que la vista '{controlador}/{accion}' está definida más de una vez");
            }

            var valor = parametros.LeerValor<string>(ltrParametrosEp.datosPeticion);
            var filtros = new Dictionary<string, object>
            {
                {nameof(PlantillaDeFiltradoDtm.IdUsuario), contexto.DatosDeConexion.IdUsuario},
                {nameof(PlantillaDeFiltradoDtm.IdNegocio), negocio.IdNegocio() },
                {nameof(PlantillaDeFiltradoDtm.Nombre), plantilla },
                {nameof(PlantillaDeFiltradoDtm.Vista), titulo }
            };
            var plantillaDtm = contexto.SeleccionarPorAk<PlantillaDeFiltradoDtm>(filtros, errorSiNoHay: false);

            if (plantillaDtm is null)
            {
                plantillaDtm = new PlantillaDeFiltradoDtm
                {
                    IdNegocio = negocio.IdNegocio(),
                    IdUsuario = contexto.DatosDeConexion.IdUsuario,
                    Nombre = plantilla,
                    Vista = titulo,
                    Valor = valor,
                    IdVista = vistaDtm.First().Id
                }.Insertar(contexto);
            }
            else if (plantillaDtm.Valor != valor)
            {
                plantillaDtm.Valor = valor;
                plantillaDtm.Modificar(contexto);
            }

            return plantillaDtm.MapearDto<PlantillaDeFiltradoDto>(contexto);
        }

        public static List<MisFiltros> LeerFiltrosDeUsuario(ContextoSe contexto)
        {
            var resultado = new List<MisFiltros>();
            var filtros = contexto.Set<PlantillaDeFiltradoDtm>().Include(f => f.Negocio).Where(f => f.IdUsuario == contexto.DatosDeConexion.IdUsuario && f.IdVista != null).OrderBy(f => f.Negocio.Enumerado).ToList();
            var idsDeNegocios = filtros.Select(f => f.IdNegocio).Distinct();
            foreach (var idNegocio in idsDeNegocios)
            {
                var filtrosPorNegocio = filtros.Where(f => f.IdNegocio == idNegocio).ToList();
                var negocio = filtros.Where(f => f.IdNegocio == idNegocio).Select(f => f.Negocio.Nombre).FirstOrDefault();
                foreach (var filtro in filtrosPorNegocio)
                {
                    var vista = contexto.Set<VistaMvcDtm>().Where(v => v.Id == filtro.IdVista).First();
                    var url = $"{vista.Controlador}/{vista.Accion}";
                    if (vista.Controlador == enumControladoresJuridicos.Contratos.ToString())
                    {
                        if (filtro.Vista == enumTituloDeVistaContratos.Venta.Descripcion())
                            url = $"{url}?clase={enumClaseDeContrato.Venta}";
                        else if (filtro.Vista == enumTituloDeVistaContratos.Compra.Descripcion())
                            url = $"{url}?clase={enumClaseDeContrato.Compra}";
                        else if (filtro.Vista == enumTituloDeVistaContratos.MatriculaDeGuarderia.Descripcion())
                            url = $"{url}?clase={enumClaseDeContrato.MatriculaDeGuarderia}";
                    }

                    resultado.Add(new MisFiltros
                    {
                        Id = filtro.Id,
                        IdVista = filtro.IdVista.Entero(),
                        Negocio = negocio,
                        Enumerado =NegociosDeSe.ToEnumerado(negocio),
                        Nombre = filtro.Nombre,
                        Titulo = filtro.Vista,
                        Url = url
                    });
                }
            }
            return resultado;

        }

    }
}
