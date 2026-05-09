using Gestor.Errores;
using ServicioDeDatos.Elemento;
using ServicioDeDatos;
using System;
using System.Linq;
using Utilidades;
using ServicioDeDatos.Callejero;
using System.Collections.Generic;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Ventas;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.Presupuesto;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using ModeloDeDto.Negocio;
using Newtonsoft.Json.Linq;
using ModeloDeDto.Terceros;
using ServicioDeDatos.Logistica;

namespace GestorDeElementos.Extensores
{
    public static class ExtensorDeDirecciones
    {

        public static IQueryable<DireccionDtm> Direcciones(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Expediente:
                    return contexto.Set<DireccionDeUnExpedienteDtm>();
                case enumNegocio.FacturaRecibida:
                    return contexto.Set<DireccionDeUnaFacturaRecDtm>();
                case enumNegocio.Pedido:
                    return contexto.Set<DireccionDeUnPedidoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<DireccionDeUnContratoDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<DireccionDeUnPleitoDtm>();
                case enumNegocio.Presupuesto:
                    return contexto.Set<DireccionDeUnPresupuestoDtm>();
                case enumNegocio.Registro:
                    return contexto.Set<DireccionDeUnRegistroEsDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<DireccionDeUnaTareaDtm>();
                case enumNegocio.Abogado:
                    return contexto.Set<DireccionDeAbogadoDtm>();
                case enumNegocio.Cliente:
                    return contexto.Set<DireccionDeUnClienteDtm>();
                case enumNegocio.Interlocutor:
                    return contexto.Set<DireccionDeInterlocutorDtm>();
                case enumNegocio.Persona:
                    return contexto.Set<DireccionDeUnaPersonaDtm>();
                case enumNegocio.Procurador:
                    return contexto.Set<DireccionDeProcuradorDtm>();
                case enumNegocio.Proveedor:
                    return contexto.Set<DireccionDeUnProveedorDtm>();
                case enumNegocio.Sociedad:
                    return contexto.Set<DireccionDeLaSociedadDtm>();
                case enumNegocio.Trabajador:
                    return contexto.Set<DireccionDeTrabajadorDtm>();
                case enumNegocio.FacturaEmitida:
                    return contexto.Set<DireccionDeUnaFacturaEmtDtm>();
                case enumNegocio.ParteDeTrabajo:
                    return contexto.Set<DireccionDeUnParteTrDtm>();
                case enumNegocio.PlanificacionDeVenta:
                    return contexto.Set<DireccionDeUnaPlanificacionDeVentaDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los archivos anexados al negocio: {negocio}");
        }

        public static IDireccionDtm InsertarDireccion(this IDireccionDtm direccion, ContextoSe contexto, Dictionary<string, object> parametros = null)
        {
            var gestor = GestorDeDirecciones.Gestor(contexto, direccion.Negocio);
            return gestor.PersistirRegistro((DireccionDtm)direccion, new ParametrosDeNegocio(enumTipoOperacion.Insertar) { Parametros = parametros });
        }

