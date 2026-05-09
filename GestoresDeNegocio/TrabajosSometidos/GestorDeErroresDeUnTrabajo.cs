using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using System;
using Gestor.Errores;
using Utilidades;
using ServicioDeDatos.Elemento;

namespace GestoresDeNegocio.TrabajosSometidos
{

    public class GestorDeErroresDeUnTrabajo : GestorDeElementos<ContextoSe, ErrorDeUnTrabajoDtm, ErrorDeUnTrabajoDto>
    {

        public class MapeadorErroresDeUnTrabajo : Profile
        {
            public MapeadorErroresDeUnTrabajo()
            {
                CreateMap<ErrorDeUnTrabajoDtm, ErrorDeUnTrabajoDto>()
                .ForMember(dto => dto.TrabajoDeUsuario, dtm => dtm.MapFrom(x => $"({x.TrabajoDeUsuario.Sometedor.Login})- {x.TrabajoDeUsuario.Trabajo.Nombre}"));


                CreateMap<ErrorDeUnTrabajoDto, ErrorDeUnTrabajoDtm>()
                .ForMember(dtm => dtm.TrabajoDeUsuario, dto => dto.Ignore());
            }
        }

        public GestorDeErroresDeUnTrabajo(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeErroresDeUnTrabajo Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeErroresDeUnTrabajo(contexto, mapeador);
        }


        public ErrorDeUnTrabajoDtm CrearError(TrabajoDeUsuarioDtm tu, string error, string detalle)
        {
            var e = new ErrorDeUnTrabajoDtm();
            e.IdTrabajoDeUsuario = tu.Id;
            e.Error = error;
            e.Detalle = detalle;
            e.Fecha = DateTime.Now;
            return PersistirRegistro(e, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }

        protected override void AntesDePersistir(ErrorDeUnTrabajoDtm error, ParametrosDeNegocio parametros)
        {
            error.Error = error.Error.Left(IDominio.Longitud(IDominio.VARCHAR_2000)-1);
            base.AntesDePersistir(error, parametros);
        }

        protected override IQueryable<ErrorDeUnTrabajoDtm> AplicarJoins(IQueryable<ErrorDeUnTrabajoDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.TrabajoDeUsuario);
            registros = registros.Include(p => p.TrabajoDeUsuario.Sometedor);
            registros = registros.Include(p => p.TrabajoDeUsuario.Trabajo);
            return registros;
        }

        internal static void AnotarError(ContextoSe contextoTu, TrabajoDeUsuarioDtm tu, Exception e)
        {
            var gestorEt = Gestor(contextoTu, contextoTu.Mapeador);     
            gestorEt.CrearError(tu, e.Message, e.MensajeCompleto(mostrarPila: true));
        }
        internal static void CrearError(ContextoSe contextoTu, TrabajoDeUsuarioDtm tu, string error, string detalle)
        {
            var gestorEt = Gestor(contextoTu, contextoTu.Mapeador);
            gestorEt.CrearError(tu, error, detalle);
        }

        internal static void EliminarErrores(ContextoSe contexto, int id)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            gestor.Contexto.Database.ExecuteSqlInterpolated($"delete from TRABAJO.ERROR where id_trabajo_usuario = {id}");
        }
    }
}

