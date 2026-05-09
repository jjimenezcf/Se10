using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using ServicioDeDatos;
using GestorDeElementos;
using ServicioDeDatos.Callejero;
using ModeloDeDto.Callejero;
using Utilidades;
using System;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto;
using Gestor.Errores;
using ServicioDeDatos.Elemento;
using static GestoresDeNegocio.Callejero.GestorDeCpsDeUnaCalle;
using static GestoresDeNegocio.Callejero.GestorDeBarriosDeUnaCalle;
using static GestoresDeNegocio.Callejero.GestorDeZonasDeUnaCalle;
using GestorDeElementos.Extensores;

namespace GestoresDeNegocio.Callejero
{
    public class GestorDeCalles : GestorDeElementos<ContextoSe, CalleDtm, CalleDto>
    {
        public override enumNegocio Negocio => enumNegocio.Calle;

        public class MapearCalles : Profile
        {
            public MapearCalles()
            {
                CreateMap<CalleDtm, CalleDto>()
                    .ForMember(dto => dto.Municipio, dtm =>  dtm.MapFrom(dtm => dtm.Municipio == null ? null : dtm.Municipio.Nombre))
                    .ForMember(dto => dto.Provincia, dtm => dtm.MapFrom(dtm => dtm.Municipio == null ? null : $"({dtm.Municipio.Provincia.Codigo}) {dtm.Municipio.Provincia.Nombre}"))
                    .ForMember(dto => dto.Pais, dtm => dtm.MapFrom(dtm => dtm.Municipio == null ? null : $"({dtm.Municipio.Provincia.Pais.Codigo}) {dtm.Municipio.Provincia.Pais.Nombre}"))
                    .ForMember(dto => dto.TipoDeVia, dtm => dtm.MapFrom(dtm => dtm.TipoDeVia == null ? null : $"{dtm.TipoDeVia.Nombre}"))
                    .ForMember(dto => dto.IdProvincia, dtm => dtm.MapFrom(dtm => dtm.Municipio == null ? 0: dtm.Municipio.Provincia.Id))
                    .ForMember(dto => dto.IdPais, dtm => dtm.MapFrom(dtm => dtm.Municipio == null ? 0 : dtm.Municipio.Provincia.Pais.Id));

                CreateMap<CalleDto, CalleDtm>()
                .ForMember(dtm => dtm.Municipio, dto => dto.Ignore())
                .ForMember(dtm => dtm.TipoDeVia, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaCreacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.FechaModificacion, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaCrea, dto => dto.Ignore())
                .ForMember(dtm => dtm.IdUsuaModi, dto => dto.Ignore());

            }

        }

        public GestorDeCalles(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeCalles Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeCalles(contexto, mapeador); ;
        }

        public static CalleDtm LeerCallePorNombre(ContextoSe contexto, string nombrePais, string nombreProvincia, string nombreMunicipio, string nombreCalle, bool paraActualizar, bool errorSiNoHay = true, bool errorSiMasDeUno = true)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var filtros = new List<ClausulaDeFiltrado>();
            var filtro1 = new ClausulaDeFiltrado(ltrCalles.filtroPorPais, enumCriteriosDeFiltrado.igual, nombrePais);
            var filtro2 = new ClausulaDeFiltrado(ltrCalles.filtroPorProvincia, enumCriteriosDeFiltrado.igual, nombreProvincia);
            var filtro3 = new ClausulaDeFiltrado(ltrCalles.filtroPorMunicipio, enumCriteriosDeFiltrado.igual, nombreMunicipio);
            var filtro4 = new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.igual, nombreCalle);
            filtros.Add(filtro1);
            filtros.Add(filtro2);
            filtros.Add(filtro3);
            filtros.Add(filtro4);
            var p = new ParametrosDeNegocio(paraActualizar ? enumTipoOperacion.LeerConBloqueo : enumTipoOperacion.LeerSinBloqueo, aplicarJoin: false);
            //p.Parametros.Add(ltrJoinAudt.IncluirUsuarioDtm, false);
            List<CalleDtm> calles = gestor.LeerRegistros(0, -1, filtros, null, p);

