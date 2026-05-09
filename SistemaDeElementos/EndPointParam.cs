using ModeloDeDto;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers;

public enum enumEstadoPeticion { Ok, Error }

public class Resultado
{
    public enumEstadoPeticion Estado { get; set; }
    public string Mensaje { get; set; }
    public string Consola { get; set; }
    public int Total { get; set; } = 0;
    public dynamic Datos { get; set; }
    public string ModoDeAcceso { get; set; }
    public bool logout { get; set; } = false;
}

public class ResultadoConsola 
{
    public List<ClausulaDeFiltrado> FiltrosDePantalla { get; set; }
    public List<ClausulaDeFiltrado> FiltrosDeIa { get; set; }
    public string TextoNatural { get; set; }
};

public class ResultadoHtml : Resultado
{
    public string Html { get; set; }
}

public class ResultadoDeLectura<T> where T : ElementoDto
{
    public List<T> registros { get; set; }
    public int posicion;
    public int cantidad;
    public int total { get; set; }
    public string Mensaje { get; set; }

    public ResultadoDeLectura()
    {

    }

    public ResultadoDeLectura(IEnumerable<IElementoDto> elementos, int posicion, int cantidad, int total)
    {
        registros = elementos.ToList().ConvertAll(x => (T)x);
        this.posicion = posicion;
        this.cantidad = cantidad;
        this.total = total;
    }
}

public class DatosPropuestos
{
    public List<CentroGestorDtm> CGsAccesibles {get; set; }
    public CentroGestorDtm CGPropuesto { get; set; }
    public List<TipoDeElementoDtm> TiposAccesibles { get; set; }
    public TipoDeElementoDtm TipoPropuesto { get; set; }

    public string Nombre { get; set; }
    public string Descripcion { get; set;}

    public Dictionary<string,object> Otros { get; set; }

    public List<PlantillaDeCreacionDto> Plantillas { get; set; }
}

public class DatosParaCreacion
{
    public DatosPropuestos datosPropuestos { get; set; }
}


public class DatosParaElMantenimiento
{
    public List<EstadoDeEspan> Espanes { get; set; }
    public List<PlantillaDeFiltradoDto> Filtros { get; set; }
    public Dictionary<string, object> Indicadores { get; }

    public DatosParaElMantenimiento(List<EstadoDeEspan> espanes, List<PlantillaDeFiltradoDto> filtros, Dictionary<string, object>  indicadores)
    {
        Espanes = espanes;
        Filtros = filtros;
        Indicadores = indicadores;
    }
}

public class LeerVinculosConParam
{
    public int IdNegocio { get; set; }
    public int IdVinculado { get; set; }
    public int IdElemento1 { get; set; }
    public IEnumerable<Parametro> ParametrosJson { get; set; }
}

public class LoginInput
{
    public string UserName { get; set; }
    public string Password { get; set; }
}

public class LoginResultOutput
{
    public string UserName { get; set; }
    public string Name { get; set; }
    public string Surname { get; set; }
}

public class LeerDatosParaElGridParam
{
    //public string Modo { get; set; }
    public epAcciones? Accion { get; set; } = epAcciones.buscar;
    public int Posicion { get; set; }
    public int? Cantidad { get; set; } = 100;
    public IEnumerable<Filtro> Filtro { get; set; }
    public IEnumerable<Orden> Orden { get; set; }
    public IEnumerable<Parametro> Parametros { get; set; }
}

public class Filtro
{
    public string Clausula { get; set; }
    public enumCriteriosDeFiltrado Criterio { get; set; }
    public string Valor { get; set; }
}

public class Orden
{
    public string OrdenarPor { get; set; }
    public ModoDeOrdenancion Modo { get; set; }
}

