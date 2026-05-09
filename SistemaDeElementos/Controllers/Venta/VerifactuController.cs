using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Ventas;
using ServicioDeDatos.Ventas;
using ModeloDeDto.Ventas;

namespace MVCSistemaDeElementos.Controllers
{
    public class VerifactuController : EntidadController<ContextoSe, VerifactuDtm, VerifactuDto>
    {
        public VerifactuController(GestorDeVerifactu gestorDeVerifactu, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDeVerifactu,
           gestorDeErrores
         )
        {
        }

    }
}
