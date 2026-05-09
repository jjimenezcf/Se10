using Microsoft.AspNetCore.Mvc;
using ServicioDeDatos;
using Gestor.Errores;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using GestoresDeNegocio.TrabajosSometidos;
using System;
using GestorDeElementos;
using System.Collections.Generic;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class TrazasDeUnTrabajoController : EntidadController<ContextoSe, TrazaDeUnTrabajoDtm, TrazaDeUnTrabajoDto>
    {

        public TrazasDeUnTrabajoController(GestorDeTrazasDeUnTrabajo gestorDeNegocios, GestorDeErrores gestorDeErrores)
        : base
        (
          gestorDeNegocios,
          gestorDeErrores
        )
        {

        }

        public IActionResult CrudDeTrazasDeUnTrabajo()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext); 
            return ViewCrud(new DescriptorDeTrazasDeUnTrabajo(Contexto, ModoDescriptor.Mantenimiento));
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_ModificarPorId(TrazaDeUnTrabajoDto elemento, Dictionary<string, object> parametros)
        {
            throw new Exception("Los mensajes de la traza de un trabajo no son modificables");
        }

        protected override ParametrosDeNegocio AntesDeEjecutar_CrearElemento(TrazaDeUnTrabajoDto elemento)
        {
            throw new Exception("No se pueden crear trazas en un trabajo");
        }
    }
}
