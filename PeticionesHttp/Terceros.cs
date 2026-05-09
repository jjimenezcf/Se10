using NUnit.Framework;
using ServicioDeDatos;
using ValidacionesContexto;

namespace PeticionesHttp
{
    public class Terceros
    {
        ContextoSe _contexto;
        [SetUp]
        public void Setup()
        {
            _contexto =  Inicializaciones.CrearContexto("raul.miras");
        }

        [Test]
        public void Test1()
        {
            Assert.Pass();
        }
    }
}