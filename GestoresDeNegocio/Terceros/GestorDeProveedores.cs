using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using Microsoft.EntityFrameworkCore;
using ModeloDeDto.Terceros;
using ModeloXml.eFactura.Facturae322;
using ServicioDeDatos;
using ServicioDeDatos.Callejero;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.Gastos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Terceros
{
    public class GestorDeProveedores : GestorDeElementos<ContextoSe, ProveedorDtm, ProveedorDto>
    {
        public override enumNegocio Negocio => enumNegocio.Proveedor;

        public class MapearProveedores : Profile
        {
            public MapearProveedores()
            {
                CreateMap<ProveedorDtm, ProveedorDto>();
                CreateMap<ProveedorDto, ProveedorDtm>()
                .ForMember(dtm => dtm.Expresion, dto => dto.Ignore())
                .ForMember(dtm => dtm.Tarjeta, dto => dto.Ignore())
                .ForMember(dtm => dtm.DomiciliadaEn, dto => dto.Ignore())
                .ForMember(dtm => dtm.Nombre, dto => dto.MapFrom(x => x.Expresion));
            }
        }

        public GestorDeProveedores(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static GestorDeProveedores Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new GestorDeProveedores(contexto, mapeador); ;
        }

        protected override IQueryable<ProveedorDtm> AplicarJoins(IQueryable<ProveedorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarJoins(consulta, filtros, parametros);
            consulta = consulta.Include(e => e.Interlocutor);
            return consulta;
        }

        protected override IQueryable<ProveedorDtm> AplicarFiltros(IQueryable<ProveedorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarFiltros(consulta, filtros, parametros);
            consulta = consulta.FiltrarPorNif(Contexto, filtros, emitirError: true);
            consulta = consulta.FiltrarPorExpresion(Contexto, filtros);
            consulta = consulta.FiltrarPorIdSociedad(filtros);
            consulta = consulta.FiltrarPorIdPersona(filtros);
            consulta = consulta.FiltrarPorVinculadosCon(Contexto, filtros);
            return consulta;
        }

        protected override IQueryable<ProveedorDtm> AplicarSeguridad(IQueryable<ProveedorDtm> consulta, List<ClausulaDeFiltrado> filtros, ParametrosDeNegocio parametros)
        {
            consulta = base.AplicarSeguridad(consulta, filtros, parametros);
            if (!Contexto.DatosDeConexion.EsAdministrador) consulta = FiltrarPorSeguridad.DeNegocio(Contexto, Negocio, consulta);
            return consulta;
        }

        protected override void AntesDePersistir(ProveedorDtm proveedor, ParametrosDeNegocio parametros)
        {
            var inter = Contexto.SeleccionarPorId<InterlocutorDtm>(proveedor.IdInterlocutor);
            proveedor.Nombre = inter.Expresion;
            base.AntesDePersistir(proveedor, parametros);

            if (!inter.Sociedad(Contexto).Autonomo && proveedor.IdIrpf is not null)
                GestorDeErrores.Emitir($"Las facturas de la sociedad '{inter.NIF(Contexto)}' no pueden tener retención de irpf");

            if (parametros.Insertando)
            {
                if (inter.EsPersona || inter.EsContacto)
                    GestorDeErrores.Emitir("Un proveedor sólo puede ser una sociedad");

                proveedor.CodigoContable = ( Contexto.Set<ProveedorDtm>().Max(x => x.CodigoContable) ?? 0) + 1;
            }
            if (parametros.Modificando)
            {
                proveedor.CodigoContable = ((ProveedorDtm)parametros.registroEnBd).CodigoContable;
            }

            if (proveedor.ModoDePago is null && (proveedor.IdDomiciliadaEn is not null || proveedor.IdTarjeta is not null))
                GestorDeErrores.Emitir($"No se puede indicar una tarjeta o cuenta de pago y no asignar modo de pago");

            if (proveedor.ModoDePago == enumModoDePagoContado.Tarjeta && proveedor.IdTarjeta is null)
                GestorDeErrores.Emitir($"No se puede asignar modo de pago '{enumModoDePagoContado.Tarjeta.Descripcion()}' y no indicar la tarjeta");

            if (proveedor.ModoDePago == enumModoDePagoContado.Domiciliacion && proveedor.IdDomiciliadaEn is null)
                GestorDeErrores.Emitir($"No se puede asignar modo de pago '{enumModoDePagoContado.Domiciliacion.Descripcion()}' y no indicar la cuenta de cargo");

            if ((proveedor.ModoDePago is not null && proveedor.ModoDePago == enumModoDePagoContado.Contado  && (proveedor.IdDomiciliadaEn is not null || proveedor.IdTarjeta is not null)))
                GestorDeErrores.Emitir($"No se puede asignar el modo de pago '{enumModoDePagoContado.Contado.Descripcion()}' y añadir una tarjeta o cuenta societaria");

        }


        protected override void DespuesDePersistir(ProveedorDtm proveedor, ParametrosDeNegocio parametros)
        {
            base.DespuesDePersistir(proveedor, parametros);
            if (parametros.Modificando) proveedor.TrazarModificaciones(Contexto, (ProveedorDtm)parametros.registroEnBd);
        }

        protected override void EliminarCaches(ProveedorDtm proveedor, ParametrosDeNegocio parametros)
        {
            base.EliminarCaches(proveedor, parametros);
            ServicioDeCaches.EliminarElemento(CacheDe.Ter_Interlocutor, $"{typeof(ProveedorDtm).Name}-{proveedor.Id}");
            ServicioDeCaches.EliminarElemento(CacheDe.Ter_NifDeProveedor, proveedor.Id.ToString());
            ServicioDeCaches.EliminarCache(CacheDe.Pag_DatosDelPagoDto);
            ServicioDeCaches.EliminarElemento(CacheDe.Int_Proveedor, proveedor.IdInterlocutor.ToString());
        }

        protected override void AlDarDeAlta(ProveedorDtm proveedor, ParametrosDeNegocio parametros)
        {
            base.AlDarDeAlta(proveedor, parametros);
            if (proveedor.Interlocutor(Contexto).Baja)
                GestorDeErrores.Emitir($"Debe dar de alta el interlocutor '{proveedor.Interlocutor(Contexto).Expresion(Contexto)}' antes de dar de alta el proveedor");
        }

        protected override void DespuesDeMapearElElemento(ProveedorDtm proveedor, ProveedorDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(proveedor, elemento, parametros);

            elemento.NIF = proveedor.NIF(Contexto);
            if (parametros.Peticion == enumPeticion.epLeerPorId || parametros.Parametros.LeerValor(ltrParametrosNeg.ObtenerDatosFiscales, false))
            {
                elemento.DireccionFiscal = proveedor.DireccionFiscal(Contexto, errorSiNoHay: false)?.Expresion;
                elemento.RazonSocial = proveedor.RazonSocial(Contexto);
                elemento.TipoFarPropuesto = Contexto.SeleccionarPorId<TipoDeFacturaRecDtm>(proveedor.IdTipoFarPropuesto.Entero(), errorSiNoHay: false)?.Nombre;
                elemento.CgPropuesto = Contexto.SeleccionarPorId<CentroGestorDtm>(proveedor.IdCgPropuesto.Entero(), errorSiNoHay: false)?.Expresion;
                elemento.DomiciliadaEn = proveedor.DomiciliadaEn(Contexto)?.Expresion(Contexto); 
                elemento.Tarjeta = proveedor.Tarjeta(Contexto)?.Expresion;
            }
            else if (parametros.CargarListaDinamica)
            {
                elemento.TipoFarPropuesto = Contexto.SeleccionarPorId<TipoDeFacturaRecDtm>(proveedor.IdTipoFarPropuesto.Entero(), errorSiNoHay: false)?.Nombre;
                elemento.CgPropuesto = Contexto.SeleccionarPorId<CentroGestorDtm>(proveedor.IdCgPropuesto.Entero(), errorSiNoHay: false)?.Expresion;
            }

        }

        public static List<ProveedorDtm> CrearProveedores(ContextoSe contexto, List<int> idsDeInter, int idCuenta)
        {
            var result = new List<ProveedorDtm>();
            foreach (var idInter in idsDeInter)
            {
                result.Add(ExtensorDeProveedores.CrearProveedor(contexto, idInter, idCuenta));
            }
            return result;
        }


        public static ProveedorDtm CrearProveedor(ContextoSe contexto, string nif, int idCuenta)
        {
            var inter = ExtensorDeInterlocutores.CrearInterlocutor(contexto, nif);
            return ExtensorDeProveedores.CrearProveedor(contexto, inter, idCuenta);
        }

        public static ProveedorDtm CrearProveedor(ContextoSe contexto, int idArchivo, string telefonoIn, string eMailIn, int? idMunicipio, int? IdTipoDeVia)
        {
            var archivo = contexto.SeleccionarPorId<ArchivoDtm>(idArchivo);
            var fichero = Path.Combine(archivo.AlmacenadoEn, $"{archivo.Id}.{ApiDeArchivos.ExtensionSe}");
            eFactura322.Parsear(fichero);
            var facturae = eFactura322.FromFile(fichero);

            var vendedor = facturae.Parties?.SellerParty;

            if (vendedor is null || vendedor.TaxIdentification is null)
            {
                GestorDeErrores.Emitir($"No se puede crear el proveedor por no estar identificado en la factura, etiqueta: 'SellerParty'");
            }

            if (vendedor.TaxIdentification.PersonTypeCode == PersonTypeCodeType.F || vendedor.TaxIdentification.PersonTypeCode == PersonTypeCodeType.J)
            {
                var datosProveedor = BuscarProveedor(contexto, vendedor);
                if (datosProveedor.Proveedor is not null)
                    return datosProveedor.Proveedor;

                ProveedorDtm proveedor;
                if (vendedor.TaxIdentification.PersonTypeCode == PersonTypeCodeType.F)
                {
                    var infoDeContacto = DatosDeContactoDelProveedor(datosProveedor.Persona.ContactDetails, telefonoIn, eMailIn);
                    proveedor = CrearDireccionFiscal(contexto, datosProveedor, infoDeContacto, idMunicipio, IdTipoDeVia);
                }
                else
                {
                    var infoDeContacto = DatosDeContactoDelProveedor(datosProveedor.Empresa.ContactDetails, telefonoIn, eMailIn);

                    proveedor = CrearDireccionFiscal(contexto, datosProveedor, infoDeContacto, idMunicipio, IdTipoDeVia);

                    var obs = enumNegocio.Proveedor.Observaciones(contexto).Any(x => x.IdElemento == proveedor.Id && x.Nombre == ltrDeUnaFacturaRec.ContactoImportado);

                    if (!obs && !infoDeContacto.Contacto.IsNullOrEmpty()) new ObservacionesDeUnProveedorDtm
                    {
                        IdElemento = proveedor.Id,
                        Nombre = ltrDeUnaFacturaRec.ContactoImportado,
                        Descripcion = infoDeContacto.Contacto
                    }.Insertar(contexto);
                }
                return proveedor;
            }

            throw new Exception($"No definido cómo importar un proveedor del tipo {vendedor.TaxIdentification.PersonTypeCode}");
        }

        private static ProveedorDtm CrearDireccionFiscal(ContextoSe contexto,
            (ProveedorDtm Proveedor, LegalEntityType Empresa, IndividualType Persona, string Nif, string Corporacion) datosProveedor,
            (string Telefono, string eMail, string Contacto) infoDeContacto,
            int? idMunicipio, int? IdTipoDeVia
        )
        {
            var direccion = datosProveedor.Empresa is not null ? (AddressType)datosProveedor.Empresa.Item : (AddressType)datosProveedor.Persona.Item;
            var pais = contexto.SeleccionarPorPropiedad<PaisDtm>(nameof(PaisDtm.Codigo), direccion.CountryCode.ToString(), errorSiNoHay: false);
            if (pais == null)
                GestorDeErrores.Emitir($"No se puede crear el proveedor ya que el código del pais '{direccion.CountryCode}' indicado en el fichero no está en la BD");

            var provincia = pais.Provincia(contexto, direccion.Province, errorSiNoExiste: false);
            if (provincia == null)
            {
                if (pais.ISO2 == ltrIsoPaises.Spain && direccion.PostCode.Length == 5 && direccion.PostCode.Entero() > 0)
                    provincia = new ProvinciaDtm
                    {
                        Codigo = direccion.PostCode.Substring(0, 2),
                        Nombre = direccion.Province,
                        IdPais = pais.Id,
                        Prefijo = infoDeContacto.Telefono.Substring(0, 2),
                        Sigla = direccion.Province.Substring(0, 2)
                    }.InsertarSiNoExiste(contexto,
                              new List<string> { nameof(ProvinciaDtm.IdPais), nameof(ProvinciaDtm.Codigo) },
                              parametros: new Dictionary<string, object> { { ltrParametrosNeg.AccionQueSeEjecuta, nameof(ExtensorDeProveedores.CrearProveedor) } }
                            );
                else
                    GestorDeErrores.Emitir($"No se puede crear el proveedor ya que la provincia '{direccion.Province}' indicada en el fichero no está en la BD");
            }

            var codigoPostal = new CodigoPostalDtm { Codigo = direccion.PostCode }.InsertarSiNoExiste(contexto, new List<string> { nameof(CodigoPostalDtm.Codigo) });
            var municipio = idMunicipio.Entero() > 0
              ? contexto.SeleccionarPorId<MunicipioDtm>((int)idMunicipio)
              : provincia.Municipio(contexto, direccion.Town, errorSiNoExiste: false);
            if (municipio == null)
            {
                if (pais.ISO2 == ltrIsoPaises.Spain && direccion.PostCode.Length == 5 && direccion.PostCode.Entero() > 0)
                    municipio = new MunicipioDtm
                    {
                        DC = direccion.PostCode.Right(3),
                        Nombre = direccion.Town,
                        IdProvincia = provincia.Id
                    }.Insertar(contexto, accionEjecutada: nameof(ExtensorDeProveedores.CrearProveedor));
                else
                    GestorDeErrores.Emitir($"No se puede crear el proveedor ya que la provincia '{direccion.Province}' indicada en el fichero no está en la BD");
            }


            var tipovia = IdTipoDeVia.Entero() > 0
              ? contexto.SeleccionarPorId<TipoDeViaDtm>((int)IdTipoDeVia)
              : BuscarTipoVia(contexto, direccion.Address);

            var calle = BuscarCrearCalle(contexto, municipio, tipovia, direccion.Address);

            if (datosProveedor.Empresa is not null)
            {

                var sociedad = new SociedadDtm
                {
                    Nombre = datosProveedor.Corporacion,
                    RazonSocial = datosProveedor.Corporacion,
                    NIF = datosProveedor.Nif,
                    eMail = infoDeContacto.eMail,
                    Telefono = infoDeContacto.Telefono
                }.InsertarSiNoExiste(contexto, new List<string> { nameof(SociedadDtm.NIF) });

                datosProveedor.Proveedor = ExtensorDeProveedores.CrearProveedor(contexto, sociedad);
            }
            else
            {
                var persona = new PersonaDtm
                {
                    Nombre = datosProveedor.Persona.Name,
                    Apellidos = datosProveedor.Persona.FirstSurname + " " + datosProveedor.Persona.SecondSurname,
                    NIF = datosProveedor.Nif,
                    EsNie = false,
                    eMail = infoDeContacto.eMail,
                    Telefono = infoDeContacto.Telefono,
                }.InsertarSiNoExiste(contexto, new List<string> { nameof(PersonaDtm.NIF) });

                datosProveedor.Proveedor = ExtensorDeProveedores.CrearProveedor(contexto, persona);
            }

            if (datosProveedor.Proveedor.DireccionFiscal(contexto, errorSiNoHay: false) is null)
                GestorDeDirecciones.Gestor(contexto, enumNegocio.Proveedor).PersistirRegistro(new DireccionDeUnProveedorDtm
                {
                    IdElemento = datosProveedor.Proveedor.Id,
                    IdPais = pais.Id,
                    IdProvincia = provincia.Id,
                    IdMunicipio = municipio.Id,
                    IdCalle = calle.Id,
                    IdCp = codigoPostal.Id,
                    Calificador = enumCalificadorDireccion.fiscal,
                    Numero = ComplementoDeDireccion(direccion.Address),
                    Otros = direccion.Address,
                    Negocio = enumNegocio.Proveedor
                }, new ParametrosDeNegocio(enumTipoOperacion.Insertar));

            return datosProveedor.Proveedor;
        }

        private static (ProveedorDtm Proveedor, LegalEntityType Empresa, IndividualType Persona, string Nif, string Nombre)
        BuscarProveedor(ContextoSe contexto, BusinessType vendedor)
        {
            string nombre, nif;
            ProveedorDtm proveedor;

            nif = vendedor.TaxIdentification.TaxIdentificationNumber;
            if (nif.IsNullOrEmpty())
                GestorDeErrores.Emitir($"No se puede crear el proveedor porque necesito el nif en la etiqueta 'TaxIdentificationNumber'");

            if (nif.StartsWith("ES") && vendedor.Item is IndividualType) nif = nif.Right(9);

            proveedor = contexto.SeleccionarPorPropiedad<ProveedorDtm>(nameof(SociedadDtm.NIF), nif, errorSiNoHay: false);
            LegalEntityType empresa = null;
            IndividualType persona = null;
            if (vendedor.Item is LegalEntityType)
            {
                empresa = (LegalEntityType)vendedor.Item;
                nombre = empresa?.CorporateName;

                if (nombre.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"No se puede crear el proveedor ya que no está identificada el nombre de la coorporación, etiqueta 'SellerParty.LegalEntityType.CorporateName'");
            }
            else
            {
                persona = (IndividualType)vendedor.Item;

                if (persona.FirstSurname.IsNullOrEmpty() || persona.Name.IsNullOrEmpty())
                    GestorDeErrores.Emitir($"No se puede crear el proveedor ya que no está identificada el nombre de la persona, etiqueta 'SellerParty.IndividualType'");

                nombre = persona.FirstSurname + " " + persona.SecondSurname + ", " + persona.Name;
            }

            return (proveedor, empresa, persona, nif, nombre);
        }

        private static (string Telefono, string eMail, string Contacto) DatosDeContactoDelProveedor(ContactDetailsType datosDeContacto, string telefonoIn, string eMailIn)
        {
            string telefono, eMail, contacto;

            if ((telefonoIn.IsNullOrEmpty() || eMailIn.IsNullOrEmpty()) && datosDeContacto is null)
                GestorDeErrores.Emitir($"No se puede crear el proveedor ya que en el xml no se identifican los datos del contacto, emali y teléfono, etiqueta: 'ContactDetails', infórmelos en pantalla");

            telefono = !telefonoIn.IsNullOrEmpty() ? telefonoIn : datosDeContacto.Telephone;
            eMail = !eMailIn.IsNullOrEmpty() ? eMailIn : datosDeContacto.ElectronicMail;
            contacto = datosDeContacto?.ContactPersons;
            if (telefono.IsNullOrEmpty() || eMail.IsNullOrEmpty())
                GestorDeErrores.Emitir($"No se puede crear el proveedor ya que no está indicado ni el teléfono ni el eMail");

            return (telefono, eMail, contacto);
        }

        private static int? ComplementoDeDireccion(string address)
        {
            var partes = address.Split(',');
            if (partes.Length > 1 && partes[1].EsNumero()) { return partes[1].Entero(); }
            return null;
        }

        private static CalleDtm BuscarCrearCalle(ContextoSe contexto, MunicipioDtm municipio, TipoDeViaDtm tipoVia, string address)
        {
            var nombreDeCalle = address;
            var partes = address.Split(' ');
            if (partes.Length > 1 && partes[0].Contains(tipoVia.Sigla, StringComparison.InvariantCultureIgnoreCase) && (partes[0].Length == tipoVia.Sigla.Length || partes[0].Length == tipoVia.Sigla.Length + 1))
                nombreDeCalle = string.Join(" ", partes.Skip(1)).Trim();

            if (nombreDeCalle.StartsWith(tipoVia.Sigla))
            {
                nombreDeCalle = nombreDeCalle.Substring(tipoVia.Sigla.Length - 1);
                if (nombreDeCalle.StartsWith(".")) nombreDeCalle = nombreDeCalle.Substring(1);
                if (nombreDeCalle.StartsWith("/")) nombreDeCalle = nombreDeCalle.Substring(1);
                if (nombreDeCalle.StartsWith(".")) nombreDeCalle = nombreDeCalle.Substring(1);
                if (nombreDeCalle.StartsWith("/.")) nombreDeCalle = nombreDeCalle.Substring(2);
            }

            if (nombreDeCalle.Split(",").Length > 1)
                nombreDeCalle = nombreDeCalle.Split(",")[0];

            return new CalleDtm { Nombre = nombreDeCalle, IdMunicipio = municipio.Id, IdTipoDeVia = tipoVia.Id }.InsertarSiNoExiste(contexto, new List<string> {
               nameof(CalleDtm.Nombre),
               nameof(CalleDtm.IdTipoDeVia),
               nameof(CalleDtm.IdMunicipio),
           });
        }

        private static TipoDeViaDtm BuscarTipoVia(ContextoSe contexto, string direccion)
        {
            var partes = direccion.Split(' ');
            if (partes.Length == 1)
                GestorDeErrores.Emitir($"La etiqueta Address no identifica el tipo de vía");
            var ultimo = partes[0].Right(1);
            var sigla = ultimo == "." || ultimo == "/" ? partes[0].Left(partes[0].Length - 1) : partes[0];

            ultimo = sigla.Right(1);
            sigla = ultimo == "/" ? sigla.Left(sigla.Length - 1) : sigla;

            var tipoVia = contexto.SeleccionarPorPropiedad<TipoDeViaDtm>(nameof(TipoDeViaDtm.Sigla), sigla, errorSiNoHay: false);

            if (tipoVia == null)
                GestorDeErrores.Emitir($"No se puede crear el proveedor ya que no se localiza el tipo de vía '{sigla}' indicado en la dirección fiscal '{direccion}'");

            return tipoVia;
        }

    }

}
