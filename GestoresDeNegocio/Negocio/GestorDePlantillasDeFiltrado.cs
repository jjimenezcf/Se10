using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ServicioDeDatos.Negocio;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Negocio;
using Gestor.Errores;
using Newtonsoft.Json.Linq;
using GestorDeElementos.Extensores;
using Newtonsoft.Json;

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

        public static PlantillaDeFiltradoDto GuardarPlantillasDeFiltrado(ContextoSe contexto, enumNegocio negocio, Dictionary<string, object> parametros)
        {
            var plantilla = parametros.LeerValor(nameof(PlantillaDeFiltradoDto.Plantilla), "").Trim();
            if (plantilla.IsNullOrEmpty()) GestorDeErrores.Emitir("Ha de indicar el nombre de la plantilla a crear");
            var vista = parametros.LeerValor(nameof(PlantillaDeFiltradoDtm.Vista), "");
            if (plantilla.IsNullOrEmpty()) GestorDeErrores.Emitir("Ha de indicar el nombre de la vista");

            var valor = parametros.LeerValor<string>(ltrParametrosEp.datosPeticion);
            var filtros = new Dictionary<string, object>
            {
                {nameof(PlantillaDeFiltradoDtm.IdUsuario), contexto.DatosDeConexion.IdUsuario},
                {nameof(PlantillaDeFiltradoDtm.IdNegocio), negocio.IdNegocio() },
                {nameof(PlantillaDeFiltradoDtm.Nombre), plantilla },
                {nameof(PlantillaDeFiltradoDtm.Vista), vista }
            };
            var plantillaDtm = contexto.SeleccionarPorAk<PlantillaDeFiltradoDtm>(filtros, errorSiNoHay: false);

            if (plantillaDtm is null)
            {
                plantillaDtm = new PlantillaDeFiltradoDtm
                {
                    IdNegocio = negocio.IdNegocio(),
                    IdUsuario = contexto.DatosDeConexion.IdUsuario,
                    Nombre = plantilla,
                    Vista = vista,
                    Valor = valor
                }.Insertar(contexto);
            }
            else if (plantillaDtm.Valor != valor)
            {
                plantillaDtm.Valor = valor;
                plantillaDtm.Modificar(contexto);
            }

            return plantillaDtm.MapearDto<PlantillaDeFiltradoDto>(contexto);
        }
    }
}
