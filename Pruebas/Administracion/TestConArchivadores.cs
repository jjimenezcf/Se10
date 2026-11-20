using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.TrabajosSometidos;
using Inicializador.SistemaDocumental;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.Acromur;
using Utilidades;
using ValidacionesBase;

namespace ValidacionesDeRn
{
    class TestConArchivadores
    {
        //----------------------------------------------------------------------------------------------------------------------------------
        [Test]
        public void SimularAcromur()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                //contexto.IniciarTraza("Entorno de Acromur");
                //InzArchivadores.DefinirModeloAcromur(contexto);
                TrabajosDeEntorno.SometerGenerarSeguridad(contexto).EjecutarTrabajo(contexto);

                var archivadorFiscal = CrearArchivadorFiscal(contexto, InzAcromur.n_usuario_fiscal);
                var archivadorContable = CrearArchivadorContable(contexto, InzAcromur.n_usuario_contable);
                contexto.AsignarUsuario(InzAcromur.n_usuario_fiscal);
                var archivoFiscal = archivadorFiscal.AnexarNuevoFichero(contexto, "fiscal.txt");

                contexto.AsignarUsuario(InzAcromur.n_usuario_contable);
                var archivoContable = archivadorContable.AnexarNuevoFichero(contexto, "contable.txt");
                //bool error = false;
                try
                {
                    //Directory.Delete(@"c:\ArchivosSincrinizados\", true);
                    // Sincronizamos el archivador contable con una carpeta de windows
                    archivadorContable.SincronizarCon = @"c:\ArchivosSincrinizados\contable";
                    ActualizarDirectorioDeSincronizaciónConError(contexto, archivadorContable);
                    archivadorContable = ActualizarDirectorioDeSincronización(contexto, archivadorContable);

                    contexto.AsignarUsuario(ContextoSe.Login_Admin);
                    GestorDeArchivadores.SincronizarArchivador(contexto, archivadorContable);
                    if (!File.Exists($@"{archivadorContable.SincronizarCon}\{archivoContable.Nombre}"))
                        throw new Exception($"El archivo {archivoContable.Nombre} debería existir en la ruta {archivadorContable.SincronizarCon}");

                    // Cambiamos nombre al archivo y vemos que en directorio de windows lo ha hecho
                    contexto.AsignarUsuario(InzAcromur.n_usuario_contable);
                    archivoContable.Nombre = $"contable_{DateTime.Now.Ticks}.txt";
                    archivoContable = archivoContable.Persistir(contexto);
                    if (!File.Exists($@"{archivadorContable.SincronizarCon}\{archivoContable.Nombre}"))
                        throw new Exception($"El archivo {archivoContable.Nombre} debería existir en la ruta {archivadorContable.SincronizarCon}");

                    // intentamos que el usuario fiscal cambie el nombre, pero no puede
                    RenombrarSinPermiso(contexto, archivoContable, InzAcromur.n_usuario_fiscal);

                    // le damos permisos al archivador contable y volemos a intentarlo, ahora si puede
                    OtorgarPermisoDeElementoAlPtFiscal(contexto, archivadorContable);
                    var nombreDeRenovar = $"fiscal_renombra_{DateTime.Now.Ticks}.txt";
                    archivoContable = RenombrarConPermiso(contexto, archivoContable, InzAcromur.n_usuario_fiscal, nombreDeRenovar);
                    if (!File.Exists($@"{archivadorContable.SincronizarCon}\{archivoContable.Nombre}"))
                        throw new Exception($"El archivo {archivoContable.Nombre} debería existir en la ruta {archivadorContable.SincronizarCon}");

                    // Añadimos un archivo, con un nombre que ya existe y debe dar error
                    AnexarUnArchivoQueYaExisteDebeDarError(contexto, archivadorContable, nombreDeRenovar);

                    //al puesto de trabajo contable, le damos permisos directos de gestion sobre el tipo fiscal y intentamos usar el archivador fiscal, debe fallar
                    OtorgarPermisoDeTipoFiscalAlPtContable(contexto, enumModoDeAccesoDeDatos.Gestor);
                    RenombrarSinPermiso(contexto, archivoFiscal, InzAcromur.n_usuario_contable);

                    //al puesto de trabajo contable, le damos primero  permisos directos de consultor, provocamos fallo, luego de gestor y debe funcionar
                    OtorgarPermisoDelCgFiscalAlPtContable(contexto, enumModoDeAccesoDeDatos.Consultor);
                    RenombrarSinPermiso(contexto, archivoFiscal, InzAcromur.n_usuario_contable);

                    OtorgarPermisoDelCgFiscalAlPtContable(contexto, enumModoDeAccesoDeDatos.Gestor);
                    var renombroOtraVez = $"contable_renombra_{DateTime.Now.Ticks}.txt";
                    archivoFiscal = RenombrarConPermiso(contexto, archivoFiscal, InzAcromur.n_usuario_contable, renombroOtraVez);

                    //al puesto de trabajo contable, le damos permisos directos de consulto sobre el tipo fiscal y intentamos usar el archivador fiscal, debe fallar
                    OtorgarPermisoDeTipoFiscalAlPtContable(contexto, enumModoDeAccesoDeDatos.Consultor);
                    RenombrarSinPermiso(contexto, archivoFiscal, InzAcromur.n_usuario_contable);


                    // Sincronizamos el archivador fiscal con una carpeta de windows
                    OtorgarPermisoAlPtFiscal(contexto, InzAcromur.n_cg_acm_codigo_fiscal, InzAcromur.n_pt_responsable, archivadorFiscal, enumModoDeAccesoDeDatos.Administrador);
                    contexto.AsignarUsuario(InzAcromur.n_usuario_fiscal);
                    archivadorFiscal.SincronizarCon = @"c:\ArchivosSincrinizados\fiscal";
                    archivadorFiscal = archivadorFiscal.Persistir(contexto);

                    contexto.AsignarUsuario(ContextoSe.Login_Admin);
                    GestorDeArchivadores.SincronizarArchivador(contexto, archivadorFiscal);
                    if (!File.Exists($@"{archivadorFiscal.SincronizarCon}\{archivoFiscal.Nombre}"))
                        throw new Exception($"El archivo {archivoFiscal.Nombre} debería existir en la ruta {archivadorFiscal.SincronizarCon}");


                    //Cambiamos la carpeta de sincronización del fiscal y volvemos a sincronizar
                    contexto.AsignarUsuario(InzAcromur.n_usuario_fiscal);
                    archivadorFiscal.SincronizarCon = @"c:\ArchivosSincrinizados\fiscal_2";
                    archivadorFiscal = archivadorFiscal.Persistir(contexto);

                    contexto.AsignarUsuario(ContextoSe.Login_Admin);
                    GestorDeArchivadores.SincronizarArchivador(contexto, archivadorFiscal);
                    if (!File.Exists($@"{archivadorFiscal.SincronizarCon}\{archivoFiscal.Nombre}"))
                        throw new Exception($"El archivo {archivoFiscal.Nombre} debería existir en la ruta {archivadorFiscal.SincronizarCon}");

                    //Creo un fichero en el directorio de windows, sincronizo y compruebo que está en el archivador
                    var ficheroDeWindows = $"fichero_creado_en_windows_{DateTime.Now.Ticks}.txt";
                    ApiDeValidaciones.CrearFicheroWindowsParaAnexar(archivadorFiscal.SincronizarCon, ficheroDeWindows);
                    GestorDeArchivadores.SincronizarArchivador(contexto, archivadorFiscal);
                    var archivos = archivadorFiscal.LeerAnexados(contexto);
                    if (archivos.Where(x => x.Nombre.Equals(ficheroDeWindows)).FirstOrDefault() == null)
                        throw new Exception($"El archivo {ficheroDeWindows} debería estar anexado al {archivadorFiscal.Nombre}");

                    //Cambiamos el nombre del fichero y tras sincronizar comprobamos que el que había lo ha quitado de los anexados y ha metido el nuevo
                    var ficheroDeWindowsSustituido = $"fichero_SUSTITUIDO_en_windows_{DateTime.Now.Ticks}.txt";
                    File.Move(Path.Combine(archivadorFiscal.SincronizarCon, ficheroDeWindows), Path.Combine(archivadorFiscal.SincronizarCon, ficheroDeWindowsSustituido));
                    GestorDeArchivadores.SincronizarArchivador(contexto, archivadorFiscal);
                    archivos = archivadorFiscal.LeerAnexados(contexto);
                    if (archivos.Where(x => x.Nombre.Equals(ficheroDeWindowsSustituido)).FirstOrDefault() == null)
                        throw new Exception($"El archivo {ficheroDeWindowsSustituido} debería estar anexado al {archivadorFiscal.Nombre}");
                    if (archivos.Where(x => x.Nombre.Equals(ficheroDeWindows)).Count() == 1)
                        throw new Exception($"El archivo {ficheroDeWindows} NO debería estar anexado al {archivadorFiscal.Nombre}");
                }
                catch
                {
                    //error = true;
                    throw;
                }
                finally
                {
                    //if (!error)
                    //{
                    //    contexto.AsignarUsuario(InzArchivadores.n_usuario_contable);
                    //    var archivos = archivadorContable.LeerAnexados(contexto);
                    //    foreach(var archivo in archivos) archivo.Eliminar(contexto);

                    contexto.AsignarUsuario(InzAcromur.n_usuario_fiscal);
                    var archivos = archivadorFiscal.LeerAnexados(contexto);
                    foreach (var archivo in archivos) archivo.Eliminar(contexto);
                    //}
                }
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        //[Test]
        //public void CrearTipoPorReflexion()
        //{
        //    //var options = new DbContextOptionsBuilder<ContextoSe>().UseSqlServer("Server=DESARROLLO2;Database=SE_2;uid=admin;Password=kadmon;MultipleActiveResultSets=true").Options;
        //    // With the options generated above, we can then just construct a new DbContext class

