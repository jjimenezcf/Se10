using GestorDeElementos;
using GestoresDeNegocio.Callejero;
using Inicializador.Expedientes;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Terceros;
using GestorDeElementos.Extensores;
using System.Collections.Generic;
using Utilidades;
using ValidacionesBase;
using ServicioDeDatos.SistemaDocumental;
using GestoresDeNegocio.Expediente;
using System.Linq;
using Gestor.Errores;
using GestoresDeNegocio.SistemaDocumental;
using SistemaDeElementos.Inicializador.Acromur;

namespace ValidacionesDeRn
{
    class TestProcesoPIM
    {     
        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void DefinirProcesoPIM()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {

                var sociedadDelSistema = InzAcromur.Sociedad(contexto);
                var tipoArchivador = GestorDeTiposDeArchivadores.PersistirTipo(contexto, ltrTipoArchivador.TipoClientes, enumClaseDeLibro.POR_CG_TIPO, "CLI", visible: true, delSistema: false, nombreModificable: false);
                var cgCliente = ExtensionCentrosGestores.Cfg_CG_De_ClienteWeb(contexto, sociedadDelSistema.Id);

                var parametroTipoArchivador = enumNegocio.Cliente.ResetearParametro(contexto, enumParametrosDeCliente.CLI_TipoArchivador, valor: tipoArchivador.Nombre);
                var parametroCgCliente = enumNegocio.Cliente.ResetearParametro(contexto, enumParametrosDeCliente.CLI_CG_De_Cliente, valor: cgCliente.Codigo);

                InzProcesoPIM.ProcesoPIM(contexto);
            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }


        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void ProbarProcesoPIM()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                InzProcesoPIM.ProcesoPIM(contexto);
                var expediente = CrearExpedienteDePIM(contexto);
                var archivadores = expediente.Vinculados<ArchivadorDtm>(contexto).Where(x => x.Nombre.Contains(AccionesDePIM.PrefijoArchivadorPIM(expediente)));
                if (archivadores.Count() != 2)
                    GestorDeErrores.Emitir("Deberían haber dos archivador PIM");


                //Cancelar expediente
                var archivador = archivadores.First();
                expediente = expediente.Cancelar(contexto);
                //intentar aportar documentación
                ApiDeValidaciones.IntentarEjecutar(() => archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("PIM.txt")), "no es modificable");
                ApiDeValidaciones.IntentarEjecutar(() => expediente.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("PIM.txt")), "no es modificable");

                //abrir otro expediente, aportar documentación
                expediente = CrearExpedienteDePIM(contexto);
                archivadores = expediente.Vinculados<ArchivadorDtm>(contexto).Where(x => x.Nombre.Contains(AccionesDePIM.PrefijoArchivadorPIM(expediente)));
                archivador = archivadores.First();
                archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("PIM_2.txt"));

                //cerrar archivador e intentar aportar documentación
                archivador.Baja = true;
                archivador = archivador.Modificar(contexto);
                ApiDeValidaciones.IntentarEjecutar(() => archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("PIM_3.txt")), "no es modificable");

                //abrir el archivador aportar documentación
                archivador.Baja = false;
                archivador = archivador.Modificar(contexto);
                archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("PIM_3.txt"));

                //transitar a cerrado, devolver a abierto
                expediente = expediente.Transitar(contexto, InzProcesoPIM.n_tran_pim_cerrar);
                expediente = expediente.Transitar(contexto, InzProcesoPIM.n_tran_pim_reabrir, parametros: new Dictionary<string, object> {
                    {ltrParametrosEp.detalleAsunto, "falta info" }
                });
                archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("PIM_4.txt"));

                //aportar renta y terminar
                expediente.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("IMPUESTO.txt"));
                expediente = expediente.Transitar(contexto, InzProcesoPIM.n_tran_pim_cerrar);

                //intentar aportar documentación
                ApiDeValidaciones.IntentarEjecutar(() => archivador.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("PIM_5.txt")), "no es modificable");
                ApiDeValidaciones.IntentarEjecutar(() => expediente.AnexarArchivo(contexto, ServidorDocumental.NuevoArchivo("PIM_6.txt")), "no es modificable");

            }
            //ApiDeValidaciones.EjecutarConCommit(contexto, prueba);
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }



        //-----------------------------------------------------------------------------------------------------------------------------------
        private static ExpedienteDtm CrearExpedienteDePIM(ContextoSe contexto)
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
                nombreTipo: InzProcesoPIM.n_exp_tipo_pim,
                enumClaseDeExpediente.DeCliente,
                solicitante.NIF(contexto),
                "mi primer expediente de PIM"); 

            return expediente;
        }
    }
}
