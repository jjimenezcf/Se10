

using Gestor.Errores;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using Utilidades;

namespace ServicioDeDatos
{
    public enum enumDtmsDeRelacion { Negocio1, Negocio2 }
    public static class ltrDtm
    {

    }

    public enum ModoDeOrdenancion { ascendente, descendente }


    public class ClausulaDeOrdenacion
    {
        public string OrdenarPor { get; set; }
        public ModoDeOrdenancion Modo { get; set; }

        public string ModoBd => Modo == ModoDeOrdenancion.ascendente ? "ASC" : "DESC";
        public ClausulaDeOrdenacion()
        {

        }
        public ClausulaDeOrdenacion(string ordenarPor, ModoDeOrdenancion modo)
        {
            OrdenarPor = ordenarPor;
            Modo = modo;
        }
    }


    public enum enumCriteriosDeFiltrado { igual, mayor, menor, esNulo, noEsNulo, contiene, noContiene, comienza, termina, mayorIgual, menorIgual, diferente, esAlgunoDe, noEsNingunoDe, entreFechas, porReferencia, porMismaReferencia, deRelacion, deTipos, entreImportes, noEstaRelacionado, entreRangos, porDiferentesPropiedades }

    public class ClausulaDeFiltrado
    {
        public string Clausula { get; set; }

        public enumCriteriosDeFiltrado Criterio { get; set; }

        private string _valor = "";

        public string Valor { get { return _valor.Trim(); } set { _valor = value == null ? "" : value; } }

        public bool Aplicado { get; set; } = false;

        public int IdNegocio => Valor.Split(";").Count() == 2 ? Valor.Split(";")[0].Entero() : (int)0;

        public int IdElemento => Valor.Split(";").Count() == 2 ? Valor.Split(";")[1].Entero() : (int)0;


        public ClausulaDeFiltrado()
        {
        }
        public ClausulaDeFiltrado(string clausula, enumCriteriosDeFiltrado criterio, string valor) :
        this(clausula, criterio)
        {
            Valor = valor;
        }
        public ClausulaDeFiltrado(string clausula, enumCriteriosDeFiltrado criterio, object valor) :
        this(clausula, criterio)
        {
            Valor = valor.ToString();
        }
        public ClausulaDeFiltrado(string clausula, enumCriteriosDeFiltrado criterio, int valor) :
        this(clausula, criterio, valor.ToString())
        {
        }
        public ClausulaDeFiltrado(string clausula, enumCriteriosDeFiltrado criterio)
        {
            Clausula = clausula;
            Criterio = criterio;
        }

        public (decimal? desde, decimal? hasta) ParsearRango()
        {
            var rango = Valor.Split(Simbolos.separadorDeRangos);
            var desde = rango.Length > 0 ? rango[0].TryNumero() : default(decimal?);
            var hasta = rango.Length > 1 ? rango[1].TryNumero() : default(decimal?);

            if (desde != default && hasta != default && (decimal)desde > (decimal)hasta)
                throw new Exception($"No se puede filtrar ya que los importes dados no son coherentes {Valor}");

            return (desde, hasta);
        }

        public (DateTime? desde, DateTime? hasta) ParsearFechas() => ParsearFechas(Valor);

        //public static (DateTime? desde, DateTime? hasta) ParsearFechas(string valor, bool errorSiNulo = true, (DateTime? desde, DateTime? hasta) fechasDeDefecto = default)
        //{
        //    if (valor is null)
        //    {
        //        if (errorSiNulo)
        //            GestorDeErrores.Emitir($"No se han indicado las fecha a 'parsear'");
        //        return fechasDeDefecto;
        //    }

        //    var fechas = valor.Split(Simbolos.separadorDeFechas);
        //    if (fechas.Length == 6) return ($"{fechas[0]}-{fechas[1]}-{fechas[2]}".Fecha(), $"{fechas[3]}-{fechas[4]}-{fechas[5]}".Fecha(finDelDia: true));
        //    if (fechas.Length == 2) return (fechas[0].Fecha(), fechas[1].Fecha(finDelDia: true));

