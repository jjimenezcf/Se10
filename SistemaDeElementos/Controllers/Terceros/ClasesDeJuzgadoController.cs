using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Terceros;
using System.Linq;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class ClasesDeJuzgadoController : EntidadController<ContextoSe, ClaseDeJuzgadoDtm, ClaseDeJuzgadoDto>
    {
        public ClasesDeJuzgadoController(GestorDeClasesDeJuzgado gestor, GestorDeErrores errores)
        : base 
        (
         gestor,
         errores
        )
        {
            if (ExtensorDeExpedientes.HayTiposJuridicos(Contexto))
                return;
            if (!ExtensorDePleitos.ModuloActivo(Contexto))
                Emitir(ltrDePleitos.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }

        public IActionResult CrudClasesDeJuzgado()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeClasesDeJuzgado(Contexto, ModoDescriptor.Mantenimiento));
        }
    }
}
