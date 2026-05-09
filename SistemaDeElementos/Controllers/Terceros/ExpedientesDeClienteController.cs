using ServicioDeDatos;
using Gestor.Errores;
using Microsoft.AspNetCore.Mvc;
using GestorDeElementos;
using System.Collections.Generic;
using ServicioDeDatos.Seguridad;
using Utilidades;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using ModeloDeDto;
using Newtonsoft.Json;
using AutoMapper;
using GestorDeElementos.Extensores;
using System.Linq;
using GestoresDeNegocio.Terceros;

namespace MVCSistemaDeElementos.Controllers
{
    public class ExpedientesDeClienteController : BaseController<ExpedienteDeClienteDto>
    {
        public ExpedientesDeClienteController(GestorDeErrores gestorDeErrores, ContextoSe contexto, IMapper mapeador)
         : base
         (
           gestorDeErrores, contexto, mapeador
         )
        {
        }

        protected override IEnumerable<ExpedienteDeClienteDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            var restrictor = ApiController.ObtenerNegocioYelemento(filtros);
            ApiDeDetalles.ValidarUsaDetalleDe(enumNegocio.Cliente, typeof(UsuarioDeClienteDtm));

            var modoAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Cliente, restrictor.idElemento);
            if (modoAcceso == enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario {Contexto.DatosDeConexion.Login} no tiene acceso al elemento del negocio: {enumNegocio.Cliente.Singular()}");
            
            var cliente = filtros.Where(x => x.Clausula == (nameof(IDetalleDto.IdElemento))).FirstOrDefault();
            if (cliente == null)
                GestorDeErrores.Emitir($"No ha indicado el cliente del que se quieren los expedientes");

            return GestorDeClientes.LeerExpedientes(contexto: Contexto, idCliente: cliente.Valor.Entero());
        }

    }
}
