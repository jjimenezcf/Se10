using AutoMapper;
using GestorDeElementos.Extensores;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MVCSistemaDeElementos.Controllers;
using ServicioDeDatos;
using SistemaDeElementos.Inicializador.Datos;
using System;
using Utilidades;

namespace MVCSistemaDeElementos
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var servidorWeb = CreateHostBuilder(args).Build();
            try
            {
                ServicioDeCaches.UsaCache = false;
                InicializarBD(servidorWeb);
            }
            finally
            {
                ServicioDeCaches.InicializarUsaCache();
            }
            servidorWeb.Run();
        }

        private static void InicializarBD(IHost sevidorWeb)
        {
            var scope = sevidorWeb.Services.CreateScope();

            var contexto = scope.ServiceProvider.GetRequiredService<ContextoSe>();
            contexto.Mapeador = scope.ServiceProvider.GetRequiredService<IMapper>();

            contexto.DatosDeConexion.CreandoModelo = true;
            contexto.AsignarRolDeAdministrador();
            try
            {
                contexto.AplicarMigraciones();

                var crearBd = Literal.VariableNoDefinida == CacheDeVariable.LeerValorDeVariable(Variable.CFG_UrlBase, emitirError: false, usarLaCache: false);
                if (crearBd)
                {
                    var t = contexto.IniciarTransaccion();
                    try
                    {
                        InzEntorno.PersistirDatosIniciales(contexto);
                        contexto.Commit(t);
                    }
                    catch
                    {
                        contexto.Rollback(t);
                        contexto.BorrarBd();
                        throw;
                    }
                }

                ServicioDeCaches.UsaCache = CacheDeVariable.LeerValorDeVariable(Variable.CFG_Usar_Cache, emitirError: false, usarLaCache: false).EsTrue();
                var valor = CacheDeVariable.LeerValorDeVariable(Variable.Cfg_Usar_Cache_Descriptores_Json, emitirError: false, usarLaCache: false);
                if (valor == Literal.VariableNoDefinida)
                {
                    CacheDeVariable.CrearVariable(Variable.Cfg_Usar_Cache_Descriptores_Json, Descripciones.Cfg_Usar_Cache_Descriptores_Json, "S");
                    ServicioDeCaches.UsaCacheParaRenderizar = true;
                }
                ServicioDeCaches.UsaCacheParaRenderizar = valor.EsTrue();

                var usuario = contexto.Administrador();
                contexto.AsignarUsuario(usuario);

                if (CacheDeVariable.Cfg_CrearRegistrosDeEntorno)
                {
                    var tran = contexto.IniciarTransaccion();
                    try
                    {
                        InicializadorController.InicializarRegistroDeEntorno(contexto);
                        if (crearBd)
                        {
                            // InicializadorController.InicializarSeed(contexto);
                        }
                        CacheDeVariable.Modificar(contexto, Variable.CFG_Crear_Registros_De_Entorno, "N");
                        contexto.Commit(tran);
                    }
                    catch (Exception e)
                    {
                        contexto.Rollback(tran);
                        contexto.AnotarExcepcion(e);
                        if (crearBd) contexto.BorrarBd();
                        throw;
                    }
                }
            }
            finally
            {
                contexto.DatosDeConexion.CreandoModelo = false;
                contexto.QuitarRolDeAdministrador();
            }
        }



        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
