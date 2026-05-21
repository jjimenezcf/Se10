using Gestor.Errores;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Juridico;
using ServicioDeDatos.RegistroEs;
using ServicioDeDatos.Tarea;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestorDeElementos.Extensores
{

    public static class ExtensorDeInterlocutores
    {
        public static bool UsaElDetalleDe(Type tipoDeDetalle)
        {
            if (tipoDeDetalle == typeof(CuentaDeInterlocutorDtm))
                return true;

            return false;
        }
        public static IQueryable<VinculoDtm> Interlocutores(this enumNegocio negocio, ContextoSe contexto)
        {
            switch (negocio)
            {
                case enumNegocio.Registro:
                    return contexto.Set<InterlocutoresDeUnRegistroDtm>();
                case enumNegocio.Tarea:
                    return contexto.Set<InterlocutoresDeUnaTareaDtm>();
                case enumNegocio.Expediente:
                    return contexto.Set<InterlocutoresDeUnExpedienteDtm>();
                case enumNegocio.Pleito:
                    return contexto.Set<InterlocutoresDeUnPleitoDtm>();
                case enumNegocio.Contrato:
                    return contexto.Set<InterlocutoresDeUnContratoDtm>();
            }

            throw new Exception($"Se debe indicar como obtener los interlocutores vinculados al negocio: {negocio}");
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static ProcuradorDto CrearProcurador(this InterlocutorDto inter, ContextoSe contexto) => inter.CrearProcurador(contexto, inter.Nombre, inter.eMail, inter.Telefono);
        private static ProcuradorDto CrearProcurador(this InterlocutorDto inter, ContextoSe contexto, string nombre, string email, string telefono)
        {
            var abogado = CrearProcurador(contexto, inter.Id, nombre, email, telefono);
            return abogado.MapearDto<ProcuradorDto>(contexto);
        }
        private static ProcuradorDtm CrearProcurador(ContextoSe contexto, int idInterlocutor, string nombre, string email, string telefono)
        {
            var clave = new Dictionary<string, object>();
            clave.Add(nameof(ProcuradorDtm.IdInterlocutor), idInterlocutor);
            clave.Add(nameof(ProcuradorDtm.Nombre), nombre);
            var procurador = contexto.SeleccionarPorAk<ProcuradorDtm>(clave, errorSiNoHay: false);
            if (procurador == null)
            {
                procurador = new ProcuradorDtm();
                procurador.Nombre = nombre;
                procurador.IdInterlocutor = idInterlocutor;
                procurador.eMail = email;
                procurador.Telefono = telefono;
                procurador = procurador.Insertar(contexto);
            }
            return procurador;
        }

        public static AbogadoDto CrearAbogado(this InterlocutorDto inter, ContextoSe contexto) => inter.CrearAbogado(contexto, inter.Nombre, inter.eMail, inter.Telefono);
        private static AbogadoDto CrearAbogado(this InterlocutorDto inter, ContextoSe contexto, string nombre, string email, string telefono)
        {
            var abogado = CrearAbogado(contexto, inter.Id, nombre, email, telefono);
            return abogado.MapearDto<AbogadoDto>(contexto);
        }
        private static AbogadoDtm CrearAbogado(ContextoSe contexto, int idInterlocutor, string nombre, string email, string telefono)
        {
            var clave = new Dictionary<string, object>();
            clave.Add(nameof(AbogadoDtm.IdInterlocutor), idInterlocutor);
            clave.Add(nameof(AbogadoDtm.Nombre), nombre);
            var abogado = contexto.SeleccionarPorAk<AbogadoDtm>(clave, errorSiNoHay: false);
            if (abogado == null)
            {
                abogado = new AbogadoDtm();
                abogado.Nombre = nombre;
                abogado.IdInterlocutor = idInterlocutor;
                abogado.eMail = email;
                abogado.Telefono = telefono;
                abogado = abogado.Insertar(contexto);
            }
            return abogado;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static ProveedorDtm Proveedor(this InterlocutorDtm interlocutor, ContextoSe contexto, bool crearProveedor = false)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Int_Proveedor);
            if (cache.ContainsKey(interlocutor.Id.ToString()))
            {
                return (ProveedorDtm)cache[interlocutor.Id.ToString()];
            }

            var proveedor = contexto.SeleccionarPorAk<ProveedorDtm>(new Dictionary<string, object> { { nameof(ProveedorDtm.IdInterlocutor), interlocutor.Id } }, errorSiNoHay: false);
            if (proveedor is null && crearProveedor)
            {
                var cuenta = contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Proveedores);
                proveedor = CrearProveedor(interlocutor, contexto, cuenta.Id);
            }
            cache[interlocutor.Id.ToString()] = proveedor;
            return proveedor;
        }

        public static ProveedorDto CrearProveedor(this InterlocutorDto inter, ContextoSe contexto, int idCuenta) => inter.CrearProveedor(contexto, inter.Nombre, inter.eMail, inter.Telefono, idCuenta);
        public static ProveedorDtm CrearProveedor(this InterlocutorDtm inter, ContextoSe contexto, int idCuenta) => CrearProveedor(contexto, inter.Id, inter.Nombre, inter.eMail, inter.Telefono, idCuenta);
        private static ProveedorDto CrearProveedor(this InterlocutorDto inter, ContextoSe contexto, string nombre, string email, string telefono, int idCuenta)
        {
            var proveedor = CrearProveedor(contexto, inter.Id, nombre, email, telefono, idCuenta);
            return proveedor.MapearDto<ProveedorDto>(contexto);
        }
        private static ProveedorDtm CrearProveedor(ContextoSe contexto, int idInterlocutor, string nombre, string email, string telefono, int idCuenta)
        {
            var clave = new Dictionary<string, object>();
            clave.Add(nameof(ProveedorDtm.IdInterlocutor), idInterlocutor);
            clave.Add(nameof(ProveedorDtm.Nombre), nombre);
            var proveedor = contexto.SeleccionarPorAk<ProveedorDtm>(clave, errorSiNoHay: false);
            if (proveedor == null)
            {
                proveedor = new ProveedorDtm();
                proveedor.Nombre = nombre;
                proveedor.IdInterlocutor = idInterlocutor;
                proveedor.eMail = email;
                proveedor.Telefono = telefono;
                proveedor.IdCuenta = idCuenta;
                proveedor = proveedor.Insertar(contexto);
            }
            return proveedor;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static ClienteDtm Cliente(this IUsaSolicitante elemento, ContextoSe contexto, bool crearCliente = true)
        =>
        elemento.Solicitante(contexto).Cliente(contexto, crearCliente);

        public static ClienteDtm Cliente(this InterlocutorDtm interlocutor, ContextoSe contexto, bool crearCliente = false)
        {

            if (interlocutor.EsContacto)
            {
                var interSociedad = contexto.Set<InterlocutorDtm>().FirstOrDefault(i => i.IdSociedad == interlocutor.IdSociedad && i.Contacto == null);

                if (interSociedad is null)
                    interSociedad = new InterlocutorDtm
                    {
                        IdSociedad = interlocutor.IdSociedad,
                        IdContacto = null,
                        IdPersona = null
                    }.InsertarComoAdministrador(contexto);

                return interSociedad.Cliente(contexto);
            }

            var cliente = contexto.SeleccionarPorAk<ClienteDtm>(new Dictionary<string, object> { { nameof(ClienteDtm.IdInterlocutor), interlocutor.Id } }, errorSiNoHay: false);
            if (cliente is null && crearCliente)
            {
                return interlocutor.CrearCliente(contexto);
            }
            return cliente;
        }

        public static ClienteDtm CrearCliente(this InterlocutorDtm interlocutor, ContextoSe contexto)
        {
            var cuenta = contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Clientes);
            return CrearCliente(interlocutor, contexto, cuenta.Id);
        }

        public static bool EsCliente(this InterlocutorDtm interlocutor, ContextoSe contexto)
        =>
        contexto.Set<ClienteDtm>().Any(cliente => cliente.IdInterlocutor == interlocutor.Id);

        public static ClienteDto CrearCliente(this InterlocutorDto interDto, ContextoSe contexto, int idCuenta) => interDto.CrearCliente(contexto, interDto.Nombre, interDto.eMail, interDto.Telefono, idCuenta);
        public static ClienteDtm CrearCliente(this InterlocutorDtm interDtm, ContextoSe contexto, int idCuenta) => CrearCliente(contexto, interDtm.Id, interDtm.Nombre, interDtm.eMail, interDtm.Telefono, idCuenta);
        private static ClienteDto CrearCliente(this InterlocutorDto inter, ContextoSe contexto, string nombre, string email, string telefono, int idCuenta)
        {
            var cliente = CrearCliente(contexto, inter.Id, nombre, email, telefono, idCuenta);
            return cliente.MapearDto<ClienteDto>(contexto);
        }
        private static ClienteDtm CrearCliente(ContextoSe contexto, int idInterlocutor, string nombre, string email, string telefono, int idCuenta)
        {
            var clave = new Dictionary<string, object>
            {
                { nameof(ClienteDtm.IdInterlocutor), idInterlocutor },
                { nameof(ClienteDtm.Nombre), nombre }
            };
            var cliente = contexto.SeleccionarPorAk<ClienteDtm>(clave, errorSiNoHay: false);
            if (cliente == null)
            {
                cliente = new ClienteDtm();
                cliente.Nombre = nombre;
                cliente.IdInterlocutor = idInterlocutor;
                cliente.eMail = email;
                cliente.Telefono = telefono;
                cliente.IdCuenta = idCuenta;
                cliente = cliente.Insertar(contexto);
            }
            return cliente;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static TrabajadorDtm CrearTrabajador(this InterlocutorDtm inter, ContextoSe contexto, CentroGestorDtm cg, int idCuenta, int? idUsuario)
        =>
        CrearTrabajador(contexto, inter.Id, cg.Id, inter.Nombre, inter.eMail, inter.Telefono, idCuenta, idUsuario);
        private static TrabajadorDtm CrearTrabajador(ContextoSe contexto, int idInterlocutor, int idCg, string nombre, string email, string telefono, int idCuenta, int? idUsuario)
        {
            var clave = new Dictionary<string, object>
            {
                { nameof(TrabajadorDtm.IdInterlocutor), idInterlocutor },
                { nameof(TrabajadorDtm.Nombre), nombre }
            };
            var trabajador = contexto.SeleccionarPorAk<TrabajadorDtm>(clave, errorSiNoHay: false);
            if (trabajador == null)
            {
                trabajador = new TrabajadorDtm();
                trabajador.Nombre = nombre;
                trabajador.IdInterlocutor = idInterlocutor;
                trabajador.eMail = email;
                trabajador.Telefono = telefono;
                trabajador.IdCuenta = idCuenta;
                trabajador.IdUsuario = idUsuario;
                trabajador.IdCg = idCg;
                trabajador = trabajador.Insertar(contexto);
            }
            return trabajador;
        }

        public static TrabajadorDtm Trabajador(this InterlocutorDtm interlocutor, ContextoSe contexto)
        =>
        Trabajador(interlocutor, contexto, null, null, crearTrabajador: false);

        public static TrabajadorDtm Trabajador(this InterlocutorDtm interlocutor, ContextoSe contexto, CentroGestorDtm cg, int? idUsuario, bool crearTrabajador = false)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Int_Trabajador);
            if (cache.ContainsKey(interlocutor.Id.ToString()))
            {
                return (TrabajadorDtm)cache[interlocutor.Id.ToString()];
            }

            var trabajador = contexto.SeleccionarPorAk<TrabajadorDtm>(new Dictionary<string, object> { { nameof(TrabajadorDtm.IdInterlocutor), interlocutor.Id } }, errorSiNoHay: false);
            if (trabajador is null && crearTrabajador)
            {
                var cuenta = contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Sueldos);
                trabajador = CrearTrabajador(interlocutor, contexto, cg, cuenta.Id, idUsuario);
            }
            return trabajador;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        public static string Expresion(this InterlocutorDtm interlocutor, ContextoSe contexto)
        {
            if (interlocutor.EsPersona)
            {
                return interlocutor.Persona(contexto).Expresion;
            }
            return interlocutor.Sociedad(contexto).Expresion;
        }

        public static string Referencia(this InterlocutorDtm interlocutor, ContextoSe contexto)
        {
            if (interlocutor.Sociedad == null && interlocutor.Persona == null)
                interlocutor = contexto.SeleccionarPorId<InterlocutorDtm>(interlocutor.Id, aplicarJoin: true);

            if (interlocutor.Sociedad != null && !interlocutor.EsContacto)
                return interlocutor.Sociedad.Referencia;

            if (interlocutor.Persona != null)
                return interlocutor.Persona.Referencia;

            return interlocutor.Contacto(contexto).Referencia(contexto);
        }

        public static DireccionDto DireccionFiscal(this InterlocutorDtm interlocutor, ContextoSe contexto, bool errorSiNoHay = true)
        =>
        interlocutor.DireccionDto(contexto, enumCalificadorDireccion.fiscal, errorSiNoHay);

        public static DireccionDto DireccionDto(this InterlocutorDtm interlocutor, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var dir = interlocutor.Direccion(contexto, calificador, errorSiNoHay);
            if (dir is null && !errorSiNoHay) return null;

            return dir.MapearDto(contexto, dir.Negocio);
        }

        public static DireccionDtm Direccion(this InterlocutorDtm interlocutor, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Interlocutor, interlocutor.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion == null)
            {
                return !interlocutor.EsPersona
                ? interlocutor.Sociedad(contexto).Direccion(contexto, calificador, errorSiNoHay)
                : interlocutor.Persona(contexto).Direccion(contexto, calificador, errorSiNoHay);
            }
            direccion.Negocio = enumNegocio.Interlocutor;
            return direccion;
        }

        public static DireccionDtm Direccion(this InterlocutorDtm interlocutor, ContextoSe contexto,  bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Interlocutor, interlocutor.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.contacto && x.Activo);
            if (direccion == null)
            {
               direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.fiscal && x.Activo);
            }
            if (direccion == null)
            {
                direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.correspondencia && x.Activo);
            }

            if (direccion == null)
            {
                return !interlocutor.EsPersona
                ? interlocutor.Sociedad(contexto).Direccion(contexto, errorSiNoHay)
                : interlocutor.Persona(contexto).Direccion(contexto, errorSiNoHay);
            }
            direccion.Negocio = enumNegocio.Interlocutor;
            return direccion;
        }

        public static DireccionDtm CopiarDireccionDelSolicitante(this IUsaSolicitante elemento, ContextoSe contexto, enumCalificadorDireccion calificador)
        {
            var solicitante = elemento.Solicitante(contexto);
            var direccion = solicitante.Direccion(contexto, calificador);
            if (direccion is not null)
                direccion = ((IElementoDtm)elemento).AsignarDireccion(contexto, direccion);
            return direccion;
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public static InterlocutorDtm CrearInterlocutor(ContextoSe contexto, string nif, bool puedeSerAutonomo  = false)
        {
            if (ApiDeTerceros.CifValido(nif) || (ApiDeTerceros.ValidarNif(nif).IsNullOrEmpty() && puedeSerAutonomo))
            {
                var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nif);
                return enumNegocio.Sociedad.CrearInterlocutores(contexto, new List<int> { sociedad.Id })[0];
            }

            if (ApiDeTerceros.ValidarNif(nif).IsNullOrEmpty() || ApiDeTerceros.ValidarNie(nif).IsNullOrEmpty())
            {
                var persona = contexto.SeleccionarPorPropiedad<PersonaDtm>(nameof(PersonaDtm.NIF), nif);
                return enumNegocio.Persona.CrearInterlocutores(contexto, new List<int> { persona.Id })[0];
            }

            throw new Exception($"El {nif} no es válido");
        }

        public static List<InterlocutorDtm> CrearInterlocutores(this enumNegocio negocio, ContextoSe contexto, List<int> ids)
        {
            var filtroPorId = new ClausulaDeFiltrado();
            var excluirContactos = new ClausulaDeFiltrado();
            var filtros = new List<ClausulaDeFiltrado> { filtroPorId };

            switch (negocio)
            {
                case enumNegocio.Sociedad:
                    filtroPorId.Clausula = nameof(InterlocutorDtm.IdSociedad);
                    excluirContactos.Clausula = nameof(InterlocutorDtm.IdContacto);
                    excluirContactos.Criterio = enumCriteriosDeFiltrado.esNulo;
                    filtros.Add(excluirContactos);
                    break;
                case enumNegocio.Persona:
                    filtroPorId.Clausula = nameof(InterlocutorDtm.IdPersona);
                    break;
                case enumNegocio.Contacto:
                    filtroPorId.Clausula = nameof(InterlocutorDtm.IdContacto);
                    break;
                default:
                    throw new Exception($"Del negocio {negocio.ToNombre()} no se puede crear un interlocutor");
            }
            filtroPorId.Criterio = enumCriteriosDeFiltrado.igual;

            var lista = new List<InterlocutorDtm>();
            foreach (var id in ids)
            {
                filtroPorId.Valor = id.ToString();
                var interDtm = enumNegocio.Interlocutor.SeleccionarPorFiltro<InterlocutorDtm>(contexto, filtros, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, false } });
                if (interDtm.Count() > 0)
                {
                    lista.Add(interDtm[0]);
                    continue;
                }

                var tercero = negocio.LeerTercero(contexto, id);
                lista.Add(tercero.CrearInterlocutor(contexto));
            }
            return lista;
        }

        public static SociedadDtm Sociedad(this InterlocutorDtm interlocutor, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (interlocutor.Sociedad != null && interlocutor.IdSociedad == interlocutor.Sociedad.Id)
            {
                return interlocutor.Sociedad;
            }

            if (interlocutor.IdSociedad is null)
            {
                if (errorSiNoHay) GestorDeErrores.Emitir($"El interlocutor '{interlocutor.Expresion}' no es una sociedad");
                return null;
            }

            var cache = ServicioDeCaches.Obtener(CacheDe.Int_Sociedad);
            var clave = interlocutor.IdSociedad.ToString();
            if (!cache.ContainsKey(clave))
            {
                cache[clave] = contexto.SeleccionarPorId<SociedadDtm>((int)interlocutor.IdSociedad, errorSiNoHay: errorSiNoHay, aplicarJoin: true);
            }
            return (SociedadDtm)cache[clave];        }

        public static PersonaDtm Persona(this InterlocutorDtm interlocutor, ContextoSe contexto, bool errorSiNoHay = true)
        {
            if (interlocutor.Persona != null && interlocutor.IdPersona == interlocutor.Persona.Id)
            {
                return interlocutor.Persona;
            }

            if (interlocutor.IdPersona is null)
            {
                if (errorSiNoHay) GestorDeErrores.Emitir($"El interlocutor '{interlocutor.Expresion}' no es una persona");
                return null;
            }

            var cache = ServicioDeCaches.Obtener(CacheDe.Int_Persona);
            var clave = interlocutor.IdPersona.ToString();
            if (!cache.ContainsKey(clave))
            {
                cache[clave] = contexto.SeleccionarPorId<PersonaDtm>((int)interlocutor.IdPersona, errorSiNoHay: errorSiNoHay, aplicarJoin: true);
            }
            return (PersonaDtm)cache[clave];        }

        public static string NIF(this InterlocutorDtm interlocutor, ContextoSe contexto, bool quitarPrefijoEs = false)
        {
            if (interlocutor.EsPersona)
            {
                return quitarPrefijoEs ? interlocutor.Persona(contexto).NIFSinIsoEs: interlocutor.Persona(contexto).NIFConIsoEs;
            }
            return quitarPrefijoEs ? interlocutor.Sociedad(contexto).NIFSinIsoEs : interlocutor.Sociedad(contexto).NIFConIsoEs;
        }

        public static string Nombre(this InterlocutorDtm interlocutor, ContextoSe contexto)
        {
            if (interlocutor.EsPersona)
            {
                var persona = interlocutor.Persona(contexto);
                return persona.Apellidos + ", " + persona.Nombre;
            }
            if (interlocutor.EsContacto)
            {
                var contacto = interlocutor.Contacto(contexto);
                return contacto.Nombre;
            }
            var sociedad = interlocutor.Sociedad(contexto);
            return sociedad.RazonSocial;
        }

        public static string RazonSocial(this InterlocutorDtm interlocutor, ContextoSe contexto)
        {
            if (interlocutor.EsPersona)
            {
                return interlocutor.Persona(contexto).Apellidos + ", " + interlocutor.Persona(contexto).Nombre;
            }
            return interlocutor.Sociedad(contexto).RazonSocial;
        }

        public static InterlocutorDtm Solicitante(this IUsaSolicitante elemento, ContextoSe contexto)
        {
            if (elemento.Solicitante is not null && elemento.IdSolicitante == elemento.Solicitante.Id)
                return elemento.Solicitante;

            elemento.Solicitante = contexto.SeleccionarPorId<InterlocutorDtm>(elemento.IdSolicitante);
            return elemento.Solicitante;
        }


        public static void ValidarQueEstaEnAlta(this InterlocutorDtm solicitante, ContextoSe contexto)
        {
            if (solicitante.Baja) GestorDeErrores.Emitir($"El solicitante '{solicitante.Referencia(contexto)}' está de baja");
        }

        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public static CuentaDeInterlocutorDtm AsociarCuenta(this InterlocutorDtm inter, ContextoSe contexto, string alias, enumClaseDeCuentaBancaria clase, CuentaBancariaDtm cuenta)
        {
            var ci = inter.CuentaDeInterlocutor(contexto, clase, errorSiNoHay: false);
            if (ci is not null)
            {
                if (ci.IdCuenta == cuenta.Id && ci.Alias == alias) return ci;
                if (ci.IdCuenta == cuenta.Id && ci.Alias != alias)
                {
                    ci.Alias = alias;
                    ci.Clase = clase;
                    return ci.Modificar(contexto);
                };
                ci.Activa = false;
                ci.Modificar(contexto);
            }

            return new CuentaDeInterlocutorDtm
            {
                Activa = true,
                Alias = alias,
                IdCuenta = cuenta.Id,
                IdElemento = inter.Id,
                Clase = clase
            }.Insertar(contexto);
        }

        public static CuentaDeInterlocutorDtm CuentaDeInterlocutor(this InterlocutorDtm interlocutor, ContextoSe contexto, enumClaseDeCuentaBancaria clase, bool errorSiNoHay = true)
        {
            var cuentas = interlocutor.Detalles<CuentaDeInterlocutorDtm>(contexto, aplicarJoin: true);

            var cuentasActivas = clase != enumClaseDeCuentaBancaria.Ambas
            ? cuentas.Where(x => x.Activa && (x.Clase == enumClaseDeCuentaBancaria.Ambas || x.Clase == clase)).ToList()
            : cuentas.Where(x => x.Activa).ToList();

            if (cuentasActivas.Count() == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"El interlocutor '{interlocutor.NIF(contexto)}' no tiene ninguna cuenta bancaria activa o le falta el certificado");

            if (cuentasActivas.Count() > 1)
                GestorDeErrores.Emitir($"El interlocutor '{interlocutor.NIF(contexto)}' tiene más de una cuenta bancaria activa");

            return cuentasActivas.Count() == 0 ? null : cuentasActivas[0];
        }

        public static CuentaDeAcreedorDto MapearCuentaDeTercero(this CuentaDeAcreedorDto cuenta, ContextoSe contexto, IUsaCuentaBancaria cbDeTercero)
        {
            var cb = cbDeTercero.Cuenta(contexto);
            cuenta.Activa = true;
            cuenta.Iban = cb.IsoPais + cb.DcIban;
            cuenta.Entidad = cb.Entidad;
            cuenta.Oficina = cb.Oficina;
            cuenta.DcCcc = cb.DcCcc;
            cuenta.Numero = cb.Numero;
            cuenta.Banco = cb.Banco(contexto).Nombre;
            return cuenta;
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        private static IEsUnTercero LeerTercero(this enumNegocio negocio, ContextoSe contexto, int id)
        {
            switch (negocio)
            {
                case enumNegocio.Sociedad:
                    return contexto.SeleccionarPorId<SociedadDtm>(id);
                case enumNegocio.Persona:
                    return contexto.SeleccionarPorId<PersonaDtm>(id);
                case enumNegocio.Contacto:
                    return contexto.SeleccionarPorId<ContactoDtm>(id, aplicarJoin: true);
            }
            throw new Exception($"Del negocio '{negocio.ToNombre()}' no se puede crear un interlocutor");
        }

        private static InterlocutorDtm CrearInterlocutor(this IEsUnTercero tercero, ContextoSe contexto)
        {
            if (tercero.GetType() == typeof(PersonaDtm))
                return ((PersonaDtm)tercero).CrearInterlocutor(contexto, errorSihay: false);

            if (tercero.GetType() == typeof(SociedadDtm))
                return ((SociedadDtm)tercero).CrearInterlocutor(contexto, errorSihay: false);


            return ((ContactoDtm)tercero).CrearInterlocutor(contexto, errorSihay: false);
        }

        public static ContactoDtm Contacto(this InterlocutorDtm interlocutor, ContextoSe contexto, bool erroSiNoHay = true)
        {
            if (!interlocutor.EsContacto && erroSiNoHay)
                GestorDeErrores.Emitir($"El interlocutor '{interlocutor.Expresion(contexto)}', no es un contacto");

            return contexto.SeleccionarPorId<ContactoDtm>((int)interlocutor.IdContacto, errorSiNoHay: erroSiNoHay, aplicarJoin: true);
        }


        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------
        //-------------------------------------------------------------------------------------------------------------------------------------------------------------------------

        public static bool TieneFacturas(this InterlocutorDtm interlocutor, ContextoSe contexto)
        {
            if (!interlocutor.EsCliente(contexto))
                return false;
            return interlocutor.Cliente(contexto).TieneFacturas(contexto);
        }


        public static void SincronizarConTerceros(this InterlocutorDtm interlocutor, ContextoSe contexto, InterlocutorDtm anterior)
        {
            if (interlocutor.EsContacto)
                return;

            if (interlocutor.Expresion == anterior.Expresion && interlocutor.eMail == anterior.eMail && interlocutor.Telefono == anterior.Telefono)
                return;

            var cliente = interlocutor.Cliente(contexto);
            if (cliente is not null)
            {
                cliente.Nombre = interlocutor.Expresion;
                if (interlocutor.eMail != anterior.eMail) cliente.eMail = interlocutor.eMail;
                if (interlocutor.Telefono != anterior.Telefono) cliente.Telefono = interlocutor.Telefono;
                cliente.Modificar(contexto);
            }
            var proveedor = interlocutor.Proveedor(contexto);
            if (proveedor is not null)
            {
                proveedor.Nombre = interlocutor.Expresion;
                if (interlocutor.eMail != anterior.eMail) proveedor.eMail = interlocutor.eMail;
                if (interlocutor.Telefono != anterior.Telefono) proveedor.Telefono = interlocutor.Telefono;
                proveedor.Modificar(contexto);
            }
            var trabajador = interlocutor.Trabajador(contexto);
            if (trabajador is not null)
            {
                trabajador.Nombre = interlocutor.Expresion;
                if (interlocutor.eMail != anterior.eMail) trabajador.eMail = interlocutor.eMail;
                if (interlocutor.Telefono != anterior.Telefono) trabajador.Telefono = interlocutor.Telefono;
                trabajador.Modificar(contexto);
            }
        }

        public static void DarDeBajaElInterlocutor<T>(this IEsUnTercero tercero, ContextoSe contexto)
        {
            var filtro = new ClausulaDeFiltrado();
            filtro.Criterio = enumCriteriosDeFiltrado.igual;
            filtro.Valor = ((RegistroDtm)tercero).Id.ToString();
            if (typeof(T) == typeof(PersonaDtm))
                filtro.Clausula = nameof(InterlocutorDtm.IdPersona);
            else if (typeof(T) == typeof(SociedadDtm))
                filtro.Clausula = nameof(InterlocutorDtm.IdSociedad);
            else if (typeof(T) == typeof(ContactoDtm))
                filtro.Clausula = nameof(InterlocutorDtm.IdContacto);

            List<InterlocutorDtm> interlocutores = contexto.SeleccionarTodos<InterlocutorDtm>(new List<ClausulaDeFiltrado> { filtro }, parametros: new Dictionary<string, object>
            {
                {ltrParametrosNeg.ValidarPermisosDeConsulta,false}
            });
            var parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
            foreach (var inter in interlocutores)
            {
                if (inter.Baja) continue;
                inter.Baja = true;
                inter.Modificar(contexto, new Dictionary<string, object> { {ltrParametrosNeg.ValidarPermisosDePersistencia,false} });
            }
        }


    }
}
