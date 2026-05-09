using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using GestorDeElementos;
using GestoresDeNegocio.Terceros;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Controllers;
using Newtonsoft.Json;
using NUnit.Framework;
using ServicioDeDatos;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Terceros;
using SistemaDeElementos.Inicializador.AytoBeniel;
using ValidacionesBase;

namespace ValidacionesHttp
{
    public class TercerosHttp
    {

        ContextoSe _contexto;
        [SetUp]
        public void Setup()
        {
        _contexto = Inicializaciones.CrearContexto();
        }


        [Test]
        public void CrearSociedad()
        {
            var t = _contexto.IniciarTransaccion();
            try
            {
                var sociedadDto = new SociedadDto();
                sociedadDto.Nombre = "28000 PT MADRID, S.L.";
                sociedadDto.eMail = "jj";
                sociedadDto.Telefono = "prueba";
                sociedadDto.Nif = "B80967979";
                var elemento = JsonConvert.SerializeObject(sociedadDto);
                var gestor = GestorDeSociedades.Gestor(_contexto, _contexto.Mapeador);
                var controlador = new SociedadesController(gestor, new Gestor.Errores.GestorDeErrores());
                var r = ApiController.PersistirElemento(gestor, elemento, Inicializaciones._httpContext, (sociedadDto) => new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                if (((Resultado)r.Value).Estado == enumEstadoPeticion.Error)
                    throw new Exception($"{((Resultado)r.Value).Mensaje}{ Environment.NewLine}{((Resultado)r.Value).Consola}");
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
        public void CrearContacto()
        {
            var t = _contexto.IniciarTransaccion();
            try
            {
                var sociedad = _contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), InzMaestrosBeniel.n_nif_beniel);

                var contactoDto = new ContactoDto();
                contactoDto.Nombre = "Miguel Ángel";
                contactoDto.eMail = "ma@beniel.com";
                contactoDto.Telefono = "619702547";
                contactoDto.Descripcion = "Alcalde";
                contactoDto.IdElemento = sociedad.Id;
                
                var elemento = JsonConvert.SerializeObject(contactoDto);
                var gestor = GestorDeContactos.Gestor(_contexto, _contexto.Mapeador);
                var controlador = new ContactosController(_contexto,_contexto.Mapeador,new Gestor.Errores.GestorDeErrores());
               
                var r = ApiController.PersistirElemento(gestor, elemento, Inicializaciones._httpContext, (contactoDto) => new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                if (((Resultado)r.Value).Estado == enumEstadoPeticion.Error) 
                    throw new Exception($"{((Resultado)r.Value).Mensaje}{Environment.NewLine}{((Resultado)r.Value).Consola}");
                _contexto.Rollback(t);

                Assert.IsTrue(true);
            }
            catch (Exception e)
            {
                Assert.Fail(e.Message);
                _contexto.Rollback(t);
            }
        }

        //[Test]
        //public void Test()
        //{
        //    var t = _contexto.IniciarTransaccion();
        //    try
        //    {
        //        RealizarPeticion(@$"https://localhost:51000/expedientes/epNegociosParaAdjuntarDocumentacion");
        //    }
        //    catch (Exception e)
        //    {
        //        Assert.Fail(e.Message);
        //        _contexto.Rollback(t);
        //    }
        //}

        private static string RealizarPeticion(string uri)
        {
            HttpClient client = new HttpClient();
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github.v3+json"));
            client.DefaultRequestHeaders.Add("User-Agent", ".NET Foundation Repository Reporter");

            var stringTask = (Task<string>)client.GetStringAsync(uri);

            var resultado = stringTask.Result;
            return resultado;
        }
    }
}