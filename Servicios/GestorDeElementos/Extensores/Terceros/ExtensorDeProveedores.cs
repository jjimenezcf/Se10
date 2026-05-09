using Gestor.Errores;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{

    public static class ExtensorDeProveedores
    {

        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(CuentaDeProveedorDtm))
                return true;

            return false;
        }

        public static bool Migrado(this ProveedorDtm proveedor, ContextoSe contexto, enumSistemaContable sistema, string codigoSociedad)
        {
            var nombreTraza = VariablesDePreasiento.IndicarMigrado(sistema, codigoSociedad);
            return enumNegocio.Proveedor.Trazas(contexto).Where(traza => traza.IdElemento == proveedor.Id && traza.Nombre == nombreTraza).Any();
        }

        public static TrazaDtm MarcarComoMigrado(this ProveedorDtm proveedor, ContextoSe contexto, enumSistemaContable sistema, string codigoSociedad)
        {
            var nombreTraza = VariablesDePreasiento.IndicarMigrado(sistema, codigoSociedad);
            return proveedor.CrearTraza(contexto, nombreTraza, $"Proveedor migrado a la contabilidad de {sistema.Descripcion()}");
        }

        public static void DesmarcarComoMigrado(this ProveedorDtm proveedor, ContextoSe contexto, enumSistemaContable sistema, string codigoSociedad)
        {
            var nombreTraza = VariablesDePreasiento.IndicarMigrado(sistema, codigoSociedad);
            proveedor.QuitarTraza(contexto, nombreTraza);
        }

        public static T InicializarDatosProveedor<T>(this T objeto, ContextoSe contexto, ParametrosDeNegocio parametros, bool validarEnAlta = true)
        where T : IUsaProveedor
        {
            var proveedor = contexto.SeleccionarPorId<ProveedorDtm>(objeto.IdProveedor, aplicarJoin: true);
            if (proveedor.Baja && validarEnAlta)
            {
                proveedor.IndicarQueEstaDeBaja(contexto);
            }
            parametros.Parametros[nameof(ProveedorDtm)] = proveedor;
            objeto.Contacto = objeto.Contacto.IsNullOrEmpty() ? proveedor.Expresion : objeto.Contacto;
            objeto.Telefono = objeto.Telefono.IsNullOrEmpty() ? proveedor.Telefono : objeto.Telefono;
            objeto.eMail = objeto.eMail.IsNullOrEmpty() ? proveedor.eMail : objeto.eMail;

            return objeto;
        }

        public static DireccionDto DireccionFiscal(this ProveedorDtm proveedor, ContextoSe contexto, bool errorSiNoHay = true)
        =>
        proveedor.DireccionDto(contexto, enumCalificadorDireccion.fiscal, errorSiNoHay);

        private static DireccionDto DireccionDto(this ProveedorDtm proveedor, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direccion = proveedor.Direccion(contexto, calificador, errorSiNoHay);
            if (direccion is null)
                return null;
            return direccion.MapearDto(contexto, direccion.Negocio);
        }

        private static DireccionDtm Direccion(this ProveedorDtm proveedor, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Proveedor, proveedor.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion is not null)
            {
                direccion.Negocio = enumNegocio.Proveedor;
                return direccion;
            }
            return proveedor.Interlocutor(contexto).Direccion(contexto, calificador, errorSiNoHay);
        }

        //public static string Referencia(this ProveedorDtm proveedor, ContextoSe contexto)
        //{
        //    if (proveedor.Interlocutor != null)
        //        return proveedor.Interlocutor.Referencia(contexto);

        //    return contexto.SeleccionarPorId<InterlocutorDtm>(proveedor.IdInterlocutor, aplicarJoin: true).Referencia(contexto);
        //}


        public static CuentaDeProveedorDtm CrearCuentaBancaria(ContextoSe contexto, int idProveedor, string numeroIban, string alias)
        {
            var isoPais = numeroIban.Substring(0, 2);
            var dcIban = numeroIban.Substring(2, 2);
            var entidad = numeroIban.Substring(4, 4);
            var oficina = numeroIban.Substring(8, 4);
            var dcCcc = numeroIban.Substring(12, 2);
            var numero = numeroIban.Substring(14, 10);

            var cb = ExtensorDeCuentasBancarias.Leer(contexto, isoPais, dcIban, entidad, oficina, dcCcc, numero, crearSiNoExiste: false);
            if (cb is null)
            {
                cb = ExtensorDeCuentasBancarias.Crear(contexto, isoPais, dcIban, entidad, oficina, dcCcc, numero); ;
            }

            var cbps = contexto.SeleccionarTodos<CuentaDeProveedorDtm>(new Dictionary<string, object> {
               { nameof(CuentaDeProveedorDtm.IdElemento),idProveedor},
               { nameof(CuentaDeProveedorDtm.IdCuenta),cb.Id} });

            if (cbps.Any(c => c.Activa && (c.Clase == enumClaseDeCuentaBancaria.Ingreso || c.Clase == enumClaseDeCuentaBancaria.Ambas)))
                return cbps.First(c => c.Activa && (c.Clase == enumClaseDeCuentaBancaria.Ingreso || c.Clase == enumClaseDeCuentaBancaria.Ambas));

            if (cbps.Any(c => !c.Activa && (c.Clase == enumClaseDeCuentaBancaria.Ingreso || c.Clase == enumClaseDeCuentaBancaria.Ambas)))
                GestorDeErrores.Emitir($"La cuenta bancaria '{numeroIban}' de '{enumClaseDeCuentaBancaria.Ingreso}' para el proveedor está desactivada, actívela si es lo que desea");

            if (cbps.Any(c => c.Activa && c.Clase == enumClaseDeCuentaBancaria.Pago))
            {
                var cbp = cbps.First(c => c.Activa && c.Clase == enumClaseDeCuentaBancaria.Pago);
                cbp.Clase = enumClaseDeCuentaBancaria.Ambas;
                return cbp.Modificar(contexto);
            }

            return new CuentaDeProveedorDtm
            {
                IdElemento = idProveedor,
                IdCuenta = cb.Id,
                Clase = enumClaseDeCuentaBancaria.Ingreso,
                IdArchivo = null,
                Alias = alias
            }.InsertarComoAdministrador(contexto);
        }

        //public static InterlocutorDtm Interlocutor(this ProveedorDtm proveedor, ContextoSe contexto)
        //=>
        //proveedor.Interlocutor != null
        //? proveedor.Interlocutor
        //: contexto.SeleccionarPorId<InterlocutorDtm>(proveedor.IdInterlocutor, aplicarJoin: true);

        public static string NIF(this ProveedorDtm proveedor, ContextoSe contexto, bool quitarPrefijoEs = false)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ter_NifDeProveedor);
            if (!cache.ContainsKey(proveedor.Id.ToString()))
                cache[proveedor.Id.ToString()] = proveedor.Interlocutor(contexto).NIF(contexto, quitarPrefijoEs);
            return quitarPrefijoEs ? ((string)cache[proveedor.Id.ToString()]).Replace(ltrIsoPaises.Spain, "") : (string)cache[proveedor.Id.ToString()];
        }

        public static CuentaDeProveedorDtm CuentaDeProveedor(this ProveedorDtm proveedor, ContextoSe contexto, enumClaseDeCuentaBancaria clase, bool soloLasActivas = true, bool errorSiNoHay = true)
        {
            var cuentas = proveedor.Detalles<CuentaDeProveedorDtm>(contexto);

            var cuentasActivas = soloLasActivas ?
            clase != enumClaseDeCuentaBancaria.Ambas
                ? cuentas.Where(x => x.Activa && (x.Clase == enumClaseDeCuentaBancaria.Ambas || x.Clase == clase)).ToList()
                : cuentas.Where(x => x.Activa).ToList()
            : clase != enumClaseDeCuentaBancaria.Ambas
                ? cuentas.Where(x => x.Clase == enumClaseDeCuentaBancaria.Ambas || x.Clase == clase).ToList()
                : cuentas.ToList();

            if (cuentasActivas.Count() == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"El proveedor '{proveedor.NIF(contexto)}' no tiene ninguna cuenta bancaria activa o le falta el certificado");

            if (cuentasActivas.Count() > 1)
                GestorDeErrores.Emitir($"El proveedor '{proveedor.NIF(contexto)}' tiene más de una cuenta bancaria activa");

            return cuentasActivas.Count() == 0 ? null : cuentasActivas[0];
        }

        public static CuentaDeProveedorDtm AsociarCuenta(this ProveedorDtm proveedor, ContextoSe contexto, string alias, enumClaseDeCuentaBancaria clase, CuentaBancariaDtm cuenta)
        {
            var cp = proveedor.CuentaDeProveedor(contexto, clase, errorSiNoHay: false);
            if (cp is not null)
            {
                if (cp.IdCuenta == cuenta.Id && cp.Alias == alias) return cp;
                if (cp.IdCuenta == cuenta.Id && cp.Alias != alias)
                {
                    cp.Alias = alias;
                    cp.Clase = clase;
                    return cp.Modificar(contexto);
                }
                ;
                cp.Activa = false;
                cp.Modificar(contexto);
            }

            return new CuentaDeProveedorDtm
            {
                Activa = true,
                Alias = alias,
                IdCuenta = cuenta.Id,
                IdElemento = proveedor.Id,
                Clase = clase
            }.Insertar(contexto);
        }

        public static IQueryable<VinculoDtm> Proveedores(this enumNegocio negocio, ContextoSe contexto)
        {
            throw new Exception($"Se debe indicar como obtener los proveedores vinculados al negocio: {negocio}");
        }

        public static ProveedorDtm Proveedor(this IUsaProveedor elemento, ContextoSe contexto, bool aplicarJoin = false)
        {
            if (elemento.Proveedor == null)
                elemento.Proveedor = contexto.SeleccionarPorId<ProveedorDtm>(elemento.IdProveedor, aplicarJoin: aplicarJoin);
            else
            {
                if (elemento.Proveedor.Cuenta == null && aplicarJoin)
                {
                    elemento.Proveedor = contexto.SeleccionarPorId<ProveedorDtm>(elemento.IdProveedor, aplicarJoin: aplicarJoin);
                }
            }

            return elemento.Proveedor;
        }

        public static string RazonSocial(this ProveedorDtm proveedor, ContextoSe contexto) => proveedor.Interlocutor(contexto).RazonSocial(contexto);

        public static ArchivoDtm AnexarArchivo(this ProveedorDtm proveedor, ContextoSe contexto, string rutaConFichero)
        {
            return (ArchivoDtm)ApiDeEnsamblados.EjecutarMetodoEstatico(ApiDeEnsamblados.GestoresDeNegocio,
                ApiDeEnsamblados.ClaseDelServidorDocumental,
                ApiDeEnsamblados.MetodoDeAnexarArchivo,
                new object[] { contexto, enumNegocio.Proveedor, ((IRegistro)proveedor).Id, rutaConFichero });
        }

        public static ProveedorDtm CrearProveedor(ContextoSe contexto, SociedadDtm datos, bool errorSiEsGestionada = true)
        {
            var interlocutores = contexto.SeleccionarTodos<InterlocutorDtm>(nameof(InterlocutorDto.Expresion), datos.NIF);
            var interlocutor = interlocutores.Count == 0 ? null : interlocutores[0];
            if (interlocutor == null)
            {
                var sociedad = ExtensorDeSociedades.CrearSiNoExiste(contexto, datos.NIF, datos.Nombre, datos.Nombre, datos.CodigoFiscal, datos.eMail, datos.Telefono);

                if (errorSiEsGestionada && sociedad.EsGestionada(contexto))
                    GestorDeErrores.Emitir(ltrProveedor.Mensaje_NoSePuedeCrearSiEsGestionada.Replace("[NIF]", datos.NIF));

                interlocutor = ExtensorDeInterlocutores.CrearInterlocutor(contexto, sociedad.NIF, puedeSerAutonomo: true);
            }
            else
            {
                if (errorSiEsGestionada && interlocutor.Sociedad(contexto).EsGestionada(contexto))
                    GestorDeErrores.Emitir(ltrProveedor.Mensaje_NoSePuedeCrearSiEsGestionada.Replace("[NIF]", datos.NIF));


            }
            return CrearProveedor(contexto, interlocutor.Id, contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Proveedores).Id);
        }

        public static ProveedorDtm CrearProveedor(ContextoSe contexto, PersonaDtm datos)
        {
            var interlocutores = contexto.SeleccionarTodos<InterlocutorDtm>(nameof(InterlocutorDto.Expresion), datos.NIF);
            var interlocutor = interlocutores.Count == 0 ? null : interlocutores[0];
            if (interlocutor == null)
            {
                var persona = ExtensorDePersonas.CrearSiNoExiste(contexto, datos.NIF, datos.Nombre, datos.Apellidos, datos.EsNie, datos.eMail, datos.Telefono);
                interlocutor = ExtensorDeInterlocutores.CrearInterlocutor(contexto, persona.NIF);
            }
            return CrearProveedor(contexto, interlocutor.Id, contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Proveedores).Id);
        }

        public static ProveedorDtm CrearProveedor(ContextoSe contexto, int idInter, int idCuenta)
        =>
        CrearProveedor(contexto, contexto.SeleccionarPorId<InterlocutorDtm>(idInter), idCuenta);


        public static ProveedorDtm CrearProveedor(ContextoSe contexto, InterlocutorDtm inter, int idCuenta)
        {
            var Proveedor = contexto.SeleccionarPorFk<ProveedorDtm>(nameof(ProveedorDtm.IdInterlocutor), inter.Id, errorSiNoHay: false);

            if (Proveedor != null)
                return Proveedor;

            if (inter.Baja)
                GestorDeErrores.Emitir($"No se puede dar de alta un Proveedor por estar de baja el interlocutor");

            var proveedorBd = contexto.SeleccionarPorNombre<ProveedorDtm>(inter.Nombre, errorSiNoHay: false, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });
            if (proveedorBd != null)
            {
                GestorDeErrores.Emitir($"No se puede crear el proveedor '{inter.Expresion}' ya que ya hay uno en la BD '{proveedorBd.Referencia(contexto)}' con el mismo nombre");
            }

            if (Proveedor == null)
            {
                Proveedor = new ProveedorDtm();
                Proveedor.IdInterlocutor = inter.Id;
                Proveedor.Nombre = inter.Expresion;
                Proveedor.Telefono = inter.Telefono;
                Proveedor.eMail = inter.eMail;
                Proveedor.IdCuenta = idCuenta;
                Proveedor = Proveedor.Insertar(contexto);
            }

            return Proveedor;
        }

        public static CuentaDeMiSociedadDtm DomiciliadaEn(this ProveedorDtm proveedor, ContextoSe contexto)
        {
            if (proveedor.DomiciliadaEn is not null && proveedor.IdDomiciliadaEn == proveedor.DomiciliadaEn.Id)
                return proveedor.DomiciliadaEn;

            if (proveedor.IdDomiciliadaEn is null)
                return null;
           
            proveedor.DomiciliadaEn = contexto.SeleccionarPorId<CuentaDeMiSociedadDtm>((int)proveedor.IdDomiciliadaEn, aplicarJoin: true);

            return proveedor.DomiciliadaEn;
        }

        public static TarjetaDeMiSociedadDtm Tarjeta(this ProveedorDtm proveedor, ContextoSe contexto)
        {
            if (proveedor.Tarjeta is not null && proveedor.IdTarjeta == proveedor.Tarjeta.Id)
                return proveedor.Tarjeta;

            if (proveedor.IdTarjeta is null)
                return null;

            proveedor.Tarjeta = contexto.SeleccionarPorId<TarjetaDeMiSociedadDtm>((int)proveedor.IdTarjeta, aplicarJoin: true);

            return proveedor.Tarjeta;
        }
        public static void IndicarQueEstaDeBaja(this ProveedorDtm proveedor, ContextoSe contexto)
        =>
        GestorDeErrores.Emitir($"El proveedor '{proveedor.Referencia(contexto)}' está de baja");

        public static bool EsIntraComunitario(this ProveedorDtm proveedor, ContextoSe contexto)
        =>
        ApiDeTerceros.ClaseDeNacionalidad(proveedor.NIF(contexto)) == enumClaseDeNacionalidad.Intracomunitario;
        public static bool EsExtraComunitario(this ProveedorDtm proveedor, ContextoSe contexto)
        =>
        ApiDeTerceros.ClaseDeNacionalidad(proveedor.NIF(contexto)) == enumClaseDeNacionalidad.Extracomunitario;
    }
}
