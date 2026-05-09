using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using ServicioDeDatos.Elemento;
using AutoMapper;

namespace MVCSistemaDeElementos.Controllers
{
    public class AccionesDeTrnController : RelacionController<ContextoSe, AccionesDeTrnDtm, AccionesDeTrnDto>
    {
        public AccionesDeTrnController(GestorDeAccionesDeTrn gestorDeAccionesDeTrn, GestorDeErrores gestorDeErrores, IMapper mapper)
        : base(gestorDeAccionesDeTrn, gestorDeErrores)
        {
            Contexto.Mapeador = mapper;
        }

    }

}
