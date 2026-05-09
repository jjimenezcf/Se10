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
using System.Linq.Dynamic.Core;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class JuzgadosController : EntidadController<ContextoSe, JuzgadoDtm, JuzgadoDto>
    {
        public JuzgadosController(GestorDeJuzgados gestorDeJuzgados, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeJuzgados,
           gestorDeErrores
         )
        {
            if (ExtensorDeExpedientes.HayTiposJuridicos(Contexto)) 
                return;

            if (!ExtensorDePleitos.ModuloActivo(Contexto))
                Emitir(ltrDePleitos.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }

        public IActionResult CrudJuzgados()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeJuzgados(Contexto, ModoDescriptor.Mantenimiento));
        }

    }
}
