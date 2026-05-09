using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.Guarderias;
using ModeloDeDto.Guarderias;
using GestoresDeNegocio.Guarderias;
using GestorDeElementos.Extensores;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class AulasDeGuarderiaController : EntidadController<ContextoSe, AulaDeGuarderiaDtm, AulaDeGuarderiaDto>
    {

        public AulasDeGuarderiaController(GestorDeAulasDeGuarderia gestorDeAulaDeGuarderia, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeAulaDeGuarderia,
           gestorDeErrores
         )
        {
            if (!ExtensorDeGuarderias.ModuloActivo(Contexto))
                Emitir(ltrDeGuarderias.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }


        public IActionResult CrudAulasDeGuarderia()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);
            return ViewCrud(new DescriptorDeAulasDeGuarderia(Contexto, ModoDescriptor.Mantenimiento));
        }


    }
}