using NUnit.Framework;
using ServicioDeDatos;
using System;
using ServicioDeDatos.Elemento;
using Utilidades;
using GestorDeElementos;
using ServicioDeDatos.Terceros;
using ValidacionesBase;
using SistemaDeElementos.Inicializador.Acromur;

namespace ValidacionesDeSql
{

    public class ObservacionseSql
    {

        [Test]
        public void ObservacionesDeUnElemento()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            var tabla = ApiDeElementoDtm.TablaDeObservacion(enumNegocio.Sociedad.TipoDtm());
            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), InzAcromur.n_nif_acromur);
            ObservacionSql.ObservacionesDeUnElemento(contexto, tabla, sociedad.Id, 0, -1);
            Assert.Pass();
        }

        [Test]
        public void LeerPorId()
        {
            var contexto = Inicializaciones.CrearContextoParaUsuario(ContextoSe.Login_Admin);
            void prueba()
            {
                var tabla = ApiDeElementoDtm.TablaDeObservacion(enumNegocio.Sociedad.TipoDtm());
                var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), InzAcromur.n_nif_acromur);
                var observacion = new ObservacionesDeUnaSociedadDtm();
                observacion.IdElemento = sociedad.Id;
                observacion.Nombre = "Mi observación";
                observacion.Descripcion = "mi descripción";
                var insertada = observacion.Insertar(contexto);
                if (ObservacionSql.LeerPorId(contexto, tabla, insertada.Id, true).Nombre != observacion.Nombre)
                    throw new Exception("No se ha leido correctamente la observación");
            }
            ApiDeValidaciones.EjecutarConRollback(contexto, prueba);
        }
    }
}