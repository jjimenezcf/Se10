using GestorDeElementos;
using GestoresDeNegocio.MaestrosTecnico;
using ModeloDeDto.MaestrosTecnico;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.MaestrosTecnico;
using SistemaDeElementos.Inicializador;
using SistemaDeElementos.Inicializador.Datos;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    public class TestConMaestros
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirMaestros()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                CrearMaestros(contexto);
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        internal static void CrearMaestros(ContextoSe contexto)
        {
            InzAcciones.DefinirAcciones(contexto);
            InzMaestros.InicializarUnidadesDeMedida(contexto);
            InzMaestros.CrearNaturalezas(contexto);
            InzMaestros.InicializarCuentasContables(contexto);
            InzMaestros.CrearIvasRepercutidos(contexto);
        }


        internal static void CrearUnitariosEjemplos(ContextoSe contexto)
        {
            CrearServicio(contexto, "servicio 1", (decimal)100.50, (decimal)202.9);
            CrearServicio(contexto, "servicio 2", (decimal)140.50, (decimal)222.9);
            CrearServicio(contexto, "servicio 3", (decimal)170.50, (decimal)282.9);
            CrearMaterial(contexto, "material 1", (decimal)200.50, (decimal)402.9);
            CrearMaterial(contexto, "material 2", (decimal)240.50, (decimal)422.9);
            CrearMaterial(contexto, "material 3", (decimal)270.50, (decimal)482.9);
        }

        private static UnitarioDto CrearServicio(ContextoSe contexto, string nombre, decimal coste, decimal venta)
        {
            var servicio = new UnitarioDto
            {
                Nombre = nombre,
                Clase = enumClaseUnitario.Servicio,
                IdUnidad = contexto.SeleccionarPorPropiedad<UnidadDtm>(nameof(UnidadDtm.Sigla), InzMaestros.n_unidad_ud).Id,
                IdNaturaleza = contexto.SeleccionarPorPropiedad<NaturalezaDtm>(nameof(NaturalezaDtm.Sigla), InzMaestros.n_natu_consultoria).Id,
                Descripcion = "...",
                Coste = coste,
                Venta = venta,
                Referencia = enumClaseUnitario.Servicio.Prefijo() + "." + InzMaestros.n_natu_consultoria,
                Proponer = true
            };
            var p = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
            servicio = GestorDeUnitarios.Gestor(contexto, contexto.Mapeador).PersistirElementoDto(servicio, p);
            return servicio;
        }

        private static UnitarioDto CrearMaterial(ContextoSe contexto, string nombre, decimal coste, decimal venta)
        {
            var material = new UnitarioDto
            {
                Nombre = nombre,
                Clase = enumClaseUnitario.Material,
                IdUnidad = contexto.SeleccionarPorPropiedad<UnidadDtm>(nameof(UnidadDtm.Sigla), InzMaestros.n_unidad_ud).Id,
                IdNaturaleza = contexto.SeleccionarPorPropiedad<NaturalezaDtm>(nameof(NaturalezaDtm.Sigla), InzMaestros.n_natu_material).Id,
                Descripcion = "...",
                Coste = coste,
                Venta = venta,
                Referencia = enumClaseUnitario.Material.Prefijo() + "." + InzMaestros.n_natu_material,
                Proponer = true
            };
            var p = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
            material = GestorDeUnitarios.Gestor(contexto, contexto.Mapeador).PersistirElementoDto(material, p);
            return material;
        }
    }
}
