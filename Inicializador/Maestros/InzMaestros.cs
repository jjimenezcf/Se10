using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Callejero;
using GestoresDeNegocio.Terceros;
using ModeloDeDto.Callejero;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.AytoBeniel;
using SistemaDeElementos.Inicializador.Mra;
using System.Linq;
using Utilidades;

namespace SistemaDeElementos.Inicializador.Datos
{
    public static class InzMaestros
    {
        public const string n_prv_valencis = "Valéncia";
        public const string n_prv_murcia = "Murcia";
        public const string n_mun_alcantarilla = "Alcantarilla";
        public const string n_mun_murcia = "Murcia";
        public const string n_mun_cullera = "Cullera";
        public static readonly string n_unidad_ud = "Ud";
        public static readonly string n_unidad_m3 = "M3";
        public static readonly string n_natu_limpieza = "LP";
        public static readonly string n_natu_consultoria = "EST";
        public static readonly string n_natu_material = ltrSiglasDeNaturalezas.MAT;
        public static readonly string n_natu_reparacion = "REP";

        public static void CrearCallejo(ContextoSe contexto)
        {
            var espana = GestorDePaises.CrearPais(contexto, "España", ltrIsoPaises.Spain, "ESP", "+34", "Spain");

            var murcia = espana.CrearProvincia(contexto, n_prv_murcia, "MU", "30", "968");
            var valencia = espana.CrearProvincia(contexto, n_prv_valencis, "V", "46", "96");
            murcia.CrearMunicipio(contexto, n_mun_alcantarilla, "8");
            murcia.CrearMunicipio(contexto, n_mun_murcia, ltrEstados.EstadoNulo);
            valencia.CrearMunicipio(contexto, n_mun_cullera, "6");
        }


        public static void CrearTerceros(ContextoSe contexto)
        {
            GestorDePersonas.CrearPersona(contexto, "27485405Z", "Juan", "Jiménez-Cervantes", "jjimenezcf@gmail.com", "619.70.25.47");
            var sociedad = GestorDeSociedades.CrearSociedad(contexto, "E73936932", "Procuraduria Jimenez-cervantes C.b.", "procuradores@jimenez-cervantes.com", "968 216 988");

            InzMra.Sociedad(contexto);
            InzMaestrosBeniel.Sociedad(contexto);

            var contactoPablo = sociedad.CrearContacto(contexto, "Pablo", "pablo@jimenez-cervantes.com", "968 216 988", "Jefe del despacho");
            var contactoJuan = sociedad.CrearContacto(contexto, "Juan", "Juan@jimenez-cervantes.com", "968 216 988", "Coordinador de asuntos");

            var pablo = contexto.SeleccionarDto<InterlocutorDto, InterlocutorDtm>(contactoPablo.IdInterlocutor.Entero());
            var juan = contexto.SeleccionarDto<InterlocutorDto, InterlocutorDtm>(contactoJuan.IdInterlocutor.Entero());

            pablo.CrearProcurador(contexto);
            juan.CrearProcurador(contexto);

            var mra = contexto.SeleccionarDtoPorAk<SociedadDto, SociedadDtm>(nameof(SociedadDtm.NIF), InzMra.n_nif_mra);
            var contactoManolo = mra.CrearContacto(contexto, "Manolo", "manuel.ramos@mra.com", "619.25.26.23", "Responsable de despacho");
            contexto.SeleccionarDto<InterlocutorDto, InterlocutorDtm>(contactoManolo.IdInterlocutor.Entero()).CrearAbogado(contexto);
        }

