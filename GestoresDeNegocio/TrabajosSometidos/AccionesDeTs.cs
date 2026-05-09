using System.Collections.Generic;
using GestorDeElementos;
using GestoresDeNegocio.Seguridad;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.TrabajosSometidos;

namespace GestoresDeNegocio.TrabajosSometidos
{
    public static class AccionesDeTs
    {
        public const string N_CrearCorreoParaPt = "Envía un mensaje al los usuarios de un Pt";
        public static Dictionary<string, object> CrearCorreoParaPt(EntornoDeUnaAccion entorno)
        {
            var gestor = GestorDeUsuariosDeUnPuesto.Gestor(entorno.Contexto, entorno.Contexto.Mapeador);
            var filtro = new ClausulaDeFiltrado(nameof(PuestosDeUnUsuarioDtm.IdPuesto)
                , enumCriteriosDeFiltrado.igual
                , entorno.Parametros[nameof(PuestosDeUnUsuarioDtm.IdPuesto)].ToString());

            var usuarios = gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro },
                               parametros: new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, true));

            var datos = new TipoDtoElmento
            {
                TipoDto = entorno.Negocio.TipoDto().FullName,
                IdElemento = entorno.Registro.Id,
                Referencia = ((ElementoDtm)entorno.Registro).Expresion
            };

            foreach (var usuario in usuarios)
            {
                GestorDeCorreos.CrearCorreoPara(entorno.Contexto, new List<string> { usuario.Usuario.eMail }
                , entorno.Parametros[nameof(CorreoDtm.Asunto)].ToString()
                , entorno.Parametros[nameof(CorreoDtm.Cuerpo)].ToString()
                , new List<TipoDtoElmento> { datos }
                , new List<string>());
            }
            return entorno.Salida;
        }

    }
}
