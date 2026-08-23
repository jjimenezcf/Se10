using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.Venta.Factura;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
using ServicioDeDatos.SistemaDocumental;
using ServicioDeDatos.Terceros;
using ServicioDeDatos.Ventas;
using System;
using System.Linq;
using Utilidades;

namespace GestoresDeNegocio.Ventas
{


    public class Facturador : GestorDeElementos<ContextoSe, PeticionDeFacturaEmtDtm, PeticionDeFacturaEmtDto>
    {
        public override enumNegocio Negocio => enumNegocio.No_Definido;

        public class MapearFacturador : Profile
        {
            public MapearFacturador()
            {
                CreateMap<PeticionDeFacturaEmtDtm, PeticionDeFacturaEmtDto>();
                CreateMap<PeticionDeFacturaEmtDto, PeticionDeFacturaEmtDtm>();
            }
        }

        public Facturador(ContextoSe contexto, IMapper mapeador)
        : base(contexto, mapeador)
        {
        }

        public static Facturador Gestor(ContextoSe contexto, IMapper mapeador)
        {
            return new Facturador(contexto, mapeador);
        }

        protected override void AntesDePersistir(PeticionDeFacturaEmtDtm peticion, ParametrosDeNegocio parametros)
        {
            base.AntesDePersistir(peticion, parametros);
            if (parametros.Insertando)
            {
                peticion.Guid = Guid.NewGuid();
                peticion.GuidDeConsultaPdf = Guid.NewGuid();
                peticion.GuidDeConsultaXml = Guid.NewGuid();
                peticion.SolicitadaEl = DateTime.Now;
            }
        }

        protected override void DespuesDeMapearElElemento(PeticionDeFacturaEmtDtm peticion, PeticionDeFacturaEmtDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(peticion, elemento, parametros);
            elemento.Id = peticion.Id;
            elemento.Facturador = peticion.Facturador(Contexto).Nombre(Contexto);
            elemento.NumeroFactura = peticion.Factura(Contexto)?.NumeroDeFactura;
            if (peticion.IdFactura != null)
                elemento.UrlDeLaFactura = enumNegocio.FacturaEmitida.ComponerUrlPorId(Contexto, peticion.IdFactura.Value).ToString();
        }

        private static FacturadorDeSociedadDtm ValidarFacturador(ContextoSe contexto, string nifEmisor, string apiKey)
        {
            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nifEmisor, errorSiNoHay: false);
            if (sociedad is null)
                GestorDeErrores.Emitir($"La sociedad '{nifEmisor}' no está dada de alta o no está activa en la BD");

            var facturadores = sociedad.Detalles<FacturadorDeSociedadDtm>(contexto);
            var facturador = facturadores.FirstOrDefault(x => x.ApiKey == apiKey);
            if (facturador == null)
                GestorDeErrores.Emitir($"La sociedad '{nifEmisor}' no tiene ningún facturador para el ApiKey '{apiKey}' proporcionado");

            GestorDeFacturadorDeSociedades.ValidarApiKey(facturador.IdElemento, facturador.IdCg, facturador.IdTipoDeFactura, apiKey);

            return facturador;
        }

        public static PeticionDeFacturaEmtDtm ObtenerFacturador(ContextoSe contexto, string nifEmisor, string apiKey, enumOperacionFacturador operacion, string validadorJson = null)
        {
            var facturador = ValidarFacturador(contexto, nifEmisor, apiKey);

            var peticion = new PeticionDeFacturaEmtDtm
            {
                IdFacturador = facturador.Id,
                Peticion = operacion,
                ValidadorJson = validadorJson
            }.Insertar(contexto);

            return peticion;
        }

