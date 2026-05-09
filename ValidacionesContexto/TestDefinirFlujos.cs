using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.TrabajosSometidos;
using Inicializador.Procesos;
using iText.Commons.Actions.Contexts;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using Newtonsoft.Json;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Negocio;
using SistemaDeElementos.Inicializador;
using SistemaDeElementos.Inicializador.AytoBeniel;
using System.Collections.Generic;
using System;
using Utilidades;
using System.Linq;
using System.Reflection;

namespace ValidacionesBase
{


    public class TestDefinirFlujos
    {

        [Test]
        public void Test()
        {
            Assert.That(GetAllTypesOf<TestDefinirFlujos>().Any(), Is.True);
        }

        private static IEnumerable<Type> GetAllTypesOf<T>()
        {
            return AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(x => x.DefinedTypes)
                .Where(type => typeof(T).GetTypeInfo().IsAssignableFrom(type.AsType()))
                .Select(type => type.AsType());
        }

        [Test]
        public void CrearFlujo()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                CrearFlujo(contexto);
            }

            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        [Test]
        public void SometerSeguridadPorUsuario()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                TrabajosDeEntorno.SometerGenerarSeguridadParaElUsuario(contexto, 4).EjecutarTrabajo(contexto);
                TrabajosDeEntorno.SometerGenerarSeguridadParaLosUsuario(contexto, new List<int> { 4, 3}).EjecutarTrabajo(contexto);
            }

            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        [Test]
        public void NegociosQuePermitenSubirDocumentacionDesdeElMovilAlUsuarioConectado()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                enumNegocio.Cliente.ResetearParametro(contexto, enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil, "S");
                enumNegocio.Proveedor.ResetearParametro(contexto, enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil, "S");
                enumNegocio.Interlocutor.ResetearParametro(contexto, enumParametrosDeNegocio.CFG_Permite_Subir_Archivos_Desde_El_Movil, "S");

                var negociosPermitidos = NegociosDeSe.NegociosQuePermitenSubirDocumentacionDesdeElMovilAlUsuarioConectado(contexto, true);
                var json = JsonConvert.SerializeObject(negociosPermitidos);

            }

            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }

        public static void CrearFlujo(ContextoSe contexto)
        {
            InzAcciones.DefinirAcciones(contexto);
            InzNegocios.DefinirAccionesDeRelacion(contexto);

            InzSeguridadComun.SeguridadFuncional(contexto);
            InzSeguridadComun.SeguridadDatosComunes(contexto);

            InzMaestrosBeniel.Sociedad(contexto);
            InzRegistroEs.ModeloDeRegistroEs(contexto);

            InzTareasRre.ModeloDeTareasTrr(contexto);

            InzSeguridadBeniel.CrearRolesDeDatos(contexto);
            InzSeguridadBeniel.CrearPuestosDeTrabajo(contexto);
            InzUsuariosBeniel.CrearUsuarios(contexto);

            InzUsuariosBeniel.CrearUsuarios(contexto);
            TrabajosDeEntorno.SometerGenerarSeguridad(contexto).EjecutarTrabajo(contexto);
        }

    }
}