        //    // var options = SqlServerDbContextOptionsExtensions.UseSqlServer(new DbContextOptionsBuilder(), "Server=DESARROLLO2;Database=SE_2;uid=admin;Password=kadmon;MultipleActiveResultSets=true").Options;
        //    var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin); void prueba()
        //    {
        //        ApiDeEnsamblados.EjecutarMetodoEstatico(ApiDeEnsamblados.DllDelGestorDeNegocio, "GestoresDeNegocio.SistemaDocumental.GestorDeTiposDeArchivadores", "PersistirTipo"
        //            , new object[] {
        //        contexto,
        //        "CTR Archivador",
        //        enumClaseDeLibro.POR_CG_TIPO,
        //        "D-CTR",
        //        true,
        //        0});
        //    }
        //    ApiDeValidaciones.EjecutarPrueba(contexto, prueba);
        //}

        private static void ActualizarDirectorioDeSincronizaciónConError(ContextoSe contexto, ArchivadorDtm archivador)
        {
            try
            {
                archivador = archivador.Persistir(contexto);
            }
            catch
            {
                contexto.AnotarTraza($"{nameof(ActualizarDirectorioDeSincronizaciónConError)} OK", $"No ha podido indicar el directorio de sincronización del archivador {archivador} por no ser Adm");
                return;
            }
            throw new Exception($"Debería dar error al indicar la sincronización, ya que no hay permisos de adm");
        }
        private static ArchivadorDtm ActualizarDirectorioDeSincronización(ContextoSe contexto, ArchivadorDtm archivador)
        {
            contexto.AsignarUsuario(ContextoSe.Login_Admin);
            archivador = archivador.Persistir(contexto);
            return archivador;
        }