        //    if (fechas.Length == 4 && fechas[3].IsNullOrEmpty()) return ($"{fechas[0]}-{fechas[1]}-{fechas[2]}".Fecha(), null);
        //    if (fechas.Length == 4 && fechas[0].IsNullOrEmpty()) return (null, $"{fechas[1]}-{fechas[2]}-{fechas[3]}".Fecha(finDelDia: true));

        //    if (fechas.Length == 2 && fechas[1].IsNullOrEmpty()) return (fechas[0].Fecha(),null);
        //    if (fechas.Length == 2 && fechas[0].IsNullOrEmpty()) return (null, fechas[1].Fecha(finDelDia: true));

        //    throw new Exception($"No se puede parsear como dos fechas '{valor}'");
        //}
        //

        public static (DateTime? desde, DateTime? hasta) ParsearFechas(string valor, bool errorSiNulo = true, (DateTime? desde, DateTime? hasta) fechasDeDefecto = default)
        {
            if (string.IsNullOrWhiteSpace(valor))
            {
                if (errorSiNulo) GestorDeErrores.Emitir("No se han indicado las fechas a 'parsear'");
                return fechasDeDefecto;
            }

            // 1. Limpiamos posibles espacios y detectamos si es un rango incompleto (ej: "fecha-")
            valor = valor.Trim();
            bool esRangoDesdeIncompleto = valor.EndsWith("-");
            bool esRangoHastaIncompleto = valor.StartsWith("-");
            string valorLimpio = valor.Trim('-');

            // 2. Intentamos extraer las fechas ISO usando una expresión regular
            // Este patrón busca fechas tipo: 2026-04-30T00:00:00.000Z o con Offset +01:00
            var matches = Regex.Matches(valor, @"\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}[^;|-]*");

            if (matches.Count > 0)
            {
                DateTime? d = null;
                DateTime? h = null;

                if (matches.Count >= 2) // Caso: Dos fechas ISO (rango completo)
                {
                    d = matches[0].Value.Fecha();
                    h = matches[1].Value.Fecha(finDelDia: true);
                }
                else if (esRangoDesdeIncompleto) // Caso: "ISO-"
                {
                    d = matches[0].Value.Fecha();
                }
                else if (esRangoHastaIncompleto) // Caso: "-ISO"
                {
                    h = matches[0].Value.Fecha(finDelDia: true);
                }
                else // Caso: "ISO" (una sola fecha)
                {
                    d = matches[0].Value.Fecha();
                    h = d; // O podrías dejar h como null según prefieras
                }
                return (d, h);
            }

            // 3. LOGICA TRADICIONAL (DD-MM-YYYY) - Solo si no es ISO
            var partes = valor.Split(new[] { Simbolos.separadorDeFechas }, StringSplitOptions.None);

            if (partes.Length == 6) // DD-MM-YYYY-DD-MM-YYYY
                return ($"{partes[0]}-{partes[1]}-{partes[2]}".Fecha(), $"{partes[3]}-{partes[4]}-{partes[5]}".Fecha(finDelDia: true));

            if (partes.Length == 2)
            {
                DateTime? d = string.IsNullOrWhiteSpace(partes[0]) ? null : partes[0].Fecha();
                DateTime? h = string.IsNullOrWhiteSpace(partes[1]) ? null : partes[1].Fecha(finDelDia: true);
                return (d, h);
            }

            if (partes.Length == 4 && partes[3].IsNullOrEmpty())
                return ($"{partes[0]}-{partes[1]}-{partes[2]}".Fecha(), null);

            if (partes.Length == 4 && partes[0].IsNullOrEmpty())
                return (null, $"{partes[1]}-{partes[2]}-{partes[3]}".Fecha(finDelDia: true));


            try { return (valor.Fecha(), valor.Fecha(finDelDia: true)); } catch { }

            throw new Exception($"No se pudo parsear el rango de fechas: '{valor}'");
        }
    }

}
