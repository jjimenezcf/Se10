using System;
using System.Linq;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.Negocio;

namespace GestorDeElementos.Extensores
{

    public static class ExtensorDeProcuradores
    {
        public static IQueryable<VinculoDtm> Procuradores(this enumNegocio negocio, ContextoSe contexto)
        {
            throw new Exception($"Se debe indicar como obtener los procuradores vinculados al negocio: {negocio}");
        }

        public static DireccionDto DireccionFiscal(this ProcuradorDtm procurador, ContextoSe contexto)
        =>
        procurador.DireccionDto(contexto, enumCalificadorDireccion.fiscal, true);

        private static DireccionDto DireccionDto(this ProcuradorDtm procurador, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direccion = procurador.Direccion(contexto, calificador, errorSiNoHay);
            return direccion.MapearDto(contexto, direccion.Negocio);
        }

        public static DireccionDtm Direccion(this ProcuradorDtm procurador, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Procurador, procurador.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion is not null)
            {
                direccion.Negocio = enumNegocio.Procurador;
                return direccion;
            }
            return procurador.Interlocutor(contexto).Direccion(contexto, calificador, errorSiNoHay);
        }


        //public static string Referencia(this ProcuradorDtm procurador, ContextoSe contexto)
        //{
        //    if (procurador.Interlocutor != null)
        //        return procurador.Interlocutor.Referencia(contexto);

        //    return contexto.SeleccionarPorId<InterlocutorDtm>(procurador.IdInterlocutor, aplicarJoin: true).Referencia(contexto);
        //}

        public static InterlocutorDtm Interlocutor(this ProcuradorDtm procurador, ContextoSe contexto)
        =>
        procurador.Interlocutor != null
        ? procurador.Interlocutor
        : contexto.SeleccionarPorId<InterlocutorDtm>(procurador.IdInterlocutor, aplicarJoin: true);

        public static string NIF(this ProcuradorDtm procurador, ContextoSe contexto) => procurador.Interlocutor(contexto).NIF(contexto);

    }
}
