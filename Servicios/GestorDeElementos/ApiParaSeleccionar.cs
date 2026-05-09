using Gestor.Errores;
using GestorDeElementos.Extensores;
using ModeloDeDto;
using ServicioDeDatos;
using ServicioDeDatos.Elemento;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos
{
    public static class ApiParaSeleccionar
    {
        public static dynamic SeleccionarPorId(this enumNegocio negocio, ContextoSe contexto, int id, bool aplicarJoin = false, bool usarLaCache = true, Dictionary<string, object> parametros = null)
        =>
        negocio.CrearGestor(contexto, negocio.TipoDtm()).LeerRegistroPorId(id, aplicarJoin, usarLaCache, parametros);

        public static RegistroConNombreDtm RegistroPorId(this enumNegocio negocio, ContextoSe contexto, int id, bool aplicarJoin = false, bool usarLaCache = true, bool errorSiNoHay = true)
        {
            var tipo = negocio.TipoDtm();
            if (tipo.HeredaDe(typeof(ElementoDtm)))
                return (RegistroConNombreDtm)negocio.ElementoPorId(contexto, id, aplicarJoin, usarLaCache, errorSiNoHay);

            if (!tipo.ImplementaNombre())
                throw new Exception($"No se puede usar el método '{nameof(RegistroPorId)}' para el tipo '{tipo.Name}' ya que no implementa la interface '{nameof(INombre)}'");

            var parametros = new Dictionary<string, object>();
            parametros[ltrParametrosNeg.ErrorSiNoLoHay] = errorSiNoHay;

            return (RegistroConNombreDtm)negocio.CrearGestor(contexto, tipo).LeerRegistroPorId(id, aplicarJoin, usarLaCache);
        }

        public static T SeleccionarPorId<T>(this ContextoSe contexto, int id, bool aplicarJoin = false, bool errorSiNoHay = true, bool usarLaCache = true, Dictionary<string, object> parametros = null, bool aplicarPermisos = false)
        where T : RegistroDtm
        {
            if (!aplicarPermisos)
            {
                if (parametros == null) parametros = new Dictionary<string, object>();
                if (!parametros.ContainsKey(ltrParametrosNeg.ValidarPermisosDeConsulta))
                    parametros.Add(ltrParametrosNeg.ValidarPermisosDeConsulta, false);
            }
            return errorSiNoHay
            ? (T)contexto.CrearGestorDeUnDtm<T>().LeerRegistroPorId(id, aplicarJoin, usarLaCache, parametros: parametros)
            : contexto.SeleccionarPorPropiedad<T>(nameof(IRegistro.Id), id.ToString(), errorSiNoHay, true, aplicarJoin, parametros: parametros);
        }

        public static T SeleccionarElemento<T>(this ContextoSe contexto, int id, bool aplicarJoin = false, bool errorSiNoHay = true, bool usarLaCache = true, Dictionary<string, object> parametros = null)
        where T : IElementoDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(typeof(T));
            parametros = parametros == null ? new Dictionary<string, object>() : parametros;
            parametros[ltrParametrosNeg.ErrorSiNoLoHay] = errorSiNoHay;
            return (T)negocio.CrearGestor(contexto, typeof(T)).LeerRegistroPorId(id, aplicarJoin, usarLaCache, parametros);
        }

        public static T SeleccionarPorAk<T>(this ContextoSe contexto, Dictionary<string, object> filtrosPorAk, bool errorSiNoHay = true, bool aplicarJoin = false, bool incluirBaja = true)
        where T : RegistroDtm
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(typeof(T));
            var filtros = filtrosPorAk.ToFiltros();

            if (negocio.UsaBaja() && incluirBaja)
            {
                filtros.Add(new ClausulaDeFiltrado(ltrParametrosNeg.IncluirBajas, enumCriteriosDeFiltrado.igual, true.ToString()));
            }

            List<T> seleccionados;
            if (negocio == enumNegocio.No_Definido)
            {
                var gestor = NegociosDeSe.CrearGestorDeUnDtm<T>(contexto);
                seleccionados = (List<T>)gestor.LeerRegistros(0, 2, filtros, aplicarJoin);
            }
            else seleccionados = negocio.SeleccionarPorFiltro<T>(contexto, filtros, aplicarJoin, new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });

            var mensaje = $"del negocio {negocio} para los criterios '{filtrosPorAk.Keys.ToList().ToString(",")}'";
            return seleccionados.DevolverSeleccionado(mensaje, errorSiNoHay, errorSiMasDeuno: true);
        }


        public static T SeleccionarPorFk<T>(this ContextoSe contexto, string propiedad, int valor, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false, bool incluirBajas = true, bool usarLaCache = true)
        where T : RegistroDtm
        {
            var p = incluirBajas ? new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } } : new Dictionary<string, object>();
            return contexto.SeleccionarPorPropiedad<T>(propiedad, valor.ToString(), errorSiNoHay, errorSiMasDeuno, aplicarJoin, usarLaCache, p);
        }

        public static T SeleccionarPorNombre<T>(this ContextoSe contexto, string valor, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false, bool usarLaCache = true, Dictionary<string, object> parametros = null)
        where T : RegistroConNombreDtm
        =>
        SeleccionarPorPropiedad<T>(contexto, nameof(INombre.Nombre), valor, errorSiNoHay, errorSiMasDeuno, aplicarJoin, usarLaCache, parametros);

        public static T SeleccionarActivosPorPropiedad<T>(this ContextoSe contexto, string propiedad, object valor, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false, bool usarLaCache = true, Dictionary<string, object> parametros = null)
        where T : RegistroDtm, IElementoDeProcesoDtm
        {
            if (parametros == null) parametros = new Dictionary<string, object>();

            parametros[ltrParametrosNeg.ExcluirCancelados] = true;
            if (!parametros.ContieneClave(ltrParametrosNeg.ExcluirTerminados)) parametros[ltrParametrosNeg.ExcluirTerminados] = false;

            return SeleccionarPorPropiedad<T>(contexto, nameof(INombre.Nombre), valor, errorSiNoHay, errorSiMasDeuno, aplicarJoin, usarLaCache, parametros);
        }

        public static T SeleccionarPorPropiedad<T>(this ContextoSe contexto, string propiedad, object valor, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false, bool usarLaCache = true, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            if (parametros == null) parametros = new Dictionary<string, object>();
            if (!parametros.ContieneClave(ltrParametrosNeg.ValidarPermisosDeConsulta))
            {
                parametros[ltrParametrosNeg.ValidarPermisosDeConsulta] = false;
            }
            if (!parametros.ContieneClave(ltrParametrosNeg.UsarLaCache))
            {
                parametros[ltrParametrosNeg.UsarLaCache] = usarLaCache;
            }

            var esElementoTipado = typeof(T).ImplementaElementoTipado();

            var indice = typeof(T).FullName + "-" + propiedad + "-" + valor + "-" + errorSiNoHay + "-" + errorSiMasDeuno + "-" + aplicarJoin + "-" + parametros.ToJson();
            var cache = ServicioDeCaches.Obtener(CacheDe.Ent_SeleccionarPorPropiedad);
            if (parametros.LeerValor(ltrParametrosNeg.UsarLaCache, true))
            {
                if (esElementoTipado)
                {
                    if (cache.ContainsKey(indice))
                        return (T)cache[indice];
                }
            }

            var gestor = NegociosDeSe.CrearGestorDeUnDtm<T>(contexto);
            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(propiedad, enumCriteriosDeFiltrado.igual, valor) };
            var seleccionados = (List<T>)gestor.LeerRegistros(0, -1, filtros, aplicarJoin, parametros);
            var mensaje = $"para la clase '{typeof(T).FullName}' filtrando por la propiedad '{propiedad}' con valor '{valor}'";
            var elemento = seleccionados.DevolverSeleccionado(mensaje, errorSiNoHay, errorSiMasDeuno);

            if (!esElementoTipado) return elemento;
            cache[indice] = elemento;
            return (T)cache[indice];
        }

        public static T SeleccionarPorPropiedades<T>(this ContextoSe contexto, List<string> propiedades, object valor, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            var gestor = NegociosDeSe.CrearGestorDeUnDtm<T>(contexto);
            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(string.Join(Simbolos.Or, propiedades), enumCriteriosDeFiltrado.porDiferentesPropiedades, valor) };
            var seleccionados = (List<T>)gestor.LeerRegistros(0, -1, filtros, aplicarJoin, parametros);
            var mensaje = $"para la clase {typeof(T).FullName} filtrando por las propiedades '{string.Join(Simbolos.Coma, propiedades)}' con valor '{valor}'";
            return seleccionados.DevolverSeleccionado(mensaje, errorSiNoHay, errorSiMasDeuno);
        }

        public static T SeleccionarUltimo<T>(this ContextoSe contexto, string propiedad, List<ClausulaDeFiltrado> filtro = null, ParametrosDeNegocio parametros = null)
        where T : RegistroDtm
        {
            var gestor = NegociosDeSe.CrearGestorDeUnDtm<T>(contexto);
            var orden = new ClausulaDeOrdenacion(propiedad, ModoDeOrdenancion.descendente);
            var seleccionados = (List<T>)gestor.LeerRegistros(0, 1, filtro, new List<ClausulaDeOrdenacion> { orden }, parametros);
            return seleccionados.Count == 0 ? null : seleccionados[0];
        }

        public static bool Existen(this enumNegocio negocio, ContextoSe contexto, string propiedad, object valor)
        {
            var gestor = negocio.CrearGestor(contexto);
            return gestor.Existen(new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(propiedad, enumCriteriosDeFiltrado.igual, valor) });
        }

        public static T SeleccionarPorPropiedad<T>(this enumNegocio negocio, ContextoSe contexto, string propiedad, string valor, bool errorSiNoHay = true, bool errorSiMasDeuno = true, bool aplicarJoin = false)
        where T : RegistroDtm
        {
            var seleccionados = negocio.SeleccionarPorFiltro<T>(contexto,
                new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(propiedad, enumCriteriosDeFiltrado.igual, valor) },
                aplicarJoin,
                parametros: null);
            var mensaje = $"del negocio {negocio} para el criterio '{propiedad}' con valor '{valor}'";
            return seleccionados.DevolverSeleccionado(mensaje, errorSiNoHay, errorSiMasDeuno);
        }

        public static List<tDtm> Registros<tDto, tDtm>(this ContextoSe contexto, List<ClausulaDeFiltrado> filtros, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where tDto : ElementoDto
        where tDtm : RegistroDtm
        =>
        (List<tDtm>)NegociosDeSe.CrearGestor(contexto, typeof(tDtm), typeof(tDto)).LeerRegistros(0, -1, filtros, aplicarJoin, parametros: parametros);



        public static int Contar<tDto, tDtm>(this ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        where tDto : ElementoDto
        where tDtm : RegistroDtm
        =>
        NegociosDeSe.CrearGestor(contexto, typeof(tDtm), typeof(tDto)).Contar(filtros);


        public static int Contar<T>(this ContextoSe contexto, string propiedad, object valor, enumCriteriosDeFiltrado criterio)
        where T : RegistroDtm
        =>
        contexto.Contar<T>(new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado { Clausula = propiedad, Criterio = criterio, Valor = valor.ToString() } });

        public static int Contar<tDtm>(this ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        where tDtm : RegistroDtm
        =>
        contexto.CrearGestorDeUnDtm<tDtm>().Contar(filtros);

        public static bool Existe<tDtm>(this ContextoSe contexto, int id, Dictionary<string, object> parametros = null)
        where tDtm : RegistroDtm
        =>
        contexto.CrearGestorDeUnDtm<tDtm>().Existen(new List<ClausulaDeFiltrado> { { new ClausulaDeFiltrado {
           Clausula = nameof(IRegistro.Id),
           Criterio = enumCriteriosDeFiltrado.igual,
           Valor = id.ToString()} }}, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, parametros));

        public static bool Existen<tDtm>(this ContextoSe contexto, string propiedad, int valor, Dictionary<string, object> parametros = null)
        where tDtm : RegistroDtm
        {
            if (parametros == null)
            {
                parametros = new Dictionary<string, object>();
                if (typeof(tDtm).ImplementaElementoDeUnProceso())
                {
                    parametros[ltrParametrosNeg.ExcluirCancelados] = true;
                    parametros[ltrParametrosNeg.ExcluirTerminados] = false;
                }
                else if (typeof(tDtm).ImplementaUsaBaja())
                {
                    parametros[ltrParametrosNeg.IncluirBajas] = false;
                }
            }

            return contexto.CrearGestorDeUnDtm<tDtm>().Existen(new List<ClausulaDeFiltrado> { { new ClausulaDeFiltrado {
                       Clausula = propiedad,
                       Criterio = enumCriteriosDeFiltrado.igual,
                       Valor = valor.ToString()} }}, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo, parametros));
        }

        public static bool Existen<tDtm>(this ContextoSe contexto, List<ClausulaDeFiltrado> filtros)
        where tDtm : RegistroDtm
        =>
        contexto.CrearGestorDeUnDtm<tDtm>().Existen(filtros);

        public static tDto ElementoPorNombre<tDto, tDtm>(this ContextoSe contexto, string nombre, bool aplicarJoin = false, bool errorSiNoHay = true, bool errorSiHayMasDeUno = true)
        where tDto : ElementoDto
        where tDtm : RegistroDtm
        {
            var filtros = new List<ClausulaDeFiltrado> { new ClausulaDeFiltrado(nameof(INombre.Nombre), enumCriteriosDeFiltrado.igual, nombre) };
            var elementos = contexto.Elementos<tDto, tDtm>(filtros, aplicarJoin);

            if (errorSiHayMasDeUno && elementos.Count > 1)
                GestorDeErrores.Emitir($"Hay más de un elemento con el mismo nombre {nombre}");

            if (errorSiNoHay && elementos.Count == 0)
                GestorDeErrores.Emitir($"No hay ningún elemento con el nombre {nombre}");

            return elementos[0];
        }


        public static List<tDto> Elementos<tDto, tDtm>(this ContextoSe contexto, List<ClausulaDeFiltrado> filtros, bool aplicarJoin = false)
        where tDto : ElementoDto
        where tDtm : RegistroDtm
        =>
        (List<tDto>)NegociosDeSe.CrearGestor(contexto, typeof(tDtm), typeof(tDto)).LeerElementos(0, -1, filtros, null, new Dictionary<string, object> { { ltrParametrosNeg.AplicarJoin, aplicarJoin } });

        public static List<T> SeleccionarTodos<T>(this ContextoSe contexto, string propiedad, object valor, enumNegocio negocio = enumNegocio.No_Definido, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        =>
        contexto.SeleccionarTodos<T>(new Dictionary<string, object> { { propiedad, valor } }, negocio, aplicarJoin, parametros);


        public static List<T> SeleccionarTodos<T>(this ContextoSe contexto, Dictionary<string, object> filtros, enumNegocio negocio = enumNegocio.No_Definido, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            filtros = filtros == null ? new Dictionary<string, object>() : filtros;

            var clausulas = filtros.ToFiltros();
            if (negocio == enumNegocio.No_Definido)
                negocio = NegociosDeSe.NegocioDeUnDtm(typeof(T));

            return negocio.SeleccionarPorFiltro<T>(contexto, clausulas, aplicarJoin, parametros);
        }

        public static List<T> SeleccionarTodos<T>(this ContextoSe contexto, List<ClausulaDeFiltrado> clausulas, enumNegocio negocio = enumNegocio.No_Definido, bool aplicarJoin = false, Dictionary<string, object> parametros = null)
        where T : RegistroDtm
        {
            if (negocio == enumNegocio.No_Definido)
                negocio = NegociosDeSe.NegocioDeUnDtm(typeof(T));

            return negocio.SeleccionarPorFiltro<T>(contexto, clausulas, aplicarJoin, parametros);
        }

        public static T DevolverSeleccionado<T>(this List<T> seleccionados, string mensaje, bool errorSiNoHay, bool errorSiMasDeuno)
        where T : IRegistro
        {
            if (seleccionados.Count == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No hay ningún elemento {mensaje}");

            if (seleccionados.Count > 1 && errorSiMasDeuno)
                GestorDeErrores.Emitir($"Hay más de un elemento {mensaje}");

            return seleccionados.Count == 0 ? default(T) : seleccionados[0];
        }


    }
}
