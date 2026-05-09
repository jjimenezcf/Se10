using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using System;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{

    public class ltrDeUnRegistroEs
    {
        public const string MostraRegistrosEsRelacionados = nameof(MostraRegistrosEsRelacionados);
        public const string IdRegistroEs = nameof(IdRegistroEs);
    }


    public static class ExtensorDeRegistroEs
    {
        public static IQueryable<VinculoDtm> RegistrosEs(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Expediente:
                    return contexto.Set<RegistrosDeUnExpedienteDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<RegistrosDeUnPleitoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<RegistrosDeUnContratoDtm>();
            }
            throw new Exception($"Se debe indicar como obtener los registros de E/S vinculadas al negocio: {negocio}");
        }
    }
}
