using AutoMapper;
using Gestor.Errores;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ModeloDeDto.Entorno;
using ModeloDeDto.Negocio;
using ModeloDeDto.Seguridad;
using ModeloDeDto.SistemaDocumental;
using ModeloDeDto.Ventas;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Utilidades;

namespace GestorDeElementos;

public class NegocioAttribute : Attribute
{
    public enumNegocio Negocio { get; set; } = enumNegocio.No_Definido;
}

public class EtapaDto
{
    public string nombre { get; set; }
    public string descripcion { get; set; }
}


public static class NegociosDeSe
{
    public static readonly string ActualizarSeguridad = nameof(ActualizarSeguridad);
    public static readonly string Dto = nameof(Dto);

    public static string TablaDePermisos(this enumNegocio negocio) => ApiDeElementoDtm.TablaDePermisos(negocio.TipoDtm());
    public static string TablaDeEstados(this enumNegocio negocio) => ApiDeElementoDtm.TablaDeEstados(negocio.TipoDtm());
    public static string TablaDeTransiciones(this enumNegocio negocio) => ApiDeElementoDtm.TablaDeTransiciones(negocio.TipoDtm());
    public static string TablaDeAcciones(this enumNegocio negocio) => ApiDeElementoDtm.TablaDeAcciones(negocio.TipoDtm());
    public static string TablaDeTrazas(this enumNegocio negocio) => ApiDeElementoDtm.TablaDeTrazas(negocio.TipoDtm());
    public static string TablaDeHitos(this enumNegocio negocio) => ApiDeElementoDtm.TablaDeHistoria(negocio.TipoDtm());

    public static EstadoDtm CrearEstado(this enumNegocio negocio)
    {
        var tipo = negocio.ObtenerMetadatos().EstadoDtm;
        Assembly assem = tipo.Assembly;
        return (EstadoDtm)assem.CreateInstance(tipo.FullName);
    }

    public static readonly string Dtm = nameof(Dtm);

    public static T NuevaTrazaDtm<T>(this enumNegocio negocio)
    where T : TrazaDtm
    => (T)ApiDeRegistroDtm.CrearDtm<T>(negocio.TablaDeTrazas());

    public static bool EsDeParametrizacion(Type tipoDtm)
    {
        if (tipoDtm.HeredaDe(typeof(EstadoDtm)) || tipoDtm.HeredaDe(typeof(TransicionDtm)) || tipoDtm.HeredaDe(typeof(AccionesDeTrnDtm)))
        {
            return true;
        }

        var negocio = LeerNegocioPorDtm(tipoDtm.FullName);
        if (negocio != null)
        {
            return negocio.EsDeParametrizacion;
        }

        if (tipoDtm.ImplementaRegistroDeParametrizacion())
        {
            return true;
        }

        return false;
    }

    public static bool EsDeParametrizacion(enumNegocio negocio)
    {
        if (negocio == enumNegocio.No_Definido)
        {
            return false;
        }

        if (negocio.Equals(enumNegocio.Archivos))
        {
            return false;
        }

        if (negocio.Equals(enumNegocio.Cobro))
        {
            return false;
        }

        if (negocio.Equals(enumNegocio.Permiso))
        {
            return true;
        }

        var negocioDto = LeerNegocioPorEnumerado(negocio);
        return negocioDto.EsDeParametrizacion;
    }

    public static bool UsaSeguridad(enumNegocio negocio)
    {
        if (negocio == enumNegocio.No_Definido)
        {
            return false;
        }

        if (negocio.Equals(enumNegocio.Archivos))
        {
            return false;
        }

        if (negocio.Equals(enumNegocio.Cobro))
        {
            return false;
        }
        var negocioDto = LeerNegocioPorEnumerado(negocio);
        return negocioDto.UsaSeguridad;
    }

    public static bool UsaPlantillasPorTipo(this enumNegocio negocio)
    =>
    GestorDeMetadatos.ExisteTabla($"{ApiDeRegistroDtm.EsquemaTabla(negocio.TipoDtm())}_{Sufijo.PLANTILLA}_{Sufijo.TIPO}");

    public static bool UsaClasesDelTipo(this enumNegocio negocio)
    =>
    GestorDeMetadatos.ExisteTabla($"{ApiDeRegistroDtm.EsquemaTabla(negocio.TipoDtm())}_{Sufijo.TIPO}_{Tablas.CLASE}");