        public static string ObtenerUrlDeDescargaDeDocumento(ContextoSe contexto, string nifEmisor, string apiKey, string numeroFactura, string guid, enumOperacionFacturador operacion)
        {
            var facturadorDeSociedad = ValidarFacturador(contexto, nifEmisor, apiKey);

            var peticion = contexto.Set<PeticionDeFacturaEmtDtm>()
                .Where(p => p.IdFacturador == facturadorDeSociedad.Id && p.IdFactura != null)
                .ToList()
                .FirstOrDefault(p => contexto.SeleccionarPorId<FacturaEmtDtm>(p.IdFactura.Value, usarLaCache: false)?.NumeroDeFactura == numeroFactura);

            if (peticion is null)
                GestorDeErrores.Emitir($"No se ha encontrado ninguna factura con el número '{numeroFactura}' para el facturador indicado");

            var guidEsperado = operacion == enumOperacionFacturador.SolicitarPdf ? peticion.GuidDeConsultaPdf : peticion.GuidDeConsultaXml;
            if (guidEsperado == null || guidEsperado.ToString() != guid)
                GestorDeErrores.Emitir($"El guid proporcionado no corresponde con la factura número '{numeroFactura}'");

            // Se lee sin caché: esta petición puede llegar mucho después de haberse creado/firmado la factura,
            // en una llamada http distinta a la que la emitió, y la caché de elementos podría tener una copia
            // desactualizada (p.ej. sin el IdArchivo asociado al firmar/generar el documento).
            var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(peticion.IdFactura.Value, usarLaCache: false);
            var esImpresa = factura.ClaseDeEmision == enumClaseDeEmision.Impresa;
            ArchivoDtm archivo;
            if (operacion == enumOperacionFacturador.SolicitarXml)
            {
                if (esImpresa)
                    GestorDeErrores.Emitir($"La factura '{factura.Referencia}' es de clase '{enumClaseDeEmision.Impresa.Descripcion()}', no tiene un xml asociado, solicite el pdf");

                archivo = factura.IdArchivo != null ? contexto.SeleccionarPorId<ArchivoDtm>(factura.IdArchivo.Value, errorSiNoHay: false) : null;
                if (archivo is null)
                    GestorDeErrores.Emitir($"No se tiene un xml de la factura '{factura.Referencia}'");
            }
            else
            {
                if (esImpresa)
                {
                    archivo = factura.IdArchivo != null ? contexto.SeleccionarPorId<ArchivoDtm>(factura.IdArchivo.Value, errorSiNoHay: false) : null;
                    if (archivo is null)
                        GestorDeErrores.Emitir($"No se tiene un pdf de la factura '{factura.Referencia}'");
                }
                else
                {
                    var archivos = GestorDeVinculos.RegistrosVinculados<ArchivoDtm>(contexto, enumNegocio.FacturaEmitida, enumNegocio.Archivos, factura.Id);
                    archivo = archivos.FirstOrDefault(a => a.Nombre.Contains($"copia-{factura.Referencia}", StringComparison.OrdinalIgnoreCase) && a.Nombre.Contains("firmado", StringComparison.OrdinalIgnoreCase) && a.Nombre.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
                           ?? archivos.FirstOrDefault(a => a.Nombre.Contains($"copia-{factura.Referencia}", StringComparison.OrdinalIgnoreCase) && a.Nombre.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase));
                    if (archivo is null)
                        GestorDeErrores.Emitir($"Para la factura '{numeroFactura}' hay '{archivos.Count}' archivo/s, ninguno de ellos tiene en el nombre la referencia '{factura.Referencia}' y es de tipo pdf");
                }
            }

            var guidDeDescarga = archivo.RegistrarDescargaConGuid(contexto, DateTime.Now.AddHours(1), null);

            var uri = new UriBuilder(CacheDeVariable.Cfg_UrlBase)
            {
                Path = $"/{nameof(enumControladoresSistemaDocumental.Archivos)}/{ltrEndPoint.epDescargaConGuid}",
                Query = $"guid={guidDeDescarga}&id={archivo.Id}"
            };
            return uri.ToString();
        }


