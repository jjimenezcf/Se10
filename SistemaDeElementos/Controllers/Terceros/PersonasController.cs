using Gestor.Errores;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using Microsoft.AspNetCore.Mvc;
using ModeloDeDto.Terceros;
using MVCSistemaDeElementos.Descriptores;
using ServicioDeDatos;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace MVCSistemaDeElementos.Controllers
{
    public class PersonasController : EntidadController<ContextoSe, PersonaDtm, PersonaDto>
    {
        public PersonasController(GestorDePersonas gestorDePersonas, GestorDeErrores gestorDeErrores)
         : base
         (
           gestorDePersonas,
           gestorDeErrores
         )
        {
        }

        public IActionResult CrudPersonas()
        {
            ApiController.CumplimentarDatosDeUsuarioDeConexion(Contexto, Contexto.Mapeador, HttpContext);

            var modo = ModoDescriptor.Mantenimiento;
            var indice = $"{Contexto.DatosDeConexion.IdUsuario.ToString()}-{modo}-{typeof(DescriptorDePersonas).FullName}";
            var cache = ServicioDeCaches.Obtener(CacheDe.RenderCrud);
            try
            {
                if (cache.ContainsKey(indice))
                {
                    ViewBag.DatosDeConexion = DatosDeConexion;
                    var destino = $"../{enumNameSpaceTs.Terceros}/{nameof(CrudPersonas)}";
                    return base.View(destino, new DescriptorDePersonas(Contexto, (string)cache[indice]));
                }
                else
                {
                    var descriptor = DescriptorDeCrud<PersonaDto>.CrearDescriptor(Contexto, modo, () => new DescriptorDePersonas(Contexto, modo));
                    return ViewCrud(descriptor);
                }
            }
            catch (Exception e)
            {
                return RenderizarErrorDe(indice, e);
            }
        }

        protected override Dictionary<string, object> IndicadoresParaInicializarLaVistaMnt(ContextoSe contexto, Dictionary<string, object> parametros)
        {
            var indicadores = base.IndicadoresParaInicializarLaVistaMnt(contexto, parametros);
            indicadores.Add(IndInterlocutor.TercerosJudiciales, ExtensorDePleitos.ModuloActivo(contexto) || ExtensorDeExpedientes.HayTiposJuridicos(Contexto));
            return indicadores;
        }

        protected override dynamic ProcesarOpcionMf(enumNegocio negocio, string opcion, Dictionary<string, object> parametros)
        {
            switch (opcion)
            {
                case eventosDeMf.Interlocutores:
                    return negocio.CrearInterlocutores(Contexto, (List<int>)parametros[ltrParametrosEp.ids]);
            }
            return base.ProcesarOpcionMf(negocio, opcion, parametros);
        }

    }
}
