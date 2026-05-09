using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using Utilidades;
using Gestor.Errores;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.TrabajosSometidos
{

    public class GestorDeTrabajosSometido : GestorDeElementos<ContextoSe, TrabajoSometidoDtm, TrabajoSometidoDto>
    {

        public class MapeadorTrabajosSometidos : Profile
        {

            public MapeadorTrabajosSometidos()
            {
                CreateMap<TrabajoSometidoDtm, TrabajoSometidoDto>()
                .ForMember(dto => dto.Ejecutor, dtm => dtm.MapFrom(x => x.Ejecutor == null ? null : $"({x.Ejecutor.Login})- {x.Ejecutor.Nombre} {x.Ejecutor.Apellido}"))
                .ForMember(dto => dto.InformarA, dtm => dtm.MapFrom(x => x.InformarA == null ? null : x.InformarA.Nombre))
                .ForMember(dto => dto.Programa, dtm => dtm.MapFrom(x => x.EsDll ? $"{x.Clase}.{x.Metodo}" : $"{x.Esquema}.{x.Pa}"));


                CreateMap<TrabajoSometidoDto, TrabajoSometidoDtm>()
                .ForMember(dtm => dtm.Ejecutor, dto => dto.Ignore())
                .ForMember(dtm => dtm.InformarA, dto => dto.Ignore());
            }
        }

        public GestorDeTrabajosSometido(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeTrabajosSometido Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTrabajosSometido(contexto, mapeador);
        }

        internal static TrabajoSometidoDtm CrearObtener(ContextoSe contexto, string nombreTs, string dll, string clase, string metodo, bool comunicarFin)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            metodo = metodo.Replace("Someter", "");
            var filtroDll = new ClausulaDeFiltrado() { Clausula = nameof(TrabajoSometidoDtm.Dll), Criterio = enumCriteriosDeFiltrado.igual, Valor = dll };
            var filtroClase = new ClausulaDeFiltrado() { Clausula = nameof(TrabajoSometidoDtm.Clase), Criterio = enumCriteriosDeFiltrado.igual, Valor = clase };
            var filtroMetodo = new ClausulaDeFiltrado() { Clausula = nameof(TrabajoSometidoDtm.Metodo), Criterio = enumCriteriosDeFiltrado.igual, Valor = metodo };

            var filtros = new List<ClausulaDeFiltrado>() { filtroDll, filtroClase, filtroMetodo };

            var ts = gestor.LeerRegistroCacheadoPoAk(filtros, aplicarJoin: false, errorSiNoHay: false, errorSiHayMasDeUno: true);
            if (ts == null)
                ts = gestor.CrearTs(nombreTs, dll, clase, metodo, comunicarFin);

            return ts;
        }

        private TrabajoSometidoDtm CrearTs(string nombreTs, string dll, string clase, string metodo, bool comunicarFin)
        {
            var ts = new TrabajoSometidoDtm();
            ts.Nombre = nombreTs;
            ts.EsDll = true;
            ts.Dll = dll;
            ts.Clase = clase;
            ts.Metodo = metodo;
            ts.ComunicarError = true;
            ts.ComunicarFin = comunicarFin;

            return PersistirRegistro(ts, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }

        protected override IQueryable<TrabajoSometidoDtm> AplicarJoins(IQueryable<TrabajoSometidoDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.InformarA);
            registros = registros.Include(p => p.Ejecutor);
            return registros;
        }
        protected override void AntesDePersistir(TrabajoSometidoDtm registro, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(registro, parametros);

            if (!parametros.Operacion.Equals(enumTipoOperacion.Eliminar))
            {
                if (registro.Pa.IsNullOrEmpty() && registro.Esquema.IsNullOrEmpty() && registro.Dll.IsNullOrEmpty() && registro.Clase.IsNullOrEmpty() && registro.Metodo.IsNullOrEmpty())
                    GestorDeErrores.Emitir("Debe indicar una Dll o un PA");

                if (!registro.EsDll && (registro.Pa.IsNullOrEmpty() || registro.Esquema.IsNullOrEmpty()))
                    GestorDeErrores.Emitir("Debe indicar el PA y su Esquema");

                if (!registro.EsDll && (!registro.Dll.IsNullOrEmpty() || !registro.Clase.IsNullOrEmpty() || !registro.Metodo.IsNullOrEmpty()))
                    GestorDeErrores.Emitir("Si ha indicado que no es una dll los campos de ddl, clase y métodos han de ser nulos");

                if (registro.EsDll && (!registro.Pa.IsNullOrEmpty() || !registro.Esquema.IsNullOrEmpty()))
                    GestorDeErrores.Emitir("Si ha indicado que es una dll los campos de PA y esquema han de ser nulos");

                if (registro.EsDll && (registro.Dll.IsNullOrEmpty() || registro.Clase.IsNullOrEmpty() || registro.Metodo.IsNullOrEmpty()))
                    GestorDeErrores.Emitir("Si ha indicado que es una dll los campos de ddl, clase y métodos han de ser indicados");

                if (registro.EsDll)
                    ApiDeEnsamblados.ObtenerMetodoEstatico(registro.Dll, registro.Clase, registro.Metodo);
                else
                    Contexto.ValidarExistePa(registro.Pa, registro.Esquema);
            }
        }

    }
}



