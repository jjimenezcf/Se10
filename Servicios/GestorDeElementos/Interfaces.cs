using AutoMapper;
using ModeloDeDto;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Utilidades;

namespace GestorDeElementos
{
    public interface IGestorGenerico
    {
        public void AsignarNegocio(enumNegocio negocio);
    }

    public interface IEsImputable
    {
        public (bool EstabaSinImputar, string Mensaje) Imputar(int id, enumNegocio negocio, int idDondeImputar);
    }

    public interface IImportadorDelCorreo
    {
        public IUsaTipoConCG ImportarDelCorreo(int idCg, int idTipo, string nombre, string descripcion, Dictionary<string, object> parametros);
    }

    public interface IGestor
    {
        public ContextoSe Contexto { get; }

        public IMapper Mapeador { get; }

        public string NombreDeLaClaseDto { get; }
        public string NombreDeLaClaseDtm { get; }
        public Type TipoDeNegocioDto { get; }
        public Type TipoDtmDelNegocio { get; }

        public Metadatos Metadatos { get; }

        public IGestorDeTipos GestorDeTipos { get; }

        public enumNegocio Negocio { get; }

        public ElementoDto MapearDto(IRegistro registro, ParametrosDeNegocio parametros);

        public RegistroDtm MapearDtm(IElementoDto elemento, ParametrosDeNegocio parametros);

        public object LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo);

        public IEnumerable<RegistroDtm> Registros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, bool aplicarJoin, Dictionary<string, object> parametros = null);

        public object LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, bool aplicarJoin, Dictionary<string, object> parametros = null);
        public object LeerRegistros(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, ParametrosDeNegocio parametros);
        public object LeerRegistroPorId(int id, bool aplicarJoin, bool usarLaCache = true, Dictionary<string, object> parametros = null);

        public object LeerRegistro(string propiedad, string valor, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true, bool aplicarJoin = true);

        public object LeerElementoPorId(int id, Dictionary<string, object> opcionesDelMapeo);
        public object TransitarElemento(object elemento, int idTransicion, Dictionary<string, object> parametros);
        public object TransitarRegistro(object registro, int idTransicion, Dictionary<string, object> parametros);

        public object EliminarRegistroPorId(int id, Dictionary<string, object> parametros);
        public int Contar(List<ClausulaDeFiltrado> clausulaDeFiltrados, ParametrosDeNegocio parametros = null);
        public bool Existen(List<ClausulaDeFiltrado> clausulaDeFiltrados, ParametrosDeNegocio parametros = null);
        public object ValidarPermisosDePersistencia(enumTipoOperacion operacion, enumNegocio negocio, int id, Dictionary<string, object> parametros = null);

        public object PersistirElementoDto(object elementoDto, ParametrosDeNegocio parametros);
        public object PersistirRegistro(object registro, ParametrosDeNegocio parametrosDeNegocio);

        public bool IniciarTransaccion();
        public void Commit(bool transaccion);
        public void Rollback(bool transaccion);
        dynamic DeserializarDto(string elementoJson);
    }

    public interface IGestorDeTipos : IGestor
    {
        public JerarquiaDto LeerJerarquia(enumNegocio negocio, int? idPadre, string filtrosJson);
    }

    public interface IGeneradorRpt<T>
    {
        public IInformacionRpt<T> ObtenerInformacionDeRpt(string plantilla);
    }

    public class ltrTotalizador
    {
        public const string Menu_MostrarTotales = "Mostrar totales";
    }

    public interface ITotalizador<T>
    {
        public Task<T> ObtenerTotalesAsync(List<ClausulaDeFiltrado> filtros, int posicion, int cantidad);
    }
    public interface IGeneradorDePreasiento
    {
        public void GenerarPreasiento(List<int> ids);
    }


    }
