using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using ModeloDeDto;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestoresDeNegocio.Seguridad
{
    /// <summary>
    /// Construye el árbol de seguridad de un usuario: sus puestos de trabajo (con roles, permisos heredados y directos)
    /// y sus permisos agrupados por negocio, tipo, estado, transición, cg y elemento.
    /// Salvo el nodo raíz, los hijos de cada nodo se leen bajo demanda.
    /// </summary>
    public static class ArbolDeSeguridad
    {
        private static readonly enumNodoDeSeguridad[] GruposDePermisos =
        {
            enumNodoDeSeguridad.PermisosPorNegocio,
            enumNodoDeSeguridad.PermisosPorTipo,
            enumNodoDeSeguridad.PermisosPorEstado,
            enumNodoDeSeguridad.PermisosPorTransicion,
            enumNodoDeSeguridad.PermisosPorCg,
            enumNodoDeSeguridad.PermisosPorElemento
        };

        public static NodoDeSeguridadDto LeerUsuario(ContextoSe contexto, int idUsuario)
        {
            var usuario = contexto.SeleccionarPorId<UsuarioDtm>(idUsuario);
            var clave = $"usuario-{idUsuario}";

            var raiz = new NodoDeSeguridadDto
            {
                Clave = clave,
                Tipo = enumNodoDeSeguridad.Usuario.ToString(),
                IdUsuario = idUsuario,
                Nombre = usuario.Expresion,
                Icono = Icono(enumNodoDeSeguridad.Usuario),
                TieneHijos = true,
                Hijos = new List<NodoDeSeguridadDto>()
            };

            raiz.Hijos.Add(Grupo(clave, enumNodoDeSeguridad.Puestos, idUsuario, 0
                , contexto.Set<PuestosDeUnUsuarioDtm>().Count(x => x.IdUsuario == idUsuario)));

            foreach (var grupo in GruposDePermisos)
                raiz.Hijos.Add(Grupo(clave, grupo, idUsuario, 0, IdsDeNegocio(contexto, grupo, idUsuario).Count()));

            return raiz;
        }

        public static List<NodoDeSeguridadDto> LeerHijos(ContextoSe contexto, IMapper mapeador, string clavePadre, enumNodoDeSeguridad tipo, int idUsuario, int id, int idNegocio, string clase)
        {
            switch (tipo)
            {
                case enumNodoDeSeguridad.Puestos:
                    return PuestosDelUsuario(contexto, mapeador, clavePadre, idUsuario);
                case enumNodoDeSeguridad.Puesto:
                    return GruposDelPuesto(contexto, clavePadre, idUsuario, idPuesto: id);
                case enumNodoDeSeguridad.RolesDelPuesto:
                    return RolesDelPuesto(contexto, mapeador, clavePadre, idUsuario, idPuesto: id);
                case enumNodoDeSeguridad.Rol:
                    return PermisosDelRol(contexto, mapeador, clavePadre, idUsuario, idRol: id);
                case enumNodoDeSeguridad.PermisosHeredados:
                    return PermisosHeredados(contexto, mapeador, clavePadre, idUsuario, idPuesto: id);
                case enumNodoDeSeguridad.PermisosDirectos:
                    return PermisosDirectos(contexto, mapeador, clavePadre, idUsuario, idPuesto: id);
                case enumNodoDeSeguridad.NegocioConPermisos:
                    return PermisosDelNegocio(contexto, mapeador, clavePadre, ApiDeEnsamblados.ToEnumerado<enumNodoDeSeguridad>(clase), idUsuario, idNegocio);
            }

            if (GruposDePermisos.Contains(tipo))
                return NegociosConPermisos(contexto, clavePadre, tipo, idUsuario);

            return new List<NodoDeSeguridadDto>();
        }

        public static ElementoDto LeerObjeto(ContextoSe contexto, IMapper mapeador, enumObjetoDeSeguridad objeto, int id)
        {
            // sin caché: al recargar el árbol se han de ver los cambios hechos en el puesto, rol o permiso
            var parametros = new Dictionary<string, object> { { ltrParametrosNeg.UsarLaCache, false } };
            switch (objeto)
            {
                case enumObjetoDeSeguridad.Puesto:
                    return GestorDePuestosDeTrabajo.Gestor(contexto, mapeador).LeerElementoPorId(id, parametros);
                case enumObjetoDeSeguridad.Rol:
                    return GestorDeRoles.Gestor(contexto, mapeador).LeerElementoPorId(id, parametros);
                case enumObjetoDeSeguridad.Permiso:
                    return GestorDePermisos.Gestor(contexto, mapeador).LeerElementoPorId(id, parametros);
            }

            throw new Exception($"No se ha definido cómo leer el objeto de seguridad '{objeto}'");
        }

        private static string Titulo(enumNodoDeSeguridad tipo)
        {
            switch (tipo)
            {
                case enumNodoDeSeguridad.Puestos: return "Puestos de trabajo";
                case enumNodoDeSeguridad.RolesDelPuesto: return "Roles";
                case enumNodoDeSeguridad.PermisosHeredados: return "Permisos heredados";
                case enumNodoDeSeguridad.PermisosDirectos: return "Permisos directos";
                case enumNodoDeSeguridad.PermisosPorNegocio: return "Permisos por negocio";
                case enumNodoDeSeguridad.PermisosPorTipo: return "Permisos por tipos";
                case enumNodoDeSeguridad.PermisosPorEstado: return "Permisos por estados";
                case enumNodoDeSeguridad.PermisosPorTransicion: return "Permisos por transiciones";
                case enumNodoDeSeguridad.PermisosPorCg: return "Permisos por cg";
                case enumNodoDeSeguridad.PermisosPorElemento: return "Permisos por elementos";
            }
            return tipo.ToString();
        }

        // los mismos iconos que usan esas opciones en el menú
        private static string Icono(enumNodoDeSeguridad tipo)
        {
            switch (tipo)
            {
                case enumNodoDeSeguridad.Usuario: return "usuario.svg";
                case enumNodoDeSeguridad.Puestos: return "puestoDeTrabajo.svg";
                case enumNodoDeSeguridad.RolesDelPuesto: return "roles.svg";
                case enumNodoDeSeguridad.PermisosHeredados: return "acceso.svg";
                case enumNodoDeSeguridad.PermisosDirectos: return "acceso.svg";
                case enumNodoDeSeguridad.PermisosPorNegocio: return "bars-solid.svg";
                case enumNodoDeSeguridad.PermisosPorTipo: return "TiposDeRegistro.svg";
                case enumNodoDeSeguridad.PermisosPorEstado: return "Estados.svg";
                case enumNodoDeSeguridad.PermisosPorTransicion: return "Transiciones.svg";
                case enumNodoDeSeguridad.PermisosPorCg: return "CentroGestor.svg";
                case enumNodoDeSeguridad.PermisosPorElemento: return "acceso.svg";
            }
            return null;
        }

        private static NodoDeSeguridadDto Grupo(string clavePadre, enumNodoDeSeguridad tipo, int idUsuario, int id, int cantidad)
        {
            return new NodoDeSeguridadDto
            {
                Clave = $"{clavePadre}.{tipo}",
                Tipo = tipo.ToString(),
                Id = id,
                IdUsuario = idUsuario,
                Nombre = Titulo(tipo),
                Icono = Icono(tipo),
                Cantidad = cantidad,
                TieneHijos = cantidad > 0
            };
        }

        // la posición forma parte de la clave porque un mismo permiso puede repetirse bajo el mismo nodo (p.e. en varios cg de un negocio)
        private static NodoDeSeguridadDto Permiso(string clavePadre, int posicion, int idUsuario, int idPermiso, string nombre, string ayuda = null, bool heredado = false)
        {
            return new NodoDeSeguridadDto
            {
                Clave = $"{clavePadre}.{posicion}-permiso-{idPermiso}",
                Tipo = enumNodoDeSeguridad.Permiso.ToString(),
                Objeto = enumObjetoDeSeguridad.Permiso.ToString(),
                Id = idPermiso,
                IdUsuario = idUsuario,
                Nombre = nombre,
                Ayuda = ayuda,
                Heredado = heredado,
                TieneHijos = false
            };
        }

        private static List<ClausulaDeFiltrado> FiltrarPor(string propiedad, int valor, List<ClausulaDeFiltrado> filtros = null)
        {
            if (filtros == null) filtros = new List<ClausulaDeFiltrado>();
            filtros.Add(new ClausulaDeFiltrado(propiedad, enumCriteriosDeFiltrado.igual, valor.ToString()));
            return filtros;
        }

        private static IEnumerable<TElemento> LeerTodos<TRegistro, TElemento>(GestorDeElementos<ContextoSe, TRegistro, TElemento> gestor, List<ClausulaDeFiltrado> filtros)
        where TRegistro : RegistroDtm
        where TElemento : ElementoDto
        {
            return gestor.LeerElementos(0, -1, filtros, null, new Dictionary<string, object>());
        }

        private static List<NodoDeSeguridadDto> PuestosDelUsuario(ContextoSe contexto, IMapper mapeador, string clavePadre, int idUsuario)
        {
            var puestos = LeerTodos(GestorDePuestosDeUnUsuario.Gestor(contexto, mapeador), FiltrarPor(nameof(PuestosDeUnUsuarioDtm.IdUsuario), idUsuario));

            return puestos
                .OrderBy(p => p.CgDelPuesto).ThenBy(p => p.Puesto)
                .Select(p => new NodoDeSeguridadDto
                {
                    Clave = $"{clavePadre}.puesto-{p.IdPuesto}",
                    Tipo = enumNodoDeSeguridad.Puesto.ToString(),
                    Objeto = enumObjetoDeSeguridad.Puesto.ToString(),
                    Id = p.IdPuesto,
                    IdUsuario = idUsuario,
                    Nombre = p.CgDelPuesto.IsNullOrEmpty() ? p.Puesto : $"{p.CgDelPuesto}: {p.Puesto}",
                    Ayuda = p.RolesDeUnPuesto,
                    TieneHijos = true
                })
                .ToList();
        }

        private static List<NodoDeSeguridadDto> GruposDelPuesto(ContextoSe contexto, string clavePadre, int idUsuario, int idPuesto)
        {
            return new List<NodoDeSeguridadDto>
            {
                Grupo(clavePadre, enumNodoDeSeguridad.RolesDelPuesto, idUsuario, idPuesto, contexto.Set<RolesDeUnPuestoDtm>().Count(x => x.IdPuesto == idPuesto)),
                Grupo(clavePadre, enumNodoDeSeguridad.PermisosHeredados, idUsuario, idPuesto, contexto.Set<PermisosHeredadosDtm>().Count(x => x.IdPuesto == idPuesto)),
                Grupo(clavePadre, enumNodoDeSeguridad.PermisosDirectos, idUsuario, idPuesto, contexto.Set<PermisosDirectosDtm>().Count(x => x.IdPuesto == idPuesto))
            };
        }

        private static List<NodoDeSeguridadDto> RolesDelPuesto(ContextoSe contexto, IMapper mapeador, string clavePadre, int idUsuario, int idPuesto)
        {
            var roles = LeerTodos(GestorDeRolesDeUnPuesto.Gestor(contexto, mapeador), FiltrarPor(nameof(RolesDeUnPuestoDtm.IdPuesto), idPuesto)).ToList();

            var idsDeRoles = roles.Select(r => r.IdRol).ToList();
            var permisosPorRol = contexto.Set<PermisosDeUnRolDtm>()
                .Where(x => idsDeRoles.Contains(x.IdRol))
                .GroupBy(x => x.IdRol)
                .Select(g => new { IdRol = g.Key, Cantidad = g.Count() })
                .ToDictionary(x => x.IdRol, x => x.Cantidad);

            return roles
                .OrderBy(r => r.Rol)
                .Select(r =>
                {
                    var cantidad = permisosPorRol.ContainsKey(r.IdRol) ? permisosPorRol[r.IdRol] : 0;
                    return new NodoDeSeguridadDto
                    {
                        Clave = $"{clavePadre}.rol-{r.IdRol}",
                        Tipo = enumNodoDeSeguridad.Rol.ToString(),
                        Objeto = enumObjetoDeSeguridad.Rol.ToString(),
                        Id = r.IdRol,
                        IdUsuario = idUsuario,
                        Nombre = r.Rol,
                        Cantidad = cantidad,
                        TieneHijos = cantidad > 0
                    };
                })
                .ToList();
        }

        private static List<NodoDeSeguridadDto> PermisosDelRol(ContextoSe contexto, IMapper mapeador, string clavePadre, int idUsuario, int idRol)
        {
            var permisos = LeerTodos(GestorDePermisosDeUnRol.Gestor(contexto, mapeador), FiltrarPor(nameof(PermisosDeUnRolDtm.IdRol), idRol));
            return permisos
                .OrderBy(p => p.Permiso)
                .Select((p, i) => Permiso(clavePadre, i, idUsuario, p.IdPermiso, p.Permiso))
                .ToList();
        }

        private static List<NodoDeSeguridadDto> PermisosHeredados(ContextoSe contexto, IMapper mapeador, string clavePadre, int idUsuario, int idPuesto)
        {
            var permisos = LeerTodos(new GestorDePermisosHeredados(contexto, mapeador), FiltrarPor(nameof(PermisosHeredadosDtm.IdPuesto), idPuesto));
            return permisos
                .OrderBy(p => p.Permiso)
                .Select((p, i) => Permiso(clavePadre, i, idUsuario, p.IdPermiso, p.Permiso, p.Roles.IsNullOrEmpty() ? null : $"Heredado de: {p.Roles}"))
                .ToList();
        }

        private static List<NodoDeSeguridadDto> PermisosDirectos(ContextoSe contexto, IMapper mapeador, string clavePadre, int idUsuario, int idPuesto)
        {
            var permisos = LeerTodos(GestorDePermisosDirectos.Gestor(contexto, mapeador), FiltrarPor(nameof(PermisosDirectosDtm.IdPuesto), idPuesto));
            return permisos
                .OrderBy(p => p.Permiso)
                .Select((p, i) => Permiso(clavePadre, i, idUsuario, p.IdPermiso, p.Permiso))
                .ToList();
        }

        private static IQueryable<int> IdsDeNegocio(ContextoSe contexto, enumNodoDeSeguridad grupo, int idUsuario)
        {
            switch (grupo)
            {
                case enumNodoDeSeguridad.PermisosPorNegocio: return contexto.Set<PermisosPorNegocioDtm>().Where(x => x.IdUsuario == idUsuario).Select(x => x.IdNegocio);
                case enumNodoDeSeguridad.PermisosPorTipo: return contexto.Set<PermisosPorTipoDtm>().Where(x => x.IdUsuario == idUsuario).Select(x => x.IdNegocio);
                case enumNodoDeSeguridad.PermisosPorEstado: return contexto.Set<PermisosPorEstadoDtm>().Where(x => x.IdUsuario == idUsuario).Select(x => x.IdNegocio);
                case enumNodoDeSeguridad.PermisosPorTransicion: return contexto.Set<PermisosPorTransicionDtm>().Where(x => x.IdUsuario == idUsuario).Select(x => x.IdNegocio);
                case enumNodoDeSeguridad.PermisosPorCg: return contexto.Set<PermisosPorCgDtm>().Where(x => x.IdUsuario == idUsuario).Select(x => x.IdNegocio);
                case enumNodoDeSeguridad.PermisosPorElemento: return contexto.Set<PermisosPorElementoDtm>().Where(x => x.IdUsuario == idUsuario).Select(x => x.IdNegocio);
            }

            throw new Exception($"El nodo '{grupo}' no es un grupo de permisos de un usuario");
        }

        private static List<NodoDeSeguridadDto> NegociosConPermisos(ContextoSe contexto, string clavePadre, enumNodoDeSeguridad grupo, int idUsuario)
        {
            var negocios = IdsDeNegocio(contexto, grupo, idUsuario)
                .GroupBy(idNegocio => idNegocio)
                .Select(g => new { IdNegocio = g.Key, Cantidad = g.Count() })
                .ToList();

            return negocios
                .Select(n => new NodoDeSeguridadDto
                {
                    Clave = $"{clavePadre}.negocio-{n.IdNegocio}",
                    Tipo = enumNodoDeSeguridad.NegocioConPermisos.ToString(),
                    Clase = grupo.ToString(),
                    IdUsuario = idUsuario,
                    IdNegocio = n.IdNegocio,
                    Nombre = NegociosDeSe.LeerNegocioPorId(n.IdNegocio).Nombre,
                    Cantidad = n.Cantidad,
                    TieneHijos = n.Cantidad > 0
                })
                .OrderBy(n => n.Nombre)
                .ToList();
        }

        private static List<NodoDeSeguridadDto> PermisosDelNegocio(ContextoSe contexto, IMapper mapeador, string clavePadre, enumNodoDeSeguridad grupo, int idUsuario, int idNegocio)
        {
            // las seis tablas de permisos otorgados usan los mismos nombres de propiedad para el negocio y el usuario.
            // El gestor ignora el filtro por negocio cuando es el del usuario (lo trata como restrictor), por eso se vuelve a filtrar en memoria
            var filtros = FiltrarPor(nameof(PermisosPorNegocioDtm.IdNegocio), idNegocio, FiltrarPor(nameof(PermisosPorNegocioDtm.IdUsuario), idUsuario));

            // (id del permiso, detalle del objeto al que se aplica, permiso, heredado)
            IEnumerable<(int IdPermiso, string Detalle, string Permiso, bool Heredado)> permisos;
            switch (grupo)
            {
                case enumNodoDeSeguridad.PermisosPorNegocio:
                    permisos = LeerTodos(GestorDePermisosPorNegocio.Gestor(contexto, mapeador), filtros)
                        .Where(p => p.IdNegocio == idNegocio).Select(p => (p.IdPermiso, (string)null, p.Permiso, p.Calculado));
                    break;
                case enumNodoDeSeguridad.PermisosPorTipo:
                    permisos = LeerTodos(GestorDePermisosPorTipo.Gestor(contexto, mapeador), filtros)
                        .Where(p => p.IdNegocio == idNegocio).Select(p => (p.IdPermiso, p.Tipo, p.Permiso, p.Calculado));
                    break;
                case enumNodoDeSeguridad.PermisosPorEstado:
                    permisos = LeerTodos(GestorDePermisosPorEstado.Gestor(contexto, mapeador), filtros)
                        .Where(p => p.IdNegocio == idNegocio).Select(p => (p.IdPermiso, p.Estado, p.Permiso, p.Calculado));
                    break;
                case enumNodoDeSeguridad.PermisosPorTransicion:
                    permisos = LeerTodos(GestorDePermisosPorTransicion.Gestor(contexto, mapeador), filtros)
                        .Where(p => p.IdNegocio == idNegocio).Select(p => (p.IdPermiso, p.Transicion, p.Permiso, p.Calculado));
                    break;
                case enumNodoDeSeguridad.PermisosPorCg:
                    permisos = LeerTodos(GestorDePermisosPorCg.Gestor(contexto, mapeador), filtros)
                        .Where(p => p.IdNegocio == idNegocio).Select(p => (p.IdPermiso, p.Cg, p.Permiso, p.Calculado));
                    break;
                case enumNodoDeSeguridad.PermisosPorElemento:
                    permisos = LeerTodos(GestorDePermisosPorElemento.Gestor(contexto, mapeador), filtros)
                        .Where(p => p.IdNegocio == idNegocio).Select(p => (p.IdPermiso, p.Elemento, p.Permiso, p.Calculado));
                    break;
                default:
                    GestorDeErrores.Emitir($"El nodo '{grupo}' no es un grupo de permisos de un usuario");
                    return null;
            }

            return permisos
                .OrderBy(p => p.Detalle).ThenBy(p => p.Permiso)
                .Select((p, i) => Permiso(clavePadre
                    , i
                    , idUsuario
                    , p.IdPermiso
                    , p.Detalle.IsNullOrEmpty() ? p.Permiso : $"{p.Detalle}: {p.Permiso}"
                    , p.Heredado ? "Permiso heredado de un puesto de trabajo" : "Permiso asignado directamente al usuario"
                    , p.Heredado))
                .ToList();
        }
    }
}
