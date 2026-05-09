using System.Collections.Generic;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Negocio;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Negocio;
using Utilidades;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.SistemaDocumental;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDePlantillasDeExportacion : GestorDeElementos<ContextoSe, PlantillaDeExportacionDtm, PlantillaDeExportacionDto>
    {
        public class MapearVariables : Profile
        {
            public MapearVariables()
            {
                CreateMap<PlantillaDeExportacionDtm, PlantillaDeExportacionDto>();

                CreateMap<PlantillaDeExportacionDto, PlantillaDeExportacionDtm>()
                .ForMember(dtm => dtm.Negocio, dto => dto.Ignore());
            }
        }

        public GestorDePlantillasDeExportacion(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }
        internal static GestorDePlantillasDeExportacion Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDePlantillasDeExportacion(contexto, mapeador);
        }

        protected override IQueryable<PlantillaDeExportacionDtm> AplicarJoins(IQueryable<PlantillaDeExportacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta.Include(x => x.Negocio);
            return consulta;
        }

        protected override IQueryable<PlantillaDeExportacionDtm> AplicarSeguridad(IQueryable<PlantillaDeExportacionDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!parametros.ValidarPermisosDePersistencia || Contexto.DatosDeConexion.EsAdministrador)
                return consulta;
            var permisosDeElUsuario = Contexto.Set<UsuariosDeUnPermisoDtm>().Where(permisos => permisos.IdUsuario == Contexto.DatosDeConexion.IdUsuario);
            consulta = consulta.Where(plantilla => permisosDeElUsuario.Any(p => p.IdPermiso == plantilla.IdPermiso));
            return consulta;
        }

        protected override void AntesDePersistir(PlantillaDeExportacionDtm plantilla, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(plantilla, parametros);

            ApiDeEnsamblados.ObtenerMetodoEstatico(plantilla.Dll, plantilla.Clase, plantilla.Metodo);
            if (parametros.Insertando)
                plantilla.IdPermiso = GestorDePermisos.CrearObtener(Contexto, enumNegocio.No_Definido, $"{plantilla.Nombre}", enumClaseDePermiso.Exportacion, enumModoDeAccesoDeDatos.Gestor).Id;
            else if (parametros.Modificando)
                plantilla.IdPermiso = ((PlantillaDeExportacionDtm)parametros.registroEnBd).IdPermiso;
        }

        protected override void DespuesDePersistir(PlantillaDeExportacionDtm plantilla, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(plantilla, parametros);

            if (parametros.Modificando)
                GestorDePermisos.ModificarPermisoDeDatos(Contexto, enumNegocio.No_Definido, plantilla.IdPermiso, plantilla.Nombre, enumClaseDePermiso.Exportacion, enumModoDeAccesoDeDatos.Gestor);

            if (parametros.Eliminando)
                GestorDePermisos.EliminarRegistroPorId(Contexto, plantilla.IdPermiso, parametros: parametros.Parametros);
        }

        protected override void DespuesDeMapearElElemento(PlantillaDeExportacionDtm plantilla, PlantillaDeExportacionDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(plantilla, elemento, parametros);
            elemento.Permiso = Contexto.SeleccionarPorId<PermisoDtm>(plantilla.IdPermiso).Expresion;
        }

        public static string Exportar(ContextoSe contexto,int idNegocio, int idPlantilla, Dictionary<string, object> parametros)
        {
            ServidorDocumental.SometerExportacion(contexto, parametros.ToJson());
            return null;

            //var plantilla = contexto.SeleccionarPorId<PlantillaDeExportacionDtm>(idPlantilla);
            //var accion = new AccionDtm { Nombre = plantilla.Nombre, Metodo = plantilla.Metodo, Dll = plantilla.Dll, Clase = plantilla.Clase, ClaseDeAccion = enumClaseDeAccion.DLL.ToString() };
            //var salida = accion.Ejecutar(contexto, plantilla, NegociosDeSe.ToEnumerado(idNegocio), parametros);

            //return ApiDeArchivos.UrlDeArchivo(ApiDeArchivos.DescargarArchivo(salida[nameof(ObjetoConstructorDeExcel.FicheroConRuta)].ToString()));
            //Creo un archivador
            //Asocio el archivo
            //otorgo permisos al usuario
            //Envío un correo de que se ha hecho
        }
    }
}
