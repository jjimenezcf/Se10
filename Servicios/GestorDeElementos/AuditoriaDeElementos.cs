using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper;
using Utilidades;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System.Linq;

namespace GestorDeElementos
{
    public class AuditoriaDeElementos
    {
        private string _Tabla { get; }
        public ContextoSe Contexto { get; }
        public class MapearAuditoria : Profile
        {
            public MapearAuditoria()
            {
                CreateMap<AuditoriaDtm, AuditoriaDto>();
                CreateMap<AuditoriaDto, AuditoriaDtm>();
            }
        }

        public AuditoriaDeElementos(ContextoSe contexto, enumNegocio negocio)
        {
            _Tabla = ApiDeElementoDtm.TablaDeAuditoria(negocio.TipoDtm());
            Contexto = contexto;
        }

        public AuditoriaDtm LeerRegistroPorId(int id, bool emitirError = true)
            => AuditoriaSql.LeerPorId(Contexto, _Tabla, id, emitirError);

        public IEnumerable<AuditoriaDtm> LeerRegistros(int idElemento, List<int> usuarios, int posicion, int cantidad)
            => AuditoriaSql.AuditoriaDeUnElemento(Contexto, _Tabla, idElemento, usuarios, posicion, cantidad);

        public int ContarRegistros(int idElemento, List<int> usuarios)
            => AuditoriaSql.ContarRegistros(Contexto, _Tabla, idElemento, usuarios);

        public static void RegistrarAuditoria(ContextoSe contexto, enumNegocio negocio, enumTipoOperacion operacion, IElementoDtm auditar)
        {
            auditar.UsuarioModificador = auditar.UsuarioCreador = null;
            var valor = serializarPropiedadesPOCO(auditar);
            AuditoriaSql.InsertarAuditoria(contexto, negocio.TipoDtm(), ((ElementoDtm)auditar).Id, operacion.ToBd(), valor);
        }

        private static string serializarPropiedadesPOCO(IElementoDtm elemento)
        {
            var propiedades = elemento.GetType().GetProperties().Where(p => p.SetMethod != null && p.SetMethod.IsPublic);
            var serializado = "";
            foreach (PropertyInfo propiedad in propiedades)
            {
                var tipo = propiedad.PropertyType.FullName.ToLower();
                string valor;
                if (tipo.Contains("int") || tipo.Contains("string") || tipo.Contains("date") || tipo.Contains("bool") || tipo.Contains("decimal"))
                    valor = $"{propiedad.GetValue(elemento)}";
                else
                    continue;
                serializado = $"{serializado}{(serializado.IsNullOrEmpty() ? "" : Environment.NewLine)}{propiedad.Name}:{valor}";
            }
            return serializado;
        }
    }


}
