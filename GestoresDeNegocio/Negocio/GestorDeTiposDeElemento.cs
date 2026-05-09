using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using Dapper;
using Utilidades;
using Gestor.Errores;
using GestorDeElementos;
using GestoresDeNegocio.Entorno;
using GestoresDeNegocio.Seguridad;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Negocio
{
    public class MapearTipoDeElemento : Profile
    {
        public MapearTipoDeElemento()
        {

        }

        protected IMappingExpression<TDto, TDtm> ReglasDeMapeoDelDtoAlDtm<TDto, TDtm>(IMappingExpression<TDto, TDtm> reglasDeMapeo)
        where TDtm : TipoDeElementoDtm
        where TDto : TipoDeElementoDto
        {
            reglasDeMapeo = reglasDeMapeo
                   .ForMember(dtm => dtm.PermisoDeGestor, dto => dto.Ignore())
                   .ForMember(dtm => dtm.PermisoDeConsultor, dto => dto.Ignore())
                   .ForMember(dtm => dtm.PermisoDeAdministrador, dto => dto.Ignore());

            if (ApiDeInterfaceDtm.ImplementaPermisosDeInterventor(typeof(TDtm)))
                reglasDeMapeo = reglasDeMapeo
               .ForMember(dtm => ((IPermisoDeInterventor)dtm).PermisoDeInterventor, dto => dto.Ignore());

            return reglasDeMapeo;
        }

        protected IMappingExpression<TDtm, TDto> ReglasDeMapeoDelDtmAlDto<TDtm, TDto>(IMappingExpression<TDtm, TDto> rn)
        where TDtm : TipoDeElementoDtm
        where TDto : TipoDeElementoDto
        {
            rn = rn
             .ForMember(dto => dto.PermisoDeGestor, dtm => dtm.MapFrom(x => x.PermisoDeGestor == null ? null : x.PermisoDeGestor.Nombre))
             .ForMember(dto => dto.PermisoDeAdministrador, dtm => dtm.MapFrom(x => x.PermisoDeAdministrador == null ? null : x.PermisoDeAdministrador.Nombre))
             .ForMember(dto => dto.PermisoDeConsultor, dtm => dtm.MapFrom(x => x.PermisoDeConsultor == null ? null : x.PermisoDeConsultor.Nombre));

            if (ApiDeInterfaceDtm.ImplementaPermisosDeInterventor(typeof(TDtm)))
                rn = rn
               .ForMember(dto => ((IPermisoDeInterventorDto)dto).PermisoDeInterventor, dtm => dtm.MapFrom(x => ((IPermisoDeInterventor)x).PermisoDeInterventor == null ? null : ((IPermisoDeInterventor)x).PermisoDeInterventor.Nombre));

            return rn;
        }
    }
    public class MapearNodoTipo : Profile
    {
        public MapearNodoTipo()
        {
            CreateMap<TipoDeElementoDtm, TipoDeElementoDto>()
            .ForMember(dto => dto.Negocio, dtm => dtm.Ignore());
        }
    }

    public class GestorDeTiposDeElemento<TContexto, TTipoDtm, TTipoDto> : GestorDeElementos<TContexto, TTipoDtm, TTipoDto>, IGestorDeTipos
        where TContexto : ContextoSe
        where TTipoDtm : TipoDeElementoDtm
        where TTipoDto : TipoDeElementoDto
    {
        enumNegocio _negocioDelTipo;
        public override enumNegocio Negocio => _negocioDelTipo;


        public class ltrDeUnTipoDeElemento
        {
            public static readonly string mostrarJerarquia = nameof(mostrarJerarquia);
            public static string filtroPorTiposNoActivo => TipoDeElementoSql.Filtro.TiposNoActivos;
            public static string filtroPorUsuario => TipoDeElementoSql.Filtro.PorUsuario;
            public static string filtroPorPuesto => TipoDeElementoSql.Filtro.PorPuesto;
            public static string filtroPorRol => TipoDeElementoSql.Filtro.PorRol;
            public static string filtroPorModoDeAcceso => TipoDeElementoSql.Filtro.ModoDeAcceso;

        }


        public GestorDeTiposDeElemento(TContexto contexto, IMapper mapeador, enumNegocio negocio)
        : base(contexto, mapeador)
        {
            _negocioDelTipo = negocio;
        }

        public static GestorDeTiposDeElemento<TContexto, TTipoDtm, TTipoDto> Gestor(TContexto contexto, IMapper mapeador, enumNegocio negocio)
        {

            var cache = ServicioDeCaches.Obtener(nameof(GestorDeElementos));
            var indice = $"{negocio}-{typeof(GestorDeTiposDeElemento<TContexto, TTipoDtm, TTipoDto>).Name}";
            if (!cache.ContainsKey(indice))
                cache[indice] = new GestorDeTiposDeElemento<TContexto, TTipoDtm, TTipoDto>(contexto, mapeador, negocio);
            return (GestorDeTiposDeElemento<TContexto, TTipoDtm, TTipoDto>)cache[indice];

        }

        internal static JerarquiaDto LeerJerarquia(TContexto contexto, enumNegocio negocio, int? idPadre, string filtrosJson)
        {
            var filtros = filtrosJson.ToDiccionario();
            var gestor =
                Gestor(contexto, contexto.Mapeador, negocio);
            List<NodoDtm> tiposLeidosDtm = gestor.LeerJerarquiaDeTipos(idPadre, filtros);

            var mostrarJerarquia = (bool)filtros[ltrDeUnTipoDeElemento.mostrarJerarquia];
            return mostrarJerarquia
                ? ApiDeJerarquias.EstructurarJerarquica(negocio, tiposLeidosDtm, typeof(TTipoDto))
                : ApiDeJerarquias.EstructuraPlana(negocio, tiposLeidosDtm, typeof(TTipoDto));
        }

        JerarquiaDto IGestorDeTipos.LeerJerarquia(enumNegocio negocio, int? idPadre, string filtrosJson)
        {
            var filtros = filtrosJson.ToDiccionario();
            List<NodoDtm> tiposLeidosDtm = LeerJerarquiaDeTipos(idPadre, filtros);

            var mostrarJerarquia = (bool)filtros[ltrDeUnTipoDeElemento.mostrarJerarquia];
            return mostrarJerarquia
                ? ApiDeJerarquias.EstructurarJerarquica(negocio, tiposLeidosDtm, typeof(TTipoDto))
                : ApiDeJerarquias.EstructuraPlana(negocio, tiposLeidosDtm, typeof(TTipoDto));
        }

       
        protected override void AntesDePersistir(TTipoDtm tipoDeElemento, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(tipoDeElemento, parametros);

            tipoDeElemento.TipoDtm = NombreDeLaClaseDtm;
            tipoDeElemento.TipoDto = NombreDeLaClaseDto;

            if (tipoDeElemento.IdPadre == 0) tipoDeElemento.IdPadre = null;

            if (!parametros.Eliminando && tipoDeElemento.Sigla.IsNullOrEmpty())
                GestorDeErrores.Emitir("Debe indicar al menos una sigla para la referencia");

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                if (LeerRegistroCacheado(nameof(tipoDeElemento.Nombre), tipoDeElemento.Nombre, false, false, false) != null)
                    GestorDeErrores.Emitir($"Ya existe un tipo con el nombre '{tipoDeElemento.Nombre}'");
                tipoDeElemento.Activo = true;
            }

            if (!tipoDeElemento.Mascara.IsNullOrEmpty() && tipoDeElemento.Marcador.IsNullOrEmpty())
                GestorDeErrores.Emitir($"Si se define una máscara, ha de definirse el marcador que explica la expresión regular");

            AnalizarSeguridad(tipoDeElemento, parametros);
            ValidarLibro(tipoDeElemento, parametros);
            ValidarNoHayElementos(tipoDeElemento, parametros);

            if (parametros.Operacion == enumTipoOperacion.Modificar) ValidarRecursividad(tipoDeElemento);

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                var filtro = new ClausulaDeFiltrado { Clausula = nameof(TipoDeElementoDtm.IdPadre), Criterio = enumCriteriosDeFiltrado.igual, Valor = tipoDeElemento.Id.ToString() };
                var hijos = LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro }, null, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, false));
                if (hijos.Count > 0)
                    GestorDeErrores.Emitir($"El tipo {tipoDeElemento.Nombre} tiene {(hijos.Count == 1 ? "un subtipo asociado" : $"{hijos.Count} subtipos asociados")}");
            }


            if (ApiDeInterfaceDtm.ImplementaPermisosDeInterventor(typeof(TTipoDtm)))
            {
                if (parametros.Operacion == enumTipoOperacion.Insertar)
                {
                    ((IPermisoDeInterventor)tipoDeElemento).IdPermisoInterventor = GestorDePermisos.CrearObtener(Contexto, Negocio, tipoDeElemento.Nombre, enumClaseDePermiso.Tipo, enumModoDeAccesoDeDatos.Interventor).Id;
                }

                if (parametros.Operacion == enumTipoOperacion.Modificar && (!parametros.Parametros.ContainsKey(NegociosDeSe.ActualizarSeguridad) || (bool)parametros.Parametros[NegociosDeSe.ActualizarSeguridad]))
                {
                    var permisosBd = Contexto.Set<PermisoDtm>();
                    var interventor = permisosBd.LeerCacheadoPorId(((IPermisoDeInterventor)parametros.registroEnBd).IdPermisoInterventor);
                    ((IPermisoDeInterventor)tipoDeElemento).IdPermisoInterventor = GestorDePermisos.ModificarPermisoDeDatos(Contexto, Negocio, interventor, tipoDeElemento.Nombre, enumClaseDePermiso.Tipo, enumModoDeAccesoDeDatos.Interventor).Id;
                }
            }
        }

        private void ValidarRecursividad(TTipoDtm tipoDeElemento)
        {
            var listaDeIds = new List<int> { tipoDeElemento.Id };
            var idPadre = tipoDeElemento.IdPadre;
            while (idPadre != null)
            {
                if (listaDeIds.Contains(idPadre.Entero()))
                    GestorDeErrores.Emitir($"No puede modificar el tipo {tipoDeElemento.Nombre}, ya que el padre indicado genera recursividad");

                var padre = LeerRegistroPorId((int)idPadre, usarLaCache: true, traqueado: false, conBloqueo: false, aplicarJoin: false);
                listaDeIds.Add(padre.Id);
                idPadre = padre.IdPadre;
            }
        }

        protected virtual int ValidarNoHayElementos(TTipoDtm registro, ParametrosDeNegocio parametros)
        {
            if (parametros.Operacion == enumTipoOperacion.Insertar) return 0;

            var filtro = new ClausulaDeFiltrado(nameof(IUsaTipo.IdTipo), enumCriteriosDeFiltrado.igual, ((IRegistro)registro).Id.ToString());
            var cantidad = Negocio.CrearGestor(Contexto).Contar(new List<ClausulaDeFiltrado> { filtro }, parametros);
            if (cantidad > 0)
            {
                if (parametros.Operacion == enumTipoOperacion.Modificar)
                {
                    if (((TTipoDtm)parametros.registroEnBd).ClaseDeLibro != registro.ClaseDeLibro
                        || ((TTipoDtm)parametros.registroEnBd).Sigla != registro.Sigla)
                        GestorDeErrores.Emitir($"No se puede modificar el tipo '{registro.Nombre}' ya que tiene elementos asociados");
                }
                else
                    GestorDeErrores.Emitir($"No se puede eliminar el tipo '{registro.Nombre}' ya que tiene elementos asociados");
            }
            return cantidad;
        }

        private void ValidarLibro(TTipoDtm tipoDeElemento, ParametrosDeNegocio parametros)
        {
            if (tipoDeElemento.ClaseDeLibro != enumClaseDeLibro.POR_CG)
            {
                var filtro = new ClausulaDeFiltrado(nameof(TipoDeElementoDtm.Sigla), enumCriteriosDeFiltrado.igual, tipoDeElemento.Sigla);
                var tipos = LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });

                if (tipos.Count > 0)
                {
                    foreach (var tipo in tipos)
                    {
                        if (tipo.ClaseDeLibro == enumClaseDeLibro.POR_CG)
                            continue;

                        if ((parametros.Modificando || parametros.Eliminando) && tipo.Id == tipoDeElemento.Id)
                            continue;

                        if (tipoDeElemento.Sigla.Equals(tipo.Sigla))
                            GestorDeErrores.Emitir($"El tipo {tipo.Nombre} usa la misma sigla, si se se comparte sigla la clase de libro sólo puede ser por Centro Gestor");
                    }
                }
            }

        }

        private void AnalizarSeguridad(TTipoDtm registro, ParametrosDeNegocio parametros)
        {

            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                registro.IdPermisoDeAdministrador = GestorDePermisos.CrearObtener(Contexto, Negocio, registro.Nombre, enumClaseDePermiso.Tipo, enumModoDeAccesoDeDatos.Administrador).Id;
                registro.IdPermisoDeGestor = GestorDePermisos.CrearObtener(Contexto, Negocio, registro.Nombre, enumClaseDePermiso.Tipo, enumModoDeAccesoDeDatos.Gestor).Id;
                registro.IdPermisoDeConsultor = GestorDePermisos.CrearObtener(Contexto, Negocio, registro.Nombre, enumClaseDePermiso.Tipo, enumModoDeAccesoDeDatos.Consultor).Id;
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar && (!parametros.Parametros.ContainsKey(NegociosDeSe.ActualizarSeguridad) || (bool)parametros.Parametros[NegociosDeSe.ActualizarSeguridad]))
            {
                registro.IdPermisoDeAdministrador = GestorDePermisos.ModificarPermisoDeDatos(Contexto, Negocio, ((ITipoDtm)parametros.registroEnBd).IdPermisoDeAdministrador, registro.Nombre, enumClaseDePermiso.Tipo, enumModoDeAccesoDeDatos.Administrador).Id;
                registro.IdPermisoDeGestor = GestorDePermisos.ModificarPermisoDeDatos(Contexto, Negocio, ((ITipoDtm)parametros.registroEnBd).IdPermisoDeGestor, registro.Nombre, enumClaseDePermiso.Tipo, enumModoDeAccesoDeDatos.Gestor).Id;
                registro.IdPermisoDeConsultor = GestorDePermisos.ModificarPermisoDeDatos(Contexto, Negocio, ((ITipoDtm)parametros.registroEnBd).IdPermisoDeConsultor, registro.Nombre, enumClaseDePermiso.Tipo, enumModoDeAccesoDeDatos.Consultor).Id;
            }

        }

        protected override void DespuesDePersistir(TTipoDtm tipoDeElemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(tipoDeElemento, parametros);
            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                GestorDePermisos.EliminarRegistroPorId(Contexto, tipoDeElemento.IdPermisoDeAdministrador, parametros: parametros.Parametros);
                GestorDePermisos.EliminarRegistroPorId(Contexto, tipoDeElemento.IdPermisoDeConsultor, parametros: parametros.Parametros);
                GestorDePermisos.EliminarRegistroPorId(Contexto, tipoDeElemento.IdPermisoDeGestor, parametros: parametros.Parametros);
            }

            if (parametros.Operacion == enumTipoOperacion.Modificar)
            {
                if (tipoDeElemento.Activo && !((TipoDeElementoDtm)parametros.registroEnBd).Activo)
                    PropagarFlag(tipoDeElemento, nameof(TipoDeElementoDtm.Activo), propagarALosHijos: false, propagarALosPadres: true, tipoDeElemento.Activo);

                if (!tipoDeElemento.Activo && ((TipoDeElementoDtm)parametros.registroEnBd).Activo)
                    PropagarFlag(tipoDeElemento, nameof(TipoDeElementoDtm.Activo), propagarALosHijos: true, propagarALosPadres: false, tipoDeElemento.Activo);

                if (tipoDeElemento.EditarTrasCrear != ((TipoDeElementoDtm)parametros.registroEnBd).EditarTrasCrear)
                    PropagarFlag(tipoDeElemento, nameof(TipoDeElementoDtm.EditarTrasCrear), propagarALosHijos: true, propagarALosPadres: false, tipoDeElemento.EditarTrasCrear);
            }
        }

        protected void PropagarFlag(TTipoDtm tipoDeElemento, string flag, bool propagarALosHijos, bool propagarALosPadres, bool valor)
        {
            if (propagarALosPadres && tipoDeElemento.IdPadre.Entero() > 0)
            {
                var padre = LeerRegistroPorId((int)tipoDeElemento.IdPadre, usarLaCache: true, traqueado: false, conBloqueo: false, aplicarJoin: false);

                if ((bool)padre.LeerPropiedad(flag) != valor)
                {
                    padre.EscribirPropiedad(flag, valor);
                    PersistirRegistro(padre, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
                }
                else PropagarFlag(padre, flag, false, true, valor);
            }


            if (propagarALosHijos)
            {
                var filtro = new ClausulaDeFiltrado { Clausula = nameof(TipoDeElementoDtm.IdPadre), Criterio = enumCriteriosDeFiltrado.igual, Valor = tipoDeElemento.Id.ToString() };
                var hijos = LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro }, null, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, false));
                foreach (var hijo in hijos)
                {
                    if ((bool)hijo.LeerPropiedad(flag) != valor)
                    {
                        hijo.EscribirPropiedad(flag, valor);
                        PersistirRegistro(hijo, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
                    }
                    else PropagarFlag(hijo, flag, true, false, valor);
                }
            }
        }

        protected override void EliminarCaches(TTipoDtm tipoDeElemento, ParametrosDeNegocio parametros)
        {
            ServicioDeCaches.EliminarTodas();
        }

        protected override void DespuesDeMapearElElemento(TTipoDtm tipoDtm, TTipoDto tipoDto, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(tipoDtm, tipoDto, parametros);
            tipoDto.Negocio = Negocio.Plural();
            tipoDto.IdNegocio = Negocio.IdNegocio();
            var padre = tipoDtm.Valor<TTipoDtm>(x => x.Name == nameof(tipoDto.Padre));
            tipoDto.Padre = padre == null ? "" : padre.Valor<string>(x => x.Name == nameof(INombre.Nombre));
            tipoDto.HayClases = Negocio.UsaClasesDelTipo() && Negocio.HayClasesDelTipo(Contexto, tipoDtm.Id);
        }

        public List<NodoDtm> LeerJerarquiaDeTipos(int? idPadre, Dictionary<string, object> filtros)
        {
            var tabla = ApiDeRegistroDtm.NombreDeTabla(typeof(TTipoDtm));
            var esquema = ApiDeRegistroDtm.EsquemaDeTabla(typeof(TTipoDtm));
            var sentenciaSql = TipoDeElementoSql.JerarquiaDeTipos.Replace("[Esquema].[Tabla]", $"{esquema}.{tabla}");
            var parametrosSql = new Dictionary<string, object>();
            sentenciaSql = TipoDeElementoSql.AplicarFiltros(sentenciaSql, idPadre, filtros, parametrosSql);

            var consulta = new ConsultaSql<NodoDtm>(Contexto, sentenciaSql);
            var registros = consulta.LanzarConsulta(new DynamicParameters(parametrosSql));
            return registros;
        }

        protected override IQueryable<TTipoDtm> AplicarJoins(IQueryable<TTipoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);

            consulta = consulta.Include(p => p.PermisoDeAdministrador);
            consulta = consulta.Include(p => p.PermisoDeConsultor);
            consulta = consulta.Include(p => p.PermisoDeGestor);

            if (ApiDeInterfaceDtm.ImplementaPermisosDeInterventor(typeof(TTipoDtm)))
                consulta = consulta.Include(p => ((IPermisoDeInterventor)p).PermisoDeInterventor);

            if (typeof(TTipoDtm).BaseType.Equals(typeof(TipoConFlujoDtm)))
                consulta = consulta.Include(p => ((IUsaEstado)p).Estado);

            return consulta;
        }

        protected override IQueryable<TTipoDtm> AplicarFiltros(IQueryable<TTipoDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            var filtro = filtros.FirstOrDefault(f => f.Clausula.ToLower() == ltrParametrosNeg.SoloEnAlta.ToLower() && f.Valor.EsTrue());
            if (filtro != null)
            {
                filtro.Aplicado = true;
                consulta = consulta.Where(x => x.Activo == true);

                if (parametros.CargarListaDinamica)
                    consulta = consulta.Where(x => x.PermiteCrear == true);

            }

            return FiltrarLosTiposSeleccionables(consulta, filtros);
        }

        private IQueryable<TTipoDtm> FiltrarLosTiposSeleccionables(IQueryable<TTipoDtm> consulta, List<ClausulaDeFiltrado> filtros)
        {
            var filtroDeSeguridad = filtros.Where(filtro => filtro.Clausula.ToLower().Equals(nameof(NegociosDeUnCgDtm.Negocio).ToLower())).FirstOrDefault();
            if (filtroDeSeguridad == null)
                return consulta;

            if (!Contexto.DatosDeConexion.EsAdministrador)
            {
                var negocio = enumNegocio.No_Definido;
                var partes = filtroDeSeguridad.Valor.Split(Simbolos.PuntoComa);
                if (partes.Length != 2)
                    GestorDeErrores.Emitir("No es valído el filtro a aplicar por tipo, necestita indicar el negocio y el modo solicitado");
                if (Enum.TryParse(partes[0], out negocio))
                {
                    var idNegocio = NegociosDeSe.IdNegocio(negocio);
                    enumModoDeAccesoDeDatos modo = enumModoDeAccesoDeDatos.SinPermiso;
                    if (Enum.TryParse(partes[1], out modo))
                    {
                        if (modo == enumModoDeAccesoDeDatos.Consultor)
                            consulta = consulta.Where(x => Contexto.Set<PermisosPorTipoDtm>().Any(y => y.IdTipo.Equals(x.Id)
                                                         && y.IdNegocio.Equals(idNegocio)
                                                         && y.IdUsuario.Equals(Contexto.DatosDeConexion.IdUsuario)
                                                         && x.IdPermisoDeConsultor == y.IdPermiso));
                        else
                            consulta = consulta.Where(x => Contexto.Set<PermisosPorTipoDtm>().Any(y => y.IdTipo.Equals(x.Id)
                                                         && y.IdNegocio.Equals(idNegocio)
                                                         && y.IdUsuario.Equals(Contexto.DatosDeConexion.IdUsuario)
                                                         && x.IdPermisoDeGestor == y.IdPermiso));
                    }
                    else
                    {
                        consulta = consulta.Where(x => false);
                    }
                }
                else
                {
                    consulta = consulta.Where(x => false);
                }
            }

            filtroDeSeguridad.Aplicado = true;

            return consulta;
        }

        protected override void ValidarPermisosDePersistencia(TTipoDtm registro, ParametrosDeNegocio parametros)
        {
            base.ValidarPermisosDePersistencia(registro, parametros);
            if (parametros.EsUnaPeticion && !Contexto.SePuedeParametrizar())
            {
                GestorDeErrores.Emitir($"La información de un tipo sólo la puede modificar un administrador marcado como parametrizador");
            }
        }

        public static void GenerarSeguridadParaElUsuario(ContextoSe contexto, int idUsuario, Action<string> logger = null)
        {
            PermisosPorTipoSql.QuitarPermisos(contexto, idUsuario, calculado: true);
            var tiposAccedidos = ApiDePermisos.PermisosCalculadosDe(contexto, idUsuario, enumClaseDePermiso.Tipo);
            foreach (var permiso in tiposAccedidos)
            {
                var negocio = enumNegocio.Parse<enumNegocio>(permiso.Nombre.Split(".")[0], true);
                var gTipos = NegociosDeSe.CrearGestor(contexto, (enumNegocio)negocio);
                var tipo = negocio.Tipos(contexto).FirstOrDefault(x => x.IdPermisoDeConsultor == permiso.IdPermiso || x.IdPermisoDeGestor == permiso.IdPermiso || x.IdPermisoDeAdministrador == permiso.IdPermiso);
                if (tipo == null)
                {
                    tipo = negocio.Tipos(contexto).First(x => ((IPermisoDeInterventor)x).IdPermisoInterventor == permiso.IdPermiso);
                }
                var tipoPermiso = enumModoDeAccesoDeDatos.Parse<enumModoDeAccesoDeDatos>(permiso.Tipo, true);
                OtorgarPermisoDeAUnUsuarioDeUnTipoDeUnNegocio(contexto, negocio.IdNegocio(), gTipos.Metadatos.TipoDtm, tipo, tipoPermiso, idUsuario, calculado: true);
            }

            PermisosPorTipoSql.QuitarPermisos(contexto, idUsuario, calculado: false);
            var permisosDirectos = ApiDePermisos.PermisosDirectosDe(contexto, idUsuario, enumClaseDePermiso.Tipo);
            foreach (var permiso in permisosDirectos)
            {
                var negocio = enumNegocio.Parse<enumNegocio>(permiso.Nombre.Split(".")[0], true);
                var gTipos = NegociosDeSe.CrearGestor(contexto, (enumNegocio)negocio);
                var tipo = negocio.Tipos(contexto).FirstOrDefault(x => x.IdPermisoDeConsultor == permiso.IdPermiso || x.IdPermisoDeGestor == permiso.IdPermiso || x.IdPermisoDeAdministrador == permiso.IdPermiso);
                if (tipo == null)
                {
                    tipo = negocio.Tipos(contexto).First(x => ((IPermisoDeInterventor)x).IdPermisoInterventor == permiso.IdPermiso);
                }
                var tipoPermiso = enumModoDeAccesoDeDatos.Parse<enumModoDeAccesoDeDatos>(permiso.Tipo, true);
                OtorgarPermisoDeAUnUsuarioDeUnTipoDeUnNegocio(contexto, negocio.IdNegocio(), gTipos.Metadatos.TipoDtm, tipo, tipoPermiso, idUsuario, calculado: false);
            }
        }

        public static void GenerarSeguridad(ContextoSe contexto, Action<string> logger = null)
        {
            PermisosPorTipoSql.EliminarTodos(contexto);
            foreach (var negocio in Enum.GetValues(typeof(enumNegocio)))
            {
                if (!NegociosDeSe.UsaTipo((enumNegocio)negocio))
                    continue;

                var gTipos = NegociosDeSe.CrearGestor(contexto, (enumNegocio)negocio);
                var negocioDtm = NegociosDeSe.LeerNegocioPorEnumerado((enumNegocio)negocio);
                dynamic tipos = gTipos.GestorDeTipos.LeerRegistros(0, -1, new List<ClausulaDeFiltrado>(), false);

                logger?.Invoke($"Procesando los tipos del negocio de {(enumNegocio)negocio}");
                OtorgarSeguridadPorTipos((TContexto)contexto, negocioDtm.Id, gTipos.Metadatos.TipoDtm, tipos, enumModoDeAccesoDeDatos.Administrador);
                if (gTipos.Metadatos.TipoDtm.ImplementaPermisosDeInterventor())
                    OtorgarSeguridadPorTipos((ContextoSe)contexto, negocioDtm.Id, gTipos.Metadatos.TipoDtm, tipos, enumModoDeAccesoDeDatos.Interventor);
                OtorgarSeguridadPorTipos((ContextoSe)contexto, negocioDtm.Id, gTipos.Metadatos.TipoDtm, tipos, enumModoDeAccesoDeDatos.Gestor);
                OtorgarSeguridadPorTipos((ContextoSe)contexto, negocioDtm.Id, gTipos.Metadatos.TipoDtm, tipos, enumModoDeAccesoDeDatos.Consultor);
            }
        }

        private static void OtorgarSeguridadPorTipos(ContextoSe contexto, int idNegocio, Type tipoDeElementoDtm, dynamic tipos, enumModoDeAccesoDeDatos tipoPermiso)
        {
            var filtro = new ClausulaDeFiltrado(nameof(PermisosDeUnRolDtm.IdPermiso), enumCriteriosDeFiltrado.igual);
            foreach (var tipo in tipos)
                OtorgarSeguridadPorTipo(contexto, idNegocio, tipoDeElementoDtm, filtro, tipo, tipoPermiso);
        }

        private static void OtorgarSeguridadPorTipo(ContextoSe contexto, int idNegocio, Type tipoDeElementoDtm, ClausulaDeFiltrado filtro, dynamic tipo, enumModoDeAccesoDeDatos tipoPermiso)
        {
            filtro.Valor = tipoPermiso.Equals(enumModoDeAccesoDeDatos.Administrador)
                ? tipo.IdPermisoDeAdministrador.ToString()
                : tipoPermiso.Equals(enumModoDeAccesoDeDatos.Interventor)
                ? ((IPermisoDeInterventor)tipo).IdPermisoInterventor.ToString()
                : tipoPermiso.Equals(enumModoDeAccesoDeDatos.Gestor)
                ? tipo.IdPermisoDeGestor.ToString()
                : tipo.IdPermisoDeConsultor.ToString();
            var usuariosConElPermiso = GestorDePermisosDeUnUsuario.Gestor(contexto, contexto.Mapeador).LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro });
            foreach (var usuario in usuariosConElPermiso)
            {
                OtorgarPermisoDeAUnUsuarioDeUnTipoDeUnNegocio(contexto, idNegocio, tipoDeElementoDtm, tipo, tipoPermiso, usuario.IdUsuario, calculado: true);
            }

            ServicioDeCaches.EliminarCache(CacheDe.Permisos_PorTipo);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TiposConGestion);
            ServicioDeCaches.EliminarCache(CacheDe.Permisos_TiposConsultor);
        }

        private static void OtorgarPermisoDeAUnUsuarioDeUnTipoDeUnNegocio(ContextoSe contexto, int idNegocio, Type tipoDeElementoDtm, dynamic tipo, enumModoDeAccesoDeDatos tipoPermiso, int idUsuario, bool calculado)
        {
            if (tipoPermiso.Equals(enumModoDeAccesoDeDatos.Administrador))
            {
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, tipo.IdPermisoDeAdministrador, calculado);
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, tipo.IdPermisoDeGestor, calculado);
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, tipo.IdPermisoDeConsultor, calculado);
                if (tipoDeElementoDtm.ImplementaPermisosDeInterventor())
                    PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, ((IPermisoDeInterventor)tipo).IdPermisoInterventor, calculado);
            }
            else if (tipoDeElementoDtm.ImplementaPermisosDeInterventor() && tipoPermiso.Equals(enumModoDeAccesoDeDatos.Interventor))
            {
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, ((IPermisoDeInterventor)tipo).IdPermisoInterventor, calculado);
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, tipo.IdPermisoDeGestor, calculado);
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, tipo.IdPermisoDeConsultor, calculado);
            }
            else if (tipoPermiso.Equals(enumModoDeAccesoDeDatos.Gestor))
            {
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, tipo.IdPermisoDeGestor, calculado);
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, tipo.IdPermisoDeConsultor, calculado);
            }
            else
            {
                PermisosPorTipoSql.Otorgar(contexto, idNegocio, tipoDeElementoDtm, tipo.Id, idUsuario, tipo.IdPermisoDeConsultor, calculado);
            }
        }

        protected override void ObtenerModoDeAccesoAlElementoQueSeDevuelve(TTipoDtm registro, TTipoDto elemento, ParametrosDeNegocio parametros)
        {
            elemento.ModoDeAcceso = Contexto.DatosDeConexion.EsAdministrador
            ? enumModoDeAccesoDeDatos.Administrador
            : enumModoDeAccesoDeDatos.Consultor;
        }

    }
}
