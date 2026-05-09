using AutoMapper;
using ServicioDeDatos.Entorno;
using ServicioDeDatos;
using ModeloDeDto.Entorno;
using GestorDeElementos;
using Utilidades;
using Gestor.Errores;
using System.Collections.Generic;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Entorno
{
    public class GestorDeAcciones : GestorDeElementos<ContextoSe, AccionDtm, AccionDto>
    {
        public override enumNegocio Negocio => enumNegocio.Accion;

        public class MapearAcciones : Profile
        {
            public MapearAcciones()
            {
                CreateMap<AccionDtm, AccionDto>();
                CreateMap<AccionDto, AccionDtm>();
            }
        }

        public GestorDeAcciones(ContextoSe contexto, IMapper mapeador)
            : base(contexto, mapeador)
        {
        }


        public static GestorDeAcciones Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAcciones(contexto, mapeador);
        }

        public  static AccionDtm LeerAccion(ContextoSe contexto, string nombre)
        {
            var f = new ClausulaDeFiltrado(nameof(AccionDtm.Nombre), enumCriteriosDeFiltrado.igual, nombre);
            var accion = Gestor(contexto, contexto.Mapeador).LeerRegistro(new List<ClausulaDeFiltrado> { f }, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo), true, true);
            return accion;
        }

        protected override void AntesDePersistir(AccionDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);
            if (parametros.AccionQueSeEjecuta != enumAccionesSistemaDocumental.RenombrarPlantilla && (parametros.Operacion == enumTipoOperacion.Insertar || parametros.Operacion == enumTipoOperacion.Modificar))
                ValidarAccion(registro);
        }

        private void ValidarAccion(AccionDtm registro)
        {
            switch (ApiDeEnsamblados.ToEnumerado<enumClaseDeAccion>(registro.ClaseDeAccion))
            {
                case enumClaseDeAccion.DLL:
                    if (!EsDll(registro)) GestorDeErrores.Emitir("Un programa lo define una dll, una clase y un método");
                    ApiDeEnsamblados.ObtenerMetodoEstatico(registro.Dll, registro.Clase, registro.Metodo);
                    break;
                case enumClaseDeAccion.PA:
                    if (!EsPa(registro)) GestorDeErrores.Emitir("Un PA lo define un esquema y un nombre de procedimiento almacenado");
                    Contexto.ValidarExistePa(registro.Esquema, registro.Pa);
                    break;

                case enumClaseDeAccion.SQL:
                    if (!EsSql(registro)) GestorDeErrores.Emitir("Un bloque Sql lo define sólo el campo 'bloque Sql'");
                    GestorDeMetadatos.ValidarSql(registro.Sql);
                    break;
                default:
                    GestorDeErrores.Emitir("Acción mal definida");
                    break;
            }
        }

        protected override void DespuesDeMapearElElemento(AccionDtm registro, AccionDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.ModoDeAcceso = ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.Consultor;
        }

        private static bool EsPa(AccionDtm a) => !a.Pa.IsNullOrEmpty() && !a.Esquema.IsNullOrEmpty()
                                        && a.Dll.IsNullOrEmpty() && a.Clase.IsNullOrEmpty() && a.Metodo.IsNullOrEmpty()
                                        && a.Sql.IsNullOrEmpty();

        private static bool EsSql(AccionDtm a) => a.Pa.IsNullOrEmpty() && a.Esquema.IsNullOrEmpty()
                                        && a.Dll.IsNullOrEmpty() && a.Clase.IsNullOrEmpty() && a.Metodo.IsNullOrEmpty()
                                        && !a.Sql.IsNullOrEmpty();
        private static bool EsDll(AccionDtm a) => a.Pa.IsNullOrEmpty() && a.Esquema.IsNullOrEmpty()
                                        && !a.Dll.IsNullOrEmpty() && !a.Clase.IsNullOrEmpty() && !a.Metodo.IsNullOrEmpty()
                                        && a.Sql.IsNullOrEmpty();

    }

}
