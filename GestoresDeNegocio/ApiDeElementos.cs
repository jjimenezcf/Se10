using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Elemento;
using Utilidades;
using GestoresDeNegocio.SistemaDocumental;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Reflection;
using System;
using ModeloDeDto;
using GestoresDeNegocio.Negocio;

namespace GestoresDeNegocio
{
    public static class APiDeElementos
    {

        public static Dictionary<string, object> ToDictionary(this IRegistro elemento, ContextoSe contexto)
        {
            var datos = new Dictionary<string, object>();

            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (negocio != enumNegocio.No_Definido)
                ObtenerDatosDeNegocio(contexto, negocio, (ElementoDtm)elemento, datos);

            var propiedades = elemento.GetType().GetProperties();
            foreach (PropertyInfo propiedad in propiedades)
            {
                datos.CrearEntrada(propiedad, propiedad.GetValue(elemento));
            }

            return datos;
        }

        public static Dictionary<string, object> ToDictionary(this IElementoDto elemento)
        {
            var datos = new Dictionary<string, object>();

            var propiedades = elemento.GetType().GetProperties();
            foreach (PropertyInfo propiedad in propiedades)
            {
                datos.CrearEntrada(propiedad, propiedad.GetValue(elemento));
            }

            return datos;
        }

        private static void CrearEntrada(this Dictionary<string, object> datos, PropertyInfo propiedad, object valor)
        {
            var tipo = propiedad.PropertyType;
            if (tipo == typeof(int) || tipo == typeof(int?)) datos[propiedad.Name] = valor;
            else if (tipo == typeof(decimal) || tipo == typeof(decimal?)) datos[propiedad.Name] = valor;
            else if (tipo == typeof(float) || tipo == typeof(float?)) datos[propiedad.Name] = valor;
            else if (tipo == typeof(DateTime) || tipo == typeof(DateTime?)) datos[propiedad.Name] = valor;
            else if (tipo == typeof(string)) datos[propiedad.Name] = valor;
            else if (tipo == typeof(bool)) datos[propiedad.Name] = valor is true ? extCadenas.enumCadenas.Si : extCadenas.enumCadenas.No;
            else if (tipo == typeof(bool?)) datos[propiedad.Name] = valor is null ? null : valor is true ? extCadenas.enumCadenas.Si.ToString() : extCadenas.enumCadenas.No.ToString();
            else if (tipo.IsEnum) datos[propiedad.Name] = valor is null ? null : ((Enum)valor).Descripcion();
            else if (tipo.IsGenericType && tipo.GetGenericTypeDefinition() == typeof(Nullable<>) && tipo.GetGenericArguments()[0].IsEnum)
                datos[propiedad.Name] = valor is null ? null : ((Enum)valor).Descripcion();
        }

