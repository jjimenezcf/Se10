using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.SistemaDocumental;
using ModeloDeDto;
using ModeloDeDto.Presupuesto;
using ModeloDeDto.Reporte;
using ModeloDeDto.Terceros;
using Newtonsoft.Json;
using ServicioDeDatos;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System.Collections.Generic;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{
    public class GeneradorDePresupuestoRpt: IGeneradorRpt<PresupuestoDto>
    {
        private ContextoSe Contexto { get; }
        private PresupuestoDtm Presupuesto { get; }

        public GeneradorDePresupuestoRpt(ContextoSe contexto, PresupuestoDtm presupuesto)
        {
            Contexto = contexto;
            Presupuesto = presupuesto;
        }

        public IInformacionRpt<PresupuestoDto> ObtenerInformacionDeRpt(string plantilla)
        {
            var informacionRpt = new PresupuestoRpt();
            var cg = Presupuesto.Cg(Contexto,aplicarJoin: true);
            informacionRpt.Datos = Presupuesto.MapearDto<PresupuestoDto>(Contexto);
            informacionRpt.Lineas = new List<LineaDeUnPptDto>();
            var lineas = Presupuesto.Detalles<LineaDeUnPptDtm>(Contexto, aplicarJoin: true);
            foreach (var linea in lineas)
            {
                informacionRpt.Lineas.Add(linea.MapearDto<LineaDeUnPptDto>(Contexto));
                informacionRpt.IncluirIva(linea);
            }            
            
            informacionRpt.Sociedad = Contexto.SeleccionarDto<SociedadDto,SociedadDtm>(
                          id: cg.IdSociedad, 
                          parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.ObtenerDatosFiscales, true } }));

            informacionRpt.Solicitante = Contexto.SeleccionarDto<InterlocutorDto, InterlocutorDtm>(id: Presupuesto.IdSolicitante);

            //parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, new Dictionary<string, object> { { ltrParametrosNeg.LeerDireccionDeContacto, true } }));

            informacionRpt.Direccion = Presupuesto.DireccionDeEjecucion(Contexto, errorSiNoHay: false);

            informacionRpt.RazonSocialDelCliente = Presupuesto.Solicitante(Contexto).RazonSocial(Contexto);

            informacionRpt.Logo =  informacionRpt.Sociedad.IdArchivo is null
            ? ApiDeArchivos.FicheroNoEncontrado 
            : ServidorDocumental.DescargarArchivo(Contexto, (int)informacionRpt.Sociedad.IdArchivo, solicitadoPorLaCola: false, erroSiNoEstaEnLaruta: false);

            var datosSocietarios = (ParametrosDeMiSociedadDtm)Presupuesto.Sociedad(Contexto).SeleccionarAmpliacion(Contexto, typeof(ParametrosDeMiSociedadDtm), errorSiNoHay: false);
            if (datosSocietarios is null)
                GestorDeErrores.Emitir($"Debe definir los datos societarios de la sociedad '{informacionRpt.Sociedad.Nif}'");
            if (datosSocietarios.PieDePresupuesto.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Debe definir en los datos societarios de la sociedad '{informacionRpt.Sociedad.Nif}' el pie que se debe mostrar al imprimir el presupuesto");
            informacionRpt.PieDePresupuesto = datosSocietarios.PieDePresupuesto;
            if (datosSocietarios.InscritoEn.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Debe definir en los datos societarios de la sociedad '{informacionRpt.Sociedad.Nif}' la información del registro mercantil de la socieda");
            informacionRpt.InscritoEn = datosSocietarios.InscritoEn;

            var datos = enumNegocio.Presupuesto.LeerCrearParametro(Contexto, enumParametrosDePresupuesto.PPT_DatosDeImpresion, valor: ltrParametrosRpt.ParametrosPorDefecto());
            informacionRpt.Parametros = JsonConvert.DeserializeObject<Dictionary<string, object>>(datos.Valor);
            if (!informacionRpt.VerificarVersionDeParametros())
            {
                informacionRpt.ActualizarVersionDeParametros(enumNegocio.Presupuesto.IdNegocio(), enumParametrosDePresupuesto.PPT_DatosDeImpresion);
            }

            return informacionRpt;
        }


    }
}