        public static void CrearJuzgados(ContextoSe contexto)
        {

            var primeInstancia = GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de Primera Instancia e Instrucción");
            var mercantil = GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de lo Mercantil");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de lo Penal");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de Violencia sobre la Mujer");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de lo Contencioso-Administrativo");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de lo Social");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de Vigilancia Penitenciaria");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de Menores");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Juzgados de Paz");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Tribunal Supremo");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Audiencia Nacional");
            GestorDeClasesDeJuzgado.CrearClaseDto(contexto, "Audiencias Provinciales");


            MunicipioDto murcia = GestorDeMunicipios.SeleccionarMunicipioDto(contexto, n_prv_murcia, n_mun_murcia);
            GestorDeJuzgados.CrearJuzgado(contexto, primeInstancia, "Nº3", murcia);


            MunicipioDto cullera = GestorDeMunicipios.SeleccionarMunicipioDto(contexto, n_prv_valencis, n_mun_cullera);
            GestorDeJuzgados.CrearJuzgado(contexto, mercantil, "Nº1", cullera);

        }

        public static void InicializarUnidadesDeMedida(ContextoSe contexto)
        {
            new UnidadDtm { Sigla = n_unidad_ud, Nombre = "Unidad" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "H", Nombre = "Hora" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "D", Nombre = "Día" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "MM", Nombre = "Mes" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "J", Nombre = "Jornada" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "HE", Nombre = "Hora extra" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "N", Nombre = "Noche" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "Kg", Nombre = "Kilo" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "M", Nombre = "Metro" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "M2", Nombre = "Metro cuadrado" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = n_unidad_m3, Nombre = "Metro cúbico" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
            new UnidadDtm { Sigla = "l", Nombre = "Litro" }.PersistirPorAk(contexto, nameof(UnidadDtm.Sigla));
        }

        public static void CrearNaturalezas(ContextoSe contexto)
        {
            new NaturalezaDtm { Sigla = n_natu_limpieza, Nombre = "Limpieza por jornada" }.PersistirPorAk(contexto, nameof(NaturalezaDtm.Sigla));
            new NaturalezaDtm { Sigla = "LM", Nombre = "Limpieza por metros" }.PersistirPorAk(contexto, nameof(NaturalezaDtm.Sigla));
            new NaturalezaDtm { Sigla = n_natu_consultoria, Nombre = "Estudios" }.PersistirPorAk(contexto, nameof(NaturalezaDtm.Sigla));
            new NaturalezaDtm { Sigla = n_natu_material, Nombre = "Materiales" }.PersistirPorAk(contexto, nameof(NaturalezaDtm.Sigla));
            new NaturalezaDtm { Sigla = n_natu_reparacion, Nombre = "Reparaciones" }.PersistirPorAk(contexto, nameof(NaturalezaDtm.Sigla));
        }

        public static void InicializarCuentasContables(ContextoSe contexto)
        {
            // sólo se crean las que no existen (ni por código ni por nombre): las existentes no se modifican
            InzContable.CrearCuentaSiNoExiste(contexto, ltrCuenta.ClienteCodigo, ltrCuenta.ClienteDescripcion);
            InzContable.CrearCuentaSiNoExiste(contexto, ltrCuenta.SueldosCodigo, ltrCuenta.SueldosDescripcion);
            InzContable.CrearCuentaSiNoExiste(contexto, ltrCuenta.ProveedorCodigo, ltrCuenta.ProveedorDescripcion);
            InzContable.CrearCuentaSiNoExiste(contexto, ltrCuenta.ConsultoriaCodigo, ltrCuenta.ServiciosProfesionales);

            InzContable.CrearCuentaSiNoExiste(contexto, ltrCuenta.IngresosConsultoriaCodigo, ltrCuenta.PrestacionDeServicios);

            InzContable.CrearCuentaSiNoExiste(contexto, ltrCuenta.IvaRepercutido, ltrCuenta.IvaRepercutidodescripcion);
            InzContable.CrearCuentaSiNoExiste(contexto, ltrCuenta.IvaSoportado, ltrCuenta.IvaSoportadodescripcion);
            InzContable.CrearCuentaSiNoExiste(contexto, ltrCuenta.IrpfPersonaFisica, ltrCuenta.IrpfPersonaFisicadescripcion);

            InzContable.InicializarCuentasDeAlmacenesYObras(contexto);
        }

        public static readonly string n_iva_general = enumClasesDeIvaRep.IRG.Descripcion();
        // Los ivas sólo se crean si no existe ninguno con su nombre: los existentes no se modifican (su porcentaje o su cuenta los puede haber
        // cambiado el usuario). La cuenta se busca por código y, si no está, por nombre (puede haberse creado con otro código, p.e. 477001)
        public static void CrearIvasRepercutidos(ContextoSe contexto)
        {
            CrearIvaRepercutidoSiNoExiste(contexto, enumClasesDeIvaRep.IRG, 21);
            CrearIvaRepercutidoSiNoExiste(contexto, enumClasesDeIvaRep.IRR, 10);
            CrearIvaRepercutidoSiNoExiste(contexto, enumClasesDeIvaRep.IRS, 4);
        }

        public static void CrearIvasSoportados(ContextoSe contexto)
        {
            CrearIvaSoportadoSiNoExiste(contexto, enumClasesDeIvaSop.ISG, 21, exento: false);
            CrearIvaSoportadoSiNoExiste(contexto, enumClasesDeIvaSop.ISR, 10, exento: false);
            CrearIvaSoportadoSiNoExiste(contexto, enumClasesDeIvaSop.ISS, 4, exento: false);
            CrearIvaSoportadoSiNoExiste(contexto, enumClasesDeIvaSop.NSJ, 0, exento: true);
            CrearIvaSoportadoSiNoExiste(contexto, enumClasesDeIvaSop.ISP, 0, exento: true);
        }

        private static void CrearIvaRepercutidoSiNoExiste(ContextoSe contexto, enumClasesDeIvaRep clase, decimal porcentaje)
        {
            var nombre = clase.Descripcion();
            if (contexto.Set<IvaRepercutidoDtm>().Any(x => x.Nombre == nombre))
                return;

            new IvaRepercutidoDtm
            {
                Clase = clase,
                Nombre = nombre,
                Porcentaje = porcentaje,
                Exento = false,
                IdCuenta = InzContable.LeerCuenta(contexto, ltrCuenta.IvaRepercutido, ltrCuenta.IvaRepercutidodescripcion).Id,
                DescripcionFiscal = null
            }.PersistirPorAk(contexto, nameof(IvaRepercutidoDtm.Nombre));
        }

        private static void CrearIvaSoportadoSiNoExiste(ContextoSe contexto, enumClasesDeIvaSop clase, decimal porcentaje, bool exento)
        {
            var nombre = clase.Descripcion();
            if (contexto.Set<IvaSoportadoDtm>().Any(x => x.Nombre == nombre))
                return;

            new IvaSoportadoDtm
            {
                Clase = clase,
                Nombre = nombre,
                Porcentaje = porcentaje,
                Exento = exento,
                IdCuenta = InzContable.LeerCuenta(contexto, ltrCuenta.IvaSoportado, ltrCuenta.IvaSoportadodescripcion).Id
            }.PersistirPorAk(contexto, nameof(IvaSoportadoDtm.Nombre));
        }
    }
}