    public static bool UsaDirecciones(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaDirecciones), (negocio) => ApiDeElementoDtm.TablaDeDireccion(TipoDtm(negocio)));
    public static bool UsaBaja(this enumNegocio negocio) => EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaUsaBaja();
    public static bool UsaBloqueos(this enumNegocio negocio) => EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaUsaBloqueo();
    public static bool UsaReferencia(this enumNegocio negocio) => EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaUsaReferencia();
    public static bool UsaObservaciones(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaObservaciones), (negocio) => ApiDeElementoDtm.TablaDeObservacion(TipoDtm(negocio)));
    public static bool UsaInterlocutores(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaInterlocutores), (negocio) => ApiDeElementoDtm.TablaDeInterlocutoresVinculados(TipoDtm(negocio)));
    public static bool UsaTareas(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaTareas), (negocio) => ApiDeElementoDtm.TablaDeTareasVinculadas(TipoDtm(negocio)));
    public static bool UsaRegistrosEs(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaRegistrosEs), (negocio) => ApiDeElementoDtm.TablaDeRegistrosEsVinculados(TipoDtm(negocio)));
    public static bool UsaTrazas(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaTrazas), (negocio) => ApiDeElementoDtm.TablaDeTrazas(TipoDtm(negocio)));
    public static bool UsaArchivadores(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaArchivadores), (negocio) => ApiDeElementoDtm.TablaDeArchivadoresVinculados(TipoDtm(negocio)));
    public static bool UsaAgenda(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaAgenda), (negocio) => ApiDeElementoDtm.TablaDeEventosVinculados(TipoDtm(negocio)));
    public static bool UsaHitos(this enumNegocio negocio) => UsaPropiedad(negocio, nameof(UsaHitos), (negocio) => ApiDeElementoDtm.TablaDeHistoria(TipoDtm(negocio)));
    public static bool PermiteConultasConGuid(this enumNegocio negocio) => negocio.ObtenerMetadatos(emitirError: false)?.DescriptoDeConsultas != null;

    private static bool UsaPropiedad(this enumNegocio negocio, string cacheDe, Func<enumNegocio, string> obtenerTabla)
    {
        var cache = ServicioDeCaches.Obtener(cacheDe);
        if (!cache.ContainsKey(negocio.ToString()))
            cache[negocio.ToString()] = EsNegocioDeBD(negocio) && GestorDeMetadatos.ExisteTabla(obtenerTabla(negocio));
        return (bool)cache[negocio.ToString()];
    }

    public static bool UsaPermisosPorElemento(this enumNegocio negocio) => !EsDeParametrizacion(negocio) && EsNegocioDeBD(negocio) && GestorDeMetadatos.ExisteTabla(ApiDeElementoDtm.TablaDePermisos(TipoDtm(negocio)));

    public static bool UsaPermisosPorElemento(Type claseDtm) => UsaPermisosPorElemento(NegocioDeUnDtm(claseDtm));

    //EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaElementoConTipo();
    public static bool UsaTipo(this enumNegocio negocio)
    =>
    EsCierto(negocio, nameof(UsaTipo), (negocio) => EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaElementoConTipo());

    public static bool UsaCg(this enumNegocio negocio)
    =>
    EsCierto(negocio, nameof(UsaCg), (negocio) => EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaUsaCg());
    //=> EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaUsaCg();

    public static bool UsaPermisosPorCg(this enumNegocio negocio)
    =>
    EsCierto(negocio, nameof(UsaPermisosPorCg), (negocio) => EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaPermisosPorCg());
    // => EsNegocioDeBD(negocio) && TipoDtm(negocio).ImplementaPermisosPorCg();

    public static bool UsaEstado(this enumNegocio negocio)
    =>
    EsCierto(negocio, nameof(UsaEstado), (negocio) => EsNegocioDeBD(negocio) && ApiDeInterfaceDtm.ImplementaUsaEstado(TipoDtm(negocio)));
    //  => EsNegocioDeBD(negocio) && ApiDeInterfaceDtm.ImplementaUsaEstado(TipoDtm(negocio));

    public static bool UsaFlujo(this enumNegocio negocio)
    =>
    EsCierto(negocio, nameof(UsaFlujo), (negocio) => EsNegocioDeBD(negocio) && ApiDeInterfaceDtm.ImplementaUsaFlujo(TipoDtm(negocio)));
    //  => EsNegocioDeBD(negocio) && ApiDeInterfaceDtm.ImplementaUsaFlujo(TipoDtm(negocio));

    private static bool EsCierto(this enumNegocio negocio, string cacheDe, Func<enumNegocio, bool> criterio)
    {
        var cache = ServicioDeCaches.Obtener(cacheDe);
        if (!cache.ContainsKey(negocio.ToString()))
            cache[negocio.ToString()] = criterio(negocio);
        return (bool)cache[negocio.ToString()];
    }

    public static bool UsaAuditoria(this enumNegocio negocio)
    {
        if (negocio == enumNegocio.Agenda || negocio == enumNegocio.Usuario || negocio == enumNegocio.Negocio)
        {
            return false;
        }

        if (EsDeParametrizacion(negocio))
        {
            return false;
        }

        if (!EsNegocioDeBD(negocio))
        {
            return false;
        }

        var tipoDtm = TipoDtm(negocio);
        if (ApiDeInterfaceDtm.ImplementaAuditoria(tipoDtm))
        {
            GestorDeMetadatos.ValidarExisteTabla(ApiDeElementoDtm.TablaDeAuditoria(tipoDtm));
            return true;
        }
        return false;
    }

    public static bool UsaArchivos(this enumNegocio negocio)
    {
        if (negocio.Equals(enumNegocio.No_Definido))
        {
            return false;
        }

        if (LeerNegocioPorEnumerado(negocio, errorSiNoHay: false) == null)
        {
            return false;
        }

        if (EsDeParametrizacion(negocio))
        {
            return false;
        }

        Type tipo = TipoDtm(negocio);
        return GestorDeMetadatos.ExisteTabla(ApiDeVinculos.TablaDeVinculacion(tipo, enumNegocio.Archivos));
    }

    public static List<Type> TiposDeAmpliaciones(this enumNegocio negocio)
    {
        var cache = ServicioDeCaches.Obtener(CacheDe.Negocio_TiposDeAmpliaciones);
        if (!cache.ContainsKey(negocio.ToString()))
        {
            var assembly = typeof(IAmpliacion).Assembly;
            var tiposHeredados = assembly.GetTypes().Where(t => t.ImplementaAmpliacion()).ToList();
            var tipoDtmDelNegocio = negocio.TipoDtm();
            var tiposQueSonAmpliacionesDelNegocio = new List<Type>();
            foreach (var tipoAmpliacion in tiposHeredados)
            {
                if (tipoAmpliacion.Name == typeof(Ampliacion<>).Name) continue;
                if (tipoAmpliacion.BaseType.GetGenericArguments().Count() == 1 && tipoAmpliacion.BaseType.GetGenericArguments()[0] == tipoDtmDelNegocio)
                    tiposQueSonAmpliacionesDelNegocio.Add(tipoAmpliacion);
            }
            cache[negocio.ToString()] = tiposQueSonAmpliacionesDelNegocio;
        }
        return (List<Type>)cache[negocio.ToString()];
    }

    public static List<Type> TiposDeDetalles(this enumNegocio negocio)
    {
        var cache = ServicioDeCaches.Obtener(CacheDe.Negocios_TiposDeDetalles);
        if (!cache.ContainsKey(negocio.ToString()))
        {
            var assembly = typeof(IDetalle).Assembly;
            var tiposHeredados = assembly.GetTypes().Where(t => t.ImplementaDetalle()).ToList();
            var tipoDtmDelNegocio = negocio.TipoDtm();
            var tiposQueSonDetallesDelNegocio = new List<Type>();
            foreach (var tipoDetalle in tiposHeredados)
            {
                if (tipoDetalle.Name == typeof(IDetalle).Name)
                    continue;
                foreach (var propiedad in tipoDetalle.GetProperties()) //.Where(x => x.PropertyType == tipoDtmDelNegocio && x.Name == nameof(IDetalle.Elemento))
                {
                    if (propiedad.PropertyType != tipoDtmDelNegocio)
                        continue;
                    if (propiedad.Name != nameof(IDetalle.Elemento))
                        continue;
                    tiposQueSonDetallesDelNegocio.Add(tipoDetalle);
                }
            }
            cache[negocio.ToString()] = tiposQueSonDetallesDelNegocio;
        }
        return (List<Type>)cache[negocio.ToString()];
    }


    public static bool EsUnRegistro(this enumNegocio negocio)
    {
        return !EsUnNegocio(negocio);
    }

    public static bool Activo(this enumNegocio negocio)
    {
        if (!EsNegocioDeBD(negocio))
        {
            return true;
        }

        var negocioDto = LeerNegocioPorEnumerado(negocio);
        return negocioDto.Activo;

        //var gestor = CrearGestor(contexto, enumNegocio.Negocio);
        //var filtro = new ClausulaDeFiltrado { Clausula = nameof(NegocioDtm.Enumerado), Criterio = enumCriteriosDeFiltrado.igual, Valor = negocio.ToString() };
        //var negocios = (List<NegocioDtm>) gestor.LeerRegistros(0, 1, new List<ClausulaDeFiltrado> { filtro });
        //return negocios.Count > 0 ? negocios[0].Activo : false;
    }

    public static bool EsUnNegocio(this enumNegocio negocio)
    {
        return negocio != enumNegocio.No_Definido;
    }

    public static string ToJson(this List<TipoDtoElmento> e)
    {
        if (e == null)
        {
            e = new List<TipoDtoElmento>();
        }

        return JsonConvert.SerializeObject(e);
    }

    public static string ToNombre(this enumNegocio negocio)
    {
        if (negocio == enumNegocio.No_Definido)
        {
            return enumNegocio.No_Definido.ToString();
        }

        var negocioDtm = LeerNegocioPorEnumerado(negocio);

        return negocioDtm.Nombre;
    }

    public static enumNegocio ToEnumerado(Type tipoDtm, bool errorSiNoEsUnNegocio = true)
    {
        var negocioDtm = LeerNegocioPorDtm(tipoDtm.FullName);

        if (negocioDtm == null)
        {
            if (errorSiNoEsUnNegocio) GestorDeErrores.Emitir($"El tipo indicado '{tipoDtm.FullName}' no implementa un negocio");
            return enumNegocio.No_Definido;
        }
        return ApiDeEnsamblados.ToEnumerado<enumNegocio>(negocioDtm.Enumerado);
    }


    public static enumNegocio ToEnumerado(string nombre, bool nullValido = false)
    {
        if (nombre.IsNullOrEmpty() && nullValido)
        {
            return enumNegocio.No_Definido;
        }

        if (nombre == enumNegocio.No_Definido.ToString())
        {
            return enumNegocio.No_Definido;
        }

        var negocioDtm = LeerNegocioPorNombre(nombre);

        if (negocioDtm == null)
        {
            try
            {
                return ApiDeEnsamblados.ToEnumerado<enumNegocio>(enumerado: nombre);
            }
            catch
            {
                return enumNegocio.No_Definido;
            }
        }

        return ApiDeEnsamblados.ToEnumerado<enumNegocio>(negocioDtm.Enumerado);
    }

    public static enumNegocio ToEnumerado(int idNegocio)
    {
        if (idNegocio == 0)
        {
            return enumNegocio.No_Definido;
        }
        var cacheDe = nameof(idNegocio) + nameof(ToEnumerado);
        var cache = ServicioDeCaches.Obtener(cacheDe);
        if (!cache.ContainsKey(idNegocio.ToString()))
        {
            var negocioDtm = LeerNegocioPorId(idNegocio);
            cache[idNegocio.ToString()] = ApiDeEnsamblados.ToEnumerado<enumNegocio>(negocioDtm.Enumerado);
        }

        return (enumNegocio)cache[idNegocio.ToString()];
    }

    public static enumNegocio ToEnumerado(this NegocioDtm negocio) => ApiDeEnsamblados.ToEnumerado<enumNegocio>(negocio.Enumerado);

    public static IQueryable<ElementoConCgDtm> ElementosConCg(this enumNegocio negocio, ContextoSe contexto)
    {
        switch (negocio)
        {
            case enumNegocio.Registro:
                return contexto.Set<RegistroEsDtm>();
            case enumNegocio.Archivador:
                return contexto.Set<ArchivadorDtm>();
            case enumNegocio.Tarea:
                return contexto.Set<TareaDtm>();
            case enumNegocio.Presupuesto:
                return contexto.Set<PresupuestoDtm>();
            case enumNegocio.Expediente:
                return contexto.Set<ExpedienteDtm>();
            case enumNegocio.Pleito:
                return contexto.Set<PleitoDtm>();
            case enumNegocio.Contrato:
                return contexto.Set<ContratoDtm>();
        }

        throw new Exception($"Se debe indicar como obtener los elementos del negocio: {negocio}");
    }

    public static bool EsVinculable(this enumNegocio negocio, enumNegocio vinculado) =>
        GestorDeMetadatos.ExisteTabla(ApiDeRegistroDtm.EsquemaDeTabla(negocio.TipoDtm()),
            ApiDeRegistroDtm.NombreDeTabla(negocio.TipoDtm()) + "_" + ApiDeRegistroDtm.NombreDeTabla(vinculado.TipoDtm()));

    public static List<enumNegocio> VinculadosConArchivadores()
    {
        var lista = new List<enumNegocio>();
        var cache = ServicioDeCaches.Obtener(nameof(VinculoDtm));
        if (!cache.ContainsKey(nameof(VinculadosConArchivadores)))
        {
            foreach (var enumerado in Enum.GetValues(typeof(enumNegocio)).Cast<enumNegocio>())
            {
                if (enumerado.UsaArchivadores())
                {
                    lista.Add(enumerado);
                }
            }
            cache[nameof(VinculadosConArchivadores)] = lista;
        }
        return (List<enumNegocio>)cache[nameof(VinculadosConArchivadores)];
    }

    public static List<enumNegocio> VinculadosConArchivos()
    {
        var lista = new List<enumNegocio>();
        var cache = ServicioDeCaches.Obtener(nameof(VinculoDtm));
        if (!cache.ContainsKey(nameof(VinculadosConArchivos)))
        {
            foreach (var enumerado in Enum.GetValues(typeof(enumNegocio)).Cast<enumNegocio>())
            {
                if (enumerado.UsaArchivos())
                {
                    lista.Add(enumerado);
                }
            }
            cache[nameof(VinculadosConArchivos)] = lista;
        }
        return (List<enumNegocio>)cache[nameof(VinculadosConArchivos)];
    }

    public static enumNegocio NegocioDeUnDtm(this Type dtm)
    {
        var cache = ServicioDeCaches.Obtener(CacheDe.Negocios_EnumeradoDeUnDtmDto);
        if (!cache.ContainsKey(dtm.FullName))
        {
            if (typeof(ArchivoDtm) == dtm)
                cache[dtm.FullName] = enumNegocio.Archivos;
            else if (typeof(CobroDeFaeDtm) == dtm)
                cache[dtm.FullName] = enumNegocio.Cobro;
            else
            {
                var registro = LeerNegocioPorDtm(dtm.FullName);

                if (registro == null)
                {
                    cache[dtm.FullName] = enumNegocio.No_Definido;
                }
                else
                {
                    foreach (enumNegocio valor in Enum.GetValues(typeof(enumNegocio)))
                    {
                        if (valor.ToString() != registro.Enumerado)
                            continue;
                        cache[dtm.FullName] = valor;
                        break;
                    }
                }
            }
        }

        if (cache.ContainsKey(dtm.FullName))
            return (enumNegocio)cache[dtm.FullName];

        throw new Exception($"No se ha localizado como negocio el dtm {dtm.FullName}");
    }

    public static enumNegocio NegocioDeUnDto(Type claseDto)
    {
        var cache = ServicioDeCaches.Obtener(CacheDe.Negocios_EnumeradoDeUnDtmDto);
        var i = claseDto.FullName;
        if (!cache.ContainsKey(i))
        {
            var registro = LeerNegocioPorDto(claseDto);

            if (registro == null)
            {
                cache[i] = enumNegocio.No_Definido;
                return enumNegocio.No_Definido;
            }

            cache[i] = ToEnumerado(registro);

            //foreach (enumNegocio valor in Enum.GetValues(typeof(enumNegocio)))
            //{
            //    if (valor.ToString() == registro.Enumerado)
            //    {
            //        cache[i] = valor;
            //        return valor;
            //    }
            //}

            //throw new Exception($"No se ha localizado como negocio el dto {claseDto}");
        }
        return (enumNegocio)cache[i];
    }

    public static VistaMvcDtm VistaMvc(this enumNegocio negocio, ContextoSe contexto)
    {
        var vistas = contexto.SeleccionarTodos<VistaMvcDtm>(new Dictionary<string, object> { { nameof(VistaMvcDtm.Controlador), negocio.Controlador() }, { nameof(VistaMvcDtm.ElementoDto), negocio.TipoDto().FullName } });
        if (vistas == null)
            GestorDeErrores.Emitir($"No se ha encontrado la vista para el negocio '{negocio.Singular()}' usando el controlador '{negocio.Controlador()}' con el dto '{negocio.TipoDto().FullName}'");
        if (vistas.Count > 1)
            GestorDeErrores.Emitir($"Se han encontrado '{vistas.Count}' vistas para el negocio '{negocio.Singular()}' usando el controlador '{negocio.Controlador()}' con el dto '{negocio.TipoDto().FullName}'");
        return vistas[0];
    }

    public static Dictionary<int, enumNegocio> NegociosConAgenda()
    {
        var cache = ServicioDeCaches.Obtener(CacheDe.ListasDeNegocios);
        if (!cache.ContainsKey(nameof(NegociosConAgenda)))
        {
            var negocios = new Dictionary<int, enumNegocio>();
            foreach (enumNegocio negocio in (enumNegocio[])Enum.GetValues(typeof(enumNegocio)))
            {
                if (negocio.UsaAgenda())
                {
                    negocios.Add(negocio.IdNegocio(), negocio);
                }
            }
            cache[nameof(NegociosConAgenda)] = negocios;
        }

        return (Dictionary<int, enumNegocio>)cache[nameof(NegociosConAgenda)];
    }

    public static Dictionary<int, enumNegocio> NegociosQuePermitenSubirDocumentacionDesdeElMovil(ContextoSe contexto)
    {
        var cache = ServicioDeCaches.Obtener(CacheDe.ListasDeNegocios);
        if (!cache.ContainsKey(nameof(NegociosQuePermitenSubirDocumentacionDesdeElMovil)))
        {
            var negocios = new Dictionary<int, enumNegocio>();
            foreach (enumNegocio negocio in (enumNegocio[])Enum.GetValues(typeof(enumNegocio)))
            {
                if (negocio == enumNegocio.No_Definido)
                {
                    continue;
                }

                if (!negocio.UsaArchivos())
                {
                    continue;
                }

                if (!negocio.EsNegocioDeBD())
                {
                    continue;
                }

                var parametro = negocio.Parametro(contexto, enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil, errorSiNoHay: false, errorSinValor: true, errorSinMasDeUno: true);
                if ((parametro is not null && (bool)parametro.Valor.TrueOrNull()) || (negocio.UsaArchivadores() && negocio.UsaHitos()))
                {
                    negocios.Add(negocio.IdNegocio(), negocio);
                }
            }
            cache[nameof(NegociosQuePermitenSubirDocumentacionDesdeElMovil)] = negocios;
        }

        return (Dictionary<int, enumNegocio>)cache[nameof(NegociosQuePermitenSubirDocumentacionDesdeElMovil)];
    }

    public static List<NegocioMovilDto> NegociosQuePermitenSubirDocumentacionDesdeElMovilAlUsuarioConectado(ContextoSe contexto, bool incluirSinPermisos)
    {
        var negocios = NegociosQuePermitenSubirDocumentacionDesdeElMovil(contexto);
        var negociosPermitidos = new List<NegocioMovilDto>();
        foreach (var negocio in negocios)
        {
            var modoDeAcceso = ApiDePermisos.LeerModoDeAccesoAlNegocio(contexto, negocio.Value);
            var incluirNegocio = modoDeAcceso.HayPermisosDe(enumModoDeAccesoDeDatos.Consultor) || incluirSinPermisos;
            if (incluirNegocio)
            {
                if (negocio.Value == enumNegocio.Contrato)
                {
                    negociosPermitidos.Add(negocio.Value.CrearNegocioMovilDto(negocio.Key, enumClaseDeContrato.Venta.Descripcion(), modoDeAcceso, new List<Parametro>
                        {
                            new(nameof(ContratoDtm.ClaseDeContrato), enumClaseDeContrato.Venta.ToString()),
                        }));
                }
                else
                {
                    negociosPermitidos.Add(negocio.Value.CrearNegocioMovilDto(negocio.Key, negocio.Value.Plural(), modoDeAcceso));
                }
            }

        }
        return negociosPermitidos;
    }

    private static NegocioMovilDto CrearNegocioMovilDto(this enumNegocio negocio, int id, string nombre, enumModoDeAccesoDeDatos modoDeAcceso, List<Parametro> parametros = null)
    {
        var negocioMovil = new NegocioMovilDto
        {
            Id = id,
            Nombre = nombre,
            ModDeAcceso = modoDeAcceso,
            Controlador = negocio.Controlador(),
            Negocio = negocio
        };

        if (parametros is not null)
        {
            negocioMovil.Parametros.AddRange(parametros);
        }

        return negocioMovil;
    }

    public static NegocioDtm LeerNegocioPorEnumerado(enumNegocio negocio, bool errorSiNoHay = true) => negocio.LeerNegocio(errorSiNoHay);

    public static NegocioDtm LeerNegocio(this enumNegocio negocio, bool errorSiNoHay = true)
    {
        var negocioDtm = BuscarNegocioPorEnumerado(negocio);
        if (negocioDtm == null && errorSiNoHay)
        {
            GestorDeErrores.Emitir($"No se ha localizado el negocio con el enumerado {negocio}");
        }

        return negocioDtm;
    }

    public static bool EsNegocioDeBD(this enumNegocio negocio)
    {
        if (negocio == enumNegocio.No_Definido)
        {
            return false;
        }

        var negocioDtm = BuscarNegocioPorEnumerado(negocio);
        if (negocioDtm == null)
        {
            return false;
        }

        return true;
    }

    public static NegocioDtm BuscarNegocioPorEnumerado(enumNegocio negocio)
    {
        var cache = ServicioDeCaches.Obtener($"{nameof(NegociosDeSe)}.{nameof(LeerNegocioPorEnumerado)}");
        var indice = $"{nameof(enumNegocio)}-{negocio}";
        if (!cache.ContainsKey(indice))
        {
            var negocioDtm = NegocioSqls.LeerNegocioPorEnumerado(negocio, erroSinoHay: false);

            cache[indice] = negocioDtm;
        }
        return (NegocioDtm)cache[indice];
    }

    public static NegocioDtm LeerNegocioPorId(int id, bool errorSiCero = true)
    {
        if (!errorSiCero && id == 0)
        {
            return null;
        }

        var cache = ServicioDeCaches.Obtener($"{nameof(NegociosDeSe)}.{nameof(LeerNegocioPorId)}");
        var indice = $"{nameof(IRegistro.Id)}-{id}";
        if (!cache.ContainsKey(indice))
        {
            var negocio = NegocioSqls.LeerPorId(id);
            if (negocio == null)
            {
                GestorDeErrores.Emitir($"No se ha definido el negocio con id {id}");
            }

            cache[indice] = negocio;
        }
        return (NegocioDtm)cache[indice];
    }

    public static NegocioDtm LeerNegocioPorNombre(string nombreNegocio)
    {
        var cache = ServicioDeCaches.Obtener($"{nameof(NegociosDeSe)}.{nameof(LeerNegocioPorNombre)}");
        var indice = $"{nameof(INombre.Nombre)}-{nombreNegocio}";
        if (!cache.ContainsKey(indice))
        {
            var negocios = NegocioSqls.LeerPorNombre(nombreNegocio);

            if (negocios.Count > 1)
            {
                GestorDeErrores.Emitir($"No se ha localizado de forma unívoca el negocio {nombreNegocio}");
            }

            if (negocios.Count == 0)
            {
                return null;
            }

            cache[indice] = negocios[0];
        }
        return (NegocioDtm)cache[indice];
    }

    public static NegocioDtm LeerNegocioPorDto(Type tipo)
    {
        string fullNameDelTipoDelDto = tipo.FullName;
        var cache = ServicioDeCaches.Obtener($"{nameof(NegociosDeSe)}.{nameof(LeerNegocioPorDto)}");
        var indice = $"{nameof(Dto)}-{fullNameDelTipoDelDto}";
        if (!cache.ContainsKey(indice))
        {
            var negocios = NegocioSqls.LeerNegocioPorDto(fullNameDelTipoDelDto);

            if (negocios.Count > 1)
            {
                GestorDeErrores.Emitir($"No se ha localizado de forma unívoca el negocio al leer por dto {fullNameDelTipoDelDto}");
            }

            cache[indice] = negocios.Count == 0 ? null : negocios[0];
        }
        return (NegocioDtm)cache[indice];
    }

    public static int IdNegocio(this enumNegocio negocio)
    {
        var cache = ServicioDeCaches.Obtener(nameof(IdNegocio));
        if (!cache.ContainsKey(negocio.ToString()))
        {
            cache[negocio.ToString()] = LeerNegocioPorEnumerado(negocio).Id;
        }
        return (int)cache[negocio.ToString()];
    }

    public static int IdNegocio(Type tipoDtm)
    {
       var negocio = NegocioDeUnDtm(tipoDtm);
        if (negocio == enumNegocio.No_Definido)
        {
            GestorDeErrores.Emitir($"El tipo '{tipoDtm.FullName}' no tiene correspondencia con ningún negocio");
        }
        return IdNegocio(negocio);
    }

    public static int IdNegocio(this enumNegocio negocio, bool errorSiNoHay)
    {
        return LeerNegocioPorEnumerado(negocio, errorSiNoHay)?.Id ?? 0;
    }
    public static NegocioDtm LeerNegocioPorDtm(string fullNameDelTipoDtm)
    {
        var cache = ServicioDeCaches.Obtener($"{nameof(NegociosDeSe)}.{nameof(LeerNegocioPorDtm)}");
        var indice = $"{nameof(Dtm)}-{fullNameDelTipoDtm}";
        if (!cache.ContainsKey(indice))
        {
            var negocios = NegocioSqls.LeerNegocioPorDtm(fullNameDelTipoDtm);

            if (negocios.Count > 1)
            {
                GestorDeErrores.Emitir($"No se ha localizado de forma unívoca el negocio al leer por dto {fullNameDelTipoDtm}");
            }

            cache[indice] = negocios.Count == 0 ? null : negocios[0];
        }
        return (NegocioDtm)cache[indice];
    }

    public static enumNegocio NegocioDeUnTipoDtm(Type tipoDtm, bool emitirError = true)
    {
        var negocio = LeerNegocioPorDtm(tipoDtm.FullName);
        if (negocio != null) return ToEnumerado(negocio);

        if (tipoDtm == typeof(ArchivoDtm)) return enumNegocio.Archivos;
        if (tipoDtm == typeof(CobroDeFaeDtm)) return enumNegocio.Cobro;

        if (emitirError)
            throw new Exception($"El tipo {tipoDtm.FullName} no tiene correspondencia con ningún negocio");

        return enumNegocio.No_Definido;
    }


    public static Type TipoDtm(this enumNegocio negocio)
    {
        if (negocio == enumNegocio.Archivos)
        {
            return typeof(ArchivoDtm);
        }

        if (negocio == enumNegocio.Cobro)
        {
            return typeof(CobroDeFaeDtm);
        }

        var negocioDtm = LeerNegocioPorEnumerado(negocio);
        if (negocioDtm.ElementoDtm.IsNullOrEmpty())
        {
            throw new Exception($"El negocio {negocio} no tiene definido el tipo Dtm");
        }

        var tipoDtm = ApiDeRegistroDtm.ObtenerTypoDtm(negocioDtm.ElementoDtm);

        return tipoDtm;
    }

    public static Type TipoDto(this enumNegocio negocio)
    {
        if (negocio == enumNegocio.Archivos)
        {
            return typeof(ArchivoDto);
        }

        if (negocio == enumNegocio.Cobro)
        {
            return typeof(CobroDeFaeDto);
        }

        var negocioDto = LeerNegocioPorEnumerado(negocio);
        if (negocioDto.ElementoDto.IsNullOrEmpty())
        {
            throw new Exception($"El negocio {negocio} no tiene definido el tipo Dto");
        }

        var tipoDto = ExtensionesDto.ObtenerTypoDto(negocioDto.ElementoDto);

        return tipoDto;
    }

    public static IGestor CrearGestorDeUnDtm<T>(this ContextoSe contexto)
    where T : RegistroDtm
    {
        var gestor = CrearGestorDeParametrizacion(contexto, typeof(T));
        if (gestor != null)
        {
            return gestor;
        }

        var negocio = NegocioDeUnDtm(typeof(T));

        gestor = negocio == enumNegocio.No_Definido
        ? CrearGestorDeUnNegocioNoDefinido<T>(contexto)
        : CrearGestor(contexto, negocio, typeof(T));
        return gestor;
    }

    public static IGestor CrearGestor(this enumNegocio negocio, ContextoSe contexto, Type tipo = null) => CrearGestor(contexto, negocio, tipo);
    public static IGestor CrearGestor(ContextoSe contexto, int idNegocio) => CrearGestor(contexto, ToEnumerado(idNegocio));
    public static IGestor CrearGestor(ContextoSe contexto, enumNegocio negocio, Type tipo = null)
    {
        if (!negocio.EsUnNegocio())
        {
            return CrearGestorDeUnNegocioNoDefinido(contexto, tipo);
        }

        if (tipo.HeredaDe(typeof(EstadoDtm)))
        {
            return contexto.CrearGestorDeEstados(negocio);
        }

        if (tipo.HeredaDe(typeof(TransicionDtm)))
        {
            return CrearGestorDeTransiciones(contexto, negocio);
        }

        if (tipo.HeredaDe(typeof(AccionesDeTrnDtm)))
        {
            return CrearGestorDeAccionesDeTrn(contexto, negocio);
        }

        var Dtm = negocio.TipoDtm();
        var Dto = negocio.TipoDto();

        var gestor = CrearGestorDeParametrizacion(contexto, Dtm);


        return gestor == null ? CrearGestor(contexto, Dtm.Name, Dto.Name) : gestor;
    }

    private static IGestor CrearGestorDeParametrizacion(ContextoSe contexto, Type Dtm)
    {

        if (ApiDeEnsamblados.HeredaDe(Dtm, typeof(TipoDeElementoDtm)))
        {
            return CrearGestorDeTipoDtm(contexto, Dtm);
        }

        if (ApiDeEnsamblados.HeredaDe(Dtm, typeof(ObservacionDtm)))
        {
            return CrearGestorDeObservacion(contexto, Dtm);
        }

        if (ApiDeEnsamblados.HeredaDe(Dtm, typeof(EstadoDtm)))
        {
            return CrearGestorDeEstados(contexto, Dtm);
        }

        if (ApiDeEnsamblados.HeredaDe(Dtm, typeof(TransicionDtm)))
        {
            return CrearGestorDeTransiciones(contexto, Dtm);
        }

        if (ApiDeEnsamblados.HeredaDe(Dtm, typeof(AccionesDeTrnDtm)))
        {
            return CrearGestorDeAccionesDeTrn(contexto, Dtm);
        }

        return null;
    }

    private static IGestor CrearGestorDeUnNegocioNoDefinido<T>(ContextoSe contexto) where T : RegistroDtm
    {
        if (typeof(T).Equals(typeof(ArchivoDtm)))
        {
            return CrearGestor(contexto, typeof(ArchivoDtm), typeof(ArchivoDto));
        }

        if (typeof(T).Equals(typeof(PermisosDirectosDtm)))
        {
            return CrearGestor(contexto, typeof(PermisosDirectosDtm), typeof(PermisosDeUnPuestoDto));
        }

        if (typeof(T).ImplementaUnaTraza())
        {
            return CrearGestor(contexto, typeof(TrazaDtm), typeof(TrazaDto));
        }

        Type tipoDto = typeof(T).ToDto();
        if (tipoDto != null)
        {
            return CrearGestor(contexto, typeof(T), tipoDto);
        }

        try
        {
            return CrearGestor(contexto, typeof(T), typeof(ElementoDto));
        }
        catch
        {

        }

        return new GestorDeElementos<ContextoSe, T, ElementoDto>(contexto);
    }

    public static Type ToDto(this Type tipoDtm)
    {
        var espacioDeNombreDeDtm = ApiDeEnsamblados.DllDelServicioDeDatos.Replace(".dll", "");
        var espacioDeNombreDeDto = ApiDeEnsamblados.DllDelModeloDeDto.Replace(".dll", "");
        var nombreDelTipoDto = tipoDtm.FullName.Replace("Dtm", "Dto").Replace(espacioDeNombreDeDtm, espacioDeNombreDeDto);
        var tipoDto = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelModeloDeDto, nombreDelTipoDto, false);
        return tipoDto;
    }

    public static Type ToDtm(this Type tipoDto)
    {
        var espacioDeNombreDeDtm = ApiDeEnsamblados.DllDelServicioDeDatos.Replace(".dll", "");
        var espacioDeNombreDeDto = ApiDeEnsamblados.DllDelModeloDeDto.Replace(".dll", "");
        var nombreDelTipoDtm = tipoDto.FullName.Replace(espacioDeNombreDeDto, espacioDeNombreDeDtm).Replace("Dto", "Dtm");
        var tipoDtm = ApiDeEnsamblados.ObtenerType(ApiDeEnsamblados.DllDelServicioDeDatos, nombreDelTipoDtm, false);
        return tipoDtm;
    }

    public static IGestor CrearGestorDeUnDetalle(this Type tipoDtm, ContextoSe contexto)
    {
        if (!tipoDtm.ImplemtaUnIDetalle())
            GestorDeErrores.Emitir($"No puede obtener un gestor de detalle del tipo {tipoDtm} por no implementar {nameof(IDetalle)}");

        return CrearGestorDeUnDtm(tipoDtm, contexto);
    }

    public static IGestor CrearGestorDeUnaAmpliacion(this Type tipoDtm, ContextoSe contexto)
    =>
    CrearGestorDeUnDtm(tipoDtm, contexto);

    public static IGestor CrearGestorDeUnDtm(this Type tipoDtm, ContextoSe contexto)
    {
        if (!tipoDtm.HeredaDe(typeof(RegistroDtm), incluirTipo: false, permitirTipoNulo: false))
            GestorDeErrores.Emitir($"No puede obtener un objeto gestor del tipo {tipoDtm}");

        var tipoDto = tipoDtm.ToDto();

        if (tipoDto != null)
        {
            return CrearGestor(contexto, tipoDtm, tipoDto);
        }

        return CrearGestor(contexto, tipoDtm, typeof(ElementoDto));
    }

    private static IGestor CrearGestorDeUnNegocioNoDefinido(ContextoSe contexto, Type tipoDtm)
    {
        if (tipoDtm.Equals(typeof(ArchivoDtm)))
        {
            return CrearGestor(contexto, typeof(ArchivoDtm), typeof(ArchivoDto));
        }

        if (tipoDtm.Equals(typeof(PermisosDirectosDtm)))
        {
            return CrearGestor(contexto, typeof(PermisosDirectosDtm), typeof(PermisosDeUnPuestoDto));
        }

        if (tipoDtm.Equals(typeof(UsuariosDeUnPermisoDtm)))
        {
            return CrearGestor(contexto, typeof(UsuariosDeUnPermisoDtm), typeof(PermisosDeUnUsuarioDto));
        }

        var gestor = CrearGestorDeParametrizacion(contexto, tipoDtm);
        if (gestor != null)
        {
            return gestor;
        }

        var nombreTipoDto = tipoDtm.Name.Replace("Dtm", "Dto");
        var tipoDto = ExtensionesDto.ObtenerTypoDto(nombreTipoDto, false);

        return CrearGestor(contexto, tipoDtm, tipoDto == null ? typeof(ElementoDto) : tipoDto);
    }

    public static IGestor CrearGestor(ContextoSe contexto, Type claseDtm, Type claseDto, enumNegocio negocio)
    {
        if (negocio == enumNegocio.No_Definido)
        {
            GestorDeErrores.Emitir($"Ha solicitado un gestor para la clase {claseDtm.Name} y no ha indicado el Negocio");
        }

        return CrearGestor(contexto, claseDtm.Name, claseDto.Name, negocio);
    }

    public static IGestor CrearGestor(ContextoSe contexto, Type claseDtm, Type claseDto)
    {
        return CrearGestor(contexto, claseDtm.Name, claseDto.Name, enumNegocio.No_Definido);
    }

    private static IGestor CrearGestorDeObservacion(ContextoSe contexto, Type claseDtm)
    {
        try
        {
            var negocio = ExtensorDeObservaciones.Negocio(claseDtm);
            return GestorDeObservaciones.Gestor(contexto, negocio);
        }
        catch (Exception e)
        {
            throw new Exception($"Ha solicitado un gestor de observaciones para la clase {claseDtm.Name} y no está definido cómo obtenerlo", e);
        }
    }

    public static IGestor CrearGestorDeTipoDtm(ContextoSe contexto, Type tipoDtm)
    {
        try
        {
            var negocio = (enumNegocio)tipoDtm.GetProperty(nameof(ITipoDeElementoDtm.Negocio), BindingFlags.Static | BindingFlags.Public).GetValue(null, null);
            if (negocio == enumNegocio.No_Definido)
            {
                negocio = ApiTipoDeElementoDtm.Negocio(tipoDtm);
            }

            return CrearGestor(contexto, negocio.TipoDtm(), negocio.TipoDto()).GestorDeTipos;
        }
        catch (Exception e)
        {
            throw new Exception($"Ha solicitado un gestor de tipo para la clase {tipoDtm.Name} y no está definido cómo obtenerlo", e);
        }
    }
    public static IGestor CrearGestorDeTipoDto(ContextoSe contexto, Type tipoDto)
    {
        try
        {
            var negocio = ApiTipoDeElementoDtm.Negocio(tipoDto);

            return CrearGestor(contexto, negocio.TipoDtm(), negocio.TipoDto()).GestorDeTipos;
        }
        catch (Exception e)
        {
            throw new Exception($"Ha solicitado un gestor de tipo para la clase {tipoDto.Name} y no está definido cómo obtenerlo", e);
        }
    }

    public static IGestorDeTipos CrearGestorDeTipo(this enumNegocio negocio, ContextoSe contexto)
    {
        if (!negocio.UsaTipo())
        {
            GestorDeErrores.Emitir($"Ha solicitado un gestor de tipos, y el negocio {negocio} no los usa");
        }

        return negocio.CrearGestor(contexto).GestorDeTipos;
    }

    public static IGestor CrearGestorDeEstados(ContextoSe contexto, Type estadoDtm) => CrearGestorDeEstados(contexto, ExtensorDeEstados.Negocio(estadoDtm));
    public static IGestor CrearGestorDeEstados(this ContextoSe contexto, enumNegocio negocio)
    {
        if (!negocio.UsaEstado())
        {
            GestorDeErrores.Emitir($"Ha solicitado un gestor de estados, y el negocio {negocio} no los usa");
        }

        return CrearGestor(contexto, typeof(EstadoDtm), typeof(EstadoDto), negocio);
    }

    public static IGestor CrearGestorDeHitos(ContextoSe contexto, Type hitoDtm) => CrearGestorDeHitos(contexto, ExtensorDeHitos.Negocio(hitoDtm));

    public static IGestor CrearGestorDeHitos(this ContextoSe contexto, enumNegocio negocio)
    {
        if (!negocio.UsaHitos())
        {
            GestorDeErrores.Emitir($"Ha solicitado un gestor de hitos, y el negocio {negocio} no los usa");
        }

        return CrearGestor(contexto, typeof(HitoDtm), typeof(HitoDto), negocio);
    }

    public static IGestor CrearGestorDeTransiciones(ContextoSe contexto, Type transicionDtm) => CrearGestorDeTransiciones(contexto, ExtensorDeTransiciones.Negocio(transicionDtm));

    public static IGestor CrearGestorDeTransiciones(this ContextoSe contexto, enumNegocio negocio)
    {
        if (!negocio.UsaFlujo())
        {
            GestorDeErrores.Emitir($"Ha solicitado un gestor de transiciones, y el negocio {negocio} no las usa");
        }

        return CrearGestor(contexto, typeof(TransicionDtm), typeof(TransicionDto), negocio);
    }

    public static IGestor CrearGestorDeTransiciones(this enumNegocio negocio, ContextoSe contexto)
    {
        if (!negocio.UsaFlujo())
        {
            GestorDeErrores.Emitir($"Ha solicitado un gestor de transiciones, y el negocio {negocio} no las usa");
        }

        return CrearGestor(contexto, typeof(TransicionDtm), typeof(TransicionDto), negocio);
    }

    public static IGestor CrearGestorDeAccionesDeTrn(ContextoSe contexto, Type accionTrn) => CrearGestorDeAccionesDeTrn(contexto, ExtensorDeAccionesDeTrn.Negocio(accionTrn));

    public static IGestor CrearGestorDeAccionesDeTrn(this ContextoSe contexto, enumNegocio negocio)
    {
        if (!negocio.UsaFlujo())
        {
            GestorDeErrores.Emitir($"Ha solicitado un gestor de acciones de transición, y el negocio {negocio} no las usa");
        }

        return CrearGestor(contexto, typeof(AccionesDeTrnDtm), typeof(AccionesDeTrnDto), negocio);
    }


    public static IGestor CrearGestor(ContextoSe contexto, string claseDtm, string claseDto, enumNegocio negocio = enumNegocio.No_Definido)
    {
        var cacheDeGestorDeRelaciones = ServicioDeCaches.Obtener(CacheDe.GestorDeRelaciones);
        var cacheDeGestorDeElementos = ServicioDeCaches.Obtener(nameof(GestorDeElementos));
        var cacheDeGestorGenerico = ServicioDeCaches.Obtener(nameof(IGestorGenerico));
        var cacheDeGestorDePlantillasPorTipo = ServicioDeCaches.Obtener(CacheDe.GestorDePlantillasPorTipo);
        var cacheDeGestorDeClasesDelTipo = ServicioDeCaches.Obtener(CacheDe.GestorDeClasesDelTipo);

        var clave = $"{claseDtm}-{claseDto}-{negocio}";
        if (cacheDeGestorDeRelaciones.ContainsKey(clave))
        {
            return (IGestor)((ConstructorInfo)cacheDeGestorDeRelaciones[clave]).Invoke(new object[] { contexto, contexto.Mapeador });
        }

        if (cacheDeGestorDeElementos.ContainsKey(clave))
        {
            return (IGestor)((ConstructorInfo)cacheDeGestorDeElementos[clave]).Invoke(new object[] { contexto, contexto.Mapeador });
        }

        if (cacheDeGestorGenerico.ContainsKey(clave))
        {
            return (IGestor)((ConstructorInfo)cacheDeGestorGenerico[clave]).Invoke(new object[] { contexto, negocio });
        }
        if (cacheDeGestorDePlantillasPorTipo.ContainsKey(clave))
        {
            return (IGestor)((ConstructorInfo)cacheDeGestorDePlantillasPorTipo[clave]).Invoke(new object[] { contexto, contexto.Mapeador });
        }
        if (cacheDeGestorDeClasesDelTipo.ContainsKey(clave))
        {
            return (IGestor)((ConstructorInfo)cacheDeGestorDeClasesDelTipo[clave]).Invoke(new object[] { contexto, contexto.Mapeador });
        }
        ConstructorInfo constructor = null;
        var assembly = ApiDeEnsamblados.ObtenerDll(ApiDeEnsamblados.DllDelGestorDeNegocio);
        var clases = assembly.GetExportedTypes();
        for (int i = 0; i < clases.Length; i++)
        {
            var clase = clases[i];

            if (clase.BaseType == null)
            {
                continue;
            }

            if (clase.BaseType.Name.Contains(ApiDeEnsamblados.ClaseBase_GestorDeRelaciones) && clase.BaseType.GenericTypeArguments[1].Name == claseDtm && clase.BaseType.GenericTypeArguments[2].Name == claseDto)
            {
                constructor = clase.GetConstructor(new Type[] { typeof(ContextoSe), typeof(IMapper) });
                if (constructor == null)
                {
                    GestorDeErrores.Emitir($"No se ha definido el constructor Gestor para la clase '{clase.Name}' con los parámetros de contexto y mapeador.");
                }

                cacheDeGestorDeRelaciones[clave] = constructor;
                return (IGestor)constructor.Invoke(new object[] { contexto, contexto.Mapeador });
            }
            if (clase.BaseType.Name.Contains(ApiDeEnsamblados.ClaseBase_GestorDePlantillasPorTipo) && clase.BaseType.GenericTypeArguments[1].Name == claseDtm && clase.BaseType.GenericTypeArguments[2].Name == claseDto)
            {
                constructor = clase.GetConstructor(new Type[] { typeof(ContextoSe), typeof(IMapper) });
                if (constructor == null)
                {
                    GestorDeErrores.Emitir($"No se ha definido el constructor Gestor para la clase '{clase.Name}' con los parámetros de contexto y mapeador.");
                }
                cacheDeGestorDePlantillasPorTipo[clave] = constructor;
                return (IGestor)constructor.Invoke(new object[] { contexto, contexto.Mapeador });
            }
            if (clase.BaseType.Name.Contains(ApiDeEnsamblados.ClaseBase_GestorDeClasesDelTipo)
                && clase.BaseType.GenericTypeArguments[1].Name == claseDtm
                && clase.BaseType.GenericTypeArguments[2].Name == claseDto)
            {
                constructor = clase.GetConstructor(new Type[] { typeof(ContextoSe), typeof(IMapper) });
                if (constructor == null)
                {
                    GestorDeErrores.Emitir($"No se ha definido el constructor Gestor para la clase '{clase.Name}' con los parámetros de contexto y mapeador.");
                }
                cacheDeGestorDeClasesDelTipo[clave] = constructor;
                return (IGestor)constructor.Invoke(new object[] { contexto, contexto.Mapeador });
            }
            if ((clase.BaseType.Name.Contains(nameof(GestorDeElementos)) || clase.BaseType.Name.Contains(ApiDeEnsamblados.ClaseBase_GestorDeTipos))
                && clase.BaseType.GenericTypeArguments[1].Name == claseDtm
                && clase.BaseType.GenericTypeArguments[2].Name == claseDto)
            {
                constructor = clase.GetConstructor(new Type[] { typeof(ContextoSe), typeof(IMapper) });
                if (constructor != null)
                {
                    cacheDeGestorDeElementos[clave] = constructor;
                    return (IGestor)constructor.Invoke(new object[] { contexto, contexto.Mapeador });
                }
                if (negocio == enumNegocio.No_Definido)
                {
                    GestorDeErrores.Emitir($"No se ha definido el constructor Gestor para la clase '{clase.Name}' con los parámetros de contexto y mapeador.");
                }

                constructor = clase.GetConstructor(new Type[] { typeof(ContextoSe), typeof(enumNegocio) });
                if (constructor == null)
                {
                    GestorDeErrores.Emitir($"No se ha localizado un constructor para el negocio '{negocio}' con los parámetros de contexto y negocio.");
                }

                cacheDeGestorGenerico[clave] = constructor;
                return (IGestor)constructor.Invoke(new object[] { contexto, negocio });
            }
            if (clase.BaseType.Name.Contains(ApiDeEnsamblados.ClaseBase_GestorDeEstados))
            {
                constructor = clase.GetConstructor(new Type[] { typeof(ContextoSe), typeof(enumNegocio) });
                if (constructor == null)
                {
                    GestorDeErrores.Emitir($"No se ha localizado un constructor de estados para el negocio '{negocio}'");
                }

                cacheDeGestorDeElementos[clave] = constructor;
                return (IGestor)constructor.Invoke(new object[] { contexto, negocio });
            }
            if (clase.BaseType.Name.Contains(ApiDeEnsamblados.ClaseBase_GestorDeTransiciones))
            {
                constructor = clase.GetConstructor(new Type[] { typeof(ContextoSe), typeof(enumNegocio) });
                if (constructor == null)
                {
                    GestorDeErrores.Emitir($"No se ha localizado un constructor de transiciones para el negocio '{negocio}' ");
                }

                cacheDeGestorDeElementos[clave] = constructor;
                return (IGestor)constructor.Invoke(new object[] { contexto, negocio });
            }
        }
        throw new Exception($"No se ha definido el constructor Gestor para la clase '{claseDtm}' con los parámetros de contexto y mapeador.");
    }

    internal static void EjecutarEventos(this enumNegocio negocio, ContextoSe contexto, IRegistro registro, enumMomentoDeAccion momento)
    {
        if (!registro.ImplementaUnElemento())
            return;

        var idNegocio = negocio.IdNegocio(errorSiNoHay: false);
        if (idNegocio == 0)
            return;

        var eventos = contexto.SeleccionarTodos<AccionDeNegocioDtm>(new Dictionary<string, object>
        {
            { nameof(AccionDeNegocioDtm.IdNegocio), idNegocio},
            { nameof(AccionDeNegocioDtm.Momento), momento }
        }, aplicarJoin: true);


        var entorno = new EntornoDeUnaAccion(contexto,
            registro,
            negocio,
            new Dictionary<string, object>());

        foreach (var evento in eventos)
        {
            entorno.Evento = evento;
            evento.Accion.Ejecutar(entorno, evento.Parametros);
        }
    }

    public static T Recargar<T>(this T registro, ContextoSe contexto, bool aplicarJoin = true)
    where T : IRegistro
    =>
    (T)CrearGestor(contexto, NegocioDeUnDtm(registro.GetType())).LeerRegistroPorId(registro.Id, aplicarJoin, usarLaCache: false);

    public static ElementoDto LeerElemento(this enumNegocio negocio, ContextoSe contexto, int idElemento, bool aplicarJoin = true, bool errorSiNoHay = true, Dictionary<string, object> parametros = null)
    {
        var gestor = CrearGestor(contexto, negocio);
        if (parametros == null)
        {
            parametros = new Dictionary<string, object>();
        }

        parametros[ltrParametrosNeg.AplicarJoin] = aplicarJoin;
        parametros[ltrParametrosNeg.ErrorSiNoLoHay] = errorSiNoHay;
        return (ElementoDto)gestor.LeerElementoPorId(idElemento, parametros);
    }

    public static IRegistro LeerRegistro(this enumNegocio negocio, ContextoSe contexto, int idRegistro, bool aplicarJoin = true, bool errorSiNoHay = true, Dictionary<string, object> parametros = null)
    {
        var gestor = CrearGestor(contexto, negocio);
        if (parametros == null)
        {
            parametros = new Dictionary<string, object>();
        }

        parametros[ltrParametrosNeg.ErrorSiNoLoHay] = errorSiNoHay;

        var usarLaCache = parametros.LeerValor(ltrParametrosNeg.UsarLaCache, true);

        return (IRegistro)gestor.LeerRegistroPorId(idRegistro, aplicarJoin, usarLaCache: usarLaCache, parametros: parametros);
    }

    public static EstadoDtm Estado(this enumNegocio negocio, ContextoSe contexto, int id)
    {
        var cache = ServicioDeCaches.Obtener(CacheDe.Estado);
        var i = $"{negocio}-{id}";
        if (!cache.ContainsKey(i))
            cache[i] = negocio.Estados(contexto).First(e => e.Id == id); // (EstadoDtm)CrearGestorDeEstados(contexto, negocio).LeerRegistroPorId(id, aplicarJoin: false, usarLaCache: true, parametros: null);

        return (EstadoDtm)cache[i];
    }

    public static EstadoDtm Estado(this enumNegocio negocio, ContextoSe contexto, string nombre, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true, bool aplicarJoin = true)
    {
        var registros = contexto.CrearGestorDeEstados(negocio).LeerRegistro(nameof(INombre.Nombre), nombre, errorSiNoHay, errorSiHayMasDeUno, aplicarJoin);
        return (EstadoDtm)registros;
    }

    public static TransicionDtm Transicion(this enumNegocio negocio, ContextoSe contexto, int id, bool usarLaCache = true)
    => negocio.ListaDeTransiciones(contexto).First(t => t.Id == id);
    //(TransicionDtm)CrearGestorDeTransiciones(contexto, negocio).LeerRegistroPorId(id, true, usarLaCache);

    public static TransicionDtm Transicion(this enumNegocio negocio, ContextoSe contexto, string nombre, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true)
    {
        var registros = CrearGestorDeTransiciones(contexto, negocio).LeerRegistro(nameof(INombre.Nombre), nombre, errorSiNoHay, errorSiHayMasDeUno, aplicarJoin: true);
        return (TransicionDtm)registros;
    }

    public static TransicionDtm Transicion(this enumNegocio negocio, ContextoSe contexto, string transicion, string estadoOrigen, string estadoDestino, bool delSistema = false, bool activa = true, bool errorSiNoHay = true)
    {
        var gestor = CrearGestorDeTransiciones(contexto, negocio);
        var f1 = new ClausulaDeFiltrado(ltrTransiciones.filtroPorEstados, enumCriteriosDeFiltrado.igual, $"{estadoOrigen};{estadoDestino}");
        var f2 = new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.igual, transicion);
        var f3 = new ClausulaDeFiltrado(nameof(TransicionDtm.DelSistema), enumCriteriosDeFiltrado.igual, delSistema);
        var f4 = new ClausulaDeFiltrado(nameof(TransicionDtm.Activo), enumCriteriosDeFiltrado.igual, activa);
        var leidos = (List<TransicionDtm>)gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { f1, f2, f3, f4 }, aplicarJoin: false);
        if (leidos.Count() == 0 && errorSiNoHay)
        {
            GestorDeErrores.Emitir($"No se ha encontrado una transición {(delSistema ? "del sistema" : "de usuario")} llamada '{transicion}' desde '{estadoOrigen}' hasta '{estadoDestino}' que esté '{(activa ? "activa" : "no activa")}' ");
        }

        if (leidos.Count() > 1)
        {
            GestorDeErrores.Emitir($"Se han encontrado más de una transición {(delSistema ? "del sistema" : "de usuario")} llamada '{transicion}' desde '{estadoOrigen}' hasta '{estadoDestino}' que esté '{(activa ? "activa" : "no activa")}' ");
        }

        return leidos[0];
    }

    public static List<T> SeleccionarReferenciados<T>(this enumNegocio negocio, ContextoSe contexto, string propiedad, int valor, bool aplicarJoin = false)
    where T : RegistroDtm
    =>
    negocio.SeleccionarPorFiltro<T>(contexto,
        new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(propiedad, enumCriteriosDeFiltrado.igual, valor) }
        , aplicarJoin: aplicarJoin
        , parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });

    public static List<T> SeleccionarPorFiltro<T>(this enumNegocio negocio, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
    where T : RegistroDtm
    {
        var gestor = negocio.CrearGestor(contexto, typeof(T));
        var registros = (List<T>)gestor.LeerRegistros(parametros.LeerValor(ltrParametrosNeg.PosicionInicial, 0), parametros.LeerValor(ltrParametrosNeg.CantidadPorLeer, -1), filtros, aplicarJoin, parametros);
        return registros is null ? new List<T> { } : registros;
    }

    public static List<RegistroDtm> SeleccionarRegistros(this enumNegocio negocio, ContextoSe contexto, List<ClausulaDeFiltrado> filtros, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
    {
        var registros = negocio.CrearGestor(contexto).Registros(0, -1, filtros, aplicarJoin, parametros);
        return registros.ToList();
    }

    public static IRegistro MapearDtm(this enumNegocio negocio, ContextoSe contexto, IElementoDto elemento)
    {
        var gestor = CrearGestor(contexto, negocio);
        return gestor.MapearDtm(elemento, new ParametrosDeNegocio(enumTipoOperacion.MapearElDtoAlDtm));
    }

    public static IElementoDto MapearDto(this enumNegocio negocio, ContextoSe contexto, IRegistro registro)
    {
        var gestor = CrearGestor(contexto, negocio);
        return gestor.MapearDto(registro, new ParametrosDeNegocio(enumTipoOperacion.MapearElDtmAlDto));
    }


    public static void ValidarAccesoPorEstado(this enumNegocio negocio, ContextoSe contexto, int idRegistro, bool validarSiTerminado = true, bool validarSiCancelado = true)
    {
        if (!negocio.UsaFlujo())
            return;

        var registro = (ElementoDeProcesoDtm)negocio.LeerRegistro(contexto, idRegistro);
        if (validarSiTerminado)
        {
            var terminados = negocio.TipoDtm().Terminados(contexto);
            if (terminados.Contains(registro.IdEstado))
                GestorDeErrores.Emitir($"El '{registro.Referencia}' no es editable ya que el estado '{registro.Estado(contexto).Nombre}' está terminado");
        }

        if (validarSiCancelado)
        {
            var cancelados = negocio.TipoDtm().Cancelados(contexto);
            if (cancelados.Contains(registro.IdEstado))
                GestorDeErrores.Emitir($"El '{registro.Referencia}' no es editable ya que el estado '{registro.Estado(contexto).Nombre}' está cancelado");
        }

        var modoDeAcceso = ApiDePermisos.LeerModoDeAcceso(contexto, negocio, idRegistro);
        if (modoDeAcceso == enumModoDeAccesoDeDatos.Consultor && !registro.EsGestor(contexto))
            GestorDeErrores.Emitir($"El '{registro.Referencia}' no es editable ya que no tiene acceso al estado '{registro.Estado(contexto).Nombre}'");
    }


    public static List<CentroGestorDtm> CentrosGestorsConAccesoDe(this enumNegocio negocio, ContextoSe contexto, enumTipoPermiso tipoPermiso)
    {
        var query = contexto.DatosDeConexion.EsAdministrador ?
            contexto.Set<CentroGestorDtm>().Where(cg => cg.Baja == false).OrderBy(t => t.Codigo) :
            contexto.Set<PermisosPorCgDtm>()
            .Join(contexto.Set<CentroGestorDtm>(), join1 => join1.IdCg, cg => cg.Id, (t1, t3) => new { t1, t3 })
            .Join(contexto.Set<PermisoDtm>(), join2 => join2.t1.IdPermiso, t2 => t2.Id, (t2, t4) => new { t2, t4 })
            .Join(contexto.Set<TipoPermisoDtm>(), join3 => join3.t4.IdTipo, t6 => t6.Id, (t5, t6) => new { t5, t6 })
            .Where(t => t.t5.t2.t1.IdNegocio == negocio.IdNegocio() &&
                        t.t5.t2.t1.IdUsuario == contexto.DatosDeConexion.IdUsuario &&
                        t.t6.Nombre.Equals(tipoPermiso.ToString()) &&
                        t.t5.t2.t3.Baja == false
                        )
            .OrderBy(t => t.t5.t2.t3.Codigo)
            .Select(t => t.t5.t2.t3)
            .Distinct();

        return query.ToList();
    }

    public static bool EsConsultor(this enumNegocio negocio, ContextoSe Contexto)
    {
        if (Contexto.DatosDeConexion.EsAdministrador) return true;
        var negocioDtm = negocio.LeerNegocio();
        var clase = Contexto.Set<ClasePermisoDtm>().Where(tipo => tipo.Nombre == ClaseDePermiso.ToString(enumClaseDePermiso.Negocio));
        var permisos = Contexto.Set<UsuariosDeUnPermisoDtm>().Where(p => p.IdUsuario == Contexto.DatosDeConexion.IdUsuario && clase.Any(c => c.Id == p.Permiso.Clase.Id));
        return Contexto.Set<UsuariosDeUnPermisoDtm>().Any(p => p.IdUsuario == Contexto.DatosDeConexion.IdUsuario && (negocioDtm.IdConsultor == p.IdPermiso || negocioDtm.IdGestor == p.IdPermiso || negocioDtm.IdAdministrador == p.IdPermiso));
    }

    public static bool EsGestor(this enumNegocio negocio, ContextoSe Contexto)
    {
        if (Contexto.DatosDeConexion.EsAdministrador) return true;
        var negocioDtm = negocio.LeerNegocio();
        var clase = Contexto.Set<ClasePermisoDtm>().Where(tipo => tipo.Nombre == ClaseDePermiso.ToString(enumClaseDePermiso.Negocio));
        var permisos = Contexto.Set<UsuariosDeUnPermisoDtm>().Where(p => p.IdUsuario == Contexto.DatosDeConexion.IdUsuario && clase.Any(c => c.Id == p.Permiso.Clase.Id));
        return Contexto.Set<UsuariosDeUnPermisoDtm>().Any(p => p.IdUsuario == Contexto.DatosDeConexion.IdUsuario && (negocioDtm.IdGestor == p.IdPermiso || negocioDtm.IdAdministrador == p.IdPermiso));
    }

    public static bool EsAdministrador(this enumNegocio negocio, ContextoSe Contexto)
    {
        if (Contexto.DatosDeConexion.EsAdministrador) return true;
        var negocioDtm = negocio.LeerNegocio();
        var clase = Contexto.Set<ClasePermisoDtm>().Where(tipo => tipo.Nombre == ClaseDePermiso.ToString(enumClaseDePermiso.Negocio));
        var permisos = Contexto.Set<UsuariosDeUnPermisoDtm>().Where(p => p.IdUsuario == Contexto.DatosDeConexion.IdUsuario && clase.Any(c => c.Id == p.Permiso.Clase.Id));
        return Contexto.Set<UsuariosDeUnPermisoDtm>().Any(p => p.IdUsuario == Contexto.DatosDeConexion.IdUsuario && negocioDtm.IdAdministrador == p.IdPermiso);
    }

    public static List<T> TiposConAccesoDe<T>(this enumNegocio negocio, ContextoSe contexto, enumTipoPermiso tipoPermiso)
    where T : TipoDeElementoDtm
    {
        var query = contexto.Set<PermisosPorTipoDtm>()
            .Join(negocio.Tipos(contexto), join1 => join1.IdTipo, tipo => tipo.Id, (t1, t3) => new { t1, t3 })
            .Join(contexto.Set<PermisoDtm>(), join2 => join2.t1.IdPermiso, t2 => t2.Id, (t2, t4) => new { t2, t4 })
            .Join(contexto.Set<TipoPermisoDtm>(), join3 => join3.t4.IdTipo, t6 => t6.Id, (t5, t6) => new { t5, t6 })
            .Where(t => t.t5.t2.t1.IdNegocio == negocio.IdNegocio() &&
                        t.t5.t2.t1.IdUsuario == contexto.DatosDeConexion.IdUsuario &&
                        t.t6.Nombre.Equals(tipoPermiso.ToString()) &&
                        t.t5.t2.t3.Activo == true
                        )
            .OrderBy(t => t.t5.t2.t3.IdPadre).ThenBy(t => t.t5.t2.t3.Nombre)
            .Select(t => t.t5.t2.t3)
            .Distinct();

        return query.Cast<T>().ToList();

        /*         
           begin
           DECLARE @__IdNegocio_0 int = 32;
           DECLARE @__ToString_1 varchar(30) = 'Gestor';
           
           SELECT distinct [c].* 
           FROM [SEGURIDAD].[PERMISO_POR_TIPO] AS [p]
           INNER JOIN [SISDOC].[archivador_tipo] AS [c] ON [p].ID_TIPO = [c].[ID]
           INNER JOIN [SEGURIDAD].[PERMISO] AS [p0] ON [p].[ID_PERMISO] = [p0].[ID]
           INNER JOIN [SEGURIDAD].[TIPO_PERMISO] AS [t] ON [p0].[IDTIPO] = [t].[ID]
           WHERE (([p].[ID_NEGOCIO] = @__IdNegocio_0) AND ([p].[ID_USUARIO] = 6)) AND ([t].[NOMBRE] = @__ToString_1)
           ORDER BY c.ID_PADRE, [c].NOMBRE
           end
           go
         */
    }

    public static bool EstaEnEtapaDeConsulta<T>(this enumNegocio negocio, ContextoSe contexto, T elemento)
    {
        if (!negocio.UsaFlujo())
            return false;

        var gestor = negocio.CrearGestor(contexto, typeof(T));
        var metodo = ApiDeEnsamblados.MetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio, gestor.GetType().FullName, ApiDeEnsamblados.EstaEnEtapaDeConsulta);
        if (metodo != null)
        {
            object[] parametros = new object[] { contexto, elemento };
            return (bool)metodo.Invoke(null, parametros);
        }

        return false;
    }

    public static IUsaTipoConCG NuevoDtm(this enumNegocio negocio, ContextoSe contexto, int idCg, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros)
    {
        var gestor = negocio.CrearGestor(contexto);
        MethodInfo importarDelCorreoMethod = gestor.GetType().GetMethod(nameof(IImportadorDelCorreo.ImportarDelCorreo), BindingFlags.Public | BindingFlags.Instance);
        return (IUsaTipoConCG)importarDelCorreoMethod.Invoke(gestor, [idCg, idTipo, nombre, descripcion, parametros]);
    }

    public static List<EtapaDto> ListaDeEtapas(this enumNegocio negocio, bool erroSiNoHay = true)
    {
        if (erroSiNoHay && !negocio.UsaFlujo())
        {
            GestorDeErrores.Emitir($"El negocio '{negocio}' no implementa etapas");
        }

        var resultado = new List<EtapaDto>();
        var metadatos = negocio.ObtenerMetadatos();

        if (metadatos.TipoEtapas is not null)
        {
            // Obtenemos todos los valores del enumerado definido en los metadatos
            var valores = Enum.GetValues(metadatos.TipoEtapas);

            foreach (var valor in valores)
            {
                // Convertimos el valor genérico al tipo Enum para acceder a los métodos de extensión
                var enumerado = (Enum)valor;

                resultado.Add(new EtapaDto
                {
                    nombre = enumerado.ToString(),
                    descripcion = enumerado.Descripcion()
                });
            }
        }

        return resultado;
    }
}
