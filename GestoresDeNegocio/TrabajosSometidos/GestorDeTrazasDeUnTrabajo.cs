using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.TrabajosSometidos;
using ModeloDeDto.TrabajosSometidos;
using System;

namespace GestoresDeNegocio.TrabajosSometidos
{

    public class GestorDeTrazasDeUnTrabajo : GestorDeElementos<ContextoSe, TrazaDeUnTrabajoDtm, TrazaDeUnTrabajoDto>
    {

        public class MapeadorTrazasDeUnTrabajo : Profile
        {
            public MapeadorTrazasDeUnTrabajo()
            {
                CreateMap<TrazaDeUnTrabajoDtm, TrazaDeUnTrabajoDto>()
                .ForMember(dto => dto.TrabajoDeUsuario, dtm => dtm.MapFrom(x => $"({x.TrabajoDeUsuario.Sometedor.Login})- {x.TrabajoDeUsuario.Trabajo.Nombre}"));


                CreateMap<TrazaDeUnTrabajoDto, TrazaDeUnTrabajoDtm>()
                .ForMember(dtm => dtm.TrabajoDeUsuario, dto => dto.Ignore());
            }
        }

        public GestorDeTrazasDeUnTrabajo(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {

        }

        public static GestorDeTrazasDeUnTrabajo Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeTrazasDeUnTrabajo(contexto, mapeador);
        }


        public TrazaDeUnTrabajoDtm CrearTraza(TrabajoDeUsuarioDtm tu, string Traza)
        {
            var t = new TrazaDeUnTrabajoDtm();
            t.IdTrabajoDeUsuario = tu.Id;
            t.Traza = Traza;
            t.Fecha = DateTime.Now;
            return PersistirRegistro(t, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }


        protected override IQueryable<TrazaDeUnTrabajoDtm> AplicarJoins(IQueryable<TrazaDeUnTrabajoDtm> registros, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            registros = base.AplicarJoins(registros, filtros, parametros);
            registros = registros.Include(p => p.TrabajoDeUsuario);
            registros = registros.Include(p => p.TrabajoDeUsuario.Sometedor);
            registros = registros.Include(p => p.TrabajoDeUsuario.Trabajo);
            return registros;
        }
        internal static TrazaDeUnTrabajoDtm AnotarTraza(ContextoSe contextoTu, TrabajoDeUsuarioDtm tu, string traza)
        {
            var gestorTraza = Gestor(contextoTu, contextoTu.Mapeador);
            return gestorTraza.CrearTraza(tu, traza);
        }

        internal static TrazaDeUnTrabajoDtm ActualizarTraza(ContextoSe contextoTu, TrazaDeUnTrabajoDtm trazaDtm)
        {
            var gestorTraza = Gestor(contextoTu, contextoTu.Mapeador);
            return gestorTraza.PersistirRegistro(trazaDtm, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
        }

        internal static void EliminarTrazas(ContextoSe contexto, int id)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            gestor.Contexto.Database.ExecuteSqlInterpolated($"delete from trabajo.traza where id_trabajo_usuario = {id}");
        }

        internal static TrazaDeUnTrabajoDtm LeerUltimaTraza(ContextoSe contextoTu, int id)
        {
            var gestorTraza = Gestor(contextoTu, contextoTu.Mapeador);
            ClausulaDeFiltrado clausulaDeFiltrado = new ClausulaDeFiltrado() { Clausula = nameof(TrazaDeUnTrabajoDtm.IdTrabajoDeUsuario), Criterio = enumCriteriosDeFiltrado.igual, Valor = id.ToString() };
            var traza = gestorTraza.LeerUltimoRegistro(new List<ClausulaDeFiltrado>() { clausulaDeFiltrado }, new ParametrosDeNegocio(enumTipoOperacion.LeerConBloqueo));
            return traza;
        }
    }
}

