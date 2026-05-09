using System;
using System.Collections.Generic;
using System.Linq;
using GestorDeElementos;
using GestoresDeNegocio.SistemaDocumental;
using GestoresDeNegocio.TrabajosSometidos;
using Microsoft.EntityFrameworkCore;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.TrabajosSometidos;
using Utilidades;
using static GestoresDeNegocio.Callejero.GestorDeCodigosPostales;

namespace GestoresDeNegocio.Callejero
{
    public static class ApiParaImportarCallejero
    {

        internal static void ImportarFicheroDePaises(EntornoDeTrabajo entorno, int idArchivo)
        {
            var gestor = GestorDePaises.Gestor(entorno.contextoDelProceso, entorno.contextoDelProceso.Mapeador);
            var rutaFichero = ServidorDocumental.DescargarArchivo(entorno.contextoDelProceso, idArchivo, entorno.ProcesoIniciadoPorLaCola);
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 0;
            entorno.CrearTraza($"Inicio del proceso");
            var trazaPrcDtm = entorno.CrearTraza($"Procesando la fila {linea}");
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            foreach (var fila in fichero)
            {
                var tran = gestor.IniciarTransaccion();
                try
                {
                    linea++;
                    if (fila.EnBlanco)
                        continue;

                    if (fila.Columnas != 5)
                        throw new Exception($"la fila {linea} solo debe tener 5 columnas");

                    if (fila["A"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica el nombre del país, celda A, no puede ser nulo");
                    if (fila["B"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica el nombre en Inglés, celda B, no puede ser nulo");
                    if (fila["C"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica el iso2, celda C, no puede ser nulo");
                    if (fila["D"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica el iso3, celda D, no puede ser nulo");
                    if (fila["E"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica el prefijo telefónico, celda E, no puede ser nulo");

                    ProcesarPaisLeido(entorno, gestor, fila["A"], fila["B"], fila["C"], fila["D"], fila["E"], trazaInfDtm);
                    gestor.Commit(tran);
                }
                catch (Exception e)
                {
                    gestor.Rollback(tran);
                    entorno.AnotarError(e);
                }
                finally
                {
                    entorno.ActualizarTraza(trazaPrcDtm, $"Procesando la fila {linea}");
                }
            }

            entorno.CrearTraza($"Procesadas un total de {linea} filas");
        }

        private static PaisDtm ProcesarPaisLeido(EntornoDeTrabajo entorno, GestorDePaises gestor, string nombrePais, string nombreEnIngles, string Iso2, string codigoPais, string prefijoTelefono, TrazaDeUnTrabajoDtm trazaInfDtm)
        {
            ParametrosDeNegocio operacion;
            var pais = gestor.LeerRegistro(nameof(PaisDtm.Codigo), codigoPais, false, true, false, false);
            if (pais == null)
            {
                pais = new PaisDtm();
                pais.Codigo = codigoPais;
                pais.Nombre = nombrePais;
                pais.NombreIngles = nombreEnIngles;
                pais.ISO2 = Iso2;
                pais.Prefijo = prefijoTelefono;
                operacion = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
                entorno.ActualizarTraza(trazaInfDtm, $"Creando el pais {nombrePais}");
            }
            else
            {
                if (pais.Nombre != nombrePais || pais.ISO2 != Iso2 || pais.NombreIngles != nombreEnIngles || pais.Prefijo != prefijoTelefono)
                {
                    pais.Nombre = nombrePais;
                    pais.NombreIngles = nombreEnIngles;
                    pais.ISO2 = Iso2;
                    pais.Prefijo = prefijoTelefono;
                    operacion = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
                    entorno.ActualizarTraza(trazaInfDtm, $"Modificando el pais {nombrePais}");
                }
                else
                {
                    entorno.ActualizarTraza(trazaInfDtm, $"El pais {nombrePais} ya existe");
                    return pais;
                }
            }

            return gestor.PersistirRegistro(pais, operacion);
        }


        internal static void ImportarFicheroDeProvincias(EntornoDeTrabajo entorno, int idArchivo)
        {
            var gestorProceso = GestorDeProvincias.Gestor(entorno.contextoDelProceso, entorno.contextoDelProceso.Mapeador);
            var rutaFichero = ServidorDocumental.DescargarArchivo(entorno.contextoDelProceso, idArchivo, entorno.ProcesoIniciadoPorLaCola);
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 0;
            entorno.CrearTraza($"Inicio del proceso");
            var trazaPrcDtm = entorno.CrearTraza($"Procesando la fila {linea}");
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            foreach (var fila in fichero)
            {
                var tran = gestorProceso.IniciarTransaccion();
                try
                {
                    linea++;
                    if (fila.EnBlanco)
                        continue;

                    if (fila.Columnas != 5)
                        throw new Exception($"la fila {linea} solo debe tener 5 columnas");

                    if (fila["A"].IsNullOrEmpty() || fila["B"].IsNullOrEmpty() ||
                        fila["C"].IsNullOrEmpty() || fila["D"].IsNullOrEmpty() ||
                        fila["E"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} debe ser: nombre de la provincia, nombre en ingles, iso de 2 iso de 3 y prefijo telefónico");

                    ProcesarProvinciaLeida(entorno, gestorProceso,
                        iso2Pais: fila["E"],
                        nombreProvincia: fila["C"],
                        sigla: fila["A"],
                        codigo: fila["B"],
                        prefijoTelefono: fila["D"],
                        trazaInfDtm);
                    gestorProceso.Commit(tran);
                }
                catch (Exception e)
                {
                    gestorProceso.Rollback(tran);
                    entorno.AnotarError(e);
                }
                finally
                {
                    entorno.ActualizarTraza(trazaPrcDtm, $"Procesando la fila {linea}");
                }
            }

            entorno.CrearTraza($"Procesadas un total de {linea} filas");
        }

        private static ProvinciaDtm ProcesarProvinciaLeida(EntornoDeTrabajo entorno, GestorDeProvincias gestor, string iso2Pais, string nombreProvincia, string sigla, string codigo, string prefijoTelefono, TrazaDeUnTrabajoDtm trazaInfDtm)
        {
            var provinciaDtm = GestorDeProvincias.LeerProvinciaPorCodigo(gestor.Contexto, iso2Pais, codigo, paraActualizar: false, errorSiNoHay: false);
            if (provinciaDtm == null)
            {
                var pais = GestorDePaises.LeerPaisPorCodigo(gestor.Contexto, iso2Pais, errorSiNoHay: false);
                provinciaDtm = new ProvinciaDtm();
                provinciaDtm.Codigo = codigo;
                provinciaDtm.Nombre = nombreProvincia;
                provinciaDtm.Sigla = sigla;
                provinciaDtm.IdPais = pais.Id;
                provinciaDtm.Prefijo = prefijoTelefono;
                entorno.ActualizarTraza(trazaInfDtm, $"Creando la provincia {nombreProvincia}");
                return provinciaDtm.Insertar(entorno.contextoDelProceso);
            }
            else
            {
                if (provinciaDtm.Nombre != nombreProvincia || provinciaDtm.Codigo != codigo || provinciaDtm.Sigla != sigla || provinciaDtm.Prefijo != prefijoTelefono)
                {
                    provinciaDtm.Nombre = nombreProvincia;
                    provinciaDtm.Sigla = sigla;
                    provinciaDtm.Codigo = codigo;
                    provinciaDtm.Prefijo = prefijoTelefono;
                    entorno.ActualizarTraza(trazaInfDtm, $"Modificando la provincia {nombreProvincia}");
                    return provinciaDtm.Modificar(entorno.contextoDelProceso);
                }
            }
            entorno.ActualizarTraza(trazaInfDtm, $"La provincia {nombreProvincia} ya exite");
            return provinciaDtm;
        }

        internal static void ImportarFicheroDeMunicipios(EntornoDeTrabajo entorno, int idArchivo)
        {
            var gestorProceso = GestorDeMunicipios.Gestor(entorno.contextoDelProceso, entorno.contextoDelProceso.Mapeador);
            var rutaFichero = ServidorDocumental.DescargarArchivo(entorno.contextoDelProceso, idArchivo, entorno.ProcesoIniciadoPorLaCola);
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 1;
            entorno.CrearTraza($"Inicio del proceso");
            var trazaPrcDtm = entorno.CrearTraza($"Procesando la fila {linea}");
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            foreach (var fila in fichero)
            {
                var tran = gestorProceso.IniciarTransaccion();
                try
                {
                    if (fila.EnBlanco)
                        continue;

                    if (fila.Columnas != 5)
                        throw new Exception($"la fila {linea} solo debe tener 5 columnas");

                    if (fila["A"].IsNullOrEmpty() || fila["B"].IsNullOrEmpty() ||
                        fila["C"].IsNullOrEmpty() || fila["D"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} debe ser: código de provincia, DC, nombre del municipio");

                    ProcesarMunicipioLeido(entorno, gestorProceso,
                        iso2Pais: fila["A"],
                        codigoProvincia: fila["B"],
                        DC: fila["D"],
                        nombreMunicipio: fila["E"],
                        trazaInfDtm);
                    gestorProceso.Commit(tran);
                }
                catch (Exception e)
                {
                    gestorProceso.Rollback(tran);
                    entorno.AnotarError($"Error al procesar la fila {linea}", e);
                }
                finally
                {
                    entorno.ActualizarTraza(trazaPrcDtm, $"Procesando la fila {linea}");
                    linea++;
                }
            }

            entorno.CrearTraza($"Procesadas un total de {linea} filas");

        }

        private static MunicipioDtm ProcesarMunicipioLeido(EntornoDeTrabajo entorno, GestorDeMunicipios gestorProceso, string iso2Pais, string codigoProvincia, string DC, string nombreMunicipio, TrazaDeUnTrabajoDtm trazaInfDtm)
        {
            var municipioDtm = GestorDeMunicipios.LeerMunicipioPorCodigo(gestorProceso.Contexto, iso2Pais, codigoProvincia, nombreMunicipio, paraActualizar: false, errorSiNoHay: false);
            if (municipioDtm == null)
            {
                var provinciaDtm = GestorDeProvincias.LeerProvinciaPorCodigo(gestorProceso.Contexto, iso2Pais, codigoProvincia, paraActualizar: false);

                municipioDtm = new MunicipioDtm();
                municipioDtm.IdProvincia = provinciaDtm.Id;
                municipioDtm.Nombre = nombreMunicipio;
                municipioDtm.DC = DC;
                entorno.ActualizarTraza(trazaInfDtm, $"Creando el municipio {nombreMunicipio}");
                return municipioDtm.Insertar(entorno.contextoDelProceso);
            }
            else
            {
                if (municipioDtm.Nombre != nombreMunicipio || municipioDtm.DC != DC)
                {
                    municipioDtm.Nombre = nombreMunicipio;
                    municipioDtm.DC = DC;
                    municipioDtm.UsuarioModificador = null;
                    entorno.ActualizarTraza(trazaInfDtm, $"Modificando el municipio {nombreMunicipio}");
                    return municipioDtm.Modificar(entorno.contextoDelProceso);
                }
            }
            entorno.ActualizarTraza(trazaInfDtm, $"el municipio {nombreMunicipio} ya exite");
            return municipioDtm;
        }

        internal static void ImportarFicheroDeCalles(EntornoDeTrabajo entorno, int idArchivo)
        {
            var gestorProceso = GestorDeCalles.Gestor(entorno.contextoDelProceso, entorno.contextoDelProceso.Mapeador);
            var rutaFichero = ServidorDocumental.DescargarArchivo(entorno.contextoDelProceso, idArchivo, entorno.ProcesoIniciadoPorLaCola);
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 1;
            entorno.CrearTraza($"Inicio del proceso");
            var trazaPrcDtm = entorno.CrearTraza($"Procesando la fila {linea}");
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            var creadas = 0;
            var modificadas = 0;
            var sinTratar = 0;
            var errores = 0;
            (CalleDtm calleDtm, enumTipoOperacion operacioRealizada) resultado;
            foreach (var fila in fichero)
            {
                var tran = gestorProceso.IniciarTransaccion();
                try
                {
                    if (fila.EnBlanco)
                        continue;

                    if (fila.Columnas != 7)
                        throw new Exception($"la fila {linea} solo debe tener 7 columnas");

                    if (fila["A"].IsNullOrEmpty() || fila["B"].IsNullOrEmpty() ||
                        fila["C"].IsNullOrEmpty() || fila["D"].IsNullOrEmpty() ||
                        fila["E"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} debe ser:Pais, Provincia, Municipio, Tipo vía, Nombre de la calle, opcional mente Barrios y CPs separados por |");

                    if (fila["D"].Contains("definida", StringComparison.CurrentCultureIgnoreCase)) continue;
                    if (fila["G"].IsNullOrEmpty()) continue;

                    var barrios = fila["F"].ToLista<string>(Simbolos.Pipe, true);
                    var cps = fila["G"].ToLista<int>(Simbolos.Pipe, true);

                    resultado = ProcesarCalleLeida(entorno, gestorProceso,
                        pais: fila["A"],
                        provincia: fila["B"],
                        municipio: fila["C"],
                        tipodevia: fila["D"],
                        nombreCalle: fila["E"],
                        listaDeBarrios: barrios,
                        listaDeCps: cps,
                        linea: linea,
                        trazaInfDtm);
                    if (resultado.operacioRealizada == enumTipoOperacion.Insertar) creadas++;
                    if (resultado.operacioRealizada == enumTipoOperacion.Modificar) modificadas++;
                    if (resultado.operacioRealizada == enumTipoOperacion.NoDefinida) sinTratar++;
                    gestorProceso.Commit(tran);
                }
                catch (Exception e)
                {
                    gestorProceso.Rollback(tran);
                    errores++;
                    entorno.AnotarError($"Error al procesar la fila {linea}", e);
                }
                finally
                {
                    entorno.ActualizarTraza(trazaPrcDtm, $"Procesada la fila {linea}, calle {fila["E"]}, creadas {creadas}, modificadas {modificadas}, sin tratar {sinTratar}, errores {errores}");
                    linea++;
                }
            }
            entorno.CrearTraza($"Procesadas un total de {linea} filas, creadas {creadas}, modificadas {modificadas} y sin tratar {sinTratar}");
        }

        private static (CalleDtm calleDtm, enumTipoOperacion operacioRealizada) ProcesarCalleLeida(EntornoDeTrabajo entorno
            , GestorDeCalles gestorProceso
            , string pais
            , string provincia
            , string municipio
            , string tipodevia
            , string nombreCalle
            , List<string> listaDeBarrios
            , List<int> listaDeCps
            , int linea
            , TrazaDeUnTrabajoDtm trazaInfDtm)
        {
            var tipoViaDtm = GestorDeTiposDeVia.Gestor(gestorProceso.Contexto, gestorProceso.Contexto.Mapeador)
                .LeerRegistroCacheado(nameof(TipoDeViaDtm.Nombre), tipodevia, true, true, false);

            var paisDtm = GestorDePaises.Gestor(gestorProceso.Contexto, gestorProceso.Contexto.Mapeador)
                .LeerRegistroCacheado(nameof(PaisDtm.Nombre), pais, true, true, false);

            var provinciaDtm = GestorDeProvincias.Gestor(gestorProceso.Contexto, gestorProceso.Contexto.Mapeador)
                .LeerRegistroCacheado(nameof(ProvinciaDtm.Nombre), provincia, true, true, false);

            var calleDtm = GestorDeCalles.LeerCallePorNombre(gestorProceso.Contexto, pais, provincia, municipio, nombreCalle, paraActualizar: false, errorSiNoHay: false);
            enumTipoOperacion operacionRealizada;
            if (calleDtm == null)
            {
                var municipioDtm = GestorDeMunicipios.LeerMunicipioCacheadoPorNombre(gestorProceso.Contexto, paisDtm.ISO2, provincia, municipio, paraActualizar: false, aplicarJoin: false);

                calleDtm = new CalleDtm();
                calleDtm.IdMunicipio = municipioDtm.Id;
                calleDtm.Nombre = nombreCalle;
                calleDtm.IdTipoDeVia = tipoViaDtm.Id;
                operacionRealizada = enumTipoOperacion.Insertar;
                entorno.ActualizarTraza(trazaInfDtm, $"Creando la calle {nombreCalle}");
            }
            else
            {
                if (calleDtm.Nombre != nombreCalle || calleDtm.IdTipoDeVia != tipoViaDtm.Id)
                {
                    calleDtm.Nombre = nombreCalle;
                    calleDtm.IdTipoDeVia = tipoViaDtm.Id;
                    operacionRealizada = enumTipoOperacion.Modificar;
                    calleDtm.UsuarioModificador = null;
                    entorno.ActualizarTraza(trazaInfDtm, $"Modificando la calle {nombreCalle}");
                }
                else
                {
                    entorno.ActualizarTraza(trazaInfDtm, $"la calle {nombreCalle} ya exite");
                    operacionRealizada = enumTipoOperacion.NoDefinida;
                }
            }

            if (operacionRealizada != enumTipoOperacion.NoDefinida)
            {
                calleDtm.Municipio = null;
                calleDtm.TipoDeVia = null;
                gestorProceso.Contexto.ChangeTracker.Clear();
                calleDtm = gestorProceso.PersistirRegistro(calleDtm, new ParametrosDeNegocio(operacionRealizada));
            }

            ProcesarBarrios(gestorProceso, calleDtm, listaDeBarrios);
            ProcesarCps(entorno, gestorProceso, calleDtm, listaDeCps, provinciaDtm, linea);

            return (calleDtm, operacioRealizada: operacionRealizada);

        }

        private static void ProcesarCps(EntornoDeTrabajo entorno, GestorDeCalles gestorDelProceso, CalleDtm calleDtm, List<int> listaDeCps, ProvinciaDtm provinciaDtm, int linea)
        {
            CodigoPostalDtm cpDtm;
            foreach (var codigo in listaDeCps)
            {
                var codigoPostal = codigo.ToString().PadLeft(5, '0');
                if (provinciaDtm.Codigo != codigoPostal.Substring(0, 2))
                {
                    entorno.AnotarError($"Error al procesar la fila {linea}", $"El cp {codigoPostal} no puede ser asociado a la calle {calleDtm.Nombre}, ya que la provincia {provinciaDtm.Nombre} tiene el código {provinciaDtm.Codigo}");
                    continue;
                }

                var cps = gestorDelProceso.Contexto.Set<CodigoPostalDtm>().AsNoTracking().Where(x => x.Codigo.Equals(codigoPostal));
                if (cps.ToList().Count == 0)
                {
                    cpDtm = new CodigoPostalDtm();
                    cpDtm.Codigo = codigoPostal;
                    var gestorDeCps = GestorDeCodigosPostales.Gestor(gestorDelProceso.Contexto, gestorDelProceso.Mapeador);
                    cpDtm = gestorDeCps.PersistirRegistro(cpDtm, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                }
                else
                    cpDtm = cps.ToList()[0];

                var gestorDeCpsDeUnaCalle = GestorDeCpsDeUnaCalle.Gestor(gestorDelProceso.Contexto, gestorDelProceso.Mapeador);
                gestorDeCpsDeUnaCalle.CrearRelacion(nameof(CpsDeUnaCalleDtm.IdCalle), calleDtm.Id, cpDtm.Id, false);
            }
        }

        private static void ProcesarBarrios(GestorDeCalles gestorDelProceso, CalleDtm calleDtm, List<string> listaDeBarrios)
        {
            BarrioDtm barrioDtm;
            foreach (var barrio in listaDeBarrios)
            {
                var barrios = gestorDelProceso.Contexto.Set<BarrioDtm>().AsNoTracking().Where(x => x.Nombre.Equals(barrio) && x.IdMunicipio.Equals(calleDtm.IdMunicipio));
                if (barrios.ToList().Count == 0)
                {
                    barrioDtm = new BarrioDtm();
                    barrioDtm.IdMunicipio = calleDtm.IdMunicipio;
                    barrioDtm.Nombre = barrio;
                    var gestorDeCps = GestorDeBarrios.Gestor(gestorDelProceso.Contexto, gestorDelProceso.Mapeador);
                    barrioDtm = gestorDeCps.PersistirRegistro(barrioDtm, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                }
                else
                    barrioDtm = barrios.ToList()[0];

                var gestorDeBarriosDeUnaCalle = GestorDeBarriosDeUnaCalle.Gestor(gestorDelProceso.Contexto, gestorDelProceso.Mapeador);
                gestorDeBarriosDeUnaCalle.CrearRelacion(nameof(BarriosDeUnaCalleDtm.IdCalle), calleDtm.Id, barrioDtm.Id, false);
            }
        }


        internal static void ImportarFicheroDeTiposDeVia(EntornoDeTrabajo entorno, int idArchivo)
        {
            var gestor = GestorDeTiposDeVia.Gestor(entorno.contextoDelProceso, entorno.contextoDelProceso.Mapeador);
            var rutaFichero = ServidorDocumental.DescargarArchivo(entorno.contextoDelProceso, idArchivo, entorno.ProcesoIniciadoPorLaCola);
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 0;
            entorno.CrearTraza($"Inicio del proceso");
            var trazaPrcDtm = entorno.CrearTraza($"Procesando la fila {linea}");
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            foreach (var fila in fichero)
            {
                var tran = gestor.IniciarTransaccion();
                try
                {
                    linea++;
                    if (fila.EnBlanco)
                        continue;

                    if (fila.Columnas != 2)
                        throw new Exception($"la fila {linea} solo debe tener 2 columnas");

                    if (fila["A"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica la sigla, celda A, no puede ser nulo");
                    if (fila["B"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica el nombre, celda B, no puede ser nulo");

                    ProcesarTipoDeViaLeido(entorno, gestor, fila["A"], fila["B"], trazaInfDtm);
                    gestor.Commit(tran);
                }
                catch (Exception e)
                {
                    gestor.Rollback(tran);
                    entorno.AnotarError($"Error al procesar la línea {linea}", e);
                }
                finally
                {
                    entorno.ActualizarTraza(trazaPrcDtm, $"Procesando la fila {linea}");
                }
            }

            entorno.CrearTraza($"Procesadas un total de {linea} filas");
        }

        private static TipoDeViaDtm ProcesarTipoDeViaLeido(EntornoDeTrabajo entorno, GestorDeTiposDeVia gestor, string sigla, string nombre, TrazaDeUnTrabajoDtm trazaInfDtm)
        {
            ParametrosDeNegocio operacion;
            var p = gestor.LeerRegistro(nameof(TipoDeViaDtm.Sigla), sigla, errorSiNoHay: false, errorSiHayMasDeUno: true, conBloqueo: false, false);
            //var p = LeerTipoDeViaPorSigla(entorno.contextoDelProceso, sigla, paraActualizar: true); 
            if (p == null)
            {
                p = new TipoDeViaDtm();
                p.Sigla = sigla;
                p.Nombre = nombre;
                operacion = new ParametrosDeNegocio(enumTipoOperacion.Insertar);
                entorno.ActualizarTraza(trazaInfDtm, $"Creando el tipo de vía {sigla}");
            }
            else
            {
                if (p.Nombre != nombre)
                {
                    p.Nombre = nombre;
                    operacion = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
                    entorno.ActualizarTraza(trazaInfDtm, $"Modificando el tipo de vía {sigla}");
                    entorno.CrearTraza($"Existe un tipo de vía con la sigla {p.Sigla}, el nombre es {p.Nombre}, vaya al mantenimiento si quiere cambiar el nombre por {nombre}");
                }
                else
                {
                    entorno.ActualizarTraza(trazaInfDtm, $"El tipo de vía {sigla} ya existe");
                    return p;
                }
            }

            return gestor.PersistirRegistro(p, operacion);
        }

        internal static void ImportarFicheroDeCodigosPostales(EntornoDeTrabajo entorno, int idArchivo)
        {
            var gestor = GestorDeCodigosPostales.Gestor(entorno.contextoDelProceso, entorno.contextoDelProceso.Mapeador);
            var rutaFichero = ServidorDocumental.DescargarArchivo(entorno.contextoDelProceso, idArchivo, entorno.ProcesoIniciadoPorLaCola);
            var fichero = new FicheroCsv(rutaFichero);
            var linea = 0;
            entorno.CrearTraza($"Inicio del proceso");
            var trazaPrcDtm = entorno.CrearTraza($"Procesando la fila {linea}");
            var trazaInfDtm = entorno.CrearTraza($"Traza informativa del proceso");
            foreach (var fila in fichero)
            {
                var tran = gestor.IniciarTransaccion();
                try
                {
                    linea++;
                    if (fila.EnBlanco)
                        continue;

                    if (fila.Columnas != 3)
                        throw new Exception($"la fila {linea} solo debe tener 3 columnas");

                    if (fila["A"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica la provincia, celda A, no puede ser nulo");
                    if (fila["B"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica el municipio, celda B, no puede ser nulo");
                    if (fila["C"].IsNullOrEmpty())
                        throw new Exception($"El contenido de la fila {linea} donde se indica el CP, celda C, no puede ser nulo");

                    ProcesarCodigosPostales(entorno, gestor, fila["A"], fila["B"], fila["C"], trazaInfDtm);
                    gestor.Commit(tran);
                }
                catch (Exception e)
                {
                    gestor.Rollback(tran);
                    entorno.AnotarError($"Error al procesar la línea {linea}", e);
                }
                finally
                {
                    entorno.ActualizarTraza(trazaPrcDtm, $"Procesando la fila {linea}");
                }
            }

            entorno.CrearTraza($"Procesadas un total de {linea} filas");
        }

        private static CodigoPostalDtm ProcesarCodigosPostales(EntornoDeTrabajo entorno, GestorDeCodigosPostales gestor, string provincia, string municipio, string cp, TrazaDeUnTrabajoDtm trazaInfDtm)
        {
            var codigoPostalDtm = gestor.LeerRegistro(nameof(CodigoPostalDtm.Codigo), cp, errorSiNoHay: false, errorSiHayMasDeUno: true, conBloqueo: false, aplicarJoin: false);
            var parametros = new Dictionary<string, object>
             {
                 { ltrDeUnCp.NombreProvincia,provincia},
                 { ltrDeUnCp.NombreMunicipio, municipio}
            };
            if (codigoPostalDtm == null)
            {
                codigoPostalDtm = new CodigoPostalDtm();
                codigoPostalDtm.Codigo = cp;
                entorno.ActualizarTraza(trazaInfDtm, $"Creando el codigo postal {cp}");
                return codigoPostalDtm.Insertar(entorno.contextoDelProceso, parametros: parametros);
            }

            entorno.ActualizarTraza(trazaInfDtm, $"El codigo postal {cp} ya existe");
            GestorDeCpsDeUnMunicipio.CrearRelacionConMunicipioSiNoExiste(entorno.contextoDelProceso, codigoPostalDtm, ltrIsoPaises.Spain, provincia, municipio);

            return codigoPostalDtm.Modificar(entorno.contextoDelProceso, parametros: parametros);
        }


    }
}
