using Gestor.Errores;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using OfficeOpenXml.Drawing.Chart.ChartEx;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using Utilidades;
using static Utilidades.Ampliaciones;

namespace GestorDeElementos.Extensores
{


    public static class ExtensorDeSociedades
    {
        public static AgendaDtm CrearAgenda(this SociedadDtm sociedad, ContextoSe contexto)
        {
            var agenda = new AgendaDtm() { Nombre = sociedad.MiAgenda }.InsertarComoAdministrador(contexto);
            sociedad.IdAgenda = agenda.Id;
            sociedad.ModificarComoAdministrador(contexto);
            return agenda;
        }

        public static AgendaDtm ActualizaAgenda(this SociedadDtm sociedad, ContextoSe contexto)
        {
            var agenda = contexto.SeleccionarPorId<AgendaDtm>((int)sociedad.IdAgenda);
            agenda.Nombre = sociedad.MiAgenda;
            agenda.ModificarComoAdministrador(contexto);
            return agenda;
        }

        public static InterlocutorDtm CrearInterlocutor(this SociedadDtm sociedad, ContextoSe contexto, bool errorSihay = true)
        {
            var porSociedad = new ClausulaDeFiltrado(nameof(InterlocutorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id);
            var porContacto = new ClausulaDeFiltrado(nameof(InterlocutorDtm.IdContacto), enumCriteriosDeFiltrado.esNulo);

            var interlocutores = enumNegocio.Interlocutor.SeleccionarPorFiltro<InterlocutorDtm>(contexto, new List<ClausulaDeFiltrado> { porSociedad, porContacto });
            if (interlocutores.Count() == 0)
            {
                return new InterlocutorDtm
                {
                    IdSociedad = sociedad.Id,
                    Nombre = sociedad.Expresion,
                    Baja = false,
                    eMail = sociedad.eMail,
                    Telefono = sociedad.Telefono
                }.Insertar(contexto);
            }

            if (errorSihay)
                GestorDeErrores.Emitir($"ya existe el interlocutor '{interlocutores[0].Referencia(contexto)}' asociado a la sociedad {sociedad.Referencia}");

            return interlocutores[0];
        }

        public static InterlocutorDtm Interlocutor(this SociedadDtm sociedad, ContextoSe contexto, bool crearSiNoLoHay = true, bool errorSiNoHay = true)
        {
            var porSociedad = new ClausulaDeFiltrado(nameof(InterlocutorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id);
            var porContacto = new ClausulaDeFiltrado(nameof(InterlocutorDtm.IdContacto), enumCriteriosDeFiltrado.esNulo);

            var interlocutores = enumNegocio.Interlocutor.SeleccionarPorFiltro<InterlocutorDtm>(contexto, new List<ClausulaDeFiltrado> { porSociedad, porContacto });
            if (interlocutores.Count() == 0 && crearSiNoLoHay)
                return sociedad.CrearInterlocutor(contexto);

            if (interlocutores.Count() == 0 && errorSiNoHay)
                GestorDeErrores.Emitir($"No se ha definido el interlocutor para la sociedad '{sociedad.Referencia}'");

            return interlocutores.Count() == 0 ? null : interlocutores[0];
        }

        public static DireccionDto DireccionFiscal(this SociedadDtm sociedad, ContextoSe contexto)
        =>
        sociedad.DireccionDto(contexto, enumCalificadorDireccion.fiscal, true);

        public static DireccionDto DireccionDto(this SociedadDtm sociedad, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay)
        =>
        sociedad.Direccion(contexto, calificador, errorSiNoHay)?.MapearDto(contexto, enumNegocio.Sociedad);

        public static DireccionDtm Direccion(this SociedadDtm sociedad, ContextoSe contexto, enumCalificadorDireccion calificador, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Sociedad, sociedad.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == calificador && x.Activo);
            if (direccion == null)
            {
                if (errorSiNoHay)
                    GestorDeErrores.Emitir($"La {enumNegocio.Sociedad.Singular(true)} '{sociedad.Referencia}' debe tener una dirección '{calificador.Descripcion()}'");
                return null;
            }
            direccion.Negocio = enumNegocio.Sociedad;
            return direccion;
        }

        public static DireccionDtm Direccion(this SociedadDtm sociedad, ContextoSe contexto, bool errorSiNoHay = false)
        {
            var direcciones = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Sociedad, sociedad.Id).ToList();
            var direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.correspondencia && x.Activo);
            if (direccion is null) direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.contacto && x.Activo);
            if (direccion is null) direccion = direcciones.FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.fiscal && x.Activo);
            if (direccion == null)
            {
                if (errorSiNoHay)
                    GestorDeErrores.Emitir($"La {enumNegocio.Sociedad.Singular(true)} '{sociedad.Referencia}' debe tener alguna dirección, ya sea de {enumCalificadorDireccion.correspondencia.Descripcion()}, {enumCalificadorDireccion.contacto.Descripcion()} o {enumCalificadorDireccion.fiscal.Descripcion()}");
                return null;
            }
            direccion.Negocio = enumNegocio.Sociedad;
            return direccion;
        }

        public static ContactoDto CrearContacto(this SociedadDto sociedad, ContextoSe contexto, string nombre, string email, string telefono, string detalle, bool crearInterlocutor = true)
        {
            var contacto = CrearContacto(contexto, sociedad.Id, nombre, email, telefono, detalle, crearInterlocutor);
            return contacto.MapearDto<ContactoDto, ContactoDtm>(contexto);
        }

        private static ContactoDtm CrearContacto(ContextoSe contexto, int idSociedad, string nombre, string email, string telefono, string detalle, bool crearInterlocutor)
        {
            var clave = new Dictionary<string, object>();
            clave.Add(nameof(ContactoDtm.IdSociedad), idSociedad);
            clave.Add(nameof(ContactoDtm.Nombre), nombre);
            var contacto = contexto.SeleccionarPorAk<ContactoDtm>(clave, errorSiNoHay: false);
            if (contacto == null)
            {
                var parametros = new Dictionary<string, object> { { nameof(ContactoDto.CrearInterlocutor), crearInterlocutor } };
                contacto = new ContactoDtm();
                contacto.Nombre = nombre;
                contacto.Descripcion = detalle;
                contacto.IdSociedad = idSociedad;
                contacto.eMail = email;
                contacto.Telefono = telefono;
                contacto = contacto.Insertar(contexto, parametros);
            }
            return contacto;
        }

        public static CentroGestorDtm CrearCg(this SociedadDtm sociedad, ContextoSe contexto, string nombreCg)
        {
            var f1 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id);
            var f2 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.Nombre), enumCriteriosDeFiltrado.igual, nombreCg);
            var cgs = enumNegocio.CentroGestor.SeleccionarPorFiltro<CentroGestorDtm>(contexto, new List<ClausulaDeFiltrado> { f1, f2 }, true);
            if (cgs.Count == 0)
            {
                var cg = new CentroGestorDtm();
                cg.Nombre = nombreCg;
                cg.Codigo = sociedad.CodigoFiscal;
                cg.eMail = sociedad.eMail;
                cg.Sigla = sociedad.Nombre.Substring(0, 3).ToUpper();
                cg.IdSociedad = sociedad.Id;
                cg.IdResponsable = sociedad.IdUsuaCrea;
                cgs.Add(cg.Insertar(contexto));
                cgs[0].Sociedad = sociedad;
            }
            return cgs[0];
        }

        public static List<CentroGestorDtm> CentrosGestores(this SociedadDtm sociedad, ContextoSe contexto, bool SoloPadres)
        {
            var f1 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id);
            var f2 = new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdCgPadre), enumCriteriosDeFiltrado.esNulo, null);

            var cgs = SoloPadres
            ? enumNegocio.CentroGestor.SeleccionarPorFiltro<CentroGestorDtm>(contexto, new List<ClausulaDeFiltrado> { f1, f2 }, aplicarJoin: true)
            : enumNegocio.CentroGestor.SeleccionarPorFiltro<CentroGestorDtm>(contexto, new List<ClausulaDeFiltrado> { f1 }, aplicarJoin: true);

            return cgs;
        }


        public static List<CuentaDeMiSociedadDtm> CuentasDeMiSociedad(this SociedadDtm sociedad, ContextoSe contexto, enumClaseDeCuentaBancaria? clase = null, bool? activa = true, bool errorSiNoHay = false)
        {
            var filtros = new Dictionary<string, object> { { nameof(CuentaDeMiSociedadDtm.IdElemento), sociedad.Id } };
            if (clase != null)
                filtros.Add(nameof(CuentaDeMiSociedadDtm.Clase), clase);
            if (activa != null)
                filtros.Add(nameof(CuentaDeMiSociedadDtm.Activa), activa);

            var cuentas = contexto.SeleccionarTodos<CuentaDeMiSociedadDtm>(filtros, aplicarJoin: true);
            if (errorSiNoHay && cuentas.Count == 0)
                GestorDeErrores.Emitir($"La sociedad '{sociedad.Referencia}' no tiene cuentas de la clase '{clase}' disponibles");

            return cuentas;
        }

        public static string Expresion(this CuentaDeMiSociedadDtm cuentaDeMiSociedad, ContextoSe contexto)
        =>
        $"({cuentaDeMiSociedad.Alias}) {cuentaDeMiSociedad.Cuenta(contexto).NumeroIban}";



        public static ClienteDtm CrearCliente(this SociedadDtm datos, ContextoSe contexto)
        {
            var interlocutores = contexto.SeleccionarTodos<InterlocutorDtm>(nameof(InterlocutorDto.Expresion), datos.NIF);
            var interlocutor = interlocutores.Count == 0 ? null : interlocutores[0];
            if (interlocutor == null)
            {
                var sociedad = CrearSiNoExiste(contexto, datos.NIF, datos.Nombre, datos.Nombre, datos.CodigoFiscal, datos.eMail, datos.Telefono);
                interlocutor = sociedad.CrearInterlocutor(contexto);
            }
            return interlocutor.CrearCliente(contexto, contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Clientes).Id);
        }

        public static void ValidarDatosSociedad(this SociedadDtm sociedad, ContextoSe contexto)
        {
            var sociedadBd = contexto.SeleccionarPorNombre<SociedadDtm>(sociedad.Nombre, errorSiNoHay: false, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });
            if (sociedadBd != null)
            {
                GestorDeErrores.Emitir($"No se puede crear la sociedad '{sociedad.Nombre}' ya que ya hay una en la BD '{sociedadBd.Expresion}' con el mismo nombre");
            }

            sociedadBd = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), sociedad.NIF, errorSiNoHay: false, parametros: new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });
            if (sociedadBd != null)
            {
                GestorDeErrores.Emitir($"No se puede crear la sociedad '{sociedad.Nombre}' ya que ya hay una en la BD '{sociedadBd.Referencia(contexto)}' con el mismo nombre");
            }
        }

        public static SociedadDtm CrearSiNoExiste(ContextoSe contexto, string nif, string nombre, string razonSocial, string codigoFiscal, string mail, string telefono)
        {

            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nif, false);
            if (sociedad == null)
            {
                sociedad = new SociedadDtm();
                sociedad.NIF = nif;
                sociedad.Nombre = nombre;
                sociedad.RazonSocial = razonSocial;
                sociedad.NIF = nif;
                sociedad.CodigoFiscal = codigoFiscal;
                sociedad.eMail = mail;
                sociedad.Telefono = telefono;
                sociedad = sociedad.Insertar(contexto);
            }
            return sociedad;
        }

        public static bool EsGestionada(this SociedadDtm sociedad, ContextoSe contexto)
        =>
        contexto.Existen<CentroGestorDtm>(nameof(CentroGestorDtm.IdSociedad), sociedad.Id, new Dictionary<string, object> { { ltrParametrosNeg.IncluirBajas, true } });

        public static List<SociedadDtm> SociedadesGestionadas(ContextoSe contexto)
        {
            return contexto.Set<SociedadDtm>()
                .Where(s => s.Baja == false &&
                           contexto.Set<CentroGestorDtm>().Any(cg => cg.Baja == false && cg.IdSociedad == s.Id))
                .ToList();
        }

        public static int NumeroDeSociedadesGestionadas(ContextoSe contexto)
        {
            return contexto.Set<CentroGestorDtm>().Where(cg => cg.Baja == false && contexto.Set<SociedadDtm>().Any(s => s.Baja == false && s.Id == cg.IdSociedad)).
            GroupBy(x => x.IdSociedad).
            Count();
        }

        public static bool TieneFacturasEmitidas(this SociedadDtm sociedad, ContextoSe contexto)
        =>
        sociedad.EsInterlocutor ? sociedad.Interlocutor(contexto, crearSiNoLoHay: false, errorSiNoHay: false).TieneFacturas(contexto) : false;

        public static bool TieneContactos(this SociedadDtm sociedad, ContextoSe contexto)
        =>
        contexto.Set<ContactoDtm>().Any(x => x.IdSociedad.Equals(sociedad.Id));

        public static SociedadDtm Cfg_Sociedad_Del_Sistema(ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(nameof(Cfg_Sociedad_Del_Sistema)))
            {
                var nif = CacheDeVariable.Cfg_Sociedad_Del_Sistema;

                if (nif == ltrDeSociedad.SociedadNula && NumeroDeSociedadesGestionadas(contexto) == 1)
                {
                    nif = SociedadesGestionadas(contexto)[0].NIF;
                    CacheDeVariable.Modificar(Variable.CFG_Sociedad_Del_Sistema, nif);
                }

                var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nif, errorSiNoHay: false);
                if (sociedad is null)
                    GestorDeErrores.Emitir($"Debe de definir la variable del sistema {Variable.CFG_Sociedad_Del_Sistema}, el NIF asignado '{nif}' no es válido, corríjalo y vuelva a repetir el proceso que estaba realizando");
                cache[nameof(Cfg_Sociedad_Del_Sistema)] = sociedad;
            }
            return (SociedadDtm)cache[nameof(Cfg_Sociedad_Del_Sistema)];
        }

        public static void SincronizarConInterlocutor(this SociedadDtm sociedad, ContextoSe contexto, SociedadDtm anterior)
        {
            if (sociedad.Expresion == anterior.Expresion && sociedad.eMail == anterior.eMail && sociedad.Telefono == anterior.Telefono)
                return;

            var interlocutor = sociedad.Interlocutor(contexto, crearSiNoLoHay: false, errorSiNoHay: false);
            if (interlocutor is null) return;
            interlocutor.Nombre = sociedad.Expresion;
            if (sociedad.eMail != anterior.eMail) interlocutor.eMail = sociedad.eMail;
            if (sociedad.Telefono != anterior.Telefono) interlocutor.Telefono = sociedad.Telefono;
            interlocutor.Modificar(contexto);
            ServicioDeCaches.EliminarCache(CacheDe.Ter_NifDeProveedor);
            ServicioDeCaches.EliminarCache(CacheDe.Ter_NifDeCliente);
        }

        public static void SincronizarConTrabajadores(this SociedadDtm sociedad, ContextoSe contexto, SociedadDtm anterior)
        {
            if (sociedad.CodigoFiscal == anterior.CodigoFiscal)
                return;

            var cgs = sociedad.CentrosGestores(contexto, SoloPadres: false);
            if (cgs.Count == 0) return;

            foreach (var cg in cgs)
            {
                var usuarioDeConexion = contexto.SeleccionarPorId<UsuarioDtm>(contexto.DatosDeConexion.IdUsuario);
                var otorgado = usuarioDeConexion.OtorgarAdministrador(contexto);
                var trabajadores = cg.Trabajadores(contexto);
                try
                {
                    foreach (var trabajador in trabajadores)
                    {
                        trabajador.Nombre = trabajador.Nombre.Replace($"{anterior.CodigoFiscal}: ", $"{sociedad.CodigoFiscal}: ");
                        trabajador.Modificar(contexto);
                    }
                }
                finally
                {
                    usuarioDeConexion.AnularAdministrador(contexto, otorgado);
                }
            }
        }

        public static void DarDeBajaLosContactos(this SociedadDtm Sociedad, ContextoSe contexto)
        {
            var filtro = new ClausulaDeFiltrado();
            filtro.Criterio = enumCriteriosDeFiltrado.igual;
            filtro.Valor = Sociedad.Id.ToString();
            filtro.Clausula = nameof(ContactoDtm.IdSociedad);

            List<ContactoDtm> contactos = contexto.SeleccionarTodos<ContactoDtm>(new List<ClausulaDeFiltrado> { filtro }, parametros: new Dictionary<string, object>
            {
                {ltrParametrosNeg.ValidarPermisosDeConsulta, false}
            });

            var parametros = new ParametrosDeNegocio(enumTipoOperacion.Modificar);
            foreach (var contacto in contactos)
            {
                if (contacto.Baja) continue;
                contacto.Baja = true;
                contacto.Modificar(contexto, parametros: new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDePersistencia, false } });
            }
        }

        public static PuestoDtm PuestoDeAdministrador(ContextoSe contexto)
        {
            var puesto = new PuestoDtm
            {
                IdCg = CgParaPuestoDeAdministrador(contexto).Id,
                Nombre = ltrDePuestoTrabajo.PtDeAdministrador,
                Descripcion = "Permisos de administración para poder parametrizar"
            }.InsertarComoAdministradorSiNoExiste(contexto, new List<string> { nameof(PuestoDtm.IdCg), nameof(PuestoDtm.Nombre) });

            var parametro = enumNegocio.Sociedad.Parametro(enumParametrosDeSociedad.SOCIEDAD_ID_PT_DE_ADMINISTRADOR, emitirError: false);
            if (parametro == null)
            {
                enumNegocio.Sociedad.CrearParametro(contexto, enumParametrosDeSociedad.SOCIEDAD_ID_PT_DE_ADMINISTRADOR, puesto.Id.ToString());
                return puesto;
            }
            if (parametro.Valor.Entero() == 0)
            {
                GestorDeErrores.Emitir($"Debe indicar en los parámetros del negocio '{enumNegocio.Sociedad.Descripcion()}' el valor del parámetro '{enumParametrosDeSociedad.SOCIEDAD_ID_PT_DE_ADMINISTRADOR}' para agrupar en él los permisos directos del administrador");
            }

            if (puesto.Id == parametro.Valor.Entero())
                return puesto;

            puesto = contexto.SeleccionarPorId<PuestoDtm>(parametro.Valor.Entero(), errorSiNoHay: false);
            if (puesto == null)
            {
                GestorDeErrores.Emitir($"Debe indicar en los parámetros del negocio '{enumNegocio.Sociedad.Descripcion()}' un id válido para el parámetro '{enumParametrosDeSociedad.SOCIEDAD_ID_PT_DE_ADMINISTRADOR}', el indicado es: {parametro.Valor.Entero()}");
            }
            return puesto;
        }

        public static CentroGestorDtm CgParaPuestoDeAdministrador(ContextoSe contexto)
        {
            var parametro = enumNegocio.Sociedad.Parametro(enumParametrosDeSociedad.SOCIEDAD_ID_CG_PARA_PT_DE_ADMINISTRADOR, emitirError: false, crearParametro: true, valorPorDefecto: 0);
            if (parametro.Valor.Entero() == 0)
            {
                GestorDeErrores.Emitir($"Debe indicar en los parámetros del negocio '{enumNegocio.Sociedad.Descripcion()}' el valor del parámetro '{enumParametrosDeSociedad.SOCIEDAD_ID_CG_PARA_PT_DE_ADMINISTRADOR}' para poder crear los puestos de trabajo");
            }
            var cg = contexto.SeleccionarPorId<CentroGestorDtm>(parametro.Valor.Entero(), errorSiNoHay: false);
            if (cg == null)
            {
                GestorDeErrores.Emitir($"Debe indicar en los parámetros del negocio '{enumNegocio.Sociedad.Descripcion()}' un id válido para el parámetro '{enumParametrosDeSociedad.SOCIEDAD_ID_CG_PARA_PT_DE_ADMINISTRADOR}', el indicado es: {parametro.Valor.Entero()}");
            }
            return cg;
        }

        public static IQueryable<CentroGestorDtm> CentrosGestores(this SociedadDtm sociedad, ContextoSe contexto) => contexto.Set<CentroGestorDtm>().Where(cg => cg.IdSociedad == sociedad.Id);

        public static CertificadoDtm ObtenerCertificado(this SociedadDtm sociedad, ContextoSe contexto, bool errorSiNoHay = true)
        {

            var certificadosDeUnaSociedad = GestorDeVinculos.RegistrosVinculados<CertificadoDtm>(contexto, enumNegocio.Sociedad, enumNegocio.Certificado, sociedad.Id);

            if (certificadosDeUnaSociedad.Count == 0)
                GestorDeErrores.Emitir($"La sociedad '{sociedad.NIF}' no tiene instalado el certificado electrónico en el sistema");

            if (certificadosDeUnaSociedad[certificadosDeUnaSociedad.Count - 1].IdArchivo is null)
                GestorDeErrores.Emitir($"No se ha indicado el archivo del certificado '{certificadosDeUnaSociedad[0].Nombre}' de la sociedad '{sociedad.NIF}'");

            return certificadosDeUnaSociedad[certificadosDeUnaSociedad.Count - 1];
        }

        public static void ValidarCertificado(this SociedadDtm sociedad, ContextoSe contexto)
        {
            var certificado = sociedad.ObtenerCertificado(contexto, errorSiNoHay: true);
            var password = ApiDeCertificados.LeerPasswordDeCertificado(contexto, certificado.Id);
            ApiDeCertificados.ValidarPassword(contexto, certificado.Id, password);
        }

        public static List<int> PermisosDeBuzones(ContextoSe contexto)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Valores);
            if (!cache.ContainsKey(ltrDeBuzonesDeMiSociedad.PermisosDeBuzones))
            {
                cache[ltrDeBuzonesDeMiSociedad.PermisosDeBuzones] = contexto.Set<BuzonDeMiSociedadDtm>().Select(buzon => buzon.IdPermiso).ToList();
            }

            return (List<int>)cache[ltrDeBuzonesDeMiSociedad.PermisosDeBuzones];
        }
    }


}
