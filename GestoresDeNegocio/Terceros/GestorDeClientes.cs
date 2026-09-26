using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Seguridad;
using GestoresDeNegocio.Venta.Factura;
using Microsoft.EntityFrameworkCore;
using GestoresDeNegocio.Callejero;
using ModeloDeDto;
using ModeloDeDto.Callejero;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Expediente;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Seguridad;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeClientes : GestorDeElementos<ContextoSe, ClienteDtm, ClienteDto>
    {
        public override enumNegocio Negocio => enumNegocio.Cliente;

        public class MapearClientes : Profile
        {
            public MapearClientes()
            {
                CreateMap<ClienteDtm, ClienteDto>();
                CreateMap<ClienteDto, ClienteDtm>()
                .ForMember(dtm => dtm.Expresion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Nombre, dto => dto.MapFrom(x => x.Expresion));
            }
        }

        public GestorDeClientes(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeClientes Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeClientes(contexto, mapeador); ;
        }

        protected override IQueryable<ClienteDtm> AplicarJoins(IQueryable<ClienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Interlocutor);
            return consulta;
        }

        protected override IQueryable<ClienteDtm> AplicarFiltros(IQueryable<ClienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            foreach (var filtro in filtros)
            {
                if (filtro.Clausula.Equals(nameof(ltrCliente.IdSociedad), StringComparison.CurrentCultureIgnoreCase))
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdSociedad == filtro.Valor.Entero() && x.Interlocutor.IdContacto == null);

                if (filtro.Clausula.Equals(nameof(ltrCliente.IdPersona), StringComparison.CurrentCultureIgnoreCase))
                    consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.IdPersona == filtro.Valor.Entero());

                if (filtro.Clausula.Equals(nameof(ClienteDto.NIF), StringComparison.CurrentCultureIgnoreCase))
                {
                    if (ApiDeTerceros.ValidarNif(filtro.Valor).IsNullOrEmpty())
                        consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.NIF.ToLower() == filtro.Valor);
                    else
                        if (ApiDeTerceros.CifValido(filtro.Valor))
                            consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Sociedad.NIF.ToLower() == filtro.Valor);
                        else
                        {
                            consulta = consulta.Where(x => false);
                            filtro.Aplicado = true;
                        }
                }

                if (filtro.Clausula.Equals(nameof(ClienteDto.Expresion), StringComparison.CurrentCultureIgnoreCase))
                {
                    if (ApiDeTerceros.ValidarNif(filtro.Valor).IsNullOrEmpty())
                        consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.NIF.ToLower() == filtro.Valor);
                    else
                        if (ApiDeTerceros.CifValido(filtro.Valor))
                            consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Sociedad.NIF.ToLower() == filtro.Valor);
                        else if (filtro.Valor.EsEntero())
                            consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.eMail.Contains(filtro.Valor)
                                                                      || x.Interlocutor.Sociedad.eMail.Contains(filtro.Valor)
                                                                      || x.Interlocutor.Persona.Telefono.Contains(filtro.Valor)
                                                                      || x.Interlocutor.Sociedad.Telefono.Contains(filtro.Valor));
                        else if (filtro.Valor.Contains("@"))
                            consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.eMail.Contains(filtro.Valor)
                                                                      || x.Interlocutor.Sociedad.eMail.Contains(filtro.Valor));
                        else
                            consulta = consulta.AplicarPredicado(filtro, x => x.Interlocutor.Persona.Apellidos.Contains(filtro.Valor)
                                                                           || x.Interlocutor.Sociedad.Nombre.Contains(filtro.Valor));
                }

                if (filtro.Clausula.Equals(ltrFiltros.VincularCon, StringComparison.CurrentCultureIgnoreCase))
                {
                    var vinculos = ExtensorDeClientes.Clientes(NegociosDeSe.ToEnumerado(filtro.IdNegocio), Contexto).Where(x => x.idElemento1 == filtro.IdElemento);
                    consulta = consulta.ElementosNoVinculadosDeLaMismaSociedad(Contexto, filtro, vinculos);
                }
            }
            return consulta;
        }

        protected override IQueryable<ClienteDtm> AplicarSeguridad(IQueryable<ClienteDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador) consulta = FiltrarPorSeguridad.DeNegocio(Contexto, Negocio, consulta);
            return consulta;
        }

        //protected override void DespuesDeMapearElRegistro(ClienteDto elemento, ClienteDtm cliente, ParametrosDeNegocio opciones)
        //{
        //    base.DespuesDeMapearElRegistro(elemento, cliente, opciones);
        //    if (!opciones.Insertando && cliente.Nombre.IsNullOrEmpty())
        //    {
        //        var inter = Contexto.SeleccionarPorId<InterlocutorDtm>(elemento.IdInterlocutor);
        //        cliente.Nombre = inter.Expresion;
        //    }
        //}

        protected override void AntesDePersistir(ClienteDtm cliente, ParametrosDeNegocio parametros)
        {
            var inter = Contexto.SeleccionarPorId<InterlocutorDtm>(cliente.IdInterlocutor);
            cliente.Nombre = inter.Expresion;
            base.AntesDePersistir(cliente, parametros);
            if (parametros.Insertando)
            {
                if (inter.EsContacto)
                    GestorDeErrores.Emitir("Un cliente no puede ser un contacto");
                cliente.CodigoContable = (Contexto.Set<ClienteDtm>().Max(x => x.CodigoContable) ?? 0) + 1;
            }
            if (parametros.Modificando)
            {
                cliente.CodigoContable = ((ClienteDtm)parametros.registroEnBd).CodigoContable;
            }

            if (VariableDeFacturasEmt.Fae_Sii_Activo() && VariablesDeClientes.Cli_Validar_En_La_AEAT())
                ValidarClienteEnAeat(Contexto, inter.NIF(Contexto, quitarPrefijoEs: true), inter.RazonSocial(Contexto));

            var direccion = cliente.DireccionFiscal(Contexto, errorSiNoHay: false);
            if (!cliente.VAT.IsNullOrEmpty())
            {
                if (cliente.SeHaModificadoElCampo<string>(x => x.Name == nameof(cliente.VAT), parametros) && cliente.TieneFacturas(Contexto))
                    GestorDeErrores.Emitir($"No se puede modificar el VAT de un cliente que tiene facturas emitidas");

                if (direccion is not null && !direccion.IntraComunitaria)
                    GestorDeErrores.Emitir($"Un cliente con una dirección fiscal no comunitaria, no tiene VAT");
            }
            else
            {
                if (direccion is not null && direccion.IntraComunitaria)
                    GestorDeErrores.Emitir($"Un cliente con una dirección fiscal comunitaria, ha de tener un VAT");
            }
        }

        public override ClienteDtm PersistirRegistro(ClienteDtm cliente, ParametrosDeNegocio parametros)
        {
            try
            {
                return base.PersistirRegistro(cliente, parametros);
            }
            catch (Exception e)
            {
                if (e.Message == msgCliente.FaltaParametroTipoArchivador)
                {
                    ParametroDeNegocioSql.Crear(enumNegocio.Cliente.IdNegocio(), enumParametrosDeCliente.CLI_TipoArchivador, ltrTipoArchivador.TipoClientes);
                    GestorDeErrores.Emitir($"Configure el parámetro '{enumParametrosDeCliente.CLI_TipoArchivador}' del negocio de '{enumNegocio.Cliente.Singular()}'");
                }
                if (e.Message == msgCliente.FaltaParametroCg)
                {
                    ParametroDeNegocioSql.Crear(enumNegocio.Cliente.IdNegocio(), enumParametrosDeCliente.CLI_CG_De_Cliente, "900");
                    GestorDeErrores.Emitir($"Configure el parámetro '{enumParametrosDeCliente.CLI_CG_De_Cliente}' del negocio de '{enumNegocio.Cliente.Singular()}', Propuesta un CG: [CodigoContable].{ltrDeSociedad.CodigoCgClienteWeb} - {ltrDeSociedad.CentroGestorDeClientesWeb}");
                }
                throw;
            }
        }

        protected override void EliminarCaches(ClienteDtm cliente, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(cliente, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Ter_NifDeCliente, cliente.Id.ToString());
            ServicioDeCaches.EliminarElemento(CacheDe.Ter_Interlocutor, $"{typeof(ClienteDtm).Name}-{cliente.Id}");
            ServicioDeCaches.EliminarElemento(CacheDe.Int_Cliente, cliente.IdInterlocutor.ToString());
        }


        protected override void DespuesDePersistir(ClienteDtm cliente, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(cliente, parametros);
            if (parametros.Modificando) cliente.TrazarModificaciones(Contexto, (ClienteDtm)parametros.registroEnBd);
        }

        protected override void AlDarDeAlta(ClienteDtm cliente, ParametrosDeNegocio parametros)
        {
            base.AlDarDeAlta(cliente, parametros);

            if (cliente.Interlocutor(Contexto).Baja)
                GestorDeErrores.Emitir($"Debe dar de alta el interlocutor '{cliente.Interlocutor(Contexto).Expresion(Contexto)}' antes de dar de alta el cliente");
        }

        public static List<ClienteDtm> CrearClientes(ContextoSe contexto, List<int> idsDeInter, int idCuenta, CrearDireccionDto direccion = null)
        {
            var result = new List<ClienteDtm>();
            foreach (var idInter in idsDeInter)
            {
                result.Add(CrearCliente(contexto, idInter, idCuenta, direccion));
            }
            return result;
        }

        public static ClienteDtm CrearCliente(ContextoSe contexto, string nif, int idCuenta, CrearDireccionDto direccion = null)
        {
            var inter = ExtensorDeInterlocutores.CrearInterlocutor(contexto, nif);
            return CrearCliente(contexto, inter, idCuenta, direccion);
        }

        public static ClienteDtm CrearCliente(ContextoSe contexto, int idInter, int idCuenta, CrearDireccionDto direccion = null)
        =>
        CrearCliente(contexto, contexto.SeleccionarPorId<InterlocutorDtm>(idInter), idCuenta, direccion);

        public static ClienteDtm CrearCliente(ContextoSe contexto, InterlocutorDtm inter, int idCuenta, CrearDireccionDto direccion = null)
        {
            var Cliente = contexto.SeleccionarPorFk<ClienteDtm>(nameof(ClienteDtm.IdInterlocutor), inter.Id, errorSiNoHay: false);

            if (Cliente != null)
                return Cliente;

            if (inter.Baja)
                GestorDeErrores.Emitir($"No se puede dar de alta un  {enumNegocio.Cliente.Singular(true)} por estar de baja el interlocutor");


            if (Cliente == null)
            {
                Cliente = new ClienteDtm();
                Cliente.IdInterlocutor = inter.Id;
                Cliente.Nombre = inter.Expresion;
                Cliente.Telefono = inter.Telefono;
                Cliente.eMail = inter.eMail;
                Cliente.IdCuenta = idCuenta;
                Cliente = Cliente.Insertar(contexto, parametros: direccion == null ? null : new Dictionary<string, object> { { Ampliaciones.Comunes.DireccionAlCrear, direccion } });
            }

            return Cliente;
        }

        public static ClienteDtm CrearClienteCompleto(ContextoSe contexto, ClienteFacturadorJson datos)
        {
            if (datos.NIF.IsNullOrEmpty())
                GestorDeErrores.Emitir("El NIF del cliente es obligatorio");

            datos.NIF = datos.NIF.ToUpper();

            var tipo = datos.TipoDeCliente.IsNullOrEmpty()
                ? ApiDeTerceros.TipoDeClienteEsp(datos.NIF)
                : ApiDeTerceros.ParsearTipoDeCliente(datos.TipoDeCliente);

            ValidarDatosDelCliente(contexto, datos, tipo);

            // una sociedad con NIF de persona (autónomo) tiene como razón social su nombre y
            // apellidos: se capitaliza igual que el de una persona; la de una empresa se respeta
            var tipoDeTercero = ApiDeTerceros.TipoDeTerceroEsp(datos.NIF);
            var esAutonomo = tipoDeTercero == enumTipoTercero.Autonomo;
            var razonSocial = tipo == enumTipoCliente.Juridica && esAutonomo ? datos.Nombre.Capitalizar() : datos.Nombre;

            if (datos.ValidarEnLaAeat)
                ValidarClienteEnAeat(contexto, datos.NIF, tipo == enumTipoCliente.Fisica ? $"{datos.Apellidos}, {datos.Nombre}" : razonSocial);

            ClienteDtm cliente;
            var clienteExistente = contexto.SeleccionarPorPropiedad<ClienteDtm>(nameof(ClienteDto.NIF), datos.NIF, errorSiNoHay: false);

            if (clienteExistente != null)
            {
                cliente = clienteExistente;
                var interlocutor = contexto.SeleccionarPorId<InterlocutorDtm>(cliente.IdInterlocutor);

                if (interlocutor.EsPersona)
                {
                    var persona = contexto.SeleccionarPorId<PersonaDtm>(interlocutor.IdPersona.Value);
                    // se compara con el valor capitalizado, que es como lo guarda GestorDePersonas; si no,
                    // un JSON en mayúsculas se tomaría siempre como un cambio
                    if ((persona.Apellidos != datos.Apellidos.Capitalizar() || persona.Nombre != datos.Nombre.Capitalizar()) && datos.SustituirDatosIdentificativos)
                    {
                        persona.Apellidos = datos.Apellidos;
                        persona.Nombre = datos.Nombre;
                        persona.Modificar(contexto);
                    }
                }
                else if (interlocutor.EsSociedad)
                {
                    var sociedad = contexto.SeleccionarPorId<SociedadDtm>(interlocutor.IdSociedad.Value);
                    if (sociedad.RazonSocial != razonSocial && datos.SustituirDatosIdentificativos)
                    {
                        sociedad.Nombre = razonSocial;
                        sociedad.RazonSocial = razonSocial;
                        sociedad.Modificar(contexto);
                    }
                }

                if ((cliente.eMail != datos.eMail || cliente.Telefono != datos.Telefono) && datos.SustituirDatosDeContacto)
                {
                    cliente.eMail = datos.eMail;
                    cliente.Telefono = datos.Telefono;
                    cliente.Modificar(contexto);
                }
            }
            else
            {
                InterlocutorDtm interlocutor;
                if (tipo == enumTipoCliente.Juridica)
                {
                    var sociedad = ExtensorDeSociedades.CrearSiNoExiste(contexto, datos.NIF, razonSocial, razonSocial, null, datos.eMail, datos.Telefono);
                    interlocutor = sociedad.CrearInterlocutor(contexto, errorSihay: false);
                }
                else
                {
                    var esNie = ApiDeTerceros.ValidarNie(datos.NIF).IsNullOrEmpty();
                    var persona = ExtensorDePersonas.CrearSiNoExiste(contexto, datos.NIF, datos.Nombre, datos.Apellidos, esNie, datos.eMail, datos.Telefono);
                    interlocutor = persona.CrearInterlocutor(contexto, errorSihay: false);
                }

                var idCuenta = contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Clientes).Id;
                cliente = CrearCliente(contexto, interlocutor, idCuenta);
            }

            if (!datos.Municipio.IsNullOrEmpty())
            {
                var municipio = contexto.SeleccionarPorNombre<MunicipioDtm>(datos.Municipio, $"No se ha localizado el municipio indicado: '{datos.Municipio}'", aplicarJoin: true);
                var tipoVia = contexto.SeleccionarPorNombre<TipoDeViaDtm>(datos.TipoDeVia, $"No se ha localizado el tipo de vía indicado: '{datos.TipoDeVia}'");
                var codigoPostal = contexto.SeleccionarPorPropiedad<CodigoPostalDtm>(nameof(CodigoPostalDtm.Codigo), datos.CodigoPostal, errorSiNoHay: false);
                if (codigoPostal == null)
                    GestorDeErrores.Emitir($"No se ha localizado el código postal indicado: '{datos.CodigoPostal}'");

                var calle = contexto.SeleccionarTodos<CalleDtm>(new Dictionary<string, object>
                {
                    { nameof(CalleDtm.Nombre), datos.Calle },
                    { nameof(CalleDtm.IdTipoDeVia), tipoVia.Id },
                    { nameof(CalleDtm.IdMunicipio), municipio.Id }
                }).FirstOrDefault();

                if (calle == null)
                {
                    if (!datos.CrearCalleSiNoExiste)
                        GestorDeErrores.Emitir($"La calle '{datos.Calle}' no existe en el municipio '{datos.Municipio}' y no se permite crearla");

                    // la existencia en el Catastro ya se validó en ValidarDireccionFiscal
                    calle = new CalleDtm { Nombre = datos.Calle, IdMunicipio = municipio.Id, IdTipoDeVia = tipoVia.Id }.Insertar(contexto);

                    GestorDeCpsDeUnaCalle.Gestor(contexto, contexto.Mapeador).CrearRelacion(new CpsDeUnaCalleDto
                    {
                        IdCalle = calle.Id,
                        IdCp = codigoPostal.Id,
                        Mano = ParseosDeManosDeUnaCalle.Ambos
                    }, new ParametrosDeNegocio(enumTipoOperacion.Insertar), errorSiYaExiste: false);
                }

                var direccionActual = GestorDeDirecciones.LeerRegistros(contexto, enumNegocio.Cliente, cliente.Id)
                    .FirstOrDefault(x => x.Calificador == enumCalificadorDireccion.fiscal && x.Activo);

                var esLaMisma = direccionActual != null
                    && direccionActual.IdCalle == calle.Id
                    && direccionActual.IdCp == codigoPostal.Id
                    && direccionActual.Numero == datos.Numero;

                if (!esLaMisma)
                {
                    if (direccionActual != null)
                        GestorDeDirecciones.Gestor(contexto, enumNegocio.Cliente).PersistirRegistro(direccionActual, new ParametrosDeNegocio(enumTipoOperacion.Eliminar));

                    GestorDeDirecciones.Gestor(contexto, enumNegocio.Cliente).PersistirRegistro(new DireccionDeUnClienteDtm
                    {
                        IdElemento = cliente.Id,
                        IdPais = municipio.Provincia.IdPais,
                        IdProvincia = municipio.IdProvincia,
                        IdMunicipio = municipio.Id,
                        IdCalle = calle.Id,
                        IdCp = codigoPostal.Id,
                        Calificador = enumCalificadorDireccion.fiscal,
                        Numero = datos.Numero,
                        Negocio = enumNegocio.Cliente
                    }, new ParametrosDeNegocio(enumTipoOperacion.Insertar));
                }
            }

            return cliente;
        }

        private static void ValidarDatosDelCliente(ContextoSe contexto, ClienteFacturadorJson datos, enumTipoCliente tipo)
        {
            if (tipo == enumTipoCliente.Fisica)
            {
                if (datos.Nombre.IsNullOrEmpty())
                    GestorDeErrores.Emitir("El nombre es obligatorio para una persona física");
                if (datos.Apellidos.IsNullOrEmpty())
                    GestorDeErrores.Emitir("Los apellidos son obligatorios para una persona física");
            }
            else
            {
                if (!datos.Apellidos.IsNullOrEmpty())
                    GestorDeErrores.Emitir("Una persona jurídica no lleva apellidos: el nombre debe ser la razón social y, si es un autónomo, el nombre y los apellidos");
                if (datos.Nombre.IsNullOrEmpty())
                    GestorDeErrores.Emitir("El nombre (razón social) es obligatorio para una persona jurídica");
            }

            if (datos.eMail.IsNullOrEmpty())
                GestorDeErrores.Emitir("El eMail del cliente es obligatorio");
            if (datos.Telefono.IsNullOrEmpty())
                GestorDeErrores.Emitir("El teléfono del cliente es obligatorio");

            if (!datos.Municipio.IsNullOrEmpty())
                ExtensorDeDirecciones.ValidarDatosDeDireccion(contexto, datos.Municipio, datos.CodigoPostal, datos.TipoDeVia, datos.Calle, datos.ValidarEnCatastro);
        }

        protected override void DespuesDeMapearElElemento(ClienteDtm cliente, ClienteDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(cliente, elemento, parametros);
            if (parametros.Peticion == enumPeticion.epLeerPorId || parametros.Parametros.LeerValor(ltrParametrosNeg.ObtenerDatosFiscales, false))
            {
                elemento.NIF = cliente.NIF(Contexto);
                elemento.TipoDeTercero = ApiDeTerceros.TipoDeTerceroEsp(elemento.NIF).ToString();
                var direccion = cliente.DireccionFiscal(Contexto, errorSiNoHay: false);
                if (direccion != null)
                {
                    elemento.DireccionFiscal = direccion.Expresion;
                    elemento.EsExtraComunitario = direccion.ExtraComunitaria;
                    elemento.EsIntraComunitario = direccion.IntraComunitaria;
                }
                if (parametros.Parametros.LeerValor(ltrParametrosNeg.ObtenerDatosFiscales, false))
                {
                    if (direccion is null)
                        GestorDeErrores.Emitir($"El cliente '{cliente.Referencia}' debe tener una dirección fiscal");

                    elemento.RazonSocial = cliente.RazonSocial(Contexto);
                }

                if (parametros.Peticion == enumPeticion.epLeerPorId)
                {
                    if (direccion == null)
                        elemento.informacion = $"Asigne una dirección fiscal al cliente";
                    else if (Contexto.HayRegistros<UsuarioDeClienteDtm>(Contexto))
                    {
                        var nombreDelPuesto = enumNegocio.Cliente.LeerCrearParametro(Contexto, enumParametrosDeCliente.CLI_PuestoDeTrabajo, valor: "");
                        if (nombreDelPuesto.Valor.IsNullOrEmpty())
                        {
                            elemento.informacion = $"Es aconsejable definir el valor del parámetro '{enumParametrosDeCliente.CLI_PuestoDeTrabajo}' con el nombre del puesto de trabajo por defecto al asociar un usuario a un cliente";
                        }
                    }
                }
            }
        }

        public static void DespuesDeVincular(EntornoDeUnaAccion entorno)
        {
            var cliente = (ClienteDtm)entorno.Registro;
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            if (vinculado == enumNegocio.Archivador)
            {
                var contexto = entorno.Contexto;
                var usuariosDeCliente = cliente.Detalles<UsuarioDeClienteDtm>(contexto, aplicarJoin: true).Where(x => x.Usuario.Activo);
                if (usuariosDeCliente.Count() == 1)
                {
                    List<int> usuarios = new List<int>();
                    foreach (var usuarioDeCliente in usuariosDeCliente.Where(x => x.Usuario.Activo)) usuarios.Add(usuarioDeCliente.IdUsuario);
                    GestorDePemisosDelElemento.OtorgarPermisoDe(contexto, vinculado, entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)), usuarios, enumModoDeAccesoDeDatos.Gestor);
                }
            }
        }

        public static void DespuesDeQuitarVinculo(EntornoDeUnaAccion entorno)
        {
            var cliente = (ClienteDtm)entorno.Registro;
            var vinculado = entorno.Parametros.LeerValor<enumNegocio>(nameof(ltrParametrosNeg.Vinculado));
            if (vinculado == enumNegocio.Archivador)
            {
                var contexto = entorno.Contexto;
                var usuariosDeCliente = cliente.Detalles<UsuarioDeClienteDtm>(contexto);
                if (usuariosDeCliente.Count() > 0)
                {
                    List<int> usuarios = new List<int>();
                    foreach (var usuarioDeCliente in usuariosDeCliente) usuarios.Add(usuarioDeCliente.IdUsuario);
                    GestorDePemisosDelElemento.QuitarPermisos(contexto, vinculado, entorno.Parametros.LeerValor<int>(nameof(ltrParametrosNeg.IdVinculado)), usuarios);
                }
            }
        }

        public static IEnumerable<ExpedienteDeClienteDto> LeerExpedientes(ContextoSe contexto, int idCliente)
        {
            var cache = ServicioDeCaches.Obtener(CacheDe.Ter_ExpedientesDeClientes);
            var indice = idCliente.ToString();

            if (!cache.ContainsKey(indice))
            {
                var cliente = new GestorDeClientes(contexto, contexto.Mapeador).LeerRegistroPorId(idCliente, usarLaCache: true, traqueado: false, conBloqueo: false, aplicarJoin: true, errorSiNoHay: true,
                    new Dictionary<string, object> { { ltrParametrosNeg.ValidarPermisosDeConsulta, true } });
                var expedientes = contexto.SeleccionarTodos<ExpedienteDtm>(new Dictionary<string, object> { { ltrDeUnExpediente.IdCliente, cliente.IdInterlocutor } }, aplicarJoin: true);
                cache[indice] = expedientes.Select(x => new ExpedienteDeClienteDto
                {
                    Cg = x.Cg.Expresion,
                    Tipo = x.Tipo.Expresion,
                    Estado = x.Estado.Nombre,
                    Expediente = x.Expresion,
                    Elemento = cliente.Expresion,
                    IdElemento = cliente.Id,
                    IdExpediente = x.Id
                }).ToList();
            }
            return (IEnumerable<ExpedienteDeClienteDto>)cache[indice];
        }

        public void ValidarClienteEnAeat(int idCliente)
        {
            var cliente = Contexto.SeleccionarPorId<ClienteDtm>(idCliente);
            if (cliente == null) GestorDeErrores.Emitir($"No existe el cliente con id {idCliente}");

            if (!VariableDeFacturasEmt.Fae_Sii_Activo())
                GestorDeErrores.Emitir($"El servicio de Verifactu no está activo, no se puede validar el NIF de la sociedad '{cliente.Referencia}' en la AEAT");

            ValidarClienteEnAeat(Contexto, cliente.NIF(Contexto, quitarPrefijoEs: true), cliente.RazonSocial(Contexto));
        }

        private static void ValidarClienteEnAeat(ContextoSe contexto, string nif, string razonSocial)
        {
            try
            {
                var miSociedad = contexto.Set<SociedadDtm>().FirstOrDefault(s => s.Id == contexto.Set<CentroGestorDtm>().First(cg => true).IdSociedad);
                var gestorSii = new GeneradorSii(contexto, miSociedad);

                gestorSii.ValidarNif(nif, razonSocial);
            }
            catch (Exception e)
            {
                if (e.Message.EndsWith(msjCertificados.CertificadoNoInstalado.Right(20)))
                    GestorDeErrores.Emitir($"Error al validar el cliente en la AEAT: {e.Message}, para darlo de alta sin validar modifique el parámetro '{enumParametrosDeCliente.CLI_Validar_Aeat}'");
                else
                    GestorDeErrores.Emitir($"Error al validar el cliente en la AEAT: {e.Message}");
            }
        }
    }


}
