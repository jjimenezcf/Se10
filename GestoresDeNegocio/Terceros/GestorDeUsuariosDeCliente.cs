using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using GestorDeElementos.Extensores;
using Gestor.Errores;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Entorno;
using ModeloDeDto;
using ServicioDeDatos.SistemaDocumental;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.Entorno;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeUsuariosDeCliente : GestorDeElementos<ContextoSe, UsuarioDeClienteDtm, UsuarioDeClienteDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrUsuariosDeCliente
        {
        }

        public class MapearUsuariosDeCliente : Profile
        {
            public MapearUsuariosDeCliente()
            {
                CreateMap<UsuarioDeClienteDtm, UsuarioDeClienteDto>()
                .ForMember(dto => dto.Activo, x => x.MapFrom(dtm => dtm.Usuario.Activo))
                .ForMember(dto => dto.Usuario, x => x.MapFrom(dtm => dtm.Usuario.Expresion));
                CreateMap<UsuarioDeClienteDto, UsuarioDeClienteDtm>()
                .ForMember(dtm => dtm.Usuario, x => x.Ignore());
            }
        }

        public GestorDeUsuariosDeCliente(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<UsuarioDeClienteDtm> AplicarJoins(IQueryable<UsuarioDeClienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Usuario);
            return consulta;
        }

        public static GestorDeUsuariosDeCliente Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeUsuariosDeCliente(contexto, mapeador);
        }

        protected override void AntesDePersistir(UsuarioDeClienteDtm usuarioDeCliente, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(usuarioDeCliente, parametros);

            if (!Contexto.DatosDeConexion.EsAdministrador && !usuarioDeCliente.DetalleDe(Contexto).EsAdministrador(Contexto)) GestorDeErrores.Emitir($"´Para realizar gestión de usuarios de un cliente necesita ser administrador");

            var usuario = usuarioDeCliente.Usuario(Contexto);
            if (!usuario.Activo) GestorDeErrores.Emitir($"´El usuario '{usuario.Expresion}' no está activo, actívelo previamente");

            if (parametros.Insertando)
            {
                var puestos = ((ClienteDtm)usuarioDeCliente.DetalleDe(Contexto)).Detalles<PuestoDeClienteDtm>(Contexto);
                if (puestos.Count == 0)
                    GestorDeErrores.Emitir($"Para añadir el usuario '{usuarioDeCliente.Usuario(Contexto)}' ha de indicar primero un puesto de trabajo");

                var cliente = Contexto.SeleccionarPorId<ClienteDtm>(usuarioDeCliente.IdElemento);
                var puestoIncorrecto = usuario.Puestos(Contexto).FirstOrDefault(x => !x.EsDelCliente(Contexto, cliente));
                if (puestoIncorrecto is not null)
                    GestorDeErrores.Emitir($"No se puede asociar el usuario '{usuario.Login}' al cliente '{cliente.Referencia(Contexto)}' por tener el puesto de trabajo '{puestoIncorrecto.Expresion}' que no pertenece a dicho cliente");
            }

            if (usuarioDeCliente.SeHaModificadoElCampo<int>(x => x.Name == nameof(UsuarioDeClienteDtm.IdUsuario), parametros))
                GestorDeErrores.Emitir($"No se puede modificar el usuario de un cliente, solicite que se desactive al usuario");
        }

        protected override void DespuesDePersistir(UsuarioDeClienteDtm usuarioDeCliente, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(usuarioDeCliente, parametros);

            if (parametros.Insertando)
            {
                AsociarPuestoDeTrabajo(usuarioDeCliente);
                OtorgarPermisoAlArchivadorDelCliente(usuarioDeCliente);
                usuarioDeCliente.Cliente(Contexto).CrearTraza(Contexto, "Se ha añadido un usuario de cliente", $"El usuario '{usuarioDeCliente.Usuario(Contexto).Expresion}' ha sido añadido por el usuario '{Contexto.DatosDeConexion.Login}'");
            }

            if (parametros.Eliminando)
            {
                var usuario = ((UsuarioDeClienteDtm)parametros.registroEnBd).Usuario(Contexto);

                ((ClienteDtm)usuarioDeCliente.DetalleDe(Contexto)).
                    CrearTraza(Contexto, "Se ha eliminado un usuario de cliente", $"El usuario '{usuario.Expresion}' ha sido eliminado por el usuario '{Contexto.DatosDeConexion.Login}' y desactivado");

                var otorgado = usuario.OtorgarAdministrador(Contexto);
                try
                {
                    usuario.Activo = false;
                    usuario.Modificar(Contexto);
                    Contexto.EliminarTodos<PuestosDeUnUsuarioDtm>(new Dictionary<string, object> { { nameof(PuestosDeUnUsuarioDtm.IdUsuario), usuario.Id } });
                    QuitarPermisoAlosArchivadoresDelCliente(usuarioDeCliente);
                }
                finally
                {
                    usuario.AnularAdministrador(Contexto, otorgado);
                }
            }

            TrabajosDeEntorno.SometerGenerarSeguridadParaElUsuario(Contexto, usuarioDeCliente.IdUsuario);
        }

        protected override void EliminarCaches(UsuarioDeClienteDtm usuarioDeCliente, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(usuarioDeCliente, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Ter_UsuarioDeCliente, usuarioDeCliente.IdUsuario.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Ter_EsClienteWeb, usuarioDeCliente.IdUsuario.ToString());
        }


        private void QuitarPermisoAlosArchivadoresDelCliente(UsuarioDeClienteDtm usuarioDeCliente)
        {
            var usuario = Contexto.SeleccionarPorId<UsuarioDtm>(usuarioDeCliente.IdUsuario);
            var archivadores = usuarioDeCliente.Cliente(Contexto).Vinculados<ArchivadorDtm>(Contexto);
            foreach (var archivador in archivadores)
            {
                GestorDePemisosDelElemento.QuitarPermisos(Contexto, enumNegocio.Archivador, archivador.Id, new List<int> { usuario.Id });
            }
        }


        private void OtorgarPermisoAlArchivadorDelCliente(UsuarioDeClienteDtm usuarioDeCliente)
        {
            var tipo = ExtensorDeClientes.LeerTipoDeArchivadorDeClientes(Contexto, errorSiNoEstaDefinido: false);
            if (tipo is null) return;

            var cg = ExtensorDeClientes.LeerCentroGestorDeClientes(Contexto, errorSiNoEstaDefinido: true);
            var usuario = Contexto.SeleccionarPorId<UsuarioDtm>(usuarioDeCliente.IdUsuario);
            var archivadores = usuarioDeCliente.Cliente(Contexto).Vinculados<ArchivadorDtm>(Contexto);
            foreach (var archivador in archivadores)
            {
                if (archivador.IdTipo != tipo.Id || cg.Id != archivador.IdCg || archivador.Baja) continue;
                var permisosDelElemento = GestorDePemisosDelElemento.Gestor(Contexto, enumNegocio.Archivador).LeerRegistroPorId(archivador.Id, true, false, false, false);
                usuario.OtorgarPermisoDe(Contexto, enumNegocio.Archivador, archivador.Id, permisosDelElemento, enumModoDeAccesoDeDatos.Gestor);
            }
        }

        private void AsociarPuestoDeTrabajo(UsuarioDeClienteDtm usuarioDeCliente)
        {
            var nombreDelPuesto = enumNegocio.Cliente.LeerCrearParametro(Contexto, enumParametrosDeCliente.CLI_PuestoDeTrabajo, valor: "");
            if (nombreDelPuesto.Valor.IsNullOrEmpty())
            {
                var puestos = ((ClienteDtm)usuarioDeCliente.DetalleDe(Contexto)).Detalles<PuestoDeClienteDtm>(Contexto);
                foreach (var puesto in puestos)
                {
                    ExtensionDePuestos.AsociarUsuarioAlPt(Contexto, puesto.IdPuesto, usuarioDeCliente.IdUsuario);
                }
            }
            else
            {
                var pt = Contexto.SeleccionarPorNombre<PuestoDtm>(nombreDelPuesto.Valor, errorSiNoHay: false);
                if (pt is null)
                {
                    GestorDeErrores.Emitir($"El puesto de trabajo '{nombreDelPuesto.Valor}' definido en el parámetro '{enumParametrosDeCliente.CLI_PuestoDeTrabajo}' no se ha localizado");
                }
                pt.AsociarUsuario(Contexto, usuarioDeCliente.IdUsuario);
            }
        }

        public static IDetalleDto CrearUsuario(ContextoSe contexto, int idElemento, IDetalleDto elemento, Dictionary<string, object> parametros)
        {
            var cliente = contexto.SeleccionarPorId<ClienteDtm>(idElemento);
            if (!cliente.EsAdministrador(contexto))
            {
                GestorDeErrores.Emitir("Para crear un usuario de cliente ha de ser administrador de clientes");
            }

            var datosUsuario = (UsuarioDeClienteNew)elemento;
            var nuevoUsuario = new UsuarioDtm
            {
                Login = datosUsuario.Login,
                Apellido = datosUsuario.Apellido,
                Nombre = datosUsuario.Nombre,
                eMail = datosUsuario.eMail,
                EsAdministrador = false
            }.
            InsertarComoAdministrador(contexto);

            new UsuarioDeClienteDtm
            {
                IdUsuario = nuevoUsuario.Id,
                IdElemento = idElemento
            }.
            InsertarComoAdministrador(contexto);

            return datosUsuario;
        }
    }
}
