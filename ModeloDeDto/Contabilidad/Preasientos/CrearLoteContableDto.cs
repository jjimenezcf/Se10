using ModeloDeDto.Terceros;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using System;
using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CrearLoteContableDto
    {
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la sociedad", Visible = false)]
        public int IdSociedad { get; set; }

        [IUPropiedad(
            Etiqueta = "Sociedad",
            Ayuda = "Sociedad a contabilizar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdSociedad),
            Controlador = nameof(enumControladoresTerceros.Sociedades),
            SeleccionarDe = typeof(SociedadDto),
            VistaDondeNavegar = enumVistasTerceros.CrudSociedades,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Sociedad) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.Sociedad,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(SociedadDto.Expresion),
            BuscarPor = ltrDeSociedad.FiltroParaSociedadesGestionadas,
            Tipo = typeof(string),
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true)]
        public string Sociedad { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Etiqueta = "Ejercicio contable",
             Ayuda = "Movimientos del ejercicio que se han de procesar",
             EditableAlCrear = true,
             Tipo = typeof(int),
             TipoDeControl = enumTipoControl.Editor,
             Alineada = enumAliniacion.derecha,
             Fila = 0,
             Columna = 1,
             AnchoMaximo = "200px",
             LongitudMaxima =4)]
        public int Ejercicio { get; set; }


        //-------------------------------------------------------------------------------------------------------------
        [IUPropiedad(
             Ayuda = "Indique la fecha de contabilización si quiere remplazar a la de los preasientos, si no, déjela nula",
             Etiqueta = "Fecha contable (remplaza la definida)",
             TipoDeControl = enumTipoControl.SelectorDeFecha,
             EditableAlCrear = true,
             Fila = 0,
             Columna = 1,
            Posicion = 1)]
        public DateTime? FechaContable { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Respetar fechas contables",
           Ayuda = "Si la fecha contable del movimiento es de un periodo pasado se traspasa el preasiento con dicha fecha, si no con la fecha primer día del periodo abierto",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           Fila = 1, Columna = 0)
        ]
        public bool RespetarFechaContable { get; set; }

        //--------------------------------------------
        [IUPropiedad(
           Etiqueta = "Descontabilizar (si lo están)",
           Ayuda = "Antes de generar el lote, descontabiliza los preasientos si lo están",
           TipoDeControl = enumTipoControl.Check,
           Obligatorio = false,
           css = enumCssControles.CheckEnLinea,
           ValorPorDefecto = false,
           Fila = 1, Columna =  0)
        ]
        public bool Descontabilizar { get; set; }

    }
}
