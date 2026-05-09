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

namespace GestoresDeNegocio.Negocio
{

    public class GestorDePlantillasDeCreacion : GestorDeElementos<ContextoSe, PlantillaDeCreacionDtm, PlantillaDeCreacionDto>
    {
        public class MapearPlantillasDeCreacion : Profile
        {
            public MapearPlantillasDeCreacion()
            {
                CreateMap<PlantillaDeCreacionDtm, PlantillaDeCreacionDto>()
                .ForMember(dto => dto.Plantilla, dtm => dtm.MapFrom(dtm => dtm.Nombre));

                CreateMap<PlantillaDeCreacionDto, PlantillaDeCreacionDtm>()
                .ForMember(dtm => dtm.Usuario, dto => dto.Ignore())
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDePlantillasDeCreacion(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDePlantillasDeCreacion Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlantillasDeCreacion(contexto, mapeador);
        }


        protected override IQueryable<PlantillaDeCreacionDtm> AplicarJoins(IQueryable<PlantillaDeCreacionDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.Negocio);
            return registros;
        }

        protected override void AntesDePersistir(PlantillaDeCreacionDtm parametro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(parametro, parametros);
        }

        protected override void DespuesDePersistir(PlantillaDeCreacionDtm parametro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(parametro, parametros);
            ServicioDeCaches.EliminarCachesDeDescriptores();
        }

        protected override void DespuesDeMapearElElemento(PlantillaDeCreacionDtm parametro, PlantillaDeCreacionDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(parametro, elemento, parametros);
        }

        public static PlantillaDeCreacionDto GuardarPlantillasDeCreacion(ContextoSe contexto, enumNegocio negocio, Dictionary<string, object> parametros)
        {
            var plantilla = parametros.LeerValor<string>(nameof(PlantillaDeCreacionDto.Plantilla), "").Trim();
            if (plantilla.IsNullOrEmpty()) GestorDeErrores.Emitir("Ha de indicar el nombre de la plantilla a crear");
            var vista = parametros.LeerValor(nameof(PlantillaDeCreacionDtm.Vista), "");
            if (plantilla.IsNullOrEmpty()) GestorDeErrores.Emitir("Ha de indicar el nombre de la vista");

            //si parámetros está vacio emito excepción
            JObject datosPorDefecto = parametros.LeerValor<JObject>(ltrParametrosEp.datosPeticion);
            var valor = datosPorDefecto.ToString();
            if (valor.IsNullOrEmpty()) GestorDeErrores.Emitir($"Ha de indicar algún valor a salvar en la plantilla '{plantilla}'");

            //busco por negocio, usuario, y plantilla si es nulo lo creo si no lo modifico
            var filtros = new Dictionary<string, object>
            {
                {nameof(PlantillaDeCreacionDtm.IdUsuario), contexto.DatosDeConexion.IdUsuario},
                {nameof(PlantillaDeCreacionDtm.IdNegocio), negocio.IdNegocio() },
                {nameof(PlantillaDeCreacionDtm.Nombre), plantilla },
                {nameof(PlantillaDeCreacionDtm.Vista), vista }
            };
            var plantillaDtm = contexto.SeleccionarPorAk<PlantillaDeCreacionDtm>(filtros, errorSiNoHay: false);

            if (plantillaDtm is null)
            {
                plantillaDtm = new PlantillaDeCreacionDtm
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

            return plantillaDtm.MapearDto<PlantillaDeCreacionDto>(contexto);
        }
    }
}
