using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Gestor.Errores;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using Utilidades;

namespace GestorDeElementos
{
    public class GestorDeTrazas : GestorDeElementos<ContextoSe, TrazaDtm, TrazaDto>
    {
        public string _Tabla { get; }

        private enumNegocio _Negocio;

        public override enumNegocio Negocio => _Negocio;

        private IGestor gestor => NegociosDeSe.CrearGestor(Contexto, Negocio);


        public class ltrTrazas
        {
        }

        public class MapearTrazas : Profile
        {
            public MapearTrazas()
            {
                CreateMap<TrazaDtm, TrazaDto>();
                CreateMap<TrazaDto, TrazaDtm>();
            }
        }

        public GestorDeTrazas(ContextoSe contexto, enumNegocio negocio)
        : base(contexto, contexto.Mapeador)
        {
            _Tabla = ApiDeElementoDtm.TablaDeTrazas(negocio.TipoDtm());
            _Negocio = negocio;
            Contexto = contexto;
        }

        public static GestorDeTrazas Gestor(ContextoSe contexto, enumNegocio negocio) => new GestorDeTrazas(contexto, negocio);

        public TrazaDtm LeerRegistroPorId(int id, bool emitirError = true)
            => TrazaDtmSql.LeerPorId(Contexto, _Tabla, Negocio.TipoDtm(), id, emitirError);

        public IEnumerable<TrazaDtm> LeerRegistros(int idElemento, int posicion, int cantidad)
            => TrazaDtmSql.TrazasDeUnElemento(Contexto, _Tabla, Negocio.TipoDtm(), idElemento, posicion, cantidad);

        public int ContarRegistros(int idElemento)
            => TrazaDtmSql.ContarRegistros(Contexto, _Tabla, idElemento);


        public override TrazaDtm LeerRegistroPorId(int id, bool usarLaCache, bool traqueado, bool conBloqueo, bool aplicarJoin, bool errorSiNoHay, Dictionary<string, object> parametros = null)
        {
            return LeerRegistroPorId(id);
        }

        public new IEnumerable<TrazaDto> LeerElementos(int posicion, int cantidad, List<ClausulaDeFiltrado> filtros, List<ClausulaDeOrdenacion> orden, Dictionary<string, object> opcionesDeMapeo)
        {
            int idElemento = 0;
            foreach (var f in filtros.Where(f => f.Clausula == nameof(TrazaDtm.IdElemento)))
            {
                idElemento = f.Valor.Entero();
            }

            var registros = LeerRegistros(idElemento, posicion, cantidad);
            return MapearElementos(registros.ToList());
        }

        protected override void AntesDePersistir(TrazaDtm registro, ParametrosDeNegocio parametros)
        {
            registro.Nombre = registro.Nombre.Left(IDominio.Longitud(IDominio.VARCHAR_250) - 1);
            registro.Descripcion = registro.Descripcion.Left(IDominio.Longitud(IDominio.VARCHAR_2000) - 1);
            base.AntesDePersistir(registro, parametros);

            if (parametros.ValidarPermisosDePersistencia && parametros.Operacion != enumTipoOperacion.Insertar)
                GestorDeErrores.Emitir("No se pueden modificar ni borrar las trazas de un elemento.");
        }

        protected override void Persistir(TrazaDtm registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Insertando)
            registro.Id = TrazaDtmSql.Insertar(Contexto, _Tabla, registro.IdElemento, registro.Nombre, registro.Descripcion).Id;
            else if (parametros.Eliminando)
                TrazaDtmSql.Eliminar(Contexto, _Tabla, registro.Id);
            else
                GestorDeErrores.Emitir("No se ha implementado modificar las trazas de un elemento.");
        }

        protected override void DespuesDeMapearElElemento(TrazaDtm registro, TrazaDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(registro, elemento, parametros);
            elemento.IdNegocio = NegociosDeSe.IdNegocio(Negocio);
            elemento.Negocio = NegociosDeSe.ToNombre(Negocio);
        }
        internal static void Baja(ContextoSe contexto, enumNegocio negocio, IRegistro registro)
        {
            if (negocio == enumNegocio.No_Definido) return;
            if (!NegociosDeSe.NegocioDeUnDtm(registro.GetType()).EsUnNegocio()) return;
            if (!negocio.UsaTrazas()) return;

            var traza = new TrazaDtm
            {
                IdElemento = registro.Id,
                Nombre = "Elemento dado de baja",
                Descripcion = $@"El elemento se ha dado de baja"
            };
            Gestor(contexto, negocio).PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }
        internal static void Alta(ContextoSe contexto, enumNegocio negocio, IRegistro registro)
        {
            if (negocio == enumNegocio.No_Definido) return;
            if (!NegociosDeSe.NegocioDeUnDtm(registro.GetType()).EsUnNegocio()) return;
            if (!negocio.UsaTrazas()) return;

            var traza = new TrazaDtm
            {
                IdElemento = registro.Id,
                Nombre = "Elemento dado de alta",
                Descripcion = $@"El elemento se ha dado de alta"
            };
            Gestor(contexto, negocio).PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }

        internal static void ObservacionModificada(ContextoSe contexto, enumNegocio negocio, ObservacionDtm nueva, ObservacionDtm anterior)
        {
            if (anterior.Descripcion != nueva.Descripcion)
            {
                var traza = new TrazaDtm
                {
                    IdElemento = nueva.IdElemento,
                    Nombre = "Observación modificada",
                    Descripcion = $@"Asunto: {anterior.Nombre}{Environment.NewLine}Descripción: {anterior.Descripcion}"
                };
                Gestor(contexto, negocio).PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            }
        }
        internal static void Bloquear(ContextoSe contexto, enumNegocio negocio, IRegistro registro)
        {
            if (negocio == enumNegocio.No_Definido) return;
            if (!NegociosDeSe.NegocioDeUnDtm(registro.GetType()).EsUnNegocio()) return;
            if (!negocio.UsaTrazas()) return;

            var traza = new TrazaDtm
            {
                IdElemento = registro.Id,
                Nombre = ltrDeTrazas.Bloqueada,
                Descripcion = $@"El elemento ha sido bloqueado"
            };
            Gestor(contexto, negocio).PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }
        internal static void Desbloquear(ContextoSe contexto, enumNegocio negocio, IRegistro registro)
        {
            if (negocio == enumNegocio.No_Definido) return;
            if (!NegociosDeSe.NegocioDeUnDtm(registro.GetType()).EsUnNegocio()) return;
            if (!negocio.UsaTrazas()) return;

            var traza = new TrazaDtm
            {
                IdElemento = registro.Id,
                Nombre = ltrDeTrazas.Desbloqueada,
                Descripcion = $@"El elemento ha sido desbloqueado"
            };
            Gestor(contexto, negocio).PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(TrazaDtm registro, TrazaDto elemento, ParametrosDeNegocio parametros)
        {
            elemento.ModoDeAcceso = enumModoDeAccesoDeDatos.Consultor;
            return;
        }
    }

}
