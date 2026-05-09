using AutoMapper;
using GestorDeElementos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos;
using Utilidades;
using ModeloDeDto;
using System;

namespace GestoresDeNegocio.SistemaDocumental
{

    public class GestorDeBloqueosDeUnArchivo : GestorDeElementos<ContextoSe, BloqueoDeUnArchivoDtm, ElementoDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;


        public GestorDeBloqueosDeUnArchivo(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeBloqueosDeUnArchivo Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeBloqueosDeUnArchivo(contexto, mapeador);
        }

        protected override void DespuesDePersistir(BloqueoDeUnArchivoDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            var motivo = parametros.Parametros.LeerValor<string>(nameof(AuditoriaDeUnArchivoDtm.Auditoria));
            new AuditoriaDeUnArchivoDtm
            {
                IdArchivo = registro.IdArchivo,
                Auditoria = Motivo(usuario: Contexto.DatosDeConexion.Login, motivo: motivo, bloqueado: registro.Bloqueado)
            }.Insertar(Contexto);
            ServicioDeCaches.EliminarElemento(CacheDe.Arc_BloqueoDeArchivos, registro.IdArchivo.ToString());
        }
        internal static string Motivo(string usuario, string motivo, bool bloqueado)
        {

            if (bloqueado)
                return ltrDeAuditoriaDeArchivo.Bloquear.Replace("[0]", usuario).Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")).Replace("[2]", motivo);
            return ltrDeAuditoriaDeArchivo.Desbloquear.Replace("[0]", usuario).Replace("[1]", DateTime.Now.ToString("dd-MM-yyyy HH:mm:ss")).Replace("[2]", motivo);
        }
    }
}
