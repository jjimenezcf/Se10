using System;
using System.Collections.Generic;
using Utilidades;
using Gestor.Errores;
using GestorDeElementos;
using ServicioDeDatos;
using ModeloDeDto;
using GestoresDeNegocio.TrabajosSometidos;
using GestoresDeNegocio.Entorno;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.Negocio
{
    public static class ApiParaElFlujo
    {

        public static ElementoDeUnProcesoDto Transitar(ContextoSe contexto, enumNegocio negocio, Dictionary<string, object> parametros)
        {
            var idTransicion = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idTransicion));
            var idElemento = Convert.ToInt32(parametros.LeerValor<long>(ltrParametrosEp.idElemento));
            var gestor = NegociosDeSe.CrearGestor(contexto, negocio);
            var elemento = (ElementoDeUnProcesoDto)gestor.LeerElementoPorId(idElemento, new Dictionary<string, object> { { ltrParametrosNeg.UsarLaCache, false } });

            if (elemento.ModoDeAcceso == ServicioDeDatos.Seguridad.enumModoDeAccesoDeDatos.SinPermiso)
                GestorDeErrores.Emitir($"El usuario conectado, {contexto.DatosDeConexion.Login}, no tiene permiso sobre el elemento '{elemento.Referencia}' que quiere transitar");

            var transaccion = contexto.IniciarTransaccion();
            try
            {
                elemento = (ElementoDeUnProcesoDto)gestor.TransitarElemento(elemento, idTransicion, parametros);
                
                var idUsuario = Convert.ToInt32((long)parametros.LeerValor(ltrParametrosEp.usuarios,(long) 0));
                if (idUsuario > 0)
                {
                    var adjunto = new TipoDtoElmento { TipoDto = negocio.TipoDto().FullName, IdElemento = elemento.Id, Referencia = elemento.Nombre };
                    GestorDeCorreos.CrearCorreoPara(contexto
                        , new List<string> { GestorDeUsuarios.LeerUsuario(contexto, idUsuario).eMail }
                        , ltrDeTrazas.transicionRealizada + 
                          $"{(parametros.LeerValor(ltrParametrosEp.asunto,"").IsNullOrEmpty() 
                          ? "" 
                          : parametros.LeerValor<string>(ltrParametrosEp.asunto))}"
                        , "Le informamos que se ha transitado el elemento adjunto" +
                          $"{(parametros.LeerValor(ltrParametrosEp.detalleAsunto, "").IsNullOrEmpty() 
                          ? "" 
                          : Environment.NewLine + parametros.LeerValor<string>(ltrParametrosEp.detalleAsunto))}"
                        , new List<TipoDtoElmento>() { adjunto }
                        , new List<string>()
                        );
                }
                contexto.Commit(transaccion);
            }
            catch
            {
                contexto.Rollback(transaccion);
                throw;
            }
            return elemento;
        }
    }
}
