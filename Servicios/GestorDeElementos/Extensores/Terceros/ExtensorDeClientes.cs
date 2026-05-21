using Gestor.Errores;
using ModeloDeDto.Negocio;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{

    public static class ExtensorDeClientes
    {
        public static IQueryable<VinculoDtm> Clientes(this enumNegocio negocio, ContextoSe contexto)
        {
            throw new Exception($"Se debe indicar como obtener los clientes vinculados al negocio: {negocio}");
        }

        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(CuentaDeClienteDtm))
                return true;

            return false;
        }

        public static T InicializarDatosCliente<T>(this T objeto, ContextoSe contexto, ParametrosDeNegocio parametros)
        where T : IUsaCliente
        {

            if (parametros.Modificando && !objeto.Contacto.IsNullOrEmpty())
                return objeto;

            if (parametros.Eliminando)
                return objeto;

            var cliente = objeto.Cliente is null ? contexto.SeleccionarPorId<ClienteDtm>(objeto.IdCliente, aplicarJoin: true) : objeto.Cliente;
            parametros.Parametros[nameof(ClienteDtm)] = cliente;
            objeto.Contacto = objeto.Contacto.IsNullOrEmpty() ? cliente.Expresion : objeto.Contacto;
            objeto.Telefono = objeto.Telefono.IsNullOrEmpty() ? cliente.Telefono : objeto.Telefono;
            objeto.eMail = objeto.eMail.IsNullOrEmpty() ? cliente.eMail : objeto.eMail;

            return objeto;
        }

        public static T CambiarDatosCliente<T>(this T objeto, ContextoSe contexto)
        where T : IUsaCliente
        {
            var cliente = contexto.SeleccionarPorId<ClienteDtm>(objeto.IdCliente, aplicarJoin: true);
            objeto.Contacto = cliente.Expresion;
            objeto.Telefono = cliente.Telefono;
            objeto.eMail = cliente.eMail;

            return objeto;
        }

        public static bool Migrado(this ClienteDtm cliente, ContextoSe contexto, enumSistemaContable sistema, string codigoSociedad)
        {
            var nombreTraza = VariablesDePreasiento.IndicarMigrado(sistema, codigoSociedad);
            return enumNegocio.Cliente.Trazas(contexto).Where(traza => traza.IdElemento == cliente.Id && traza.Nombre == nombreTraza).Any();
        }

        public static TrazaDtm MarcarComoMigrado(this ClienteDtm cliente, ContextoSe contexto, enumSistemaContable sistema, string codigoSociedad)
        {
            var nombreTraza = VariablesDePreasiento.IndicarMigrado(sistema, codigoSociedad);
            return cliente.CrearTraza(contexto, nombreTraza, $"Cliente migrado a la contabilidad de {sistema.Descripcion()}");
        }

        public static void DesmarcarComoMigrado(this ClienteDtm cliente, ContextoSe contexto, enumSistemaContable sistema, string codigoSociedad)
        {
            var nombreTraza = VariablesDePreasiento.IndicarMigrado(sistema, codigoSociedad);
            cliente.QuitarTraza(contexto, nombreTraza);
        }
        public static DireccionDto DireccionFiscal(this ClienteDtm cliente, ContextoSe contexto, bool errorSiNoHay = true)
        =>
        cliente.DireccionDto(contexto, enumCalificadorDireccion.fiscal, errorSiNoHay);

        public static bool EsExtranjero(this ClienteDtm cliente, ContextoSe contexto)
        {
            var dir = cliente.Direccion(contexto, enumCalificadorDireccion.fiscal, true);
            var pais = contexto.SeleccionarPorId<PaisDtm>(dir.IdPais);
            return pais.ISO2 != ltrIsoPaises.Spain;
        }

        private static DireccionDto DireccionDto(this ClienteDtm cliente, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direccion = cliente.Direccion(contexto, calificador, errorSiNoHay);
            return direccion is null ? null : direccion.MapearDto(contexto, direccion.Negocio);
        }

        public static DireccionDtm Direccion(this ClienteDtm cliente, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Cliente, cliente.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion is not null)
            {
                direccion.Negocio = enumNegocio.Cliente;
                return direccion;
            }
            return cliente.Interlocutor(contexto).Direccion(contexto, calificador, errorSiNoHay);
        }


        //public static string Referencia(this ClienteDtm cliente, ContextoSe contexto)
        //{
        //    if (cliente.Interlocutor != null)
        //        return cliente.Interlocutor.Referencia(contexto);

        //    return contexto.SeleccionarPorId<InterlocutorDtm>(cliente.IdInterlocutor, aplicarJoin: true).Referencia(contexto);
        //}

        //public static InterlocutorDtm Interlocutor(this ClienteDtm cliente, ContextoSe contexto)
        //=> cliente.Interlocutor ??= contexto.SeleccionarPorId<InterlocutorDtm>(cliente.IdInterlocutor, aplicarJoin: true);

        public static string NIF(this ClienteDtm cliente, ContextoSe contexto, bool quitarPrefijoEs = false)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ter_NifDeCliente);
            if (!cache.ContainsKey(cliente.Id.ToString()))
                cache[cliente.Id.ToString()] = cliente.VAT.IsNullOrEmpty() ? cliente.Interlocutor(contexto).NIF(contexto, quitarPrefijoEs) : cliente.VAT.ToUpper();

            var nif = (string)cache[cliente.Id.ToString()];

            if (quitarPrefijoEs)
            {
                nif = SociedadDtm.QuitarIsoDeNif(nif);
            }
            else
            {   
                nif = SociedadDtm.PonerIsoDeNif(nif);
            }


            return nif;
        }

        public static string RazonSocial(this ClienteDtm cliente, ContextoSe contexto) => cliente.Interlocutor(contexto).RazonSocial(contexto);

        public static CuentaDeClienteDtm CuentaDeCliente(this ClienteDtm cliente, ContextoSe contexto, enumClaseDeCuentaBancaria clase, bool errorSiNoHay = true)
        {
            var cuentas = cliente.Detalles<CuentaDeClienteDtm>(contexto);

            var cuentasActivas = clase != enumClaseDeCuentaBancaria.Ambas
            ? cuentas.Where(x => x.Activa && x.IdArchivo != null && (x.Clase == enumClaseDeCuentaBancaria.Ambas || x.Clase == clase)).ToList()
            : cuentas.Where(x => x.Activa && x.IdArchivo != null).ToList();

            if (errorSiNoHay && cuentasActivas.Count() == 0)
                GestorDeErrores.Emitir($"El cliente '{cliente.NIF(contexto)}' no tiene ninguna cuenta bancaria activa o le falta el certificado");

            if (cuentasActivas.Count() > 1)
                GestorDeErrores.Emitir($"El cliente '{cliente.NIF(contexto)}' tiene más de una cuenta bancaria activa");

            return cuentasActivas.Count == 0 ? null : cuentasActivas[0];
        }

        public static ArchivoDtm CertificadoDeCuenta(this CuentaDeClienteDtm cc, ContextoSe contexto, bool erroSiNoHay = true)
        {
            if (erroSiNoHay) cc.ValidarElCertificado(contexto);
            if (cc.IdArchivo is null) return null;
            return contexto.SeleccionarPorId<ArchivoDtm>(cc.IdArchivo.Entero());
        }

        public static bool TieneFacturas(this ClienteDtm cliente, ContextoSe contexto) => contexto.Set<FacturaEmtDtm>().Any(factura => factura.IdCliente == cliente.Id);
        public static void ValidarElCertificado(this CuentaDeClienteDtm cc, ContextoSe contexto)
        {
            if (cc.IdArchivo is null)
                GestorDeErrores.Emitir($"La cuenta '({cc.Alias}) {cc.CuentaBancaria(contexto).NumeroIban}' del cliente '{((ClienteDtm)cc.Elemento(contexto)).NIF(contexto)}' no tiene el certificado de cuenta");

            if (!cc.Activa)
                GestorDeErrores.Emitir($"La cuenta '({cc.Alias}) {cc.CuentaBancaria(contexto).NumeroIban}' del cliente '{((ClienteDtm)cc.Elemento(contexto)).NIF(contexto)}' no está activa");

        }

        public static CuentaBancariaDtm CuentaBancaria(this CuentaDeClienteDtm cc, ContextoSe contexto)
        => cc.Cuenta == null
        ? contexto.SeleccionarPorId<CuentaBancariaDtm>(cc.IdCuenta)
        : cc.Cuenta;

        public static string CodigoDeCtaContable(this ClienteDtm cliente, ContextoSe contexto)
        {
            var interlocutor = cliente.Interlocutor(contexto);
            return cliente.CuentaContable(contexto).Codigo + cliente.CodigoContable.Entero().ToString().PadLeft(4, '0');
        }

        public static UsuarioDtm Usuario(this UsuarioDeClienteDtm usuarioDeCliente, ContextoSe contexto)
        => usuarioDeCliente.Usuario == null
        ? contexto.SeleccionarPorId<UsuarioDtm>(usuarioDeCliente.IdUsuario)
        : usuarioDeCliente.Usuario;

        public static PuestoDtm Puesto(this PuestoDeClienteDtm puestoDeCliente, ContextoSe contexto)
        => puestoDeCliente.Puesto == null
        ? contexto.SeleccionarPorId<PuestoDtm>(puestoDeCliente.IdPuesto)
        : puestoDeCliente.Puesto;

        public static ClienteDtm Cliente(this IUsaCliente elemento, ContextoSe contexto, bool aplicarJoin = false)
        =>
        elemento.Cliente != null ? elemento.Cliente : contexto.SeleccionarPorId<ClienteDtm>(elemento.IdCliente, aplicarJoin);

        public static string NombreDeArchivadorClientes(this ClienteDtm cliente, ContextoSe contexto) => $"Datos iniciales del cliente '{cliente.Referencia(contexto)}'";

        public static ArchivadorDtm AsociarArchivador(this ClienteDtm cliente, ContextoSe contexto, int idCg, int idTipoArchivador, bool crearPermisos = true)
        {
            ArchivadorDtm archivador = new ArchivadorDtm
            {
                IdCg = idCg,
                IdTipo = idTipoArchivador,
                Nombre = cliente.NombreDeArchivadorClientes(contexto),
                Descripcion = $"Archivador del cliente: {cliente.Referencia(contexto)}"
            }.
            InsertarComoAdministrador(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.CrearPermisosDelElemento, crearPermisos } }, accionEjecutada: nameof(AsociarArchivador));
            cliente.Vincular(contexto, archivador, parametros: new Dictionary<string, object> { { ltrCliente.OtorgarPermisos, true } });
            return archivador;
        }

        public static ClienteDtm Cliente(this UsuarioDeClienteDtm usuarioDeCliente, ContextoSe contexto, bool aplicarJoin = false)
        => usuarioDeCliente.Elemento == null
        ? (ClienteDtm)usuarioDeCliente.DetalleDe(contexto, aplicarJoin)
        : usuarioDeCliente.Elemento;

        public static CentroGestorDtm LeerCentroGestorDeClientes(ContextoSe contexto, bool errorSiNoEstaDefinido)
        {
            var parametroCgCliente = enumNegocio.Cliente.LeerCrearParametro(contexto, enumParametrosDeCliente.CLI_CG_De_Cliente, valor: "");
            if (parametroCgCliente.Valor.IsNullOrEmpty() && errorSiNoEstaDefinido)
                GestorDeErrores.Emitir($"Debe indicar el código del CG donde crear los archivadores de un cliente en el parámetro '{enumParametrosDeCliente.CLI_CG_De_Cliente}'");

            var cg = contexto.SeleccionarPorPropiedad<CentroGestorDtm>(nameof(CentroGestorDtm.Codigo), parametroCgCliente.Valor, errorSiNoHay: false);
            if (cg is null) GestorDeErrores.Emitir($"El CG del archivador '{parametroCgCliente.Valor}' definido en el parámetro '{enumParametrosDeCliente.CLI_CG_De_Cliente}' no se ha localizado");
            return cg;
        }

        public static TipoDeArchivadorDtm LeerTipoDeArchivadorDeClientes(ContextoSe contexto, bool errorSiNoEstaDefinido)
        {
            var parametroTipoArchivador = enumNegocio.Cliente.LeerCrearParametro(contexto, enumParametrosDeCliente.CLI_TipoArchivador, valor: "");
            if (parametroTipoArchivador.Valor.IsNullOrEmpty() && errorSiNoEstaDefinido)
                GestorDeErrores.Emitir(msgCliente.FaltaParametroTipoArchivador);


            var tipo = contexto.SeleccionarPorNombre<TipoDeArchivadorDtm>(parametroTipoArchivador.Valor, errorSiNoHay: false);
            if (tipo is null) GestorDeErrores.Emitir(msgCliente.FaltaParametroCg);
            return tipo;
        }

        public static bool HayUsuariosDeClientes(ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Dtm_HayRegistros);
            if (!cache.ContainsKey(typeof(UsuarioDeClienteDtm).Name))
            {
                cache[typeof(UsuarioDeClienteDtm).Name] = contexto.Set<UsuarioDeClienteDtm>().Any(x => true);
            }
            return (bool)cache[typeof(UsuarioDeClienteDtm).Name];
        }
    }
}