            if (calles.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha localizado la calle con Iso2 del pais {nombrePais}, provincia {nombreProvincia} y municipio {nombreMunicipio}");
            if (calles.Count > 1 && errorSiMasDeUno)
                GestorDeErrores.Emitir($"Se han localizado más de un registro con Iso2 del pais {nombrePais}, provincia {nombreProvincia} y municipio {nombreMunicipio}");

            return calles.Count == 1 ? calles[0] : null;
        }

        protected override IQueryable<CalleDtm> AplicarJoins(IQueryable<CalleDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);

            if (parametros.HacerJoinCon(ltrCalles.JoinConMunicipio)) consulta = consulta.Include(p => p.Municipio);
            if (parametros.HacerJoinCon(ltrCalles.JoinConProvincia)) consulta = consulta.Include(p => p.Municipio.Provincia);
            if (parametros.HacerJoinCon(ltrCalles.JoinConPais)) consulta = consulta.Include(p => p.Municipio.Provincia.Pais);
            if (parametros.HacerJoinCon(ltrCalles.JoinConTipoDeVia)) consulta = consulta.Include(p => p.TipoDeVia);

            return consulta;
        }

        protected override IQueryable<CalleDtm> AplicarFiltros(IQueryable<CalleDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);

            var seleccionarParaDireccion = filtros.FirstOrDefault(x => x.Clausula == ltrCalles.SeleccionarParaDireccion);
            if (seleccionarParaDireccion != null)
            {
                seleccionarParaDireccion.Clausula = nameof(INombre.Nombre);
                parametros.Parametros[ltrCalles.SeleccionarParaDireccion] = true;
            }

            foreach (ClausulaDeFiltrado filtro in filtros)
            {
                if (filtro.Aplicado)
                    continue;

                if (filtro.Clausula.Equals(nameof(ProvinciaDtm.IdPais), StringComparison.CurrentCultureIgnoreCase))
                    filtro.Clausula = ltrCalles.filtroPorIdPais;

                if (filtro.Clausula.Equals(nameof(MunicipioDtm.IdProvincia), StringComparison.CurrentCultureIgnoreCase))
                    filtro.Clausula = ltrCalles.filtroPorIdProvincia;

                if (filtro.Clausula.Equals(ltrCalles.filtroPorBarrio, StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = consulta.Where(x => x.Barrios.Any(b => b.Barrio.Nombre.Contains(filtro.Valor)));
                    filtro.Aplicado = true;
                }

                if (filtro.Clausula.Equals(ltrCalles.filtroPorZona, StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = consulta.Where(x => x.Zonas.Any(b => b.Zona.Nombre.Contains(filtro.Valor)));
                    filtro.Aplicado = true;
                }

                //Selecciona las calles que no están relacionados con el barrio del municipio
                if (filtro.Clausula.Equals(nameof(CallesDeUnBarrioDto.IdBarrio), StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = consulta.Where(calle => Contexto.Set<BarrioDtm>().Any(barrio => barrio.IdMunicipio == calle.IdMunicipio) && !calle.Barrios.Any(b => b.IdBarrio == filtro.Valor.Entero()));
                }

                //Selecciona las calles que no están relacionados con la zona del municipio
                if (filtro.Clausula.Equals(nameof(CallesDeUnaZonaDto.IdZona), StringComparison.CurrentCultureIgnoreCase))
                {
                    consulta = consulta.Where(c => Contexto.Set<ZonaDtm>().Any(z => z.IdMunicipio == c.IdMunicipio) && !c.Zonas.Any(z => z.IdZona == filtro.Valor.Entero()));
                }
            }

            return consulta;


        }

        protected override void AntesDeMapearElRegistroParaInsertar(CalleDto calleDto, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(calleDto, opciones);

            if (calleDto.IdProvincia == 0 && calleDto.IdMunicipio > 0)
            {
                var municipio = Contexto.SeleccionarPorId<MunicipioDtm>(calleDto.IdMunicipio, aplicarJoin: true);
                calleDto.IdProvincia = municipio.IdProvincia;
                calleDto.IdPais = municipio.Provincia.IdPais;
            }

            if (!calleDto.Barrio.IsNullOrEmpty() && calleDto.IdBarrio.Entero() == 0)
            {
                calleDto.IdBarrio = new BarrioDtm { Nombre = calleDto.Barrio, IdMunicipio = calleDto.IdMunicipio }.InsertarSiNoExiste(Contexto, new List<string> { nameof(BarrioDtm.Nombre), nameof(BarrioDtm.IdMunicipio) }).Id;
            }

            if (!calleDto.Zona.IsNullOrEmpty() && calleDto.IdZona.Entero() == 0)
            {
                calleDto.IdZona = new ZonaDtm { Nombre = calleDto.Zona, IdMunicipio = calleDto.IdMunicipio }.InsertarSiNoExiste(Contexto, new List<string> { nameof(ZonaDtm.Nombre), nameof(ZonaDtm.IdMunicipio) }).Id;
            }

            if (!calleDto.CodigoPostal.IsNullOrEmpty() && calleDto.IdCp.Entero() == 0)
            {
                calleDto.IdCp = new CodigoPostalDtm { Codigo = calleDto.CodigoPostal }.InsertarSiNoExiste(Contexto, new List<string> { nameof(CodigoPostalDtm.Codigo) }).Id;
            }

            ValidarCodigoDeProvinciaConCpDeLaCalle(calleDto);
        }
        protected override void AntesDeMapearElRegistroParaModificar(CalleDto calleDto, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaModificar(calleDto, opciones);
            ValidarCodigoDeProvinciaConCpDeLaCalle(calleDto);
        }

        private void ValidarCodigoDeProvinciaConCpDeLaCalle(CalleDto calleDto)
        {
            if (calleDto.IdCp.HasValue && (int)calleDto.IdCp > 0 && calleDto.CodigoPostal.IsNullOrEmpty())
            {
                var cp = GestorDeCodigosPostales.LeerRegistroPorId(Contexto, (int)calleDto.IdCp);
                calleDto.CodigoPostal = cp.Codigo;
            }
            if (!calleDto.CodigoPostal.IsNullOrEmpty())
            {
                var provincia = GestorDeProvincias.LeerRegistroPorId(Contexto, calleDto.IdProvincia);
                if (!calleDto.CodigoPostal.Substring(0, 2).Equals(provincia.Codigo))
                    GestorDeErrores.Emitir($"El código postal asociado a esta calle debe empezar por {provincia.Codigo}, y uds ha indicado {calleDto.CodigoPostal}");
            }
        }


        /// <summary>
        /// Tras crear la calle si viene informado un CP, lo creo si hace falta y lo relaciono
        /// </summary>
        /// <param name="calleDto"></param>
        /// <param name="parametros"></param>
        protected override CalleDto DespuesDePersistirElementoDto(CalleDto calleDto, CalleDtm calleDtm, ParametrosDeNegocio parametros)
        {
            var nuevoDto = base.DespuesDePersistirElementoDto(calleDto, calleDtm, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar)
            {
                calleDto.Id = nuevoDto.Id;
                TrasCrearCalleRelacionarConCp(calleDto);
                TrasCrearCalleRelacionarConBarrio(calleDto);
                TrasCrearCalleRelacionarConZona(calleDto);
            }
            return calleDto;
        }

        private void TrasCrearCalleRelacionarConZona(CalleDto calleDto)
        {
            if (!calleDto.IdZona.MayorQueCero() && !calleDto.Zona.IsNullOrEmpty())
            {
                var gestorDeZona = GestorDeZonas.Gestor(Contexto, Contexto.Mapeador);
                var zonaDtm = gestorDeZona.LeerRegistro(nameof(ZonaDtm.Nombre), calleDto.Zona, errorSiNoHay: false, errorSiHayMasDeUno: true, conBloqueo: false, aplicarJoin: false); ;
                if (zonaDtm == null)
                {
                    zonaDtm = new ZonaDtm();
                    zonaDtm.Nombre = calleDto.Zona;
                    zonaDtm.IdMunicipio = calleDto.IdMunicipio;
                    gestorDeZona.PersistirRegistro(zonaDtm, new ParametrosDeNegocio(enumTipoOperacion.Insertar, false));
                }
                calleDto.IdZona = zonaDtm.Id;
            }

            if (calleDto.IdZona.MayorQueCero())
            {
                var gestorDeZonasDeUnaCalle = GestorDeZonasDeUnaCalle.Gestor(Contexto, Contexto.Mapeador);
                var relacion = new ZonasDeUnaCalleDto();
                relacion.IdCalle = calleDto.Id;
                relacion.idZona = (int)calleDto.IdZona;
                gestorDeZonasDeUnaCalle.CrearRelacion(relacion, new ParametrosDeNegocio(enumTipoOperacion.Insertar), errorSiYaExiste: false);
            }
        }

        private void TrasCrearCalleRelacionarConBarrio(CalleDto calleDto)
        {
            if (!calleDto.IdBarrio.MayorQueCero() && !calleDto.Barrio.IsNullOrEmpty())
            {
                var gestorDeBarrio = GestorDeBarrios.Gestor(Contexto, Contexto.Mapeador);
                var barrioDtm = gestorDeBarrio.LeerRegistro(nameof(BarrioDtm.Nombre), calleDto.Barrio, errorSiNoHay: false, errorSiHayMasDeUno: true, conBloqueo: false, aplicarJoin: false); ;
                if (barrioDtm == null)
                {
                    barrioDtm = new BarrioDtm();
                    barrioDtm.Nombre = calleDto.Barrio;
                    barrioDtm.IdMunicipio = calleDto.IdMunicipio;
                    gestorDeBarrio.PersistirRegistro(barrioDtm, new ParametrosDeNegocio(enumTipoOperacion.Insertar, false));
                }
                calleDto.IdBarrio = barrioDtm.Id;
            }

            if (calleDto.IdBarrio.MayorQueCero())
            {
                var gestorDeBarriosDeUnaCalle = GestorDeBarriosDeUnaCalle.Gestor(Contexto, Contexto.Mapeador);
                var relacion = new BarriosDeUnaCalleDto();
                relacion.IdCalle = calleDto.Id;
                relacion.idBarrio = (int)calleDto.IdBarrio;
                relacion.Mano = calleDto.ManoBarrio.IsNullOrEmpty() ? enumManoDeUnaCalle.Ambos.ToBd() : calleDto.ManoBarrio;
                relacion.Desde = calleDto.DesdeBarrio == null ? calleDto.DesdeBarrio : 0;
                relacion.Hasta = calleDto.HastaBarrio == null ? calleDto.HastaBarrio : 99999;
                gestorDeBarriosDeUnaCalle.CrearRelacion(relacion, new ParametrosDeNegocio(enumTipoOperacion.Insertar), errorSiYaExiste: false);
            }
        }

        private void TrasCrearCalleRelacionarConCp(CalleDto calleDto)
        {
            if (!calleDto.IdCp.MayorQueCero() && !calleDto.CodigoPostal.IsNullOrEmpty())
            {
                var gestorDeCp = GestorDeCodigosPostales.Gestor(Contexto, Contexto.Mapeador);
                var cpDtm = gestorDeCp.LeerRegistro(nameof(CodigoPostalDtm.Codigo), calleDto.CodigoPostal, errorSiNoHay: false, errorSiHayMasDeUno: true, conBloqueo: false, aplicarJoin: false); ;
                if (cpDtm == null)
                {
                    cpDtm = new CodigoPostalDtm();
                    cpDtm.Codigo = calleDto.CodigoPostal;
                    gestorDeCp.PersistirRegistro(cpDtm, new ParametrosDeNegocio(enumTipoOperacion.Insertar, false));
                }
                calleDto.IdCp = cpDtm.Id;
            }

            if (calleDto.IdCp.MayorQueCero())
            {
                var gestorDeCpsDeCalle = GestorDeCpsDeUnaCalle.Gestor(Contexto, Contexto.Mapeador);
                var relacion = new CpsDeUnaCalleDto();
                relacion.IdCalle = calleDto.Id;
                relacion.IdCp = (int)calleDto.IdCp;
                relacion.Mano = calleDto.ManoCp.IsNullOrEmpty() ? enumManoDeUnaCalle.Ambos.ToBd() : calleDto.ManoCp;
                relacion.Desde = calleDto.DesdeCp == null ? calleDto.DesdeCp : 0;
                relacion.Hasta = calleDto.HastaCp == null ? calleDto.HastaCp : 99999;
                gestorDeCpsDeCalle.CrearRelacion(relacion, new ParametrosDeNegocio(enumTipoOperacion.Insertar), errorSiYaExiste: false);
            }
        }

        //Todo: --> Reglas de negocio
        protected override void AntesDePersistir(CalleDtm calle, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(calle, parametros);

            if (parametros.Insertando || parametros.Modificando)
            {
                //Si la calle esta relacionada con CPs validar que esos Cps corresponden al municipio

            }

            if (parametros.Modificando && calle.SeHaModificadoElCampo<int>(x => x.Name == nameof(calle.IdMunicipio), parametros))
                GestorDeErrores.Emitir("El campo municipio de una calle no es modificable");

            if (parametros.Operacion == enumTipoOperacion.Eliminar)
            {
                Contexto.Set<BarriosDeUnaCalleDtm>().RemoveRange(Contexto.Set<BarriosDeUnaCalleDtm>().Where(x => x.IdCalle == calle.Id));
                Contexto.Set<CpsDeUnaCalleDtm>().RemoveRange(Contexto.Set<CpsDeUnaCalleDtm>().Where(x => x.IdCalle == calle.Id));
                Contexto.Set<ZonasDeUnaCalleDtm>().RemoveRange(Contexto.Set<ZonasDeUnaCalleDtm>().Where(x => x.IdCalle == calle.Id));
            }

        }

        //Todo: --> Reglas de negocio
        protected override void DespuesDePersistir(CalleDtm registro, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(registro, parametros);
            if (parametros.Operacion == enumTipoOperacion.Modificar || parametros.Operacion == enumTipoOperacion.Insertar)
            {

            }
        }


        protected override void DespuesDeMapearElElemento(CalleDtm calle, CalleDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(calle, elemento, parametros);
            List<ZonasDeUnaCalleDtm> zonas = ZonasDeUnaCalle(calle);
            var zonasConcatenadas = $"{(zonas.Count == 0 ? "" : "(" + string.Join(",", zonas.Select(x => x.Zona.Nombre)) + ")")}";
            elemento.Expresion = elemento.Expresion + (elemento.Expresion.Contains(zonasConcatenadas)? "" : zonasConcatenadas) ;


            if (parametros.Parametros.LeerValor(ltrCalles.SeleccionarParaDireccion, false) ||
                parametros.Peticion == enumPeticion.epLeerPorId)
            {
                var municipio = Contexto.SeleccionarPorId<MunicipioDtm>(calle.IdMunicipio);
                var provincia = Contexto.SeleccionarPorId<ProvinciaDtm>(municipio.IdProvincia);
                var pais = Contexto.SeleccionarPorId<PaisDtm>(provincia.IdPais);
                elemento.Pais = pais.Expresion;
                elemento.IdPais = pais.Id;
                elemento.Provincia = provincia.Expresion;
                elemento.IdProvincia = provincia.Id;
                elemento.Municipio = municipio.Expresion;

                List<CpsDeUnaCalleDtm> cps = CpsDeUnaCalle(calle);
                if (cps.Count == 1)
                {
                    elemento.CodigoPostal = cps[0].Cp.Codigo;
                    elemento.IdCp = cps[0].Cp.Id;
                }
                List<BarriosDeUnaCalleDtm> barrios = BarriosDeUnaCalle(calle);
                if (barrios.Count == 1)
                {
                    elemento.Barrio = barrios[0].Barrio.Expresion;
                    elemento.IdBarrio = barrios[0].Barrio.Id;
                }
                if (zonas.Count == 1)
                {
                    elemento.Zona = zonas[0].Zona.Expresion;
                    elemento.IdZona = zonas[0].Zona.Id;
                }
            }
        }

        private List<CpsDeUnaCalleDtm> CpsDeUnaCalle(CalleDtm calle)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Callejero_CpsDeUnaCalle);
            if (cache.ContainsKey(calle.Id.ToString())) return (List<CpsDeUnaCalleDtm>)cache[calle.Id.ToString()];
            cache[calle.Id.ToString()] = Contexto.SeleccionarTodos<CpsDeUnaCalleDtm>(filtros: new Dictionary<string, object>
                {
                    {nameof(CpsDeUnaCalleDtm.IdCalle), calle.Id }
                },
                aplicarJoin: true,
                parametros: new Dictionary<string, object>
                {
                        { ltrCpsDeUnaCalle.JoinConCalles, false },
                        { ltrCpsDeUnaCalle.JoinConCps, true }
                });
            return (List<CpsDeUnaCalleDtm>)cache[calle.Id.ToString()];
        }
        private List<BarriosDeUnaCalleDtm> BarriosDeUnaCalle(CalleDtm calle)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Callejero_BarriosDeUnaCalle);
            if (cache.ContainsKey(calle.Id.ToString())) return (List<BarriosDeUnaCalleDtm>)cache[calle.Id.ToString()];
            cache[calle.Id.ToString()] = Contexto.SeleccionarTodos<BarriosDeUnaCalleDtm>(filtros: new Dictionary<string, object>
                {
                    {nameof(BarriosDeUnaCalleDtm.IdCalle), calle.Id }
                },
            aplicarJoin: true,
            parametros: new Dictionary<string, object>
            {
                    { ltrBarriosDeUnaCalle.JoinConCalles, false },
                    { ltrBarriosDeUnaCalle.JoinConBarrios, true }
            });
            return (List<BarriosDeUnaCalleDtm>)cache[calle.Id.ToString()];
        }
        private List<ZonasDeUnaCalleDtm> ZonasDeUnaCalle(CalleDtm calle)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Callejero_ZonasDeUnaCalle);
            if (cache.ContainsKey(calle.Id.ToString())) return (List<ZonasDeUnaCalleDtm>)cache[calle.Id.ToString()];

            cache[calle.Id.ToString()] = Contexto.SeleccionarTodos<ZonasDeUnaCalleDtm>(filtros: new Dictionary<string, object>
                {
                    {nameof(ZonasDeUnaCalleDtm.IdCalle), calle.Id }
                },
            aplicarJoin: true,
            parametros: new Dictionary<string, object>
            {
                    { ltrZonasDeUnaCalle.JoinConCalles, false },
                    { ltrZonasDeUnaCalle.JoinConZonas, true }
            });
            return (List<ZonasDeUnaCalleDtm>)cache[calle.Id.ToString()];
        }
    }
}
