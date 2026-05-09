using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Entorno;
using GestoresDeNegocio.Entorno;
using ModeloDeDto.Entorno;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class CertificadosController : EntidadController<ContextoSe, CertificadoDtm, CertificadoDto>
    {

        public CertificadosController(GestorDeCertificados gestorDeCertificados, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCertificados,
           gestorDeErrores
         )
        {
        }


        public IActionResult CrudCertificados()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeCertificados(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