        public static List<DireccionDtm> Direcciones(this IElementoDtm elemento, ContextoSe contexto)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!negocio.UsaDirecciones())
                GestorDeErrores.Emitir($"El {negocio.Singular(true)} no usa direcciones'");

            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, negocio, elemento.Id).ToList();
            return direcciones;
        }

        public static DireccionDtm Direccion(this IElementoDtm elemento, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!negocio.UsaDirecciones())
                GestorDeErrores.Emitir($"El {negocio.Singular(true)} no usa direcciones y ha solictado obtener la dirección '{calificador.Descripcion()}'");

            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, negocio, elemento.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion == null)
            {
                if (errorSiNoHay)
                    GestorDeErrores.Emitir($"{negocio.Singular(true)}: '{(elemento.GetType().ImplementaUsaReferencia() ? ((IUsaReferencia)elemento).Referencia : elemento.Nombre)}' debe tener una dirección '{calificador.Descripcion()}'");
                return null;
            }
            return direccion;
        }

        public static DireccionDtm QuitarDireccion(this IElementoDtm elemento, ContextoSe contexto, DireccionDtm direccion)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!negocio.UsaDirecciones())
                GestorDeErrores.Emitir($"El {negocio.Singular(true)} no usa direcciones y ha solictado quitar la dirección '{direccion.Expresion(contexto)}'");

            GestorDeDirecciones.Gestor(contexto, negocio).PersistirRegistro(direccion, new ParametrosDeNegocio(enumTipoOperacion.Eliminar));

            return direccion;
        }

        public static DireccionDtm AsociarDireccion(this IElementoDtm elemento, ContextoSe contexto, DireccionDtm direccion)
        {
            var negocio = NegociosDeSe.NegocioDeUnDtm(elemento.GetType());
            if (!negocio.UsaDirecciones())
                GestorDeErrores.Emitir($"El {negocio.Singular(true)} no usa direcciones y ha solictado asociar la dirección '{direccion.Expresion(contexto)}'");
            direccion.Id = 0;
            GestorDeDirecciones.Gestor(contexto, negocio).PersistirRegistro(direccion, new ParametrosDeNegocio(enumTipoOperacion.Insertar));

            return direccion;
        }

        internal static void DireccionModificada(ContextoSe contexto, enumNegocio negocio, DireccionDtm nueva, DireccionDtm anterior)
        {
            var gestorDeCalles = NegociosDeSe.CrearGestor(contexto, enumNegocio.Calle);
            var calleNueva = (CalleDtm)gestorDeCalles.LeerRegistroPorId(nueva.IdCalle, true);
            var traza = new TrazaDtm
            {
                IdElemento = nueva.IdElemento,
                Nombre = "Direccion modificada",
                Descripcion = $"El usuario {contexto.DatosDeConexion.Login} ha modificado la dirección {Environment.NewLine}" +
                $"Calificador: {anterior.Calificador} --> {nueva.Calificador}{Environment.NewLine}" +
                $"Municipio: {anterior.Municipio} --> {calleNueva.Municipio.Nombre}{Environment.NewLine}" +
                $"Calle: {anterior.Calle} --> {calleNueva.Nombre}{Environment.NewLine}" +
                $"Numero: {anterior.Numero} --> {nueva.Numero}{Environment.NewLine}"
            };

            GestorDeTrazas.Gestor(contexto, negocio).PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
        }

        internal static void DireccionEliminada(ContextoSe contexto, enumNegocio negocio, DireccionDtm direccion)
        {
            var traza = new TrazaDtm
            {
                IdElemento = direccion.IdElemento,
                Nombre = "Direccion eliminada",
                Descripcion = $"La dirección {direccion.Expresion(contexto)} ha sido eliminada por el usuario {contexto.DatosDeConexion.Login}"
            };

            GestorDeTrazas.Gestor(contexto, negocio).PersistirRegistro(traza, new ParametrosDeNegocio(enumTipoOperacion.Insertar));

        }


        public static void DesactivarDireccion(this DireccionDtm direccion, ContextoSe contexto)
        {
            var gestor = new GestorDeDirecciones(contexto, direccion.Negocio);
            direccion.Activo = false;
            gestor.PersistirRegistro(direccion, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
        }

        public static void ActivarDireccion(this DireccionDtm direccion, ContextoSe contexto)
        {
            var gestor = new GestorDeDirecciones(contexto, direccion.Negocio);
            direccion.Activo = true;
            gestor.PersistirRegistro(direccion, new ParametrosDeNegocio(enumTipoOperacion.Modificar));
        }

        public static void EliminarDireccion(this DireccionDtm direccion, ContextoSe contexto)
        {
            var gestor = new GestorDeDirecciones(contexto, direccion.Negocio);
            gestor.PersistirRegistro(direccion, new ParametrosDeNegocio(enumTipoOperacion.Eliminar));
        }

        public static DireccionDtm Buscar(this List<DireccionDtm> direcciones, DireccionDtm direccion, bool comparaCalificador)
        {
            foreach (var item in direcciones)
            {
                if (item.EsIgualA(direccion, comparaCalificador)) return item;
            }
            return null;
        }

        public static DireccionDtm AsignarDireccionSiNoExiste(this IElementoDtm elemento, ContextoSe contexto, DireccionDtm direccionExistente, enumCalificadorDireccion? calificador)
        {
            var direcciones = elemento.Direcciones(contexto);
            foreach (var direccion in direcciones)
            {
                if (direccion.EsIgualA(direccionExistente, compararCalificador: false))
                {
                    return direccionExistente;
                }
            }
            return elemento.AsignarDireccion(contexto, direccionExistente, calificador == null ? direccionExistente.Calificador : calificador);
        }

        public static DireccionDtm AsignarDireccion(this IElementoDtm elemento, ContextoSe contexto, DireccionDtm direccion, enumCalificadorDireccion? calificador = null)
        {
            var direccionDelElemento = elemento.Direccion(contexto, calificador is null ? direccion.Calificador : (enumCalificadorDireccion)calificador);
            if (direccionDelElemento is null)
            {
                direccion.Calificador = calificador is null ? direccion.Calificador : (enumCalificadorDireccion)calificador;
                direccion = elemento.CopiarDireccion(contexto, direccion);
            }
            return direccion;
        }

        internal static DireccionDtm CopiarDireccion(this IElementoDtm elemento, ContextoSe contexto, DireccionDtm direccion)
        {
            var d = direccion.Copiar();
            d.IdElemento = elemento.Id;
            direccion = GestorDeDirecciones.Gestor(contexto, NegociosDeSe.NegocioDeUnDtm(elemento.GetType())).PersistirRegistro(d, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
            return direccion;
        }

        public static DireccionDtm CrearDireccion(this CalleDtm calle, enumCalificadorDireccion calificador)
        {
            DireccionDtm direccion = new DireccionDtm();
            direccion.IdPais = calle.Municipio.Provincia.IdPais;
            direccion.IdProvincia = calle.Municipio.IdProvincia;
            direccion.IdMunicipio = calle.IdMunicipio;
            direccion.IdCalle = calle.Id;
            direccion.Calificador = calificador;
            return direccion;
        }

        public static string Expresion(this DireccionDtm direccion, ContextoSe contexto)
        {
            return $"({direccion.Calificador}) {direccion.Nombre(contexto)}";
        }

        public static string Nombre(this DireccionDtm direccion, ContextoSe contexto)
        {
            var calle = contexto.SeleccionarPorId<CalleDtm>(direccion.IdCalle, aplicarJoin: true);

            var nombredireccion = $"{calle.TipoDeVia.Nombre} {calle.Nombre}, {(direccion.Numero is null ? "" : $"{direccion.Numero},")} ";
            if (!direccion.Cp.IsNullOrEmpty())
                nombredireccion = nombredireccion + direccion.Cp + "-";

            if (!direccion.Zona.IsNullOrEmpty())
            {
                if (direccion.Zona == direccion.Municipio)
                    nombredireccion = nombredireccion + direccion.Municipio + ", ";
                else
                    nombredireccion = nombredireccion + direccion.Zona + ", " + direccion.Municipio + ", ";
            }
            else
            {
                nombredireccion = nombredireccion + direccion.Municipio + ", ";
            }

            if (direccion.Municipio == direccion.Provincia)
                nombredireccion = nombredireccion + direccion.Pais;
            else
                nombredireccion = nombredireccion + direccion.Provincia + ", " + direccion.Pais;

            return nombredireccion;
        }

        public static PaisDtm Pais(this DireccionDtm direccion, ContextoSe contexto) => contexto.SeleccionarPorId<PaisDtm>(direccion.IdPais);

        public static bool EsIntraComunitario(this DireccionDtm direccion, ContextoSe contexto) => direccion.Pais(contexto).IntraComunitario;

        public static bool EsExtraComunitario(this DireccionDtm direccion, ContextoSe contexto) => direccion.Pais(contexto).ExtraComunitario;

        public static ProvinciaDtm Provincia(this PaisDtm pais, ContextoSe contexto, string nombre, bool errorSiNoExiste = true)
        {
            var provincias = nombre.Length == 2 && nombre.Entero() > 0
                    ? contexto.SeleccionarTodos<ProvinciaDtm>(new List<ClausulaDeFiltrado> {
                new ClausulaDeFiltrado { Clausula = nameof(ProvinciaDtm.IdPais), Criterio = enumCriteriosDeFiltrado.igual, Valor = pais.Id.ToString() },
                new ClausulaDeFiltrado { Clausula = nameof(ProvinciaDtm.Codigo), Criterio = enumCriteriosDeFiltrado.igual, Valor = nombre }
                })
                    : contexto.SeleccionarTodos<ProvinciaDtm>(new List<ClausulaDeFiltrado> {
                new ClausulaDeFiltrado { Clausula = nameof(ProvinciaDtm.IdPais), Criterio = enumCriteriosDeFiltrado.igual, Valor = pais.Id.ToString() },
                new ClausulaDeFiltrado { Clausula = nameof(ProvinciaDtm.Nombre), Criterio = enumCriteriosDeFiltrado.igual, Valor = nombre }
                });

            if (provincias.Count == 0 && !errorSiNoExiste)
                return null;

            if (provincias.Count == 0)
                GestorDeErrores.Emitir($"Defina la provincia '{nombre}' del estado '{pais.Nombre}'");

            if (provincias.Count > 1)
                GestorDeErrores.Emitir($"Hay más de una provincia '{nombre}' en el estado '{pais.Nombre}'");

            return provincias[0];
        }

        public static MunicipioDtm Municipio(this ProvinciaDtm provincia, ContextoSe contexto, string nombre, bool errorSiNoExiste = true)
        {
            var municipio = contexto.SeleccionarTodos<MunicipioDtm>(new List<ClausulaDeFiltrado> {
                new ClausulaDeFiltrado { Clausula = nameof(MunicipioDtm.IdProvincia), Criterio = enumCriteriosDeFiltrado.igual, Valor = provincia.Id.ToString() },
                new ClausulaDeFiltrado { Clausula = nameof(MunicipioDtm.Nombre), Criterio = enumCriteriosDeFiltrado.igual, Valor = nombre }
                });

            if (municipio.Count == 0 && !errorSiNoExiste)
                return null;

            if (municipio.Count == 0)
                GestorDeErrores.Emitir($"Defina el municipio '{nombre}' en la provincia '{provincia.Nombre}'");

            if (municipio.Count > 1)
                GestorDeErrores.Emitir($"Hay más de un municipio '{nombre}' en el estado '{provincia.Nombre}'");

            return municipio[0];
        }

        public static CrearDireccionDto HayQueCrearDireccion(Dictionary<string, object> parametros)
        {
            var crearDireccionJson = parametros.LeerValor<JObject>(Ampliaciones.Comunes.DireccionAlCrear, null);
            if (crearDireccionJson != null)
            {
                var crearDireccionDto = crearDireccionJson.ToObject<CrearDireccionDto>();
                if (crearDireccionDto.IdCalle.Entero() > 0)
                {
                    if (crearDireccionDto.Calificador.IsNullOrEmpty())
                        GestorDeErrores.Emitir("Debe indicar el calificador de la dirección");

                    return crearDireccionDto;
                }
            }
            return null;
        }
        public static enumNegocio DeQuienEsLaDireccion(this CrearDireccionDto crearDireccionDto, Dictionary<string, object> parametros)
        {
            if (crearDireccionDto != null &&
                 (ApiDeEnsamblados.ToEnumerado<enumCalificadorDireccion>(crearDireccionDto.Calificador) == enumCalificadorDireccion.contacto ||
                  ApiDeEnsamblados.ToEnumerado<enumCalificadorDireccion>(crearDireccionDto.Calificador) == enumCalificadorDireccion.fiscal) &&
                (parametros.LeerValor(nameof(SociedadDto.CrearInterlocutor), false) || parametros.LeerValor(nameof(SociedadDto.CrearCliente), false))
               )
            {
                parametros.Remove(Ampliaciones.Comunes.DireccionAlCrear);
                if (ApiDeEnsamblados.ToEnumerado<enumCalificadorDireccion>(crearDireccionDto.Calificador) == enumCalificadorDireccion.contacto)
                    return enumNegocio.Interlocutor;
                return enumNegocio.Cliente;
            }
            return enumNegocio.No_Definido;
        }

        public static CalleDtm Calle(this DireccionDtm direccion,  ContextoSe contexto, bool aplicarJoin) => contexto.SeleccionarPorId<CalleDtm>(direccion.IdCalle, aplicarJoin: true);

        //public static DireccionDtm CrearDireccion(ContextoSe contexto, string pais, string provincia, string municipio, string tipoDeVia, string calle, string cp, string np, string rd, bool errorSiNoSePuede = false)
        //{
        //    var provinciaDtm = contexto.SeleccionarPorNombre<ProvinciaDtm>(provincia, errorSiNoHay: false);

        //    var municipioDtm = contexto.SeleccionarTodos<MunicipioDtm>(new Dictionary<string, object> {
        //        { nameof(MunicipioDtm.IdProvincia), provinciaDtm.Id },
        //        { nameof(MunicipioDtm.Nombre), municipio } });
        //    if (municipioDtm.Count() != 1)
        //        return null;

        //    var tipoDeViaDtm = contexto.SeleccionarTodos<TipoDeViaDtm>(new Dictionary<string, object> { { nameof(TipoDeViaDtm.Nombre), tipoDeVia } });
        //    if (tipoDeViaDtm.Count() != 1)
        //        return null;

        //    var calleDtm = contexto.SeleccionarTodos<CalleDtm>(new Dictionary<string, object> {
        //        { nameof(CalleDtm.Nombre), calle },
        //         { nameof(CalleDtm.IdMunicipio), municipioDtm[0].Id },
        //         { nameof(CalleDtm.IdTipoDeVia), tipoDeViaDtm[0].Id },
        //    });


        //    if (calleDtm.Count() == 0)
        //    {
        //        calleDtm.Add(new CalleDtm
        //        {
        //            Nombre = calle,
        //            IdTipoDeVia = tipoDeViaDtm[0].Id,
        //            IdMunicipio = municipioDtm[0].Id
        //        }.InsertarComoAdministrador(contexto));
        //    }

        //    if (calleDtm.Count() != 1)
        //        return null;

        //    new DireccionDtm
        //    {
        //        IdPais = provinciaDtm.IdPais,
        //        IdProvincia = provinciaDtm.Id,
        //        IdMunicipio = municipioDtm[0].Id,
        //        IdCalle = calleDtm[0].Id,
        //        Calificador = enumCalificadorDireccion.fiscal,
        //        Numero = np,
        //        r

        //    }

        //    return null;
        //}
    }

}



