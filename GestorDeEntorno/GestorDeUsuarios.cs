using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using ServicioDeDatos.Entorno;
using ServicioDeDatos;
using ServicioDeDatos.Archivos;
using ModeloDeDto.Entorno;

namespace Gestor.Elementos.Entorno
{

    public static partial class Joins
    {
        public static IQueryable<T> AplicarJoinDeArchivo<T>(this IQueryable<T> registros, List<ClausulaDeJoin> joins, ParametrosDeNegocio parametros) where T : UsuarioDtm
        {
            foreach (ClausulaDeJoin join in joins)
            {
                if (join.Dtm == typeof(ArchivoDtm))
                    registros = registros.Include(p => p.Archivo);
            }

            return registros;
        }
    }


    static class FiltrosDeUsuario
    {
        public static IQueryable<T> FiltrarPorNombreCompleto<T>(this IQueryable<T> regristros, List<ClausulaDeFiltrado> filtros) where T : UsuarioDtm
        {
            foreach (ClausulaDeFiltrado filtro in filtros)
                if (filtro.Clausula.ToLower() == UsuariosPor.NombreCompleto)
                    return regristros.Where(x => x.Apellido.Contains(filtro.Valor) || x.Nombre.Contains(filtro.Valor));

            return regristros;
        }

        public static IQueryable<T> FiltrarPorRelacion<T>(this IQueryable<T> registros, List<ClausulaDeFiltrado> filtros) where T : UsuarioDtm
        {
            foreach (ClausulaDeFiltrado filtro in filtros)
                if (filtro.Clausula.ToLower() == UsuariosPor.Permisos)
                {
                    var listaIds = filtro.Valor.ListaEnteros();
                    foreach (int id in listaIds)
                    {
                        registros = registros.Where(u => u.Permisos.Any(up => up.IdPermiso == id && up.IdUsua == u.Id));
                    }
                }

            return registros;
        }
    }

    static class OrdenacioDeUsuarios
    {
        public static IQueryable<UsuarioDtm> Orden(this IQueryable<UsuarioDtm> set, List<ClausulaDeOrdenacion> ordenacion)
        {
            if (ordenacion.Count == 0)
                return set.OrderBy(x => x.Apellido);

            foreach (var orden in ordenacion)
            {
                if (orden.Propiedad == nameof(UsuarioDtm.Apellido).ToLower())
                    return orden.Modo == ModoDeOrdenancion.ascendente
                        ? set.OrderBy(x => x.Apellido)
                        : set.OrderByDescending(x => x.Apellido);

                if (orden.Propiedad == nameof(UsuarioDtm.Login).ToLower())
                    return orden.Modo == ModoDeOrdenancion.ascendente
                        ? set.OrderBy(x => x.Login)
                        : set.OrderByDescending(x => x.Login);

                if (orden.Propiedad == nameof(UsuarioDtm.Alta).ToLower())
                    return orden.Modo == ModoDeOrdenancion.ascendente
                        ? set.OrderBy(x => x.Alta)
                        : set.OrderByDescending(x => x.Alta);
            }

            return set;
        }
    }

    public class GestorDeUsuarios : GestorDeElementos<ContextoSe, UsuarioDtm, UsuarioDto>
    {

        public class MapearUsuario : Profile
        {
            public MapearUsuario()
            {
                CreateMap<UsuarioDtm, UsuarioDto>();
                CreateMap<UsuarioDto, UsuarioDtm>();
            }
        }

        public GestorDeUsuarios(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {

        }

        public static GestorDeUsuarios Gestor(IMapper mapeador)
        {
            var contexto = ContextoSe.ObtenerContexto();
            return (GestorDeUsuarios)CrearGestor<GestorDeUsuarios>(() => new GestorDeUsuarios(contexto, mapeador));
        }

        protected override void DefinirJoins(List<ClausulaDeFiltrado> filtros, List<ClausulaDeJoin> joins, ParametrosDeNegocio parametros)
        {
            base.DefinirJoins(filtros, joins, parametros);
            joins.Add(new ClausulaDeJoin { Dtm = typeof(ArchivoDtm) });
        }

        protected override IQueryable<UsuarioDtm> AplicarJoins(IQueryable<UsuarioDtm> registros, List<ClausulaDeJoin> joins, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, joins, parametros);
            return Joins.AplicarJoinDeArchivo(registros, joins, parametros);
        }

        protected override IQueryable<UsuarioDtm> AplicarOrden(IQueryable<UsuarioDtm> registros, List<ClausulaDeOrdenacion> ordenacion)
        {
            registros = base.AplicarOrden(registros, ordenacion);
            return registros.Orden(ordenacion);
        }

        protected override IQueryable<UsuarioDtm> AplicarFiltros(IQueryable<UsuarioDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarFiltros(registros, filtros, parametros);

            if (HayFiltroPorId(registros))
                return registros;

            return registros.FiltrarPorNombreCompleto(filtros).FiltrarPorRelacion(filtros);
        }

        protected override void AntesMapearRegistroParaInsertar(UsuarioDto usuarioDto, ParametrosDeNegocio opciones)
        {
            base.AntesMapearRegistroParaInsertar(usuarioDto, opciones);
            usuarioDto.Alta = System.DateTime.Now;
            validarDatos(usuarioDto);
        }

        protected override void AntesMapearRegistroParaModificar(UsuarioDto usuarioDto, ParametrosDeNegocio opciones)
        {
            base.AntesMapearRegistroParaModificar(usuarioDto, opciones);
            validarDatos(usuarioDto);
        }

        protected override void AntesDePersistir(UsuarioDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);

            if (registro.IdArchivo == 0 || registro.IdArchivo == null)
            {
                registro.IdArchivo = null;
                registro.Archivo = null;
            }
        }


        private void validarDatos(UsuarioDto usuarioDto)
        {
            if (usuarioDto.Login.IsNullOrEmpty())
                Errores.GestorDeErrores.Emitir("Es necesario indicar el login del usuario");
            if (usuarioDto.Apellido.IsNullOrEmpty())
                Errores.GestorDeErrores.Emitir("Es necesario indicar el apellido del usuario");
            if (usuarioDto.Nombre.IsNullOrEmpty())
                Errores.GestorDeErrores.Emitir("Es necesario indicar el nombre del usuario");
        }

        protected override void DespuesDeMapearElemento(UsuarioDtm registro, UsuarioDto elemento, ParametrosDeMapeo parametros)
        {
            base.DespuesDeMapearElemento(registro, elemento, parametros);
            if (registro.Archivo != null)
            {
                elemento.Foto = Utilidades.DescargarArchivo(registro.Archivo.Id, registro.Archivo.Nombre, registro.Archivo.AlmacenadoEn);
            }
        }


    }


}
