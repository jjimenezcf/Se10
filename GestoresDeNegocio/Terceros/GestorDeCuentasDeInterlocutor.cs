using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Utilidades;
using System.Linq;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Contabilidad;
using GestorDeElementos.Extensores;
using ServicioDeDatos.SistemaDocumental;
using System.IO;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeCuentasDeInterlocutor : GestorDeElementos<ContextoSe, CuentaDeInterlocutorDtm, CuentaDeInterlocutorDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class ltrCuentasDeInterlocutor
        {
        }

        public class MapearCuentasDeInterlocutor : Profile
        {
            public MapearCuentasDeInterlocutor()
            {
                CreateMap<CuentaDeInterlocutorDtm, CuentaDeInterlocutorDto>()
                .ForMember(dto => dto.Entidad, x => x.MapFrom(dtm => dtm.Cuenta.Entidad))
                .ForMember(dto => dto.Oficina, x => x.MapFrom(dtm => dtm.Cuenta.Oficina))
                .ForMember(dto => dto.DcCcc, x => x.MapFrom(dtm => dtm.Cuenta.DcCcc))
                .ForMember(dto => dto.Numero, x => x.MapFrom(dtm => dtm.Cuenta.Numero));
                CreateMap<CuentaDeInterlocutorDto, CuentaDeInterlocutorDtm>()
                .ForMember(dtm => dtm.Cuenta, x => x.Ignore())
                .ForMember(dtm => dtm.Elemento, x => x.Ignore());
            }
        }

        public GestorDeCuentasDeInterlocutor(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        protected override IQueryable<CuentaDeInterlocutorDtm> AplicarJoins(IQueryable<CuentaDeInterlocutorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(x => x.Cuenta);
            return consulta;
        }

        public static GestorDeCuentasDeInterlocutor Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCuentasDeInterlocutor(contexto, mapeador);
        }

        protected override void DespuesDeMapearElRegistro(CuentaDeInterlocutorDto ciDto, CuentaDeInterlocutorDtm ciDtm, ParametrosDeNegocio opciones)
        {
            base.DespuesDeMapearElRegistro(ciDto, ciDtm, opciones);
            if (opciones.Insertando)
                ciDtm.IdCuenta =
                    ExtensorDeCuentasBancarias.Leer(Contexto, ciDto.Iban.Substring(0, 2), ciDto.Iban.Substring(2, 2), ciDto.Entidad, ciDto.Oficina, ciDto.DcCcc, ciDto.Numero).Id;
        }

        protected override void AntesDePersistir(CuentaDeInterlocutorDtm ci, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(ci, parametros);
            if (parametros.Insertando) ci.Activa = true;
            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                ci.IdCuenta = ((CuentaDeInterlocutorDtm)parametros.registroEnBd).IdCuenta;
                if (ci.IdArchivo is null)
                    ci.IdArchivo = ((CuentaDeInterlocutorDtm)parametros.registroEnBd).IdArchivo;

                if (ci.SeHaModificadoElCampo<bool>(x => x.Name == nameof(CuentaDeInterlocutorDtm.Activa), parametros))
                    ci.IdArchivo = ((CuentaDeInterlocutorDtm)parametros.registroEnBd).IdArchivo;

                ci.Cuenta(Contexto).Validar();

            }
        }

        protected override void DespuesDePersistir(CuentaDeInterlocutorDtm ci, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(ci, parametros);

            if (parametros.Insertando)
                ((InterlocutorDtm)ci.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido una cuenta al interlocutor", $"La cuenta {ci.Cuenta(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");


            if (parametros.Modificando && ci.SeHaModificadoElCampo<bool>(x => x.Name == nameof(CuentaDeInterlocutorDtm.Activa), parametros))
                ((InterlocutorDtm)ci.DetalleDe(Contexto)).CrearTraza(Contexto, $"{(ci.Activa ? "Activada " : "Desactivada")} la cuenta del interlocutor", $"La cuenta {ci.Cuenta(Contexto).NumeroIban} ha sido {(ci.Activa ? "activada" : "desactivada")} por el usuario {Contexto.DatosDeConexion.Login}");

            if (parametros.Eliminando)
                ((InterlocutorDtm)ci.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta del interlocutor", $"La cuenta {ci.Cuenta(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");

            if (ci.IdArchivo != default)
            {
                if (parametros.Insertando || (parametros.Modificando && !GestorDeVinculos.Existe(Contexto, enumNegocio.Interlocutor, enumNegocio.Archivos, ci.IdElemento, (int)ci.IdArchivo)))
                {
                    ci.AsignarNombreAlCertificado(Contexto, enumNegocio.Interlocutor, ci.IdElemento, parametros.Modificando ? (IUsaCuentaBancaria)parametros.registroEnBd: null);
                    ((InterlocutorDtm)ci.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha añadido el certificado de cuenta", $"El certificado de cuenta {ci.Cuenta(Contexto).NumeroIban} ha sido añadido por el usuario {Contexto.DatosDeConexion.Login}");
                }

                if (parametros.Eliminando && GestorDeVinculos.Existe(Contexto, enumNegocio.Interlocutor, enumNegocio.Archivos, ci.IdElemento, (int)ci.IdArchivo))
                {
                    ((InterlocutorDtm)ci.DetalleDe(Contexto)).CrearTraza(Contexto, "Se ha eliminado la cuenta del interlocutor", $"La cuenta {ci.Cuenta(Contexto).NumeroIban} ha sido eliminada por el usuario {Contexto.DatosDeConexion.Login}");
                    GestorDeVinculos.BorrarVinculo(Contexto, enumNegocio.Interlocutor, enumNegocio.Archivos, ci.IdElemento, (int)ci.IdArchivo, new Dictionary<string, object>());
                }
            }
        }


        protected override void DespuesDeMapearElElemento(CuentaDeInterlocutorDtm ci, CuentaDeInterlocutorDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(ci, elemento, parametros);

            elemento.ModoDeAcceso = ApiDePermisos.LeerModoDeAcceso(Contexto, enumNegocio.Interlocutor, ci.DetalleDe(Contexto));
            ci.Cuenta = ci.Cuenta(Contexto);
            elemento.Iban = ci.Cuenta.IsoPais + ci.Cuenta.DcIban;
            elemento.NombreArchivo = ci.IdArchivo is null ? ltrCuentasBancarias.SinCertificado : Contexto.SeleccionarPorId<ArchivoDtm>((int)ci.IdArchivo).Expresion;
            elemento.Banco = ci.Cuenta.Banco(Contexto, errorSiNoHay: false).Expresion(Contexto);
            elemento.Elemento = Contexto.SeleccionarPorId<InterlocutorDtm>(ci.IdElemento).Expresion;
        }

    }
}