//public static void SincronizarDireccion(this DireccionDtm nueva, ContextoSe contexto, DireccionDtm anterior)
//{
//    // si usa el módulo de guarderías y la dirección es de una persona o interlocutor
//    if (!ExtensorDeGuarderias.ModuloActivo(contexto))
//        return;

//    // busco si hay infantes dependiente (contacto, papa, mama)
//    var infantes = nueva.Negocio == enumNegocio.Interlocutor
//    ? contexto.Set<InfanteDtm>().Where(x => x.IdContacto == nueva.IdElemento).ToList()
//    : nueva.Negocio == enumNegocio.Persona
//    ? contexto.Set<InfanteDtm>().Where(x => x.IdMadre == nueva.IdElemento || x.IdPadre == nueva.IdElemento).ToList()
//    : null;

//    if (infantes is null) return;

//    // para cada infante busco si tiene una dirección de contacto como la anterior
//    var insertando = anterior == null;
//    foreach (var infante in infantes)
//    {
//        if (insertando) infante.AsignarDireccionSiNoExiste(contexto, nueva, enumCalificadorDireccion.contacto);
//        else
//        {
//            DireccionDtm direccionDelInfanteExistente = infante.Direcciones(contexto).Buscar(anterior, comparaCalificador: false);
//            DireccionDtm direccionDelInfanteNueva = infante.Direcciones(contexto).Buscar(nueva, comparaCalificador: false);

