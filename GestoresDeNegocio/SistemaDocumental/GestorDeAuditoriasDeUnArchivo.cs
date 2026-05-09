using AutoMapper;
using GestorDeElementos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto;

namespace GestoresDeNegocio.SistemaDocumental
{

    public class GestorDeAuditoriasDeUnArchivo : GestorDeElementos<ContextoSe, AuditoriaDeUnArchivoDtm, ElementoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;


        public GestorDeAuditoriasDeUnArchivo(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeAuditoriasDeUnArchivo Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeAuditoriasDeUnArchivo(contexto, mapeador);
        }

        protected override void DespuesDePersistir(AuditoriaDeUnArchivoDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Arc_AuditoriaDeArchivo, registro.IdArchivo.ToString());
        }
    }
}
