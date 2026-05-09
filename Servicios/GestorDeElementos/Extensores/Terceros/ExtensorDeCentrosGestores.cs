using System.Collections.Generic;
using System.Linq;
using Gestor.Errores;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using Utilidades;

namespace GestorDeElementos.Extensores
{
    public static class ExtensionCentrosGestores
    {
        public static CentroGestorDtm CrearHijo(this CentroGestorDtm cgPadre, ContextoSe contexto, string nombreCg, string codigo)
        {
            var f1 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, cgPadre.Sociedad.Id);
            var f2 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.Nombre), enumCriteriosDeFiltrado.igual, nombreCg);
            var cgs = enumNegocio.CentroGestor.SeleccionarPorFiltro<CentroGestorDtm>(contexto, new List<ClausulaDeFiltrado> { f1, f2 });
            if (cgs.Count == 0)
            {
                var cg = new CentroGestorDtm();
                cg.Nombre = nombreCg;
                cg.Codigo = codigo;
                cg.eMail = cgPadre.Sociedad.eMail;
                cg.Sigla = cgPadre.Sociedad.CodigoFiscal + "." + codigo.Split(".")[1];
                cg.IdSociedad = cgPadre.Sociedad.Id;
                cg.IdResponsable = cgPadre.Sociedad.IdUsuaCrea;
                cg.IdCgPadre = cgPadre.Id;
                cgs.Add(cg.Insertar(contexto));
            }
            return cgs[0];
        }

        public static PuestoDtm PuestoDeTrabajo(this CentroGestorDtm cg, ContextoSe contexto, string nombre, string descripcion, bool errorSiExiste = false)
        {
            var puestos = contexto.SeleccionarTodos<PuestoDtm>(new Dictionary<string, object>
            {
                {nameof(PuestoDtm.IdCg), cg.Id },
                {nameof(PuestoDtm.Nombre), nombre}
            });

            if (puestos.Count == 0)
                return new PuestoDtm { IdCg = cg.Id, Nombre = nombre, Descripcion = descripcion }.Insertar(contexto);

            if (errorSiExiste)
                GestorDeErrores.Emitir($"El PT '{puestos[0].Expresion}' que está intentando crear, ya existe en el sistema para el Cg '{cg.Expresion}'");

            if (puestos[0].Descripcion == descripcion)
            {
                return puestos[0];
            }

            puestos[0].Descripcion = descripcion;

            return puestos[0].Modificar(contexto);
        }

        public static List<TrabajadorDtm> Trabajadores(this CentroGestorDtm cg, ContextoSe contexto)
        =>
        contexto.SeleccionarTodos<TrabajadorDtm>(new Dictionary<string, object> { { nameof(PuestoDtm.IdCg), cg.Id } });

        public static CentroGestorDtm Cfg_CG_De_Documentacion(ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Cfg_CG_De_Documentacion)))
            {
                var idUsuario = contexto.DatosDeConexion.IdUsuario;
                contexto.AsignarUsuario(contexto.Administrador());
                try
                {
                    var sociedad = ExtensorDeSociedades.Cfg_Sociedad_Del_Sistema(contexto);
                    var codigoCG = sociedad.CodigoFiscal.IsNullOrEmpty()
                      ? ltrDeSociedad.CodigoCgDocumentacion
                      : sociedad.CodigoFiscal + "." + ltrDeSociedad.CodigoCgDocumentacion;
                    var nombreCG = CacheDeVariable.Cfg_CG_De_Documentacion;
                    var cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), codigoCG, errorSiNoHay: false, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });

                    if (cg != null && cg.Baja)
                        GestorDeErrores.Emitir($"El cento documental con código '{codigoCG}' está de baja, delo de alta");

                    if (cg == null)
                    {
                        cg = contexto.SeleccionarPorNombre<CentroGestorDtm>(CacheDeVariable.Cfg_CG_De_Documentacion, errorSiNoHay: false, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true} });
                        
                        if (cg != null && cg.Baja)
                            GestorDeErrores.Emitir($"El cento documental '{cg.Expresion}' está de baja, delo de alta");

                        if (cg == null) cg = new CentroGestorDtm
                        {
                            Nombre = CacheDeVariable.Cfg_CG_De_Documentacion,
                            Codigo = codigoCG,
                            eMail = sociedad.eMail,
                            Sigla = ltrDeSociedad.SiglasDelCGDeDocumentacion,
                            IdSociedad = sociedad.Id,
                            IdResponsable = sociedad.IdUsuaCrea,
                        }.Insertar(contexto);
                    }
                    cache[nameof(Cfg_CG_De_Documentacion)] = cg;
                }
                finally
                {
                    contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
                }
            }
            return (CentroGestorDtm)cache[nameof(Cfg_CG_De_Documentacion)];
        }

        public static CentroGestorDtm Cfg_CG_De_ClienteWeb(ContextoSe contexto, int? idSociedadSe)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Cfg_CG_De_ClienteWeb)))
            {
                var idUsuario = contexto.DatosDeConexion.IdUsuario;
                contexto.AsignarUsuario(contexto.Administrador());
                try
                {
                    var sociedadSe = idSociedadSe is not null
                    ? contexto.SeleccionarPorId<SociedadDtm>(idSociedadSe.Entero())
                    : ExtensorDeSociedades.Cfg_Sociedad_Del_Sistema(contexto);

                    var codigoCG = sociedadSe.CodigoFiscal + "." + ltrDeSociedad.CodigoCgClienteWeb;
                    if (sociedadSe.CodigoFiscal.IsNullOrEmpty())
                        GestorDeErrores.Emitir($"No ha definido un código fiscal para la sociedad '{sociedadSe.Referencia}', recuerde que se debe definir un CG en la sociedad gestionada con el [código fiscal].{ltrDeSociedad.CodigoCgClienteWeb}");
                    var nombreCG = CacheDeVariable.Cfg_CG_De_ClientesWeb;
                    var cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), codigoCG, errorSiNoHay: false);
                    if (cg == null)
                    {
                        cache[nameof(Cfg_CG_De_ClienteWeb)] = new CentroGestorDtm
                        {
                            Nombre = nombreCG,
                            Codigo = codigoCG,
                            eMail = sociedadSe.eMail,
                            Sigla = ltrDeSociedad.SiglasDelCGDeClienteWeb,
                            IdSociedad = sociedadSe.Id,
                            IdResponsable = sociedadSe.IdUsuaCrea,
                        }.Insertar(contexto);
                    }
                    else
                    {
                        cache[nameof(Cfg_CG_De_ClienteWeb)] = cg;
                    }
                }
                finally
                {
                    contexto.AsignarUsuario(contexto.SeleccionarPorId<UsuarioDtm>(idUsuario));
                }
            }
            return (CentroGestorDtm)cache[nameof(Cfg_CG_De_ClienteWeb)];
        }

        public static CentroGestorDtm Cg(this IUsaCg elemento, ContextoSe contexto, bool aplicarJoin = false)
        {
            elemento.Cg ??= contexto.SeleccionarPorId<CentroGestorDtm>(elemento.IdCg, aplicarJoin);

            if (aplicarJoin && elemento.Cg.Sociedad is null)
                elemento.Cg.Sociedad = contexto.SeleccionarPorId<SociedadDtm>(elemento.Cg.IdSociedad, aplicarJoin);

            return elemento.Cg;
        }

        public static SociedadDtm Sociedad(this CentroGestorDtm cg, ContextoSe contexto)
        =>
        cg.Sociedad ??= contexto.SeleccionarPorId<SociedadDtm>(cg.IdSociedad);

        public static bool EsGestor(this CentroGestorDtm cg, ContextoSe contexto)
        {
            if (contexto.DatosDeConexion.EsAdministrador)
                return true;
            var permisoDeGestor = cg.Gestor(contexto);
            var negocio = enumNegocio.CentroGestor.LeerNegocio();
            return contexto.Set<UsuariosDeUnPermisoDtm>().Any(x => (x.IdPermiso == permisoDeGestor.Id || x.IdPermiso == negocio.IdGestor || x.IdPermiso == negocio.IdAdministrador) && x.IdUsuario == contexto.DatosDeConexion.IdUsuario);
        }

        public static bool EsConsultor(this CentroGestorDtm cg, ContextoSe contexto)
        {
            if (contexto.DatosDeConexion.EsAdministrador)
                return true;
            var permisoDeConsulta = cg.Consultor(contexto);
            var negocio = enumNegocio.CentroGestor.LeerNegocio();
            return contexto.Set<UsuariosDeUnPermisoDtm>().Any(x => (x.IdPermiso == permisoDeConsulta.Id || x.IdPermiso == negocio.IdConsultor || x.IdPermiso == negocio.IdGestor || x.IdPermiso == negocio.IdAdministrador) && x.IdUsuario == contexto.DatosDeConexion.IdUsuario);
        }

        public static PermisoDtm Gestor(this CentroGestorDtm cg, ContextoSe contexto)
        =>
        cg.Gestor ??= contexto.SeleccionarPorId<PermisoDtm>(cg.IdGestor, aplicarJoin: true);

        public static PermisoDtm Consultor(this CentroGestorDtm cg, ContextoSe contexto)
        =>
        cg.Consultor ??= contexto.SeleccionarPorId<PermisoDtm>(cg.IdConsultor, aplicarJoin: true);

    }
}