//            //Si la anterior es la misma que la nueva, es que puede ser que se esté activando o desactivando, si es así, la activo o la desactivo y me voy
//            if (direccionDelInfanteNueva != null && direccionDelInfanteExistente != null && direccionDelInfanteExistente.Id == direccionDelInfanteNueva.Id)
//            {
//                if (direccionDelInfanteExistente.Activo == direccionDelInfanteNueva.Activo) return;
//                if (nueva.Activo && !direccionDelInfanteNueva.Activo) direccionDelInfanteNueva.ActivarDireccion(contexto);
//                if (!nueva.Activo && direccionDelInfanteNueva.Activo) direccionDelInfanteNueva.DesactivarDireccion(contexto);
//                return;
//            }
//            if (direccionDelInfanteNueva != null && direccionDelInfanteExistente != null && direccionDelInfanteExistente.Id != direccionDelInfanteNueva.Id)
//            {
//                direccionDelInfanteExistente.DesactivarDireccion(contexto);
//                direccionDelInfanteNueva.ActivarDireccion(contexto);
//                return;
//            }
//            //si no es la misma la que tiene y la que viene como nueva
//            if (direccionDelInfanteExistente != null && direccionDelInfanteExistente.Activo)
//                direccionDelInfanteExistente.DesactivarDireccion(contexto);

//            infante.AsignarDireccion(contexto, nueva, enumCalificadorDireccion.contacto);
//        }
//    }
//}

