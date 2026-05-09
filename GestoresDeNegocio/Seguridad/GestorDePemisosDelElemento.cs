using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Negocio;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ModeloDeDto.Seguridad;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;


namespace GestoresDeNegocio.Seguridad
{
    public class GestorDePemisosDelElemento : GestorDeElementos<ContextoSe, PermisosDelElementoDtm, PermisosDelElementoDto>
    {
        public string _Tabla { get; }

        private enumNegocio _Negocio;

        public override enumNegocio Negocio => _Negocio;

        private IGestor GestorDelElemento => NegociosDeSe.CrearGestor(Contexto, Negocio);


        public class ltrPermisosDelElemento
        {
        }

        public class MapearPermisosDelElemento : Profile
        {
            public MapearPermisosDelElemento()
            {
                CreateMap<PermisosDelElementoDtm, PermisosDelElementoDto>();
                CreateMap<PermisosDelElementoDto, PermisosDelElementoDtm>();
            }
        }

        public GestorDePemisosDelElemento(ContextoSe contexto, enumNegocio negocio)
        : base(contexto, contexto.Mapeador)
        {
            _Tabla = negocio.TablaDePermisos();
            _Negocio = negocio;
            Contexto = contexto;
        }

        public static GestorDePemisosDelElemento Gestor(ContextoSe contexto, enumNegocio negocio)
        =>
        new GestorDePemisosDelElemento(contexto, negocio);

        public static PermisosDelElementoDtm CrearPermisosDelElemento(ContextoSe contexto, enumNegocio negocio, int idDelElemento)
        {
            var idUsuario = contexto.DatosDeConexion.IdUsuario;
            contexto.AsignarUsuario(contexto.Administrador());
            try
            {
                var permisos = new GestorDePemisosDelElemento(contexto, negocio).PersistirRegistro(new PermisosDelElementoDtm { IdElemento = idDelElemento }, new ParametrosDeNegocio(enumTipoOperacion.Insertar, new Dictionary<string, object>
                     {
                         {ltrParametrosNeg.CrearPermisosDelElemento, true },
                         {nameof(PermisosDelElementoDtm.Elemento), negocio.LeerRegistro(contexto, idDelElemento) }
                     }));
                return permisos;
            }
            finally
            {
                contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
            }
        }

        public PermisosDelElementoDtm LeerRegistroPorIdElemento(int idElemento, bool errorSiNohay)
        =>
        PermisosDelElementoSql.LeerPorIdElemento(Contexto, _Tabla, NegociosDeSe.TipoDtm(Negocio), idElemento, errorSiNohay);

        public override PermisosDelElementoDtm LeerRegistroPorId(int idElemento, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, Dictionary<string, object> parametros = null)
        {
            var permisos = LeerRegistroPorIdElemento(idElemento, errorSiNohay: false);
            if (permisos == null)
            {
                var permisoDto = new PermisosDelElementoDto { IdNegocio = NegociosDeSe.IdNegocio(Negocio), IdElemento = idElemento };
                PersistirElementoDto(permisoDto, new ParametrosDeNegocio(enumTipoOperacion.Insertar) { ValidarPermisosDePersistencia = false });
                return LeerRegistroPorIdElemento(idElemento, errorSiNohay: true);
            }
            return permisos;
        }

        protected override void ValidarPermisosDePersistencia(PermisosDelElementoDtm registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Parametros.LeerValor(ltrParametrosNeg.CrearPermisosDelElemento, false)) return;

            base.ValidarPermisosDePersistencia(registro, parametros);

            parametros.Parametros[nameof(PermisosDelElementoDtm.Elemento)] = NegociosDeSe.CrearGestor(Contexto, Negocio).LeerRegistroPorId(registro.IdElemento, false);

            var elemento = parametros.Parametros[nameof(PermisosDelElementoDtm.Elemento)];

            if (Negocio == enumNegocio.Sociedad && !Contexto.DatosDeConexion.EsAdministrador)
                GestorDeErrores.Emitir($"Los permisos de una sociedad sólo los puede crear el administrador");

            if (Contexto.DatosDeConexion.EsAdministrador)
                return;

