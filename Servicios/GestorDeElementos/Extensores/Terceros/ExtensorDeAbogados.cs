using System;
using System.Linq;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Terceros;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto.Negocio;

namespace GestorDeElementos.Extensores
{

    public static class ExtensorDeAbogados
    {
        public static IQueryable<VinculoDtm> Abogados(this enumNegocio negocio, ContextoSe contexto)
        {
            throw new Exception($"Se debe indicar como obtener los abogados vinculados al negocio: {negocio}");
        }


  
        public static DireccionDto DireccionFiscal(this AbogadoDtm abogado, ContextoSe contexto)
        =>
        abogado.DireccionDto(contexto, enumCalificadorDireccion.fiscal, true);

        private static DireccionDto DireccionDto(this AbogadoDtm abogado, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direccion = abogado.Direccion(contexto, calificador, errorSiNoHay);
            return direccion.MapearDto(contexto, direccion.Negocio);
        }

        public static DireccionDtm Direccion(this AbogadoDtm abogado, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Abogado, abogado.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion is not null)
            {
                direccion.Negocio = enumNegocio.Abogado;
                return direccion;
            }
            return abogado.Interlocutor(contexto).Direccion(contexto, calificador, errorSiNoHay);
        }


        //public static string Referencia(this AbogadoDtm abogado, ContextoSe contexto)
        //{
        //    if (abogado.Interlocutor != null)
        //        return abogado.Interlocutor.Referencia(contexto);

        //    return contexto.SeleccionarPorId<InterlocutorDtm>(abogado.IdInterlocutor, aplicarJoin: true).Referencia(contexto);
        //}

        public static InterlocutorDtm Interlocutor(this AbogadoDtm abogado, ContextoSe contexto)
        =>
        abogado.Interlocutor != null
        ? abogado.Interlocutor
        : contexto.SeleccionarPorId<InterlocutorDtm>(abogado.IdInterlocutor, aplicarJoin: true);

        public static string NIF(this AbogadoDtm abogado, ContextoSe contexto) => abogado.Interlocutor(contexto).NIF(contexto);


    }
}