        public static PeticionDeFacturaEmtDto CrearFactura(ContextoSe contexto, PeticionDeFacturaEmtDtm facturador, string facturaJson)
        {

            facturador.FacturaJson = facturaJson;

            PeticionDeFacturaEmtDto resultado = null;
            try
            {
                var prefactura = ExtensorDeFacturasEmt.CrearPrefacturaDeUnJson(contexto, facturador.Facturador(contexto), facturaJson);
                try
                {
                    var factura = prefactura.TransitarALaEtapa(contexto, enumEtapasDeFacturasEmt.FAE_Etapa_Emitida.EstadosDeLaEtapa(), new System.Collections.Generic.Dictionary<string, object>());
                    facturador.IdFactura = factura.Id;
                    resultado = facturador.MapearDto<PeticionDeFacturaEmtDto>(contexto);

                    if (factura.UsaVerifactu(contexto) && GeneradorSii.VerifactuActivo(contexto, factura))
                    {
                        var envioDeFactura = GestorDeFacturasEmt.EnviarFacturaAeat(contexto, factura.Id, someterEnvio: true);
                        if (envioDeFactura)
                        {
                            resultado.Mensaje = ltrFacturador.SometidoEnvioDeFactura;
                        }
                        else
                            resultado.Mensaje = ltrFacturador.SometidoLoteDeEnvio;
                    }
                    else
                    {
                        GestorDeFacturasEmt.EmitirPdfFactura(contexto, factura.MapearDto<FacturaEmtDto>(contexto));
                    }
                }
                catch (Exception ex)
                {
                    facturador.IdFactura = prefactura.Id;
                    throw new Exception($"Creada prefactura '{prefactura.Referencia}' pero no se ha emitido por:{Environment.NewLine}{ex.MensajeCompleto()}");
                }
            }
            catch (Exception ex)
            {
                if (resultado is null)
                {
                    resultado = facturador.MapearDto<PeticionDeFacturaEmtDto>(contexto);
                }
                resultado.Mensaje = ex.MensajeCompleto();
            }

            facturador.Error = resultado.Mensaje.Contains(ltrFacturador.SometidoEnvioDeFactura) ||
                resultado.Mensaje.Contains(ltrFacturador.SometidoLoteDeEnvio) ||
                resultado.Mensaje.Contains(ltrFacturador.NoUsaVerifactu)
                ? null
                : resultado.Mensaje;

            facturador = facturador.Modificar(contexto);
            return resultado;
        }

        public static PeticionDeFacturaEmtDto CrearFactura(ContextoSe contexto, string nifEmisor, string guid, string facturaJson)
        {
            var facturadorAsociado = ExtensorDelFacturador.FacturadorDeUnGuid(contexto, Guid.Parse(guid));

            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nifEmisor, errorSiNoHay: false);
            if (sociedad is null)
                GestorDeErrores.Emitir($"La sociedad '{nifEmisor}' no está dada de alta o no está activa en la BD");

            if (facturadorAsociado.IdElemento != sociedad.Id)
                GestorDeErrores.Emitir($"La sociedad '{nifEmisor}' no corresponde con la del facturador");

            var facturador = contexto.SeleccionarPorPropiedad<PeticionDeFacturaEmtDtm>(nameof(PeticionDeFacturaEmtDtm.Guid), guid, errorSiNoHay: true);

            if (facturador.SolicitadaEl.AddMinutes(1) < DateTime.Now)
            {
                GestorDeErrores.Emitir($"El guid operacional ha caducado por haber pasado más de 1 minuto desde su creación, solicite uno nuevo");
            }

            if (facturador.IdFactura != null)
            {
                var factura = contexto.SeleccionarPorId<FacturaEmtDtm>(facturador.IdFactura.Value, errorSiNoHay: true);
                GestorDeErrores.Emitir($"El guid operacional ya ha sido usado, y se le ha asociado la factura con referencia '{factura.Referencia}'" +
                    $"{(factura.EstaEnLaEtapa(enumEtapasDeFacturasEmt.FAE_Etapa_Prefactura) ? " y no se ha podido emitir": $"y se ha emitido con número '{factura.NumeroDeFactura}'")}");
            }

            if (facturador.Peticion != enumOperacionFacturador.CrearFactura)
            {
                GestorDeErrores.Emitir($"El guid operacional es para crear una factura,invoque correctamente la petición de '{facturador.Peticion.Descripcion()}'");
            }

            return CrearFactura(contexto, facturador, facturaJson);
        }
    }
}
