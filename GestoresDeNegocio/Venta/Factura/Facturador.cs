using AutoMapper;
using Gestor.Errores;
using GestorDeElementos;
using GestorDeElementos.Extensores;
using GestoresDeNegocio.Terceros;
using GestoresDeNegocio.Venta.Factura;
using ModeloDeDto.Ventas;
using ServicioDeDatos;
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
                peticion.SolicitadaEl = DateTime.Now;
            }
        }

        protected override void DespuesDeMapearElElemento(PeticionDeFacturaEmtDtm peticion, PeticionDeFacturaEmtDto elemento, ParametrosDeNegocio parametros)
        {
            base.DespuesDeMapearElElemento(peticion, elemento, parametros);
            elemento.Id = peticion.Id;
            elemento.Facturador = peticion.Facturador(Contexto).Nombre(Contexto);
            elemento.NumeroFactura = peticion.Factura(Contexto)?.NumeroDeFactura;
        }

        public static PeticionDeFacturaEmtDtm ObtenerFacturador(ContextoSe contexto, string nifEmisor, string apiKey, enumOperacionFacturador operacion, string validadorJson = null)
        {

            var sociedad = contexto.SeleccionarPorPropiedad<SociedadDtm>(nameof(SociedadDtm.NIF), nifEmisor, errorSiNoHay: false);
            if (sociedad is null)
                GestorDeErrores.Emitir($"La sociedad '{nifEmisor}' no está dada de alta o no está activa en la BD");

            var facturadores = sociedad.Detalles<FacturadorDeSociedadDtm>(contexto);
            var facturador = facturadores.FirstOrDefault(x => x.ApiKey == apiKey);
            if (facturador == null)
                GestorDeErrores.Emitir($"La sociedad '{nifEmisor}' no tiene ningún facturador para el ApiKey '{apiKey}' proporcionado");

            GestorDeFacturadorDeSociedades.ValidarApiKey(facturador.IdElemento, facturador.IdCg, facturador.IdTipoDeFactura, apiKey);

            var peticion = new PeticionDeFacturaEmtDtm
            {
                IdFacturador = facturador.Id,
                Peticion = operacion,
                ValidadorJson = validadorJson
            }.Insertar(contexto);

            return peticion;
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
