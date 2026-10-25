using ModeloDeDto.Terceros;
using ServicioDeDatos.Contabilidad;
using System;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace ModeloDeDto.Contabilidad
{
    // Datos que pide la modal de crear cierre; al seleccionar la sociedad se proponen su último cierre, su contabilidad y dígitos
    // (ver GestorDeSociedades.DespuesDeMapearElElemento)
    [IUDto(MostrarExpresion = nameof(IUsaNombreDto.Nombre))]
    public class CrearCierreContableDto
    {
        //----------------------------------------------------------
        [IUPropiedad(Etiqueta = "Id de la sociedad", Visible = false)]
        public int IdSociedad { get; set; }

        [IUPropiedad(
            Etiqueta = "Sociedad",
            Ayuda = "Sociedad gestionada, que no es de un autónomo y de la que es interventor, a cerrar",
            TipoDeControl = enumTipoControl.ListaDinamica,
            GuardarEn = nameof(IdSociedad),
            Controlador = nameof(enumControladoresTerceros.Sociedades),
            SeleccionarDe = typeof(SociedadDto),
            VistaDondeNavegar = enumVistasTerceros.CrudSociedades,
            RestrictorFijo = ltrParametrosDto.Negocio + ";" + nameof(enumNegocio.Sociedad) + ";" + nameof(enumModoDeAccesoDeDatos.Gestor),
            Negocio = enumNegocio.Sociedad,
            LongitudMinimaParaBuscar = 1,
            MostrarExpresion = nameof(SociedadDto.Expresion),
            BuscarPor = ltrDeSociedad.FiltroParaSociedadesACerrar,
            Tipo = typeof(string),
            Obligatorio = true,
            Fila = 0,
            Columna = 0,
            EditableAlCrear = true,
            trasSeleccionar = "javascript:" + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.CierreContable_TrasSeleccionarSociedad) + "()",
            trasBlanquear = "javascript:" + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.CierreContable_TrasBlanquearSociedad) + "()")]
        public string Sociedad { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        // Fecha del último cierre de la sociedad (1 de enero del año en curso si no se ha cerrado nunca), solo informativa
        [IUPropiedad(
             Etiqueta = "Último cierre",
             Ayuda = "Fecha hasta la que está cerrada la sociedad",
             TipoDeControl = enumTipoControl.SelectorDeFecha,
             EditableAlCrear = false,
             Obligatorio = false,
             Fila = 1,
             Columna = 0)]
        public DateTime? UltimoCierre { get; set; }

        // Ha de ser posterior al último cierre y no posterior a hoy; su año es el ejercicio que se cierra
        [IUPropiedad(
             Etiqueta = "Cerrar hasta",
             Ayuda = "Fecha hasta la que se cierra la sociedad",
             TipoDeControl = enumTipoControl.SelectorDeFecha,
             EditableAlCrear = true,
             Obligatorio = true,
             Fila = 1,
             Columna = 1)]
        public DateTime CerrarHasta { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        // No es obligatoria en la lista para que muestre "Seleccionar..." mientras no haya sociedad; se valida en el servidor
        [IUPropiedad(
            Etiqueta = "Contabilizar en",
            Ayuda = "Seleccionar...",
            TipoDeControl = enumTipoControl.Enumerado,
            Tipo = typeof(enumSistemaContable),
            GuardarEn = nameof(Contabilidad),
            Obligatorio = false,
            EditableAlCrear = true,
            Fila = 1,
            Columna = 2,
            OnChange = "javascript:" + nameof(enumNameSpaceTs.Contabilidad) + "." + nameof(enumFunctionTs.CierreContable_TrasCambiarContabilidad) + "()")]
        public string Contabilidad { get; set; }

        //-------------------------------------------------------------------------------------------------------------
        // Ncs siempre 11 (bloqueado); ContaSol los de la sociedad, 8 si no tiene sistema contable
        [IUPropiedad(
             Etiqueta = "Dígitos",
             Ayuda = "Dígitos de las cuentas auxiliares en la contabilidad",
             EditableAlCrear = true,
             Tipo = typeof(int),
             TipoDeControl = enumTipoControl.Editor,
             Alineada = enumAliniacion.derecha,
             Obligatorio = true,
             Fila = 1,
             Columna = 3,
             AnchoMaximo = "100px",
             LongitudMaxima = 2)]
        public int Digitos { get; set; }
    }
}
