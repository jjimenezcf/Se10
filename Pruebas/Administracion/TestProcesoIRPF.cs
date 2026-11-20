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
using GestoresDeNegocio.SistemaDocumental;

namespace ValidacionesDeRn
{
    class TestProcesoIRPF
    {

        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirProcesoIRPF()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzExpedientesDeIrpf.ProcesoIrpf(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarProcesoIRPF()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzExpedientesDeIrpf.ProcesoIrpf(contexto);
                var expediente = CrearExpedienteDeIRPF(contexto);
                var archivadores = expediente.Vinculados<ArchivadorDtm>(contexto).Where(x => x.Nombre.Contains(AccionesDeIrpf.PrefijoArchivadorIrpf(expediente)));
                if (archivadores.Count() != 1)
                    GestorDeErrores.Emitir("Deberían haber un archivadores de Irpf");


                //Cancelar expediente
                var archivador = archivadores.First();
                expediente = expediente.Cancelar(contexto);
                //intentar aportar documentación
                ApiDeValidaciones.IntentarEjecutar(() => archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF.txt")), "no es modificable");
                ApiDeValidaciones.IntentarEjecutar(() => expediente.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF.txt")), "no es modificable");

                //abrir otro expediente, aportar documentación
                expediente = CrearExpedienteDeIRPF(contexto);
                archivadores = expediente.Vinculados<ArchivadorDtm>(contexto).Where(x => x.Nombre.Contains(AccionesDeIrpf.PrefijoArchivadorIrpf(expediente)));
                archivador = archivadores.First();
                archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF_2.txt"));

                //cerrar archivador e intentar aportar documentación
                archivador.Baja = true;
                archivador = archivador.Modificar(contexto);
                ApiDeValidaciones.IntentarEjecutar(() => archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF_3.txt")), "no es modificable");

                //abrir el archivador aportar documentación
                archivador.Baja = false;
                archivador = archivador.Modificar(contexto);
                archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF_3.txt"));

                //transitar a en elaboración, luego a parado, aportr documentación y debolver a elaborar
                expediente = expediente.Transitar(contexto, InzExpedientesDeIrpf.n_tran_irpf_comenzar);
                expediente = expediente.Transitar(contexto, InzExpedientesDeIrpf.n_tran_irpf_parar, parametros: new Dictionary<string, object> {
                    {ltrParametrosEp.detalleAsunto, "falta info" }
                });
                archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF_4.txt"));
                expediente = expediente.Transitar(contexto, InzExpedientesDeIrpf.n_tran_irpf_continuar);

                //aportar renta y terminar
                expediente.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("RENTA.txt"));
                expediente = expediente.Transitar(contexto, InzExpedientesDeIrpf.n_tran_irpf_presentar);

                //intentar aportar documentación
                ApiDeValidaciones.IntentarEjecutar(() => archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF_5.txt")), "no es modificable");
                ApiDeValidaciones.IntentarEjecutar(() => expediente.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF_6.txt")), "no es modificable");

                //reabrir para corregir, aportar documentación y terminar
                expediente = expediente.Transitar(contexto, InzExpedientesDeIrpf.n_tran_irpf_corregir, parametros: new Dictionary<string, object> {
                    {ltrParametrosEp.detalleAsunto, "nuevos datos" }
                });
                archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IRPF_7.txt"));
                expediente.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("RENTA_1.txt"));
                expediente = expediente.Transitar(contexto, InzExpedientesDeIrpf.n_tran_irpf_presentar);

            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }



        //-----------------------------------------------------------------------------------------------------------------------------------
        private static ExpedienteDtm CrearExpedienteDeIRPF(ContextoSe contexto)
        {
            var sociedad = TestConTerceros.CrearSociedadConInterlocutorYCalleDto(contexto, enumCalificadorDireccion.correspondencia);
            var solicitante = ((SociedadDtm)sociedad.MapearDtm(contexto)).Interlocutor(contexto);

            if (solicitante.Baja)
            {
                solicitante.Baja = false;
                solicitante = solicitante.Modificar(contexto);
            }

            var cliente = solicitante.Cliente(contexto, crearCliente: true);
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
                nombreTipo: InzExpedientesDeIrpf.n_exp_tipo_expediente_irpf,
                enumClaseDeExpediente.DeCliente,
                solicitante.NIF(contexto),
                "mi primer expediente de IRPF");

            return expediente;
        }
    }
}
