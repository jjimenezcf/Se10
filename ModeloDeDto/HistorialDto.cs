using System;
using Utilidades;

namespace ModeloDeDto
{
    public static class ltrSucesosCss
    {
        public static readonly string suceso = nameof(suceso);
        public static readonly string hito = suceso + "-" + nameof(hito);
        public static readonly string observacion = suceso + "-" + nameof(observacion);
        public static readonly string tarea = suceso + "-" + nameof(tarea);
        public static readonly string archivador = suceso + "-" + nameof(archivador);
        public static readonly string archivo = suceso + "-" + nameof(archivo);
        public static readonly string presupuesto = suceso + "-" + nameof(presupuesto);
        public static readonly string correo = suceso + "-" + nameof(correo);
        public static readonly string evento = suceso + "-" + nameof(evento);
    }

    public static class ltrSucesosFiltros
    {
        public static readonly string suceso = nameof(suceso);
        public static readonly string excluir = nameof(excluir);
        public static readonly string excluidos = nameof(excluidos);
        public static readonly string referencia = nameof(referencia);
        public static readonly string nivel_2 = nameof(nivel_2);
    }

    public static class ltrSucesosExcluir
    {
        public static readonly string hitos = nameof(hitos);
        public static readonly string tareas = nameof(tareas);
        public static readonly string archivos = nameof(archivos);
        public static readonly string archivadores = nameof(archivadores);
        public static readonly string observaciones = nameof(observaciones);
        public static readonly string correos = nameof(correos);
        public static readonly string eventos = nameof(eventos);
        public static readonly string presupuestos = nameof(presupuestos);
        public static readonly string nivel2hitos = nameof(nivel2hitos);
        public static readonly string nivel2ob = nameof(nivel2ob);
        public static readonly string nivel2correos = nameof(nivel2correos);
        public static readonly string nivel2archivos = nameof(nivel2archivos);
        public static readonly string nivel2archivadores = nameof(nivel2archivadores);
        public static readonly string nivel2tareas = nameof(nivel2tareas);
    }

    public static class ltrAccionesDeSucesos
    {
        public static readonly string MostrarObservacion = nameof(MostrarObservacion);
        public static readonly string MostrarEvento = nameof(MostrarEvento);
        public static readonly string MostrarCorreo = nameof(MostrarCorreo);
        public static readonly string AbrirTarea = nameof(AbrirTarea);
        public static readonly string AbrirArchivador = nameof(AbrirArchivador);
        public static readonly string AbrirPpt = nameof(AbrirPpt);
        public static readonly string Descargar = nameof(Descargar);
        public static readonly string Auditoria = nameof(Auditoria);
    }



    [IUDto(AnchoEtiqueta = 20, AnchoSeparador = 5, MostrarExpresion = nameof(Suceso))]
    public class HistorialDto : ElementoDto
    {
        [IUPropiedad(
            Etiqueta = "IdElemento",
            Ayuda = "id del elemento",
            Tipo = typeof(int),
            Visible = false
            )
        ]
        public int IdRegistro { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "1em"
         , Etiqueta = "Clase"
         , EtiquetaGrid = " "
         , TipoDeControl = enumTipoControl.CirculoEnCelda
         , Alineada = enumAliniacion.centrada)]
        public string Clase { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "10em"
         , Etiqueta = "Ocurrido el"
         , Ordenar = true
         , OrdenarGridPor = nameof(OcurridoEl)
         , TipoDeControl = enumTipoControl.SelectorDeFechaHora
         , Alineada = enumAliniacion.centrada)]
        public DateTime OcurridoEl { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Elemento",
            EtiquetaGrid ="Referencia",
            Ayuda = "Elemento afectado",
            Tipo = typeof(string),
           TamanoFijo = "14em"
          )
        ]
        public string Elemento { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Suceso",
            Ayuda = "Suceso sobre el elemento",
            Tipo = typeof(string),
            PorAnchoMnt = 30
          )
        ]
        public string Suceso { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
            Etiqueta = "Usuario",
            Ayuda = "Usuario que creao el suceso",
            Tipo = typeof(string),
            TamanoFijo = "10em"
          )
        ]
        public string Usuario { get; set; }


        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(
           TamanoFijo = "10em"
         , Etiqueta = "Acción"
         , EtiquetaGrid = ""
         , TipoDeControl = enumTipoControl.Referencia
         , AccionRef = "javascript: " + nameof(enumNameSpaceTs.Crud) + "." + nameof(enumFunctionTs.Historial_EjecutarAccionAsociada) + "(numeroDeFila)"
         , Alineada = enumAliniacion.derecha)]
        public string Accion { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string AccionJs { get; set; }

        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Detalle { get; set; }
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public enumNegocio enumNegocio { get; set; }
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public string Negocio { get; set; }
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int IdNegocio { get; set; }
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int Nivel { get; set; }
        //------------------------------------------------------------------------------------------------------
        [IUPropiedad(Visible = false)]
        public int? IdElemento { get; set; }
    }
}
