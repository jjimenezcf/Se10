
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Gestor.Errores;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using ServicioDeDatos.Contabilidad;
using ServicioDeDatos.Elemento;
using ServicioDeDatos.MaestrosTecnico;
using ServicioDeDatos.Ventas;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using Utilidades;

namespace ServicioDeDatos.Terceros
{

    public static class ltrDeFacturadorDeSociedad
    {
        public const string jsonDeMapeo = @"{
  ""ClaseDeEmision"": ""eFactura322"",
  ""Unidades"": [
    { ""clave"": ""Unidad"", ""valor"": 1 },
    { ""clave"": ""Hora"", ""valor"": 2 }
  ],
  ""Naturaleza"": [
    { ""clave"": ""Materiales"", ""valor"": 1 },
    { ""clave"": ""Servicios"", ""valor"": 2 }
   ],
  ""Ivas"": [
    { ""clave"": ""21"", ""valor"": 1 },
    { ""clave"": ""10"", ""valor"": 2 }
  ],
  ""Irpfs"": [
    { ""clave"": ""15"", ""valor"": 1 },
    { ""clave"": ""7"", ""valor"": 2 }
   ]
}";
    }

    public class MapeoDelFacturador
    {
        public string Clave { get; set; }
        public int Valor { get; set; }
    }

    public class MapeosDelFacturador
    {
        public enumClaseDeEmision ClaseDeEmision { get; set; }
        public List<MapeoDelFacturador> Unidades { get; set; }
        public List<MapeoDelFacturador> Naturalezas { get; set; }
        public List<MapeoDelFacturador> Ivas { get; set; }
        public List<MapeoDelFacturador> Irpfs { get; set; }
    }

    [Table(Tablas.FACTURADOR, Schema = Esquemas.TERCEROS)]
    public class FacturadorDeSociedadDtm : RegistroDtm, IDetalle, IUsaActiva
    {
        public int IdElemento { get; set; }
        public SociedadDtm Elemento { get; set; }
        IElementoDtm IDetalle.Elemento => Elemento;

        public int IdTipoDeFactura { get; set; }

        public TipoDeFacturaEmtDtm TipoDeFactura { get; set; }

        public int IdCg { get; set; }

        public CentroGestorDtm Cg { get; set; }

        public string ApiKey { get; set; }

        public string MapeosJson { get; set; }

        public bool Activa { get; set; }

        public enumNegocio Negocio => enumNegocio.Sociedad;

        public MapeosDelFacturador ParsearMapeos()
        {
            var json = extJson.Deserializar(MapeosJson);
            var mapeos = new MapeosDelFacturador();

            foreach (var parClaveValor in json)
            {
                string nombreDePropiedad = parClaveValor.Key;
                JToken valorJson = parClaveValor.Value;

                 object valorFinal = null;

                if (nombreDePropiedad == nameof(MapeosDelFacturador.ClaseDeEmision))
                {
                    if (valorJson.Type == JTokenType.String)
                    {
                        valorFinal = Enum.Parse(typeof(enumClaseDeEmision), valorJson.Value<string>(), true);
                    }
                }
                else if (valorJson.Type == JTokenType.Array)
                {
                    valorFinal = valorJson.ToObject<List<MapeoDelFacturador>>();
                }

                if (valorFinal != null)
                {
                    mapeos.EscribirPropiedad(nombreDePropiedad, valorFinal);
                }
            }
            return mapeos;
        }
    }

    public static class ValidadoresDeMapeosJsonDeFacturadorDeSociedades
    {
        public static void ValidarClaseDeEmision(ContextoSe contexto, JToken valorJson)
        {
            string clase = valorJson.ToString();

            if (string.IsNullOrWhiteSpace(clase))
            {
                GestorDeErrores.Emitir($"El campo '{nameof(FacturaEmtDtm.ClaseDeEmision)}' en el JSON ha de ser alguno de los enumerados de {nameof(enumClaseDeEmision)}.");
            }

            var valor = ApiDeEnsamblados.ToEnumerado<enumClaseDeEmision>(clase, errorSiNoEsValido: false);
            if (valor == null) GestorDeErrores.Emitir($"El valor asignado a '{nameof(FacturaEmtDtm.ClaseDeEmision)}', '{clase}', no es válido, debe ser alguno de los enumerados de {nameof(enumClaseDeEmision)}.");
        }

        public static void ValidarUnidades(ContextoSe contexto, JToken valorJson)
        {
            if (valorJson.Type != JTokenType.Array)
            {
                GestorDeErrores.Emitir($"El campo '{nameof(MapeosDelFacturador.Unidades)}' a de ser un array de (clave, valor), donde valor es la sigla de la unidad en el sitema de elementos.");
            }

            var unidades = valorJson.ToObject<List<MapeoDelFacturador>>();

            foreach (var unidad in unidades)
            {
                if (!contexto.Set<UnidadDtm>().Any(u => u.Id == unidad.Valor))
                    GestorDeErrores.Emitir($"El campo '{nameof(MapeosDelFacturador.Unidades)}' a de tener siglas válidas del maestro de Unidades, item no válido '{unidad.Clave},{unidad.Valor}'.");
            }
        }

        public static void ValidarNaturalezas(ContextoSe contexto, JToken valorJson)
        {
            if (valorJson.Type != JTokenType.Array)
            {
                GestorDeErrores.Emitir($"El campo '{nameof(MapeosDelFacturador.Naturalezas)}' a de ser un array de (clave, valor), donde valor es la sigla de la naturaleza en el sitema de elementos.");
            }

            var naturalezas = valorJson.ToObject<List<MapeoDelFacturador>>();

            foreach (var naturaleza in naturalezas)
            {
                if (!contexto.Set<NaturalezaDtm>().Any(u => u.Id == naturaleza.Valor))
                    GestorDeErrores.Emitir($"El campo '{nameof(MapeosDelFacturador.Naturalezas)}' a de tener siglas válidas del maestor de Naturalezas, item no válido '{naturaleza.Clave},{naturaleza.Valor}'.");
            }
        }

        public static void ValidarIvas(ContextoSe contexto, JToken valorJson)
        {
            if (valorJson.Type != JTokenType.Array)
            {
                GestorDeErrores.Emitir($"El campo '{nameof(MapeosDelFacturador.Ivas)}' a de ser un array de (clave, valor), donde valor es el id del iva en el sitema de elementos.");
            }

            var ivas = valorJson.ToObject<List<MapeoDelFacturador>>();

            foreach (var iva in ivas)
            {
                if (!contexto.Set<IvaRepercutidoDtm>().Any(u => u.Id == iva.Valor))
                    GestorDeErrores.Emitir($"El campo '{nameof(MapeosDelFacturador.Ivas)}' a de tener ids válidos del maestor de Ivas Repercutido, item no válido '{iva.Clave},{iva.Valor}'.");
            }
        }

        public static void ValidarIrpfs(ContextoSe contexto, JToken valorJson)
        {
            if (valorJson.Type != JTokenType.Array)
            {
                GestorDeErrores.Emitir($"El campo '{nameof(MapeosDelFacturador.Irpfs)}' a de ser un array de (clave, valor), donde valor es el id del irpf en el sitema de elementos.");
            }

            var irpfs = valorJson.ToObject<List<MapeoDelFacturador>>();

            foreach (var irpf in irpfs)
            {
                if (!contexto.Set<IrpfDtm>().Any(u => u.Id == irpf.Valor))
                    GestorDeErrores.Emitir($"El campo '{nameof(MapeosDelFacturador.Irpfs)}' a de tener ids válidos del maestor de Irpfs, item no válido '{irpf.Clave},{irpf.Valor}'.");
            }
        }
    }


    public static partial class ModeloDeTerceros
    {
        public static void FacturadorDeSociedades(ModelBuilder modelBuilder)
        {
            ApiDeRegistroDtm.DefinirCampoIdDtm<FacturadorDeSociedadDtm>(modelBuilder);
            ApiDeElementoDtm.DefinirCamposDeDetalle<FacturadorDeSociedadDtm>(modelBuilder);

            ApiDeRegistroDtm.DefinirCampoFk<FacturadorDeSociedadDtm>(modelBuilder, nameof(FacturadorDeSociedadDtm.TipoDeFactura), nameof(FacturadorDeSociedadDtm.IdTipoDeFactura), ICampos.ID_TIPO_FACTURA, requerida: true, unico: false);
            ApiDeElementoDtm.DefinirCampoCg<FacturadorDeSociedadDtm>(modelBuilder, nameof(FacturadorDeSociedadDtm.Cg));

            modelBuilder.Entity<FacturadorDeSociedadDtm>().Property(x => x.Activa).HasColumnName(ICampos.ACTIVA).HasColumnType(IDominio.BIT).IsRequired(true).HasDefaultValue(true);
            modelBuilder.Entity<FacturadorDeSociedadDtm>().Property(x => x.MapeosJson).HasColumnName(ICampos.MAPEOS_JSON).HasColumnType(IDominio.VARCHAR_MAX).IsRequired(true);
            modelBuilder.Entity<FacturadorDeSociedadDtm>().Property(x => x.ApiKey).HasColumnName(ICampos.APIKEY).HasColumnType(IDominio.VARCHAR_250).IsRequired(true);

            modelBuilder.Entity<FacturadorDeSociedadDtm>().HasIndex(new string[] { nameof(FacturadorDeSociedadDtm.IdElemento), nameof(FacturadorDeSociedadDtm.ApiKey) }).IsUnique(true).HasDatabaseName($"I_{Tablas.FACTURADOR}_{ICampos.ID_ELEMENTO}_{ICampos.APIKEY}");
            modelBuilder.Entity<FacturadorDeSociedadDtm>().HasIndex(new string[] { nameof(FacturadorDeSociedadDtm.IdElemento), nameof(FacturadorDeSociedadDtm.IdCg), nameof(FacturadorDeSociedadDtm.IdTipoDeFactura) }).IsUnique(true).HasDatabaseName($"I_{Tablas.FACTURADOR}_{ICampos.ID_ELEMENTO}_{ICampos.ID_CG}_{ICampos.ID_TIPO_FACTURA}");
        }


    }
}
