using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using ModeloDeDto.Entorno;
using ServicioDeDatos.Entorno;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using GestoresDeNegocio.Seguridad;
using ServicioDeDatos.Seguridad;
using Gestor.Errores;
using ServicioDeDatos.SistemaDocumental;
using System;
using ServicioDeDatos.Terceros;

namespace GestoresDeNegocio.Entorno
{
    public class GestorDeCertificados : GestorDeElementos<ContextoSe, CertificadoDtm, CertificadoDto>
    {
        public override enumNegocio Negocio => enumNegocio.Certificado;

        public class MapearCertificados : Profile
        {
            public MapearCertificados()
            {
                CreateMap<CertificadoDtm, CertificadoDto>()
                .ForMember(x => x.Gestor, y => y.MapFrom(y => y.Gestor.Nombre));
                CreateMap<CertificadoDto, CertificadoDtm>()
                .ForMember(x => x.Gestor, y => y.Ignore())
                .ForMember(x => x.IdArchivo, dto => dto.MapFrom(dto => dto.IdArchivoDelCertificado))
                .ForMember(x => x.Password, dto => dto.MapFrom(dto => dto.PassworDelCertificado));
            }
        }

        public GestorDeCertificados(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeCertificados Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCertificados(contexto, mapeador);
        }

        protected override IQueryable<CertificadoDtm> AplicarJoins(IQueryable<CertificadoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Gestor);
            return consulta;
        }

        protected override IQueryable<CertificadoDtm> AplicarSeguridad(IQueryable<CertificadoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador || parametros.Firmar)
            {
                consulta = consulta.Where(Certificados => Contexto.Set<UsuariosDeUnPermisoDtm>().Any(
                    permisos => permisos.IdUsuario == Contexto.DatosDeConexion.IdUsuario &&
                    permisos.IdPermiso == Certificados.IdGestor
                    ));
            }
            return consulta;
        }

        protected override IQueryable<CertificadoDtm> AplicarFiltros(IQueryable<CertificadoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            var filtro = filtros.FirstOrDefault(x => x.Clausula.ToLower() == ltrCertificados.FiltrarParaSociedad.ToLower());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                filtros.Add(new ClausulaDeFiltrado(nameof(CertificadoDtm.Nombre), enumCriteriosDeFiltrado.contiene, filtro.Valor));
                consulta = consulta.Where(x => x.Clase == enumClaseDeCertificados.CertificadoCorporativo);
            }

            return consulta;
        }

        protected override void DespuesDeMapearElRegistro(CertificadoDto elemento, CertificadoDtm registro, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(elemento, registro, opciones);
        }

        protected override void AntesDePersistir(CertificadoDtm certificado, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(certificado, parametros);

            if (certificado.Clase == enumClaseDeCertificados.Personal && !(bool)parametros.Parametros.LeerValor(ltrCertificados.EsMiCertificado, false))
                GestorDeErrores.Emitir($"No se puede ni crear ni modificar un certificado personal");

            if (parametros.Insertando)
            {
                certificado.IdGestor = GestorDePermisos.CrearObtener(Contexto, enumNegocio.Certificado, $"{certificado.Nombre}", enumClaseDePermiso.Certificado, enumModoDeAccesoDeDatos.Gestor).Id;
                certificado.SubidoEl = DateTime.Now;
            }
            if (parametros.Modificando)
            {
                if (certificado.IdArchivo == null)
                {
                    certificado.IdArchivo = ((CertificadoDtm)parametros.registroEnBd).IdArchivo;
                    certificado.Password = ((CertificadoDtm)parametros.registroEnBd).Password;
                }
                else
                {
                    certificado.SubidoEl = DateTime.Now;
                    ApiDeCertificados.ValidarCertificado(Contexto, certificado);
                }
            }

            if (parametros.Eliminando)
            {
                if (Contexto.SeleccionarPorFk<UsuarioDtm>(nameof(UsuarioDtm.IdCertificado), certificado.Id, errorSiNoHay: false) != null)
                {
                    GestorDeErrores.Emitir($"El usuario tiene el certificado '{certificado.Nombre}' asociado, desasócielo primero");
                }
                var certificadoDeSociedad = Contexto.SeleccionarPorFk<CertificadosDeUnaSociedadDtm>(nameof(CertificadosDeUnaSociedadDtm.idElemento2), certificado.Id, errorSiNoHay: false, aplicarJoin:true);
                if ( certificadoDeSociedad!= null)
                {
                    var sociedad = Contexto.SeleccionarPorId<SociedadDtm>(certificadoDeSociedad.idElemento1);
                    GestorDeErrores.Emitir($"la sociedad '{sociedad.Referencia}' tiene el certificado '{certificado.Nombre}' asociado, desasócielo primero");
                }

                var archivosFirmados = Contexto.SeleccionarTodos<FirmadoDtm>(nameof(FirmadoDtm.IdCertificado), certificado.Id, parametros: new Dictionary<string, object>
                {
                    {ltrParametrosNeg.AplicarElOrden, new List<ClausulaDeOrdenacion> { new ClausulaDeOrdenacion(nameof(FirmadoDtm.Id), ModoDeOrdenancion.descendente) } },
                    {ltrParametrosNeg.CantidadPorLeer, 1 }
                });
                if (archivosFirmados.Count > 0)
                {
                    var original = Contexto.SeleccionarPorId<ArchivoDtm>(archivosFirmados[0].IdOriginal);
                    GestorDeErrores.Emitir($"Hay archivos firmados con el certificado '{certificado.Nombre}', el último fue '{original.Nombre}'");
                }

            }
        }

        protected override void DespuesDePersistir(CertificadoDtm certificado, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(certificado, parametros);

            if (parametros.Modificando || parametros.Insertando)
            {
                GestorDePassword.RegistrarPasswordDeCertificado(Contexto, certificado.Id, certificado.Password);
            }

            if (parametros.Insertando)
            {
                ApiDeCertificados.ValidarCertificado(Contexto, certificado);
            }

            if (parametros.Modificando)
            {
                GestorDePermisos.ModificarPermisoDeDatos(Contexto, enumNegocio.Certificado, certificado.IdGestor, certificado.Nombre, enumClaseDePermiso.Certificado, enumModoDeAccesoDeDatos.Gestor);
            }

            if (parametros.Eliminando)
            {
                GestorDePermisos.EliminarRegistroPorId(Contexto, certificado.IdGestor, parametros: parametros.Parametros);
            }
        }

        protected override void DespuesDeMapearElElemento(CertificadoDtm registro, CertificadoDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.PassworDelCertificado = null;
            elemento.DatosCertificado = registro.Datos;
        }

        public static (bool creado, CertificadoDtm certificado) PersisitirMiCertificado(ContextoSe contexto, UsuarioDtm usuario, int idArchivo, string password)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            if (usuario.IdCertificado.Entero() > 0)
            {
                var certificado = contexto.SeleccionarPorId<CertificadoDtm>(usuario.IdCertificado.Entero());
                certificado.Nombre = usuario.MiCertificado;
                certificado.IdArchivo = archivo.Id;
                certificado.Password = password;
                certificado.Modificar(contexto, new Dictionary<string, object> { { ltrCertificados.EsMiCertificado, true } });
                return (false, certificado);
            }
            else
            {
                var certificado = new CertificadoDtm
                {
                    Nombre = usuario.MiCertificado,
                    Clase = enumClaseDeCertificados.Personal,
                    Password = password,
                    IdArchivo = archivo.Id
                }
                .Insertar(contexto, new Dictionary<string, object> { { ltrCertificados.EsMiCertificado, true } });
                return (true, certificado);
            }
        }




    }
}
