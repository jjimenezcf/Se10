using System.Collections.Generic;
using Utilidades;
using GestorDeElementos;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Negocio
{
    public class AuditoriaDeNegocio
    {
        public static IEnumerable<AuditoriaDto> LeerElementos(ContextoSe contexto, enumNegocio negocio, int idElemento, List<int> usuarios, int posicion, int cantidad)
        {
            var a = new AuditoriaDeElementos(contexto, negocio);
            var registros = a.LeerRegistros(idElemento, usuarios, posicion, cantidad);
            var elementos = new List<AuditoriaDto>();
            var gestor = NegociosDeSe.CrearGestor(contexto, negocio);
            foreach (var registro in registros)
            {
                var elemento = MapearRegistro(contexto, registro, negocio, gestor);
                elementos.Add(elemento);
            }
            return elementos;
        }


        public static int ContarElementos(ContextoSe contexto, enumNegocio negocio, int idElemento, List<int> usuarios)
        {
            var a = new AuditoriaDeElementos(contexto, negocio);
            var cantidad = a.ContarRegistros(idElemento, usuarios);
            return cantidad;
        }

        public static AuditoriaDto LeerElemento(ContextoSe contexto, enumNegocio negocio, int id)
        {
            var a = new AuditoriaDeElementos(contexto, negocio);
            var registro = a.LeerRegistroPorId(id);
            var elemento = MapearRegistro(contexto, registro, negocio, NegociosDeSe.CrearGestor(contexto,negocio));
            return elemento;
        }
        private static AuditoriaDto MapearRegistro(ContextoSe contexto, AuditoriaDtm registro, enumNegocio negocio, IGestor gestor)
        {
            var elemento = new AuditoriaDto();
            elemento.Id = registro.Id;
            elemento.IdElemento = registro.IdElemento;
            elemento.IdUsuario = registro.IdUsuario;
            elemento.AuditadoEl = registro.AuditadoEl;
            elemento.Operacion = registro.Operacion;
            elemento.registroJson = registro.registroJson;
            elemento.Usuario = UsuarioDtm.NombreCompleto(Entorno.GestorDeUsuarios.LeerUsuario(contexto, elemento.IdUsuario));
            elemento.Negocio = NegociosDeSe.ToNombre(negocio);
            elemento.Elemento = ((INombre)gestor.LeerRegistroPorId(registro.IdElemento, aplicarJoin: false)).Nombre;

            if (contexto.DatosDeConexion.EsAdministrador)
                elemento.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
            else
            {

                var ma = ApiDePermisos.LeerModoDeAcceso(contexto, gestor.Negocio, registro.IdElemento);
                elemento.ModoDeAcceso = ma == ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Administrador ?
                    ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor :
                    ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.SinPermiso;
            }

            return elemento;
        }
    }
}
