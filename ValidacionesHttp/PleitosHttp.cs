using MVCSistemaDeElementos.Controllers;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using System;
using ValidacionesBase;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto;

namespace ValidacionesHttp
{
    internal class PleitosHttp
    {


        ContextoSe _contexto;
        [SetUp]
        public void Setup()
        {
            _contexto = Inicializaciones.CrearContexto();
        }

        [Test]
        public void CrearBorrarPlantillaPorTipoAlPleito_1()
        {
            var t = _contexto.IniciarTransaccion();
            try
            {
                _contexto.AsignarUsuario(_contexto.SeleccionarPorPropiedad<UsuarioDtm>(nameof(UsuarioDtm.Login), "admin.se "));
                var controlador = new TiposDeElementoController(_contexto, _contexto.Mapeador, new Gestor.Errores.GestorDeErrores());
                var r = controlador.epCrearPlantilla(37, "{\"id\":\"0\",\"idAccion\":2,\"idnegocio\":37,\"idtipo\":9,\"nombre\":\"rrr\"}");
                if (r.Value.GetType().GetProperty(nameof(Resultado.Estado)).GetValue(r.Value, null).ToString() == enumEstadoPeticion.Error.ToString())
                    throw
                        new Exception($"Error: {r.Value.GetType().GetProperty(nameof(Resultado.Mensaje)).GetValue(r.Value, null)}{Environment.NewLine}{r.Value.GetType().GetProperty(nameof(Resultado.Consola)).GetValue(r.Value, null)}");
                
                var datos = r.Value.GetType().GetProperty(nameof(Resultado.Datos)).GetValue(r.Value, null);
                r = controlador.epBorrarPlantilla(((PlantillaPorTipoDto)datos).Id, "[{\"parametro\":\"idnegocio\",\"valor\":37}]");
                if (r.Value.GetType().GetProperty(nameof(Resultado.Estado)).GetValue(r.Value, null).ToString() == enumEstadoPeticion.Error.ToString())
                    throw
                        new Exception($"Error: {r.Value.GetType().GetProperty(nameof(Resultado.Mensaje)).GetValue(r.Value, null)}{Environment.NewLine}{r.Value.GetType().GetProperty(nameof(Resultado.Consola)).GetValue(r.Value, null)}");


                _contexto.Rollback(t);
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
                _contexto.Rollback(t);
            }
        }

        [Test]
        public void LeerPlantillaPorTipo()
        {
            var t = _contexto.IniciarTransaccion();
            try
            {
                _contexto.AsignarUsuario(_contexto.SeleccionarPorPropiedad<UsuarioDtm>(nameof(UsuarioDtm.Login), "admin.se "));
                var controlador = new TiposDeElementoController(_contexto, _contexto.Mapeador, new Gestor.Errores.GestorDeErrores());
                var r = controlador.epProcesarOpcionMf(37, eventosDeMf.Comun_Imprimir, esContextual: false, parametrosJson: "[{\"parametro\":\"ids\",\"valor\":[1]},{\"parametro\":\"Vista\",\"valor\":\"Gestión de pleitos\"}]");
               
                if (r.Value.GetType().GetProperty(nameof(Resultado.Estado)).GetValue(r.Value, null).ToString() == enumEstadoPeticion.Error.ToString())
                    throw
                        new Exception($"Error: {r.Value.GetType().GetProperty(nameof(Resultado.Mensaje)).GetValue(r.Value, null)}{Environment.NewLine}{r.Value.GetType().GetProperty(nameof(Resultado.Consola)).GetValue(r.Value, null)}");
                _contexto.Rollback(t);
                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
                _contexto.Rollback(t);
            }
        }
    }
}
