using GestorDeElementos;
using GestoresDeNegocio.Callejero;
using Inicializador.Expedientes;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.Acromur;
using GestorDeElementos.Extensores;
using System.Collections.Generic;
using Utilidades;
using ValidacionesBase;
using ServicioDeDatos.SistemaDocumental;
using GestoresDeNegocio.Expediente;
using System.Linq;
using Gestor.Errores;

namespace ValidacionesDeRn
{
    class TestProcesoFCT
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirProcesoFCT()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzExpedientesDeFacturacion.ProcesoFCT(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarProcesoFCTn()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                //InzExpedientesDeFacturacion.ModeloDeExpedienteDeFacturas(contexto);
                var expediente = CrearExpedienteDeFCT(contexto);
                var archivadores = expediente.Vinculados<ArchivadorDtm>(contexto).Where(x => x.Nombre.Contains(expediente.PrefijoArchivadorFCT()));
                if (archivadores.Count() != 2)
                    GestorDeErrores.Emitir("Deberían haber dos archivadores de facturación");

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_enero, mes: 1);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_enero, mes: 1);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_enero, mes: 1);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_febrero, mes: 2);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_febrero, mes: 2);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_febrero, mes: 2);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_marzo, mes: 3);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_marzo, mes: 3);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_marzo, mes: 3);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_abril, mes: 4);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_abril, mes: 4);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_abril, mes: 4);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_mayo, mes: 5);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_mayo, mes: 5);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_mayo, mes: 5);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_junio, mes: 6);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_junio, mes: 6);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_junio, mes: 6);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_julio, mes: 7);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_julio, mes: 7);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_julio, mes: 7);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_agosto, mes: 8);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_agosto, mes: 8);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_agosto, mes: 8);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_septiembre, mes: 9);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_septiembre, mes: 9);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_septiembre, mes: 9);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_octubre, mes: 10);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_octubre, mes: 10);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_octubre, mes: 10);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_noviembre, mes: 11);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_noviembre, mes: 11);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_noviembre, mes: 11);

                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_ejercicio, mes: 12);
                expediente = ReabrirElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_abrir_ejercicio, mes: 12);
                expediente = CerrarElMesDe(contexto, expediente, InzExpedientesDeFacturacion.n_tran_fct_cerrar_ejercicio, mes: 12);

                var relacionados = expediente.Vinculados<ExpedienteDtm>(contexto);
                if (relacionados.Count() != 1)
                    GestorDeErrores.Emitir("Deberían haber un expediente relacionado");
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        private static ExpedienteDtm CerrarElMesDe(ContextoSe contexto, ExpedienteDtm expediente, string transicion, int mes)
        {
            expediente = expediente.Transitar(contexto, transicion, parametros: new Dictionary<string, object> { { AccionesDeFacturacion.PermitirAperturaSiempre, true } });
            var archivadores = expediente.Vinculados<ArchivadorDtm>(contexto, new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } }).Where(x => x.Nombre.Contains(expediente.PrefijoArchivadorFCT())).ToList();
            if (archivadores.Count() != mes + 2 && archivadores.Count() < 12)
                GestorDeErrores.Emitir($"Deberían haber {mes + 2} archivadores de facturación");

            for (var i = 1; i <= mes; i++)
                for (var j = 0; j < archivadores.Count(); j++)
                    if (archivadores[j].Nombre == expediente.NombreDeArchivadorFCT(i))
                    {
                        if (!archivadores[j].Baja)
                            GestorDeErrores.Emitir($"El archivador de {extFechas.Mes(i)} debería estar de baja");
                        break;
                    }

            return expediente;
        }

        private static ExpedienteDtm ReabrirElMesDe(ContextoSe contexto, ExpedienteDtm expediente, string transicion, int mes)
        {
            expediente = expediente.Transitar(contexto, transicion, new Dictionary<string, object> { { ltrParametrosEp.detalleAsunto, $"Reabriendo el mes de {extFechas.Mes(mes)}" } });
            var archivadores = expediente.Vinculados<ArchivadorDtm>(contexto, new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } }).Where(x => x.Nombre.Contains(expediente.PrefijoArchivadorFCT())).ToList();
            bool activo = false;
            for (var j = 0; j < archivadores.Count(); j++)
                if (archivadores[j].Nombre == expediente.NombreDeArchivadorFCT(mes))
                {
                    if (archivadores[j].Baja)
                        GestorDeErrores.Emitir($"El archivador de {extFechas.Mes(mes)} debería estar abierto");
                    activo = true;
                    break;
                }

            if (!activo)
                GestorDeErrores.Emitir($"El expediente debería tener un archivador activo para el mes de {extFechas.Mes(mes)}");

            return expediente;
        }

        //-----------------------------------------------------------------------------------------------------------------------------------
        private static ExpedienteDtm CrearExpedienteDeFCT(ContextoSe contexto)
        {
            var sociedad = TestConTerceros.CrearSociedadConInterlocutorYCalleDto(contexto, enumCalificadorDireccion.correspondencia);
            var solicitante = ((SociedadDtm)sociedad.MapearDtm(contexto)).Interlocutor(contexto);

            if (solicitante.Baja)
            {
                solicitante.Baja = false;
                solicitante = solicitante.Modificar(contexto);
            }

            var cliente = solicitante.Cliente(contexto, crearCliente:true);
            if (cliente.Baja)
            {
                cliente.Baja = false;
                cliente.Modificar(contexto);
            }

            DireccionDtm d = new DireccionDtm();
            var calles = GestorDeCalles.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, 1, new List<ClausulaDeFiltrado>(), parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));
            if (calles.Count > 0)
            {
                var c = calles[0];
                d.IdElemento = solicitante.Id;
                d.IdPais = c.Municipio.Provincia.IdPais;
                d.IdProvincia = c.Municipio.IdProvincia;
                d.IdMunicipio = c.IdMunicipio;
                d.IdCalle = c.Id;
                d.Calificador = enumCalificadorDireccion.contacto;

                GestorDeDirecciones.Gestor(contexto, enumNegocio.Interlocutor).PersistirRegistro(d, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            }

            var expediente = TestConObras.CrearExpediente(contexto,
                codigCg: contexto.Set<CentroGestorDtm>().First(x => true).Codigo,
                nombreTipo: InzExpedientesDeFacturacion.n_exp_tipo_expediente_facturas,
                enumClaseDeExpediente.DeCliente,
                solicitante.NIF(contexto),
                "mi primer expediente de facturación");

            return expediente;
        }
    }
}
