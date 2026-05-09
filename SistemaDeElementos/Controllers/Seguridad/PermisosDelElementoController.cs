using ServicioDeDatos;
using Gestor.Errores;
using GestoresDeNegocio.Negocio;
using ModeloDeDto.Negocio;
using AutoMapper;
using System.Collections.Generic;
using System;
using GestorDeElementos;
using System.Linq;
using GestoresDeNegocio.Seguridad;
using ModeloDeDto.Seguridad;

namespace MVCSistemaDeElementos.Controllers
{
    public class PermisosDelElementoController : BaseController<PermisosDelElementoDto>
    {
        public PermisosDelElementoController(ContextoSe contexto, IMapper mapeador, GestorDeErrores gestorDeErrores)
        : base(gestorDeErrores, contexto, mapeador)
        {
        }

        protected override PermisosDelElementoDto LeerPorId(int idElemento, Dictionary<string, object> parametros)
        {
            if (!parametros.Keys.Contains(NegocioPor.idNegocio))
                GestorDeErrores.Emitir("Debe definir el negocio del que han de leerse las PermisosDelElemento");

            var id32 = Convert.ToInt32(parametros[NegocioPor.idNegocio]);

            var negocioDtm = GestorDeNegocios.LeerNegocio(Contexto, id32);

            var gestor = GestorDePemisosDelElemento.Gestor(Contexto, NegociosDeSe.ToEnumerado(negocioDtm.Nombre));
            //Dictionary<string, object> opcionesDelMapeo = new Dictionary<string, object>{ { ltrParametrosNeg.ErrorSiNoLoHay, false } };
            return gestor.LeerElementoPorId(idElemento);
        }

    }

}
