using AutoMapper;
using DocumentFormat.OpenXml.Presentation;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Negocio;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using Utilidades;

namespace GestoresDeNegocio.Negocio
{

    public class GestorDeConsultasConGuid : GestorDeElementos<ContextoSe, ConsultaConGuidDtm, ConsultaConGuidDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;


        public GestorDeConsultasConGuid(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeConsultasConGuid Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeConsultasConGuid(contexto, mapeador);
        }

        public static void ValidarGuid(ContextoSe contexto, enumNegocio negocio, int id, string guid)
        {
            if (!GuidValido(contexto, negocio, id, guid))
                GestorDeErrores.Emitir("No está permitido el acceso al elemento indicado, o el acceso ha caducado, solicítelo");

            contexto.GuidDeConsulta = guid;
        }


        public static bool GuidValido(ContextoSe contexto, enumNegocio negocio, int id, string guid)
        {
            if (!negocio.PermiteConultasConGuid())
            {
                GestorDeErrores.Emitir($"El negocio '{negocio.ToNombre()}' no permite consultas por Guid");
            }

            var registro = contexto.SeleccionarPorAk<ConsultaConGuidDtm>(new Dictionary<string, object>
            {
                {nameof(ConsultaConGuidDtm.IdNegocio), negocio.IdNegocio() },
                {nameof(ConsultaConGuidDtm.Guid), guid },
                {nameof(ConsultaConGuidDtm.IdElemento),id }
            }, errorSiNoHay: false);

            if (registro == null || DateTime.Now > registro.CaducaEl)
            {
                return false;
            }

            return true;
        }

        public static string RegistrarConsultaConGuid(IElementoDtm elementoDtm, ContextoSe contexto, DateTime? caducaEl, int? maximoDeDescargas, bool auditar = true)
        {
            if (!elementoDtm.EsInterventor(contexto))
            {
                GestorDeErrores.Emitir("No se puede gestionar la consulta con Guid, el usuario no es interventor del elemento");
            }

            var desactivar = caducaEl.HasValue && caducaEl < DateTime.Now;

            var consultasDefinidas = desactivar
                ? contexto.SeleccionarTodos<ConsultaConGuidDtm>(new Dictionary<string, object>
                    {
                        {nameof(ConsultaConGuidDtm.IdNegocio), NegociosDeSe.IdNegocio(elementoDtm.GetType()) },
                        {nameof(ConsultaConGuidDtm.IdElemento), elementoDtm.Id }
                    })
                : contexto.SeleccionarTodos<ConsultaConGuidDtm>(new Dictionary<string, object>
                    {
                        {nameof(ConsultaConGuidDtm.IdNegocio), NegociosDeSe.IdNegocio(elementoDtm.GetType()) },
                        {nameof(ConsultaConGuidDtm.IdElemento), elementoDtm.Id },
                        {nameof(ConsultaConGuidDtm.IdUsuario), contexto.Usuario.Id }
                    });

            var consultasActivas = consultasDefinidas.Where(c => c.CaducaEl > DateTime.Now && (c.Descargas == null || (maximoDeDescargas != null && c.Descargas <= maximoDeDescargas))).ToList();


            if (desactivar)
            {
                foreach (var consultaActiva in consultasActivas)
                {
                    consultaActiva.CaducaEl = DateTime.Now;
                    consultaActiva.Modificar(contexto);
                }
                if (auditar)
                {
                    ((IUsaTraza)elementoDtm).CrearTraza(contexto, "Desactivación de consultas con Guid", $"El usuario '{contexto.Usuario.Login}' a desactivado todas las consultas con guid");
                }
                return null;
            }

            if (!consultasActivas.Any())
                return elementoDtm.RegistrarConsultaConGuid(contexto, caducaEl, maximoDeDescargas, auditar);

            var consulta = consultasActivas.First();
            consulta.CaducaEl = caducaEl;
            consulta.MaximoDescargas = maximoDeDescargas;
            consulta.Modificar(contexto);

            if (auditar)
            {
                ((IUsaTraza)elementoDtm).CrearTraza(contexto, "Renovación de consulta con Guid", $"El usuario '{contexto.Usuario.Login}' a actualizado la consulta con el guid '{consulta.Guid}' hasta el '{caducaEl.FechaCorta()}'");
            }

            return consulta.Guid.ToString();

        }
    }
}
