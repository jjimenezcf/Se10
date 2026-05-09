using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Utilidades;
using Gestor.Errores;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Microsoft.Win32;

namespace GestorDeElementos
{
    public interface IGestorDeRelaciones
    {
        public enumNegocio Negocio1 { get; }
        public enumNegocio Negocio2 { get; }
    }

    public class GestorDeRelaciones<TContexto, TRelacion, TElemento> : GestorDeElementos<TContexto, TRelacion, TElemento>, IGestorDeRelaciones
    where TRelacion : RelacionDtm
    where TElemento : ElementoDto
    where TContexto : ContextoSe
    {
        public enumNegocio Negocio1 => ApiDeRelaciones.ObtenerTipoDtm<TRelacion>(enumDtmsDeRelacion.Negocio1).NegocioDeUnDtm();
        public enumNegocio Negocio2 => ApiDeRelaciones.ObtenerTipoDtm<TRelacion>(enumDtmsDeRelacion.Negocio2).NegocioDeUnDtm();

        protected bool PermitirMasDeUnRegistroParaLosMismosIds { get; set; } = false;

        public GestorDeRelaciones(TContexto contexto, IMapper mapper)
        : base(contexto, mapper)
        {
        }

        public GestorDeRelaciones(TContexto contexto)
        :base(contexto)
        {
        }

        public (TRelacion relacio, bool existe) CrearRelacion(TRelacion relacionDtm)
        {
            //ApiDePermisos.ValidarPermisosDeRelacion(Contexto, Negocio1, Negocio2, relacionDtm);
            relacionDtm = PersistirRegistro(relacionDtm, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return (relacionDtm, false);
        }


        public (TRelacion relacio, bool existe) CrearRelacion(TElemento relacionDto, ParametrosDeNegocio parametros, bool errorSiYaExiste)
        {
            TRelacion relacion = MapearRegistro(relacionDto, parametros);
            if (PermitirMasDeUnRegistroParaLosMismosIds)
                return CrearRelacion(relacion);

            var f1 = new ClausulaDeFiltrado(relacion.PropiedadDelIdElemento1, enumCriteriosDeFiltrado.igual, relacion.IdElemento1.ToString());
            var f2 = new ClausulaDeFiltrado(relacion.PropiedadDelIdElemento2, enumCriteriosDeFiltrado.igual, relacion.IdElemento2.ToString());
            var filtros = new List<ClausulaDeFiltrado> { f1, f2 };
            var registros = LeerRegistros(0, 1, filtros, new List<ClausulaDeOrdenacion>(), new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo)).ToList();
            if (registros.Count == 0)
            {
                return CrearRelacion(relacion);
            }
            if (errorSiYaExiste)
                GestorDeErrores.Emitir("ya existe la relación para las claves indicadas");
            relacion = registros[0];
            return (relacion, true);
        }

        public (TRelacion relacio, bool existe) CrearRelacion(string propiedadIdElemento1, int idElemento1, int idElemento2, bool errorSiYaExiste, Dictionary<string,object> parametros = null)
        {
            TRelacion relacion = ApiDeRegistroDtm.RegistroVacio<TRelacion>();

            var filtros = new List<ClausulaDeFiltrado>();
            DefinirFiltroDeRelacion(relacion, filtros, propiedadIdElemento1, idElemento1, idElemento2);
            var registros = LeerRegistros(0, 1, filtros, null, null).ToList();

            if (registros.Count != 0 && errorSiYaExiste)
                GestorDeErrores.Emitir($"El registro {relacion} ya existe");

            if (registros.Count == 0)        
            {
               // ApiDePermisos.ValidarPermisosDeRelacion(Contexto, Negocio1, Negocio2, relacion);
                MapearDatosDeRelacion(relacion, propiedadIdElemento1, idElemento1, idElemento2);
                return (PersistirRegistro(relacion, new ParametrosDeNegocio(enumTipoOperacion.Insertar,parametros)), false);
            }

            return (registros[0], true);
        }

        public TRelacion ModificarRelacion(TElemento relacionDto, ParametrosDeNegocio parametros)
        {
            TRelacion relacion = MapearRegistro(relacionDto, parametros);
            //ApiDePermisos.ValidarPermisosDeRelacion(Contexto, Negocio1, Negocio2, relacion);
            relacion = PersistirRegistro(relacion, new ParametrosDeNegocio(enumTipoOperacion.Modificar, parametros: parametros.Parametros));
            return relacion;
        }

        public void BorrarRelacion(int id, Dictionary<string, object> parametros)
        {
            var registro = LeerRegistroPorId(id, true, false, false, false);
           // ApiDePermisos.ValidarPermisosDeRelacion(Contexto, Negocio1, Negocio2, registro);
            PersistirRegistro(registro, new ParametrosDeNegocio(enumTipoOperacion.Eliminar, parametros: parametros));
        }


        private void DefinirFiltroDeRelacion(TRelacion registro, List<ClausulaDeFiltrado> filtros, string propiedadIdElemento1, int idElemento1, int idElemento2)
        {
            var propiedades = registro.PropiedadesDelObjeto();
            foreach (var propiedad in propiedades)
            {
                var c = new ClausulaDeFiltrado
                {
                    Clausula = propiedad.Name,
                    Criterio = enumCriteriosDeFiltrado.igual
                };

                if (propiedad.Name == registro.LeerPropiedad(nameof(IRelacion.PropiedadDelIdElemento1)).ToString())
                    c.Valor = propiedad.Name.Equals(propiedadIdElemento1, StringComparison.CurrentCultureIgnoreCase) ? idElemento1.ToString() : idElemento2.ToString();

                if (propiedad.Name == registro.LeerPropiedad(nameof(IRelacion.PropiedadDelIdElemento2)).ToString())
                    c.Valor = propiedad.Name.Equals(propiedadIdElemento1, StringComparison.CurrentCultureIgnoreCase) ? idElemento1.ToString() : idElemento2.ToString();

                if (c.Valor.Entero() > 0)
                    filtros.Add(c);

                if (filtros.Count == 2)
                    break;
            }
        }


        private void MapearDatosDeRelacion(TRelacion registro, string propiedadIdElemento1, int idElemento1, int idElemento2)
        {
            var propiedades = registro.PropiedadesDelObjeto();
            foreach (var propiedad in propiedades)
            {
                if (propiedad.Name.Equals(registro.LeerPropiedad(nameof(IRelacion.PropiedadDelIdElemento1)).ToString(), StringComparison.CurrentCultureIgnoreCase))
                    propiedad.SetValue(registro, propiedadIdElemento1.Equals(propiedad.Name, StringComparison.CurrentCultureIgnoreCase) ? idElemento1 : idElemento2);

                if (propiedad.Name.Equals(registro.LeerPropiedad(nameof(IRelacion.PropiedadDelIdElemento2)).ToString(), StringComparison.CurrentCultureIgnoreCase))
                    propiedad.SetValue(registro, propiedadIdElemento1.Equals(propiedad.Name, StringComparison.CurrentCultureIgnoreCase) ? idElemento1 : idElemento2);
            }
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(TRelacion registro, TElemento elemento, ParametrosDeNegocio parametros)
        {

            if (typeof(TRelacion).ImplementaAmpliacion())
            {
                elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, ((IAmpliacion)registro).Negocio, ((IAmpliacion)registro).IdElemento);
                return;
            }

            if (typeof(TRelacion).ImplementaDetalle())
            {
                elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, ((IDetalle)registro).Negocio, ((IDetalle)registro).IdElemento);
                return;
            }

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAccesoDeRelacion(Contexto, ((IGestorDeRelaciones)this).Negocio1, ((IGestorDeRelaciones)this).Negocio2, registro);
        }

    }

}