        private static void OtorgarPermisoDelCgFiscalAlPtContable(ContextoSe contexto, enumModoDeAccesoDeDatos modo)
        {
            contexto.AsignarUsuario(ContextoSe.Login_Admin);
            var cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo_fiscal);
            var filtroPorAk = new Dictionary<string, object> { { nameof(NegociosDeUnCgDtm.IdCg), cg.Id.ToString() }, { nameof(NegociosDeUnCgDtm.IdNegocio), NegociosDeSe.IdNegocio(enumNegocio.Archivador).ToString() } };
            var idpermiso = modo == enumModoDeAccesoDeDatos.Gestor ? contexto.SeleccionarPorAk<NegociosDeUnCgDtm>(filtroPorAk).IdGestor : contexto.SeleccionarPorAk<NegociosDeUnCgDtm>(filtroPorAk).IdConsultor;

            cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo_contable);
            filtroPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), InzAcromur.n_pt_responsable } };
            var puesto = contexto.SeleccionarPorAk<PuestoDtm>(filtroPorAk, false);

            var p = new PermisosDirectosDtm();
            p.IdPermiso = idpermiso;
            p.IdPuesto = puesto.Id;
            p.Insertar(contexto);
            GestorDeCentrosGestores.GenerarSeguridad(contexto);
        }

        private static void OtorgarPermisoAlPtFiscal(ContextoSe contexto, string codigoCg, string nombrePt, ArchivadorDtm archivador, enumModoDeAccesoDeDatos modo)
        {
            contexto.AsignarUsuario(ContextoSe.Login_Admin);

            var cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), codigoCg);
            var filtroPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), nombrePt } };
            var puesto = contexto.SeleccionarPorAk<PuestoDtm>(filtroPorAk, false);


            var permisos = GestorDePemisosDelElemento.CrearPermisos(contexto, enumNegocio.Archivador, new List<int> { archivador.Id });
            permisos = GestorDePemisosDelElemento.CrearPermisos(contexto, enumNegocio.Archivador, new List<int> { archivador.Id });

            var p = new PermisosDirectosDtm();
            p.IdPermiso = modo == enumModoDeAccesoDeDatos.Administrador
                ? permisos[0].IdAdministrador
                : modo == enumModoDeAccesoDeDatos.Gestor
                ? permisos[0].IdGestor
                : permisos[0].IdConsultor;
            p.IdPuesto = puesto.Id;
            p.Insertar(contexto);
            GestorDePemisosDelElemento.GenerarSeguridad(contexto);
        }


        private static void OtorgarPermisoDeTipoFiscalAlPtContable(ContextoSe contexto, enumModoDeAccesoDeDatos modo)
        {
            contexto.AsignarUsuario(ContextoSe.Login_Admin);
            var idPermiso = modo == enumModoDeAccesoDeDatos.Gestor ?
                contexto.SeleccionarPorPropiedad<TipoDeArchivadorDtm>(nameof(TipoDeArchivadorDtm.Nombre), InzArchivadoresEcoFin.n_tipo_fiscal).IdPermisoDeGestor :
                contexto.SeleccionarPorPropiedad<TipoDeArchivadorDtm>(nameof(TipoDeArchivadorDtm.Nombre), InzArchivadoresEcoFin.n_tipo_fiscal).IdPermisoDeConsultor;

            var cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo_contable);
            var filtroPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), InzAcromur.n_pt_responsable } };
            var puesto = contexto.SeleccionarPorAk<PuestoDtm>(filtroPorAk, false);

            var p = new PermisosDirectosDtm();
            p.IdPermiso = idPermiso;
            p.IdPuesto = puesto.Id;
            p.Insertar(contexto);

            if (modo == enumModoDeAccesoDeDatos.Consultor)
            {
                filtroPorAk = new Dictionary<string, object> { { nameof(PermisosDirectosDtm.IdPuesto), puesto.Id.ToString() }
                    , { nameof(PermisosDirectosDtm.IdPermiso), contexto.SeleccionarPorPropiedad<TipoDeArchivadorDtm>(nameof(TipoDeArchivadorDtm.Nombre), InzArchivadoresEcoFin.n_tipo_fiscal).IdPermisoDeGestor.ToString() } };
                var permisos = contexto.SeleccionarPorAk<PermisosDirectosDtm>(filtroPorAk, false);
                if (permisos != null)
                    permisos.Eliminar(contexto);
            }

            GestorDeTiposDeArchivadores.GenerarSeguridad(contexto);
        }

        private static void OtorgarPermisoDeElementoAlPtFiscal(ContextoSe contexto, ArchivadorDtm archivadorContable)
        {
            contexto.AsignarUsuario(ContextoSe.Login_Admin);
            GestorDePemisosDelElemento.CrearPermisos(contexto, enumNegocio.Archivador, new List<int> { archivadorContable.Id });

            var cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo_fiscal);
            var filtroPorAk = new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id.ToString() }, { nameof(PuestoDtm.Nombre), InzAcromur.n_pt_responsable } };
            var puesto = contexto.SeleccionarPorAk<PuestoDtm>(filtroPorAk, false);

            GestorDePermisosDirectos.OtorgarPermisos(contexto, enumNegocio.Archivador, archivadorContable.Id, puesto.Id, enumModoDeAccesoDeDatos.Gestor);
            GestorDePemisosDelElemento.GenerarSeguridad(contexto);
        }


        private static ArchivoDtm RenombrarConPermiso(ContextoSe contexto, ArchivoDtm archivo, string login, string nombreArchivo)
        {
            contexto.AsignarUsuario(login);
            archivo.Nombre = nombreArchivo;
            archivo = archivo.Persistir(contexto);
            return archivo;
        }

        private static void AnexarUnArchivoQueYaExisteDebeDarError(ContextoSe contexto, ArchivadorDtm archivadorContable, string nombre)
        {
            ArchivoDtm archivo;
            contexto.AsignarUsuario(InzAcromur.n_usuario_contable);
            archivo = archivadorContable.AnexarNuevoFichero(contexto, $"nuevo_fichero_{DateTime.Now.Ticks}.txt");
            archivo.Nombre = nombre;
            //no debe de dejat por existir
            try
            {
                archivo = archivo.Persistir(contexto);
            }
            catch
            {
                contexto.AnotarTraza($"{nameof(AnexarUnArchivoQueYaExisteDebeDarError)} OK", $"No ha podido renombrar como {archivo.Nombre} por estar duplicado");
                return;
            }

            throw new Exception("No debería haber dejado añadir el fichero, tenía que decir que estaba duplicado");
        }

        private static void RenombrarSinPermiso(ContextoSe contexto, ArchivoDtm archivo, string login)
        {
            contexto.AsignarUsuario(login);
            var nombre = archivo.Nombre;
            archivo.Nombre = "debe_de_fallar.txt";
            //no debe de dejat por no tener permiso
            try
            {
                archivo = archivo.Persistir(contexto);
            }
            catch
            {
                contexto.AnotarTraza($"{nameof(RenombrarSinPermiso)} OK", $"No ha podido renombrar como {archivo.Nombre} por no tener permisos");
                archivo.Nombre = nombre;
                return;
            }

            throw new Exception("No debería haber dejado renombrar el fichero, tenía que decir que no había permisos");
        }

        private ArchivadorDtm CrearArchivadorFiscal(ContextoSe contexto, string login)
        {
            Inicializaciones.AsignarUsuario(contexto, login);
            var archivador = new ArchivadorDtm();
            archivador.Nombre = "Mi primer archivador";
            archivador.IdCg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo_fiscal).Id;
            archivador.IdTipo = contexto.SeleccionarPorPropiedad<TipoDeArchivadorDtm>(nameof(TipoDeArchivadorDtm.Nombre), InzArchivadoresEcoFin.n_tipo_fiscal).Id;
            return archivador.Insertar(contexto);
        }

        private ArchivadorDtm CrearArchivadorContable(ContextoSe contexto, string login)
        {
            Inicializaciones.AsignarUsuario(contexto, login);
            var archivador = new ArchivadorDtm();
            archivador.Nombre = "Mi primer archivador";
            archivador.IdCg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), InzAcromur.n_cg_acm_codigo_contable).Id;
            archivador.IdTipo = contexto.SeleccionarPorPropiedad<TipoDeArchivadorDtm>(nameof(TipoDeArchivadorDtm.Nombre), InzArchivadoresEcoFin.n_tipo_contable).Id;
            return archivador.Insertar(contexto);
        }
    }
}