            if (((IElementoDtm)elemento).IdUsuaCrea != Contexto.DatosDeConexion.IdUsuario)
                GestorDeErrores.Emitir($"Los permisos de un elemento del negocio {NegociosDeSe.ToNombre(Negocio)} sólo los puede crear el creador del elemento o administrador");

        }

        protected override void AntesDePersistir(PermisosDelElementoDtm registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Parametros.LeerValor(ltrParametrosNeg.CrearPermisosDelElemento, false)) return;
            base.AntesDePersistir(registro, parametros);

            if (parametros.Operacion != enumTipoOperacion.Insertar)
                GestorDeErrores.Emitir($"Los permisos de elemento sólo se pueden crear");

            if (!ApiDeInterfaceDtm.ImplementaTieneReferencia(parametros.Parametros[nameof(PermisosDelElementoDtm.Elemento)].GetType()))
                GestorDeErrores.Emitir($"Los permisos de elemento deben implementar la interface {nameof(ITieneReferencia)}");
        }

        protected override void Persistir(PermisosDelElementoDtm registro, ParametrosDeNegocio parametros)
        {
            var nombrePermiso = ((ITieneReferencia)parametros.Parametros[nameof(PermisosDelElementoDtm.Elemento)]).Referencia;

            int idAdministrador = GestorDePermisos.CrearObtener(Contexto, Negocio, nombrePermiso, enumClaseDePermiso.Elemento, enumModoDeAccesoDeDatos.Administrador).Id;
            int idGestor = GestorDePermisos.CrearObtener(Contexto, Negocio, nombrePermiso, enumClaseDePermiso.Elemento, enumModoDeAccesoDeDatos.Gestor).Id;
            int idConsultor = GestorDePermisos.CrearObtener(Contexto, Negocio, nombrePermiso, enumClaseDePermiso.Elemento, enumModoDeAccesoDeDatos.Consultor).Id;

            registro.Id = PermisosDelElementoSql.Insertar(Contexto, _Tabla, registro.IdElemento, idAdministrador, idGestor, idConsultor).Id;
            registro.IdAdministrador = idAdministrador;
            registro.IdConsultor = idConsultor;
            registro.IdGestor = idGestor;
        }

        protected override void DespuesDeMapearElElemento(PermisosDelElementoDtm registro, PermisosDelElementoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.IdNegocio = NegociosDeSe.IdNegocio(Negocio);
            elemento.Negocio = NegociosDeSe.ToNombre(Negocio);
            elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
        }

        public static void OtorgarPermisoDe(ContextoSe contexto, enumNegocio negocio, int idElemento, IEnumerable<int> idsDeUsuarios, enumModoDeAccesoDeDatos modo)
        {
            var permiso = Gestor(contexto, negocio).LeerRegistroPorId(idElemento, usarLaCache: true, traqueado: false, conBloqueo: false, aplicarJoin: false);
            foreach (var idUsuario in idsDeUsuarios)
                ExtensorDeUsuarios.OtorgarPermisoDe(contexto, idUsuario, negocio, idElemento, permiso, modo);
        }

        public static void QuitarPermisos(ContextoSe contexto, enumNegocio negocio, int idElemento, IEnumerable<int> idsDeUsuarios)
        {
            var permiso = Gestor(contexto, negocio).LeerRegistroPorId(idElemento, usarLaCache: true, traqueado: false, conBloqueo: false, aplicarJoin: false);
            foreach (var idUsuario in idsDeUsuarios)
                ExtensorDeUsuarios.QuitarPermisos(contexto, idUsuario, negocio, idElemento, permiso);
        }

        public static void GenerarSeguridadParaElUsuario(ContextoSe contexto, int idUsuario, Action<string> logger = null)
        {

            PermisosPorElementoSql.QuitarPermisos(contexto, idUsuario, calculado: true);

            var permisosPorNegocio = contexto.Set<UsuarioDtm>()
                     .Join(contexto.Set<PuestosDeUnUsuarioDtm>(), u => u.Id, up => up.IdUsuario, (u, up) => new { Usuario = u, Usu_Puesto = up })
                     .Join(contexto.Set<RolesDeUnPuestoDtm>(), up => up.Usu_Puesto.IdPuesto, rp => rp.IdPuesto, (up, rp) => new { up.Usuario, up.Usu_Puesto, Rol_Puesto = rp })
                     .Join(contexto.Set<PermisosDeUnRolDtm>(), rp => rp.Rol_Puesto.IdRol, rp2 => rp2.IdRol, (rp, rp2) => new { rp.Usuario, rp.Usu_Puesto, rp.Rol_Puesto, Rol_Permiso = rp2 })
                     .Join(contexto.Set<PermisoDtm>(), rp2 => rp2.Rol_Permiso.IdPermiso, p => p.Id, (rp2, p) => new { rp2.Usuario, rp2.Usu_Puesto, rp2.Rol_Puesto, rp2.Rol_Permiso, Permiso = p })
                     .Join(contexto.Set<TipoPermisoDtm>(), p => p.Permiso.IdTipo, tp => tp.Id, (p, tp) => new { p.Usuario, p.Usu_Puesto, p.Rol_Puesto, p.Rol_Permiso, p.Permiso, Tipo_Permiso = tp })
                     .Join(contexto.Set<ClasePermisoDtm>(), p => p.Permiso.IdClase, cp => cp.Id, (p, cp) => new { p.Usuario, p.Usu_Puesto, p.Rol_Puesto, p.Rol_Permiso, p.Permiso, p.Tipo_Permiso, Clase_Permiso = cp })
                     .Where(x => x.Clase_Permiso.Nombre.Equals(enumClaseDePermiso.Elemento.ToString()) && x.Usuario.Id == idUsuario)
                     .OrderBy(x => x.Usuario.Id)
                     .ThenBy(x => x.Permiso.Nombre)
                     .Select(x => new
                     {
                         IdPermiso = x.Rol_Permiso.IdPermiso,
                         Nombre = x.Permiso.Nombre,
                         Tipo = x.Tipo_Permiso.Nombre
                     });

            foreach (var permiso in permisosPorNegocio)
            {
                var negocio = enumNegocio.Parse<enumNegocio>(permiso.Nombre.Split(".")[0], true);
                var permisoElemento = PermisosDelElementoSql.LeerPermisoPorIdPermiso(contexto, negocio.TablaDePermisos(), permiso.IdPermiso, errorSiNoHay: false);
                if (permisoElemento == null) continue;

                var tipoPermiso = enumModoDeAccesoDeDatos.Parse<enumModoDeAccesoDeDatos>(permiso.Tipo, true);
                if (tipoPermiso == enumModoDeAccesoDeDatos.Administrador)
                {
                    PermisosPorElementoSql.Insertar(contexto, negocio.IdNegocio(), permisoElemento.IdElemento, permisoElemento.IdAdministrador, idUsuario,calculado: true);
                    PermisosPorElementoSql.Insertar(contexto, negocio.IdNegocio(), permisoElemento.IdElemento, permisoElemento.IdGestor, idUsuario,calculado: true);
                    PermisosPorElementoSql.Insertar(contexto, negocio.IdNegocio(), permisoElemento.IdElemento, permisoElemento.IdConsultor, idUsuario,calculado: true);
                }
                else if (tipoPermiso == enumModoDeAccesoDeDatos.Gestor)
                {
                    PermisosPorElementoSql.Insertar(contexto, negocio.IdNegocio(), permisoElemento.IdElemento, permisoElemento.IdGestor, idUsuario,calculado: true);
                    PermisosPorElementoSql.Insertar(contexto, negocio.IdNegocio(), permisoElemento.IdElemento, permisoElemento.IdConsultor, idUsuario,calculado: true);
                }
                else
                {
                    PermisosPorElementoSql.Insertar(contexto, negocio.IdNegocio(), permisoElemento.IdElemento, permisoElemento.IdConsultor, idUsuario,calculado: true);
                }
            }
        }

        public static void GenerarSeguridad(ContextoSe contexto, Action<string> logger = null)
        {
            var filtros = new List<ClausulaDeFiltrado>();
            var gNegocios = GestorDeNegocios.Gestor(contexto, contexto.Mapeador);
            var negocios = gNegocios.LeerRegistros(0, -1, filtros);
            PermisosPorElementoSql.EliminarTodos(contexto);
            foreach (var negocio in negocios)
            {
                var enumNegocio = NegociosDeSe.ToEnumerado(negocio.Id);
                if (!NegociosDeSe.UsaPermisosPorElemento(enumNegocio))
                    continue;
                var tipoDeElemento = NegociosDeSe.TipoDtm(enumNegocio);
                var tablaDePermisos = ApiDeElementoDtm.TablaDePermisos(tipoDeElemento);
                var permisosDeElementos = PermisosDelElementoSql.LeerElementosConSeguridad(contexto, tablaDePermisos);
                if (permisosDeElementos.Count == 0)
                    continue;

                logger?.Invoke($"Asignar permisos de los elementos de negocio {enumNegocio}");

                foreach (var permisos in permisosDeElementos)
                    OtorgarSeguridadPorElemento(contexto, negocio.Id, permisos);
            }
        }

        private static void OtorgarSeguridadPorElemento(ContextoSe contexto, int idNegocio, PermisosDelElementoDtm permiso)
        {
            var gestorDePermisosDeUsuario = GestorDePermisosDeUnUsuario.Gestor(contexto, contexto.Mapeador);

            var filtro = new ClausulaDeFiltrado(nameof(PermisosDeUnRolDtm.IdPermiso), enumCriteriosDeFiltrado.igual, permiso.IdConsultor.ToString());
            var usuariosConElPermiso = gestorDePermisosDeUsuario.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });
            foreach (var usuarioConPermiso in usuariosConElPermiso)
                PermisosPorElementoSql.Insertar(contexto, idNegocio, permiso.IdElemento, permiso.IdConsultor, usuarioConPermiso.IdUsuario, calculado: true);

            filtro.Valor = permiso.IdGestor.ToString();
            usuariosConElPermiso = gestorDePermisosDeUsuario.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });
            foreach (var usuarioConPermiso in usuariosConElPermiso)
            {
                PermisosPorElementoSql.Insertar(contexto, idNegocio, permiso.IdElemento, permiso.IdGestor, usuarioConPermiso.IdUsuario, calculado: true);
                PermisosPorElementoSql.Insertar(contexto, idNegocio, permiso.IdElemento, permiso.IdConsultor, usuarioConPermiso.IdUsuario, calculado: true);
            }

            filtro.Valor = permiso.IdAdministrador.ToString();
            usuariosConElPermiso = gestorDePermisosDeUsuario.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });
            foreach (var usuarioConPermiso in usuariosConElPermiso)
            {
                PermisosPorElementoSql.Insertar(contexto, idNegocio, permiso.IdElemento, permiso.IdAdministrador, usuarioConPermiso.IdUsuario, calculado: true);
                PermisosPorElementoSql.Insertar(contexto, idNegocio, permiso.IdElemento, permiso.IdGestor, usuarioConPermiso.IdUsuario, calculado: true);
                PermisosPorElementoSql.Insertar(contexto, idNegocio, permiso.IdElemento, permiso.IdConsultor, usuarioConPermiso.IdUsuario, calculado: true);
            }
        }

        public static List<PermisosDelElementoDto> CrearPermisos(ContextoSe contexto, enumNegocio negocio, List<int> ids)
        {
            var gestor = Gestor(contexto, negocio);
            var permisos = new List<PermisosDelElementoDto>();
            var t = contexto.IniciarTransaccion();
            try
            {
                foreach (var idElemento in ids)
                {
                    var permisoDtm = gestor.LeerRegistroPorId(idElemento, false, false, false, true);
                    permisos.Add(gestor.MapearElemento(permisoDtm));
                }
                contexto.Commit(t);
            }
            catch
            {
                contexto.Rollback(t);
                throw;
            }
            return permisos;
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(PermisosDelElementoDtm registro, PermisosDelElementoDto elemento, ParametrosDeNegocio parametros)
        {
            elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
        }
    }

}
