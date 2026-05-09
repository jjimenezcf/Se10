using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Juridico.Procedimiento;
using GestoresDeNegocio.Venta.Factura;
using GestoresDeNegocio.Ventas;
using ModeloDeDto.Negocio;
using ModeloDeDto.Terceros;
using ServicioDeDatos;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Entorno;
using ServicioDeDatos.Negocio;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;
using static GestoresDeNegocio.Terceros.GestorDeCentrosGestores;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeSociedades : GestorDeElementos<ContextoSe, SociedadDtm, SociedadDto>
    {
        public override enumNegocio Negocio => enumNegocio.Sociedad;

        public class MapearSociedades : Profile
        {
            public MapearSociedades()
            {
                CreateMap<SociedadDtm, SociedadDto>();
                // .ForMember(dto => dto.Expresion, dtm => dtm.MapFrom(dtm => $"({dtm.NIF}) {dtm.Nombre}"));

                CreateMap<SociedadDto, SociedadDtm>()
                .ForMember(dtm => dtm.Expresion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Archivo, dto => dto.Ignore());
            }
        }

        public static SociedadDto CrearSociedad(ContextoSe contexto, string nif, string razonSocial, string email, string telefono, bool crearInterlocutor = true)
        {
            var parametros = new Dictionary<string, object> { { nameof(SociedadDto.CrearInterlocutor), crearInterlocutor } };
            var sociedad = CrearSociedad(contexto, nif, razonSocial, email, telefono, parametros);

            return sociedad.MapearDto<SociedadDto>(contexto);
        }

        private static SociedadDtm CrearSociedad(ContextoSe contexto, string nif, string razonSocial, string email, string teléfono, Dictionary<string, object> parametros)
        {
            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nif, errorSiNoHay: false);
            if (sociedad == null)
            {
                sociedad = new SociedadDtm();
                sociedad.NIF = nif;
                sociedad.Nombre = razonSocial;
                sociedad.RazonSocial = razonSocial;
                sociedad.Baja = false;
                sociedad.eMail = email;
                sociedad.Telefono = teléfono;
                sociedad = sociedad.Insertar(contexto, parametros);
            }
            return sociedad;
        }

        public GestorDeSociedades(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeSociedades Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeSociedades(contexto, mapeador); ;
        }

        public static List<SociedadDtm> SociedadesConCg(ContextoSe contexto, Dictionary<string, object> filtros)
        {
            var gestor = Gestor(contexto, contexto.Mapeador);
            var filtro = new ClausulaDeFiltrado { Criterio = enumCriteriosDeFiltrado.noEsNulo, Clausula = ltrDeSociedad.FiltoPorCg };

            if (filtros.ContainsKey(ltrCentrosGestores.filtroPorSociedad))
                return new List<SociedadDtm>
            {
              gestor.LeerRegistroPorId((int)filtros[ltrCentrosGestores.filtroPorSociedad], usarLaCache: true, traqueado: false, conBloqueo: false, aplicarJoin: false)
            };

            var ordenacion = new List<ClausulaDeOrdenacion> { new ClausulaDeOrdenacion { Modo = ModoDeOrdenancion.ascendente, OrdenarPor = nameof(SociedadDtm.Nombre) } };
            return gestor.LeerRegistros(0, -1, new List<ClausulaDeFiltrado> { filtro }, ordenacion, new ParametrosDeNegocio(enumTipoOperacion.LeerSinBloqueo));
        }

        internal static JerarquiaDto JerarquiaDeSociedades(ContextoSe contexto, Dictionary<string, object> filtros)
        {
            var sociedadesDtm = SociedadesConCg(contexto, filtros);
            var jerarquia = new JerarquiaDto();
            foreach (var sociedad in sociedadesDtm)
            {
                var nodoDtm = new NodoDtm();
                nodoDtm.Activo = !sociedad.Baja;
                nodoDtm.Id = sociedad.Id;
                nodoDtm.IdPadre = null;
                nodoDtm.Nombre = sociedad.Expresion;
                nodoDtm.TipoDtm = typeof(SociedadDtm).FullName;
                var nodoDto = new NodoDto(nodoDtm, enumNegocio.Sociedad.ToNombre(), typeof(SociedadDto).FullName, nodoDtm.modoAcceso);
                var nodoDeJerarquia = new NodoDeJerarquiaDto(nodoDto);
                jerarquia.Ramas.Add(nodoDeJerarquia);
            }
            return jerarquia;
        }

        protected override IQueryable<SociedadDtm> AplicarFiltros(IQueryable<SociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorPuestoDeCliente(filtros);
            consulta = consulta.FiltrarPorExpresion(filtros);
            consulta = consulta.FiltrarPorCg(Contexto, filtros);
            consulta = consulta.FiltrarPorCif(filtros);
            consulta = consulta.FiltrarPorConContactos(Contexto, filtros);
            consulta = consulta.FiltrarPorConCertificados(Contexto, filtros);
            consulta = consulta.FiltrarPorSerInterlocutor(Contexto, filtros);
            consulta = consulta.FiltrarPorSociedadesGestionadas(Contexto, filtros);
            return consulta;
        }
        protected override IQueryable<SociedadDtm> AplicarSeguridad(IQueryable<SociedadDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador) consulta = FiltrarPorSeguridad.DeNegocio<SociedadDtm, PermisoDeLaSociedadDtm>(Contexto, Negocio, consulta);
            return consulta;
        }
        protected override void AntesDePersistir(SociedadDtm sociedad, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(sociedad, parametros);
            if (sociedad.IdArchivo == 0 || sociedad.IdArchivo == null)
            {
                sociedad.IdArchivo = null;
                sociedad.Archivo = null;
            }

            if (parametros.Insertando || parametros.Modificando)
            {
                ApiDeTerceros.ValidarCif(sociedad);
                if (sociedad.RazonSocial.IsNullOrEmpty() && !sociedad.Nombre.IsNullOrEmpty())
                    sociedad.RazonSocial = sociedad.Nombre;
            }

            if (parametros.Insertando)
            {
                sociedad.ValidarDatosSociedad(Contexto);

            }

            if (!parametros.Insertando && sociedad.EsGestionada(Contexto) && !sociedad.EsInterventor(Contexto))
                GestorDeErrores.Emitir($"No se puede modificar la sociedad '{sociedad.Referencia}' por ser gestionada por el sistema y no tener permisos de interventor");

            if (sociedad.SeHaModificadoElCampo<string>(x => x.Name == nameof(sociedad.NIF), parametros) && sociedad.TieneFacturasEmitidas(Contexto))
                GestorDeErrores.Emitir($"No se puede modificar el NIF de un cliente que tiene facturas emitidas");

            if (parametros.Modificando && ((SociedadDtm)parametros.registroEnBd).IdAgenda is not null)
            {
                sociedad.IdAgenda = ((SociedadDtm)parametros.registroEnBd).IdAgenda;
            }

            if (parametros.Eliminando || sociedad.SeHaModificadoElCampo<bool>(x => x.Name == nameof(SociedadDtm.Baja), parametros))
            {
                if (!Contexto.DatosDeConexion.EsAdministrador && Contexto.Existen<CentroGestorDtm>(new List<ClausulaDeFiltrado> {
                   new ClausulaDeFiltrado(nameof(CentroGestorDtm.IdSociedad), enumCriteriosDeFiltrado.igual, sociedad.Id),
                   new ClausulaDeFiltrado(ltrParametrosNeg.SoloEnAlta, enumCriteriosDeFiltrado.igual, true)
                }))
                    GestorDeErrores.Emitir($"No se puede dar de baja la sociedad '{sociedad.Referencia}' por tener centros gestores activos");
            }
        }

        protected override void DespuesDePersistir(SociedadDtm sociedad, ParametrosDeNegocio parametros)
        {
            var crearDireccionDto = ExtensorDeDirecciones.HayQueCrearDireccion(parametros.Parametros);
            var negocioAQuienAsociar = crearDireccionDto.DeQuienEsLaDireccion(parametros.Parametros);

            base.DespuesDePersistir(sociedad, parametros);
            if (parametros.Operacion == enumTipoOperacion.Insertar && parametros.Parametros.LeerValor(nameof(SociedadDto.CrearInterlocutor), false))
            {
                var interlocutor = sociedad.CrearInterlocutor(Contexto);
                if (negocioAQuienAsociar == enumNegocio.Interlocutor) interlocutor.CrearDireccion(Contexto, crearDireccionDto);
                parametros.Parametros[nameof(ltrDeSociedad.Interlocutores)] = new List<InterlocutorDtm> { interlocutor };

                if (parametros.Parametros.LeerValor(nameof(SociedadDto.CrearProcurador), false))
                    parametros.Parametros[nameof(ltrDeSociedad.Procuradores)] =
                    GestorDeProcuradores.CrearProcuradores(Contexto, new List<int> { interlocutor.Id });

                if (parametros.Parametros.LeerValor(nameof(SociedadDto.CrearAbogado), false))
                    parametros.Parametros[nameof(ltrDeSociedad.Abogados)] =
                    GestorDeAbogados.CrearAbogados(Contexto, new List<int> { interlocutor.Id });

                if (parametros.Parametros.LeerValor(nameof(SociedadDto.CrearCliente), false))
                {
                    var cuenta = Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Clientes);
                    var direccion = negocioAQuienAsociar == enumNegocio.Cliente ? crearDireccionDto : null;
                    parametros.Parametros[nameof(ltrDeSociedad.Clientes)] = GestorDeClientes.CrearClientes(Contexto, new List<int> { interlocutor.Id }, cuenta.Id, direccion);
                }

                if (parametros.Parametros.LeerValor(nameof(SociedadDto.CrearProveedor), false))
                    parametros.Parametros[nameof(ltrDeSociedad.Proveedores)] =
                    GestorDeProveedores.CrearProveedores(Contexto, new List<int> { interlocutor.Id },
                           Contexto.SeleccionarPorPropiedad<CuentaDtm>(nameof(CuentaDtm.Codigo), VariablesDeCuentas.Proveedores).Id);
            }

            if (sociedad.PropiedadCambiada<string>(nameof(SociedadDtm.NIF), parametros) && sociedad.IdAgenda is not null)
            {
                sociedad.ActualizaAgenda(Contexto);
            }

            if (parametros.Modificando)
            {
                sociedad.TrazarModificaciones(Contexto, (SociedadDtm)parametros.registroEnBd);
                sociedad.SincronizarConInterlocutor(Contexto, (SociedadDtm)parametros.registroEnBd);
                sociedad.SincronizarConTrabajadores(Contexto, (SociedadDtm)parametros.registroEnBd);
            }
        }

        protected override void EliminarCaches(SociedadDtm sociedad, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(sociedad, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Int_Sociedad, $"{sociedad.Id}");
            ServicioDeCaches.EliminarCache(CacheDe.Ter_NifDeProveedor);
            ServicioDeCaches.EliminarCache(CacheDe.Ter_NifDeCliente);
            ServicioDeCaches.EliminarCache(CacheDe.Valores);
        }

        protected override void AlDarDeBaja(SociedadDtm sociedad, ParametrosDeNegocio parametros)
        {
            base.AlDarDeBaja(sociedad, parametros);
            if (sociedad.EsGestionada(Contexto)) DarDeBajaLosCgsDeUnaSociedad(Contexto, Sociedad: sociedad);
            if (sociedad.EsInterlocutor) sociedad.DarDeBajaElInterlocutor<SociedadDtm>(Contexto);
            if (sociedad.TieneContactos(Contexto)) sociedad.DarDeBajaLosContactos(Contexto);
        }

        protected override void AntesDeMapearElRegistroParaInsertar(SociedadDto elemento, ParametrosDeNegocio opciones)
        {
            base.AntesDeMapearElRegistroParaInsertar(elemento, opciones);

            if (!opciones.Parametros.ContainsKey(nameof(SociedadDto.CrearInterlocutor)))
                opciones.Parametros.Add(nameof(SociedadDto.CrearInterlocutor), elemento.CrearInterlocutor);

            if (!opciones.Parametros.ContainsKey(nameof(SociedadDto.CrearProcurador)))
                opciones.Parametros.Add(nameof(SociedadDto.CrearProcurador), elemento.CrearProcurador);

            if (!opciones.Parametros.ContainsKey(nameof(SociedadDto.CrearAbogado)))
                opciones.Parametros.Add(nameof(SociedadDto.CrearAbogado), elemento.CrearAbogado);

            if (!opciones.Parametros.ContainsKey(nameof(SociedadDto.CrearCliente)))
                opciones.Parametros.Add(nameof(SociedadDto.CrearCliente), elemento.CrearCliente);

            if (!opciones.Parametros.ContainsKey(nameof(SociedadDto.CrearProveedor)))
                opciones.Parametros.Add(nameof(SociedadDto.CrearProveedor), elemento.CrearProveedor);
        }

        public static void AsociarCertificado(ContextoSe contexto, int idSociedad, int idCertificado)
        {
            var tran = contexto.IniciarTransaccion();
            try
            {
                var elementos = (elemento1: (RegistroDtm)null, elemento2: (RegistroDtm)null);
                var certificadoAnterior = "";
                if (GestorDeVinculos.Cantidad(contexto, enumNegocio.Sociedad, enumNegocio.Certificado, idSociedad) >= 1)
                {
                    var vinculados = GestorDeVinculos.RegistrosVinculados<CertificadoDtm>(contexto, enumNegocio.Sociedad, enumNegocio.Certificado, idSociedad);
                    elementos = GestorDeVinculos.BorrarVinculo(contexto, enumNegocio.Sociedad, enumNegocio.Certificado, idSociedad, vinculados[0].Id, new Dictionary<string, object>());
                    certificadoAnterior = ((INombre)elementos.elemento2).Nombre;
                }

                GestorDeVinculos.Vincular(contexto, enumNegocio.Sociedad, enumNegocio.Certificado, idSociedad, idCertificado);
                new TrazasDeUnaSociedadDtm
                {
                    IdElemento = idSociedad,
                    Nombre = !certificadoAnterior.IsNullOrEmpty() ? "Cambio de certificado" : "Añadido certificado",
                    Descripcion = !certificadoAnterior.IsNullOrEmpty()
                    ? $"El usuario {contexto.DatosDeConexion.Login} ha cambiado el certificado anterior '{((INombre)elementos.elemento2).Nombre}' por este '{contexto.SeleccionarPorId<CertificadoDtm>(idCertificado).Nombre}'"
                    : $"El usuario {contexto.DatosDeConexion.Login} ha añadido el certificado '{contexto.SeleccionarPorId<CertificadoDtm>(idCertificado).Nombre}'"
                }.InsertarTraza(contexto);

                contexto.Commit(tran);
            }
            catch (Exception exc)
            {
                contexto.Rollback(tran, exc);
                throw;
            }
        }

        protected override void DespuesDeMapearElElemento(SociedadDtm sociedad, SociedadDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(sociedad, elemento, parametros);

            elemento.TipoDeTercero = ApiDeTerceros.TipoDeTerceroEsp(sociedad.NIF).ToString();
            if (parametros.Parametros.LeerValor(ltrParametrosNeg.ObtenerDatosFiscales, false))
                elemento.DireccionFiscal = sociedad.DireccionFiscal(Contexto).Expresion;

            elemento.EsUnaDeMisSociedades = sociedad.EsGestionada(Contexto);
            if (elemento.EsUnaDeMisSociedades)
            {
                var certificados = sociedad.Vinculados<CertificadoDtm>(Contexto);
                if (certificados.Count == 1)
                {
                    elemento.Certificado = certificados[0].Expresion;
                }

                elemento.VerifactuEnProductivo = sociedad.EstaElVerifactuEnProductivo();
                elemento.UsaVerifactu = elemento.VerifactuEnProductivo || sociedad.UsaVerifactu();
            }

            if (parametros.Parametros.LeerValor(ltrDeSociedad.Interlocutores, new List<InterlocutorDtm>()).Count() == 1)
            {
                elemento.IdInterlocutor = parametros.Parametros.LeerValor(ltrDeSociedad.Interlocutores, new List<InterlocutorDtm>())[0].Id;
                if (parametros.Parametros.LeerValor(ltrDeSociedad.Clientes, new List<ClienteDtm>()).Count() == 1)
                    elemento.IdCliente = parametros.Parametros.LeerValor(ltrDeSociedad.Clientes, new List<ClienteDtm>())[0].Id;
                else
                if (parametros.Parametros.LeerValor(ltrDeSociedad.Proveedores, new List<ProveedorDtm>()).Count() == 1)
                    elemento.IdProveedor = parametros.Parametros.LeerValor(ltrDeSociedad.Proveedores, new List<ProveedorDtm>())[0].Id;
                else
                if (parametros.Parametros.LeerValor(ltrDeSociedad.Abogados, new List<AbogadoDtm>()).Count() == 1)
                    elemento.IdAbogado = parametros.Parametros.LeerValor(ltrDeSociedad.Abogados, new List<AbogadoDtm>())[0].Id;
                else
                if (parametros.Parametros.LeerValor(ltrDeSociedad.Procuradores, new List<ProcuradorDtm>()).Count() == 1)
                    elemento.IdProcurador = parametros.Parametros.LeerValor(ltrDeSociedad.Procuradores, new List<ProcuradorDtm>())[0].Id;
            }

            if (parametros.Peticion == enumPeticion.epLeerPorId)
            {
                var interlocutor = sociedad.Interlocutor(Contexto, crearSiNoLoHay: false, errorSiNoHay: false);
                if (interlocutor != null)
                {
                    elemento.IdInterlocutor = interlocutor?.Id ?? null;
                    elemento.Interlocutor = interlocutor.Expresion(Contexto);

                    var cliente = interlocutor.Cliente(Contexto, crearCliente: false);
                    elemento.IdCliente = cliente?.Id ?? null;
                    elemento.Cliente = cliente?.Expresion ?? "";

                    var proveedor = interlocutor.Proveedor(Contexto, crearProveedor: false);
                    elemento.IdProveedor = proveedor?.Id ?? null;
                    elemento.Proveedor = proveedor?.Expresion ?? "";
                }
            }
        }


        public string ActivarVerifactu(ContextoSe contexto, int idSociedad)
        {
            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);

            if (!sociedad.EsGestionada(Contexto))
                GestorDeErrores.Emitir($"La sociedad '{sociedad.Referencia}' no es gestionada por el sistema, no se puede activar Verifactu");

            var verifactuEnProductivo = sociedad.EstaElVerifactuEnProductivo();

            if (verifactuEnProductivo)
                GestorDeErrores.Emitir($"La sociedad '{sociedad.Referencia}' ya tiene activado el servicio de Verifactu y está en productivo");

            if (!sociedad.UsaVerifactu())
            {
                PonerVerifactuEnDesarrollo(contexto, sociedad);
                return "Trabajo de activación de Verifactu sometido";
            }
            PonerVerifactuEnProductivo(contexto, sociedad);
            return "Verifactu en productivo";
        }

        private static void PonerVerifactuEnDesarrollo(ContextoSe contexto, SociedadDtm sociedad)
        {
            int ejercicio = DateTime.Now.Year;
            var facturasEnviadas = contexto.Set<FacturaEmtDtm>().Where(fae => fae.Ano == ejercicio &&
                            contexto.Set<VerifactuDtm>().Any(verifactu => verifactu.IdElemento == fae.Id) &&
                            contexto.Set<CentroGestorDtm>().Any(cg => cg.IdSociedad == sociedad.Id && cg.Id == fae.IdCg))
                .OrderBy(fae => fae.FacturadaEl).ThenBy(fae => fae.Id);

            var ultimaEnviada = facturasEnviadas.FirstOrDefault();
            var fechaDeUltimoEnvio = ultimaEnviada is null ? $"01-01-{ejercicio}".ParsearFecha() : ultimaEnviada.FacturadaEl;
            var idUltimoEnvio = ultimaEnviada is null ? 0 : ultimaEnviada.Id;

            var estadosNoValidos = new List<List<int>> { enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura.Lista(), enumEtapasDeFacturasEmt.FAE_Etapa_Anulada.Lista() }.UnirListas();

            var facturasNoEnviadas = contexto.Set<FacturaEmtDtm>().Where(fae => fae.Ano == ejercicio &&
                           !estadosNoValidos.Contains(fae.IdEstado) &&
                           !contexto.Set<VerifactuDtm>().Any(verifactu => verifactu.IdElemento == fae.Id) &&
                           contexto.Set<CentroGestorDtm>().Any(cg => cg.IdSociedad == sociedad.Id && cg.Id == fae.IdCg) &&
                           (fae.FacturadaEl == null ? false : fae.FacturadaEl >= fechaDeUltimoEnvio) &&
                           fae.Id > idUltimoEnvio)
                   .OrderBy(fae => fae.FacturadaEl).ThenBy(fae => fae.Id).ToList();

            var loteDeEnvio = facturasNoEnviadas.Select(fae => fae.Id);
            var idSemaforo = ActivarVerifactu(contexto, sociedad);
            TrabajosDeFacturasEmt.SometerEnvioDeLoteDeFacturaAeat(contexto, loteDeEnvio.ToList(), sociedad.Id, idSemaforo);
        }

        private void PonerVerifactuEnProductivo(ContextoSe contexto, SociedadDtm sociedad)
        {
            GeneradorSii.BlanquearDirectorios(sociedad);
            sociedad.PonerElVerifactuEnProductivo();
            enumNegocio.FacturaEmitida.ResetearParametro(contexto, enumParametrosDeFacturasEmt.SII_URL_DE_VERIFICACION, valor: EndpointsDe.Prod);
            enumNegocio.FacturaEmitida.ResetearParametro(contexto, enumParametrosDeFacturasEmt.FAE_URL_GenerarQR, valor: EndpointsDe.ProdValidate);
            enumNegocio.FacturaEmitida.ResetearParametro(contexto, enumParametrosDeFacturasEmt.FAE_URL_GenerarQRNoVerifactu, valor: EndpointsDe.ProdNoVerifactu);
        }

        public void RecomponerBlockChain(ContextoSe contexto, int idSociedad)
        {
            if (!Contexto.SePuedeParametrizar())
                GestorDeErrores.Emitir("El usuario ha de ser parametrizadaro para ejecutar la recomposición de fichero de BlockChain");

            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);

            if (!sociedad.EsGestionada(Contexto))
                GestorDeErrores.Emitir($"La sociedad '{sociedad.Referencia}' no es gestionada por el sistema, no se puede recomponer su blockChain de emisión de facturas");
            var tran = contexto.IniciarTransaccion();
            try
            {
                new GeneradorSii(contexto, sociedad).RecomponerBlockChain();
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }

        public void CatalogosJudiciales(ContextoSe contexto, int idSociedad)
        {
            if (!Contexto.SePuedeParametrizar())
                GestorDeErrores.Emitir("El usuario ha de ser parametrizadaro para ejecutar la obteneción de catálogos judiciales");

            var sociedad = contexto.SeleccionarPorId<SociedadDtm>(idSociedad);

            if (!sociedad.EsGestionada(Contexto))
                GestorDeErrores.Emitir($"La sociedad '{sociedad.Referencia}' no es gestionada por el sistema, no se puede obtener sus catálogos judiciales");
            var tran = contexto.IniciarTransaccion();
            try
            {
                TrabajosDeProcedimientos.SometerLexnetActualizarCatalogos(contexto, idSociedad);
                contexto.Commit(tran);
            }
            catch
            {
                contexto.Rollback(tran);
                throw;
            }
        }
        private static int ActivarVerifactu(ContextoSe contexto, SociedadDtm sociedad)
        {
            var implantacionJson = ParametrosDelSii.SSII_Implantacion;
            try
            {
                try
                {
                    ExtensorDeFacturasEmt.ValidarEstructuraParaAlmacenarAuditoriaSii(contexto);
                    GeneradorSii.CrearDirectorios(sociedad);

                    sociedad.ValidarCertificado(contexto);
                    var valor = sociedad.SII_ActivarVerifactuEnLaSociedad(contexto);
                    enumNegocio.FacturaEmitida.ResetearParametro(contexto, enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION, valor);
                }
                catch
                {
                    ServicioDeCaches.EliminarCache(CacheDe.Negocio_ParametrosDeNegocio);
                    throw;
                }
                var idSemaforo = SemaforoDeProcesoSql.PonerSemaforo(contexto, enumNegocio.Sociedad.IdNegocio(), sociedad.Id, enumOpercionesDeSemaforo.VERI, sociedad.Referencia).Id;
                return idSemaforo;
            }
            catch
            {
                enumNegocio.FacturaEmitida.ResetearParametro(contexto, enumParametrosDeFacturasEmt.SII_SSII_IMPLANTACION, implantacionJson);
                throw;
            }
        }
    }
}