        private static void ObtenerDatosDeNegocio(ContextoSe Contexto, enumNegocio negocio, ElementoDtm elemento, Dictionary<string, object> datos)
        {

            if (negocio.UsaTipo())
            {
                var tipo = (ITipoDtm)negocio.CrearGestorDeTipo(Contexto).LeerRegistroPorId(((IUsaTipo)elemento).IdTipo, aplicarJoin: false);
                datos[$"{nameof(IUsaTipo.Tipo)}.{nameof(INombre.Nombre)}"] = tipo.Nombre;
                datos[$"{nameof(IUsaTipo.Tipo)}.{nameof(ITipoDtm.Activo)}"] = tipo.Activo ? extCadenas.enumCadenas.Si : extCadenas.enumCadenas.No;
                datos[$"{nameof(IUsaTipo.Tipo)}.{nameof(ITipoDtm.Sigla)}"] = tipo.Sigla;
            }

            if (negocio.UsaCg())
            {
                var cg = Contexto.SeleccionarPorId<CentroGestorDtm>(((IUsaCg)elemento).IdCg, aplicarJoin: true);
                datos[$"CG.{nameof(CentroGestorDtm.Codigo)}"] = cg.Codigo;
                datos[$"CG.{nameof(CentroGestorDtm.Nombre)}"] = cg.Nombre;
                datos[$"CG.{nameof(CentroGestorDtm.eMail)}"] = cg.eMail;
                datos[$"CG.{nameof(CentroGestorDtm.Sigla)}"] = cg.Sigla;
                datos[$"CG.{nameof(CentroGestorDtm.Expresion)}"] = cg.Expresion;
                datos[$"CG.{nameof(CentroGestorDtm.Baja)}"] = cg.Baja ? extCadenas.enumCadenas.Si : extCadenas.enumCadenas.No;
                datos[$"CG.{nameof(CentroGestorDtm.Archivo)}"] = cg.IdArchivo is not null ? ServidorDocumental.DescargarArchivo(Contexto, (int)cg.IdArchivo, solicitadoPorLaCola: false) : ApiDeArchivos.FicheroNoEncontrado;

                datos[$"CG.{nameof(CentroGestorDtm.Responsable)}.{nameof(UsuarioDtm.Login)}"] = cg.IdResponsable is not null ? cg.Responsable.Login : "";
                datos[$"CG.{nameof(CentroGestorDtm.Responsable)}.{nameof(UsuarioDtm.Expresion)}"] = cg.IdResponsable is not null ? cg.Responsable.Expresion : "";
                datos[$"CG.{nameof(CentroGestorDtm.Responsable)}.{nameof(UsuarioDtm.eMail)}"] = cg.IdResponsable is not null ? cg.Responsable.eMail : "";

                datos[$"CG.{nameof(CentroGestorDtm.CgPadre)}.{nameof(CentroGestorDtm.Codigo)}"] = cg.IdCgPadre is not null ? cg.CgPadre.Codigo : "";
                datos[$"CG.{nameof(CentroGestorDtm.CgPadre)}.{nameof(CentroGestorDtm.Nombre)}"] = cg.IdCgPadre is not null ? cg.CgPadre.Nombre : "";
                datos[$"CG.{nameof(CentroGestorDtm.CgPadre)}.{nameof(CentroGestorDtm.Expresion)}"] = cg.IdCgPadre is not null ? cg.CgPadre.Expresion : "";

                datos[$"CG.{nameof(CentroGestorDtm.Sociedad)}.{nameof(SociedadDtm.NIF)}"] = cg.Sociedad.NIF;
                datos[$"CG.{nameof(CentroGestorDtm.Sociedad)}.{nameof(SociedadDtm.Nombre)}"] = cg.Sociedad.Nombre;
                datos[$"CG.{nameof(CentroGestorDtm.Sociedad)}.{nameof(SociedadDtm.RazonSocial)}"] = cg.Sociedad.RazonSocial;
                datos[$"CG.{nameof(CentroGestorDtm.Sociedad)}.{nameof(SociedadDtm.eMail)}"] = cg.Sociedad.eMail;
                datos[$"CG.{nameof(CentroGestorDtm.Sociedad)}.{nameof(SociedadDtm.Telefono)}"] = cg.Sociedad.Telefono;
                datos[$"CG.{nameof(CentroGestorDtm.Sociedad)}.{nameof(SociedadDtm.Expresion)}"] = cg.Sociedad.Expresion;
                datos[$"CG.{nameof(CentroGestorDtm.Sociedad)}.{nameof(SociedadDtm.Archivo)}"] = cg.Sociedad.IdArchivo is not null ? ServidorDocumental.DescargarArchivo(Contexto, (int)cg.Sociedad.IdArchivo, solicitadoPorLaCola: false) : ApiDeArchivos.FicheroNoEncontrado;
            }

            if (negocio.UsaFlujo())
            {
                var estado = negocio.Estado(Contexto, ((IUsaEstado)elemento).IdEstado);
                datos[$"{nameof(IUsaEstado.Estado)}.{nameof(EstadoDtm.Nombre)}"] = estado.Nombre;
                datos[$"{nameof(IUsaEstado.Estado)}.{nameof(EstadoDtm.Inicial)}"] = estado.Inicial ? extCadenas.enumCadenas.Si : extCadenas.enumCadenas.No;
                datos[$"{nameof(IUsaEstado.Estado)}.{nameof(EstadoDtm.Cancelado)}"] = estado.Cancelado ? extCadenas.enumCadenas.Si : extCadenas.enumCadenas.No;
                datos[$"{nameof(IUsaEstado.Estado)}.{nameof(EstadoDtm.Terminado)}"] = estado.Terminado ? extCadenas.enumCadenas.Si : extCadenas.enumCadenas.No;
            }
        }


        public static List<CualquierVinculo> VinculadosAl(this IElementoDtm elementoVinculado, ContextoSe contexto)
        {
            var vinculadosAl = new List<CualquierVinculo>();
            var tipoVinculado = elementoVinculado.GetType();
            var negocioVinculado = tipoVinculado.NegocioDeUnDtm();
            var tiposPadre = negocioVinculado.VinculosCon(contexto);
            foreach (var tipoPadre in tiposPadre)
            {
                var vinculos = VinculoSql.LeerVinculosAl(contexto, tipoPadre, negocioVinculado, tipoVinculado, elementoVinculado.Id, filtros: null);
                if (vinculos.Count == 0)
                    continue;
                var negocioPadre = NegociosDeSe.NegocioDeUnDtm(tipoPadre);
                foreach (var vinculo in vinculos)
                {
                    vinculadosAl.Add(new CualquierVinculo
                    {
                        Id = vinculo.Id,
                        Negocio1 = negocioPadre,
                        idElemento1 = vinculo.idElemento1,
                        Negocio2 = negocioVinculado,
                        idElemento2 = elementoVinculado.Id
                    });
                }
            }
            return vinculadosAl;
        }

        public static List<Type> VinculosCon(this enumNegocio negocio, ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(nameof(VinculosCon));
            if (!cache.ContainsKey(negocio.ToString()))
            {
                var gestor = GestorDeNegocios.Gestor(contexto, contexto.Mapeador);
                var lista = new List<Type>();
                var negocios = gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado>());
                foreach (var n in negocios)
                {
                    var tipo = ApiDeRegistroDtm.ObtenerTypoDtm(n.ElementoDtm);
                    if (GestorDeMetadatos.ExisteTabla(ApiDeVinculos.TablaDeVinculacion(tipo, negocio)))
                        lista.Add(tipo);
                }
                cache[negocio.ToString()] = lista;
            }
            return (List<Type>)cache[negocio.ToString()];
        }
    }
}
