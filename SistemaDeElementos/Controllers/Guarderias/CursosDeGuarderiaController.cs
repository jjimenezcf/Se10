using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Guarderias;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Guarderias;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Guarderias;
using System;
using Utilidades;
using static Gestor.Errores.GestorDeErrores;

namespace MVCSistemaDeElementos.Controllers
{
    public class CursosDeGuarderiaController : EntidadController<ContextoSe, CursoDeGuarderiaDtm, CursoDeGuarderiaDto>
    {
        public CursosDeGuarderiaController(GestorDeCursosDeGuarderia gestorDeCursosDeGuarderia, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeCursosDeGuarderia,
           gestorDeErrores
         )
        {
            if (!ExtensorDeGuarderias.ModuloActivo(Contexto))
                Emitir(ltrDeGuarderias.ModuloNoActivo, enumCodigoDeError.ModuloNoActivo);
        }

        public IActionResult CrudCursosDeGuarderia()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDeCursosDeGuarderia).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Guarderias}/{nameof(CrudCursosDeGuarderia)}";
                    return base.View(destino, new DescriptorDeCursosDeGuarderia(Contexto, (string)cache[indice]));
                }
                else
                {
                    enumNegocio.CursoDeGuarderia.DefinirParametro(Contexto, enumParametrosDeGuarderia.CURSO_CG_PARA_PUESTOS_DE_TRABAJO, valor: Literal.Cero);
                    var descriptor = DescriptorDeCrud<CursoDeGuarderiaDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDeCursosDeGuarderia(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }


    }
}
